<#
.SYNOPSIS
Exports LogSmith as a .unitypackage for Asset Store distribution.

.DESCRIPTION
This script exports the com.irsiksoftware.logsmith package along with its samples
into a .unitypackage file suitable for Asset Store submission.
#>

param(
    [string]$UnityVersion = "6000.2.5f1",
    [string]$OutputPath = "LogSmith.unitypackage"
)

# Set strict mode
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectPath = $PSScriptRoot

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "LogSmith Package Export Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# Force close any running Unity instances
Write-Host "`nClosing any running Unity instances..." -ForegroundColor Cyan
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
    Write-Host "  Waiting for processes to terminate..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
    Write-Host "Unity processes closed." -ForegroundColor Green
} else {
    Write-Host "No running Unity instances found." -ForegroundColor Gray
}

# Define paths
$unityPath = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
$logFile = Join-Path $projectPath "export-log.txt"
$outputFile = Join-Path $projectPath $OutputPath

# Verify Unity installation
if (-Not (Test-Path $unityPath)) {
    Write-Error "Unity $UnityVersion not found at: $unityPath"
    exit 1
}

Write-Host "`nUsing Unity: $UnityVersion" -ForegroundColor Cyan
Write-Host "Unity path: $unityPath" -ForegroundColor Cyan
Write-Host "Output package: $outputFile" -ForegroundColor Cyan
Write-Host "Log file: $logFile" -ForegroundColor Cyan

# Remove old files
if (Test-Path $logFile) {
    Remove-Item -Path $logFile -Force -ErrorAction SilentlyContinue
}
if (Test-Path $outputFile) {
    Write-Host "`nRemoving existing package..." -ForegroundColor Yellow
    Remove-Item -Path $outputFile -Force -ErrorAction SilentlyContinue
}

# Export package
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Exporting Package..." -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

$exportStartTime = Get-Date

& $unityPath `
    -quit `
    -batchmode `
    -nographics `
    -projectPath $projectPath `
    -logFile $logFile `
    -exportPackage "Packages/com.irsiksoftware.logsmith" $outputFile

$exportExitCode = 0
if (Get-Variable -Name LASTEXITCODE -ErrorAction SilentlyContinue) {
    $exportExitCode = $LASTEXITCODE
}

# Wait for export to complete and file to be written
Write-Host "`nWaiting for export to complete..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

# Wait for file to exist (up to 10 seconds)
$maxWait = 10
$waited = 0
while (-not (Test-Path $outputFile) -and $waited -lt $maxWait) {
    Start-Sleep -Seconds 1
    $waited++
}

$exportEndTime = Get-Date
$exportDuration = ($exportEndTime - $exportStartTime).TotalSeconds

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Export Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Duration: $([math]::Round($exportDuration, 2))s" -ForegroundColor White
Write-Host "Exit Code: $exportExitCode" -ForegroundColor $(if ($exportExitCode -eq 0) { "Green" } else { "Red" })

# Verify package was created
if (Test-Path $outputFile) {
    $packageSize = (Get-Item $outputFile).Length
    $packageSizeMB = [math]::Round($packageSize / 1MB, 2)

    Write-Host "`nPackage created successfully!" -ForegroundColor Green
    Write-Host "  Location: $outputFile" -ForegroundColor White
    Write-Host "  Size: $packageSizeMB MB" -ForegroundColor White

    # Check log for errors or warnings
    if (Test-Path $logFile) {
        $logContent = Get-Content $logFile -Raw

        if ($logContent -match "error|exception|failed") {
            Write-Warning "`nWarnings or errors found in export log:"
            Write-Host "Check $logFile for details" -ForegroundColor Yellow
        }
    }

    exit 0
} else {
    Write-Error "`nPackage export failed!"
    Write-Host "Check log at: $logFile" -ForegroundColor Yellow
    exit 1
}
