<#
.SYNOPSIS
Builds the Unity project and checks for compilation errors.

.DESCRIPTION
This script builds the Unity project in batch mode, checks for compiler errors,
and provides clear output about the build status.
#>

param(
    [string]$UnityVersion = "6000.2.5f1"
)

# Set strict mode
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Define project path early
$projectPath = $PSScriptRoot

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Unity Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Force close any running Unity instances
Write-Host "`nClosing any running Unity instances..." -ForegroundColor Cyan

# Look for Unity processes with various names
$processNames = @("Unity", "Unity.exe", "UnityHelper")
$foundProcesses = $false

foreach ($processName in $processNames) {
    $unityProcesses = Get-Process -Name $processName -ErrorAction SilentlyContinue
    if ($unityProcesses) {
        $foundProcesses = $true
        $unityProcesses | ForEach-Object {
            Write-Host "  Killing $processName process (PID: $($_.Id))..." -ForegroundColor Yellow
            Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
        }
    }
}

if ($foundProcesses) {
    # Wait longer for processes to fully terminate and release file locks
    Write-Host "  Waiting for processes to terminate..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5

    # Verify all Unity processes are gone
    $remainingProcesses = Get-Process | Where-Object { $_.Name -like "*Unity*" }
    if ($remainingProcesses) {
        Write-Warning "Some Unity processes may still be running:"
        $remainingProcesses | ForEach-Object { Write-Warning "  $($_.Name) (PID: $($_.Id))" }
        Start-Sleep -Seconds 3
    }

    Write-Host "Unity processes closed." -ForegroundColor Green
} else {
    Write-Host "No running Unity instances found." -ForegroundColor Gray
}

# Remove any Unity lock files for this project
Write-Host "`nCleaning Unity lock files..." -ForegroundColor Cyan

$tempPath = Join-Path $projectPath "Temp"
if (Test-Path $tempPath) {
    Write-Host "  Removing Temp folder..." -ForegroundColor Yellow
    Remove-Item -Path $tempPath -Recurse -Force -ErrorAction SilentlyContinue
}

# Also remove lock file from Library if it exists
$libraryPath = Join-Path $projectPath "Library"
$lockFile = Join-Path $libraryPath "UnityLockfile"
if (Test-Path $lockFile) {
    Write-Host "  Removing Unity lock file..." -ForegroundColor Yellow
    Remove-Item -Path $lockFile -Force -ErrorAction SilentlyContinue
}

Write-Host "Unity lock files cleaned." -ForegroundColor Green

# Define paths
$unityPath = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
$logFile = Join-Path $projectPath "build-log.txt"

# Verify Unity installation
if (-Not (Test-Path $unityPath)) {
    Write-Error "Unity $UnityVersion not found at: $unityPath"
    exit 1
}

Write-Host "`nUsing Unity: $UnityVersion" -ForegroundColor Cyan
Write-Host "Unity path: $unityPath" -ForegroundColor Cyan
Write-Host "Log file: $logFile" -ForegroundColor Cyan

# Remove old log file if it exists
if (Test-Path $logFile) {
    Remove-Item -Path $logFile -Force -ErrorAction SilentlyContinue
}

# Run Unity build
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Running Unity Build..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$buildStartTime = Get-Date

& $unityPath `
    -quit `
    -batchmode `
    -nographics `
    -projectPath $projectPath `
    -logFile $logFile

$buildExitCode = 0
if (Get-Variable -Name LASTEXITCODE -ErrorAction SilentlyContinue) {
    $buildExitCode = $LASTEXITCODE
}

# Wait for log file to be written
Write-Host "`nWaiting for log file..." -ForegroundColor Yellow
Start-Sleep -Seconds 2

$buildEndTime = Get-Date
$buildDuration = ($buildEndTime - $buildStartTime).TotalSeconds

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Build Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Duration: $([math]::Round($buildDuration, 2))s" -ForegroundColor White
Write-Host "Exit Code: $buildExitCode" -ForegroundColor $(if ($buildExitCode -eq 0) { "Green" } else { "Red" })

# Check for compiler errors in log
if (Test-Path $logFile) {
    $logContent = Get-Content $logFile -Raw

    # Look for compiler errors
    $compilerErrors = Select-String -Path $logFile -Pattern "error CS\d+" -AllMatches
    $errorCount = 0
    if ($compilerErrors) {
        $errorCount = ($compilerErrors.Matches | Measure-Object).Count
    }

    if ($errorCount -gt 0) {
        Write-Host "`nCompiler Errors Found: $errorCount" -ForegroundColor Red
        Write-Host "`nShowing first 10 errors:" -ForegroundColor Yellow

        $errors = $compilerErrors.Matches | Select-Object -First 10
        foreach ($error in $errors) {
            $line = (Get-Content $logFile | Select-String -Pattern $error.Value -Context 0,1 | Select-Object -First 1)
            Write-Host "  $line" -ForegroundColor Red
        }

        Write-Host "`nFull log available at: $logFile" -ForegroundColor Yellow
        exit 1
    } else {
        Write-Host "`nNo compiler errors found!" -ForegroundColor Green

        # Check for warnings
        $warnings = Select-String -Path $logFile -Pattern "warning CS\d+" -AllMatches
        $warningCount = 0
        if ($warnings) {
            $warningCount = ($warnings.Matches | Measure-Object).Count
        }

        if ($warningCount -gt 0) {
            Write-Host "Compiler Warnings: $warningCount" -ForegroundColor Yellow
        }

        Write-Host "`nBuild succeeded!" -ForegroundColor Green
        exit 0
    }
} else {
    Write-Warning "Log file not found at: $logFile"
    Write-Warning "Cannot verify build status"
    exit 1
}
