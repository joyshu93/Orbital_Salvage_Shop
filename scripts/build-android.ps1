param(
    [string]$UnityPath = 'C:\Program Files\Unity 6000.3.21f1\Editor\Unity.exe'
)

$ErrorActionPreference = 'Stop'
$ProjectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$AabPath = Join-Path $ProjectRoot 'Builds\Android\CurioClerk.aab'
$LogPath = Join-Path $ProjectRoot 'Logs\AndroidBuild.log'

if (-not (Test-Path -LiteralPath $UnityPath)) {
    throw "Unity was not found at $UnityPath"
}

$outputDirectory = Split-Path $AabPath
if (Test-Path -LiteralPath $AabPath) {
    Remove-Item -LiteralPath $AabPath -Force
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
    throw "Unity Android build exited with code $($process.ExitCode). See $LogPath"
}

if (-not (Test-Path -LiteralPath $AabPath)) {
    throw "Android AAB was not created. See $LogPath"
}

$bundle = Get-Item -LiteralPath $AabPath
Write-Host "AAB ready: $($bundle.FullName) ($([math]::Round($bundle.Length / 1MB, 2)) MB)"
$symbols = @(Get-ChildItem -LiteralPath $outputDirectory -Filter '*.symbols.zip' -File -ErrorAction SilentlyContinue)
if ($symbols.Count -eq 0) {
    throw "Android symbols zip was not created. See $LogPath"
}

$symbols | ForEach-Object {
    Write-Host "Symbols ready: $($_.FullName)"
}
