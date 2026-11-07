param(
    [string]$UnityEditor = "D:\Would\Unity\6000.6.8f1\Editor\Unity.exe",
    [string]$ProjectPath = (Split-Path $PSScriptRoot -Parent)
)
$ErrorActionPreference = 'Stop'
$editorFile = (Resolve-Path -LiteralPath $UnityEditor).Path
$projectFolder = (Resolve-Path -LiteralPath $ProjectPath).Path
if (-not $env:ALLUSERSPROFILE) { $env:ALLUSERSPROFILE = $env:ProgramData }
# This launcher intentionally shows the editor requested by the user.
Start-Process -FilePath $editorFile -WorkingDirectory $projectFolder -ArgumentList @('-projectPath', ('"' + $projectFolder + '"'), '-force-d3d11')
