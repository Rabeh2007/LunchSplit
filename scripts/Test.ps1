# Load optional, untracked Sandbox settings without printing their values.
$projectRoot = Split-Path -Parent $PSScriptRoot
$configPath = Join-Path $projectRoot '.env.local'
if (Test-Path -LiteralPath $configPath) {
    foreach ($line in Get-Content -LiteralPath $configPath) {
        if ($line -match '^(SQUARE_SANDBOX_ACCESS_TOKEN|SQUARE_SANDBOX_LOCATION_ID)=(.+)$') {
            [Environment]::SetEnvironmentVariable($Matches[1], $Matches[2].Trim(), 'Process')
        }
    }
}

Push-Location $projectRoot
try {
    dotnet test @args
    $testExitCode = $LASTEXITCODE
}
finally {
    Pop-Location
}
exit $testExitCode
