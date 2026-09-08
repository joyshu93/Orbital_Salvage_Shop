$ErrorActionPreference = 'Stop'

$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$projectBuilderPath = Join-Path $projectRoot 'Assets\Scripts\Editor\ProjectBuilder.cs'
$runtimeAssemblyPath = Join-Path $projectRoot 'Assets\Scripts\Runtime\CurioClerk.Runtime.asmdef'
$buildWrapperPath = Join-Path $projectRoot 'scripts\build-android-dev.ps1'
$commandWrapperPath = Join-Path $projectRoot 'scripts\build-android-dev.cmd'

function Assert-Contract {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

Assert-Contract (Test-Path -LiteralPath $buildWrapperPath -PathType Leaf) `
    'The QA APK PowerShell wrapper is missing.'
Assert-Contract (Test-Path -LiteralPath $commandWrapperPath -PathType Leaf) `
    'The execution-policy-free QA APK command wrapper is missing.'

$runtimeAssembly = Get-Content -LiteralPath $runtimeAssemblyPath -Raw | ConvertFrom-Json
Assert-Contract ([bool]$runtimeAssembly.overrideReferences) `
    'The runtime assembly contract expects explicit precompiled references.'
Assert-Contract (@($runtimeAssembly.precompiledReferences) -contains 'GoogleMobileAds.Core.dll') `
    'The Android player assembly must reference the GMA Core assembly that owns AdRequest and Reward.'

$projectBuilder = Get-Content -LiteralPath $projectBuilderPath -Raw
Assert-Contract ($projectBuilder -match 'public\s+static\s+void\s+BuildAndroidDevelopment\s*\(') `
    'ProjectBuilder must expose the QA APK batch entry point.'
Assert-Contract ($projectBuilder -match 'MenuItem\("Tools/Curio Clerk/Build Android QA APK"\)') `
    'ProjectBuilder must expose the QA APK menu command.'
Assert-Contract ($projectBuilder -match 'CurioClerk-qa\.apk') `
    'The development build must use the dedicated QA APK output.'
Assert-Contract ($projectBuilder -match 'EditorUserBuildSettings\.buildAppBundle\s*=\s*false') `
    'The QA build must emit an APK instead of an app bundle.'
Assert-Contract ($projectBuilder -match 'BuildOptions\.Development') `
    'The QA build must define DEVELOPMENT_BUILD for the sample rewarded-ad route.'
Assert-Contract ($projectBuilder -match 'ConfigureServiceAssets\(GoogleSampleAppId, GoogleSampleRewardedId\)') `
    'The QA build must use Google sample ad identifiers.'
Assert-Contract ($projectBuilder -match '"cmake",\s*"3\.22\.1",\s*"bin",\s*"cmake\.exe"') `
    'The Android builder must reject an SDK that lacks Unity-required CMake 3.22.1.'
Assert-Contract ($projectBuilder -match 'LoadOrCreateGoogleMobileAdsSettings\(\)') `
    'The builder must create the ignored Google Mobile Ads settings asset when it is absent.'
Assert-Contract ($projectBuilder.Contains(
    'GoogleMobileAds.Editor.GoogleMobileAdsSettings, GoogleMobileAds.Editor')) `
    'The builder must resolve the pinned plugin settings type from its editor assembly.'
Assert-Contract ($projectBuilder -match 'GetMethod\("LoadInstance",\s*BindingFlags\.Public\s*\|\s*BindingFlags\.NonPublic\s*\|\s*BindingFlags\.Static\)') `
    'The builder must use the plugin-owned LoadInstance creation path.'
Assert-Contract ($projectBuilder -match '_hadMobileAdsSettings') `
    'The QA state scope must remember whether the ignored settings asset existed before the build.'
Assert-Contract ($projectBuilder -match '(?s)developmentStateScope\s*=\s*ConfigureAndroidDevelopmentState\(BuildAll\);.*?finally\s*\{.*?developmentStateScope\?\.Dispose\(\);') `
    'The QA build must restore temporary service, signing, and bundle settings.'

$buildWrapper = Get-Content -LiteralPath $buildWrapperPath -Raw
Assert-Contract ($buildWrapper -match 'check-android-toolchain\.ps1') `
    'The QA wrapper must run the Android toolchain preflight.'
Assert-Contract ($buildWrapper -match 'test-unity\.ps1') `
    'The one-command QA wrapper must run Unity tests before building.'
$testIndex = $buildWrapper.IndexOf('test-unity.ps1', [StringComparison]::Ordinal)
$buildIndex = $buildWrapper.IndexOf(
    'CurioClerk.Editor.ProjectBuilder.BuildAndroidDevelopment', [StringComparison]::Ordinal)
Assert-Contract ($testIndex -ge 0 -and $buildIndex -gt $testIndex) `
    'The one-command QA wrapper must run Unity tests before invoking the development build.'
Assert-Contract ($buildWrapper -match "CurioClerk\.Editor\.ProjectBuilder\.BuildAndroidDevelopment") `
    'The QA wrapper must invoke only the development build entry point.'
Assert-Contract ($buildWrapper -match 'CurioClerk-qa\.apk') `
    'The QA wrapper must verify the dedicated APK output.'
Assert-Contract (-not $buildWrapper.Contains('CURIO_ADMOB_')) `
    'The QA wrapper must not require or consume live AdMob identifiers.'
Assert-Contract (-not $buildWrapper.Contains('CURIO_ANDROID_KEYSTORE')) `
    'The QA wrapper must not require or consume release signing secrets.'

$commandWrapper = Get-Content -LiteralPath $commandWrapperPath -Raw
Assert-Contract ($commandWrapper -match 'ExecutionPolicy Bypass') `
    'The QA command wrapper must work under the managed PowerShell execution policy.'
Assert-Contract ($commandWrapper -match 'build-android-dev\.ps1') `
    'The QA command wrapper must invoke the QA PowerShell wrapper.'

Write-Host 'Development Android build static contracts passed.'
