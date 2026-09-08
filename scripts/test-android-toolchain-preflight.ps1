$ErrorActionPreference = 'Stop'

$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$preflightScript = Join-Path $projectRoot 'scripts\check-android-toolchain.ps1'
$fixtureRoot = Join-Path ([System.IO.Path]::GetTempPath()) `
    ("curio-android-toolchain-$([guid]::NewGuid().ToString('N'))")
$fixtureProject = Join-Path $fixtureRoot 'project'
$fixtureEditor = Join-Path $fixtureRoot 'Unity\Editor'
$fixtureUnity = Join-Path $fixtureEditor 'Unity.exe'
$pwshPath = (Get-Process -Id $PID).Path

function Assert-Contract {
    param(
        [bool]$Condition,
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Invoke-Preflight {
    $outputPath = Join-Path $fixtureRoot ("output-$([guid]::NewGuid().ToString('N')).txt")
    $errorPath = Join-Path $fixtureRoot ("error-$([guid]::NewGuid().ToString('N')).txt")
    $process = Start-Process -FilePath $pwshPath -ArgumentList @(
        '-NoProfile',
        '-NonInteractive',
        '-ExecutionPolicy', 'Bypass',
        '-File', "`"$preflightScript`"",
        '-ProjectRoot', "`"$fixtureProject`"",
        '-UnityPath', "`"$fixtureUnity`""
    ) -RedirectStandardOutput $outputPath -RedirectStandardError $errorPath -Wait -PassThru

    $output = Get-Content -LiteralPath $outputPath -Raw
    if (Test-Path -LiteralPath $errorPath) {
        $output += Get-Content -LiteralPath $errorPath -Raw
    }

    return [pscustomobject]@{
        ExitCode = $process.ExitCode
        Output = $output
    }
}

try {
    [System.IO.Directory]::CreateDirectory((Join-Path $fixtureProject 'ProjectSettings')) | Out-Null
    [System.IO.Directory]::CreateDirectory($fixtureEditor) | Out-Null
    [System.IO.File]::WriteAllText(
        (Join-Path $fixtureProject 'ProjectSettings\ProjectVersion.txt'),
        "m_EditorVersion: 6000.3.21f1`nm_EditorVersionWithRevision: 6000.3.21f1 (fixture)`n")
    [System.IO.File]::WriteAllBytes($fixtureUnity, [byte[]](1))

    $missing = Invoke-Preflight
    Assert-Contract ($missing.ExitCode -ne 0) `
        'The preflight must fail when Android SDK, NDK, and OpenJDK are absent.'
    Assert-Contract ($missing.Output -match 'Android build toolchain: BLOCKED') `
        'The failure must use a single clear BLOCKED summary.'
    foreach ($component in @('Android SDK', 'Android NDK', 'OpenJDK')) {
        Assert-Contract ($missing.Output -match [Regex]::Escape("[MISSING] $component")) `
            "The failure must identify the missing $component component."
    }
    Assert-Contract ($missing.Output -notmatch [Regex]::Escape($fixtureRoot)) `
        'The diagnostic output must not disclose an absolute machine path.'

    $androidRoot = Join-Path $fixtureEditor 'Data\PlaybackEngines\AndroidPlayer'
    $requiredFiles = @(
        (Join-Path $androidRoot 'SDK\platforms\android-36\android.jar'),
        (Join-Path $androidRoot 'SDK\build-tools\36.0.0\aapt2.exe'),
        (Join-Path $androidRoot 'SDK\platform-tools\adb.exe'),
        (Join-Path $androidRoot 'NDK\ndk-build.cmd'),
        (Join-Path $androidRoot 'OpenJDK\bin\java.exe')
    )
    foreach ($file in $requiredFiles) {
        [System.IO.Directory]::CreateDirectory((Split-Path -Parent $file)) | Out-Null
        [System.IO.File]::WriteAllBytes($file, [byte[]](1))
    }

    $ready = Invoke-Preflight
    Assert-Contract ($ready.ExitCode -eq 0) `
        "A complete pinned fixture must pass. Output: $($ready.Output)"
    Assert-Contract ($ready.Output -match 'Android build toolchain: READY') `
        'The successful result must use a single clear READY summary.'
    Assert-Contract ($ready.Output -match '\[READY\] Target API 36') `
        'The successful result must confirm the pinned target API platform.'
    Assert-Contract ($ready.Output -notmatch [Regex]::Escape($fixtureRoot)) `
        'The successful diagnostic output must not disclose an absolute machine path.'

    $fixtureWrapperRoot = Join-Path $fixtureRoot 'wrapper'
    $fixtureScripts = Join-Path $fixtureWrapperRoot 'scripts'
    [System.IO.Directory]::CreateDirectory($fixtureScripts) | Out-Null
    [System.IO.File]::Copy(
        (Join-Path $projectRoot 'scripts\build-android.ps1'),
        (Join-Path $fixtureScripts 'build-android.ps1'),
        $true)
    [System.IO.File]::WriteAllText(
        (Join-Path $fixtureScripts 'check-android-toolchain.ps1'),
        "Write-Host 'fixture toolchain blocked'`nexit 2`n")
    [System.IO.File]::WriteAllText(
        (Join-Path $fixtureScripts 'check-no-remote-telemetry.ps1'),
        "Write-Host 'telemetry gate should not run'`nexit 0`n")

    $wrapperOutputPath = Join-Path $fixtureRoot 'wrapper-output.txt'
    $wrapperErrorPath = Join-Path $fixtureRoot 'wrapper-error.txt'
    $wrapperProcess = Start-Process -FilePath $pwshPath -ArgumentList @(
        '-NoProfile',
        '-NonInteractive',
        '-ExecutionPolicy', 'Bypass',
        '-File', "`"$(Join-Path $fixtureScripts 'build-android.ps1')`"",
        '-UnityPath', "`"$fixtureUnity`""
    ) -RedirectStandardOutput $wrapperOutputPath -RedirectStandardError $wrapperErrorPath -Wait -PassThru
    $wrapperOutput = (Get-Content -LiteralPath $wrapperOutputPath -Raw) +
        (Get-Content -LiteralPath $wrapperErrorPath -Raw)
    Assert-Contract ($wrapperProcess.ExitCode -ne 0) `
        'The release build wrapper must stop when the Android toolchain preflight fails.'
    Assert-Contract ($wrapperOutput -match 'fixture toolchain blocked') `
        'The release build wrapper must run the Android toolchain preflight.'
    Assert-Contract ($wrapperOutput -notmatch 'telemetry gate should not run') `
        'The Android toolchain preflight must run before later release gates.'

    $cmdWrapperPath = Join-Path $projectRoot 'scripts\check-android-toolchain.cmd'
    $cmdOutputPath = Join-Path $fixtureRoot 'cmd-output.txt'
    $cmdErrorPath = Join-Path $fixtureRoot 'cmd-error.txt'
    $cmdLine = "$cmdWrapperPath -ProjectRoot $fixtureProject -UnityPath $fixtureUnity"
    $cmdProcess = Start-Process -FilePath $env:ComSpec -ArgumentList @('/d', '/c', $cmdLine) `
        -RedirectStandardOutput $cmdOutputPath -RedirectStandardError $cmdErrorPath -Wait -PassThru
    $cmdOutput = (Get-Content -LiteralPath $cmdOutputPath -Raw) +
        (Get-Content -LiteralPath $cmdErrorPath -Raw)
    Assert-Contract ($cmdProcess.ExitCode -eq 0 -and $cmdOutput -match 'Android build toolchain: READY') `
        'The execution-policy-free CMD entry point must forward arguments to the preflight.'

    $buildCmdPath = Join-Path $projectRoot 'scripts\build-android.cmd'
    $buildCmdOutputPath = Join-Path $fixtureRoot 'build-cmd-output.txt'
    $buildCmdErrorPath = Join-Path $fixtureRoot 'build-cmd-error.txt'
    $missingBuildUnity = Join-Path $fixtureRoot 'MissingUnity\Editor\Unity.exe'
    $buildCmdLine = "$buildCmdPath -UnityPath $missingBuildUnity"
    $buildCmdProcess = Start-Process -FilePath $env:ComSpec -ArgumentList @('/d', '/c', $buildCmdLine) `
        -RedirectStandardOutput $buildCmdOutputPath -RedirectStandardError $buildCmdErrorPath -Wait -PassThru
    $buildCmdOutput = (Get-Content -LiteralPath $buildCmdOutputPath -Raw) +
        (Get-Content -LiteralPath $buildCmdErrorPath -Raw)
    Assert-Contract ($buildCmdProcess.ExitCode -ne 0 -and
        $buildCmdOutput -match 'Android build toolchain: BLOCKED') `
        "The release CMD entry point must bypass execution policy and preserve the toolchain failure. Output: $buildCmdOutput"
}
finally {
    if (Test-Path -LiteralPath $fixtureRoot) {
        $resolvedFixtureRoot = [System.IO.Path]::GetFullPath($fixtureRoot)
        $temporaryRoot = [System.IO.Path]::GetFullPath([System.IO.Path]::GetTempPath())
        if (-not $temporaryRoot.EndsWith([System.IO.Path]::DirectorySeparatorChar.ToString(),
                [StringComparison]::Ordinal)) {
            $temporaryRoot += [System.IO.Path]::DirectorySeparatorChar
        }

        Assert-Contract ($resolvedFixtureRoot.StartsWith($temporaryRoot, [StringComparison]::OrdinalIgnoreCase) -and
            [System.IO.Path]::GetFileName($resolvedFixtureRoot).StartsWith('curio-android-toolchain-',
                [StringComparison]::Ordinal)) `
            'Refusing to remove a toolchain fixture outside the expected temporary directory.'
        Remove-Item -LiteralPath $fixtureRoot -Recurse -Force
    }
}

Write-Host 'Android toolchain preflight controlled fixtures passed.'
