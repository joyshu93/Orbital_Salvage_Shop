$ErrorActionPreference = 'Stop'

$gatePath = Join-Path $PSScriptRoot 'check-no-remote-telemetry.ps1'
$fixtureRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("curio-no-telemetry-gate-$([guid]::NewGuid().ToString('N'))")
$pwshPath = (Get-Process -Id $PID).Path

function Write-FixtureFile {
    param(
        [string]$RelativePath,
        [string]$Content
    )

    $path = Join-Path $fixtureRoot $RelativePath
    $directory = Split-Path -Parent $path
    [System.IO.Directory]::CreateDirectory($directory) | Out-Null
    [System.IO.File]::WriteAllText($path, $Content)
}

function Invoke-Gate {
    param([string]$Mode = 'Repository')

    $output = @(& $pwshPath -NoProfile -File $gatePath -ProjectRoot $fixtureRoot -Mode $Mode 2>&1)
    return [pscustomobject]@{
        ExitCode = $LASTEXITCODE
        Output = $output -join "`n"
    }
}

try {
    Write-FixtureFile 'Packages/manifest.json' @'
{
  "dependencies": {
    "com.google.ads.mobile": "11.3.0",
    "com.google.external-dependency-manager": "file:../GooglePackages/com.google.external-dependency-manager-1.2.188.tgz"
  }
}
'@
    Write-FixtureFile 'Packages/packages-lock.json' '{ "dependencies": {} }'
    Write-FixtureFile 'Assets/Scripts/Runtime/CurioClerk.Runtime.asmdef' @'
{
  "name": "CurioClerk.Runtime",
  "precompiledReferences": [
    "GoogleMobileAds.dll",
    "GoogleMobileAds.Core.dll",
    "GoogleMobileAds.Ump.dll"
  ]
}
'@
    Write-FixtureFile 'Assets/Scripts/Runtime/Infrastructure/ServiceFactory.cs' @'
namespace CurioClerk.Infrastructure
{
    public static class ServiceFactory
    {
        public static IAnalyticsService CreateAnalyticsService() => new ConsentAwareAnalyticsService();
        public static ICrashReporter CreateCrashReporter() => new ConsentAwareCrashReporter();
    }
}
'@

    $allowedTelemetryFiles = @(
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
    foreach ($relativePath in $allowedTelemetryFiles) {
        Write-FixtureFile $relativePath '// approved fixture file'
    }
    Write-FixtureFile 'Assets/Scripts/Runtime/Infrastructure/Analytics/IAnalyticsService.cs' @'
// Documentation may name HttpClient, Firebase, or Sentry without adding executable code.
internal static class MarkerDocumentationFixture
{
    private const string ReviewNote = "UnityWebRequest is forbidden for gameplay telemetry.";
}
'@
    Write-FixtureFile 'Assets/Scripts/Runtime/Infrastructure/Feedback/ProceduralTone.cs' @'
internal static class ProceduralTone
{
    public static float Scale(float amplitude, float envelope)
    {
        return amplitude * envelope;
    }
}
'@

    $edmArchive = Join-Path $fixtureRoot 'GooglePackages/com.google.external-dependency-manager-1.2.188.tgz'
    [System.IO.Directory]::CreateDirectory((Split-Path -Parent $edmArchive)) | Out-Null
    [System.IO.File]::WriteAllBytes($edmArchive, [byte[]]::new(0))

    $baseline = Invoke-Gate
    if ($baseline.ExitCode -ne 0) {
        throw "Baseline fixture should pass the gate, but failed:`n$($baseline.Output)"
    }

    $manifestPath = Join-Path $fixtureRoot 'Packages/manifest.json'
    $originalManifestBytes = [System.IO.File]::ReadAllBytes($manifestPath)
    try {
        Write-FixtureFile 'Packages/manifest.json' '{ "scopedRegistries": [] }'
        $missingDependencyObject = Invoke-Gate
        if ($missingDependencyObject.ExitCode -eq 0 -or
            -not $missingDependencyObject.Output.Contains('Packages/manifest.json must contain a valid dependencies object.')) {
            throw "Missing manifest dependencies object was not rejected explicitly:`n$($missingDependencyObject.Output)"
        }

        Write-FixtureFile 'Packages/manifest.json' @'
{
  "dependencies": {
    "com.google.external-dependency-manager": "file:../GooglePackages/com.google.external-dependency-manager-1.2.188.tgz"
  }
}
'@
        $missingRequiredDependency = Invoke-Gate
        if ($missingRequiredDependency.ExitCode -eq 0 -or
            -not $missingRequiredDependency.Output.Contains('Required v1 dependency is missing or changed: com.google.ads.mobile must be 11.3.0')) {
            throw "Missing required GMA dependency was not rejected explicitly:`n$($missingRequiredDependency.Output)"
        }
    }
    finally {
        [System.IO.File]::WriteAllBytes($manifestPath, $originalManifestBytes)
    }

    $unresolvedRelease = Invoke-Gate -Mode 'Release'
    if ($unresolvedRelease.ExitCode -eq 0 -or
        -not $unresolvedRelease.Output.Contains('Release mode requires resolved packages-lock entries for: com.google.ads.mobile, com.google.external-dependency-manager.')) {
        throw "Release mode did not reject the unresolved packages-lock graph explicitly:`n$($unresolvedRelease.Output)"
    }

    $packageLockPath = Join-Path $fixtureRoot 'Packages/packages-lock.json'
    $originalPackageLockBytes = [System.IO.File]::ReadAllBytes($packageLockPath)
    try {
        Write-FixtureFile 'Packages/packages-lock.json' @'
{
  "dependencies": {
    "com.google.ads.mobile": {
      "version": "11.3.0"
    },
    "com.google.external-dependency-manager": {
      "version": "file:../GooglePackages/com.google.external-dependency-manager-1.2.188.tgz"
    }
  }
}
'@
        $resolvedRelease = Invoke-Gate -Mode 'Release'
        if ($resolvedRelease.ExitCode -ne 0) {
            throw "Release mode rejected an exact resolved GMA/EDM graph:`n$($resolvedRelease.Output)"
        }
    }
    finally {
        [System.IO.File]::WriteAllBytes($packageLockPath, $originalPackageLockBytes)
    }

    $localAnalyticsPath = Join-Path $fixtureRoot 'Assets/Scripts/Runtime/Infrastructure/Analytics/ConsentAwareAnalyticsService.cs'
    $originalLocalAnalyticsBytes = [System.IO.File]::ReadAllBytes($localAnalyticsPath)
    try {
        Write-FixtureFile 'Assets/Scripts/Runtime/Infrastructure/Analytics/ConsentAwareAnalyticsService.cs' @'
using System.IO;
internal sealed class ConsentAwareAnalyticsService
{
    public void Track(string payload)
    {
        File.AppendAllText("telemetry.log", payload);
    }
}
'@
        $persistenceMutation = Invoke-Gate
        if ($persistenceMutation.ExitCode -eq 0 -or
            -not $persistenceMutation.Output.Contains("Persistence/logging marker 'System.IO' in approved Analytics/Diagnostics runtime source")) {
            throw "Approved local service persistence mutation was not rejected explicitly:`n$($persistenceMutation.Output)"
        }
    }
    finally {
        [System.IO.File]::WriteAllBytes($localAnalyticsPath, $originalLocalAnalyticsBytes)
    }
    $restoredLocalAnalyticsBytes = [System.IO.File]::ReadAllBytes($localAnalyticsPath)
    if ([Convert]::ToBase64String($restoredLocalAnalyticsBytes) -ne [Convert]::ToBase64String($originalLocalAnalyticsBytes)) {
        throw 'Approved local service fixture was not restored byte-for-byte after the persistence mutation.'
    }

    Write-FixtureFile 'Assets/Scripts/Runtime/Infrastructure/Analytics/AndroidTelemetryService.cs' @'
#if UNITY_ANDROID
using System.Net.Http;
internal sealed class AndroidTelemetryService
{
    private readonly HttpClient client = new HttpClient();
}
#endif
'@
    Write-FixtureFile 'Assets/Scripts/Runtime/Infrastructure/ServiceFactory.cs' @'
namespace CurioClerk.Infrastructure
{
    public static class ServiceFactory
    {
        public static IAnalyticsService CreateAnalyticsService()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidTelemetryService();
#else
            return new ConsentAwareAnalyticsService();
#endif
        }

        public static ICrashReporter CreateCrashReporter() => new ConsentAwareCrashReporter();
    }
}
'@
    Write-FixtureFile 'Assets/Plugins/Telemetry/AndroidManifest.xml' @'
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
  <application>
    <service android:name="com.example.telemetry.AnalyticsUploadService" />
    <meta-data android:name="analytics_collection_enabled" android:value="true" />
  </application>
</manifest>
'@

    $mutation = Invoke-Gate
    if ($mutation.ExitCode -eq 0) {
        throw 'Android-only remote telemetry mutation unexpectedly passed the gate.'
    }

    foreach ($expectedFailure in @(
        'Unreviewed Analytics/Diagnostics runtime file',
        'Direct transport or telemetry SDK marker',
        'ServiceFactory analytics construction is not the approved local implementation',
        'Forbidden analytics/crash component or metadata in Android manifest'
    )) {
        if (-not $mutation.Output.Contains($expectedFailure)) {
            throw "Mutation failed, but did not report '$expectedFailure':`n$($mutation.Output)"
        }
    }

    Write-Host 'No-remote-telemetry controlled mutation test passed.'
}
finally {
    $resolvedTemp = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
    $resolvedFixture = [System.IO.Path]::GetFullPath($fixtureRoot)
    if ($resolvedFixture.StartsWith($resolvedTemp, [System.StringComparison]::OrdinalIgnoreCase) -and
        [System.IO.Path]::GetFileName($resolvedFixture).StartsWith('curio-no-telemetry-gate-', [System.StringComparison]::Ordinal)) {
        Remove-Item -LiteralPath $resolvedFixture -Recurse -Force -ErrorAction SilentlyContinue
    }
}
