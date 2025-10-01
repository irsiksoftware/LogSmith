<#
.SYNOPSIS
Validates IL2CPP compilation for LogSmith package.

.DESCRIPTION
This script builds a minimal Unity project with IL2CPP scripting backend
to validate that all code is AOT-compatible and no missing types occur.
#>

param(
    [string]$UnityVersion = "6000.2.5f1",
    [string]$BuildTarget = "StandaloneWindows64"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectPath = $PSScriptRoot
$buildOutputPath = Join-Path $projectPath "Build\IL2CPP-Validation"
$unityPath = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
$logFile = Join-Path $projectPath "il2cpp-build-log.txt"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "IL2CPP Validation Build" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Verify Unity installation
if (-Not (Test-Path $unityPath)) {
    Write-Error "Unity $UnityVersion not found at: $unityPath"
    exit 1
}

Write-Host "Unity: $UnityVersion" -ForegroundColor Cyan
Write-Host "Target: $BuildTarget" -ForegroundColor Cyan
Write-Host "Output: $buildOutputPath" -ForegroundColor Cyan
Write-Host "Log: $logFile" -ForegroundColor Cyan

# Clean previous build
if (Test-Path $buildOutputPath) {
    Write-Host "`nCleaning previous build..." -ForegroundColor Yellow
    Remove-Item -Path $buildOutputPath -Recurse -Force -ErrorAction SilentlyContinue
}

# Remove old log
if (Test-Path $logFile) {
    Remove-Item -Path $logFile -Force -ErrorAction SilentlyContinue
}

Write-Host "`nStarting IL2CPP build..." -ForegroundColor Cyan
Write-Host "(This may take several minutes)" -ForegroundColor Yellow

$buildStartTime = Get-Date

# Build with IL2CPP backend using our Editor script
& $unityPath `
    -quit `
    -batchmode `
    -nographics `
    -projectPath $projectPath `
    -executeMethod BuildIL2CPP.BuildIL2CPPCommandLine `
    -logFile $logFile

$buildExitCode = 0
if (Get-Variable -Name LASTEXITCODE -ErrorAction SilentlyContinue) {
    $buildExitCode = $LASTEXITCODE
}
$buildEndTime = Get-Date
$buildDuration = ($buildEndTime - $buildStartTime).TotalSeconds

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Build Results" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Duration: $([math]::Round($buildDuration, 2))s" -ForegroundColor White
Write-Host "Exit Code: $buildExitCode" -ForegroundColor $(if ($buildExitCode -eq 0) { "Green" } else { "Red" })

# Analyze log for IL2CPP-specific errors
if (Test-Path $logFile) {
    $logContent = Get-Content $logFile -Raw

    # Check for IL2CPP conversion errors
    $il2cppErrors = Select-String -Path $logFile -Pattern "IL2CPP error|AOT.*error|missing.*type|MissingMethodException" -AllMatches

    if ($il2cppErrors) {
        $errorCount = ($il2cppErrors.Matches | Measure-Object).Count
        Write-Host "`nIL2CPP/AOT Errors Found: $errorCount" -ForegroundColor Red
        Write-Host "`nShowing errors:" -ForegroundColor Yellow

        foreach ($error in $il2cppErrors.Matches) {
            $line = (Get-Content $logFile | Select-String -Pattern $error.Value -Context 1,1 | Select-Object -First 1)
            Write-Host "  $line" -ForegroundColor Red
        }

        Write-Host "`nFull log: $logFile" -ForegroundColor Yellow
        exit 1
    }

    # Check for compiler errors
    $compilerErrors = Select-String -Path $logFile -Pattern "error CS\d+|Error:" -AllMatches
    if ($compilerErrors) {
        $errorCount = ($compilerErrors.Matches | Measure-Object).Count
        Write-Host "`nCompiler Errors: $errorCount" -ForegroundColor Red
        Write-Host "`nShowing first 10:" -ForegroundColor Yellow

        $errors = $compilerErrors.Matches | Select-Object -First 10
        foreach ($error in $errors) {
            $line = (Get-Content $logFile | Select-String -Pattern $error.Value -Context 0,1 | Select-Object -First 1)
            Write-Host "  $line" -ForegroundColor Red
        }

        Write-Host "`nFull log: $logFile" -ForegroundColor Yellow
        exit 1
    }

    Write-Host "`nIL2CPP validation successful!" -ForegroundColor Green
    Write-Host "No AOT compatibility issues found." -ForegroundColor Green
    exit 0

} else {
    Write-Error "Log file not found: $logFile"
    exit 1
}
