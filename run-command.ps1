param(
    [Parameter(Mandatory=$true)]
    [string]$Command
)

# Execute the command and capture output
$output = Invoke-Expression $Command 2>&1
$exitCode = $LASTEXITCODE

# Write output
$output | ForEach-Object { Write-Host $_ }

# Exit with same code
exit $exitCode
