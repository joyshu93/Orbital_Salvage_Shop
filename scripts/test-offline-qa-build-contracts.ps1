$ErrorActionPreference = 'Stop'

$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$projectBuilderPath = Join-Path $projectRoot 'Assets\Scripts\Editor\ProjectBuilder.cs'
$serviceFactoryPath = Join-Path $projectRoot 'Assets\Scripts\Runtime\Infrastructure\ServiceFactory.cs'
$rewardedServicePath = Join-Path $projectRoot `
    'Assets\Scripts\Runtime\Infrastructure\Ads\GoogleRewardedAdService.cs'
$privacyServicePath = Join-Path $projectRoot `
    'Assets\Scripts\Runtime\Infrastructure\Privacy\GoogleUmpPrivacyService.cs'

function Assert-Contract {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

$projectBuilder = Get-Content -LiteralPath $projectBuilderPath -Raw
$serviceFactory = Get-Content -LiteralPath $serviceFactoryPath -Raw
$rewardedService = Get-Content -LiteralPath $rewardedServicePath -Raw
$privacyService = Get-Content -LiteralPath $privacyServicePath -Raw

Assert-Contract ($projectBuilder -match 'public\s+static\s+void\s+BuildAndroidOfflineQa\s*\(') `
    'ProjectBuilder must expose the offline QA APK entry point.'
Assert-Contract ($projectBuilder -match 'MenuItem\("Tools/Curio Clerk/Build Android Offline QA APK"\)') `
    'ProjectBuilder must expose the offline QA APK menu command.'
Assert-Contract ($projectBuilder -match 'CurioClerk-qa\.apk') `
    'The offline QA build must use the dedicated QA APK output.'
Assert-Contract ($projectBuilder -match 'CURIO_OFFLINE_QA') `
    'The offline QA build must define its player-only isolation symbol.'
Assert-Contract ($projectBuilder -match 'enableGradleBuildPreProcessor') `
    'The offline QA scope must disable the Google Mobile Ads synchronous Gradle preprocessor.'
Assert-Contract ($projectBuilder -match 'AutoResolveOnBuild') `
    'The offline QA scope must disable EDM4U resolve-on-build.'
Assert-Contract ($projectBuilder -match 'OfflineQaBuildStateScope') `
    'The offline QA build must restore temporary package and player settings.'
Assert-Contract ($projectBuilder -match 'GetOfflineQaGraphicsApis') `
    'The offline QA build must expose its compatibility graphics policy for validation.'
Assert-Contract ($projectBuilder -match 'GraphicsDeviceType\.OpenGLES3') `
    'The offline QA build must use OpenGL ES 3 for broad emulator compatibility.'
Assert-Contract ($projectBuilder -match '(?s)public\s+void\s+Dispose\s*\(\).*?LoadMainAssetAtPath\(GoogleMobileAdsSettingsPath\).*?WriteSerializedString') `
    'The offline QA cleanup must reload Google Mobile Ads settings after the player build invalidates cached Unity objects.'
Assert-Contract ($projectBuilder -match 'public\s+static\s+string\[\]\s+ResolveAndroidToolchainRoots\s*\(') `
    'ProjectBuilder must expose deterministic Android toolchain discovery for tests.'
Assert-Contract ($projectBuilder -match 'CURIO_ANDROID_SDK_ROOT') `
    'Android toolchain discovery must support complete explicit overrides.'
Assert-Contract ($projectBuilder -match 'UnityPersonal.*OpenJDK17') `
    'Android toolchain discovery must support the approved external OpenJDK 17 location.'
Assert-Contract ($serviceFactory -match 'CURIO_OFFLINE_QA') `
    'The service factory must route offline QA builds away from Google services.'
Assert-Contract ($rewardedService -match '!CURIO_OFFLINE_QA') `
    'The Google rewarded implementation must be excluded from offline QA player compilation.'
Assert-Contract ($privacyService -match '!CURIO_OFFLINE_QA') `
    'The Google UMP implementation must be excluded from offline QA player compilation.'

Write-Host 'Offline QA Android build static contracts passed.'
