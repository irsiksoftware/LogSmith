@echo off
setlocal enabledelayedexpansion

REM ============================================================================
REM Unity Test Runner Script
REM Usage: run-tests.bat [EditMode|PlayMode] [TestClassName]
REM
REM Examples:
REM   run-tests.bat EditMode              - Run all EditMode tests
REM   run-tests.bat PlayMode              - Run all PlayMode tests
REM   run-tests.bat EditMode ClassName    - Run specific test class
REM ============================================================================

set "SCRIPT_DIR=%~dp0"
set "PROJECT_DIR=%SCRIPT_DIR:~0,-1%"
set "RESULTS_DIR=%PROJECT_DIR%\TestResults"

REM Default to EditMode if not specified
set "TEST_PLATFORM=%~1"
if "%TEST_PLATFORM%"=="" set "TEST_PLATFORM=EditMode"

REM Optional test class filter
set "TEST_FILTER=%~2"

REM Validate test platform
if /i not "%TEST_PLATFORM%"=="EditMode" if /i not "%TEST_PLATFORM%"=="PlayMode" (
    echo Error: Invalid test platform "%TEST_PLATFORM%"
    echo Usage: run-tests.bat [EditMode^|PlayMode] [TestClassName]
    exit /b 1
)

REM Find Unity installation
set "UNITY_VERSION=6000.3.2f1"
set "UNITY_PATH=C:\Program Files\Unity\Hub\Editor\%UNITY_VERSION%\Editor\Unity.exe"

if not exist "%UNITY_PATH%" (
    echo Error: Unity %UNITY_VERSION% not found at %UNITY_PATH%
    echo Please verify Unity Hub installation.
    exit /b 1
)

echo ============================================================
echo Unity Test Runner
echo ============================================================
echo Platform:    %TEST_PLATFORM%
echo Project:     %PROJECT_DIR%
echo Unity:       %UNITY_PATH%
if not "%TEST_FILTER%"=="" echo Filter:      %TEST_FILTER%
echo ============================================================

REM Create TestResults directory if it doesn't exist
if not exist "%RESULTS_DIR%" (
    echo Creating TestResults directory...
    mkdir "%RESULTS_DIR%"
)

REM Set output paths based on platform
set "PLATFORM_LOWER=%TEST_PLATFORM%"
REM Convert to lowercase for filenames
for %%a in ("EditMode=editmode" "PlayMode=playmode") do (
    for /f "tokens=1,2 delims==" %%b in (%%a) do (
        if /i "%TEST_PLATFORM%"=="%%b" set "PLATFORM_LOWER=%%c"
    )
)

set "RESULTS_XML=%RESULTS_DIR%\%PLATFORM_LOWER%-results.xml"
set "LOG_FILE=%RESULTS_DIR%\%PLATFORM_LOWER%-log.txt"
set "LATEST_DIR=%RESULTS_DIR%\latest-%PLATFORM_LOWER%"

REM Create latest results directory
if not exist "%LATEST_DIR%" mkdir "%LATEST_DIR%"

REM Build Unity arguments
set "UNITY_ARGS=-runTests -batchmode -nographics -projectPath "%PROJECT_DIR%" -testPlatform %TEST_PLATFORM% -testResults "%RESULTS_XML%" -logFile "%LOG_FILE%""

REM Add test filter if specified
if not "%TEST_FILTER%"=="" (
    set "UNITY_ARGS=%UNITY_ARGS% -testFilter %TEST_FILTER%"
)

echo.
echo Running %TEST_PLATFORM% tests...
echo.

REM Run Unity tests
"%UNITY_PATH%" %UNITY_ARGS%
set "EXIT_CODE=%ERRORLEVEL%"

REM Parse results and generate JSON files
echo.
echo ============================================================
echo Processing Results...
echo ============================================================

REM Generate failures.json and successes.json using PowerShell
powershell -NoProfile -ExecutionPolicy Bypass -Command ^
    "$xmlPath = '%RESULTS_XML%'; " ^
    "$latestDir = '%LATEST_DIR%'; " ^
    "if (Test-Path $xmlPath) { " ^
    "  [xml]$xml = Get-Content $xmlPath; " ^
    "  $failures = @(); " ^
    "  $successes = @(); " ^
    "  foreach ($testCase in $xml.SelectNodes('//test-case')) { " ^
    "    $test = @{ " ^
    "      name = $testCase.name; " ^
    "      fullname = $testCase.fullname; " ^
    "      classname = $testCase.classname; " ^
    "      result = $testCase.result; " ^
    "      duration = $testCase.duration " ^
    "    }; " ^
    "    if ($testCase.result -eq 'Failed') { " ^
    "      $failure = $testCase.SelectSingleNode('failure'); " ^
    "      if ($failure) { " ^
    "        $test.message = $failure.SelectSingleNode('message').'#text'; " ^
    "        $test.stacktrace = $failure.SelectSingleNode('stack-trace').'#text'; " ^
    "      }; " ^
    "      $failures += $test " ^
    "    } elseif ($testCase.result -eq 'Passed') { " ^
    "      $successes += $test " ^
    "    } " ^
    "  }; " ^
    "  $failures | ConvertTo-Json -Depth 5 | Out-File \"$latestDir\failures.json\" -Encoding UTF8; " ^
    "  $successes | ConvertTo-Json -Depth 5 | Out-File \"$latestDir\successes.json\" -Encoding UTF8; " ^
    "  Write-Host \"Generated: $latestDir\failures.json ($($failures.Count) failures)\"; " ^
    "  Write-Host \"Generated: $latestDir\successes.json ($($successes.Count) passed)\"; " ^
    "} else { " ^
    "  Write-Host 'Warning: Test results XML not found' " ^
    "}"

echo.
echo ============================================================
echo Test Run Complete
echo ============================================================
echo Results XML: %RESULTS_XML%
echo Log File:    %LOG_FILE%
echo JSON Output: %LATEST_DIR%
echo Exit Code:   %EXIT_CODE%
echo ============================================================

if %EXIT_CODE%==0 (
    echo Status: PASSED
) else if %EXIT_CODE%==2 (
    echo Status: SOME TESTS FAILED
) else if %EXIT_CODE%==3 (
    echo Status: ALL TESTS FAILED
) else (
    echo Status: ERROR (Unity crashed or configuration issue)
)

exit /b %EXIT_CODE%
