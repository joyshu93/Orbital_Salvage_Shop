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
    return [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
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

function Get-RequiredIntegerField([string]$Text, [string]$Name, [string]$DocumentName) {
    $value = Get-StructuredField $Text $Name $DocumentName
    $number = 0
    if (-not [int]::TryParse($value, [ref]$number) -or $number -lt 0) {
        throw "$DocumentName field '$Name' must be a non-negative integer."
    }
    return $number
}

function Assert-SanitizedEvidence([string]$Text, [string]$DocumentName) {
    $forbiddenPatterns = @(
        '(?im)(?:^|[\s`"''(])(?:[A-Za-z]:[\\/]|file:/+|/(?:Users|home|var|tmp|etc)/|/(?!/)[A-Za-z0-9._-]+/)',
        '(?i)-----BEGIN [A-Z ]*PRIVATE KEY-----',
        '(?i)\b(?:gh[pousr]_[A-Za-z0-9]{20,}|AKIA[0-9A-Z]{16}|Bearer\s+[A-Za-z0-9._~+/-]{12,})\b',
        '(?im)^\s*(?:access token|refresh token|password|credential|secret|api key)\s*:\s*(?!REDACTED\b)\S+',
        '(?i)ca-app-pub-\d+[/~]\d+',
        '(?im)^\s*(?:government ID|passport|bank account|tax ID|identity document|financial record)\s*:',
        '(?m)(?:\[[A-Z][A-Z0-9_]+\]|\b(?:PENDING|TODO|TBD|PLACEHOLDER|NOT RUN)\b)'
    )
    foreach ($pattern in $forbiddenPatterns) {
        if ($Text -match $pattern) {
            throw "$DocumentName contains forbidden path, secret, identity/financial record, ad identifier, or pending placeholder evidence."
        }
    }
}

function Read-SubmissionEvidence([string]$RelativePath) {
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Submission evidence is missing: $RelativePath"
    }
    $text = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
    Assert-SanitizedEvidence $text $RelativePath
    return $text
}

function Assert-CommonEvidence([string]$Text, [string]$DocumentName, [string]$ExpectedRcSha) {
    $status = Get-StructuredField $Text 'Evidence status' $DocumentName
    $date = Get-StructuredField $Text 'Evidence date' $DocumentName
    $rcSha = Get-StructuredField $Text 'RC Git SHA' $DocumentName
    if ($status -ne 'DEVELOPER_RECORDED') { throw "$DocumentName Evidence status must be DEVELOPER_RECORDED." }
    if (-not (Test-IsoDate $date)) { throw "$DocumentName Evidence date must be ISO yyyy-MM-dd." }
    if ($rcSha -notmatch '\A[0-9a-fA-F]{40}\z' -or $rcSha -ne $ExpectedRcSha) { throw "$DocumentName RC Git SHA must match the signed RC SHA." }
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
    '(?im)^\s*(?:An?\s+)?ad\s+will\s+always\s+be\s+available\b',
    '(?im)^\s*Guaranteed ad availability\b',
    '(?im)^\s*Approved by (?:Samsung|Galaxy Store)\b'
)) {
    if ($affirmativeStoreCopy -match $unsupportedClaim) {
        throw "Unsupported affirmative store claim found: $unsupportedClaim"
    }
}
foreach ($contradictoryClaim in @(
    '(?im)^\s*(?:(?:You|Players?) can\s+)?(?:Create|Register|Sign in|Log in)(?: for| with| to)? (?:an? )?account\b',
    '(?im)^\s*(?:(?:You|Players?) can\s+)?(?:Buy|Purchase) (?:coins|items|charms)\b',
    '(?im)^\s*(?:In-app purchases?) (?:are|is) available\b',
    '(?im)^\s*(?:Sync|Save) (?:your )?(?:progress )?(?:to|with) (?:the )?cloud\b',
    '(?im)^\s*Your progress syncs (?:to|with) (?:the )?cloud\b',
    '(?im)^\s*(?:We|The developer|The game) (?:collect|upload|send)s? (?:gameplay |crash )?(?:analytics|events|reports)\b',
    '(?im)^\s*Gameplay events are sent to (?:the )?(?:developer|server|backend)\b'
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

    $artReviewPath = Join-Path $root 'Docs/ArtReleaseReview.md'
    if (-not (Test-Path -LiteralPath $artReviewPath -PathType Leaf)) {
        $submissionBlockers.Add('Docs/ArtReleaseReview.md is missing.')
    }
    foreach ($assetName in @('Application icon', 'Phone screenshots')) {
        $row = [regex]::Match($assetInventory, "(?m)^\| $([regex]::Escape($assetName)) \|(?<body>.*)\|$")
        if (-not $row.Success -or $row.Groups['body'].Value -match '(?i)missing|not uploaded|prototype only|blocked|no files created') {
            $submissionBlockers.Add("Required media is not submission-ready: $assetName.")
        }
    }
    $task11EvidencePaths = @(
        'Docs/ReleaseEvidence/1.0.0/automated-tests.md',
        'Docs/ReleaseEvidence/1.0.0/owned-device.md',
        'Docs/ReleaseEvidence/1.0.0/remote-test-lab.md',
        'Docs/ReleaseEvidence/1.0.0/service-validation.md',
        'Docs/ReleaseEvidence/1.0.0/rc-decision.md'
    )
    foreach ($relative in $task11EvidencePaths) {
        if (-not (Test-Path -LiteralPath (Join-Path $root $relative) -PathType Leaf)) {
            $submissionBlockers.Add("Task 11 evidence is missing: $relative")
        }
    }
    if ($evidenceIndex -match '(?m)^\| (?:Automated tests|AAB inspection|Owned-device validation|Remote Test Lab|Service validation|RC decision) \| (?:Not run|Pending developer evidence) \|') {
        $submissionBlockers.Add('Evidence index still contains pending/not-run release evidence.')
    }

    $artReview = $null
    if (Test-Path -LiteralPath $artReviewPath -PathType Leaf) {
        $artReview = Read-SubmissionEvidence 'Docs/ArtReleaseReview.md'
    }
    $task11Evidence = @{}
    foreach ($relative in $task11EvidencePaths) {
        if (Test-Path -LiteralPath (Join-Path $root $relative) -PathType Leaf) {
            $task11Evidence[$relative] = Read-SubmissionEvidence $relative
        }
    }

    if ($mediaStatus -eq 'HUMAN_APPROVED' -and $null -ne $artReview) {
        if ((Get-StructuredField $artReview 'Release approval status' 'ArtReleaseReview.md') -ne 'HUMAN_APPROVED') { throw 'Art release approval status must be HUMAN_APPROVED.' }
        if (-not (Test-IsoDate (Get-StructuredField $artReview 'Approval date' 'ArtReleaseReview.md'))) { throw 'Art release approval date must be ISO yyyy-MM-dd.' }
        $attestation = Get-StructuredField $artReview 'Reviewer / attestation' 'ArtReleaseReview.md'
        if ([string]::IsNullOrWhiteSpace($attestation) -or $attestation.Length -lt 12) { throw 'Art release review requires a meaningful reviewer attestation.' }
        if ((Get-StructuredField $artReview 'Human creative pass' 'ArtReleaseReview.md') -ne 'COMPLETED') { throw 'Human creative pass must be COMPLETED.' }
        if ((Get-StructuredField $artReview 'Similarity review' 'ArtReleaseReview.md') -ne 'PASSED') { throw 'Similarity review must be PASSED.' }
        if ((Get-StructuredField $artReview 'Rights review' 'ArtReleaseReview.md') -ne 'PASSED') { throw 'Rights review must be PASSED.' }
        $approvedAssetId = Get-StructuredField $artReview 'Approved asset ID' 'ArtReleaseReview.md'
        if ($approvedAssetId -notmatch '\AART-BRAND-[0-9]{3}\z') { throw 'Approved asset ID must use the ART-BRAND-### format.' }
        if ($assetInventory -notmatch [regex]::Escape($approvedAssetId)) { throw 'Approved asset ID must appear in AssetInventory.md.' }
        $approvedIconSha = Get-StructuredField $artReview 'Approved icon SHA-256' 'ArtReleaseReview.md'
        $iconPath = Join-Path $root 'Assets/Art/Brand/AppIcon.png'
        if (-not (Test-Path -LiteralPath $iconPath -PathType Leaf)) { throw 'Approved application icon file is missing.' }
        $actualIconSha = (Get-FileHash -LiteralPath $iconPath -Algorithm SHA256).Hash
        if ($approvedIconSha -notmatch '\A[0-9a-fA-F]{64}\z' -or $approvedIconSha -ne $actualIconSha) { throw 'Approved icon SHA-256 does not match Assets/Art/Brand/AppIcon.png.' }
        $provenance = $documents['Docs/AIAssetProvenance.md']
        $provenanceSection = [regex]::Match($provenance, "(?ms)^###\s+$([regex]::Escape($approvedAssetId))\b.*?\r?\n(?<body>.*?)(?=^###\s+|\z)")
        if (-not $provenanceSection.Success -or $provenanceSection.Groups['body'].Value -notmatch [regex]::Escape($actualIconSha)) {
            throw 'Approved asset ID and current icon SHA-256 must appear together in AIAssetProvenance.md.'
        }
    }

    $allTask11Present = @($task11EvidencePaths | Where-Object { -not (Test-Path -LiteralPath (Join-Path $root $_) -PathType Leaf) }).Count -eq 0
    if ($evidenceStatus -eq 'DEVELOPER_CONFIRMED' -and $allTask11Present) {
        $automated = $task11Evidence['Docs/ReleaseEvidence/1.0.0/automated-tests.md']
        $owned = $task11Evidence['Docs/ReleaseEvidence/1.0.0/owned-device.md']
        $remoteLab = $task11Evidence['Docs/ReleaseEvidence/1.0.0/remote-test-lab.md']
        $service = $task11Evidence['Docs/ReleaseEvidence/1.0.0/service-validation.md']
        $rcEvidence = $task11Evidence['Docs/ReleaseEvidence/1.0.0/rc-decision.md']
        foreach ($entry in @(
            @{ Text = $automated; Name = 'automated-tests.md' },
            @{ Text = $owned; Name = 'owned-device.md' },
            @{ Text = $remoteLab; Name = 'remote-test-lab.md' },
            @{ Text = $service; Name = 'service-validation.md' },
            @{ Text = $rcEvidence; Name = 'rc-decision.md' }
        )) {
            Assert-CommonEvidence $entry.Text $entry.Name $signedRcSha
        }

        if ((Get-StructuredField $automated 'Unity version' 'automated-tests.md') -ne '6000.3.21f1') { throw 'Automated evidence Unity version must be 6000.3.21f1.' }
        foreach ($modeName in @('EditMode', 'PlayMode')) {
            if ((Get-StructuredField $automated "$modeName status" 'automated-tests.md') -ne 'PASSED') { throw "$modeName automated-test status must be PASSED." }
            $passed = Get-RequiredIntegerField $automated "$modeName passed" 'automated-tests.md'
            $total = Get-RequiredIntegerField $automated "$modeName total" 'automated-tests.md'
            if ($total -le 0 -or $passed -ne $total) { throw "$modeName automated-test passed/total counts must be equal and greater than zero." }
        }

        $ownedAabSha = Get-StructuredField $owned 'AAB SHA-256' 'owned-device.md'
        if ($ownedAabSha -notmatch '\A[0-9a-fA-F]{64}\z') { throw 'Owned-device AAB SHA-256 must be 64 hexadecimal characters.' }
        if ((Get-StructuredField $owned 'Matrix status' 'owned-device.md') -ne 'PASSED') { throw 'Owned-device matrix status must be PASSED.' }
        $ownedModel = Get-StructuredField $owned 'Owned device model' 'owned-device.md'
        if ([string]::IsNullOrWhiteSpace($ownedModel)) { throw 'Owned-device evidence requires a device model.' }
        $ownedApi = Get-RequiredIntegerField $owned 'Android API' 'owned-device.md'
        if ($ownedApi -lt 29) { throw 'Owned-device Android API must be at least 29.' }
        foreach ($check in @('First launch', 'Tutorial', 'Three shifts')) {
            if ((Get-StructuredField $owned $check 'owned-device.md') -ne 'PASSED') { throw "Owned-device check must be PASSED: $check." }
        }
        foreach ($zeroField in @('P0 defects', 'P1 defects', 'Reward anomalies')) {
            if ((Get-RequiredIntegerField $owned $zeroField 'owned-device.md') -ne 0) { throw "Owned-device $zeroField must be zero." }
        }

        if ((Get-StructuredField $remoteLab 'Matrix status' 'remote-test-lab.md') -ne 'PASSED') { throw 'Remote Test Lab matrix status must be PASSED.' }
        if ((Get-RequiredIntegerField $remoteLab 'Profile count' 'remote-test-lab.md') -lt 3) { throw 'Remote Test Lab requires at least three profiles.' }
        foreach ($profileField in @('Galaxy A-series profile', 'Galaxy S-series profile', 'Galaxy Fold profile')) {
            $profile = Get-StructuredField $remoteLab $profileField 'remote-test-lab.md'
            if ([string]::IsNullOrWhiteSpace($profile) -or $profile.Length -lt 10) { throw "Remote Test Lab profile is incomplete: $profileField." }
        }

        if ((Get-StructuredField $service 'Service validation status' 'service-validation.md') -ne 'PASSED') { throw 'Service validation status must be PASSED.' }
        if ((Get-StructuredField $service 'No-remote Release gate' 'service-validation.md') -ne 'PASSED') { throw 'No-remote Release gate evidence must be PASSED.' }
        if ((Get-StructuredField $service 'Observed service traffic' 'service-validation.md') -ne 'ADMOB_UMP_ONLY') { throw 'Observed service traffic must be ADMOB_UMP_ONLY.' }
        if ((Get-RequiredIntegerField $service 'Duplicate reward grants' 'service-validation.md') -ne 0) { throw 'Duplicate reward grants must be zero.' }
        if ((Get-StructuredField $service 'Unavailable-ad base progression' 'service-validation.md') -ne 'PASSED') { throw 'Unavailable-ad base progression must be PASSED.' }
        if ((Get-StructuredField $service 'UMP launch update' 'service-validation.md') -ne 'PASSED') { throw 'UMP launch update must be PASSED.' }
        if ((Get-RequiredIntegerField $service 'Ad requests before CanRequestAds' 'service-validation.md') -ne 0) { throw 'Ad requests before CanRequestAds must be zero.' }

        if ((Get-StructuredField $rcEvidence 'RC Decision' 'rc-decision.md') -ne 'ACCEPT RC') { throw 'RC evidence Decision must be ACCEPT RC.' }
        $rcAabSha = Get-StructuredField $rcEvidence 'AAB SHA-256' 'rc-decision.md'
        if ($rcAabSha -notmatch '\A[0-9a-fA-F]{64}\z' -or $rcAabSha -ne $ownedAabSha) { throw 'RC and owned-device AAB SHA-256 values must match.' }
        foreach ($zeroField in @('P0 defects', 'P1 defects')) {
            if ((Get-RequiredIntegerField $rcEvidence $zeroField 'rc-decision.md') -ne 0) { throw "RC $zeroField must be zero." }
        }
        foreach ($gateField in @('Rights gate', 'Store docs gate', 'Test matrix')) {
            if ((Get-StructuredField $rcEvidence $gateField 'rc-decision.md') -ne 'PASSED') { throw "RC $gateField must be PASSED." }
        }
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
