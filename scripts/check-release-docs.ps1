param(
    [ValidateSet('Repository', 'Submission')]
    [string]$Mode = 'Repository',

    [string]$ProjectRoot
)

$ErrorActionPreference = 'Stop'
$root = if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
} else {
    [System.IO.Path]::GetFullPath($ProjectRoot)
}

$required = @(
    'README.md',
    'Docs/ReleaseChecklist.md',
    'Docs/PrivacyPolicy.md',
    'Docs/AIAssetProvenance.md',
    'Docs/ThirdPartyNotices.md',
    'Docs/Store/SamsungSellerSetup.md',
    'Docs/Store/GalaxyStoreListing.ko.md',
    'Docs/Store/GalaxyStoreListing.en.md',
    'Docs/Store/ReviewNotes.md',
    'Docs/Store/DataSafety.md',
    'Docs/Store/RatingAnswers.md',
    'Docs/Store/AssetInventory.md',
    'Docs/ReleaseEvidence/1.0.0/README.md'
)

function Read-ReleaseDocument([string]$RelativePath) {
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing release document: $RelativePath"
    }

    return Get-Content -LiteralPath $path -Raw
}

function Assert-Contains([string]$Text, [string]$Pattern, [string]$Message) {
    if ($Text -notmatch $Pattern) {
        throw $Message
    }
}

function Get-FactIds([string]$Text) {
    return @([regex]::Matches($Text, '(?m)^-\s+(FACT_[A-Z0-9_]+)\s*$') | ForEach-Object { $_.Groups[1].Value } | Sort-Object -Unique)
}

$documents = @{}
foreach ($relative in $required) {
    $documents[$relative] = Read-ReleaseDocument $relative
}

$releaseChecklist = $documents['Docs/ReleaseChecklist.md']
foreach ($forbidden in @('12 testers', 'Play Console personal account', 'Submit production access')) {
    if ($releaseChecklist.Contains($forbidden)) {
        throw "Legacy Play-only release instruction remains: $forbidden"
    }
}

$listingEn = $documents['Docs/Store/GalaxyStoreListing.en.md']
$listingKo = $documents['Docs/Store/GalaxyStoreListing.ko.md']
$expectedFactIds = @(
    'FACT_WARM_OCCULT_RULE_SORTING',
    'FACT_PORTRAIT_ONE_HAND',
    'FACT_TWELVE_CURIO_SHIFT',
    'FACT_THREE_DESTINATIONS',
    'FACT_HOLD_SLOT',
    'FACT_CASEBOOK',
    'FACT_DESK_CHARMS',
    'FACT_OFFLINE_PLAY',
    'FACT_OPTIONAL_REWARDED_ADS',
    'FACT_NO_ACCOUNT',
    'FACT_NO_BACKEND_OR_CLOUD_SAVE',
    'FACT_NO_IAP'
) | Sort-Object

$enFactIds = Get-FactIds $listingEn
$koFactIds = Get-FactIds $listingKo
if (($enFactIds -join '|') -ne ($expectedFactIds -join '|') -or
    ($koFactIds -join '|') -ne ($expectedFactIds -join '|')) {
    throw 'English/Korean store listing fact parity is incomplete or inconsistent.'
}

Assert-Contains $listingEn '(?m)^# Galaxy Store listing — English$' 'English listing heading is missing.'
Assert-Contains $listingKo '(?m)^# Galaxy Store 등록 문구 — 한국어$' 'Korean listing heading is missing.'
foreach ($listing in @($listingEn, $listingKo)) {
    Assert-Contains $listing '(?m)^Package ID:\s+`com\.joyshu93\.curioclerknightshift`\s*$' 'Store listing package ID is missing or wrong.'
    Assert-Contains $listing '(?m)^Release version:\s+`1\.0\.0`\s*$' 'Store listing version is missing or wrong.'
    Assert-Contains $listing '(?m)^Account:\s+None\s*$' 'Store listing must declare no account.'
    Assert-Contains $listing '(?m)^Gameplay backend:\s+None\s*$' 'Store listing must declare no gameplay backend.'
    Assert-Contains $listing '(?m)^Cloud save:\s+None\s*$' 'Store listing must declare no cloud save.'
    Assert-Contains $listing '(?m)^In-app purchases:\s+None\s*$' 'Store listing must declare no in-app purchases.'
    Assert-Contains $listing '(?m)^Remote gameplay analytics:\s+None\s*$' 'Store listing must declare no remote gameplay analytics.'
    Assert-Contains $listing '(?m)^Remote crash reporting:\s+None\s*$' 'Store listing must declare no remote crash reporting.'
    Assert-Contains $listing '(?m)^Rewarded ads:\s+Optional only; base progression remains available without an ad\.\s*$' 'Store listing must disclose optional rewarded ads and ad-independent base progression.'
    Assert-Contains $listing '(?m)^Offline play:\s+Available for all base gameplay\.\s*$' 'Store listing must disclose offline base gameplay.'
}

Assert-Contains $listingEn '(?ms)^## Title\s+Curio Clerk: Night Shift\s*$' 'English product title is missing.'
Assert-Contains $listingKo '(?ms)^## 제목\s+기묘한 분실물 야간반\s*$' 'Korean product title is missing.'
Assert-Contains $listingEn '(?is)warm occult.*rule-sorting.*12-curio.*Repair.*Storage.*Vault.*Hold.*casebook.*desk charms.*offline.*optional rewarded ads' 'English full description omits a required product fact.'
Assert-Contains $listingKo '(?s)따뜻한 오컬트.*규칙.*12개.*수리실.*보관실.*금고.*보류.*도감.*책상 장식.*오프라인.*선택형 보상형 광고' 'Korean full description omits a required product fact.'

foreach ($forbiddenClaim in @(
    '(?i)award[- ]winning',
    '(?i)\b#?1\b.*(?:game|puzzle|rank)',
    '(?i)multiplayer',
    '(?i)cloud sync',
    '(?i)guaranteed ad',
    '(?i)analytics-driven live ops',
    '(?i)approved by (?:Samsung|Galaxy Store)'
)) {
    if (($listingEn + "`n" + $listingKo) -match $forbiddenClaim) {
        throw "Unsupported store claim found: $forbiddenClaim"
    }
}
foreach ($contradictoryClaim in @(
    '(?i)(?:create|register|sign in|log in)(?: for| with| to)? (?:an? )?account',
    '(?i)(?:buy|purchase) (?:coins|items|charms)',
    '(?i)in-app purchases? (?:are|is) available',
    '(?i)(?:sync|save) (?:your )?(?:progress )?(?:to|with) (?:the )?cloud',
    '(?i)(?:we|the developer|the game) (?:collect|upload|send)s? (?:gameplay |crash )?(?:analytics|events|reports)'
)) {
    if (($listingEn + "`n" + $listingKo) -match $contradictoryClaim) {
        throw "Store copy contradicts the v1 service boundary: $contradictoryClaim"
    }
}

$reviewNotes = $documents['Docs/Store/ReviewNotes.md']
foreach ($step in @(
    'Launch without an account',
    'Complete the tutorial',
    'Finish or fail a shift',
    'base progression works without an ad',
    'revive or double coins once',
    'other option is locked',
    'no ad is available',
    'no-fill',
    'change language',
    'privacy options when required',
    'Force-stop and relaunch'
)) {
    Assert-Contains $reviewNotes ([regex]::Escape($step)) "Reviewer flow is missing: $step"
}
Assert-Contains $reviewNotes '(?i)release configuration' 'Reviewer notes must identify the certification release configuration.'
if ($reviewNotes -match '(?i)Google(?:''s)? sample (?:ad|unit|identifier)') {
    throw 'Reviewer notes must not instruct certification with a Google sample ad identifier.'
}

$dataSafety = $documents['Docs/Store/DataSafety.md']
foreach ($dataStatement in @(
    'UMP consent information update on every Android launch',
    'Consent-authorized rewarded ad preload',
    'IP address / network-derived approximate location',
    'User product interactions / advertising interaction data',
    'Diagnostics and performance data',
    'Device and account identifiers, including advertising ID and app set ID',
    'Consent and privacy choices',
    'Advertising delivery and personalization when allowed',
    'Measurement and analytics for the advertising SDK',
    'Fraud prevention, security, compliance, and SDK operation'
)) {
    Assert-Contains $dataSafety ([regex]::Escape($dataStatement)) "Data Safety worksheet omits: $dataStatement"
}
foreach ($absence in @(
    'Developer gameplay account \| Absent',
    'Gameplay backend / cloud save \| Absent',
    'Firebase \| Absent',
    'Remote gameplay analytics \| Absent',
    'Remote crash reporting \| Absent'
)) {
    Assert-Contains $dataSafety "(?m)^\| $absence \|$" "Data Safety worksheet must record this v1 absence: $absence"
}
Assert-Contains $dataSafety 'Google Play''s disclosure guidance is a reconciliation source, not a claim that Samsung uses the same form verbatim\.' 'Data Safety worksheet must distinguish Google Play guidance from Samsung forms.'

$privacyPolicy = $documents['Docs/PrivacyPolicy.md']
Assert-Contains $privacyPolicy 'On Android app launch, UMP may contact Google to update advertising consent status' 'Privacy policy omits the launch-time UMP update.'
Assert-Contains $privacyPolicy 'AdMob may initialize and preload an optional rewarded advertisement' 'Privacy policy omits the consent-authorized rewarded-ad preload.'
foreach ($sharedDisclosure in @(
    'device identifiers',
    'advertising data',
    'diagnostics',
    'consent choices',
    'approximate location derived from network information'
)) {
    Assert-Contains $privacyPolicy ([regex]::Escape($sharedDisclosure)) "Privacy policy omits the AdMob/UMP disclosure: $sharedDisclosure"
    Assert-Contains $dataSafety ([regex]::Escape($sharedDisclosure)) "Data Safety worksheet disagrees with PrivacyPolicy.md: $sharedDisclosure"
}

$ratingAnswers = $documents['Docs/Store/RatingAnswers.md']
Assert-Contains $ratingAnswers '(?m)^Questionnaire status:\s+PENDING_DEVELOPER_CONFIRMATION\s*$' 'Rating worksheet must remain pending developer confirmation.'
Assert-Contains $ratingAnswers '(?m)^Official rating assigned:\s+No\s*$' 'Rating worksheet must not assign an official rating.'
foreach ($ratingFact in @('Warm occult / supernatural theme', 'Gambling', 'In-app purchases', 'Chat or user-generated content', 'Realistic violence', 'Rewarded advertising')) {
    Assert-Contains $ratingAnswers ([regex]::Escape($ratingFact)) "Rating worksheet omits the content fact: $ratingFact"
}

$assetInventory = $documents['Docs/Store/AssetInventory.md']
Assert-Contains $assetInventory '(?m)^Media approval status:\s+BLOCKED\s*$' 'Store media must remain blocked until human approval.'
foreach ($assetFact in @(
    'Application icon',
    'Phone screenshots',
    'Store video',
    'Docs/AIAssetProvenance.md',
    'Docs/ArtReleaseReview.md',
    'Prototype only',
    'Missing / not uploaded'
)) {
    Assert-Contains $assetInventory ([regex]::Escape($assetFact)) "Asset inventory omits: $assetFact"
}

$evidenceIndex = $documents['Docs/ReleaseEvidence/1.0.0/README.md']
Assert-Contains $evidenceIndex '(?m)^Evidence status:\s+PENDING_DEVELOPER_EVIDENCE\s*$' 'Release evidence must remain pending developer evidence.'
foreach ($pendingEvidence in @('Automated tests', 'AAB inspection', 'Owned-device validation', 'Remote Test Lab', 'Service validation', 'RC decision')) {
    Assert-Contains $evidenceIndex "(?m)^\| $([regex]::Escape($pendingEvidence)) \| (?:Not run|Pending developer evidence) \|" "Evidence index invents or omits status for: $pendingEvidence"
}
Assert-Contains $evidenceIndex '(?i)Do not commit identity documents' 'Evidence index must prohibit identity documents.'
Assert-Contains $evidenceIndex '(?i)machine-absolute paths' 'Evidence index must prohibit machine-absolute paths.'
if ($evidenceIndex -match '(?i)(?:[A-Z]:\\Users\\|/Users/|/home/|-----BEGIN [A-Z ]*PRIVATE KEY-----|ca-app-pub-\d+[/~]\d+|AKIA[0-9A-Z]{16})') {
    throw 'Release evidence index contains a machine path, credential, signing material, or real ad identifier.'
}

foreach ($rightsDocument in @('Docs/AIAssetProvenance.md', 'Docs/ThirdPartyNotices.md')) {
    Assert-Contains $assetInventory ([regex]::Escape($rightsDocument)) "Asset inventory must link $rightsDocument."
}

foreach ($sourceUrl in @(
    'https://developers.google.com/admob/unity/privacy/play-data-disclosure',
    'https://developers.google.com/admob/unity/privacy',
    'https://developer.samsung.com/galaxy-store/launch.html',
    'https://developer.samsung.com/galaxy-store/self-check-list-galaxy.html?lang=en'
)) {
    Assert-Contains ($dataSafety + "`n" + $ratingAnswers + "`n" + $assetInventory + "`n" + $reviewNotes) ([regex]::Escape($sourceUrl)) "Official source is not recorded: $sourceUrl"
}
Assert-Contains ($dataSafety + "`n" + $ratingAnswers + "`n" + $assetInventory + "`n" + $reviewNotes) 'Accessed 2026-08-21' 'Official source access date is missing.'

if ($Mode -eq 'Submission') {
    $submissionDocs = @(
        $documents['Docs/PrivacyPolicy.md'],
        $documents['Docs/Store/SamsungSellerSetup.md'],
        $listingEn,
        $listingKo,
        $reviewNotes,
        $dataSafety,
        $ratingAnswers,
        $assetInventory,
        $evidenceIndex
    ) -join "`n"

    $submissionBlockers = [System.Collections.Generic.List[string]]::new()
    $unresolvedTokens = @([regex]::Matches($submissionDocs, '\[[A-Z][A-Z0-9_]+\]') | ForEach-Object { $_.Value } | Sort-Object -Unique)
    if ($unresolvedTokens.Count -gt 0) {
        $submissionBlockers.Add("Unresolved submission tokens: $($unresolvedTokens -join ', ')")
    }
    if ($privacyPolicy -match 'must be hosted at a public URL') {
        $submissionBlockers.Add('Privacy policy hosting and public URL are unresolved.')
    }
    if ($documents['Docs/Store/SamsungSellerSetup.md'] -match 'Seller verification status:\s+record the dated status') {
        $submissionBlockers.Add('Samsung commercial-seller verification status and date await the developer.')
    }
    if ($dataSafety -match '(?m)^Reconciliation status:\s+PENDING_FINAL_CONFIGURATION_CONFIRMATION\s*$') {
        $submissionBlockers.Add('Final AdMob/UMP and Seller Portal Data Safety reconciliation awaits the developer.')
    }
    if ($ratingAnswers -match '(?m)^Questionnaire status:\s+PENDING_DEVELOPER_CONFIRMATION\s*$') {
        $submissionBlockers.Add('Seller Portal rating answers await developer confirmation.')
    }
    if ($assetInventory -match '(?m)^Media approval status:\s+BLOCKED\s*$') {
        $submissionBlockers.Add('Required store media are missing or not human-approved.')
    }
    if ($evidenceIndex -match '(?m)^Evidence status:\s+PENDING_DEVELOPER_EVIDENCE\s*$') {
        $submissionBlockers.Add('Release evidence and RC decision await the developer.')
    }
    if ($submissionBlockers.Count -gt 0) {
        throw "Submission documentation is blocked:`n- $($submissionBlockers -join "`n- ")"
    }
}

Write-Host "Release documentation gate passed ($Mode mode)."
