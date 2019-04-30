Push-Location $PSScriptRoot

try {
    $DigitalOceanToken = $env:DOTokenSecure
    $DockerMachine = 'PrismaDB-BenchmarkTest'
	
    Set-Location PrismaBenchmark | dotnet restore | dotnet publish -c Release -o out

    docker-machine create --driver digitalocean --digitalocean-access-token $DigitalOceanToken `
        --digitalocean-region='nyc3' --digitalocean-size='c-32' $DockerMachine
	
    docker-machine ls
    docker-machine env --shell powershell $DockerMachine
    docker-machine env --shell powershell $DockerMachine | Invoke-Expression
    #see which is active
    docker-machine active
	
    docker load -i "$PSScriptRoot/prismadb-proxy-mssql.tar"
	
    docker-compose up -d --build --exit-code-from prismabenchmark prismadb prismaproxy prismabenchmark
    docker-compose logs -f prismabenchmark
}
finally {
    if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
    Pop-Location
}
