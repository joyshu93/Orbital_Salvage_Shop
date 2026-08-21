param()

$ErrorActionPreference = 'Stop'
$sourceRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$gate = Join-Path $PSScriptRoot 'check-release-docs.ps1'
$tempBase = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
$fixtures = [System.Collections.Generic.List[string]]::new()

function Invoke-Gate([string]$Root, [string]$Mode) {
    & $gate -ProjectRoot $Root -Mode $Mode *> $null
}

function Expect-Pass([string]$Name, [scriptblock]$Action) {
    try {
        & $Action
        Write-Host "PASS: $Name"
    } catch {
        throw "Expected PASS for '$Name', but failed: $($_.Exception.Message)"
    }
}

function Expect-Fail([string]$Name, [scriptblock]$Action) {
    try {
        & $Action
    } catch {
        Write-Host "PASS: $Name rejected"
        return
    }

    throw "Expected FAIL for '$Name', but it passed."
}

function New-Fixture {
    $root = Join-Path $tempBase ("curio-release-docs-" + [Guid]::NewGuid().ToString('N'))
    [void][System.IO.Directory]::CreateDirectory($root)
    Copy-Item -LiteralPath (Join-Path $sourceRoot 'README.md') -Destination (Join-Path $root 'README.md')
    Copy-Item -LiteralPath (Join-Path $sourceRoot 'Docs') -Destination (Join-Path $root 'Docs') -Recurse
    $fixtures.Add($root)
    return $root
}

function Replace-Literal([string]$Root, [string]$RelativePath, [string]$Before, [string]$After) {
    $path = Join-Path $Root $RelativePath
    $text = [System.IO.File]::ReadAllText($path)
    if (-not $text.Contains($Before)) {
        throw "Fixture replacement source not found in ${RelativePath}: $Before"
    }
    [System.IO.File]::WriteAllText($path, $text.Replace($Before, $After))
}

function Replace-Pattern([string]$Root, [string]$RelativePath, [string]$Pattern, [string]$After) {
    $path = Join-Path $Root $RelativePath
    $text = [System.IO.File]::ReadAllText($path)
    $updated = [regex]::Replace($text, $Pattern, $After)
    if ($updated -eq $text) {
        throw "Fixture pattern did not match in ${RelativePath}: $Pattern"
    }
    [System.IO.File]::WriteAllText($path, $updated)
}

function Set-ConfirmedFixture([string]$Root) {
    Replace-Literal $Root 'Docs/PrivacyPolicy.md' '[EFFECTIVE_DATE]' '2026-09-01'
    Replace-Literal $Root 'Docs/PrivacyPolicy.md' '[DEVELOPER_DISPLAY_NAME]' 'Example Fixture Studio'
    Replace-Literal $Root 'Docs/PrivacyPolicy.md' '[SUPPORT_EMAIL]' 'support@example.invalid'
    Replace-Literal $Root 'Docs/PrivacyPolicy.md' '[PRIVACY_POLICY_URL]' 'https://example.invalid/privacy'

    Replace-Literal $Root 'Docs/Store/SamsungSellerSetup.md' 'Account registration status: PENDING_DEVELOPER_ACTION' 'Account registration status: REGISTERED'
    Replace-Literal $Root 'Docs/Store/SamsungSellerSetup.md' 'Identity / financial evidence status: NOT_SUBMITTED_OR_UNCONFIRMED' 'Identity / financial evidence status: SUBMITTED'
    Replace-Literal $Root 'Docs/Store/SamsungSellerSetup.md' 'Seller verification status: PENDING_DEVELOPER_CONFIRMATION' 'Seller verification status: VERIFIED'
    Replace-Literal $Root 'Docs/Store/SamsungSellerSetup.md' 'Seller verification date: PENDING' 'Seller verification date: 2026-09-01'
    Replace-Literal $Root 'Docs/Store/SamsungSellerSetup.md' '[DEVELOPER_DISPLAY_NAME]' 'Example Fixture Studio'
    Replace-Literal $Root 'Docs/Store/SamsungSellerSetup.md' '[SUPPORT_EMAIL]' 'support@example.invalid'
    Replace-Literal $Root 'Docs/Store/SamsungSellerSetup.md' '[PRIVACY_POLICY_URL]' 'https://example.invalid/privacy'

    Replace-Literal $Root 'Docs/Store/DataSafety.md' 'Reconciliation status: PENDING_FINAL_CONFIGURATION_CONFIRMATION' 'Reconciliation status: DEVELOPER_CONFIRMED'
    Replace-Literal $Root 'Docs/Store/DataSafety.md' 'Confirmation date: PENDING' 'Confirmation date: 2026-09-01'
    Replace-Literal $Root 'Docs/Store/DataSafety.md' 'Signed RC Git SHA: PENDING' 'Signed RC Git SHA: 0123456789abcdef0123456789abcdef01234567'

    Replace-Literal $Root 'Docs/Store/RatingAnswers.md' 'Questionnaire status: PENDING_DEVELOPER_CONFIRMATION' 'Questionnaire status: DEVELOPER_CONFIRMED'
    Replace-Literal $Root 'Docs/Store/RatingAnswers.md' 'Confirmation date: PENDING' 'Confirmation date: 2026-09-01'
    Replace-Literal $Root 'Docs/Store/RatingAnswers.md' 'Official rating result: PENDING' 'Official rating result: Fixture confirmed result'

    Replace-Literal $Root 'Docs/Store/AssetInventory.md' 'Media approval status: BLOCKED' 'Media approval status: HUMAN_APPROVED'
    Replace-Literal $Root 'Docs/Store/AssetInventory.md' 'Approval date: PENDING' 'Approval date: 2026-09-01'
    Replace-Pattern $Root 'Docs/Store/AssetInventory.md' '(?m)^\| Application icon \|.*$' '| Application icon | Fixture-approved icon | Ready for submission | `Docs/AIAssetProvenance.md`; `Docs/ArtReleaseReview.md` | Human approved. |'
    Replace-Pattern $Root 'Docs/Store/AssetInventory.md' '(?m)^\| Phone screenshots \|.*$' '| Phone screenshots | Fixture-approved screenshots | Ready for submission | `Docs/AIAssetProvenance.md`; `Docs/ThirdPartyNotices.md`; `Docs/ArtReleaseReview.md` | Human approved. |'
    $artReview = Join-Path $Root 'Docs/ArtReleaseReview.md'
    [System.IO.File]::WriteAllText($artReview, "# Fixture art release review`n`nHuman approval fixture only.`n")

    Replace-Literal $Root 'Docs/ReleaseEvidence/1.0.0/README.md' 'Evidence status: PENDING_DEVELOPER_EVIDENCE' 'Evidence status: DEVELOPER_CONFIRMED'
    Replace-Literal $Root 'Docs/ReleaseEvidence/1.0.0/README.md' 'RC decision: PENDING' 'RC decision: ACCEPTED'
    Replace-Literal $Root 'Docs/ReleaseEvidence/1.0.0/README.md' 'Decision date: PENDING' 'Decision date: 2026-09-01'
    Replace-Pattern $Root 'Docs/ReleaseEvidence/1.0.0/README.md' '(?m)^\| (Automated tests|AAB inspection|Owned-device validation|Remote Test Lab|Service validation|RC decision) \| (Not run|Pending developer evidence) \|' '| $1 | Developer evidence recorded |'
    foreach ($file in @('automated-tests.md', 'owned-device.md', 'remote-test-lab.md', 'service-validation.md', 'rc-decision.md')) {
        [System.IO.File]::WriteAllText((Join-Path $Root "Docs/ReleaseEvidence/1.0.0/$file"), "# Synthetic fixture`n")
    }
}

function New-ConfirmedFixture {
    $root = New-Fixture
    Set-ConfirmedFixture $root
    return $root
}

try {
    Expect-Pass 'current repository state' { Invoke-Gate $sourceRoot 'Repository' }
    Expect-Fail 'current submission state' { Invoke-Gate $sourceRoot 'Submission' }

    $confirmed = New-ConfirmedFixture
    Expect-Pass 'complete synthetic confirmed submission' { Invoke-Gate $confirmed 'Submission' }

    $stateMutations = @(
        @{ Name = 'account registration status'; File = 'Docs/Store/SamsungSellerSetup.md'; Before = 'Account registration status: REGISTERED'; After = 'Account registration status: PENDING_DEVELOPER_ACTION' },
        @{ Name = 'identity evidence status'; File = 'Docs/Store/SamsungSellerSetup.md'; Before = 'Identity / financial evidence status: SUBMITTED'; After = 'Identity / financial evidence status: NOT_SUBMITTED_OR_UNCONFIRMED' },
        @{ Name = 'seller verification status'; File = 'Docs/Store/SamsungSellerSetup.md'; Before = 'Seller verification status: VERIFIED'; After = 'Seller verification status: PENDING_DEVELOPER_CONFIRMATION' },
        @{ Name = 'seller verification date'; File = 'Docs/Store/SamsungSellerSetup.md'; Before = 'Seller verification date: 2026-09-01'; After = 'Seller verification date: tomorrow' },
        @{ Name = 'public developer name'; File = 'Docs/Store/SamsungSellerSetup.md'; Before = 'Public developer name: Example Fixture Studio'; After = 'Public developer name: [DEVELOPER_DISPLAY_NAME]' },
        @{ Name = 'public support email'; File = 'Docs/Store/SamsungSellerSetup.md'; Before = 'Public support email: support@example.invalid'; After = 'Public support email: not-an-email' },
        @{ Name = 'public privacy URL'; File = 'Docs/Store/SamsungSellerSetup.md'; Before = 'Public privacy policy URL: https://example.invalid/privacy'; After = 'Public privacy policy URL: http://example.invalid/privacy' },
        @{ Name = 'privacy URL parity'; File = 'Docs/PrivacyPolicy.md'; Before = 'Public URL: https://example.invalid/privacy'; After = 'Public URL: https://example.invalid/different' },
        @{ Name = 'privacy effective date'; File = 'Docs/PrivacyPolicy.md'; Before = 'Effective date: `2026-09-01`'; After = 'Effective date: `tomorrow`' },
        @{ Name = 'Data Safety status'; File = 'Docs/Store/DataSafety.md'; Before = 'Reconciliation status: DEVELOPER_CONFIRMED'; After = 'Reconciliation status: PENDING_FINAL_CONFIGURATION_CONFIRMATION' },
        @{ Name = 'Data Safety confirmation date'; File = 'Docs/Store/DataSafety.md'; Before = 'Confirmation date: 2026-09-01'; After = 'Confirmation date: PENDING' },
        @{ Name = 'signed RC SHA'; File = 'Docs/Store/DataSafety.md'; Before = 'Signed RC Git SHA: 0123456789abcdef0123456789abcdef01234567'; After = 'Signed RC Git SHA: short' },
        @{ Name = 'GMA version'; File = 'Docs/Store/DataSafety.md'; Before = 'Google Mobile Ads Unity version: 11.3.0'; After = 'Google Mobile Ads Unity version: 11.2.0' },
        @{ Name = 'EDM4U version'; File = 'Docs/Store/DataSafety.md'; Before = 'External Dependency Manager for Unity version: 1.2.188'; After = 'External Dependency Manager for Unity version: 1.2.187' },
        @{ Name = 'rating status'; File = 'Docs/Store/RatingAnswers.md'; Before = 'Questionnaire status: DEVELOPER_CONFIRMED'; After = 'Questionnaire status: PENDING_DEVELOPER_CONFIRMATION' },
        @{ Name = 'rating date'; File = 'Docs/Store/RatingAnswers.md'; Before = 'Confirmation date: 2026-09-01'; After = 'Confirmation date: PENDING' },
        @{ Name = 'rating result'; File = 'Docs/Store/RatingAnswers.md'; Before = 'Official rating result: Fixture confirmed result'; After = 'Official rating result: PENDING' },
        @{ Name = 'media status'; File = 'Docs/Store/AssetInventory.md'; Before = 'Media approval status: HUMAN_APPROVED'; After = 'Media approval status: BLOCKED' },
        @{ Name = 'media date'; File = 'Docs/Store/AssetInventory.md'; Before = 'Approval date: 2026-09-01'; After = 'Approval date: PENDING' },
        @{ Name = 'required icon readiness'; File = 'Docs/Store/AssetInventory.md'; Before = '| Application icon | Fixture-approved icon | Ready for submission |'; After = '| Application icon | Fixture prototype | Not uploaded |' },
        @{ Name = 'required screenshot readiness'; File = 'Docs/Store/AssetInventory.md'; Before = '| Phone screenshots | Fixture-approved screenshots | Ready for submission |'; After = '| Phone screenshots | No files created | Missing / not uploaded |' },
        @{ Name = 'evidence status'; File = 'Docs/ReleaseEvidence/1.0.0/README.md'; Before = 'Evidence status: DEVELOPER_CONFIRMED'; After = 'Evidence status: PENDING_DEVELOPER_EVIDENCE' },
        @{ Name = 'RC decision'; File = 'Docs/ReleaseEvidence/1.0.0/README.md'; Before = 'RC decision: ACCEPTED'; After = 'RC decision: PENDING' },
        @{ Name = 'RC decision date'; File = 'Docs/ReleaseEvidence/1.0.0/README.md'; Before = 'Decision date: 2026-09-01'; After = 'Decision date: PENDING' }
    )
    foreach ($mutation in $stateMutations) {
        $fixture = New-ConfirmedFixture
        Replace-Literal $fixture $mutation.File $mutation.Before $mutation.After
        Expect-Fail $mutation.Name { Invoke-Gate $fixture 'Submission' }
    }

    $missingArtReview = New-ConfirmedFixture
    [System.IO.File]::Delete((Join-Path $missingArtReview 'Docs/ArtReleaseReview.md'))
    Expect-Fail 'missing ArtReleaseReview' { Invoke-Gate $missingArtReview 'Submission' }

    $missingTask11Evidence = New-ConfirmedFixture
    [System.IO.File]::Delete((Join-Path $missingTask11Evidence 'Docs/ReleaseEvidence/1.0.0/service-validation.md'))
    Expect-Fail 'missing Task 11 evidence' { Invoke-Gate $missingTask11Evidence 'Submission' }

    $categoryParity = New-ConfirmedFixture
    Replace-Literal $categoryParity 'Docs/PrivacyPolicy.md' '<!-- AD_DATA_CATEGORY: PRODUCT_INTERACTIONS -->' '<!-- category removed by fixture -->'
    Expect-Fail 'AdMob/UMP category parity' { Invoke-Gate $categoryParity 'Submission' }

    $negativePolarity = New-ConfirmedFixture
    Replace-Literal $negativePolarity 'Docs/Store/GalaxyStoreListing.en.md' 'English and Korean are supported.' "English and Korean are supported.`n`nYou do not need to create an account. There is no guaranteed ad availability."
    Expect-Pass 'negative account/ad wording' { Invoke-Gate $negativePolarity 'Submission' }

    foreach ($claim in @(
        'Create an account to save progress.',
        'Ads are guaranteed.',
        'Purchase coins from the store.',
        'Sync your progress to the cloud.',
        'The game sends gameplay events.'
    )) {
        $affirmative = New-ConfirmedFixture
        Replace-Literal $affirmative 'Docs/Store/GalaxyStoreListing.en.md' 'English and Korean are supported.' "English and Korean are supported.`n`n$claim"
        Expect-Fail "affirmative contradiction: $claim" { Invoke-Gate $affirmative 'Submission' }
    }

    Write-Host 'Release-document gate mutation suite passed.'
} finally {
    foreach ($fixture in $fixtures) {
        $full = [System.IO.Path]::GetFullPath($fixture)
        if (-not $full.StartsWith($tempBase, [StringComparison]::OrdinalIgnoreCase) -or
            -not ([System.IO.Path]::GetFileName($full)).StartsWith('curio-release-docs-', [StringComparison]::Ordinal)) {
            throw "Refusing to delete unexpected fixture path: $full"
        }
        if ([System.IO.Directory]::Exists($full)) {
            [System.IO.Directory]::Delete($full, $true)
        }
    }
}
