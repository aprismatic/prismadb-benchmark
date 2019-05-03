
Push-Location $PSScriptRoot

try {
    $myPublishPath = "$PSScriptRoot/PrismaDB-Proxy-MySQL"
    $downloadLocation = "$PSScriptRoot/Downloads"
    $artifactFileName = "prismadb-proxy-test-4jg8dfk3.zip"
    $localArtifactPath = "$downloadLocation\$artifactFileName"

    Invoke-RestMethod -Method Get -Uri "http://aoa.cczy.my/stuff/$artifactFileName" `
                          -OutFile $localArtifactPath

    "Listing downloaded artifacts:"
    Get-ChildItem $downloadLocation

    foreach ($file in Get-ChildItem $downloadLocation) {
        $fn = $file.Name
        $path = $file.FullName
        if ($fn -eq $artifactFileName) {
            "Extracting $fn into $myPublishPath..."
            7z e -y $path "-o$myPublishPath"
        }
    }
}
finally {
    if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
    Pop-Location
}
