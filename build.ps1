# Stop executing script if a cmdlet fails
$ErrorActionPreference = "Stop"

$publishFolder = "Publish"

Write-Output "`nDeleting existing Publish folder..."
if (Test-Path $publishFolder) {
    Remove-Item $publishFolder -Recurse -Force
}

$publishProfiles = Get-ChildItem "Properties/PublishProfiles" -Filter *.pubxml
foreach ($file in $publishProfiles) {
    $profileName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
    Write-Output "`nStarting build for $profileName..."
    dotnet publish AssEmbly.csproj -p:PublishProfile="Properties/PublishProfiles/$profileName.pubxml" -p:TreatWarningsAsErrors=true -warnaserror
    
    if ($LastExitCode -ne 0) {
        exit $LastExitCode
    }
}

Write-Output ""
$subFolders = Get-ChildItem $publishFolder -Directory
foreach ($folder in $subFolders) {
    $zipName = "AssEmbly-" + $folder.Name + ".zip"
    Write-Output "Compressing into $zipName..."
    $zipPath = Join-Path $publishFolder $zipName
    Get-ChildItem -Path $folder -Exclude "*.pdb" |
        Compress-Archive -DestinationPath $zipPath -CompressionLevel Optimal
}

Write-Output "`nBuilding documentation..."
.\build-docs.ps1
$docsZipPath = Join-Path $publishFolder "Documentation.zip"
Write-Output "`nCompressing documentation..."
Compress-Archive -Path "Documentation\ReferenceManual\ReferenceManual.*" -DestinationPath $docsZipPath -CompressionLevel Optimal

Write-Output "Done."
