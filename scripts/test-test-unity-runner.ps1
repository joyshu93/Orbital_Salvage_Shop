$ErrorActionPreference = 'Stop'

$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$runnerPath = Join-Path $PSScriptRoot 'test-unity.ps1'
$fixtureRoot = Join-Path ([System.IO.Path]::GetTempPath()) ("curio-unity-runner-" + [Guid]::NewGuid().ToString('N'))
$fakeUnityPath = Join-Path $fixtureRoot 'FakeUnity.exe'
$evidencePaths = @(
    (Join-Path $projectRoot 'Logs\EditMode-results.xml'),
    (Join-Path $projectRoot 'Logs\EditMode.log'),
    (Join-Path $projectRoot 'Logs\PlayMode-results.xml'),
    (Join-Path $projectRoot 'Logs\PlayMode.log')
)
$evidenceBackups = @{}
foreach ($evidencePath in $evidencePaths)
{
    if (Test-Path -LiteralPath $evidencePath)
    {
        $evidenceBackups[$evidencePath] = [System.IO.File]::ReadAllBytes($evidencePath)
    }
}

$fakeUnitySource = @'
using System;
using System.Diagnostics;
using System.IO;

public static class FakeUnity
{
    public static int Main(string[] args)
    {
        string resultPath = null;
        for (var index = 0; index < args.Length - 1; index++)
        {
            if (string.Equals(args[index], "-testResults", StringComparison.OrdinalIgnoreCase))
            {
                resultPath = args[index + 1].Trim('"');
                break;
            }
        }

        if (string.IsNullOrEmpty(resultPath))
        {
            return 2;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(resultPath));
        File.WriteAllText(resultPath, "<test-run result=\"Passed\" passed=\"1\" failed=\"0\" total=\"1\" />");
        Process.Start(new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = "-NoProfile -Command Start-Sleep -Seconds 5",
            UseShellExecute = false,
            CreateNoWindow = true
        });
        return 0;
    }
}
'@

try
{
    New-Item -ItemType Directory -Force -Path $fixtureRoot | Out-Null
    Add-Type -TypeDefinition $fakeUnitySource -Language CSharp -OutputAssembly $fakeUnityPath -OutputType ConsoleApplication

    $watch = [System.Diagnostics.Stopwatch]::StartNew()
    $output = & powershell.exe -NoProfile -ExecutionPolicy Bypass -File $runnerPath -UnityPath $fakeUnityPath 2>&1
    $exitCode = $LASTEXITCODE
    $watch.Stop()

    if ($exitCode -ne 0)
    {
        throw "Test runner exited with code $exitCode.`n$($output -join [Environment]::NewLine)"
    }

    if ($output -notcontains 'EditMode passed: 1/1' -or $output -notcontains 'PlayMode passed: 1/1')
    {
        throw "Test runner did not complete both modes.`n$($output -join [Environment]::NewLine)"
    }

    if ($watch.Elapsed.TotalSeconds -ge 4)
    {
        throw "Test runner waited $([Math]::Round($watch.Elapsed.TotalSeconds, 2)) seconds for child processes after FakeUnity exited."
    }

    Write-Host "Unity runner process-wait regression passed in $([Math]::Round($watch.Elapsed.TotalSeconds, 2)) seconds."
}
finally
{
    foreach ($evidencePath in $evidencePaths)
    {
        if ($evidenceBackups.ContainsKey($evidencePath))
        {
            [System.IO.File]::WriteAllBytes($evidencePath, $evidenceBackups[$evidencePath])
        }
        elseif (Test-Path -LiteralPath $evidencePath)
        {
            Remove-Item -LiteralPath $evidencePath -Force
        }
    }

    if (Test-Path -LiteralPath $fixtureRoot)
    {
        Remove-Item -LiteralPath $fixtureRoot -Recurse -Force
    }
}
