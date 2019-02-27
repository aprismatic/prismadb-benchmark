
Push-Location $PSScriptRoot

try {
    $myPublishPath = "$PSScriptRoot/PrismaDB-Plugin-MySQL"
    $myPublishPathD = "$PSScriptRoot/PrismaDB-Plugin-MySQL-debug"

    $apiUrl = 'https://ci.appveyor.com/api'
    $token = $env:AuthTokenSecure
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-type" = "application/json"
    }
    $accountName = $env:AccountNameSecure
    $projectSlugMy = 'PrismaDB-Plugin-MySQL'

    $downloadLocation = "$PSScriptRoot/Downloads"
    mkdir -Force $downloadLocation | Out-Null

    # get project with last build details
    $projectMy = Invoke-RestMethod -Method Get -Uri "$apiUrl/projects/$accountName/$projectSlugMy" -Headers $headers

    # we assume here that build has a single job
    # get this job id
    $jobIdMy = $projectMy.build.jobs[0].jobId

    # get job artifacts (just to see what we've got)
    $artifactsMy = Invoke-RestMethod -Method Get -Uri "$apiUrl/buildjobs/$jobIdMy/artifacts" -Headers $headers

    "Downloading artifacts for MySQL Plugin..."
    foreach ($artifact in $artifactsMy) {
        $artifactFileName = $artifact.fileName
        $localArtifactPath = "$downloadLocation\$artifactFileName"

        Invoke-RestMethod -Method Get -Uri "$apiUrl/buildjobs/$jobIdMy/artifacts/$artifactFileName" `
                          -OutFile $localArtifactPath -Headers @{ "Authorization" = "Bearer $token" }
    }

    "Listing downloaded artifacts:"
    ls $downloadLocation

    foreach ($file in Get-ChildItem $downloadLocation) {
        $fn = $file.Name
        $path = $file.FullName
        if ($fn -eq "PrismaDB-Plugin-MySQL.zip") {
            "Extracting $fn into $myPublishPath..."
            7z e -y $path "-o$myPublishPath"
        }
        if ($fn -eq "PrismaDB-Plugin-MySQL-debug.zip") {
            "Extracting $fn into $myPublishPathD..."
            7z e -y $path "-o$myPublishPathD"
        }
    }
}
finally {
    if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
    Pop-Location
}
