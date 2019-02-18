
Push-Location $PSScriptRoot

try {
	#$base = 'https://github.com/docker/machine/releases/download/v0.16.1'
	#$DMPath = 'C:\Program Files\Docker\Docker\resources\bin'
	#curl.exe -o $DMPath\docker-machine.exe $base/docker-machine-Windows-x86_64.exe
	
    $myPublishPath = "$PSScriptRoot/Prisma-Mysql-Proxy"
    $myPublishPathP = "$myPublishPath/Plugins"

    $apiUrl = 'https://ci.appveyor.com/api'
    $token = 'obv75bsvkg8y6wjjqm2k'
    #$token = $env:AuthTokenSecure
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-type" = "application/json"
    }
    #$accountName = $env:AccountNameSecure
    $accountName = 'bazzilic'
    $projectSlugMyPeek = 'PrismaDB-Proxy-MySQL'
    $projectSlugMyPlugin = 'PrismaDB-Plugin-MySQL'

    $downloadLocation = "$PSScriptRoot/Downloads"
    mkdir -Force $downloadLocation | Out-Null

    # get project with last build details
    $projectMyPeek = Invoke-RestMethod -Method Get -Headers $headers `
									   -Uri "$apiUrl/projects/$accountName/$projectSlugMyPeek"
    $projectMyPlugin = Invoke-RestMethod -Method Get -Headers $headers `
										 -Uri "$apiUrl/projects/$accountName/$projectSlugMyPlugin"

    # we assume here that build has a single job
    # get this job id
    $jobIdMyPeek = $projectMyPeek.build.jobs[0].jobId
    $jobIdMyPlugin = $projectMyPlugin.build.jobs[0].jobId

    "Downloading artifacts for MySQL Proxy..."
	$artifactPeek = 'sqlpeek-MySQL-linux-musl-x64.zip'
	$ArtifactPathPeek = "$downloadLocation\$artifactPeek"

	Invoke-RestMethod -Method Get -Uri "$apiUrl/buildjobs/$jobIdMyPeek/artifacts/$artifactPeek" `
					  -OutFile $ArtifactPathPeek -Headers @{ "Authorization" = "Bearer $token" }
					
    "Downloading artifacts for MySQL Proxy Plugins..."					
	$artifactPlugin = 'PrismaDB-Plugin-MySQL.zip'
	$ArtifactPathPlugin = "$downloadLocation\$artifactPlugin"

	Invoke-RestMethod -Method Get -Uri "$apiUrl/buildjobs/$jobIdMyPlugin/artifacts/$artifactPlugin" `
					  -OutFile $ArtifactPathPlugin -Headers @{ "Authorization" = "Bearer $token" }
					  
    "Listing downloaded artifacts:"
    ls $downloadLocation

    foreach ($file in Get-ChildItem $downloadLocation) {
        $fn = $file.Name
        $path = $file.FullName
        if ($fn -eq $artifactPeek) {
            "Extracting $fn into $myPublishPath..."
            7z e -y $path "-o$myPublishPath"
        }
		if ($fn -eq $artifactPlugin) {
            "Extracting $fn into $myPublishPathP..."
            7z e -y $path "-o$myPublishPathP"
        }
    }
}

finally {
    if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
    Pop-Location
}
