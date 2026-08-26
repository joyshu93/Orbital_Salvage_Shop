param(
    [string]$ProjectRoot,
    [string]$UnityPath = 'C:\Program Files\Unity 6000.3.21f1\Editor\Unity.exe'
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

$sdkRoot = Join-Path $androidRoot 'SDK'
$targetPlatformJar = Join-Path $sdkRoot 'platforms\android-36\android.jar'
$adbPath = Join-Path $sdkRoot 'platform-tools\adb.exe'
$buildToolsRoot = Join-Path $sdkRoot 'build-tools'
$hasBuildTools = $false
if (Test-Path -LiteralPath $buildToolsRoot -PathType Container) {
    $hasBuildTools = $null -ne (Get-ChildItem -LiteralPath $buildToolsRoot -Directory -ErrorAction SilentlyContinue |
        Where-Object { Test-Path -LiteralPath (Join-Path $_.FullName 'aapt2.exe') -PathType Leaf } |
        Select-Object -First 1)
}

$hasSdk = (Test-Path -LiteralPath $targetPlatformJar -PathType Leaf) -and
    (Test-Path -LiteralPath $adbPath -PathType Leaf) -and $hasBuildTools
Write-ComponentStatus $hasSdk 'Android SDK' `
    'Install Android SDK & NDK Tools with API 36 and Android SDK Build Tools for this Unity Editor.'
Write-ComponentStatus (Test-Path -LiteralPath $targetPlatformJar -PathType Leaf) 'Target API 36' `
    'Install the Android SDK Platform 36 component for this Unity Editor.'

$ndkBuildPath = Join-Path $androidRoot 'NDK\ndk-build.cmd'
Write-ComponentStatus (Test-Path -LiteralPath $ndkBuildPath -PathType Leaf) 'Android NDK' `
    'Install the Unity-pinned Android NDK component for this Unity Editor.'

$javaPath = Join-Path $androidRoot 'OpenJDK\bin\java.exe'
Write-ComponentStatus (Test-Path -LiteralPath $javaPath -PathType Leaf) 'OpenJDK' `
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
