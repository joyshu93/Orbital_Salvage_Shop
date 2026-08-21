param(
    [ValidateSet('Repository', 'Submission')]
    [string]$Mode = 'Repository'
)

$ErrorActionPreference = 'Stop'
$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$required = @(
    'README.md',
    'Docs/ReleaseChecklist.md',
    'Docs/PrivacyPolicy.md',
    'Docs/Store/SamsungSellerSetup.md'
)

foreach ($relative in $required) {
    $path = Join-Path $projectRoot $relative
    if (-not (Test-Path -LiteralPath $path)) {
        throw "Missing release document: $relative"
    }
}

$releaseChecklist = Get-Content -LiteralPath (Join-Path $projectRoot 'Docs/ReleaseChecklist.md') -Raw
foreach ($forbidden in @('12 testers', 'Play Console personal account', 'Submit production access')) {
    if ($releaseChecklist.Contains($forbidden)) {
        throw "Legacy Play-only release instruction remains: $forbidden"
    }
}

$allPublicDocs = @(
    Get-Content -LiteralPath (Join-Path $projectRoot 'Docs/PrivacyPolicy.md') -Raw
    Get-Content -LiteralPath (Join-Path $projectRoot 'Docs/Store/SamsungSellerSetup.md') -Raw
) -join "`n"
if ($Mode -eq 'Submission' -and $allPublicDocs -match '\[(EFFECTIVE_DATE|DEVELOPER_DISPLAY_NAME|SUPPORT_EMAIL)\]') {
    throw 'Public identity fields are unresolved.'
}

Write-Host 'Release documentation gate passed.'
