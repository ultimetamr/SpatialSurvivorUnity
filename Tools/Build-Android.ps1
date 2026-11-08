param(
    [string]$UnityEditor = "D:\Would\Unity\6000.6.8f1\Editor\Unity.exe",
    [string]$ProjectPath = (Split-Path $PSScriptRoot -Parent),
    [string]$AndroidSdk = ''
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
    # An incomplete bundled SDK can trigger a stalled automatic download.
    # Prefer an existing complete SDK for this build, restoring editor settings afterwards.
    $embeddedSdk = Join-Path (Split-Path $editorFile -Parent) 'Data\PlaybackEngines\AndroidPlayer\SDK'
    if (-not $AndroidSdk -and -not (Test-Path -LiteralPath (Join-Path $embeddedSdk 'platform-tools\adb.exe'))) {
        foreach ($candidateSdk in @($env:ANDROID_SDK_ROOT, $env:ANDROID_HOME, (Join-Path $env:LOCALAPPDATA 'Android\Sdk'))) {
            if ($candidateSdk -and (Test-Path -LiteralPath (Join-Path $candidateSdk 'platform-tools\adb.exe')) -and (Test-Path -LiteralPath (Join-Path $candidateSdk 'build-tools'))) { $AndroidSdk = $candidateSdk; break }
        }
    }
    if ($AndroidSdk) {
        $resolvedSdk = (Resolve-Path -LiteralPath $AndroidSdk).Path
        if (-not (Test-Path -LiteralPath (Join-Path $resolvedSdk 'cmake\3.22.1\bin\cmake.exe')) -and (Test-Path -LiteralPath (Join-Path $embeddedSdk 'cmake\3.22.1\bin\cmake.exe'))) {
            $overlaySdk = Join-Path $logFolder 'android-sdk-overlay'
            New-Item -ItemType Directory -Force -Path $overlaySdk | Out-Null
            foreach ($sdkPart in @('platform-tools','build-tools','platforms','cmdline-tools','licenses','cmake')) {
                $partSource = Join-Path $(if ($sdkPart -eq 'cmake') {$embeddedSdk} else {$resolvedSdk}) $sdkPart
                $partLink = Join-Path $overlaySdk $sdkPart
                if (-not (Test-Path -LiteralPath $partSource)) { throw "Required Android SDK directory missing: $partSource" }
                if (Test-Path -LiteralPath $partLink) {
                    $existingLink = Get-Item -LiteralPath $partLink
                    if ($existingLink.LinkType -ne 'Junction' -or $existingLink.Target -ne $partSource) { throw "Existing SDK overlay target differs: $partLink" }
                } else { New-Item -ItemType Junction -Path $partLink -Target $partSource | Out-Null }
            }
            $resolvedSdk = $overlaySdk
        }
        $methodIndex = [Array]::IndexOf($arguments, 'SpatialSurvivor.Editor.SurvivorProject.BuildAndroid')
        $arguments[$methodIndex] = 'SpatialSurvivor.Editor.SurvivorProject.BuildAndroidWithSdk'
        $arguments += @('-survivorAndroidSdk', ('"' + $resolvedSdk + '"'))
        Write-Output "Using existing Android SDK: $resolvedSdk"
    }
    $buildProcess = Start-Process -FilePath $editorFile -WorkingDirectory $projectFolder -WindowStyle Hidden -ArgumentList $arguments -PassThru
    # Wait only for Unity; a reusable Gradle daemon can outlive a successful build.
    $buildProcess.WaitForExit()
    if ($buildProcess.ExitCode -ne 0) { throw "Unity Android build failed with exit code $($buildProcess.ExitCode). See Logs/unity6-android-build.log." }
    Write-Output (Join-Path $projectFolder 'Builds/SpatialSurvivorUnity.apk')
} finally {
    $env:TEMP = $previousTemp
    $env:TMP = $previousTmp
}
