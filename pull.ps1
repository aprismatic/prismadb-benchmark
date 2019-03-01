
Push-Location $PSScriptRoot

try {
    $msPublishPath = "$PSScriptRoot/PrismaDB-Plugin-MSSQL"

    $apiUrl = 'https://ci.appveyor.com/api'
    $token = $env:AuthTokenSecure
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-type" = "application/json"
    }
    $accountName = $env:AccountNameSecure
    $projectSlugMS = 'PrismaDB-Plugin-MSSQL'

    $downloadLocation = "$PSScriptRoot/Downloads"
    mkdir -Force $downloadLocation | Out-Null

    # get project with last build details
    $projectMS = Invoke-RestMethod -Method Get -Uri "$apiUrl/projects/$accountName/$projectSlugMS" -Headers $headers

    # we assume here that build has a single job
    # get this job id
    $jobIdMS = $projectMS.build.jobs[0].jobId

    # get job artifacts (just to see what we've got)
    $artifactsMS = Invoke-RestMethod -Method Get -Uri "$apiUrl/buildjobs/$jobIdMS/artifacts" -Headers $headers

    "Downloading artifacts for MSSQL Plugin..."
    foreach ($artifact in $artifactsMS) {
        $artifactFileName = $artifact.fileName
        $localArtifactPath = "$downloadLocation\$artifactFileName"

        Invoke-RestMethod -Method Get -Uri "$apiUrl/buildjobs/$jobIdMS/artifacts/$artifactFileName" `
                          -OutFile $localArtifactPath -Headers @{ "Authorization" = "Bearer $token" }
    }

    "Listing downloaded artifacts:"
    Get-ChildItem $downloadLocation

    foreach ($file in Get-ChildItem $downloadLocation) {
        $fn = $file.Name
        $path = $file.FullName
        if ($fn -eq "PrismaDB-Plugin-MSSQL.zip") {
            "Extracting $fn into $msPublishPath..."
            7z e -y $path "-o$msPublishPath"
            Remove-Item $msPublishPath\sni.dll
        }
    }
}
finally {
    if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
    Pop-Location
}
