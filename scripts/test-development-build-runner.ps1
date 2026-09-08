$ErrorActionPreference = 'Stop'
$fixtureRoot = Join-Path ([IO.Path]::GetTempPath()) ('curio-qa-runner-' + [Guid]::NewGuid().ToString('N'))
$fixtureScripts = Join-Path $fixtureRoot 'scripts'
$fakeUnity = Join-Path $fixtureRoot 'FakeUnity.exe'
$source = @'
using System;
using System.Diagnostics;
using System.IO;
public static class FakeUnity
{
    public static int Main(string[] args)
    {
        string project = null;
        string method = null;
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "-projectPath") project = args[i + 1].Trim('"');
            if (args[i] == "-executeMethod") method = args[i + 1];
        }
        if (project == null || method != "CurioClerk.Editor.ProjectBuilder.BuildAndroidDevelopment") return 2;
        if (!File.Exists(Path.Combine(project, "tests-ran"))) return 3;
        string apk = Path.Combine(project, "Builds", "Android", "CurioClerk-qa.apk");
        Directory.CreateDirectory(Path.GetDirectoryName(apk));
        File.Copy(Path.Combine(project, "seed.apk"), apk);
        Process.Start(new ProcessStartInfo {
            FileName = "powershell.exe", Arguments = "-NoProfile -Command Start-Sleep -Seconds 5",
            UseShellExecute = false, CreateNoWindow = true
        });
        return 0;
    }
}
'@
try {
    [IO.Directory]::CreateDirectory($fixtureScripts) | Out-Null
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'build-android-dev.ps1') -Destination $fixtureScripts
    [IO.File]::WriteAllText((Join-Path $fixtureScripts 'check-android-toolchain.ps1'), 'exit 0')
    [IO.File]::WriteAllText((Join-Path $fixtureScripts 'test-unity.ps1'),
        '[IO.File]::WriteAllText((Join-Path $PSScriptRoot "../tests-ran"), "passed"); exit 0')
    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [IO.Compression.ZipFile]::Open((Join-Path $fixtureRoot 'seed.apk'), [IO.Compression.ZipArchiveMode]::Create)
    try { $zip.CreateEntry('lib/arm64-v8a/libil2cpp.so') | Out-Null }
    finally { $zip.Dispose() }
    Add-Type -TypeDefinition $source -OutputAssembly $fakeUnity -OutputType ConsoleApplication
    $watch = [Diagnostics.Stopwatch]::StartNew()
    $output = & powershell.exe -NoProfile -NonInteractive -ExecutionPolicy Bypass `
        -File (Join-Path $fixtureScripts 'build-android-dev.ps1') -UnityPath $fakeUnity
    $exitCode = $LASTEXITCODE
    $watch.Stop()
    if ($exitCode -ne 0) { throw "QA wrapper fixture exited with $exitCode." }
    if ($output -notcontains 'QA APK ready: Builds/Android/CurioClerk-qa.apk') {
        throw 'QA wrapper did not validate and report its artifact.'
    }
    if ($watch.Elapsed.TotalSeconds -ge 4) {
        throw 'QA wrapper waited for an unrelated descendant after Unity exited.'
    }
    Write-Host 'Development build process-wait regression passed.'
}
finally {
    $resolved = [IO.Path]::GetFullPath($fixtureRoot)
    $temporary = [IO.Path]::GetFullPath([IO.Path]::GetTempPath()).TrimEnd('\') + '\'
    if (-not $resolved.StartsWith($temporary, [StringComparison]::OrdinalIgnoreCase) -or
        -not [IO.Path]::GetFileName($resolved).StartsWith('curio-qa-runner-')) {
        throw 'Unsafe fixture cleanup path.'
    }
    if (Test-Path -LiteralPath $resolved) { Remove-Item -LiteralPath $resolved -Recurse -Force }
}
