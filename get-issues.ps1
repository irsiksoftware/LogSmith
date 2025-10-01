# Get GitHub issues with labels
$issues = gh issue list --state open --json number,title,labels --limit 50 | ConvertFrom-Json

# Filter for in-progress or ready issues
$inProgress = @()
$ready = @()
$other = @()

foreach ($issue in $issues) {
    $labels = $issue.labels.name
    if ($labels -contains 'in-progress') {
        $inProgress += $issue
    } elseif ($labels -contains 'ready') {
        $ready += $issue
    } else {
        $other += $issue
    }
}

Write-Host "`n========== IN-PROGRESS ISSUES ==========" -ForegroundColor Cyan
if ($inProgress.Count -gt 0) {
    foreach ($issue in $inProgress) {
        Write-Host "#$($issue.number): $($issue.title)" -ForegroundColor Yellow
    }
} else {
    Write-Host "None" -ForegroundColor Gray
}

Write-Host "`n========== READY ISSUES ==========" -ForegroundColor Cyan
if ($ready.Count -gt 0) {
    foreach ($issue in $ready) {
        Write-Host "#$($issue.number): $($issue.title)" -ForegroundColor Green
    }
} else {
    Write-Host "None" -ForegroundColor Gray
}

Write-Host "`n========== OTHER OPEN ISSUES ==========" -ForegroundColor Cyan
foreach ($issue in $other) {
    $labelText = if ($issue.labels.name.Count -gt 0) { "[$($issue.labels.name -join ', ')]" } else { "[no labels]" }
    Write-Host "#$($issue.number): $($issue.title) $labelText" -ForegroundColor Gray
}
