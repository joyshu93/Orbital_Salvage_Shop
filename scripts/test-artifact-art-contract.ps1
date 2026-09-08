param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot)
)

$ErrorActionPreference = 'Stop'
$catalogPath = Join-Path $ProjectRoot 'Assets\Scripts\Runtime\Content\ContentCatalog.cs'
$artRoot = Join-Path $ProjectRoot 'Assets\Resources\Art\Artifacts'

if (-not (Test-Path -LiteralPath $catalogPath)) {
    throw "Artifact catalog was not found at $catalogPath"
}

$catalog = Get-Content -LiteralPath $catalogPath -Raw
$artifactIds = @(
    [regex]::Matches($catalog, 'A\("(?<id>[a-z0-9-]+)"') |
        ForEach-Object { $_.Groups['id'].Value }
)

if ($artifactIds.Count -ne 24) {
    throw "Expected 24 artifact IDs in ContentCatalog, found $($artifactIds.Count)."
}

Add-Type -AssemblyName System.Drawing
$problems = [System.Collections.Generic.List[string]]::new()

foreach ($artifactId in $artifactIds) {
    $imagePath = Join-Path $artRoot "$artifactId.png"
    if (-not (Test-Path -LiteralPath $imagePath)) {
        $problems.Add("Missing artifact art: $artifactId.png")
        continue
    }

    $bitmap = $null
    try {
        $bitmap = [System.Drawing.Bitmap]::FromFile($imagePath)
        if ($bitmap.Width -ne $bitmap.Height) {
            $problems.Add("Artifact art must be square: $artifactId.png is $($bitmap.Width)x$($bitmap.Height)")
        }

        if ($bitmap.Width -lt 512) {
            $problems.Add("Artifact art is too small: $artifactId.png is $($bitmap.Width)x$($bitmap.Height)")
        }

        $cornerAlpha = @(
            $bitmap.GetPixel(0, 0).A,
            $bitmap.GetPixel($bitmap.Width - 1, 0).A,
            $bitmap.GetPixel(0, $bitmap.Height - 1).A,
            $bitmap.GetPixel($bitmap.Width - 1, $bitmap.Height - 1).A
        )
        if (@($cornerAlpha | Where-Object { $_ -gt 1 }).Count -gt 0) {
            $problems.Add("Artifact art must have effectively transparent corners: $artifactId.png")
        }
    }
    catch {
        $problems.Add("Artifact art is not a readable PNG: $artifactId.png ($($_.Exception.Message))")
    }
    finally {
        if ($null -ne $bitmap) {
            $bitmap.Dispose()
        }
    }
}

if ($problems.Count -gt 0) {
    throw ($problems -join [Environment]::NewLine)
}

Write-Host "Artifact art contract passed: $($artifactIds.Count)/$($artifactIds.Count) square transparent sprites."
