param(
    [Parameter(Mandatory = $true)]
    [string]$TagName
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$originalRef = git -C $repoRoot rev-parse --abbrev-ref HEAD

try {
    git -C $repoRoot checkout $TagName | Out-Null
    dotnet build "$repoRoot/MinKlinik.slnx" | Out-Null
    dotnet test "$repoRoot/MinKlinik.slnx" | Out-Null
    Write-Host "OK: $TagName"
    exit 0
}
catch {
    Write-Host "FAIL: $TagName"
    Write-Host $_.Exception.Message
    exit 1
}
finally {
    git -C $repoRoot checkout $originalRef | Out-Null
}
