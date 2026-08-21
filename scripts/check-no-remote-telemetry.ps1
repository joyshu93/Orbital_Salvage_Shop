$ErrorActionPreference = 'Stop'

$projectRoot = [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$failures = [System.Collections.Generic.List[string]]::new()

function Add-Failure {
    param([string]$Message)

    $failures.Add($Message)
}

function Read-JsonFile {
    param([string]$RelativePath)

    $path = Join-Path $projectRoot $RelativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "Missing required JSON file: $RelativePath"
        return $null
    }

    try {
        return Get-Content -LiteralPath $path -Raw | ConvertFrom-Json
    }
    catch {
        Add-Failure "Invalid JSON in ${RelativePath}: $($_.Exception.Message)"
        return $null
    }
}

$manifest = Read-JsonFile 'Packages/manifest.json'
if ($null -ne $manifest -and $null -ne $manifest.dependencies) {
    foreach ($dependency in $manifest.dependencies.PSObject.Properties) {
        if ($dependency.Name -like 'com.google.firebase.*') {
            Add-Failure "Firebase package dependency remains in Packages/manifest.json: $($dependency.Name)"
        }
    }
}

$packageLockPath = Join-Path $projectRoot 'Packages/packages-lock.json'
if (Test-Path -LiteralPath $packageLockPath -PathType Leaf) {
    $packageLock = Read-JsonFile 'Packages/packages-lock.json'
    if ($null -ne $packageLock -and $null -ne $packageLock.dependencies) {
        foreach ($dependency in $packageLock.dependencies.PSObject.Properties) {
            if ($dependency.Name -like 'com.google.firebase.*') {
                Add-Failure "Firebase package dependency remains in Packages/packages-lock.json: $($dependency.Name)"
            }
        }
    }
}

$runtimeAsmdef = Read-JsonFile 'Assets/Scripts/Runtime/CurioClerk.Runtime.asmdef'
if ($null -ne $runtimeAsmdef) {
    $allowedPrecompiledReferences = @('GoogleMobileAds.dll', 'GoogleMobileAds.Ump.dll')
    foreach ($reference in @($runtimeAsmdef.precompiledReferences)) {
        if ($reference -like 'Firebase.*') {
            Add-Failure "Firebase precompiled reference remains in CurioClerk.Runtime.asmdef: $reference"
        }
        elseif ($reference -notin $allowedPrecompiledReferences) {
            Add-Failure "Unexpected runtime precompiled reference could add a remote transport: $reference"
        }
    }
}

$forbiddenRuntimePaths = @(
    'Assets/Scripts/Runtime/Infrastructure/Firebase.meta',
    'Assets/Scripts/Runtime/Infrastructure/Analytics/FirebaseAnalyticsService.cs',
    'Assets/Scripts/Runtime/Infrastructure/Analytics/FirebaseAnalyticsService.cs.meta',
    'Assets/Scripts/Runtime/Infrastructure/Diagnostics/FirebaseCrashReporter.cs',
    'Assets/Scripts/Runtime/Infrastructure/Diagnostics/FirebaseCrashReporter.cs.meta'
)

$firebaseRuntimeDirectory = Join-Path $projectRoot 'Assets/Scripts/Runtime/Infrastructure/Firebase'
if (Test-Path -LiteralPath $firebaseRuntimeDirectory -PathType Container) {
    $firebaseRuntimeEntries = @(Get-ChildItem -LiteralPath $firebaseRuntimeDirectory -Recurse -Force)
    if ($firebaseRuntimeEntries.Count -gt 0) {
        Add-Failure 'Firebase runtime adapter directory contains shipping files: Assets/Scripts/Runtime/Infrastructure/Firebase'
    }
}

foreach ($relativePath in $forbiddenRuntimePaths) {
    if (Test-Path -LiteralPath (Join-Path $projectRoot $relativePath)) {
        Add-Failure "Firebase runtime adapter path remains: $relativePath"
    }
}

$localServicePaths = @(
    'Assets/Scripts/Runtime/Infrastructure/Analytics/ConsentAwareAnalyticsService.cs',
    'Assets/Scripts/Runtime/Infrastructure/Diagnostics/ConsentAwareCrashReporter.cs'
)
$transportOrPersistenceMarkers = @(
    'UnityEngine',
    'System.IO',
    'System.Net',
    'HttpClient',
    'UnityWebRequest',
    'Debug.',
    'PlayerPrefs',
    'File.'
)
foreach ($relativePath in $localServicePaths) {
    $path = Join-Path $projectRoot $relativePath
    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        Add-Failure "Missing local non-transport service: $relativePath"
        continue
    }

    $content = Get-Content -LiteralPath $path -Raw
    foreach ($marker in $transportOrPersistenceMarkers) {
        if ($content.Contains($marker)) {
            Add-Failure "Local telemetry service contains transport/persistence marker '$marker': $relativePath"
        }
    }
}

$runtimeRoot = Join-Path $projectRoot 'Assets/Scripts/Runtime'
if (Test-Path -LiteralPath $runtimeRoot -PathType Container) {
    foreach ($source in Get-ChildItem -LiteralPath $runtimeRoot -Recurse -File -Filter '*.cs') {
        $content = Get-Content -LiteralPath $source.FullName -Raw
        if ($content -match '(?m)^\s*using\s+(global::)?Firebase(?:\.|\s*;)' -or
            $content -match '\bFirebaseApp\b|\bFirebaseAnalytics\b|\bCrashlytics\b') {
            $relative = [System.IO.Path]::GetRelativePath($projectRoot, $source.FullName).Replace('\', '/')
            Add-Failure "Firebase SDK symbol remains in shipping runtime source: $relative"
        }
    }
}

$vendoredRoot = Join-Path $projectRoot 'GooglePackages'
if (Test-Path -LiteralPath $vendoredRoot -PathType Container) {
    foreach ($archive in Get-ChildItem -LiteralPath $vendoredRoot -Recurse -File -Filter 'com.google.firebase*.tgz') {
        $relative = [System.IO.Path]::GetRelativePath($projectRoot, $archive.FullName).Replace('\', '/')
        Add-Failure "Vendored Firebase package remains: $relative"
    }
}

$androidManifestPath = Join-Path $projectRoot 'Assets/Plugins/Android/AndroidManifest.xml'
if (Test-Path -LiteralPath $androidManifestPath -PathType Leaf) {
    try {
        [xml]$androidManifest = Get-Content -LiteralPath $androidManifestPath -Raw
        $androidNamespace = 'http://schemas.android.com/apk/res/android'
        $firebaseMetadata = @($androidManifest.manifest.application.'meta-data' | Where-Object {
            $_.GetAttribute('name', $androidNamespace) -like 'firebase_*'
        })
        if ($firebaseMetadata.Count -gt 0) {
            Add-Failure 'Firebase-only Android manifest metadata remains in Assets/Plugins/Android/AndroidManifest.xml.'
        }
    }
    catch {
        Add-Failure "Invalid Android manifest XML: $($_.Exception.Message)"
    }
}

if ($failures.Count -gt 0) {
    foreach ($failure in $failures) {
        Write-Error $failure -ErrorAction Continue
    }

    throw "No-remote-telemetry gate failed with $($failures.Count) violation(s)."
}

Write-Host 'No-remote-telemetry gate passed.'
