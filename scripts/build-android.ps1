param(
    [string]$UnityPath = 'C:\Program Files\Unity 6000.3.21f1\Editor\Unity.exe'
)

$ErrorActionPreference = 'Stop'
$ProjectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$AabPath = Join-Path $ProjectRoot 'Builds\Android\CurioClerk.aab'
$ManifestPath = Join-Path $ProjectRoot 'Builds\Android\CurioClerk-build.json'
$LogPath = Join-Path $ProjectRoot 'Logs\AndroidBuild.log'
$TelemetryGatePath = Join-Path $PSScriptRoot 'check-no-remote-telemetry.ps1'
$requiredEnvironmentNames = @(
    'CURIO_ADMOB_APP_ID',
    'CURIO_ADMOB_REWARDED_ID',
    'CURIO_ANDROID_KEYSTORE_PATH',
    'CURIO_ANDROID_KEYSTORE_PASS',
    'CURIO_ANDROID_KEY_ALIAS',
    'CURIO_ANDROID_KEY_PASS'
)

$global:LASTEXITCODE = 0
& $TelemetryGatePath -ProjectRoot $ProjectRoot -Mode Release
if ($LASTEXITCODE -ne 0) {
    throw 'The release no-remote-telemetry gate failed. Unity was not started.'
}

$missingEnvironmentNames = @($requiredEnvironmentNames | Where-Object {
    [string]::IsNullOrWhiteSpace([Environment]::GetEnvironmentVariable($_))
})
if ($missingEnvironmentNames.Count -gt 0) {
    throw "Missing required release environment variables: $($missingEnvironmentNames -join ', ')"
}

if (-not (Test-Path -LiteralPath $UnityPath -PathType Leaf)) {
    throw 'Unity 6000.3.21f1 was not found at the configured location.'
}

$outputDirectory = Split-Path -Parent $AabPath
if (Test-Path -LiteralPath $AabPath) {
    Remove-Item -LiteralPath $AabPath -Force
}

if (Test-Path -LiteralPath $ManifestPath) {
    Remove-Item -LiteralPath $ManifestPath -Force
}

if (Test-Path -LiteralPath $outputDirectory) {
    Get-ChildItem -LiteralPath $outputDirectory -Filter '*.symbols.zip' -File | Remove-Item -Force
}

$arguments = @(
    '-batchmode',
    '-nographics',
    '-quit',
    '-projectPath', "`"$ProjectRoot`"",
    '-executeMethod', 'CurioClerk.Editor.ProjectBuilder.BuildAndroid',
    '-logFile', "`"$LogPath`""
)
$process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -Wait -PassThru -WindowStyle Hidden
if ($process.ExitCode -ne 0) {
    throw "Unity Android build exited with code $($process.ExitCode). See Logs/AndroidBuild.log."
}

if (-not (Test-Path -LiteralPath $AabPath -PathType Leaf)) {
    throw 'Android AAB was not created. See Logs/AndroidBuild.log.'
}

$symbols = @(Get-ChildItem -LiteralPath $outputDirectory -Filter '*.symbols.zip' -File -ErrorAction SilentlyContinue)
if ($symbols.Count -ne 1) {
    throw "Expected exactly one general IL2CPP symbols zip, but found $($symbols.Count)."
}

if (-not (Test-Path -LiteralPath $ManifestPath -PathType Leaf)) {
    throw 'Sanitized Android build manifest was not created.'
}

try {
    $manifest = Get-Content -LiteralPath $ManifestPath -Raw | ConvertFrom-Json
}
catch {
    throw 'Sanitized Android build manifest is not valid JSON.'
}

if ($manifest.aabSha256 -notmatch '\A[0-9A-F]{64}\z') {
    throw 'Sanitized Android build manifest contains an invalid AAB SHA-256.'
}

$actualSha256 = (Get-FileHash -LiteralPath $AabPath -Algorithm SHA256).Hash
if (-not [string]::Equals($manifest.aabSha256, $actualSha256, [StringComparison]::OrdinalIgnoreCase)) {
    throw 'AAB SHA-256 does not match the sanitized build manifest.'
}

Write-Host 'Release AAB, one IL2CPP symbols archive, and matching sanitized manifest are ready.'
