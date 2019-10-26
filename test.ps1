Push-Location $PSScriptRoot

try {
    $DigitalOceanToken = $env:DOTokenSecure
    $DockerMachine = 'PrismaDB-Benchmark-Test'
    
    dotnet restore PrismaBenchmark
    dotnet publish PrismaBenchmark -c Release -o PrismaBenchmark/out

    docker-machine create --driver digitalocean --digitalocean-access-token $DigitalOceanToken `
        --digitalocean-region='nyc3' --digitalocean-size='c-32' $DockerMachine
	
    docker-machine ls
    docker-machine env --shell powershell $DockerMachine
    docker-machine env --shell powershell $DockerMachine | Invoke-Expression
    #see which is active
    docker-machine active
	
    docker load -i "$PSScriptRoot/prismadb-proxy-mssql.tar"
	
    docker-compose up -d prismadb prismaproxy
    docker-compose up --exit-code-from prismabenchmark --build prismabenchmark
}
finally {
    if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
    Pop-Location
}
