
Push-Location $PSScriptRoot

try {
    $msPublishPath = "$PSScriptRoot/Prisma-MSSQL-Proxy"
    $msPublishPathP = "$msPublishPath/Plugins"

    $apiUrl = 'https://ci.appveyor.com/api'
    $token = $env:AuthTokenSecure
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-type" = "application/json"
    }
    $accountName = $env:AccountNameSecure
    $projectSlugMSPeek = 'PrismaDB-Proxy'
    $projectSlugMSPlugin = 'PrismaDB-Plugin-MSSQL'

    $downloadLocation = "$PSScriptRoot/Downloads"
    mkdir -Force $downloadLocation | Out-Null

    # get project with last build details
    $projectMSPeek = Invoke-RestMethod -Method Get -Headers $headers `
                                       -Uri "$apiUrl/projects/$accountName/$projectSlugMSPeek"
    $projectMSPlugin = Invoke-RestMethod -Method Get -Headers $headers `
                                         -Uri "$apiUrl/projects/$accountName/$projectSlugMSPlugin"

    # we assume here that build has a single job
    # get this job id
    $jobIdMSPeek = $projectMSPeek.build.jobs[0].jobId
    $jobIdMSPlugin = $projectMSPlugin.build.jobs[0].jobId

    "Downloading artifacts for MSSQL Proxy..."
    $artifactPeek = 'sqlpeek-MSSQL-linux-musl-x64.zip'
    $ArtifactPathPeek = "$downloadLocation\$artifactPeek"

    Invoke-RestMethod -Method Get -Uri "$apiUrl/buildjobs/$jobIdMSPeek/artifacts/$artifactPeek" `
                      -OutFile $ArtifactPathPeek -Headers @{ "Authorization" = "Bearer $token" }

    "Downloading artifacts for MSSQL Proxy Plugins..."                  
    $artifactPlugin = 'PrismaDB-Plugin-MSSQL.zip'
    $ArtifactPathPlugin = "$downloadLocation\$artifactPlugin"

    Invoke-RestMethod -Method Get -Uri "$apiUrl/buildjobs/$jobIdMSPlugin/artifacts/$artifactPlugin" `
                      -OutFile $ArtifactPathPlugin -Headers @{ "Authorization" = "Bearer $token" }

    "Listing downloaded artifacts:"
    ls $downloadLocation

    foreach ($file in Get-ChildItem $downloadLocation) {
        $fn = $file.Name
        $path = $file.FullName
        if ($fn -eq $artifactPeek) {
            "Extracting $fn into $msPublishPath..."
            7z e -y $path "-o$msPublishPath"
        }
        if ($fn -eq $artifactPlugin) {
            "Extracting $fn into $msPublishPathP..."
            7z e -y $path "-o$msPublishPathP"
			rm $msPublishPathP\sni.dll
        }
    }
}

finally {
    if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
    Pop-Location
}
