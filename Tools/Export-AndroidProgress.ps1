param(
    [Parameter(Mandatory=$true)][string]$DeviceId,
    [string]$OutFile = "$PSScriptRoot\legacy-progress.xml",
    [switch]$PushToUnity
)
$ErrorActionPreference='Stop'
# The original release is debuggable. Android package isolation prevents Unity
# from opening another app's private SharedPreferences directly.
$adbPath=(Get-Command adb -ErrorAction Stop).Source
$originalPackage='com.example.spatialsurvivor'
$unityPackage='com.example.spatialsurvivor.unity'
$preferenceNames=& $adbPath -s $DeviceId shell run-as $originalPackage ls shared_prefs
if($LASTEXITCODE -ne 0){throw 'Cannot read original debug package. Keep the original installed and enable device debugging.'}
$preferenceFile=@($preferenceNames | Where-Object { $_.Trim() -eq 'spatial_survivor_progression.xml' })
if($preferenceFile.Count -ne 1){throw "Expected one permanent progression preferences file; found $($preferenceFile.Count)."}
$preferenceXml=& $adbPath -s $DeviceId shell run-as $originalPackage cat "shared_prefs/$($preferenceFile[0].Trim())"
if($LASTEXITCODE -ne 0){throw 'Original preference export failed.'}
$exportText=$preferenceXml -join "`n"
if($exportText -notmatch 'total_crystals'){throw 'Preference export did not contain total_crystals.'}
[IO.File]::WriteAllText([IO.Path]::GetFullPath($OutFile),$exportText,[Text.UTF8Encoding]::new($false))
Write-Output "Exported original progress: $OutFile"
if($PushToUnity){
    $destination="/sdcard/Android/data/$unityPackage/files"
    & $adbPath -s $DeviceId shell mkdir -p $destination
    if($LASTEXITCODE -ne 0){throw 'Cannot prepare Unity save folder. Launch the Unity app once first.'}
    & $adbPath -s $DeviceId push $OutFile "$destination/legacy-progress.xml"
    if($LASTEXITCODE -ne 0){throw 'Cannot copy original progress to Unity.'}
    Write-Output 'In the Unity main menu, use Permanent Progression -> Import Original Progress.'
}
