param(
    [Parameter(Mandatory = $true)]
    [string]$AabPath,

    [Parameter(Mandatory = $true)]
    [string]$BundletoolPath
)

$ErrorActionPreference = 'Stop'
$ProjectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$InspectionPath = Join-Path $ProjectRoot 'Builds\Android\inspection.txt'

if (-not (Test-Path -LiteralPath $AabPath -PathType Leaf)) {
    throw 'The AAB to inspect does not exist.'
}

if (-not (Test-Path -LiteralPath $BundletoolPath -PathType Leaf)) {
    throw 'The bundletool jar does not exist.'
}

$resolvedAabPath = (Resolve-Path -LiteralPath $AabPath).Path
$resolvedBundletoolPath = (Resolve-Path -LiteralPath $BundletoolPath).Path
if ([System.IO.Path]::GetFileName($resolvedBundletoolPath) -cne 'bundletool-all-1.18.3.jar') {
    throw 'Bundle inspection requires the pinned bundletool-all-1.18.3.jar.'
}

if ($null -eq (Get-Command 'java' -ErrorAction SilentlyContinue)) {
    throw 'Java was not found on PATH.'
}

function Invoke-Bundletool {
    param([string[]]$Arguments)

    $output = @(& java -jar $resolvedBundletoolPath @Arguments 2>&1)
    if ($LASTEXITCODE -ne 0) {
        throw 'bundletool rejected the AAB. No inspection report was written.'
    }

    return ($output | ForEach-Object { $_.ToString() }) -join [Environment]::NewLine
}

function ConvertTo-SanitizedText {
    param([string]$Text)

    $sanitized = $Text
    foreach ($path in @($resolvedAabPath, $resolvedBundletoolPath, $ProjectRoot, $env:USERPROFILE)) {
        if (-not [string]::IsNullOrWhiteSpace($path)) {
            $sanitized = [Regex]::Replace($sanitized, [Regex]::Escape($path), '[path]',
                [Text.RegularExpressions.RegexOptions]::IgnoreCase)
        }
    }

    $sanitized = [Regex]::Replace($sanitized, 'ca-app-pub-[0-9]+[~/][0-9]+', '[redacted-service-id]')
    return [Regex]::Replace($sanitized, '(?i)\b[A-Z]:\\[^\r\n"'']+', '[path]')
}

$validateOutput = Invoke-Bundletool @('validate', "--bundle=$resolvedAabPath")
$manifestOutput = Invoke-Bundletool @('dump', 'manifest', "--bundle=$resolvedAabPath", '--module=base')

$manifestContracts = @(
    [pscustomobject]@{ Label = 'package ID'; Pattern = 'package="com\.joyshu93\.curioclerknightshift"' },
    [pscustomobject]@{ Label = 'version name'; Pattern = '(?:android:)?versionName="1\.0\.0"' },
    [pscustomobject]@{ Label = 'version code'; Pattern = '(?:android:)?versionCode="10000"' },
    [pscustomobject]@{ Label = 'minimum SDK'; Pattern = '(?:android:)?minSdkVersion="29"' },
    [pscustomobject]@{ Label = 'target SDK'; Pattern = '(?:android:)?targetSdkVersion="36"' }
)
foreach ($contract in $manifestContracts) {
    if ($manifestOutput -notmatch $contract.Pattern) {
        throw "AAB manifest does not contain the required $($contract.Label)."
    }
}

Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [System.IO.Compression.ZipFile]::OpenRead($resolvedAabPath)
try {
    $nativeLibraries = @($archive.Entries | Where-Object {
        $_.FullName -match '(?i)(?:^|/)lib/[^/]+/[^/]+\.so$'
    })
    if ($nativeLibraries.Count -gt 0 -and
        -not ($nativeLibraries | Where-Object { $_.FullName -match '(?i)(?:^|/)lib/arm64-v8a/' })) {
        throw 'AAB contains native libraries but no arm64-v8a native library.'
    }
}
finally {
    $archive.Dispose()
}

$inspectionDirectory = Split-Path -Parent $InspectionPath
[System.IO.Directory]::CreateDirectory($inspectionDirectory) | Out-Null
$abiSummary = if ($nativeLibraries.Count -gt 0) {
    'Native libraries detected; arm64-v8a is present.'
}
else {
    'No native libraries were present in the AAB archive.'
}
$report = @(
    'bundletool 1.18.3 validation: PASS',
    (ConvertTo-SanitizedText $validateOutput),
    '',
    'base manifest:',
    (ConvertTo-SanitizedText $manifestOutput),
    '',
    $abiSummary
) -join [Environment]::NewLine
[System.IO.File]::WriteAllText($InspectionPath, $report)

Write-Host 'AAB validation and sanitized inspection report completed.'
