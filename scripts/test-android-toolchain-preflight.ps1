$ErrorActionPreference = 'Stop'

$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$preflightScript = Join-Path $projectRoot 'scripts\check-android-toolchain.ps1'
$fixtureRoot = Join-Path ([System.IO.Path]::GetTempPath()) `
    ("curio-android-toolchain-$([guid]::NewGuid().ToString('N'))")
$fixtureProject = Join-Path $fixtureRoot 'project'
$fixtureEditor = Join-Path $fixtureRoot 'Unity\Editor'
$fixtureUnity = Join-Path $fixtureEditor 'Unity.exe'
$pwshPath = (Get-Process -Id $PID).Path
$toolchainEnvironmentNames = @(
    'CURIO_ANDROID_SDK_ROOT',
    'CURIO_ANDROID_NDK_ROOT',
    'CURIO_ANDROID_JDK_ROOT'
)
$originalToolchainEnvironment = @{}

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
    param(
        [string[]]$AdditionalArguments = @('-DisableExternalAutoDiscovery')
    )

    $outputPath = Join-Path $fixtureRoot ("output-$([guid]::NewGuid().ToString('N')).txt")
    $errorPath = Join-Path $fixtureRoot ("error-$([guid]::NewGuid().ToString('N')).txt")
    $arguments = @(
        '-NoProfile',
        '-NonInteractive',
        '-ExecutionPolicy', 'Bypass',
        '-File', "`"$preflightScript`"",
        '-ProjectRoot', "`"$fixtureProject`"",
        '-UnityPath', "`"$fixtureUnity`""
    ) + $AdditionalArguments
    $process = Start-Process -FilePath $pwshPath -ArgumentList $arguments `
        -RedirectStandardOutput $outputPath -RedirectStandardError $errorPath -Wait -PassThru

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
    foreach ($environmentName in $toolchainEnvironmentNames) {
        $originalToolchainEnvironment[$environmentName] =
            [Environment]::GetEnvironmentVariable($environmentName, 'Process')
        [Environment]::SetEnvironmentVariable($environmentName, $null, 'Process')
    }

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
    foreach ($component in @('Android SDK', 'Target API 36', 'CMake 3.22.1', 'Android NDK', 'OpenJDK')) {
        Assert-Contract ($missing.Output -match [Regex]::Escape("[MISSING] $component")) `
            "The failure must identify the missing $component component."
    }
    Assert-Contract ($missing.Output -notmatch [Regex]::Escape($fixtureRoot)) `
        'The diagnostic output must not disclose an absolute machine path.'

    $externalSdk = Join-Path $fixtureRoot 'external\sdk'
    $externalNdk = Join-Path $fixtureRoot 'external\ndk\27.2.12479018'
    $externalJdk = Join-Path $fixtureRoot 'external\jdk17'
    $externalFiles = @(
        (Join-Path $externalSdk 'platforms\android-36\android.jar'),
        (Join-Path $externalSdk 'build-tools\36.0.0\aapt2.exe'),
        (Join-Path $externalSdk 'platform-tools\adb.exe'),
        (Join-Path $externalSdk 'cmdline-tools\16.0\bin\sdkmanager.bat'),
        (Join-Path $externalSdk 'cmake\3.22.1\bin\cmake.exe'),
        (Join-Path $externalNdk 'ndk-build.cmd'),
        (Join-Path $externalJdk 'bin\java.exe')
    )
    foreach ($file in $externalFiles) {
        [System.IO.Directory]::CreateDirectory((Split-Path -Parent $file)) | Out-Null
        [System.IO.File]::WriteAllBytes($file, [byte[]](1))
    }

    $env:CURIO_ANDROID_SDK_ROOT = $externalSdk
    $env:CURIO_ANDROID_NDK_ROOT = $externalNdk
    $env:CURIO_ANDROID_JDK_ROOT = $externalJdk
    $externalReady = Invoke-Preflight
    Assert-Contract ($externalReady.ExitCode -eq 0) `
        "A complete external toolchain supplied through process environment must pass. Output: $($externalReady.Output)"
    Assert-Contract ($externalReady.Output -match 'Android build toolchain: READY') `
        'The external toolchain result must use the standard READY summary.'
    Assert-Contract ($externalReady.Output -notmatch [Regex]::Escape($fixtureRoot)) `
        'The external toolchain result must not disclose an absolute machine path.'

    foreach ($environmentName in $toolchainEnvironmentNames) {
        [Environment]::SetEnvironmentVariable($environmentName, $null, 'Process')
    }

    $autoLocalApplicationData = Join-Path $fixtureRoot 'auto-user\AppData\Local'
    $autoUserProfile = Join-Path $fixtureRoot 'auto-user'
    $autoSdk = Join-Path $autoLocalApplicationData 'Android\Sdk'
    $autoNdk = Join-Path $autoSdk 'ndk\27.2.12479018'
    $autoJdk = Join-Path $autoUserProfile 'UnityPersonal\OpenJDK17\jdk-17-fixture'
    foreach ($file in @(
        (Join-Path $autoSdk 'platforms\android-36\android.jar'),
        (Join-Path $autoSdk 'build-tools\36.0.0\aapt2.exe'),
        (Join-Path $autoSdk 'platform-tools\adb.exe'),
        (Join-Path $autoSdk 'cmdline-tools\16.0\bin\sdkmanager.bat'),
        (Join-Path $autoSdk 'cmake\3.22.1\bin\cmake.exe'),
        (Join-Path $autoNdk 'ndk-build.cmd'),
        (Join-Path $autoJdk 'bin\java.exe')
    )) {
        [System.IO.Directory]::CreateDirectory((Split-Path -Parent $file)) | Out-Null
        [System.IO.File]::WriteAllBytes($file, [byte[]](1))
    }

    $autoReady = Invoke-Preflight -AdditionalArguments @(
        '-LocalApplicationDataRoot', "`"$autoLocalApplicationData`"",
        '-UserProfileRoot', "`"$autoUserProfile`""
    )
    Assert-Contract ($autoReady.ExitCode -eq 0 -and
        $autoReady.Output -match 'Android build toolchain: READY') `
        "The default external locations must be discoverable from explicit profile roots. Output: $($autoReady.Output)"
    Assert-Contract ($autoReady.Output -notmatch [Regex]::Escape($fixtureRoot)) `
        'Automatic external discovery must not disclose an absolute machine path.'

    $androidRoot = Join-Path $fixtureEditor 'Data\PlaybackEngines\AndroidPlayer'
    [System.IO.Directory]::CreateDirectory((Join-Path $androidRoot 'SDK')) | Out-Null
    $partialBundle = Invoke-Preflight -AdditionalArguments @(
        '-LocalApplicationDataRoot', "`"$autoLocalApplicationData`"",
        '-UserProfileRoot', "`"$autoUserProfile`""
    )
    Assert-Contract ($partialBundle.ExitCode -eq 0) `
        'An incomplete bundled SDK must fall back to the complete external toolchain.'

    $env:CURIO_ANDROID_SDK_ROOT = $externalSdk
    $partialOverrides = Invoke-Preflight
    Assert-Contract ($partialOverrides.ExitCode -ne 0) `
        'A partial explicit override must be rejected, not mixed with discovered roots.'
    $env:CURIO_ANDROID_SDK_ROOT = $null

    $requiredFiles = @(
        (Join-Path $androidRoot 'SDK\platforms\android-36\android.jar'),
        (Join-Path $androidRoot 'SDK\build-tools\36.0.0\aapt2.exe'),
        (Join-Path $androidRoot 'SDK\platform-tools\adb.exe'),
        (Join-Path $androidRoot 'SDK\cmdline-tools\16.0\bin\sdkmanager.bat'),
        (Join-Path $androidRoot 'SDK\cmake\3.22.1\bin\cmake.exe'),
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

    foreach ($requiredMarker in @('cmdline-tools\16.0\bin\sdkmanager.bat', 'build-tools\36.0.0\aapt2.exe')) {
        $markerPath = Join-Path (Join-Path $androidRoot 'SDK') $requiredMarker
        $markerBytes = [System.IO.File]::ReadAllBytes($markerPath)
        Remove-Item -LiteralPath $markerPath
        try {
            $incomplete = Invoke-Preflight
            Assert-Contract ($incomplete.ExitCode -ne 0) `
                "A toolchain missing $requiredMarker must fail before Unity starts."
        }
        finally { [System.IO.File]::WriteAllBytes($markerPath, $markerBytes) }
    }

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
    $cmdLine = "$cmdWrapperPath -ProjectRoot $fixtureProject -UnityPath $fixtureUnity -DisableExternalAutoDiscovery"
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
    foreach ($environmentName in $toolchainEnvironmentNames) {
        [Environment]::SetEnvironmentVariable(
            $environmentName,
            $originalToolchainEnvironment[$environmentName],
            'Process')
    }

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
