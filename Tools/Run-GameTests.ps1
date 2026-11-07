param(
    [string]$UnityEditor = "D:\Would\Unity\6000.6.8f1\Editor\Unity.exe",
    [string]$ProjectPath = (Split-Path $PSScriptRoot -Parent)
)
$ErrorActionPreference = 'Stop'
$editorPath = (Resolve-Path -LiteralPath $UnityEditor).Path
$projectFolder = (Resolve-Path -LiteralPath $ProjectPath).Path
$logFolder = Join-Path $projectFolder 'Logs'
New-Item -ItemType Directory -Force -Path $logFolder | Out-Null
# Some automation hosts omit this Windows environment value. Set it only in
# the current process so Unity Package Manager can locate its config folder.
if (-not $env:ALLUSERSPROFILE) { $env:ALLUSERSPROFILE = $env:ProgramData }
$arguments = @(
    '-batchmode', '-force-d3d11',
    '-projectPath', ('"' + $projectFolder + '"'),
    '-runTests', '-testPlatform', 'EditMode',
    '-assemblyNames', 'SpatialSurvivor.Tests',
    '-testResults', ('"' + (Join-Path $logFolder 'game-tests.xml') + '"'),
    '-logFile', ('"' + (Join-Path $logFolder 'rewrite-tests.log') + '"')
)
$process = Start-Process -FilePath $editorPath -WorkingDirectory $projectFolder -WindowStyle Hidden -ArgumentList $arguments -PassThru -Wait
Write-Output "Test result: $(Join-Path $logFolder 'game-tests.xml')"
exit $process.ExitCode
