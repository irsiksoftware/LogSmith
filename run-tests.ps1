<#
.SYNOPSIS
Runs Unity tests on Windows and generates formatted reports.

.DESCRIPTION
This script runs Unity tests for a single Unity version (6000.2.5f1) on Windows,
then generates both CSV and HTML reports with test results.
#>

param(
    [string]$UnityVersion = "6000.2.5f1"
)

# Set strict mode
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Force close any running Unity instances
Write-Host "Closing any running Unity instances..." -ForegroundColor Cyan
$unityProcesses = Get-Process -Name "Unity" -ErrorAction SilentlyContinue
if ($unityProcesses) {
    $unityProcesses | ForEach-Object {
        Write-Host "  Killing Unity process (PID: $($_.Id))..." -ForegroundColor Yellow
        Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
    }
    # Wait a moment for processes to fully terminate
    Start-Sleep -Seconds 2
    Write-Host "Unity processes closed." -ForegroundColor Green
} else {
    Write-Host "No running Unity instances found." -ForegroundColor Gray
}

# Create timestamp for output files
$timestamp = Get-Date -Format "MM-dd-yyyy-HH-mm-ss"

# Define paths
$projectPath = $PSScriptRoot
$unityPath = "C:\Program Files\Unity\Hub\Editor\$UnityVersion\Editor\Unity.exe"
$outputDir = Join-Path $projectPath "TestOutputs"
$csvOutputPath = Join-Path $outputDir "TestResults-for-claude-$timestamp.csv"
$htmlOutputPath = Join-Path $outputDir "Test-Results-$timestamp.html"
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
        table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }
        th {
            background-color: #333;
            color: white;
            padding: 12px;
            text-align: left;
            font-weight: 600;
            position: sticky;
            top: 0;
        }
        td {
            padding: 10px 12px;
            border-bottom: 1px solid #ddd;
        }
        tr.passed {
            background-color: #E8F5E9;
        }
        tr.failed {
            background-color: #FFEBEE;
        }
        tr.skipped {
            background-color: #FFF3E0;
        }
        tr:hover {
            opacity: 0.8;
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
        .message {
            font-size: 12px;
            color: #666;
            font-family: monospace;
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

        <table>
            <thead>
                <tr>
                    <th>Mode</th>
                    <th>Test Suite</th>
                    <th>Test Name</th>
                    <th>Result</th>
                    <th>Duration (s)</th>
                    <th>Message</th>
                </tr>
            </thead>
            <tbody>
"@

# Add table rows
foreach ($result in $allResults) {
    $rowClass = $result.Result.ToLower()
    $badgeClass = $result.Result.ToLower()
    $message = $result.Message -replace '&', '&amp;' -replace '<', '&lt;' -replace '>', '&gt;' -replace '"', '&quot;' -replace "'", '&#39;'

    $htmlContent += @"
                <tr class="$rowClass">
                    <td>$($result.TestMode)</td>
                    <td>$($result.TestSuite)</td>
                    <td>$($result.TestName)</td>
                    <td><span class="result-badge $badgeClass">$($result.Result)</span></td>
                    <td>$($result.Duration)</td>
                    <td class="message">$message</td>
                </tr>
"@
}

$htmlContent += @"
            </tbody>
        </table>

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
        table {
            width: 100%;
            border-collapse: collapse;
            margin-top: 20px;
        }
        th {
            background-color: #333;
            color: white;
            padding: 12px;
            text-align: left;
            font-weight: 600;
            position: sticky;
            top: 0;
        }
        td {
            padding: 10px 12px;
            border-bottom: 1px solid #ddd;
        }
        tr.passed {
            background-color: #E8F5E9;
        }
        tr.failed {
            background-color: #FFEBEE;
        }
        tr.skipped {
            background-color: #FFF3E0;
        }
        tr:hover {
            opacity: 0.8;
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
        .message {
            font-size: 12px;
            color: #666;
            font-family: monospace;
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

        <table>
            <thead>
                <tr>
                    <th>Mode</th>
                    <th>Test Suite</th>
                    <th>Test Name</th>
                    <th>Result</th>
                    <th>Duration (s)</th>
                    <th>Message</th>
                </tr>
            </thead>
            <tbody>
"@

# Add table rows with proper HTML encoding
foreach ($result in $allResults) {
    $rowClass = $result.Result.ToLower()
    $badgeClass = $result.Result.ToLower()
    $message = $result.Message -replace '&', '&amp;' -replace '<', '&lt;' -replace '>', '&gt;' -replace '"', '&quot;' -replace "'", '&#39;'

    $htmlContent += @"
                <tr class="$rowClass">
                    <td>$($result.TestMode)</td>
                    <td>$($result.TestSuite)</td>
                    <td>$($result.TestName)</td>
                    <td><span class="result-badge $badgeClass">$($result.Result)</span></td>
                    <td>$($result.Duration)</td>
                    <td class="message">$message</td>
                </tr>
"@
}

$htmlContent += @"
            </tbody>
        </table>

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
