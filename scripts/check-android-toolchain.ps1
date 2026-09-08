param(
    [string]$ProjectRoot,
    [string]$UnityPath = 'C:\Program Files\Unity 6000.3.21f1\Editor\Unity.exe',
    [string]$SdkRoot = $env:CURIO_ANDROID_SDK_ROOT,
    [string]$NdkRoot = $env:CURIO_ANDROID_NDK_ROOT,
    [string]$JdkRoot = $env:CURIO_ANDROID_JDK_ROOT,
    [string]$LocalApplicationDataRoot = $env:LOCALAPPDATA,
    [string]$UserProfileRoot = $env:USERPROFILE,
    [switch]$DisableExternalAutoDiscovery
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($ProjectRoot)) {
    $ProjectRoot = Join-Path $PSScriptRoot '..'
}

$expectedUnityVersion = '6000.3.21f1'
$projectRootPath = [System.IO.Path]::GetFullPath($ProjectRoot)
$unityPathValue = [System.IO.Path]::GetFullPath($UnityPath)
$editorRoot = Split-Path -Parent $unityPathValue
$androidRoot = Join-Path $editorRoot 'Data\PlaybackEngines\AndroidPlayer'
$failures = [System.Collections.Generic.List[string]]::new()

function Find-ExternalJdkRoot {
    $container = Join-Path $UserProfileRoot 'UnityPersonal\OpenJDK17'
    if (Test-Path -LiteralPath (Join-Path $container 'bin\java.exe') -PathType Leaf) {
        return $container
    }

    if (-not (Test-Path -LiteralPath $container -PathType Container)) {
        return $null
    }

    return Get-ChildItem -LiteralPath $container -Directory -ErrorAction SilentlyContinue |
        Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'bin\java.exe') -PathType Leaf } |
        Sort-Object Name -Descending |
        Select-Object -First 1 -ExpandProperty FullName
}

$bundledSdkRoot = Join-Path $androidRoot 'SDK'
$bundledNdkRoot = Join-Path $androidRoot 'NDK'
$bundledJdkRoot = Join-Path $androidRoot 'OpenJDK'

function Test-CompleteToolchain([string]$Sdk, [string]$Ndk, [string]$Jdk) {
    foreach ($marker in @(
        (Join-Path $Sdk 'platforms\android-36\android.jar'),
        (Join-Path $Sdk 'build-tools\36.0.0\aapt2.exe'),
        (Join-Path $Sdk 'platform-tools\adb.exe'),
        (Join-Path $Sdk 'cmdline-tools\16.0\bin\sdkmanager.bat'),
        (Join-Path $Sdk 'cmake\3.22.1\bin\cmake.exe'),
        (Join-Path $Ndk 'ndk-build.cmd'),
        (Join-Path $Jdk 'bin\java.exe')
    )) {
        if (-not (Test-Path -LiteralPath $marker -PathType Leaf)) { return $false }
    }
    return $true
}

$explicitRoots = @($SdkRoot, $NdkRoot, $JdkRoot) | Where-Object { -not [string]::IsNullOrWhiteSpace($_) }
if (@($explicitRoots).Count -gt 0 -and @($explicitRoots).Count -ne 3) {
    Write-Host 'Android build toolchain: BLOCKED'
    Write-Host '- Supply all three SDK, NDK and JDK overrides together.'
    [Environment]::Exit(2)
}
if (@($explicitRoots).Count -eq 0) {
    if ($DisableExternalAutoDiscovery -or
        (Test-CompleteToolchain $bundledSdkRoot $bundledNdkRoot $bundledJdkRoot)) {
        $SdkRoot = $bundledSdkRoot
        $NdkRoot = $bundledNdkRoot
        $JdkRoot = $bundledJdkRoot
    }
    else {
        if ([string]::IsNullOrWhiteSpace($LocalApplicationDataRoot)) {
            $LocalApplicationDataRoot = [Environment]::GetFolderPath([Environment+SpecialFolder]::LocalApplicationData)
        }
        if ([string]::IsNullOrWhiteSpace($UserProfileRoot)) {
            $UserProfileRoot = [Environment]::GetFolderPath([Environment+SpecialFolder]::UserProfile)
        }
        $SdkRoot = Join-Path $LocalApplicationDataRoot 'Android\Sdk'
        $NdkRoot = Join-Path $SdkRoot 'ndk\27.2.12479018'
        $JdkRoot = Find-ExternalJdkRoot
    }
}

function Write-ComponentStatus {
    param(
        [bool]$Ready,
        [string]$Label,
        [string]$FailureMessage
    )

    if ($Ready) {
        Write-Host "[READY] $Label"
        return
    }

    Write-Host "[MISSING] $Label"
    $failures.Add($FailureMessage)
}

$projectVersionPath = Join-Path $projectRootPath 'ProjectSettings\ProjectVersion.txt'
$projectVersionMatches = $false
if (Test-Path -LiteralPath $projectVersionPath -PathType Leaf) {
    $projectVersion = Get-Content -LiteralPath $projectVersionPath -Raw
    $projectVersionMatches = $projectVersion -match `
        "(?m)^m_EditorVersion:\s*$([Regex]::Escape($expectedUnityVersion))\s*$"
}

Write-ComponentStatus $projectVersionMatches "Unity project $expectedUnityVersion" `
    "ProjectSettings/ProjectVersion.txt must pin Unity $expectedUnityVersion."
Write-ComponentStatus (Test-Path -LiteralPath $unityPathValue -PathType Leaf) `
    "Unity Editor $expectedUnityVersion" `
    "Install Unity Editor $expectedUnityVersion in the configured location."

$targetPlatformJar = Join-Path $SdkRoot 'platforms\android-36\android.jar'
$adbPath = Join-Path $SdkRoot 'platform-tools\adb.exe'
$hasBuildTools = Test-Path -LiteralPath (Join-Path $SdkRoot 'build-tools\36.0.0\aapt2.exe') -PathType Leaf
$hasCommandLineTools = Test-Path -LiteralPath (Join-Path $SdkRoot 'cmdline-tools\16.0\bin\sdkmanager.bat') -PathType Leaf

$hasSdk = (Test-Path -LiteralPath $targetPlatformJar -PathType Leaf) -and
    (Test-Path -LiteralPath $adbPath -PathType Leaf) -and $hasBuildTools -and $hasCommandLineTools
Write-ComponentStatus $hasSdk 'Android SDK' `
    'Install Android SDK & NDK Tools with API 36 and Android SDK Build Tools for this Unity Editor.'
Write-ComponentStatus (Test-Path -LiteralPath $targetPlatformJar -PathType Leaf) 'Target API 36' `
    'Install the Android SDK Platform 36 component for this Unity Editor.'

$cmakePath = Join-Path $SdkRoot 'cmake\3.22.1\bin\cmake.exe'
Write-ComponentStatus (Test-Path -LiteralPath $cmakePath -PathType Leaf) 'CMake 3.22.1' `
    'Install CMake 3.22.1 from Android Studio SDK Tools.'

$ndkBuildPath = Join-Path $NdkRoot 'ndk-build.cmd'
Write-ComponentStatus (Test-Path -LiteralPath $ndkBuildPath -PathType Leaf) 'Android NDK' `
    'Install the Unity-pinned Android NDK component for this Unity Editor.'

$hasJava = -not [string]::IsNullOrWhiteSpace($JdkRoot) -and
    (Test-Path -LiteralPath (Join-Path $JdkRoot 'bin\java.exe') -PathType Leaf)
Write-ComponentStatus $hasJava 'OpenJDK' `
    'Install the OpenJDK component for this Unity Editor.'

if ($failures.Count -gt 0) {
    Write-Host ''
    Write-Host 'Android build toolchain: BLOCKED'
    foreach ($failure in $failures) {
        Write-Host "- $failure"
    }
    Write-Host '- Keep the company Unity installation unchanged; use a separate installation path for personal Android builds.'
    [Environment]::Exit(2)
}

Write-Host ''
Write-Host 'Android build toolchain: READY'
