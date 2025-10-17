while ($true) {
    $data = gh pr view 83 --json statusCheckRollup | ConvertFrom-Json
    $incomplete = ($data.statusCheckRollup | Where-Object { $_.status -ne 'COMPLETED' }).Count
    if ($incomplete -eq 0) { break }
    Write-Host "Waiting for $incomplete checks..."
    Start-Sleep -Seconds 20
}
Write-Host 'All checks complete'
gh pr view 83 --json statusCheckRollup
