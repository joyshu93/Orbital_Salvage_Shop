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

## Application icon release row

| Approved asset ID | Repository path | Release decision |
| --- | --- | --- |
| ART-BRAND-001 | Assets/Art/Brand/AppIcon.png | Approved for release |

Release approval status: HUMAN_APPROVED
Approval date: 2026-09-01
Developer attestation: I directly made and reviewed the creative changes documented below and approve this application icon for release.
Human creative pass: COMPLETED
Approved asset ID: ART-BRAND-001
Repository path: Assets/Art/Brand/AppIcon.png
Release decision: Approved for release
Composition changes: Rebalanced the moon, tag, and cabinet so the focal point remains legible at launcher size.
Silhouette changes: Redrew the outer cabinet contour and widened the tag gap for a distinct small-size silhouette.
Palette changes: Chose a restrained amber, plum, and cream palette and manually tuned contrast for the store tile.
Line / shape cleanup: Rebuilt uneven contours, removed stray marks, and normalized corner radii by hand.
Before evidence: Git commit 1111111111111111111111111111111111111111
After evidence: Assets/Art/Brand/AppIcon.png
Similarity search method: Developer performed reverse-image and Galaxy Store keyword searches for confusingly similar icons.
Similarity review result: PASSED
Trademark review: PASSED
Rights review: PASSED
Watermark review: PASSED
Signature review: PASSED
Named-artist review: PASSED
Protected-character review: PASSED
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
EditMode XML path: Docs/ReleaseEvidence/1.0.0/logs/editmode.xml
EditMode log path: Docs/ReleaseEvidence/1.0.0/logs/editmode.log
PlayMode XML path: Docs/ReleaseEvidence/1.0.0/logs/playmode.xml
PlayMode log path: Docs/ReleaseEvidence/1.0.0/logs/playmode.log
"@
        'aab-inspection.md' = @"
# Synthetic AAB inspection evidence
Evidence status: DEVELOPER_RECORDED
Evidence date: 2026-09-01
RC Git SHA: $rcSha
Inspection status: PASSED
Bundletool version: 1.18.3
AAB SHA-256: $aabSha
Hash match: PASSED
Package ID: com.joyshu93.curioclerknightshift
Version name: 1.0.0
Version code: 10000
Minimum API: 29
Target API: 36
Architecture: ARM64
Backend: IL2CPP
Symbols: PRESENT
"@
        'owned-device.md' = @"
# Synthetic owned-device evidence
Evidence status: DEVELOPER_RECORDED
Evidence date: 2026-09-01
RC Git SHA: $rcSha
AAB SHA-256: $aabSha
Matrix status: PASSED
Owned device model: Fixture Galaxy S device
Android version: 16
Android API: 36
Resolution / aspect: 1080x2340 / 19.5:9
Install source: bundletool-generated universal APK from the recorded AAB
Build version name: 1.0.0
Build version code: 10000
First launch: PASSED
Tutorial: PASSED
Three shifts: PASSED
Drag / buttons / Hold: PASSED
Offline mode: PASSED
Pause / resume: PASSED
Force-stop recovery: PASSED
Corrupt-save recovery: PASSED
EN / KO language: PASSED
UMP grant: PASSED
UMP deny: PASSED
UMP privacy options: PASSED
Ad earned: PASSED
Ad dismissed: PASSED
Ad no-fill: PASSED
Ad failure: PASSED
Ad duplicate callback: PASSED
Relaunch: PASSED
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
Galaxy A model: Fixture Galaxy A55
Galaxy A Android major: 14
Galaxy A aspect class: TALL_SLAB
Galaxy S model: Fixture Galaxy S25
Galaxy S Android major: 15
Galaxy S aspect class: STANDARD_SLAB
Galaxy Fold model: Fixture Galaxy Z Fold6
Galaxy Fold Android major: 15
Galaxy Fold aspect class: FOLDABLE
Galaxy A install: PASSED
Galaxy A launch: PASSED
Galaxy A tutorial: PASSED
Galaxy A one shift: PASSED
Galaxy A language: PASSED
Galaxy A safe area: PASSED
Galaxy A pause / resume: PASSED
Galaxy S install: PASSED
Galaxy S launch: PASSED
Galaxy S tutorial: PASSED
Galaxy S one shift: PASSED
Galaxy S language: PASSED
Galaxy S safe area: PASSED
Galaxy S pause / resume: PASSED
Galaxy Fold install: PASSED
Galaxy Fold launch: PASSED
Galaxy Fold tutorial: PASSED
Galaxy Fold one shift: PASSED
Galaxy Fold language: PASSED
Galaxy Fold safe area: PASSED
Galaxy Fold pause / resume: PASSED
"@
        'service-validation.md' = @"
# Synthetic service-validation evidence
Evidence status: DEVELOPER_RECORDED
Evidence date: 2026-09-01
RC Git SHA: $rcSha
Service validation status: PASSED
No-remote Release gate: PASSED
Google Mobile Ads Unity version: 11.3.0
EDM4U version: 1.2.188
Observed service traffic: ADMOB_UMP_ONLY
Duplicate reward grants: 0
Unavailable-ad base progression: PASSED
UMP update every launch: PASSED
Ad requests before CanRequestAds: 0
Earned rewards: 1
Gameplay / crash endpoints observed: 0
Package graph remote telemetry: ABSENT
Local payload transmissions: 0
Local payload logs: 0
Local payload cache writes: 0
Local payload persistence writes: 0
"@
        'rc-decision.md' = @"
# Synthetic RC decision
Evidence status: DEVELOPER_RECORDED
Evidence date: 2026-09-01
RC Git SHA: $rcSha
AAB SHA-256: $aabSha
RC Decision: ACCEPT RC
Version name: 1.0.0
Version code: 10000
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
    Replace-Literal $replacementIcon 'Docs/ArtReleaseReview.md' '| ART-BRAND-001 | Assets/Art/Brand/AppIcon.png |' '| ART-BRAND-002 | Assets/Art/Brand/AppIcon.png |'
    Replace-Literal $replacementIcon 'Docs/Store/AssetInventory.md' 'ART-BRAND-001' 'ART-BRAND-002'
    $provenancePath = Join-Path $replacementIcon 'Docs/AIAssetProvenance.md'
    $provenance = [System.IO.File]::ReadAllText($provenancePath, [System.Text.Encoding]::UTF8)
    [System.IO.File]::WriteAllText($provenancePath, "$provenance`n### ART-BRAND-002 — synthetic replacement`n`n| Repository path | `Assets/Art/Brand/AppIcon.png` |`n| SHA-256 | $replacementIconHash |`n", $utf8NoBom)
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

    $missingAabInspection = New-ConfirmedFixture
    [System.IO.File]::Delete((Join-Path $missingAabInspection 'Docs/ReleaseEvidence/1.0.0/aab-inspection.md'))
    Expect-Fail 'missing AAB inspection evidence' { Invoke-Gate $missingAabInspection 'Submission' }

    $categoryParity = New-ConfirmedFixture
    Replace-Literal $categoryParity 'Docs/PrivacyPolicy.md' '<!-- AD_DATA_CATEGORY: PRODUCT_INTERACTIONS -->' '<!-- category removed by fixture -->'
    Expect-Fail 'AdMob/UMP category parity' { Invoke-Gate $categoryParity 'Submission' }

    $deepEvidenceMutations = @(
        @{ Name = 'art approval status'; File = 'Docs/ArtReleaseReview.md'; Before = 'Release approval status: HUMAN_APPROVED'; After = 'Release approval status: BLOCKED' },
        @{ Name = 'art icon hash mismatch'; File = 'Docs/ArtReleaseReview.md'; Before = 'Approved icon SHA-256: '; After = 'Approved icon SHA-256: 0000000000000000000000000000000000000000000000000000000000000000`nOriginal hash: ' },
        @{ Name = 'art row release decision'; File = 'Docs/ArtReleaseReview.md'; Before = '| ART-BRAND-001 | Assets/Art/Brand/AppIcon.png | Approved for release |'; After = '| ART-BRAND-001 | Assets/Art/Brand/AppIcon.png | Blocked |' },
        @{ Name = 'art developer attestation'; File = 'Docs/ArtReleaseReview.md'; Before = 'Developer attestation: I directly made and reviewed the creative changes documented below and approve this application icon for release.'; After = 'Developer attestation: looks good' },
        @{ Name = 'art composition substance'; File = 'Docs/ArtReleaseReview.md'; Before = 'Composition changes: Rebalanced the moon, tag, and cabinet so the focal point remains legible at launcher size.'; After = 'Composition changes: changed it' },
        @{ Name = 'art before evidence'; File = 'Docs/ArtReleaseReview.md'; Before = 'Before evidence: Git commit 1111111111111111111111111111111111111111'; After = 'Before evidence: none' },
        @{ Name = 'art similarity method'; File = 'Docs/ArtReleaseReview.md'; Before = 'Similarity search method: Developer performed reverse-image and Galaxy Store keyword searches for confusingly similar icons.'; After = 'Similarity search method: checked' },
        @{ Name = 'art trademark result'; File = 'Docs/ArtReleaseReview.md'; Before = 'Trademark review: PASSED'; After = 'Trademark review: FAILED' },
        @{ Name = 'automated EditMode counts'; File = 'Docs/ReleaseEvidence/1.0.0/automated-tests.md'; Before = 'EditMode passed: 24'; After = 'EditMode passed: 23' },
        @{ Name = 'automated Unity version'; File = 'Docs/ReleaseEvidence/1.0.0/automated-tests.md'; Before = 'Unity version: 6000.3.21f1'; After = 'Unity version: 6000.3.20f1' },
        @{ Name = 'automated retained path'; File = 'Docs/ReleaseEvidence/1.0.0/automated-tests.md'; Before = 'EditMode XML path: Docs/ReleaseEvidence/1.0.0/logs/editmode.xml'; After = 'EditMode XML path: editmode.xml' },
        @{ Name = 'AAB bundletool version'; File = 'Docs/ReleaseEvidence/1.0.0/aab-inspection.md'; Before = 'Bundletool version: 1.18.3'; After = 'Bundletool version: 1.17.2' },
        @{ Name = 'AAB target API'; File = 'Docs/ReleaseEvidence/1.0.0/aab-inspection.md'; Before = 'Target API: 36'; After = 'Target API: 35' },
        @{ Name = 'AAB symbols'; File = 'Docs/ReleaseEvidence/1.0.0/aab-inspection.md'; Before = 'Symbols: PRESENT'; After = 'Symbols: ABSENT' },
        @{ Name = 'owned-device P0 defect'; File = 'Docs/ReleaseEvidence/1.0.0/owned-device.md'; Before = 'P0 defects: 0'; After = 'P0 defects: 1' },
        @{ Name = 'owned-device reward anomaly'; File = 'Docs/ReleaseEvidence/1.0.0/owned-device.md'; Before = 'Reward anomalies: 0'; After = 'Reward anomalies: 1' },
        @{ Name = 'owned-device explicit check'; File = 'Docs/ReleaseEvidence/1.0.0/owned-device.md'; Before = 'Ad no-fill: PASSED'; After = 'Ad no-fill: FAILED' },
        @{ Name = 'owned-device build version'; File = 'Docs/ReleaseEvidence/1.0.0/owned-device.md'; Before = 'Build version code: 10000'; After = 'Build version code: 9999' },
        @{ Name = 'RTL profile coverage'; File = 'Docs/ReleaseEvidence/1.0.0/remote-test-lab.md'; Before = 'Galaxy Fold model: Fixture Galaxy Z Fold6'; After = 'Galaxy Fold model: PENDING' },
        @{ Name = 'RTL Android-major diversity'; File = 'Docs/ReleaseEvidence/1.0.0/remote-test-lab.md'; Before = 'Galaxy A Android major: 14'; After = 'Galaxy A Android major: 15' },
        @{ Name = 'RTL explicit check'; File = 'Docs/ReleaseEvidence/1.0.0/remote-test-lab.md'; Before = 'Galaxy Fold safe area: PASSED'; After = 'Galaxy Fold safe area: FAILED' },
        @{ Name = 'inconsistent evidence RC SHA'; File = 'Docs/ReleaseEvidence/1.0.0/remote-test-lab.md'; Before = 'RC Git SHA: 0123456789abcdef0123456789abcdef01234567'; After = 'RC Git SHA: 1111111111111111111111111111111111111111' },
        @{ Name = 'service no-remote failure'; File = 'Docs/ReleaseEvidence/1.0.0/service-validation.md'; Before = 'No-remote Release gate: PASSED'; After = 'No-remote Release gate: FAILED' },
        @{ Name = 'service duplicate reward'; File = 'Docs/ReleaseEvidence/1.0.0/service-validation.md'; Before = 'Duplicate reward grants: 0'; After = 'Duplicate reward grants: 1' },
        @{ Name = 'service GMA version'; File = 'Docs/ReleaseEvidence/1.0.0/service-validation.md'; Before = 'Google Mobile Ads Unity version: 11.3.0'; After = 'Google Mobile Ads Unity version: 11.2.0' },
        @{ Name = 'service gameplay endpoint'; File = 'Docs/ReleaseEvidence/1.0.0/service-validation.md'; Before = 'Gameplay / crash endpoints observed: 0'; After = 'Gameplay / crash endpoints observed: 1' },
        @{ Name = 'service local persistence'; File = 'Docs/ReleaseEvidence/1.0.0/service-validation.md'; Before = 'Local payload persistence writes: 0'; After = 'Local payload persistence writes: 1' },
        @{ Name = 'RC nonzero P1'; File = 'Docs/ReleaseEvidence/1.0.0/rc-decision.md'; Before = 'P1 defects: 0'; After = 'P1 defects: 1' },
        @{ Name = 'RC version code'; File = 'Docs/ReleaseEvidence/1.0.0/rc-decision.md'; Before = 'Version code: 10000'; After = 'Version code: 9999' },
        @{ Name = 'RC AAB SHA mismatch'; File = 'Docs/ReleaseEvidence/1.0.0/rc-decision.md'; Before = 'AAB SHA-256: ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789'; After = 'AAB SHA-256: 1111111111111111111111111111111111111111111111111111111111111111' }
    )
    foreach ($mutation in $deepEvidenceMutations) {
        $fixture = New-ConfirmedFixture
        Replace-Literal $fixture $mutation.File $mutation.Before $mutation.After
        Expect-Fail $mutation.Name { Invoke-Gate $fixture 'Submission' }
    }

    $rtlAspectDiversity = New-ConfirmedFixture
    Replace-Literal $rtlAspectDiversity 'Docs/ReleaseEvidence/1.0.0/remote-test-lab.md' 'Galaxy A aspect class: TALL_SLAB' 'Galaxy A aspect class: FOLDABLE'
    Replace-Literal $rtlAspectDiversity 'Docs/ReleaseEvidence/1.0.0/remote-test-lab.md' 'Galaxy S aspect class: STANDARD_SLAB' 'Galaxy S aspect class: FOLDABLE'
    Expect-Fail 'RTL aspect diversity' { Invoke-Gate $rtlAspectDiversity 'Submission' }

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

    $readmeToken = New-ConfirmedFixture
    Replace-Literal $readmeToken 'Docs/ReleaseEvidence/1.0.0/README.md' 'Decision date: 2026-09-01' 'Decision date: 2026-09-01`nLeak: [UNRESOLVED_EVIDENCE]'
    Expect-Fail 'recursive README unresolved token' { Invoke-Gate $readmeToken 'Submission' }

    $requiredUnc = New-ConfirmedFixture
    Replace-Literal $requiredUnc 'Docs/ReleaseEvidence/1.0.0/automated-tests.md' 'PlayMode log path: Docs/ReleaseEvidence/1.0.0/logs/playmode.log' 'PlayMode log path: \\server\share\playmode.log'
    Expect-Fail 'required evidence UNC path' { Invoke-Gate $requiredUnc 'Submission' }

    $futureEvidence = New-ConfirmedFixture
    [System.IO.File]::WriteAllText((Join-Path $futureEvidence 'Docs/ReleaseEvidence/1.0.0/future-note.md'), "Evidence status: PENDING`nSource: \\server\share\note`nToken: github_pat_abcdefghijklmnopqrstuvwxyz1234567890", $utf8NoBom)
    Expect-Fail 'recursive future evidence injection' { Invoke-Gate $futureEvidence 'Submission' }

    $binaryEvidence = New-ConfirmedFixture
    [System.IO.File]::WriteAllBytes((Join-Path $binaryEvidence 'Docs/ReleaseEvidence/1.0.0/raw.bin'), [byte[]](0, 1, 2, 3))
    Expect-Fail 'binary evidence file' { Invoke-Gate $binaryEvidence 'Submission' }

    $binaryMarkdown = New-ConfirmedFixture
    [System.IO.File]::WriteAllBytes((Join-Path $binaryMarkdown 'Docs/ReleaseEvidence/1.0.0/future-note.md'), [byte[]](0, 1, 2, 3))
    Expect-Fail 'binary content disguised as Markdown' { Invoke-Gate $binaryMarkdown 'Submission' }

    $negativePolarity = New-ConfirmedFixture
    Replace-Literal $negativePolarity 'Docs/Store/GalaxyStoreListing.en.md' 'English and Korean are supported.' "English and Korean are supported.`n`nYou do not need to create an account. There is no guaranteed ad availability. No IAP, cloud save, or remote telemetry is provided."
    Expect-Pass 'negative account/ad wording' { Invoke-Gate $negativePolarity 'Submission' }

    foreach ($claim in @(
        'Create an account to save progress.',
        'You can create an account to save progress.',
        'You may create an account to save progress.',
        'Ads are guaranteed.',
        'An ad will always be available.',
        'Ads are always available.',
        'Purchase coins from the store.',
        'You can buy coins.',
        'Sync your progress to the cloud.',
        'Your progress syncs to the cloud.',
        'Cloud saves are available.',
        'The game sends gameplay events.',
        'Gameplay events are sent to the developer.',
        'We upload gameplay data to our servers.'
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
