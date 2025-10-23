param(
    [string]$XmlPath,
    [string]$HtmlPath,
    [string]$Branch,
    [string]$RunNumber,
    [string]$RunId,
    [string]$CommitSha
)

if (-not (Test-Path $XmlPath)) {
    Write-Host "::warning::Test results XML not found at: $XmlPath"
    exit 0
}

[xml]$xml = Get-Content $XmlPath

$passed = [int]$xml.'test-run'.passed
$failed = [int]$xml.'test-run'.failed
$skipped = [int]$xml.'test-run'.skipped
$total = [int]$xml.'test-run'.total
$duration = [double]$xml.'test-run'.duration

$status = if ($failed -eq 0) { "PASSED" } else { "FAILED" }
$statusColor = if ($failed -eq 0) { "#4CAF50" } else { "#f44336" }
$statusGradient = if ($failed -eq 0) { "#45a049" } else { "#da190b" }

# Build test details HTML
$testDetailsHtml = ""
$testCases = $xml.SelectNodes("//test-case")

Write-Host "Generating HTML report for $($testCases.Count) test cases..."

foreach ($test in $testCases) {
    $testName = [System.Security.SecurityElement]::Escape($test.fullname)
    $testResult = $test.result
    $testDuration = $test.duration

    $icon = switch ($testResult) {
        "Passed" { "✓" }
        "Failed" { "✗" }
        "Skipped" { "⊘" }
        default { "?" }
    }

    $rowClass = switch ($testResult) {
        "Passed" { "test-passed" }
        "Failed" { "test-failed" }
        "Skipped" { "test-skipped" }
        default { "" }
    }

    $failureMessage = ""
    if ($testResult -eq "Failed" -and $test.failure) {
        $message = [System.Security.SecurityElement]::Escape($test.failure.message)
        $failureMessage = "<div class='failure-message'>$message</div>"
    }

    $testDetailsHtml += @"
<tr class='$rowClass'>
  <td class='test-icon'>$icon</td>
  <td class='test-name'>$testName$failureMessage</td>
  <td class='test-duration'>$([math]::Round([double]$testDuration, 3))s</td>
  <td class='test-result'>$testResult</td>
</tr>
"@
}

$commitShort = $CommitSha.Substring(0, 7)

$html = @"
<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <title>LogSmith - Test Report #$RunNumber</title>
  <style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body {
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, Cantarell, sans-serif;
      background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
      padding: 20px;
      min-height: 100vh;
    }
    .container {
      max-width: 1400px;
      margin: 0 auto;
      background-color: white;
      border-radius: 12px;
      box-shadow: 0 20px 60px rgba(0,0,0,0.3);
      overflow: hidden;
    }
    .header {
      background: linear-gradient(135deg, $statusColor 0%, $statusGradient 100%);
      color: white;
      padding: 40px;
      text-align: center;
    }
    h1 { font-size: 36px; margin-bottom: 10px; font-weight: 600; }
    .status-badge {
      display: inline-block;
      background: rgba(255,255,255,0.2);
      padding: 10px 30px;
      border-radius: 50px;
      font-size: 18px;
      font-weight: 600;
      margin-top: 10px;
    }
    .meta {
      background: #f8f9fa;
      padding: 20px 40px;
      border-bottom: 1px solid #e0e0e0;
      display: flex;
      flex-wrap: wrap;
      gap: 30px;
    }
    .meta-item { display: flex; flex-direction: column; }
    .meta-label {
      font-size: 11px;
      text-transform: uppercase;
      color: #666;
      letter-spacing: 1px;
      margin-bottom: 5px;
    }
    .meta-value { font-size: 16px; font-weight: 600; color: #333; }
    .summary {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 0;
      border-bottom: 1px solid #e0e0e0;
    }
    .stat-box { padding: 30px; text-align: center; border-right: 1px solid #e0e0e0; }
    .stat-box:last-child { border-right: none; }
    .stat-box.total { background: #f5f7fa; }
    .stat-box.passed { background: #e8f5e9; }
    .stat-box.failed { background: #ffebee; }
    .stat-box.skipped { background: #fff3e0; }
    .stat-number { font-size: 48px; font-weight: bold; margin-bottom: 10px; }
    .stat-box.total .stat-number { color: #667eea; }
    .stat-box.passed .stat-number { color: #4CAF50; }
    .stat-box.failed .stat-number { color: #f44336; }
    .stat-box.skipped .stat-number { color: #ff9800; }
    .stat-label { font-size: 12px; text-transform: uppercase; letter-spacing: 1px; color: #666; }
    .content { padding: 40px; }
    h2 {
      font-size: 24px;
      color: #333;
      margin-bottom: 20px;
      padding-bottom: 10px;
      border-bottom: 2px solid #e0e0e0;
      font-weight: 600;
    }
    table { width: 100%; border-collapse: collapse; margin-top: 20px; }
    th {
      background: #f5f7fa;
      padding: 15px;
      text-align: left;
      font-weight: 600;
      color: #333;
      border-bottom: 2px solid #e0e0e0;
      font-size: 13px;
    }
    td { padding: 12px 15px; border-bottom: 1px solid #f0f0f0; }
    tr:hover { background: #f8f9fa; }
    .test-icon { width: 40px; text-align: center; font-size: 20px; }
    .test-passed .test-icon { color: #4CAF50; }
    .test-failed .test-icon { color: #f44336; }
    .test-skipped .test-icon { color: #ff9800; }
    .test-name { font-family: 'Consolas', 'Monaco', monospace; font-size: 13px; }
    .test-duration { width: 100px; text-align: right; color: #666; font-size: 12px; }
    .test-result { width: 100px; text-align: center; font-weight: 600; font-size: 12px; }
    .test-passed .test-result { color: #4CAF50; }
    .test-failed .test-result { color: #f44336; }
    .test-skipped .test-result { color: #ff9800; }
    .failure-message {
      margin-top: 8px;
      padding: 8px 12px;
      background: #ffebee;
      border-left: 3px solid #f44336;
      font-size: 12px;
      color: #c62828;
      font-family: 'Consolas', 'Monaco', monospace;
      white-space: pre-wrap;
      word-break: break-word;
    }
    .footer {
      background: #f5f7fa;
      padding: 20px 40px;
      text-align: center;
      color: #999;
      font-size: 12px;
      border-top: 1px solid #e0e0e0;
    }
    .footer a { color: #667eea; text-decoration: none; }
    .footer a:hover { text-decoration: underline; }
  </style>
</head>
<body>
  <div class="container">
    <div class="header">
      <h1>LogSmith - Unity Test Report</h1>
      <div class="status-badge">$status</div>
    </div>

    <div class="meta">
      <div class="meta-item">
        <span class="meta-label">Branch</span>
        <span class="meta-value">$Branch</span>
      </div>
      <div class="meta-item">
        <span class="meta-label">Unity Version</span>
        <span class="meta-value">6000.0.59f2 (Unity 6)</span>
      </div>
      <div class="meta-item">
        <span class="meta-label">Run Number</span>
        <span class="meta-value">#$RunNumber</span>
      </div>
      <div class="meta-item">
        <span class="meta-label">Duration</span>
        <span class="meta-value">$([math]::Round($duration, 2))s</span>
      </div>
      <div class="meta-item">
        <span class="meta-label">Commit</span>
        <span class="meta-value">$commitShort</span>
      </div>
    </div>

    <div class="summary">
      <div class="stat-box total">
        <div class="stat-number">$total</div>
        <div class="stat-label">Total Tests</div>
      </div>
      <div class="stat-box passed">
        <div class="stat-number">$passed</div>
        <div class="stat-label">Passed</div>
      </div>
      <div class="stat-box failed">
        <div class="stat-number">$failed</div>
        <div class="stat-label">Failed</div>
      </div>
      <div class="stat-box skipped">
        <div class="stat-number">$skipped</div>
        <div class="stat-label">Skipped</div>
      </div>
    </div>

    <div class="content">
      <h2>Test Results ($total tests)</h2>
      <table>
        <thead>
          <tr>
            <th></th>
            <th>Test Name</th>
            <th>Duration</th>
            <th>Result</th>
          </tr>
        </thead>
        <tbody>
$testDetailsHtml
        </tbody>
      </table>
    </div>

    <div class="footer">
      Generated by GitHub Actions - Run #$RunNumber - $RunId
    </div>
  </div>
</body>
</html>
"@

$html | Out-File -FilePath $HtmlPath -Encoding UTF8
Write-Host "Enhanced HTML report generated: $HtmlPath"
Write-Host "Total test cases: $($testCases.Count)"
Write-Host "Passed: $passed | Failed: $failed | Skipped: $skipped"
