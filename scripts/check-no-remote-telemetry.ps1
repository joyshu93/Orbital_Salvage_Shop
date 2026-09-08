param(
    [string]$ProjectRoot,
    [ValidateSet('Repository', 'Release')]
    [string]$Mode = 'Repository'
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $ProjectRoot = Join-Path $PSScriptRoot '..'
}

$projectRootPath = [System.IO.Path]::GetFullPath($ProjectRoot)
$failures = [System.Collections.Generic.List[string]]::new()

function Add-Failure {
    param([string]$Message)

    $failures.Add($Message)
}

function Get-NormalizedRelativePath {
    param([string]$Path)

    $rootWithSeparator = $projectRootPath
    $separator = [System.IO.Path]::DirectorySeparatorChar.ToString()
    if (-not $rootWithSeparator.EndsWith($separator, [StringComparison]::Ordinal)) {
        $rootWithSeparator += $separator
    }

    $rootUri = [Uri]$rootWithSeparator
    $pathUri = [Uri]([System.IO.Path]::GetFullPath($Path))
    return [Uri]::UnescapeDataString($rootUri.MakeRelativeUri($pathUri).ToString()).Replace('\', '/')
}

function Read-JsonFile {
    param([string]$RelativePath)

    $path = Join-Path $projectRootPath $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "Missing required JSON file: $RelativePath"
        return $null
    }

    try {
        return Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
    }
    catch {
        Add-Failure "Invalid JSON in ${RelativePath}: $($_.Exception.Message)"
        return $null
    }
}

function Remove-CSharpCommentsAndLiterals {
    param([string]$Content)

    # Strip normal/verbatim strings, chars, and both comment forms before marker scans.
    # This keeps examples, URLs, and documentation comments from triggering the release gate.
    $nonCodePattern = '(?ms)("(?:\\.|[^"\\])*"|@"(?:""|[^"])*"|''(?:\\.|[^''\\])*''|//.*?$|/\*.*?\*/)'
    return [System.Text.RegularExpressions.Regex]::Replace($Content, $nonCodePattern, ' ')
}

$requiredDependencies = [ordered]@{
    'com.google.ads.mobile' = '11.3.0'
    'com.google.external-dependency-manager' = 'file:../GooglePackages/com.google.external-dependency-manager-1.2.188.tgz'
}
$forbiddenDependencyNamePattern = '(?i)(firebase|analytics|crashlytics|telemetry|sentry|appcenter|bugsnag|datadog|newrelic|amplitude|mixpanel|gameanalytics)'

$manifest = Read-JsonFile 'Packages/manifest.json'
$dependencies = $null
if ($null -ne $manifest) {
    $dependenciesProperty = $manifest.PSObject.Properties['dependencies']
    if ($null -eq $dependenciesProperty -or $dependenciesProperty.Value -isnot [pscustomobject]) {
        Add-Failure 'Packages/manifest.json must contain a valid dependencies object.'
    }
    else {
        $dependencies = $dependenciesProperty.Value.PSObject.Properties
    }
}

if ($null -ne $dependencies) {
    foreach ($dependency in $dependencies) {
        if ($dependency.Name -match $forbiddenDependencyNamePattern) {
            Add-Failure "Forbidden remote telemetry package dependency remains in Packages/manifest.json: $($dependency.Name)"
        }
    }

    foreach ($required in $requiredDependencies.GetEnumerator()) {
        $property = $dependencies | Where-Object Name -eq $required.Key | Select-Object -First 1
        if ($null -eq $property -or $property.Value -ne $required.Value) {
            Add-Failure "Required v1 dependency is missing or changed: $($required.Key) must be $($required.Value)"
        }
    }
}

$packageLock = Read-JsonFile 'Packages/packages-lock.json'
$lockDependencies = $null
if ($null -ne $packageLock) {
    $lockDependenciesProperty = $packageLock.PSObject.Properties['dependencies']
    if ($null -eq $lockDependenciesProperty -or $lockDependenciesProperty.Value -isnot [pscustomobject]) {
        Add-Failure 'Packages/packages-lock.json must contain a valid dependencies object.'
    }
    else {
        $lockDependencies = $lockDependenciesProperty.Value.PSObject.Properties
        foreach ($dependency in $lockDependencies) {
            if ($dependency.Name -match $forbiddenDependencyNamePattern) {
                Add-Failure "Forbidden remote telemetry package dependency remains in Packages/packages-lock.json: $($dependency.Name)"
            }
        }
    }
}

$unresolvedRequiredLockEntries = [System.Collections.Generic.List[string]]::new()
foreach ($required in $requiredDependencies.GetEnumerator()) {
    $resolved = $null
    if ($null -ne $lockDependencies) {
        $resolved = $lockDependencies | Where-Object Name -eq $required.Key | Select-Object -First 1
    }

    if ($null -eq $resolved -or $resolved.Value.version -ne $required.Value) {
        $unresolvedRequiredLockEntries.Add($required.Key)
    }
}

if ($unresolvedRequiredLockEntries.Count -gt 0) {
    $entryList = $unresolvedRequiredLockEntries -join ', '
    if ($Mode -eq 'Release') {
        Add-Failure "Release mode requires resolved packages-lock entries for: $entryList."
    }
    else {
        Write-Host "Repository mode: packages-lock is not yet resolved for required entries: $entryList. Release mode will fail until Unity resolves them."
    }
}

$runtimeAsmdef = Read-JsonFile 'Assets/Scripts/Runtime/CurioClerk.Runtime.asmdef'
if ($null -ne $runtimeAsmdef) {
    $allowedPrecompiledReferences = @(
        'GoogleMobileAds.dll',
        'GoogleMobileAds.Core.dll',
        'GoogleMobileAds.Ump.dll'
    )
    $actualPrecompiledReferences = @($runtimeAsmdef.precompiledReferences)
    foreach ($reference in $actualPrecompiledReferences) {
        if ($reference -like 'Firebase.*') {
            Add-Failure "Firebase precompiled reference remains in CurioClerk.Runtime.asmdef: $reference"
        }
        elseif ($reference -notin $allowedPrecompiledReferences) {
            Add-Failure "Unexpected runtime precompiled reference could add a remote transport: $reference"
        }
    }

    foreach ($requiredReference in $allowedPrecompiledReferences) {
        if ($requiredReference -notin $actualPrecompiledReferences) {
            Add-Failure "Required GMA/UMP runtime reference is missing: $requiredReference"
        }
    }
}

$edmArchivePath = Join-Path $projectRootPath 'GooglePackages/com.google.external-dependency-manager-1.2.188.tgz'
if (-not (Test-Path -LiteralPath $edmArchivePath -PathType Leaf)) {
    Add-Failure 'Required EDM4U 1.2.188 archive is missing.'
}

$vendoredRoot = Join-Path $projectRootPath 'GooglePackages'
if (Test-Path -LiteralPath $vendoredRoot -PathType Container) {
    foreach ($archive in Get-ChildItem -LiteralPath $vendoredRoot -Recurse -File -Filter 'com.google.firebase*.tgz') {
        Add-Failure "Vendored Firebase package remains: $(Get-NormalizedRelativePath $archive.FullName)"
    }
}

$allowedTelemetryRuntimeFiles = @(
    'Assets/Scripts/Runtime/Infrastructure/Analytics/IAnalyticsService.cs',
    'Assets/Scripts/Runtime/Infrastructure/Analytics/IAnalyticsService.cs.meta',
    'Assets/Scripts/Runtime/Infrastructure/Analytics/ConsentAwareAnalyticsService.cs',
    'Assets/Scripts/Runtime/Infrastructure/Analytics/ConsentAwareAnalyticsService.cs.meta',
    'Assets/Scripts/Runtime/Infrastructure/Analytics/AnalyticsEvents.cs',
    'Assets/Scripts/Runtime/Infrastructure/Analytics/AnalyticsEvents.cs.meta',
    'Assets/Scripts/Runtime/Infrastructure/Analytics/GameTelemetry.cs',
    'Assets/Scripts/Runtime/Infrastructure/Analytics/GameTelemetry.cs.meta',
    'Assets/Scripts/Runtime/Infrastructure/Diagnostics/ICrashReporter.cs',
    'Assets/Scripts/Runtime/Infrastructure/Diagnostics/ICrashReporter.cs.meta',
    'Assets/Scripts/Runtime/Infrastructure/Diagnostics/ConsentAwareCrashReporter.cs',
    'Assets/Scripts/Runtime/Infrastructure/Diagnostics/ConsentAwareCrashReporter.cs.meta'
)
$telemetryRuntimeRoots = @(
    'Assets/Scripts/Runtime/Infrastructure/Analytics',
    'Assets/Scripts/Runtime/Infrastructure/Diagnostics'
)

# These patterns apply specifically to the approved Analytics/Diagnostics sources.
# They match executable persistence/logging APIs, not harmless interface declarations
# such as ICrashReporter.Log(string).
$forbiddenPayloadPersistenceMarkers = @(
    [pscustomobject]@{ Name = 'System.IO'; Pattern = '\bSystem\.IO(?:\.|\s*;)' },
    [pscustomobject]@{ Name = 'File/Directory API'; Pattern = '\b(?:File|Directory)\s*\.' },
    [pscustomobject]@{ Name = 'file/stream/writer API'; Pattern = '\b(?:FileInfo|DirectoryInfo|IsolatedStorageFile|FileStream|MemoryStream|BufferedStream|StreamWriter|StreamReader|BinaryWriter|BinaryReader|TextWriter|TextReader)\b' },
    [pscustomobject]@{ Name = 'persistentDataPath'; Pattern = '\b(?:UnityEngine\s*\.\s*)?Application\s*\.\s*persistentDataPath\b' },
    [pscustomobject]@{ Name = 'PlayerPrefs'; Pattern = '\bPlayerPrefs\s*\.' },
    [pscustomobject]@{ Name = 'JsonUtility storage serialization'; Pattern = '\bJsonUtility\s*\.\s*(?:ToJson|FromJson|FromJsonOverwrite)\s*\(' },
    [pscustomobject]@{ Name = 'JSON serializer'; Pattern = '\b(?:JsonConvert|JsonSerializer)\s*\.\s*(?:Serialize|SerializeObject|Deserialize|DeserializeObject)\s*\(' },
    [pscustomobject]@{ Name = 'Debug logging'; Pattern = '\b(?:UnityEngine\s*\.\s*)?Debug\s*\.\s*(?:Log|LogWarning|LogError|LogException|LogFormat|LogAssertion)\s*\(' },
    [pscustomobject]@{ Name = 'Console logging'; Pattern = '\b(?:System\s*\.\s*)?Console(?:\s*\.\s*(?:Out|Error))?\s*\.\s*Write(?:Line)?\s*\(' },
    [pscustomobject]@{ Name = 'trace logging'; Pattern = '\b(?:Trace|System\s*\.\s*Diagnostics\s*\.\s*Debug)\s*\.\s*(?:Write|WriteLine|TraceInformation|TraceWarning|TraceError)\s*\(' },
    [pscustomobject]@{ Name = 'logger call'; Pattern = '\.\s*(?:Log|LogWarning|LogError|LogException)\s*\(' }
)

foreach ($allowedPath in $allowedTelemetryRuntimeFiles) {
    if (-not (Test-Path -LiteralPath (Join-Path $projectRootPath $allowedPath) -PathType Leaf)) {
        Add-Failure "Approved v1 Analytics/Diagnostics runtime file is missing: $allowedPath"
    }
}

foreach ($relativeRoot in $telemetryRuntimeRoots) {
    $root = Join-Path $projectRootPath $relativeRoot
    if (-not (Test-Path -LiteralPath $root -PathType Container)) {
        Add-Failure "Approved v1 runtime directory is missing: $relativeRoot"
        continue
    }

    foreach ($file in Get-ChildItem -LiteralPath $root -Recurse -File) {
        $relative = Get-NormalizedRelativePath $file.FullName
        if ($relative -notin $allowedTelemetryRuntimeFiles) {
            Add-Failure "Unreviewed Analytics/Diagnostics runtime file: $relative"
        }

        if ($file.Extension -eq '.cs') {
            $codeOnly = Remove-CSharpCommentsAndLiterals (Get-Content -LiteralPath $file.FullName -Raw)
            foreach ($marker in $forbiddenPayloadPersistenceMarkers) {
                if ($codeOnly -match $marker.Pattern) {
                    Add-Failure "Persistence/logging marker '$($marker.Name)' in approved Analytics/Diagnostics runtime source: $relative"
                }
            }
        }
    }
}

$forbiddenRuntimePaths = @(
    'Assets/Scripts/Runtime/Infrastructure/Firebase.meta',
    'Assets/Scripts/Runtime/Infrastructure/Firebase'
)
foreach ($relativePath in $forbiddenRuntimePaths) {
    if (Test-Path -LiteralPath (Join-Path $projectRootPath $relativePath)) {
        Add-Failure "Firebase runtime adapter path remains: $relativePath"
    }
}

# These code-token patterns cover direct .NET/Unity transports plus common analytics and
# crash-reporting SDK entry points. GMA/UMP types are intentionally absent from this list;
# their reviewed adapters remain the only v1 network-capable service path.
$forbiddenRuntimeMarkers = @(
    [pscustomobject]@{ Name = 'UnityWebRequest'; Pattern = '\bUnityWebRequest\b' },
    [pscustomobject]@{ Name = 'System.Net'; Pattern = '\bSystem\.Net(?:\.|\s*;)' },
    [pscustomobject]@{ Name = 'HttpClient'; Pattern = '\bHttpClient\b' },
    [pscustomobject]@{ Name = 'WebRequest/WebClient'; Pattern = '\b(?:WebRequest|WebClient)\b' },
    [pscustomobject]@{ Name = 'socket transport'; Pattern = '\b(?:Socket|TcpClient|UdpClient|ClientWebSocket)\b' },
    [pscustomobject]@{ Name = 'Firebase/Crashlytics'; Pattern = '\b(?:FirebaseApp|FirebaseAnalytics|Firebase|Crashlytics)\b' },
    [pscustomobject]@{ Name = 'Sentry'; Pattern = '\bSentry(?:Sdk)?\b' },
    [pscustomobject]@{ Name = 'Application Insights'; Pattern = '\b(?:TelemetryClient|ApplicationInsights)\b' },
    [pscustomobject]@{ Name = 'App Center'; Pattern = '\bAppCenter\b' },
    [pscustomobject]@{ Name = 'Bugsnag/Datadog/New Relic'; Pattern = '\b(?:Bugsnag|Datadog|NewRelic)\b' },
    [pscustomobject]@{ Name = 'analytics SDK'; Pattern = '\b(?:Amplitude(?:\.|Client\b|Analytics\b)|Mixpanel\b|GameAnalytics\b)' },
    [pscustomobject]@{ Name = 'Unity Analytics'; Pattern = '\bUnity(?:Engine|\.Services)\.Analytics\b' },
    [pscustomobject]@{ Name = 'crash SDK'; Pattern = '\b(?:CrashReportHandler|Backtrace|Raygun)\b' }
)

$runtimeRoot = Join-Path $projectRootPath 'Assets/Scripts/Runtime'
if (-not (Test-Path -LiteralPath $runtimeRoot -PathType Container)) {
    Add-Failure 'Missing first-party runtime source root: Assets/Scripts/Runtime'
}
else {
    foreach ($source in Get-ChildItem -LiteralPath $runtimeRoot -Recurse -File -Filter '*.cs') {
        $codeOnly = Remove-CSharpCommentsAndLiterals (Get-Content -LiteralPath $source.FullName -Raw)
        foreach ($marker in $forbiddenRuntimeMarkers) {
            if ($codeOnly -match $marker.Pattern) {
                $relative = Get-NormalizedRelativePath $source.FullName
                Add-Failure "Direct transport or telemetry SDK marker '$($marker.Name)' in first-party runtime source: $relative"
            }
        }
    }
}

$serviceFactoryPath = Join-Path $projectRootPath 'Assets/Scripts/Runtime/Infrastructure/ServiceFactory.cs'
if (-not (Test-Path -LiteralPath $serviceFactoryPath -PathType Leaf)) {
    Add-Failure 'Missing ServiceFactory.cs.'
}
else {
    $factoryCode = Remove-CSharpCommentsAndLiterals (Get-Content -LiteralPath $serviceFactoryPath -Raw)
    $analyticsFactoryPattern = 'public\s+static\s+IAnalyticsService\s+CreateAnalyticsService\s*\(\s*\)\s*=>\s*new\s+ConsentAwareAnalyticsService\s*\(\s*\)\s*;'
    $crashFactoryPattern = 'public\s+static\s+ICrashReporter\s+CreateCrashReporter\s*\(\s*\)\s*=>\s*new\s+ConsentAwareCrashReporter\s*\(\s*\)\s*;'
    if ([regex]::Matches($factoryCode, '\bCreateAnalyticsService\s*\(').Count -ne 1 -or
        $factoryCode -notmatch $analyticsFactoryPattern) {
        Add-Failure 'ServiceFactory analytics construction is not the approved local implementation; remove target conditionals and return ConsentAwareAnalyticsService directly.'
    }
    if ([regex]::Matches($factoryCode, '\bCreateCrashReporter\s*\(').Count -ne 1 -or
        $factoryCode -notmatch $crashFactoryPattern) {
        Add-Failure 'ServiceFactory crash construction is not the approved local implementation; remove target conditionals and return ConsentAwareCrashReporter directly.'
    }
}

$assetsRoot = Join-Path $projectRootPath 'Assets'
$forbiddenManifestValuePattern = '(?i)(firebase|crashlytics|analytics|telemetry|appcenter|sentry|bugsnag|datadog|newrelic|amplitude|mixpanel|gameanalytics|crash[_\.-]?(?:report|collection|upload|handler))'
if (Test-Path -LiteralPath $assetsRoot -PathType Container) {
    foreach ($manifestFile in Get-ChildItem -LiteralPath $assetsRoot -Recurse -File -Filter 'AndroidManifest.xml') {
        try {
            [xml]$androidManifest = Get-Content -LiteralPath $manifestFile.FullName -Raw
            $forbiddenValues = [System.Collections.Generic.List[string]]::new()
            foreach ($node in @($androidManifest.SelectNodes('//*'))) {
                foreach ($attribute in @($node.Attributes)) {
                    if ($attribute.Value -match $forbiddenManifestValuePattern) {
                        $forbiddenValues.Add($attribute.Value)
                    }
                }
            }

            if ($forbiddenValues.Count -gt 0) {
                $relative = Get-NormalizedRelativePath $manifestFile.FullName
                Add-Failure "Forbidden analytics/crash component or metadata in Android manifest: $relative ($($forbiddenValues -join ', '))"
            }
        }
        catch {
            $relative = Get-NormalizedRelativePath $manifestFile.FullName
            Add-Failure "Invalid Android manifest XML in ${relative}: $($_.Exception.Message)"
        }
    }
}

if ($failures.Count -gt 0) {
    foreach ($failure in $failures) {
        Write-Error $failure -ErrorAction Continue
    }

    throw "No-remote-telemetry gate failed with $($failures.Count) violation(s)."
}

Write-Host 'No-remote-telemetry gate passed.'
