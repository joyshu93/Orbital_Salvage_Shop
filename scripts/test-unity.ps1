param(
    [string]$UnityPath = 'C:\Program Files\Unity 6000.3.21f1\Editor\Unity.exe'
)

$ErrorActionPreference = 'Stop'
$ProjectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$LogsRoot = Join-Path $ProjectRoot 'Logs'
New-Item -ItemType Directory -Force -Path $LogsRoot | Out-Null

if (-not (Test-Path -LiteralPath $UnityPath)) {
    throw "Unity was not found at $UnityPath"
}

function Invoke-CurioTests([string]$Platform) {
    $resultPath = Join-Path $LogsRoot "$Platform-results.xml"
    $logPath = Join-Path $LogsRoot "$Platform.log"
    $arguments = @(
        '-batchmode',
        '-nographics',
        '-projectPath', "`"$ProjectRoot`"",
        '-runTests',
        '-testPlatform', $Platform,
        '-testResults', "`"$resultPath`"",
        '-logFile', "`"$logPath`""
    )
    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -PassThru -WindowStyle Hidden
    $process.WaitForExit()
    if ($process.ExitCode -ne 0) {
        throw "$Platform Unity process exited with code $($process.ExitCode). See $logPath"
    }

    if (-not (Test-Path -LiteralPath $resultPath)) {
        throw "$Platform did not produce a test result. See $logPath"
    }

    [xml]$result = Get-Content -LiteralPath $resultPath -Raw
    if ($result.'test-run'.result -ne 'Passed') {
        throw "$Platform tests failed. See $resultPath"
    }

    Write-Host "$Platform passed: $($result.'test-run'.passed)/$($result.'test-run'.total)"
}

Invoke-CurioTests 'EditMode'
Invoke-CurioTests 'PlayMode'
