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
    $output = @(& $pwshPath -NoProfile -File $gatePath -ProjectRoot $fixtureRoot 2>&1)
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

    $edmArchive = Join-Path $fixtureRoot 'GooglePackages/com.google.external-dependency-manager-1.2.188.tgz'
    [System.IO.Directory]::CreateDirectory((Split-Path -Parent $edmArchive)) | Out-Null
    [System.IO.File]::WriteAllBytes($edmArchive, [byte[]]::new(0))

    $baseline = Invoke-Gate
    if ($baseline.ExitCode -ne 0) {
        throw "Baseline fixture should pass the gate, but failed:`n$($baseline.Output)"
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
