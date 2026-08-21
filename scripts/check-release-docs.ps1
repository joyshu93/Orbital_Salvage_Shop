param(
    [ValidateSet('Repository', 'Submission')]
    [string]$Mode = 'Repository',

    [string]$ProjectRoot
)

$ErrorActionPreference = 'Stop'
$strictUtf8 = New-Object System.Text.UTF8Encoding($false, $true)
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

function Read-StrictUtf8File([string]$Path, [string]$DocumentName) {
    try {
        return [System.IO.File]::ReadAllText($Path, $strictUtf8)
    } catch [System.Text.DecoderFallbackException] {
        throw "$DocumentName is not valid UTF-8 text."
    }
}

function Read-ReleaseDocument([string]$RelativePath) {
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Missing release document: $RelativePath"
    }
    return Read-StrictUtf8File $path $RelativePath
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
        '[\x00-\x08\x0B\x0C\x0E-\x1F]',
        '(?im)(?:^|[\s`"''(])(?:[A-Za-z]:[\\/]|file:/+|/(?:Users|home|var|tmp|etc)/|/(?!/)[A-Za-z0-9._-]+/)',
        '(?m)(?:^|[\s`"''(])(?:\\\\|//)[A-Za-z0-9._-]+[\\/][A-Za-z0-9.$_-]+',
        '(?i)-----BEGIN [A-Z ]*PRIVATE KEY-----',
        '(?i)\b(?:gh[pousr]_[A-Za-z0-9]{20,}|github_pat_[A-Za-z0-9_]{20,}|AKIA[0-9A-Z]{16}|Bearer\s+[A-Za-z0-9._~+/-]{12,})\b',
        '(?i)\beyJ[A-Za-z0-9_-]{8,}\.[A-Za-z0-9_-]{8,}\.[A-Za-z0-9_-]{8,}\b',
        '(?im)^\s*(?:access[_ -]?token|refresh[_ -]?token|password|credential|client[_ -]?secret|secret|api[_ -]?key)\s*[:=]\s*(?!REDACTED\b)\S+',
        '(?i)ca-app-pub-\d+[/~]\d+',
        '(?im)^\s*(?:government ID|driver(?:''s)? licen[cs]e(?: number)?|passport(?: number)?|national ID|resident registration(?: number)?|social security(?: number)?|tax ID|bank account(?: number)?|bank statement|routing number|credit card(?: number)?|birth certificate|insurance ID|identity document|financial record)\s*[:=]\s*(?!\[?REDACTED\]?\s*$)\S.*$',
        '(?m)(?:\[[A-Z][A-Z0-9_]+\]|\b(?:PENDING|TODO|TBD|PLACEHOLDER|NOT RUN)\b|^\s*-\s*\[\s\])'
    )
    foreach ($pattern in $forbiddenPatterns) {
        $match = [regex]::Match($Text, $pattern)
        if ($match.Success) {
            throw "$DocumentName contains forbidden path, secret, identity/financial record, ad identifier, or pending placeholder evidence near '$($match.Value)'."
        }
    }
}

function Assert-RepositoryRelativeEvidencePath([string]$Value, [string]$DocumentName, [string]$FieldName, [string]$ExtensionPattern) {
    if ($Value -notmatch "\A(?:Docs|Assets|ProjectSettings|Packages|scripts)/[A-Za-z0-9._/-]+$ExtensionPattern\z" -or
        $Value -match '(?:^|/)\.\.(?:/|$)') {
        throw "$DocumentName field '$FieldName' must be a sanitized repository-relative path."
    }
}

function Read-SubmissionEvidence([string]$RelativePath) {
    $path = Join-Path $root $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw "Submission evidence is missing: $RelativePath"
    }
    $text = Read-StrictUtf8File $path $RelativePath
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

$affirmativeStoreCopy = $listingEn + "`n" + $listingKo
foreach ($unsupportedClaim in @(
    '(?im)^\s*(?:An?\s+)?award[- ]winning\b',
    '(?im)^\s*(?:#?1|number one)\b.*(?:game|puzzle|rank)',
    '(?im)^\s*(?:Ads?|Rewarded ads?)\s+(?:are|is)\s+guaranteed\b',
    '(?im)^\s*Ads?\s+(?:are|is)\s+always\s+available\b',
    '(?im)^\s*(?:An?\s+)?ad\s+will\s+always\s+be\s+available\b',
    '(?im)^\s*Guaranteed ad availability\b',
    '(?im)^\s*Approved by (?:Samsung|Galaxy Store)\b'
)) {
    if ($affirmativeStoreCopy -match $unsupportedClaim) {
        throw "Unsupported affirmative store claim found: $unsupportedClaim"
    }
}
foreach ($contradictoryClaim in @(
    '(?im)^\s*(?:(?:You|Players?) (?:can|may)\s+)?(?:Create|Register|Sign in|Log in)(?: for| with| to)? (?:an? )?account\b',
    '(?im)^\s*(?:(?:You|Players?) can\s+)?(?:Buy|Purchase) (?:coins|items|charms)\b',
    '(?im)^\s*(?:In-app purchases?) (?:are|is) available\b',
    '(?im)^\s*(?:Sync|Save) (?:your )?(?:progress )?(?:to|with) (?:the )?cloud\b',
    '(?im)^\s*Your progress syncs (?:to|with) (?:the )?cloud\b',
    '(?im)^\s*Cloud saves? (?:are|is) available\b',
    '(?im)^\s*(?:We|The developer|The game) (?:collect|upload|send)s? (?:gameplay |crash )?(?:analytics|events|reports)\b',
    '(?im)^\s*Gameplay events are sent to (?:the )?(?:developer|server|backend)\b',
    '(?im)^\s*We upload gameplay data to (?:our|the) servers?\b'
)) {
    if ($affirmativeStoreCopy -match $contradictoryClaim) {
        throw "Store copy contradicts the v1 service boundary: $contradictoryClaim"
    }
}
foreach ($koreanContradictoryClaim in @(
    '(?m)^\s*(?:계정(?:을|이)?\s*(?:생성|만들|등록)할\s*수\s*있습니다|(?:계정\s*)?(?:생성|등록|로그인)(?:이|은)?\s*가능합니다|로그인할\s*수\s*있습니다)',
    '(?m)^\s*광고(?:는|가)?\s*(?:항상|언제나)\s*(?:(?:이용|사용|시청)할\s*수\s*있습니다|제공됩니다)',
    '(?m)^\s*(?:(?:코인|아이템|장식)(?:을|를)?\s*(?:구매할|살)\s*수\s*있습니다|인앱\s*구매(?:를)?\s*(?:이용|사용)할\s*수\s*있습니다)',
    '(?m)^\s*클라우드\s*(?:저장|세이브)(?:은|이|을|를)?\s*(?:(?:사용|이용)할\s*수\s*있습니다|제공됩니다)',
    '(?m)^\s*(?:진행\s*상황|진행도)(?:을|를)?\s*클라우드에\s*동기화합니다',
    '(?m)^\s*(?:게임플레이\s*데이터|게임\s*이벤트|충돌\s*(?:데이터|보고서)|크래시\s*(?:데이터|보고서))(?:를|을)?(?:\s*서버로)?\s*(?:업로드|전송|수집)합니다'
)) {
    if ($affirmativeStoreCopy -match $koreanContradictoryClaim) {
        throw "Store copy contradicts the v1 service boundary: $koreanContradictoryClaim"
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

    foreach ($assetName in @('Application icon', 'Phone screenshots')) {
        $row = [regex]::Match($assetInventory, "(?m)^\| $([regex]::Escape($assetName)) \|(?<body>.*)\|$")
        if (-not $row.Success -or $row.Groups['body'].Value -match '(?i)missing|not uploaded|prototype only|blocked|no files created') {
            $submissionBlockers.Add("Required media is not submission-ready: $assetName.")
        }
    }

    $artReviewRelative = 'Docs/ArtReleaseReview.md'
    $evidenceRootRelative = 'Docs/ReleaseEvidence/1.0.0'
    $requiredEvidenceNames = @(
        'README.md',
        'automated-tests.md',
        'aab-inspection.md',
        'owned-device.md',
        'remote-test-lab.md',
        'service-validation.md',
        'rc-decision.md'
    )
    $artReviewPath = Join-Path $root $artReviewRelative
    if (-not (Test-Path -LiteralPath $artReviewPath -PathType Leaf)) { $submissionBlockers.Add("$artReviewRelative is missing.") }
    $evidenceRoot = Join-Path $root $evidenceRootRelative
    foreach ($name in $requiredEvidenceNames) {
        if (-not (Test-Path -LiteralPath (Join-Path $evidenceRoot $name) -PathType Leaf)) {
            $submissionBlockers.Add("Task 11 evidence is missing: $evidenceRootRelative/$name")
        }
    }
    if ($evidenceIndex -match '(?m)^\| (?:Automated tests|AAB inspection|Owned-device validation|Remote Test Lab|Service validation|RC decision) \| (?:Not run|Pending developer evidence) \|') {
        $submissionBlockers.Add('Evidence index still contains pending/not-run release evidence.')
    }
    if ($submissionBlockers.Count -gt 0) {
        throw "Submission documentation is blocked:`n- $($submissionBlockers -join "`n- ")"
    }

    # A confirmed Submission scans the complete sanitized evidence tree, including future files.
    $recursiveEvidence = @{}
    foreach ($path in [System.IO.Directory]::EnumerateFiles($evidenceRoot, '*', [System.IO.SearchOption]::AllDirectories)) {
        if ([System.IO.Path]::GetExtension($path) -ne '.md') {
            throw "Release evidence must contain sanitized Markdown only: $path"
        }
        $relative = $path.Substring($root.Length).TrimStart([char[]]@('\', '/')).Replace('\', '/')
        $text = Read-StrictUtf8File $path $relative
        Assert-SanitizedEvidence $text $relative
        $recursiveEvidence[$relative] = $text
    }
    $artReview = Read-StrictUtf8File $artReviewPath $artReviewRelative
    Assert-SanitizedEvidence $artReview $artReviewRelative

    # Task 4 human art approval is bound to the exact submitted icon and provenance record.
    if ((Get-StructuredField $artReview 'Release approval status' 'ArtReleaseReview.md') -ne 'HUMAN_APPROVED') { throw 'Art release approval status must be HUMAN_APPROVED.' }
    if (-not (Test-IsoDate (Get-StructuredField $artReview 'Approval date' 'ArtReleaseReview.md'))) { throw 'Art release approval date must be ISO yyyy-MM-dd.' }
    $attestation = Get-StructuredField $artReview 'Developer attestation' 'ArtReleaseReview.md'
    if ($attestation -notmatch '(?i)\b(?:I|developer)\b.*\b(?:made|created|edited|changed)\b.*\b(?:reviewed|approve)\b.*\brelease\b' -or $attestation.Length -lt 70) {
        throw 'Art release review requires a substantive developer creative-work attestation.'
    }
    if ((Get-StructuredField $artReview 'Human creative pass' 'ArtReleaseReview.md') -ne 'COMPLETED') { throw 'Human creative pass must be COMPLETED.' }
    $approvedAssetId = Get-StructuredField $artReview 'Approved asset ID' 'ArtReleaseReview.md'
    if ($approvedAssetId -notmatch '\AART-BRAND-[0-9]{3}\z') { throw 'Approved asset ID must use the ART-BRAND-### format.' }
    $approvedPath = Get-StructuredField $artReview 'Repository path' 'ArtReleaseReview.md'
    if ($approvedPath -ne 'Assets/Art/Brand/AppIcon.png') { throw 'Art release Repository path must be Assets/Art/Brand/AppIcon.png.' }
    if ((Get-StructuredField $artReview 'Release decision' 'ArtReleaseReview.md') -ne 'Approved for release') { throw 'Art release decision must be Approved for release.' }
    $reviewRowPattern = "(?m)^\|\s*$([regex]::Escape($approvedAssetId))\s*\|\s*Assets/Art/Brand/AppIcon\.png\s*\|\s*Approved for release\s*\|$"
    if ($artReview -notmatch $reviewRowPattern) { throw 'Art review Application icon row must bind the approved ID, path, and decision.' }
    $iconInventoryRow = [regex]::Match($assetInventory, '(?m)^\| Application icon \|(?<body>.*)\|$')
    if (-not $iconInventoryRow.Success -or $iconInventoryRow.Groups['body'].Value -notmatch [regex]::Escape($approvedAssetId)) {
        throw 'Approved asset ID must appear in the Application icon inventory row.'
    }
    foreach ($changeField in @('Composition changes', 'Silhouette changes', 'Palette changes', 'Line / shape cleanup')) {
        $value = Get-StructuredField $artReview $changeField 'ArtReleaseReview.md'
        if ($value.Length -lt 45 -or $value -match '(?i)^(?:changed it|done|complete|n/a|none)$') { throw "Art review field '$changeField' is not substantive." }
    }
    $beforeEvidence = Get-StructuredField $artReview 'Before evidence' 'ArtReleaseReview.md'
    $afterEvidence = Get-StructuredField $artReview 'After evidence' 'ArtReleaseReview.md'
    $repositoryRelativePattern = '(?!\.git(?:/|$))[A-Za-z0-9._-]+(?:/[A-Za-z0-9._-]+)+'
    if ($beforeEvidence -notmatch "\A(?:Git commit [0-9a-fA-F]{40}|$repositoryRelativePattern)\z") { throw 'Before evidence must be a repository-relative file or full Git commit.' }
    if ($afterEvidence -notmatch "\A$repositoryRelativePattern\z") { throw 'After evidence must be a repository-relative file.' }
    if ($beforeEvidence -eq $afterEvidence -or $beforeEvidence -match '(?:^|/)\.\.(?:/|$)' -or $afterEvidence -match '(?:^|/)\.\.(?:/|$)') {
        throw 'Art before/after evidence must be distinct, safe references.'
    }
    if ($afterEvidence -ne 'Assets/Art/Brand/AppIcon.png') { throw 'After evidence must bind the current Assets/Art/Brand/AppIcon.png.' }

    $iconPath = Join-Path $root 'Assets/Art/Brand/AppIcon.png'
    if (-not (Test-Path -LiteralPath $iconPath -PathType Leaf)) { throw 'Approved application icon file is missing.' }
    $actualIconSha = (Get-FileHash -LiteralPath $iconPath -Algorithm SHA256).Hash
    if ($beforeEvidence -match '\AGit commit (?<sha>[0-9a-fA-F]{40})\z') {
        $beforeCommitSha = $Matches['sha']
        $commitArgs = @('-C', $root, 'rev-parse', '--verify', "${beforeCommitSha}^{commit}")
        $commitObject = & git @commitArgs 2>$null
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace(($commitObject -join ''))) { throw 'Before-evidence Git commit does not exist.' }
        $historicalArgs = @('-C', $root, 'rev-parse', '--verify', "${beforeCommitSha}:Assets/Art/Brand/AppIcon.png")
        $historicalBlob = & git @historicalArgs 2>$null
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace(($historicalBlob -join ''))) { throw 'Before-evidence Git commit has no AppIcon blob.' }
        $currentArgs = @('-C', $root, 'hash-object', '--', $iconPath)
        $currentBlob = & git @currentArgs 2>$null
        if ($LASTEXITCODE -ne 0 -or (($historicalBlob -join '').Trim() -eq ($currentBlob -join '').Trim())) { throw 'Before-evidence Git AppIcon must differ from the current icon.' }
    } else {
        $beforePath = Join-Path $root $beforeEvidence
        if (-not (Test-Path -LiteralPath $beforePath -PathType Leaf)) { throw 'Before-evidence repository file does not exist.' }
        $beforeSha = (Get-FileHash -LiteralPath $beforePath -Algorithm SHA256).Hash
        if ($beforeSha -eq $actualIconSha) { throw 'Before-evidence file must differ from the current icon.' }
    }
    $similarityMethod = Get-StructuredField $artReview 'Similarity search method' 'ArtReleaseReview.md'
    if ($similarityMethod.Length -lt 55 -or $similarityMethod -notmatch '(?i)(?:reverse.image|search).*(?:store|keyword|similar)') { throw 'Similarity search method must describe a substantive search.' }
    foreach ($resultField in @('Similarity review result', 'Trademark review', 'Rights review', 'Watermark review', 'Signature review', 'Named-artist review', 'Protected-character review')) {
        if ((Get-StructuredField $artReview $resultField 'ArtReleaseReview.md') -ne 'PASSED') { throw "Art review field '$resultField' must be PASSED." }
    }
    $approvedIconSha = Get-StructuredField $artReview 'Approved icon SHA-256' 'ArtReleaseReview.md'
    if ($approvedIconSha -notmatch '\A[0-9a-fA-F]{64}\z' -or $approvedIconSha -ne $actualIconSha) { throw 'Approved icon SHA-256 does not match Assets/Art/Brand/AppIcon.png.' }
    $provenance = $documents['Docs/AIAssetProvenance.md']
    $provenanceSection = [regex]::Match($provenance, "(?ms)^###\s+$([regex]::Escape($approvedAssetId))\b.*?\r?\n(?<body>.*?)(?=^###\s+|\z)")
    $provenanceBody = if ($provenanceSection.Success) { $provenanceSection.Groups['body'].Value } else { '' }
    if (-not $provenanceSection.Success -or
        $provenanceBody -notmatch [regex]::Escape($actualIconSha) -or
        $provenanceBody -notmatch 'Assets/Art/Brand/AppIcon\.png' -or
        $provenanceBody -notmatch '(?i)Approved for release' -or
        $provenanceBody -notmatch [regex]::Escape($beforeEvidence) -or
        $provenanceBody -notmatch [regex]::Escape($afterEvidence)) {
        throw 'Approved provenance must contain the current icon path/hash/status and matching before/after evidence.'
    }
    $provenanceHumanEdits = [regex]::Match($provenanceBody, '(?im)^\|\s*Human edits\s*\|(?<value>.*?)\|\s*$').Groups['value'].Value
    if ($provenanceHumanEdits.Length -lt 80 -or
        $provenanceHumanEdits -notmatch '(?i)composition' -or
        $provenanceHumanEdits -notmatch '(?i)silhouette' -or
        $provenanceHumanEdits -notmatch '(?i)palette' -or
        $provenanceHumanEdits -notmatch '(?i)line\s*/?\s*shape') {
        throw 'Approved provenance Human edits must substantively cover composition, silhouette, palette, and line/shape cleanup.'
    }

    $automated = $recursiveEvidence["$evidenceRootRelative/automated-tests.md"]
    $aabInspection = $recursiveEvidence["$evidenceRootRelative/aab-inspection.md"]
    $owned = $recursiveEvidence["$evidenceRootRelative/owned-device.md"]
    $remoteLab = $recursiveEvidence["$evidenceRootRelative/remote-test-lab.md"]
    $service = $recursiveEvidence["$evidenceRootRelative/service-validation.md"]
    $rcEvidence = $recursiveEvidence["$evidenceRootRelative/rc-decision.md"]
    foreach ($entry in @(
        @{ Text = $automated; Name = 'automated-tests.md' },
        @{ Text = $aabInspection; Name = 'aab-inspection.md' },
        @{ Text = $owned; Name = 'owned-device.md' },
        @{ Text = $remoteLab; Name = 'remote-test-lab.md' },
        @{ Text = $service; Name = 'service-validation.md' },
        @{ Text = $rcEvidence; Name = 'rc-decision.md' }
    )) { Assert-CommonEvidence $entry.Text $entry.Name $signedRcSha }

    if ((Get-StructuredField $automated 'Unity version' 'automated-tests.md') -ne '6000.3.21f1') { throw 'Automated evidence Unity version must be 6000.3.21f1.' }
    foreach ($modeName in @('EditMode', 'PlayMode')) {
        if ((Get-StructuredField $automated "$modeName status" 'automated-tests.md') -ne 'PASSED') { throw "$modeName status must be PASSED." }
        $passed = Get-RequiredIntegerField $automated "$modeName passed" 'automated-tests.md'
        $total = Get-RequiredIntegerField $automated "$modeName total" 'automated-tests.md'
        if ($total -le 0 -or $passed -ne $total) { throw "$modeName passed and total must match and be positive." }
        Assert-RepositoryRelativeEvidencePath (Get-StructuredField $automated "$modeName XML path" 'automated-tests.md') 'automated-tests.md' "$modeName XML path" '\.xml'
        Assert-RepositoryRelativeEvidencePath (Get-StructuredField $automated "$modeName log path" 'automated-tests.md') 'automated-tests.md' "$modeName log path" '\.log'
    }

    if ((Get-StructuredField $aabInspection 'Inspection status' 'aab-inspection.md') -ne 'PASSED') { throw 'AAB inspection must be PASSED.' }
    if ((Get-StructuredField $aabInspection 'Bundletool version' 'aab-inspection.md') -ne '1.18.3') { throw 'AAB inspection bundletool version must be 1.18.3.' }
    $aabSha = Get-StructuredField $aabInspection 'AAB SHA-256' 'aab-inspection.md'
    if ($aabSha -notmatch '\A[0-9a-fA-F]{64}\z') { throw 'AAB SHA-256 must be 64 hexadecimal characters.' }
    $aabExactFields = @{
        'Hash match' = 'PASSED'; 'Package ID' = 'com.joyshu93.curioclerknightshift'; 'Version name' = '1.0.0';
        'Version code' = '10000'; 'Minimum API' = '29'; 'Target API' = '36'; 'Architecture' = 'ARM64';
        'Backend' = 'IL2CPP'; 'Symbols' = 'PRESENT'
    }
    foreach ($field in $aabExactFields.Keys) {
        if ((Get-StructuredField $aabInspection $field 'aab-inspection.md') -ne $aabExactFields[$field]) { throw "AAB inspection field '$field' is invalid." }
    }

    if ((Get-StructuredField $owned 'AAB SHA-256' 'owned-device.md') -ne $aabSha) { throw 'Owned-device and inspected AAB SHA-256 values must match.' }
    if ((Get-StructuredField $owned 'Matrix status' 'owned-device.md') -ne 'PASSED') { throw 'Owned-device matrix status must be PASSED.' }
    if ((Get-StructuredField $owned 'Owned device model' 'owned-device.md').Length -lt 8) { throw 'Owned-device model is incomplete.' }
    if ((Get-StructuredField $owned 'Android version' 'owned-device.md') -notmatch '\A[0-9]{2}(?:\.[0-9]+)?\z') { throw 'Owned-device Android version is invalid.' }
    if ((Get-StructuredField $owned 'Resolution / aspect' 'owned-device.md') -notmatch '\A[0-9]{3,5}x[0-9]{3,5}\s*/\s*[0-9.]+:[0-9.]+\z') { throw 'Owned-device resolution/aspect is invalid.' }
    if ((Get-StructuredField $owned 'Install source' 'owned-device.md').Length -lt 20) { throw 'Owned-device install source is incomplete.' }
    if ((Get-RequiredIntegerField $owned 'Android API' 'owned-device.md') -lt 29) { throw 'Owned-device Android API must be at least 29.' }
    if ((Get-StructuredField $owned 'Build version name' 'owned-device.md') -ne '1.0.0' -or (Get-StructuredField $owned 'Build version code' 'owned-device.md') -ne '10000') { throw 'Owned-device build must be 1.0.0/10000.' }
    foreach ($check in @('First launch', 'Tutorial', 'Three shifts', 'Drag / buttons / Hold', 'Offline mode', 'Pause / resume', 'Force-stop recovery', 'Corrupt-save recovery', 'EN / KO language', 'UMP grant', 'UMP deny', 'UMP privacy options', 'Ad earned', 'Ad dismissed', 'Ad no-fill', 'Ad failure', 'Ad duplicate callback', 'Relaunch')) {
        if ((Get-StructuredField $owned $check 'owned-device.md') -ne 'PASSED') { throw "Owned-device check must be PASSED: $check." }
    }
    foreach ($zero in @('P0 defects', 'P1 defects', 'Reward anomalies')) { if ((Get-RequiredIntegerField $owned $zero 'owned-device.md') -ne 0) { throw "Owned-device $zero must be zero." } }

    if ((Get-StructuredField $remoteLab 'Matrix status' 'remote-test-lab.md') -ne 'PASSED' -or (Get-RequiredIntegerField $remoteLab 'Profile count' 'remote-test-lab.md') -lt 3) { throw 'Remote Test Lab matrix requires at least three passed profiles.' }
    $androidMajors = @()
    $aspectClasses = @()
    $rtlModels = @()
    $modelPatterns = @{
        'Galaxy A' = '\AGalaxy A[A-Za-z0-9 +()._-]+\z'
        'Galaxy S' = '\AGalaxy S[A-Za-z0-9 +()._-]+\z'
        'Galaxy Fold' = '\A(?:Galaxy Z Fold|Galaxy Fold)[A-Za-z0-9 +()._-]+\z'
    }
    foreach ($profile in @('Galaxy A', 'Galaxy S', 'Galaxy Fold')) {
        $model = Get-StructuredField $remoteLab "$profile model" 'remote-test-lab.md'
        if ($model -notmatch $modelPatterns[$profile]) { throw "$profile model must identify the required Samsung Galaxy family." }
        $rtlModels += $model
        $major = Get-RequiredIntegerField $remoteLab "$profile Android major" 'remote-test-lab.md'
        if ($major -lt 10) { throw "$profile Android major is invalid." }
        $androidMajors += $major
        $aspect = Get-StructuredField $remoteLab "$profile aspect class" 'remote-test-lab.md'
        if ($aspect -notmatch '\A[A-Z][A-Z0-9_]+\z') { throw "$profile aspect class is invalid." }
        $aspectClasses += $aspect
        foreach ($check in @('install', 'launch', 'tutorial', 'one shift', 'language', 'safe area', 'pause / resume')) {
            if ((Get-StructuredField $remoteLab "$profile $check" 'remote-test-lab.md') -ne 'PASSED') { throw "$profile check must be PASSED: $check." }
        }
    }
    if (@($rtlModels | Sort-Object -Unique).Count -ne 3) { throw 'Remote Test Lab model values must be distinct.' }
    if (@($androidMajors | Sort-Object -Unique).Count -lt 2 -or @($aspectClasses | Sort-Object -Unique).Count -lt 2) { throw 'Remote Test Lab must span at least two Android majors and two aspect classes.' }

    $serviceExact = @{
        'Service validation status'='PASSED'; 'No-remote Release gate'='PASSED'; 'Google Mobile Ads Unity version'='11.3.0';
        'EDM4U version'='1.2.188'; 'UMP update every launch'='PASSED'; 'Unavailable-ad base progression'='PASSED';
        'Package graph remote telemetry'='ABSENT'; 'Observed service traffic'='ADMOB_UMP_ONLY'
    }
    foreach ($field in $serviceExact.Keys) { if ((Get-StructuredField $service $field 'service-validation.md') -ne $serviceExact[$field]) { throw "Service field '$field' is invalid." } }
    if ((Get-RequiredIntegerField $service 'Earned rewards' 'service-validation.md') -ne 1) { throw 'Service validation must record exactly one earned reward.' }
    foreach ($zero in @('Ad requests before CanRequestAds', 'Duplicate reward grants', 'Gameplay / crash endpoints observed', 'Local payload transmissions', 'Local payload logs', 'Local payload cache writes', 'Local payload persistence writes')) {
        if ((Get-RequiredIntegerField $service $zero 'service-validation.md') -ne 0) { throw "Service field '$zero' must be zero." }
    }

    if ((Get-StructuredField $rcEvidence 'RC Decision' 'rc-decision.md') -ne 'ACCEPT RC') { throw 'RC evidence Decision must be ACCEPT RC.' }
    if ((Get-StructuredField $rcEvidence 'Version name' 'rc-decision.md') -ne '1.0.0' -or (Get-StructuredField $rcEvidence 'Version code' 'rc-decision.md') -ne '10000') { throw 'RC build must be 1.0.0/10000.' }
    if ((Get-StructuredField $rcEvidence 'AAB SHA-256' 'rc-decision.md') -ne $aabSha) { throw 'RC and inspected AAB SHA-256 values must match.' }
    foreach ($zero in @('P0 defects', 'P1 defects')) { if ((Get-RequiredIntegerField $rcEvidence $zero 'rc-decision.md') -ne 0) { throw "RC $zero must be zero." } }
    foreach ($gateField in @('Rights gate', 'Store docs gate', 'Test matrix')) { if ((Get-StructuredField $rcEvidence $gateField 'rc-decision.md') -ne 'PASSED') { throw "RC $gateField must be PASSED." } }

    $submissionDocs = @($privacyPolicy, $sellerSetup, $listingEn, $listingKo, $reviewNotes, $dataSafety, $ratingAnswers, $assetInventory, $evidenceIndex) -join "`n"
    $unresolvedTokens = @([regex]::Matches($submissionDocs, '\[[A-Z][A-Z0-9_]+\]') | ForEach-Object { $_.Value } | Sort-Object -Unique)
    if ($unresolvedTokens.Count -gt 0) { throw "Unresolved submission tokens: $($unresolvedTokens -join ', ')" }
    Write-Host 'Release documentation gate passed (Submission mode).'
    return
}


Write-Host "Release documentation gate passed ($Mode mode)."
