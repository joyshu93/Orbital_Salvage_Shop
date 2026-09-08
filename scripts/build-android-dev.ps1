param(
    [string]$UnityPath = 'C:\Program Files\Unity 6000.3.21f1\Editor\Unity.exe'
)

$ErrorActionPreference = 'Stop'
$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$apkPath = Join-Path $projectRoot 'Builds\Android\CurioClerk-qa.apk'
$logPath = Join-Path $projectRoot 'Logs\AndroidDevelopmentBuild.log'
$toolchainPreflightPath = Join-Path $PSScriptRoot 'check-android-toolchain.ps1'
$testScriptPath = Join-Path $PSScriptRoot 'test-unity.ps1'

$windowsPowerShell = Join-Path ([Environment]::GetFolderPath([Environment+SpecialFolder]::System)) `
    'WindowsPowerShell\v1.0\powershell.exe'
if (-not (Test-Path -LiteralPath $windowsPowerShell -PathType Leaf)) {
    throw 'The Windows PowerShell host required by the Android toolchain preflight is missing.'
}

$global:LASTEXITCODE = 0
& $windowsPowerShell -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $toolchainPreflightPath `
    -ProjectRoot $projectRoot -UnityPath $UnityPath
if ($LASTEXITCODE -ne 0) {
    throw 'Android toolchain preflight failed. Unity was not started.'
}

$global:LASTEXITCODE = 0
Write-Host 'Running Unity EditMode and PlayMode tests...'
& $windowsPowerShell -NoProfile -NonInteractive -ExecutionPolicy Bypass -File $testScriptPath `
    -UnityPath $UnityPath
if ($LASTEXITCODE -ne 0) {
    throw 'Unity tests failed. The QA APK was not built.'
}

if (-not (Test-Path -LiteralPath $UnityPath -PathType Leaf)) {
    throw 'Unity 6000.3.21f1 was not found at the configured location.'
}

$outputDirectory = Split-Path -Parent $apkPath
[System.IO.Directory]::CreateDirectory($outputDirectory) | Out-Null
if (Test-Path -LiteralPath $apkPath -PathType Leaf) {
    Remove-Item -LiteralPath $apkPath -Force
}

$arguments = @(
    '-batchmode',
    '-nographics',
    '-quit',
    '-projectPath', "`"$projectRoot`"",
    '-executeMethod', 'CurioClerk.Editor.ProjectBuilder.BuildAndroidDevelopment',
    '-logFile', "`"$logPath`""
)
Write-Host 'Building the ARM64 IL2CPP QA APK. The first build can take several minutes...'
$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -PassThru -WindowStyle Hidden
$process.WaitForExit()
if ($process.ExitCode -ne 0) {
    throw "Unity Android QA build exited with code $($process.ExitCode). See Logs/AndroidDevelopmentBuild.log."
}

if (-not (Test-Path -LiteralPath $apkPath -PathType Leaf) -or
    (Get-Item -LiteralPath $apkPath).Length -le 0) {
    throw 'Android QA APK was not created. See Logs/AndroidDevelopmentBuild.log.'
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($apkPath)
try {
    $arm64Library = $archive.GetEntry('lib/arm64-v8a/libil2cpp.so')
    if ($null -eq $arm64Library) {
        throw 'Android QA APK does not contain the ARM64 IL2CPP player library.'
    }
}
finally {
    $archive.Dispose()
}

Write-Host 'QA APK ready: Builds/Android/CurioClerk-qa.apk'
Write-Host 'This development build uses Google sample rewarded ads and Unity debug signing.'
