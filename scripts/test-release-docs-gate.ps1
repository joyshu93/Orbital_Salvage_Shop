param()

$ErrorActionPreference = 'Stop'
$sourceRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$gate = Join-Path $PSScriptRoot 'check-release-docs.ps1'
$tempBase = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
$fixtures = [System.Collections.Generic.List[string]]::new()
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)

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
    $iconDirectory = Join-Path $root 'Assets/Art/Brand'
    [void][System.IO.Directory]::CreateDirectory($iconDirectory)
    Copy-Item -LiteralPath (Join-Path $sourceRoot 'Assets/Art/Brand/AppIcon.png') -Destination (Join-Path $iconDirectory 'AppIcon.png')
    $fixtures.Add($root)
    return $root
}

function Replace-Literal([string]$Root, [string]$RelativePath, [string]$Before, [string]$After) {
    $path = Join-Path $Root $RelativePath
    $text = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
    if (-not $text.Contains($Before)) {
        throw "Fixture replacement source not found in ${RelativePath}: $Before"
    }
    [System.IO.File]::WriteAllText($path, $text.Replace($Before, $After), $utf8NoBom)
}

function Replace-Pattern([string]$Root, [string]$RelativePath, [string]$Pattern, [string]$After) {
    $path = Join-Path $Root $RelativePath
    $text = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
    $updated = [regex]::Replace($text, $Pattern, $After)
    if ($updated -eq $text) {
        throw "Fixture pattern did not match in ${RelativePath}: $Pattern"
    }
    [System.IO.File]::WriteAllText($path, $updated, $utf8NoBom)
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
    Replace-Pattern $Root 'Docs/Store/AssetInventory.md' '(?m)^\| Application icon \|.*$' '| Application icon | Fixture-approved icon | Ready for submission | `Docs/AIAssetProvenance.md` `ART-BRAND-001`; `Docs/ArtReleaseReview.md` | Human approved. |'
    Replace-Pattern $Root 'Docs/Store/AssetInventory.md' '(?m)^\| Phone screenshots \|.*$' '| Phone screenshots | Fixture-approved screenshots | Ready for submission | `Docs/AIAssetProvenance.md`; `Docs/ThirdPartyNotices.md`; `Docs/ArtReleaseReview.md` | Human approved. |'
    $artReview = Join-Path $Root 'Docs/ArtReleaseReview.md'
    $iconHash = (Get-FileHash -LiteralPath (Join-Path $Root 'Assets/Art/Brand/AppIcon.png') -Algorithm SHA256).Hash
    $artReviewContent = @"
# Fixture art release review

Release approval status: HUMAN_APPROVED
Approval date: 2026-09-01
Reviewer / attestation: Example fixture reviewer confirms this sanitized approval record.
Human creative pass: COMPLETED
Similarity review: PASSED
Rights review: PASSED
Approved asset ID: ART-BRAND-001
Approved icon SHA-256: $iconHash
"@
    [System.IO.File]::WriteAllText($artReview, $artReviewContent, $utf8NoBom)

    Replace-Literal $Root 'Docs/ReleaseEvidence/1.0.0/README.md' 'Evidence status: PENDING_DEVELOPER_EVIDENCE' 'Evidence status: DEVELOPER_CONFIRMED'
    Replace-Literal $Root 'Docs/ReleaseEvidence/1.0.0/README.md' 'RC decision: PENDING' 'RC decision: ACCEPTED'
    Replace-Literal $Root 'Docs/ReleaseEvidence/1.0.0/README.md' 'Decision date: PENDING' 'Decision date: 2026-09-01'
    Replace-Pattern $Root 'Docs/ReleaseEvidence/1.0.0/README.md' '(?m)^\| (Automated tests|AAB inspection|Owned-device validation|Remote Test Lab|Service validation|RC decision) \| (Not run|Pending developer evidence) \|' '| $1 | Developer evidence recorded |'
    $rcSha = '0123456789abcdef0123456789abcdef01234567'
    $aabSha = 'ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789'
    $evidence = @{
        'automated-tests.md' = @"
# Synthetic automated-test evidence
Evidence status: DEVELOPER_RECORDED
Evidence date: 2026-09-01
RC Git SHA: $rcSha
Unity version: 6000.3.21f1
EditMode status: PASSED
EditMode passed: 24
EditMode total: 24
PlayMode status: PASSED
PlayMode passed: 12
PlayMode total: 12
"@
        'owned-device.md' = @"
# Synthetic owned-device evidence
Evidence status: DEVELOPER_RECORDED
Evidence date: 2026-09-01
RC Git SHA: $rcSha
AAB SHA-256: $aabSha
Matrix status: PASSED
Owned device model: Fixture Galaxy S device
Android API: 36
First launch: PASSED
Tutorial: PASSED
Three shifts: PASSED
P0 defects: 0
P1 defects: 0
Reward anomalies: 0
"@
        'remote-test-lab.md' = @"
# Synthetic Remote Test Lab evidence
Evidence status: DEVELOPER_RECORDED
Evidence date: 2026-09-01
RC Git SHA: $rcSha
Matrix status: PASSED
Profile count: 3
Galaxy A-series profile: Fixture Galaxy A | Android 14 | slab
Galaxy S-series profile: Fixture Galaxy S | Android 15 | slab
Galaxy Fold profile: Fixture Galaxy Fold | Android 15 | foldable
"@
        'service-validation.md' = @"
# Synthetic service-validation evidence
Evidence status: DEVELOPER_RECORDED
Evidence date: 2026-09-01
RC Git SHA: $rcSha
Service validation status: PASSED
No-remote Release gate: PASSED
Observed service traffic: ADMOB_UMP_ONLY
Duplicate reward grants: 0
Unavailable-ad base progression: PASSED
UMP launch update: PASSED
Ad requests before CanRequestAds: 0
"@
        'rc-decision.md' = @"
# Synthetic RC decision
Evidence status: DEVELOPER_RECORDED
Evidence date: 2026-09-01
RC Git SHA: $rcSha
AAB SHA-256: $aabSha
RC Decision: ACCEPT RC
P0 defects: 0
P1 defects: 0
Rights gate: PASSED
Store docs gate: PASSED
Test matrix: PASSED
"@
    }
    foreach ($file in $evidence.Keys) {
        [System.IO.File]::WriteAllText((Join-Path $Root "Docs/ReleaseEvidence/1.0.0/$file"), $evidence[$file], $utf8NoBom)
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

    $replacementIcon = New-ConfirmedFixture
    $replacementIconHash = (Get-FileHash -LiteralPath (Join-Path $replacementIcon 'Assets/Art/Brand/AppIcon.png') -Algorithm SHA256).Hash
    Replace-Literal $replacementIcon 'Docs/ArtReleaseReview.md' 'Approved asset ID: ART-BRAND-001' 'Approved asset ID: ART-BRAND-002'
    Replace-Literal $replacementIcon 'Docs/Store/AssetInventory.md' 'ART-BRAND-001' 'ART-BRAND-002'
    $provenancePath = Join-Path $replacementIcon 'Docs/AIAssetProvenance.md'
    $provenance = [System.IO.File]::ReadAllText($provenancePath, [System.Text.Encoding]::UTF8)
    [System.IO.File]::WriteAllText($provenancePath, "$provenance`n### ART-BRAND-002 — synthetic replacement`n`n| SHA-256 | $replacementIconHash |`n", $utf8NoBom)
    Expect-Pass 'separately documented replacement icon asset ID' { Invoke-Gate $replacementIcon 'Submission' }

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

    $deepEvidenceMutations = @(
        @{ Name = 'art approval status'; File = 'Docs/ArtReleaseReview.md'; Before = 'Release approval status: HUMAN_APPROVED'; After = 'Release approval status: BLOCKED' },
        @{ Name = 'art icon hash mismatch'; File = 'Docs/ArtReleaseReview.md'; Before = 'Approved icon SHA-256: '; After = 'Approved icon SHA-256: 0000000000000000000000000000000000000000000000000000000000000000`nOriginal hash: ' },
        @{ Name = 'automated EditMode counts'; File = 'Docs/ReleaseEvidence/1.0.0/automated-tests.md'; Before = 'EditMode passed: 24'; After = 'EditMode passed: 23' },
        @{ Name = 'automated Unity version'; File = 'Docs/ReleaseEvidence/1.0.0/automated-tests.md'; Before = 'Unity version: 6000.3.21f1'; After = 'Unity version: 6000.3.20f1' },
        @{ Name = 'owned-device P0 defect'; File = 'Docs/ReleaseEvidence/1.0.0/owned-device.md'; Before = 'P0 defects: 0'; After = 'P0 defects: 1' },
        @{ Name = 'owned-device reward anomaly'; File = 'Docs/ReleaseEvidence/1.0.0/owned-device.md'; Before = 'Reward anomalies: 0'; After = 'Reward anomalies: 1' },
        @{ Name = 'RTL profile coverage'; File = 'Docs/ReleaseEvidence/1.0.0/remote-test-lab.md'; Before = 'Galaxy Fold profile: Fixture Galaxy Fold | Android 15 | foldable'; After = 'Galaxy Fold profile: PENDING' },
        @{ Name = 'inconsistent evidence RC SHA'; File = 'Docs/ReleaseEvidence/1.0.0/remote-test-lab.md'; Before = 'RC Git SHA: 0123456789abcdef0123456789abcdef01234567'; After = 'RC Git SHA: 1111111111111111111111111111111111111111' },
        @{ Name = 'service no-remote failure'; File = 'Docs/ReleaseEvidence/1.0.0/service-validation.md'; Before = 'No-remote Release gate: PASSED'; After = 'No-remote Release gate: FAILED' },
        @{ Name = 'service duplicate reward'; File = 'Docs/ReleaseEvidence/1.0.0/service-validation.md'; Before = 'Duplicate reward grants: 0'; After = 'Duplicate reward grants: 1' },
        @{ Name = 'RC nonzero P1'; File = 'Docs/ReleaseEvidence/1.0.0/rc-decision.md'; Before = 'P1 defects: 0'; After = 'P1 defects: 1' },
        @{ Name = 'RC AAB SHA mismatch'; File = 'Docs/ReleaseEvidence/1.0.0/rc-decision.md'; Before = 'AAB SHA-256: ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789'; After = 'AAB SHA-256: 1111111111111111111111111111111111111111111111111111111111111111' }
    )
    foreach ($mutation in $deepEvidenceMutations) {
        $fixture = New-ConfirmedFixture
        Replace-Literal $fixture $mutation.File $mutation.Before $mutation.After
        Expect-Fail $mutation.Name { Invoke-Gate $fixture 'Submission' }
    }

    foreach ($injection in @(
        @{ Name = 'machine path injection'; Text = 'Retained log: C:\Users\Fixture\test.log' },
        @{ Name = 'forward absolute path injection'; Text = 'Retained log: /opt/fixture/test.log' },
        @{ Name = 'file URI injection'; Text = 'Retained log: file:///C:/Fixture/test.log' },
        @{ Name = 'credential injection'; Text = 'Access token: ghp_abcdefghijklmnopqrstuvwxyz123456' },
        @{ Name = 'real AdMob ID injection'; Text = 'Ad unit: ca-app-pub-1234567890123456/1234567890' },
        @{ Name = 'identity record injection'; Text = 'Government ID: fixture-record' },
        @{ Name = 'pending evidence injection'; Text = 'Extra verification: PENDING' }
    )) {
        $fixture = New-ConfirmedFixture
        Replace-Literal $fixture 'Docs/ReleaseEvidence/1.0.0/service-validation.md' 'Ad requests before CanRequestAds: 0' "Ad requests before CanRequestAds: 0`n$($injection.Text)"
        Expect-Fail $injection.Name { Invoke-Gate $fixture 'Submission' }
    }

    $negativePolarity = New-ConfirmedFixture
    Replace-Literal $negativePolarity 'Docs/Store/GalaxyStoreListing.en.md' 'English and Korean are supported.' "English and Korean are supported.`n`nYou do not need to create an account. There is no guaranteed ad availability."
    Expect-Pass 'negative account/ad wording' { Invoke-Gate $negativePolarity 'Submission' }

    foreach ($claim in @(
        'Create an account to save progress.',
        'You can create an account to save progress.',
        'Ads are guaranteed.',
        'An ad will always be available.',
        'Purchase coins from the store.',
        'You can buy coins.',
        'Sync your progress to the cloud.',
        'Your progress syncs to the cloud.',
        'The game sends gameplay events.',
        'Gameplay events are sent to the developer.'
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
