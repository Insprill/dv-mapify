param (
    [switch]$NoArchive,
    [string]$OutputDirectory = $PSScriptRoot
)

Set-Location "$PSScriptRoot"

$DistDir = "$OutputDirectory/dist"
if ($NoArchive) {
    $ZipWorkDir = "$OutputDirectory"
} else {
    $ZipWorkDir = "$DistDir/tmp"
}
$ZipRootDir = "$ZipWorkDir/BepInEx"
$ZipInnerDir = "$ZipRootDir/plugins/Mapify/"
$RuntimeBuildDir = "build/runtime"
$LicenseFile = "LICENSE"
$LocaleFile = "locale.csv"
$MapifyDll = "$RuntimeBuildDir/Mapify.dll"
$MapifyEditorDll = "$RuntimeBuildDir/MapifyEditor.dll"

New-Item "$ZipInnerDir" -ItemType Directory -Force
Copy-Item -Force -Path $LicenseFile, $LocaleFile, $MapifyDll, $MapifyEditorDll -Destination $ZipInnerDir

if (!$NoArchive)
{
    $VERSION = (Select-String -Pattern '([0-9]+\.[0-9]+\.[0-9]+)' -Path Mapify/Mapify.cs).Matches.Value
    $FILE_NAME = "$DistDir/Mapify_v$VERSION.zip"
    Compress-Archive -Update -CompressionLevel Fastest -Path "$ZipRootDir" -DestinationPath "$FILE_NAME"
}
