$ErrorActionPreference = 'Stop'

$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$projectBuilderPath = Join-Path $projectRoot 'Assets\Scripts\Editor\ProjectBuilder.cs'
$releaseConfigurationPath = Join-Path $projectRoot 'Assets\Scripts\Editor\ReleaseConfiguration.cs'
$releaseManifestPath = Join-Path $projectRoot 'Assets\Scripts\Editor\ReleaseBuildManifest.cs'
$serviceFactoryPath = Join-Path $projectRoot 'Assets\Scripts\Runtime\Infrastructure\ServiceFactory.cs'
$buildWrapperPath = Join-Path $projectRoot 'scripts\build-android.ps1'
$inspectScriptPath = Join-Path $projectRoot 'scripts\inspect-aab.ps1'
$telemetryGatePath = Join-Path $projectRoot 'scripts\check-no-remote-telemetry.ps1'

function Assert-Contract {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

$windowsPowerShell = Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::System)) `
    'WindowsPowerShell\v1.0\powershell.exe'
Assert-Contract (Test-Path -LiteralPath $windowsPowerShell -PathType Leaf) `
    'The pinned Windows PowerShell host required by the direct Unity release gate is missing.'
& $windowsPowerShell -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $telemetryGatePath `
    -ProjectRoot $projectRoot -Mode Repository | Out-Null
Assert-Contract ($LASTEXITCODE -eq 0) `
    'The no-remote gate must execute successfully through the pinned Windows PowerShell host.'

$releaseConfiguration = Get-Content -LiteralPath $releaseConfigurationPath -Raw
Assert-Contract ($releaseConfiguration -match 'public\s+const\s+string\s+UnityVersion\s*=\s*"6000\.3\.21f1"\s*;') `
    'ReleaseConfiguration must pin Unity 6000.3.21f1 in one public constant.'

$projectBuilder = Get-Content -LiteralPath $projectBuilderPath -Raw
$unityVersionGateIndex = $projectBuilder.IndexOf('ValidateUnityVersion();', [StringComparison]::Ordinal)
$telemetryGateIndex = $projectBuilder.IndexOf('RunReleaseNoRemoteTelemetryGate();', [StringComparison]::Ordinal)
$environmentReadIndex = $projectBuilder.IndexOf('ReadAndValidateReleaseEnvironment();', [StringComparison]::Ordinal)
Assert-Contract ($unityVersionGateIndex -ge 0 -and
    $telemetryGateIndex -gt $unityVersionGateIndex -and
    $environmentReadIndex -gt $telemetryGateIndex) `
    'BuildAndroid must validate Unity and run the Release telemetry gate before reading release environment values.'
Assert-Contract ($projectBuilder -match 'ProcessStartInfo' -and
    $projectBuilder -match 'CreateNoWindow\s*=\s*true' -and
    $projectBuilder -match 'UseShellExecute\s*=\s*false' -and
    $projectBuilder -match '-Mode Release') `
    'The direct release gate must use a hidden, non-shell child process in Release mode.'
Assert-Contract ($projectBuilder -match 'startInfo\.EnvironmentVariables\.Remove\(environmentName\)') `
    'The release privacy-gate child process must not inherit release IDs or signing secrets.'
Assert-Contract ($projectBuilder -match '(?s)ClearReleaseSecrets\(ReleaseEnvironment environment\).*?if \(environment == null\).*?return;') `
    'A Unity-version or privacy-gate failure must not mutate signing settings before release state exists.'

$releaseManifest = Get-Content -LiteralPath $releaseManifestPath -Raw
Assert-Contract ($releaseManifest -match 'unityVersion\s*=\s*ReleaseConfiguration\.UnityVersion') `
    'The public build manifest must use the single pinned Unity version constant.'
$expectedManifestFieldNames = @(
    'product', 'packageId', 'versionName', 'versionCode', 'unityVersion',
    'minimumApi', 'targetApi', 'architecture', 'backend', 'aabSha256'
)
$actualManifestFieldNames = @([Regex]::Matches($releaseManifest, 'public\s+(?:string|int)\s+([a-zA-Z0-9_]+)\s*;') |
    ForEach-Object { $_.Groups[1].Value })
Assert-Contract ($actualManifestFieldNames.Count -eq $expectedManifestFieldNames.Count -and
    @(Compare-Object -ReferenceObject $expectedManifestFieldNames -DifferenceObject $actualManifestFieldNames).Count -eq 0) `
    'ReleaseBuildManifest must serialize exactly the ten approved public fields.'

$serviceFactory = Get-Content -LiteralPath $serviceFactoryPath -Raw
Assert-Contract ($serviceFactory -match '(?s)#if CURIO_OFFLINE_QA.*?new UnavailableAdService\(\).*?#elif UNITY_ANDROID && !UNITY_EDITOR && DEVELOPMENT_BUILD.*?new GoogleRewardedAdService\(AndroidRewardedTestUnitId\).*?#elif UNITY_ANDROID && !UNITY_EDITOR.*?Resources\.Load<ServiceConfiguration>\("ServiceConfiguration"\).*?new GoogleRewardedAdService\(rewardedId\).*?#else.*?new UnavailableAdService\(\)') `
    'ServiceFactory must preserve offline QA, development sample, validated live Android release, and unavailable fallback routing.'
Assert-Contract ($serviceFactory -match 'string\.Equals\(rewardedId, AndroidRewardedTestUnitId, StringComparison\.Ordinal\)') `
    'The Android release route must reject the Google sample rewarded unit.'

$buildWrapper = Get-Content -LiteralPath $buildWrapperPath -Raw
foreach ($contract in @(
    [pscustomobject]@{ Field = 'product'; Value = 'Curio Clerk: Night Shift' },
    [pscustomobject]@{ Field = 'packageId'; Value = 'com.joyshu93.curioclerknightshift' },
    [pscustomobject]@{ Field = 'versionName'; Value = '1.0.0' },
    [pscustomobject]@{ Field = 'versionCode'; Value = '10000' },
    [pscustomobject]@{ Field = 'unityVersion'; Value = '6000.3.21f1' },
    [pscustomobject]@{ Field = 'minimumApi'; Value = '29' },
    [pscustomobject]@{ Field = 'targetApi'; Value = '36' },
    [pscustomobject]@{ Field = 'architecture'; Value = 'ARM64' },
    [pscustomobject]@{ Field = 'backend'; Value = 'IL2CPP' }
)) {
    Assert-Contract ($buildWrapper.Contains("'$($contract.Field)' = '$($contract.Value)'")) `
        "The build wrapper must validate manifest field $($contract.Field)."
}

$fixtureRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("curio-release-build-contract-$([guid]::NewGuid().ToString('N'))")
$fixtureScripts = Join-Path $fixtureRoot 'scripts'
$fixtureTools = Join-Path $fixtureRoot 'tools\bundletool'
$fixtureBuilds = Join-Path $fixtureRoot 'Builds\Android'
$fixtureBin = Join-Path $fixtureRoot 'bin'
$inspectionPath = Join-Path $fixtureBuilds 'inspection.txt'
$copiedInspectScript = Join-Path $fixtureScripts 'inspect-aab.ps1'
$fakeJarPath = Join-Path $fixtureTools 'bundletool-all-1.18.3.jar'
$fakeAabPath = Join-Path $fixtureBuilds 'CurioClerk.aab'
$fakeJavaPath = Join-Path $fixtureBin 'java.cmd'
$pwshPath = (Get-Process -Id $PID).Path
$originalPath = $env:PATH

try {
    foreach ($directory in @($fixtureScripts, $fixtureTools, $fixtureBuilds, $fixtureBin)) {
        [System.IO.Directory]::CreateDirectory($directory) | Out-Null
    }

    [System.IO.File]::Copy($inspectScriptPath, $copiedInspectScript, $true)
    [System.IO.File]::WriteAllBytes($fakeJarPath, [byte[]](1))

    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [System.IO.Compression.ZipFile]::Open($fakeAabPath, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        $entry = $archive.CreateEntry('base/lib/arm64-v8a/libil2cpp.so')
        $stream = $entry.Open()
        try {
            $stream.WriteByte(1)
        }
        finally {
            $stream.Dispose()
        }
    }
    finally {
        $archive.Dispose()
    }

    [System.IO.File]::WriteAllText($fakeJavaPath, @'
@echo off
if /I "%~3"=="version" (
  echo %CURIO_FAKE_BUNDLETOOL_VERSION%
  exit /b 0
)
if /I "%~3"=="validate" (
  echo %CURIO_FAKE_AAB_BACKSLASH%
  echo %CURIO_FAKE_AAB_FORWARD%
  echo %CURIO_FAKE_AAB_URI%
  echo %CURIO_FAKE_JAR_BACKSLASH%
  echo %CURIO_FAKE_JAR_FORWARD%
  echo %CURIO_FAKE_JAR_URI%
  echo %CURIO_FAKE_PROJECT_BACKSLASH%
  echo %CURIO_FAKE_PROJECT_FORWARD%
  echo %CURIO_FAKE_PROJECT_URI%
  echo %CURIO_FAKE_PROFILE_BACKSLASH%
  echo %CURIO_FAKE_PROFILE_FORWARD%
  echo %CURIO_FAKE_PROFILE_URI%
  echo ca-app-pub-1234567890123456~1234567890
  exit /b %CURIO_FAKE_VALIDATE_EXIT%
)
if /I "%~3"=="dump" (
  echo ^<manifest package="com.joyshu93.curioclerknightshift" android:versionName="1.0.0" android:versionCode="10000"^>
  echo ^<uses-sdk android:minSdkVersion="29" android:targetSdkVersion="36" /^>
  exit /b 0
)
exit /b 2
'@)

    $env:PATH = "$fixtureBin;$originalPath"
    $env:CURIO_FAKE_AAB_BACKSLASH = $fakeAabPath
    $env:CURIO_FAKE_AAB_FORWARD = $fakeAabPath.Replace('\', '/')
    $env:CURIO_FAKE_AAB_URI = ([Uri]$fakeAabPath).AbsoluteUri
    $env:CURIO_FAKE_JAR_BACKSLASH = $fakeJarPath
    $env:CURIO_FAKE_JAR_FORWARD = $fakeJarPath.Replace('\', '/')
    $env:CURIO_FAKE_JAR_URI = ([Uri]$fakeJarPath).AbsoluteUri
    $env:CURIO_FAKE_PROJECT_BACKSLASH = $fixtureRoot
    $env:CURIO_FAKE_PROJECT_FORWARD = $fixtureRoot.Replace('\', '/')
    $env:CURIO_FAKE_PROJECT_URI = ([Uri]$fixtureRoot).AbsoluteUri
    $env:CURIO_FAKE_PROFILE_BACKSLASH = $env:USERPROFILE
    $env:CURIO_FAKE_PROFILE_FORWARD = $env:USERPROFILE.Replace('\', '/')
    $env:CURIO_FAKE_PROFILE_URI = ([Uri]$env:USERPROFILE).AbsoluteUri
    $env:CURIO_FAKE_VALIDATE_EXIT = '0'

    [System.IO.File]::WriteAllText($inspectionPath, 'stale PASS report')
    $env:CURIO_FAKE_BUNDLETOOL_VERSION = '1.18.2'
    & $pwshPath -NoProfile -File $copiedInspectScript -AabPath $fakeAabPath -BundletoolPath $fakeJarPath 2>&1 | Out-Null
    Assert-Contract ($LASTEXITCODE -ne 0) 'A wrong actual bundletool version must fail.'
    Assert-Contract (-not (Test-Path -LiteralPath $inspectionPath)) `
        'A wrong bundletool version must remove the stale report and leave no PASS report.'

    [System.IO.File]::WriteAllText($inspectionPath, 'stale PASS report')
    $env:CURIO_FAKE_BUNDLETOOL_VERSION = '1.18.3'
    $env:CURIO_FAKE_VALIDATE_EXIT = '1'
    & $pwshPath -NoProfile -File $copiedInspectScript -AabPath $fakeAabPath -BundletoolPath $fakeJarPath 2>&1 | Out-Null
    Assert-Contract ($LASTEXITCODE -ne 0) 'A bundletool validation failure must fail.'
    Assert-Contract (-not (Test-Path -LiteralPath $inspectionPath)) `
        'A validation failure must remove the stale report and leave no PASS report.'

    $env:CURIO_FAKE_VALIDATE_EXIT = '0'
    & $pwshPath -NoProfile -File $copiedInspectScript -AabPath $fakeAabPath -BundletoolPath $fakeJarPath 2>&1 | Out-Null
    Assert-Contract ($LASTEXITCODE -eq 0) 'The exact bundletool 1.18.3 fixture must pass.'
    Assert-Contract (Test-Path -LiteralPath $inspectionPath -PathType Leaf) `
        'A complete successful inspection must atomically publish a report.'
    $report = Get-Content -LiteralPath $inspectionPath -Raw
    Assert-Contract ($report.Contains('bundletool 1.18.3 validation: PASS')) `
        'The successful report must identify the verified bundletool version.'
    foreach ($forbiddenValue in @(
        $env:CURIO_FAKE_AAB_BACKSLASH, $env:CURIO_FAKE_AAB_FORWARD, $env:CURIO_FAKE_AAB_URI,
        $env:CURIO_FAKE_JAR_BACKSLASH, $env:CURIO_FAKE_JAR_FORWARD, $env:CURIO_FAKE_JAR_URI,
        $env:CURIO_FAKE_PROJECT_BACKSLASH, $env:CURIO_FAKE_PROJECT_FORWARD, $env:CURIO_FAKE_PROJECT_URI,
        $env:CURIO_FAKE_PROFILE_BACKSLASH, $env:CURIO_FAKE_PROFILE_FORWARD, $env:CURIO_FAKE_PROFILE_URI,
        'ca-app-pub-1234567890123456~1234567890'
    )) {
        Assert-Contract (-not $report.Contains($forbiddenValue)) `
            'The final report retained a known path or service identifier variant.'
    }

    Assert-Contract (@(Get-ChildItem -LiteralPath $fixtureBuilds -Filter 'inspection.txt.*.tmp' -File).Count -eq 0) `
        'A successful inspection must leave no temporary report.'
}
finally {
    $env:PATH = $originalPath
    foreach ($name in @(
        'CURIO_FAKE_BUNDLETOOL_VERSION', 'CURIO_FAKE_VALIDATE_EXIT',
        'CURIO_FAKE_AAB_BACKSLASH', 'CURIO_FAKE_AAB_FORWARD', 'CURIO_FAKE_AAB_URI',
        'CURIO_FAKE_JAR_BACKSLASH', 'CURIO_FAKE_JAR_FORWARD', 'CURIO_FAKE_JAR_URI',
        'CURIO_FAKE_PROJECT_BACKSLASH', 'CURIO_FAKE_PROJECT_FORWARD', 'CURIO_FAKE_PROJECT_URI',
        'CURIO_FAKE_PROFILE_BACKSLASH', 'CURIO_FAKE_PROFILE_FORWARD', 'CURIO_FAKE_PROFILE_URI'
    )) {
        [Environment]::SetEnvironmentVariable($name, $null)
    }

    if (Test-Path -LiteralPath $fixtureRoot) {
        $resolvedFixtureRoot = [System.IO.Path]::GetFullPath($fixtureRoot)
        $temporaryRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if (-not $temporaryRoot.EndsWith([System.IO.Path]::DirectorySeparatorChar.ToString(),
                [StringComparison]::Ordinal)) {
            $temporaryRoot += [System.IO.Path]::DirectorySeparatorChar
        }

        Assert-Contract ($resolvedFixtureRoot.StartsWith($temporaryRoot, [StringComparison]::OrdinalIgnoreCase) -and
            [System.IO.Path]::GetFileName($resolvedFixtureRoot).StartsWith('curio-release-build-contract-',
                [StringComparison]::Ordinal)) `
            'Refusing to remove a release-build fixture outside the expected temporary directory.'
        Remove-Item -LiteralPath $fixtureRoot -Recurse -Force
    }
}

Write-Host 'Release build controlled fixture and static contracts passed.'
