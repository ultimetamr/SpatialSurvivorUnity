param(
    [string]$PicoSdkPath,
    [string]$ProjectPath = (Split-Path $PSScriptRoot -Parent)
)
$ErrorActionPreference = 'Stop'
$projectFolder = (Resolve-Path -LiteralPath $ProjectPath).Path
$destination = Join-Path $projectFolder 'Packages/com.bytedance.pico.xr'
if (-not (Test-Path -LiteralPath (Join-Path $destination 'package.json'))) {
    if (-not $PicoSdkPath) { throw 'Download PICO Unity SDK 6.1.1 under its license, extract the com.bytedance.pico.xr package, then pass -PicoSdkPath to that package directory.' }
    $source = (Resolve-Path -LiteralPath $PicoSdkPath).Path
    $package = Get-Content -LiteralPath (Join-Path $source 'package.json') -Raw | ConvertFrom-Json
    if ($package.name -ne 'com.bytedance.pico.xr' -or $package.version -ne '6.1.1') {
        throw 'This project requires the com.bytedance.pico.xr package at version 6.1.1.'
    }
    Copy-Item -LiteralPath $source -Destination $destination -Recurse
}
$installed = Get-Content -LiteralPath (Join-Path $destination 'package.json') -Raw | ConvertFrom-Json
if ($installed.version -ne '6.1.1') { throw 'Installed PICO SDK must be 6.1.1.' }
# Apply the project's two small SDK compatibility guards to the local copy.
$patches = @(
    @{ File = 'Editor/PXR_Utils.cs'; Method = 'public static void SetDefaultDefineSymbols()' },
    @{ File = 'Editor/PXR_PortalEditor.cs'; Method = 'private static void InitializeOnLoad()' }
)
foreach ($patch in $patches) {
    $file = Join-Path $destination $patch.File
    $text = [IO.File]::ReadAllText($file)
    $pattern = '(' + [regex]::Escape($patch.Method) + '\s*\{\s*)'
    $match = [regex]::Match($text, $pattern)
    if (-not $match.Success) { throw ('SDK patch location missing: ' + $patch.File) }
    $following = $text.Substring($match.Index + $match.Length)
    if (-not $following.StartsWith('if (AssetDatabase.IsAssetImportWorkerProcess()) return;')) {
        $text = $text.Insert($match.Index + $match.Length, "if (AssetDatabase.IsAssetImportWorkerProcess()) return;`n            ")
        [IO.File]::WriteAllText($file, $text, [Text.UTF8Encoding]::new($false))
    }
}
$font = Join-Path $projectFolder 'Assets/Resources/Fonts/Chinese.ttf'
if (-not (Test-Path -LiteralPath $font)) {
    $openFont = Join-Path $projectFolder 'PublicDependencies/Fonts/NotoSansSC-VF.ttf'
    if (-not (Test-Path -LiteralPath $openFont)) { throw 'Bundled Noto Sans SC font is missing.' }
    Copy-Item -LiteralPath $openFont -Destination $font
    $meta = $font + '.meta'
    $text = [IO.File]::ReadAllText($meta).Replace('- SimHei', '- Noto Sans SC')
    [IO.File]::WriteAllText($meta, $text, [Text.UTF8Encoding]::new($false))
}
Write-Output 'PICO SDK compatibility guards and public font are ready. Open the project with Unity 6000.6.8f1.'
