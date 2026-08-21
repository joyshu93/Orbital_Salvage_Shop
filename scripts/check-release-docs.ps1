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

function Get-StructuredField([string]$Text, [string]$Name, [string]$DocumentName) {
    $pattern = "(?m)^$([regex]::Escape($Name)):\s*(.+?)\s*$"
    $matches = [regex]::Matches($Text, $pattern)
    if ($matches.Count -ne 1) {
        throw "$DocumentName must contain exactly one '$Name' structured field."
    }
    return $matches[0].Groups[1].Value.Trim().Trim('`')
}

function Get-Section([string]$Text, [string]$Heading, [string]$DocumentName) {
    $match = [regex]::Match($Text, "(?ms)^##\s+$([regex]::Escape($Heading))\s*\r?\n(?<body>.*?)(?=^##\s+|\z)")
    if (-not $match.Success) {
        throw "$DocumentName is missing section: $Heading"
    }
    return $match.Groups['body'].Value
}

function Get-LineIds([string]$Text, [string]$Pattern, [int]$Group = 1) {
    return @([regex]::Matches($Text, $Pattern) | ForEach-Object { $_.Groups[$Group].Value } | Sort-Object -Unique)
}

function Test-IsoDate([string]$Value) {
    $parsed = [DateTime]::MinValue
    return [DateTime]::TryParseExact(
        $Value,
        'yyyy-MM-dd',
        [Globalization.CultureInfo]::InvariantCulture,
        [Globalization.DateTimeStyles]::None,
        [ref]$parsed)
}

function Test-Email([string]$Value) {
    return $Value -match '\A[^@\s]+@[^@\s]+\.[^@\s]+\z'
}

function Test-HttpsUrl([string]$Value) {
    $uri = $null
    return [Uri]::TryCreate($Value, [UriKind]::Absolute, [ref]$uri) -and
        $uri.Scheme -eq 'https' -and
        -not [string]::IsNullOrWhiteSpace($uri.Host)
}

function Assert-Allowed([string]$Value, [string[]]$Allowed, [string]$Name) {
    if ($Value -notin $Allowed) {
        throw "$Name has invalid value '$Value'. Allowed: $($Allowed -join ', ')."
    }
}

function Assert-PendingOrDate([string]$Value, [string]$Name) {
    if ($Value -ne 'PENDING' -and -not (Test-IsoDate $Value)) {
        throw "$Name must be PENDING or an ISO yyyy-MM-dd date."
    }
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
$enFactIds = Get-LineIds $listingEn '(?m)^-\s+(FACT_[A-Z0-9_]+)\s*$'
$koFactIds = Get-LineIds $listingKo '(?m)^-\s+(FACT_[A-Z0-9_]+)\s*$'
if (($enFactIds -join '|') -ne ($expectedFactIds -join '|') -or
    ($koFactIds -join '|') -ne ($expectedFactIds -join '|')) {
    throw 'English/Korean store listing fact parity is incomplete or inconsistent.'
}

Assert-Contains $listingEn '(?m)^# Galaxy Store listing — English$' 'English listing heading is missing.'
Assert-Contains $listingKo '(?m)^# Galaxy Store 등록 문구 — 한국어$' 'Korean listing heading is missing.'
foreach ($listing in @($listingEn, $listingKo)) {
    Assert-Contains $listing '(?m)^Package ID:\s+`com\.joyshu93\.curioclerknightshift`\s*$' 'Store listing package ID is missing or wrong.'
    Assert-Contains $listing '(?m)^Release version:\s+`1\.0\.0`\s*$' 'Store listing version is missing or wrong.'
    foreach ($declaration in @(
        'Account:\s+None',
        'Gameplay backend:\s+None',
        'Cloud save:\s+None',
        'In-app purchases:\s+None',
        'Remote gameplay analytics:\s+None',
        'Remote crash reporting:\s+None',
        'Rewarded ads:\s+Optional only; base progression remains available without an ad\.',
        'Offline play:\s+Available for all base gameplay\.'
    )) {
        Assert-Contains $listing "(?m)^$declaration\s*$" "Store listing declaration is missing or inconsistent: $declaration"
    }
}

Assert-Contains $listingEn '(?ms)^## Title\s+Curio Clerk: Night Shift\s*$' 'English product title is missing.'
Assert-Contains $listingKo '(?ms)^## 제목\s+기묘한 분실물 야간반\s*$' 'Korean product title is missing.'
$fullDescriptionEn = Get-Section $listingEn 'Full description' 'English listing'
$fullDescriptionKo = Get-Section $listingKo '자세한 설명' 'Korean listing'
Assert-Contains $fullDescriptionEn '(?is)warm occult.*rule-sorting.*12-curio.*Repair.*Storage.*Vault.*Hold.*casebook.*desk charms.*offline.*optional rewarded ads' 'English full description omits a required product fact.'
Assert-Contains $fullDescriptionKo '(?s)따뜻한 오컬트.*규칙.*12개.*수리실.*보관실.*금고.*보류.*도감.*책상 장식.*오프라인.*선택형 보상형 광고' 'Korean full description omits a required product fact.'

$affirmativeStoreCopy = $fullDescriptionEn + "`n" + $fullDescriptionKo
foreach ($unsupportedClaim in @(
    '(?im)^\s*(?:An?\s+)?award[- ]winning\b',
    '(?im)^\s*(?:#?1|number one)\b.*(?:game|puzzle|rank)',
    '(?im)^\s*(?:Ads?|Rewarded ads?)\s+(?:are|is)\s+guaranteed\b',
    '(?im)^\s*Guaranteed ad availability\b',
    '(?im)^\s*Approved by (?:Samsung|Galaxy Store)\b'
)) {
    if ($affirmativeStoreCopy -match $unsupportedClaim) {
        throw "Unsupported affirmative store claim found: $unsupportedClaim"
    }
}
foreach ($contradictoryClaim in @(
    '(?im)^\s*(?:Create|Register|Sign in|Log in)(?: for| with| to)? (?:an? )?account\b',
    '(?im)^\s*(?:Buy|Purchase) (?:coins|items|charms)\b',
    '(?im)^\s*(?:In-app purchases?) (?:are|is) available\b',
    '(?im)^\s*(?:Sync|Save) (?:your )?(?:progress )?(?:to|with) (?:the )?cloud\b',
    '(?im)^\s*(?:We|The developer|The game) (?:collect|upload|send)s? (?:gameplay |crash )?(?:analytics|events|reports)\b'
)) {
    if ($affirmativeStoreCopy -match $contradictoryClaim) {
        throw "Store copy contradicts the v1 service boundary: $contradictoryClaim"
    }
}

$reviewNotes = $documents['Docs/Store/ReviewNotes.md']
foreach ($step in @('Launch without an account', 'Complete the tutorial', 'Finish or fail a shift', 'base progression works without an ad', 'revive or double coins once', 'other option is locked', 'no ad is available', 'no-fill', 'change language', 'privacy options when required', 'Force-stop and relaunch')) {
    Assert-Contains $reviewNotes ([regex]::Escape($step)) "Reviewer flow is missing: $step"
}
Assert-Contains $reviewNotes '(?i)release configuration' 'Reviewer notes must identify the certification release configuration.'
if ($reviewNotes -match '(?i)Google(?:''s)? sample (?:ad|unit|identifier)') {
    throw 'Reviewer notes must not instruct certification with a Google sample ad identifier.'
}

$privacyPolicy = $documents['Docs/PrivacyPolicy.md']
$dataSafety = $documents['Docs/Store/DataSafety.md']
$expectedCategoryIds = @(
    'IP_NETWORK_APPROXIMATE_LOCATION',
    'PRODUCT_INTERACTIONS',
    'DIAGNOSTICS_PERFORMANCE',
    'DEVICE_ACCOUNT_IDENTIFIERS',
    'CONSENT_PRIVACY_CHOICES'
) | Sort-Object
$privacyCategoryIds = Get-LineIds $privacyPolicy '(?m)^<!-- AD_DATA_CATEGORY:\s*([A-Z0-9_]+)\s*-->$'
$dataCategoryIds = Get-LineIds $dataSafety '(?m)^<!-- AD_DATA_CATEGORY:\s*([A-Z0-9_]+)\s*-->$'
if (($privacyCategoryIds -join '|') -ne ($expectedCategoryIds -join '|') -or
    ($dataCategoryIds -join '|') -ne ($expectedCategoryIds -join '|')) {
    throw 'PrivacyPolicy and DataSafety AdMob/UMP category parity is incomplete or inconsistent.'
}
foreach ($privacyDisclosure in @(
    'IP address, which may be used for network-derived approximate location',
    'product interactions, including app launch, taps, and video views',
    'diagnostics and performance data',
    'device and account identifiers, including advertising ID and app set ID',
    'consent and privacy choices'
)) {
    Assert-Contains $privacyPolicy ([regex]::Escape($privacyDisclosure)) "Privacy policy omits: $privacyDisclosure"
}
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
foreach ($absence in @('Developer gameplay account \| Absent', 'Gameplay backend / cloud save \| Absent', 'Firebase \| Absent', 'Remote gameplay analytics \| Absent', 'Remote crash reporting \| Absent')) {
    Assert-Contains $dataSafety "(?m)^\| $absence \|$" "Data Safety worksheet must record this v1 absence: $absence"
}
Assert-Contains $dataSafety 'Google Play''s disclosure guidance is a reconciliation source, not a claim that Samsung uses the same form verbatim\.' 'Data Safety worksheet must distinguish Google Play guidance from Samsung forms.'
Assert-Contains $privacyPolicy 'On Android app launch, UMP may contact Google to update advertising consent status' 'Privacy policy omits the launch-time UMP update.'
Assert-Contains $privacyPolicy 'AdMob may initialize and preload an optional rewarded advertisement' 'Privacy policy omits the consent-authorized rewarded-ad preload.'
Assert-Contains $privacyPolicy '(?i)does not include Firebase' 'Privacy policy must exclude Firebase.'
Assert-Contains $privacyPolicy '(?i)does not send gameplay events or crash reports to the developer' 'Privacy policy must exclude remote gameplay/crash reporting.'

$sellerSetup = $documents['Docs/Store/SamsungSellerSetup.md']
$accountStatus = Get-StructuredField $sellerSetup 'Account registration status' 'SamsungSellerSetup.md'
$identityEvidenceStatus = Get-StructuredField $sellerSetup 'Identity / financial evidence status' 'SamsungSellerSetup.md'
$sellerVerificationStatus = Get-StructuredField $sellerSetup 'Seller verification status' 'SamsungSellerSetup.md'
$sellerVerificationDate = Get-StructuredField $sellerSetup 'Seller verification date' 'SamsungSellerSetup.md'
$publicDeveloperName = Get-StructuredField $sellerSetup 'Public developer name' 'SamsungSellerSetup.md'
$publicSupportEmail = Get-StructuredField $sellerSetup 'Public support email' 'SamsungSellerSetup.md'
$publicPrivacyUrl = Get-StructuredField $sellerSetup 'Public privacy policy URL' 'SamsungSellerSetup.md'
Assert-Allowed $accountStatus @('PENDING_DEVELOPER_ACTION', 'REGISTERED') 'Account registration status'
Assert-Allowed $identityEvidenceStatus @('NOT_SUBMITTED_OR_UNCONFIRMED', 'SUBMITTED') 'Identity / financial evidence status'
Assert-Allowed $sellerVerificationStatus @('PENDING_DEVELOPER_CONFIRMATION', 'VERIFIED') 'Seller verification status'
Assert-PendingOrDate $sellerVerificationDate 'Seller verification date'
if ($sellerVerificationStatus -eq 'VERIFIED' -and -not (Test-IsoDate $sellerVerificationDate)) {
    throw 'Verified seller status requires an ISO verification date.'
}
if ($sellerVerificationStatus -ne 'VERIFIED' -and $sellerVerificationDate -ne 'PENDING') {
    throw 'Pending seller verification must keep Seller verification date PENDING.'
}

$effectiveDate = Get-StructuredField $privacyPolicy 'Effective date' 'PrivacyPolicy.md'
$privacyDeveloperName = Get-StructuredField $privacyPolicy 'Developer' 'PrivacyPolicy.md'
$privacySupportEmail = Get-StructuredField $privacyPolicy 'Support' 'PrivacyPolicy.md'
$privacyPublicUrl = Get-StructuredField $privacyPolicy 'Public URL' 'PrivacyPolicy.md'
if ($effectiveDate -ne '[EFFECTIVE_DATE]' -and -not (Test-IsoDate $effectiveDate)) { throw 'Privacy effective date must be [EFFECTIVE_DATE] or ISO yyyy-MM-dd.' }
if ($publicDeveloperName -ne '[DEVELOPER_DISPLAY_NAME]' -and [string]::IsNullOrWhiteSpace($publicDeveloperName)) { throw 'Public developer name is empty.' }
if ($publicSupportEmail -ne '[SUPPORT_EMAIL]' -and -not (Test-Email $publicSupportEmail)) { throw 'Public support email is invalid.' }
if ($publicPrivacyUrl -ne '[PRIVACY_POLICY_URL]' -and -not (Test-HttpsUrl $publicPrivacyUrl)) { throw 'Public privacy policy URL must be HTTPS.' }
if ($privacyDeveloperName -ne $publicDeveloperName -or $privacySupportEmail -ne $publicSupportEmail -or $privacyPublicUrl -ne $publicPrivacyUrl) {
    throw 'Public developer name, support email, or privacy URL disagrees between SamsungSellerSetup and PrivacyPolicy.'
}

$dataStatus = Get-StructuredField $dataSafety 'Reconciliation status' 'DataSafety.md'
$dataConfirmationDate = Get-StructuredField $dataSafety 'Confirmation date' 'DataSafety.md'
$signedRcSha = Get-StructuredField $dataSafety 'Signed RC Git SHA' 'DataSafety.md'
$gmaVersion = Get-StructuredField $dataSafety 'Google Mobile Ads Unity version' 'DataSafety.md'
$edmVersion = Get-StructuredField $dataSafety 'External Dependency Manager for Unity version' 'DataSafety.md'
Assert-Allowed $dataStatus @('PENDING_FINAL_CONFIGURATION_CONFIRMATION', 'DEVELOPER_CONFIRMED') 'Data Safety reconciliation status'
Assert-PendingOrDate $dataConfirmationDate 'Data Safety confirmation date'
if ($signedRcSha -ne 'PENDING' -and $signedRcSha -notmatch '\A[0-9a-fA-F]{40}\z') { throw 'Signed RC Git SHA must be PENDING or 40 hexadecimal characters.' }
if ($gmaVersion -ne '11.3.0') { throw 'Google Mobile Ads Unity version must be 11.3.0.' }
if ($edmVersion -ne '1.2.188') { throw 'EDM4U version must be 1.2.188.' }
if ($dataStatus -eq 'DEVELOPER_CONFIRMED') {
    if (-not (Test-IsoDate $dataConfirmationDate) -or $signedRcSha -notmatch '\A[0-9a-fA-F]{40}\z') { throw 'Confirmed Data Safety requires an ISO date and signed RC Git SHA.' }
} elseif ($dataConfirmationDate -ne 'PENDING' -or $signedRcSha -ne 'PENDING') {
    throw 'Pending Data Safety must keep its date and signed RC SHA PENDING.'
}

$ratingAnswers = $documents['Docs/Store/RatingAnswers.md']
$ratingStatus = Get-StructuredField $ratingAnswers 'Questionnaire status' 'RatingAnswers.md'
$ratingDate = Get-StructuredField $ratingAnswers 'Confirmation date' 'RatingAnswers.md'
$ratingResult = Get-StructuredField $ratingAnswers 'Official rating result' 'RatingAnswers.md'
Assert-Allowed $ratingStatus @('PENDING_DEVELOPER_CONFIRMATION', 'DEVELOPER_CONFIRMED') 'Rating questionnaire status'
Assert-PendingOrDate $ratingDate 'Rating confirmation date'
if ($ratingResult -ne 'PENDING' -and ($ratingResult -match '\[[A-Z0-9_]+\]' -or [string]::IsNullOrWhiteSpace($ratingResult))) { throw 'Official rating result is invalid.' }
if ($ratingStatus -eq 'DEVELOPER_CONFIRMED') {
    if (-not (Test-IsoDate $ratingDate) -or $ratingResult -eq 'PENDING') { throw 'Confirmed rating requires an ISO date and official result.' }
} elseif ($ratingDate -ne 'PENDING' -or $ratingResult -ne 'PENDING') {
    throw 'Pending rating must keep date and official result PENDING.'
}
foreach ($ratingFact in @('Warm occult / supernatural theme', 'Gambling', 'In-app purchases', 'Chat or user-generated content', 'Realistic violence', 'Rewarded advertising')) {
    Assert-Contains $ratingAnswers ([regex]::Escape($ratingFact)) "Rating worksheet omits: $ratingFact"
}

$assetInventory = $documents['Docs/Store/AssetInventory.md']
$mediaStatus = Get-StructuredField $assetInventory 'Media approval status' 'AssetInventory.md'
$mediaDate = Get-StructuredField $assetInventory 'Approval date' 'AssetInventory.md'
Assert-Allowed $mediaStatus @('BLOCKED', 'HUMAN_APPROVED') 'Media approval status'
Assert-PendingOrDate $mediaDate 'Media approval date'
if ($mediaStatus -eq 'HUMAN_APPROVED' -and -not (Test-IsoDate $mediaDate)) { throw 'Human-approved media require an ISO approval date.' }
if ($mediaStatus -eq 'BLOCKED' -and $mediaDate -ne 'PENDING') { throw 'Blocked media must keep Approval date PENDING.' }
foreach ($assetFact in @('Application icon', 'Phone screenshots', 'Store video', 'Docs/AIAssetProvenance.md', 'Docs/ThirdPartyNotices.md', 'Docs/ArtReleaseReview.md')) {
    Assert-Contains $assetInventory ([regex]::Escape($assetFact)) "Asset inventory omits: $assetFact"
}

$evidenceIndex = $documents['Docs/ReleaseEvidence/1.0.0/README.md']
$evidenceStatus = Get-StructuredField $evidenceIndex 'Evidence status' 'Release evidence README'
$rcDecision = Get-StructuredField $evidenceIndex 'RC decision' 'Release evidence README'
$rcDecisionDate = Get-StructuredField $evidenceIndex 'Decision date' 'Release evidence README'
Assert-Allowed $evidenceStatus @('PENDING_DEVELOPER_EVIDENCE', 'DEVELOPER_CONFIRMED') 'Evidence status'
Assert-Allowed $rcDecision @('PENDING', 'ACCEPTED') 'RC decision'
Assert-PendingOrDate $rcDecisionDate 'RC decision date'
if ($evidenceStatus -eq 'DEVELOPER_CONFIRMED') {
    if ($rcDecision -ne 'ACCEPTED' -or -not (Test-IsoDate $rcDecisionDate)) { throw 'Confirmed evidence requires ACCEPTED RC decision and ISO date.' }
} elseif ($rcDecision -ne 'PENDING' -or $rcDecisionDate -ne 'PENDING') {
    throw 'Pending evidence must keep RC decision and date PENDING.'
}
foreach ($pendingEvidence in @('Automated tests', 'AAB inspection', 'Owned-device validation', 'Remote Test Lab', 'Service validation', 'RC decision')) {
    Assert-Contains $evidenceIndex "(?m)^\| $([regex]::Escape($pendingEvidence)) \| (?:Not run|Pending developer evidence|Developer evidence recorded) \|" "Evidence index omits or invalidates: $pendingEvidence"
}
Assert-Contains $evidenceIndex '(?i)Do not commit identity documents' 'Evidence index must prohibit identity documents.'
Assert-Contains $evidenceIndex '(?i)machine-absolute paths' 'Evidence index must prohibit machine-absolute paths.'
if ($evidenceIndex -match '(?i)(?:[A-Z]:\\Users\\|/Users/|/home/|-----BEGIN [A-Z ]*PRIVATE KEY-----|ca-app-pub-\d+[/~]\d+|AKIA[0-9A-Z]{16})') {
    throw 'Release evidence index contains a machine path, credential, signing material, or real ad identifier.'
}

foreach ($sourceUrl in @('https://developers.google.com/admob/unity/privacy/play-data-disclosure', 'https://developers.google.com/admob/unity/privacy', 'https://developer.samsung.com/galaxy-store/launch.html', 'https://developer.samsung.com/galaxy-store/self-check-list-galaxy.html?lang=en')) {
    Assert-Contains ($dataSafety + "`n" + $ratingAnswers + "`n" + $assetInventory + "`n" + $reviewNotes) ([regex]::Escape($sourceUrl)) "Official source is not recorded: $sourceUrl"
}
Assert-Contains ($dataSafety + "`n" + $ratingAnswers + "`n" + $assetInventory + "`n" + $reviewNotes) 'Accessed 2026-08-21' 'Official source access date is missing.'

if ($Mode -eq 'Submission') {
    $submissionBlockers = [System.Collections.Generic.List[string]]::new()
    if ($accountStatus -ne 'REGISTERED') { $submissionBlockers.Add('Samsung account registration is not confirmed REGISTERED.') }
    if ($identityEvidenceStatus -ne 'SUBMITTED') { $submissionBlockers.Add('Identity/financial evidence is not confirmed SUBMITTED.') }
    if ($sellerVerificationStatus -ne 'VERIFIED' -or -not (Test-IsoDate $sellerVerificationDate)) { $submissionBlockers.Add('Samsung seller verification and date are not confirmed.') }
    if ($publicDeveloperName -eq '[DEVELOPER_DISPLAY_NAME]' -or [string]::IsNullOrWhiteSpace($publicDeveloperName)) { $submissionBlockers.Add('Public developer name is unresolved.') }
    if (-not (Test-Email $publicSupportEmail)) { $submissionBlockers.Add('Public support email is unresolved or invalid.') }
    if (-not (Test-HttpsUrl $publicPrivacyUrl)) { $submissionBlockers.Add('Public privacy policy URL is unresolved or not HTTPS.') }
    if (-not (Test-IsoDate $effectiveDate)) { $submissionBlockers.Add('Privacy effective date is unresolved or invalid.') }
    if ($dataStatus -ne 'DEVELOPER_CONFIRMED' -or -not (Test-IsoDate $dataConfirmationDate) -or $signedRcSha -notmatch '\A[0-9a-fA-F]{40}\z') { $submissionBlockers.Add('Final Data Safety reconciliation, date, or signed RC SHA is unresolved.') }
    if ($ratingStatus -ne 'DEVELOPER_CONFIRMED' -or -not (Test-IsoDate $ratingDate) -or $ratingResult -eq 'PENDING') { $submissionBlockers.Add('Seller Portal rating result is not developer-confirmed.') }
    if ($mediaStatus -ne 'HUMAN_APPROVED' -or -not (Test-IsoDate $mediaDate)) { $submissionBlockers.Add('Required store media are not human-approved with a date.') }
    if ($evidenceStatus -ne 'DEVELOPER_CONFIRMED' -or $rcDecision -ne 'ACCEPTED' -or -not (Test-IsoDate $rcDecisionDate)) { $submissionBlockers.Add('Release evidence or accepted RC decision is unresolved.') }

    if (-not (Test-Path -LiteralPath (Join-Path $root 'Docs/ArtReleaseReview.md') -PathType Leaf)) {
        $submissionBlockers.Add('Docs/ArtReleaseReview.md is missing.')
    }
    foreach ($assetName in @('Application icon', 'Phone screenshots')) {
        $row = [regex]::Match($assetInventory, "(?m)^\| $([regex]::Escape($assetName)) \|(?<body>.*)\|$")
        if (-not $row.Success -or $row.Groups['body'].Value -match '(?i)missing|not uploaded|prototype only|blocked|no files created') {
            $submissionBlockers.Add("Required media is not submission-ready: $assetName.")
        }
    }
    foreach ($relative in @(
        'Docs/ReleaseEvidence/1.0.0/automated-tests.md',
        'Docs/ReleaseEvidence/1.0.0/owned-device.md',
        'Docs/ReleaseEvidence/1.0.0/remote-test-lab.md',
        'Docs/ReleaseEvidence/1.0.0/service-validation.md',
        'Docs/ReleaseEvidence/1.0.0/rc-decision.md'
    )) {
        if (-not (Test-Path -LiteralPath (Join-Path $root $relative) -PathType Leaf)) {
            $submissionBlockers.Add("Task 11 evidence is missing: $relative")
        }
    }
    if ($evidenceIndex -match '(?m)^\| (?:Automated tests|AAB inspection|Owned-device validation|Remote Test Lab|Service validation|RC decision) \| (?:Not run|Pending developer evidence) \|') {
        $submissionBlockers.Add('Evidence index still contains pending/not-run release evidence.')
    }

    $submissionDocs = @($privacyPolicy, $sellerSetup, $listingEn, $listingKo, $reviewNotes, $dataSafety, $ratingAnswers, $assetInventory, $evidenceIndex) -join "`n"
    $unresolvedTokens = @([regex]::Matches($submissionDocs, '\[[A-Z][A-Z0-9_]+\]') | ForEach-Object { $_.Value } | Sort-Object -Unique)
    if ($unresolvedTokens.Count -gt 0) {
        $submissionBlockers.Add("Unresolved submission tokens: $($unresolvedTokens -join ', ')")
    }
    if ($submissionBlockers.Count -gt 0) {
        throw "Submission documentation is blocked:`n- $($submissionBlockers -join "`n- ")"
    }
}

Write-Host "Release documentation gate passed ($Mode mode)."
