<#
.SYNOPSIS
Runs Unity tests on Windows and generates formatted reports.

.DESCRIPTION
This script runs Unity tests for a single Unity version (6000.2.5f1) on Windows,
then generates both CSV and HTML reports with test results.
#>

param(
    [string]$UnityVersion = "6000.2.5f1",
    [string]$IssueNumber = ""
)

# Set strict mode
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Define project path early
$projectPath = $PSScriptRoot

# Force close any running Unity instances
Write-Host "Closing any running Unity instances..." -ForegroundColor Cyan

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
Write-Host "Cleaning Unity lock files..." -ForegroundColor Cyan

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

# Create timestamp for output files
$timestamp = Get-Date -Format "MM-dd-yyyy-HH-mm-ss"

# Create issue prefix if issue number is provided
$issuePrefix = if ($IssueNumber) { "GH$IssueNumber-" } else { "" }

# Define paths
$unityPath = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
$outputDir = Join-Path $projectPath "TestOutputs"
$csvOutputPath = Join-Path $outputDir "TestResults-for-claude-$issuePrefix$timestamp.csv"
$htmlOutputPath = Join-Path $outputDir "Test-Results-$issuePrefix$timestamp.html"
$tempResultsDir = Join-Path $projectPath "TempTestResults"

# Verify Unity installation
if (-Not (Test-Path $unityPath)) {
    Write-Error "Unity $UnityVersion not found at: $unityPath"
    exit 1
}

Write-Host "Using Unity: $UnityVersion" -ForegroundColor Cyan
Write-Host "Unity path: $unityPath" -ForegroundColor Cyan

# Create output directories
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null
New-Item -ItemType Directory -Force -Path $tempResultsDir | Out-Null

# Setup Unity license
Write-Host "`nSetting up Unity license..." -ForegroundColor Cyan
$licenseDir = "$env:LOCALAPPDATA\Unity\licenses"
New-Item -ItemType Directory -Force -Path $licenseDir | Out-Null

# Run EditMode tests
Write-Host "`nRunning EditMode tests..." -ForegroundColor Cyan
$editModeResults = Join-Path $tempResultsDir "EditMode-results.xml"
$editModeLog = Join-Path $tempResultsDir "EditMode-log.txt"

& $unityPath `
    -runTests `
    -batchmode `
    -nographics `
    -projectPath $projectPath `
    -testPlatform EditMode `
    -testResults $editModeResults `
    -logFile $editModeLog

# Wait for Unity to fully close and release locks
Write-Host "Waiting for Unity to close..." -ForegroundColor Yellow
Start-Sleep -Seconds 3

# Ensure no Unity processes are running
$remainingUnity = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($remainingUnity) {
    Write-Host "  Waiting for remaining Unity processes to close..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
}

# Clean lock files again before next test run
Remove-Item -Path (Join-Path $projectPath "Temp") -Recurse -Force -ErrorAction SilentlyContinue
$lockFile = Join-Path (Join-Path $projectPath "Library") "UnityLockfile"
Remove-Item -Path $lockFile -Force -ErrorAction SilentlyContinue

# Run PlayMode tests
Write-Host "`nRunning PlayMode tests..." -ForegroundColor Cyan
$playModeResults = Join-Path $tempResultsDir "PlayMode-results.xml"
$playModeLog = Join-Path $tempResultsDir "PlayMode-log.txt"

& $unityPath `
    -runTests `
    -batchmode `
    -nographics `
    -projectPath $projectPath `
    -testPlatform PlayMode `
    -testResults $playModeResults `
    -logFile $playModeLog

# Parse test results
Write-Host "`nParsing test results..." -ForegroundColor Cyan

function Parse-NUnitXml {
    param([string]$xmlPath)

    if (-Not (Test-Path $xmlPath)) {
        Write-Warning "Test results file not found: $xmlPath"
        return @()
    }

    [xml]$xml = Get-Content $xmlPath
    $testResults = @()

    # Parse test-case elements
    $testCases = $xml.SelectNodes("//test-case")

    foreach ($testCase in $testCases) {
        $result = [PSCustomObject]@{
            TestMode     = ""
            TestSuite    = $testCase.classname
            TestName     = $testCase.name
            Result       = $testCase.result
            Duration     = [math]::Round([double]$testCase.duration, 3)
            Message      = ""
        }

        # Handle failure messages
        if ($testCase.result -eq "Failed") {
            $failure = $testCase.SelectSingleNode("failure")
            if ($failure) {
                $result.Message = $failure.message -replace "`r`n", " " -replace "`n", " "
                if ($result.Message.Length -gt 200) {
                    $result.Message = $result.Message.Substring(0, 197) + "..."
                }
            }
        }

        $testResults += $result
    }

    return $testResults
}

$allResults = @()

# Parse EditMode results
if (Test-Path $editModeResults) {
    $editResults = Parse-NUnitXml -xmlPath $editModeResults
    foreach ($result in $editResults) {
        $result.TestMode = "EditMode"
    }
    $allResults += $editResults
}

# Parse PlayMode results
if (Test-Path $playModeResults) {
    $playResults = Parse-NUnitXml -xmlPath $playModeResults
    foreach ($result in $playResults) {
        $result.TestMode = "PlayMode"
    }
    $allResults += $playResults
}

# Generate CSV report (minimalist for Claude)
Write-Host "`nGenerating CSV report..." -ForegroundColor Cyan
$csvData = $allResults | Select-Object TestMode, TestSuite, TestName, Result, Duration, Message
$csvData | Export-Csv -Path $csvOutputPath -NoTypeInformation -Encoding UTF8

# Calculate statistics
$totalTests = @($allResults).Count
$passedTests = @($allResults | Where-Object { $_.Result -eq "Passed" }).Count
$failedTests = @($allResults | Where-Object { $_.Result -eq "Failed" }).Count
$skippedTests = @($allResults | Where-Object { $_.Result -eq "Skipped" -or $_.Result -eq "Ignored" }).Count
$totalDuration = if ($allResults.Count -gt 0) { ($allResults | Measure-Object -Property Duration -Sum).Sum } else { 0 }

# Generate HTML report
Write-Host "Generating HTML report..." -ForegroundColor Cyan

$htmlContent = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>LogSmith Test Results - $timestamp</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f5f5f5;
        }
        .container {
            max-width: 1400px;
            margin: 0 auto;
            background-color: white;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        h1 {
            color: #333;
            border-bottom: 3px solid #4CAF50;
            padding-bottom: 10px;
        }
        .summary {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
            margin: 20px 0;
        }
        .summary-card {
            padding: 20px;
            border-radius: 6px;
            text-align: center;
        }
        .summary-card.total {
            background-color: #2196F3;
            color: white;
        }
        .summary-card.passed {
            background-color: #4CAF50;
            color: white;
        }
        .summary-card.failed {
            background-color: #f44336;
            color: white;
        }
        .summary-card.skipped {
            background-color: #FF9800;
            color: white;
        }
        .summary-card h3 {
            margin: 0 0 10px 0;
            font-size: 14px;
            text-transform: uppercase;
            opacity: 0.9;
        }
        .summary-card .value {
            font-size: 32px;
            font-weight: bold;
        }
        .test-results {
            margin-top: 20px;
            text-align: left;
        }
        .test-item {
            margin-bottom: 40px;
        }
        .field-label {
            font-weight: bold;
            font-size: 14px;
            color: #000;
            margin: 0;
        }
        .field-value {
            font-size: 14px;
            color: #333;
            margin: 0 0 10px 0;
            word-wrap: break-word;
            overflow-wrap: break-word;
            word-break: break-word;
            white-space: pre-wrap;
        }
        .field-value.message {
            font-family: monospace;
            font-size: 12px;
            color: #d32f2f;
        }
        .result-badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: bold;
            text-transform: uppercase;
        }
        .result-badge.passed {
            background-color: #4CAF50;
            color: white;
        }
        .result-badge.failed {
            background-color: #f44336;
            color: white;
        }
        .result-badge.skipped {
            background-color: #FF9800;
            color: white;
        }
        .footer {
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #ddd;
            text-align: center;
            color: #666;
            font-size: 14px;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>LogSmith Test Results</h1>
        <p><strong>Unity Version:</strong> $UnityVersion | <strong>Platform:</strong> Windows | <strong>Generated:</strong> $timestamp</p>

        <div class="summary">
            <div class="summary-card total">
                <h3>Total Tests</h3>
                <div class="value">$totalTests</div>
            </div>
            <div class="summary-card passed">
                <h3>Passed</h3>
                <div class="value">$passedTests</div>
            </div>
            <div class="summary-card failed">
                <h3>Failed</h3>
                <div class="value">$failedTests</div>
            </div>
            <div class="summary-card skipped">
                <h3>Skipped</h3>
                <div class="value">$skippedTests</div>
            </div>
        </div>

        <div class="test-results">
"@

# Add test items
foreach ($result in $allResults) {
    $badgeClass = $result.Result.ToLower()
    $message = $result.Message -replace '&', '&amp;' -replace '<', '&lt;' -replace '>', '&gt;' -replace '"', '&quot;' -replace "'", '&#39;'

    $htmlContent += @"
            <div class="test-item">
                <p class="field-label">Mode</p>
                <p class="field-value">$($result.TestMode)</p>

                <p class="field-label">Test Suite</p>
                <p class="field-value">$($result.TestSuite)</p>

                <p class="field-label">Test Name</p>
                <p class="field-value">$($result.TestName)</p>

                <p class="field-label">Result</p>
                <p class="field-value"><span class="result-badge $badgeClass">$($result.Result)</span></p>

                <p class="field-label">Duration</p>
                <p class="field-value">$($result.Duration)s</p>
"@

    # Only add message field if there's a message
    if ($message) {
        $htmlContent += @"

                <p class="field-label">Message</p>
                <p class="field-value message">$message</p>
"@
    }

    $htmlContent += @"
            </div>
            <br /><br /><br />
"@
}

$htmlContent += @"
        </div>

        <div class="footer">
            <p>Total execution time: $([math]::Round($totalDuration, 2)) seconds</p>
            <p>Generated with LogSmith Test Runner</p>
        </div>
    </div>
</body>
</html>
"@

# Need to add System.Web assembly for HTML encoding
Add-Type -AssemblyName System.Web

# Re-generate HTML with proper encoding
$htmlContent = @"
<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>LogSmith Test Results - $timestamp</title>
    <style>
        body {
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
            margin: 0;
            padding: 20px;
            background-color: #f5f5f5;
        }
        .container {
            max-width: 1400px;
            margin: 0 auto;
            background-color: white;
            padding: 30px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        h1 {
            color: #333;
            border-bottom: 3px solid #4CAF50;
            padding-bottom: 10px;
        }
        .summary {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
            gap: 15px;
            margin: 20px 0;
        }
        .summary-card {
            padding: 20px;
            border-radius: 6px;
            text-align: center;
        }
        .summary-card.total {
            background-color: #2196F3;
            color: white;
        }
        .summary-card.passed {
            background-color: #4CAF50;
            color: white;
        }
        .summary-card.failed {
            background-color: #f44336;
            color: white;
        }
        .summary-card.skipped {
            background-color: #FF9800;
            color: white;
        }
        .summary-card h3 {
            margin: 0 0 10px 0;
            font-size: 14px;
            text-transform: uppercase;
            opacity: 0.9;
        }
        .summary-card .value {
            font-size: 32px;
            font-weight: bold;
        }
        .test-results {
            margin-top: 20px;
            text-align: left;
        }
        .test-item {
            margin-bottom: 40px;
        }
        .field-label {
            font-weight: bold;
            font-size: 14px;
            color: #000;
            margin: 0;
        }
        .field-value {
            font-size: 14px;
            color: #333;
            margin: 0 0 10px 0;
            word-wrap: break-word;
            overflow-wrap: break-word;
            word-break: break-word;
            white-space: pre-wrap;
        }
        .field-value.message {
            font-family: monospace;
            font-size: 12px;
            color: #d32f2f;
        }
        .result-badge {
            display: inline-block;
            padding: 4px 12px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: bold;
            text-transform: uppercase;
        }
        .result-badge.passed {
            background-color: #4CAF50;
            color: white;
        }
        .result-badge.failed {
            background-color: #f44336;
            color: white;
        }
        .result-badge.skipped {
            background-color: #FF9800;
            color: white;
        }
        .footer {
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #ddd;
            text-align: center;
            color: #666;
            font-size: 14px;
        }
    </style>
</head>
<body>
    <div class="container">
        <h1>LogSmith Test Results</h1>
        <p><strong>Unity Version:</strong> $UnityVersion | <strong>Platform:</strong> Windows | <strong>Generated:</strong> $timestamp</p>

        <div class="summary">
            <div class="summary-card total">
                <h3>Total Tests</h3>
                <div class="value">$totalTests</div>
            </div>
            <div class="summary-card passed">
                <h3>Passed</h3>
                <div class="value">$passedTests</div>
            </div>
            <div class="summary-card failed">
                <h3>Failed</h3>
                <div class="value">$failedTests</div>
            </div>
            <div class="summary-card skipped">
                <h3>Skipped</h3>
                <div class="value">$skippedTests</div>
            </div>
        </div>

        <div class="test-results">
"@

# Add test items
foreach ($result in $allResults) {
    $badgeClass = $result.Result.ToLower()
    $message = $result.Message -replace '&', '&amp;' -replace '<', '&lt;' -replace '>', '&gt;' -replace '"', '&quot;' -replace "'", '&#39;'

    $htmlContent += @"
            <div class="test-item">
                <p class="field-label">Mode</p>
                <p class="field-value">$($result.TestMode)</p>

                <p class="field-label">Test Suite</p>
                <p class="field-value">$($result.TestSuite)</p>

                <p class="field-label">Test Name</p>
                <p class="field-value">$($result.TestName)</p>

                <p class="field-label">Result</p>
                <p class="field-value"><span class="result-badge $badgeClass">$($result.Result)</span></p>

                <p class="field-label">Duration</p>
                <p class="field-value">$($result.Duration)s</p>
"@

    # Only add message field if there's a message
    if ($message) {
        $htmlContent += @"

                <p class="field-label">Message</p>
                <p class="field-value message">$message</p>
"@
    }

    $htmlContent += @"
            </div>
            <br /><br /><br />
"@
}

$htmlContent += @"
        </div>

        <div class="footer">
            <p>Total execution time: $([math]::Round($totalDuration, 2)) seconds</p>
            <p>Generated with LogSmith Test Runner</p>
        </div>
    </div>
</body>
</html>
"@

$htmlContent | Out-File -FilePath $htmlOutputPath -Encoding UTF8

# Display summary
Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "Test Execution Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Total Tests:   $totalTests" -ForegroundColor White
Write-Host "Passed:        $passedTests" -ForegroundColor Green
Write-Host "Failed:        $failedTests" -ForegroundColor Red
Write-Host "Skipped:       $skippedTests" -ForegroundColor Yellow
Write-Host "Duration:      $([math]::Round($totalDuration, 2))s" -ForegroundColor White
Write-Host "`nReports generated:" -ForegroundColor Cyan
Write-Host "  CSV:  $csvOutputPath" -ForegroundColor White
Write-Host "  HTML: $htmlOutputPath" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan

# Clean up temp results
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue $tempResultsDir

# Exit with appropriate code
if ($failedTests -gt 0) {
    exit 1
}
exit 0
