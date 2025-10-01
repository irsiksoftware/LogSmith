<#
.SYNOPSIS
Runs Render Pipeline validation tests across multiple Unity versions.

.DESCRIPTION
This script validates LogSmith's render pipeline adapters (Built-in, URP, HDRP)
across Unity 2022.3 LTS, 2023 LTS, and 6000.2 LTS.

.PARAMETER UnityVersions
Array of Unity versions to test. Defaults to all supported LTS versions.

.PARAMETER Pipelines
Array of render pipelines to test. Defaults to all supported pipelines.

.EXAMPLE
.\run-rp-validation.ps1
Runs all RP tests across all Unity versions.

.EXAMPLE
.\run-rp-validation.ps1 -UnityVersions @("6000.2.5f1") -Pipelines @("URP")
Runs only URP tests on Unity 6000.2.5f1.
#>

param(
    [string[]]$UnityVersions = @("2022.3.50f1", "2023.2.20f1", "6000.2.5f1"),
    [string[]]$Pipelines = @("Built-in", "URP", "HDRP")
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$projectPath = $PSScriptRoot
$timestamp = Get-Date -Format "MM-dd-yyyy-HH-mm-ss"
$outputDir = Join-Path $projectPath "TestOutputs"
$summaryFile = Join-Path $outputDir "RP-Validation-Summary-$timestamp.txt"

# Create output directory
New-Item -ItemType Directory -Force -Path $outputDir | Out-Null

# Initialize summary
$summary = @()
$summary += "=" * 80
$summary += "LogSmith Render Pipeline Validation"
$summary += "Started: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$summary += "=" * 80
$summary += ""

$allPassed = $true

foreach ($version in $UnityVersions) {
    $unityPath = "C:\Program Files\Unity\Hub\Editor\$version\Editor\Unity.exe"

    if (-Not (Test-Path $unityPath)) {
        Write-Warning "Unity $version not found at: $unityPath - SKIPPING"
        $summary += "Unity $version: NOT FOUND - SKIPPED"
        $summary += ""
        continue
    }

    Write-Host "`n========================================" -ForegroundColor Cyan
    Write-Host "Testing Unity $version" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan

    $summary += "Unity $version"
    $summary += "-" * 40

    foreach ($pipeline in $Pipelines) {
        Write-Host "`nTesting $pipeline pipeline..." -ForegroundColor Yellow

        # Determine test filter
        $testFilter = switch ($pipeline) {
            "Built-in" { "BuiltInRenderPipelineAdapterTests" }
            "URP"      { "URPAdapterTests" }
            "HDRP"     { "HDRPAdapterTests" }
        }

        # Setup paths
        $resultsFile = Join-Path $outputDir "RP-$version-$pipeline-results-$timestamp.xml"
        $logFile = Join-Path $outputDir "RP-$version-$pipeline-log-$timestamp.txt"

        # Force close Unity processes before running tests
        Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
        Start-Sleep -Seconds 2

        try {
            # Run PlayMode tests for this RP
            & $unityPath `
                -runTests `
                -batchmode `
                -nographics `
                -projectPath $projectPath `
                -testPlatform PlayMode `
                -testFilter $testFilter `
                -testResults $resultsFile `
                -logFile $logFile

            # Wait for Unity to close
            Start-Sleep -Seconds 3
            Get-Process -Name "Unity" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

            # Parse results
            if (Test-Path $resultsFile) {
                [xml]$results = Get-Content $resultsFile
                $total = $results.SelectNodes("//test-case").Count
                $passed = $results.SelectNodes("//test-case[@result='Passed']").Count
                $failed = $results.SelectNodes("//test-case[@result='Failed']").Count

                $resultText = "  $pipeline : $passed/$total passed"
                if ($failed -gt 0) {
                    Write-Host $resultText -ForegroundColor Red
                    $summary += "$resultText - FAILED"
                    $allPassed = $false
                } else {
                    Write-Host $resultText -ForegroundColor Green
                    $summary += "$resultText - PASSED"
                }
            } else {
                Write-Warning "Results file not found for $pipeline on $version"
                $summary += "  $pipeline : NO RESULTS FILE - FAILED"
                $allPassed = $false
            }
        }
        catch {
            Write-Error "Test execution failed for $pipeline on $version: $_"
            $summary += "  $pipeline : EXECUTION ERROR - FAILED"
            $allPassed = $false
        }
    }

    $summary += ""
}

# Final summary
$summary += "=" * 80
if ($allPassed) {
    $summary += "RESULT: ALL TESTS PASSED ✅"
    Write-Host "`n✅ ALL RP VALIDATION TESTS PASSED" -ForegroundColor Green
} else {
    $summary += "RESULT: SOME TESTS FAILED ❌"
    Write-Host "`n❌ SOME RP VALIDATION TESTS FAILED" -ForegroundColor Red
}
$summary += "Completed: $(Get-Date -Format 'yyyy-MM-dd HH:mm:ss')"
$summary += "=" * 80

# Write summary to file
$summary | Out-File -FilePath $summaryFile -Encoding UTF8

Write-Host "`nSummary saved to: $summaryFile" -ForegroundColor Cyan

# Display summary
Write-Host "`n$($summary -join "`n")" -ForegroundColor White

# Exit with appropriate code
if (-Not $allPassed) {
    exit 1
}
exit 0
