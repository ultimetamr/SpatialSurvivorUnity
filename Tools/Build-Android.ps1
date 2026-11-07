param(
    [string]$UnityEditor = "D:\Would\Unity\6000.6.8f1\Editor\Unity.exe",
    [string]$ProjectPath = (Split-Path $PSScriptRoot -Parent)
)
$ErrorActionPreference = 'Stop'
$editorFile = (Resolve-Path -LiteralPath $UnityEditor).Path
$projectFolder = (Resolve-Path -LiteralPath $ProjectPath).Path
$logFolder = Join-Path $projectFolder 'Logs'
$socketFolder = Join-Path $logFolder 'java-sockets'
New-Item -ItemType Directory -Force -Path $socketFolder | Out-Null
if (-not $env:ALLUSERSPROFILE) { $env:ALLUSERSPROFILE = $env:ProgramData }
# The bundled JDK cannot create NIO loopback pipes in this host's short TEMP
# path. Use a project-local socket directory only for this build process.
$previousTemp = $env:TEMP
$previousTmp = $env:TMP
try {
    $env:TEMP = $socketFolder
    $env:TMP = $socketFolder
    $arguments = @('-batchmode', '-quit', '-force-d3d11', '-projectPath', ('"' + $projectFolder + '"'), '-buildTarget', 'Android', '-executeMethod', 'SpatialSurvivor.Editor.SurvivorProject.BuildAndroid', '-logFile', ('"' + (Join-Path $logFolder 'unity6-android-build.log') + '"'))
    $buildProcess = Start-Process -FilePath $editorFile -WorkingDirectory $projectFolder -WindowStyle Hidden -ArgumentList $arguments -PassThru -Wait
    if ($buildProcess.ExitCode -ne 0) { throw "Unity Android build failed with exit code $($buildProcess.ExitCode). See Logs/unity6-android-build.log." }
    Write-Output (Join-Path $projectFolder 'Builds/SpatialSurvivorUnity.apk')
} finally {
    $env:TEMP = $previousTemp
    $env:TMP = $previousTmp
}
