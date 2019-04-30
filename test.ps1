Push-Location $PSScriptRoot

try {
    $DigitalOceanToken = $env:DOTokenSecure
    $DockerMachine = 'prismadb-benchmarktest'
	
    Set-Location PrismaBenchmark | dotnet restore | dotnet publish -c Release -o out

    docker-machine create --driver azure --azure-subscription-id $DigitalOceanToken `
        --azure-location='northcentralus' --azure-size='Standard_F16s_v2' `
        --azure-resource-group='PrismaDB-Benchmark' --azure-subnet='prismadb-benchmark' `
        --azure-vnet='prismadb-benchmark' $DockerMachine
	
    docker-machine ls
    docker-machine env --shell powershell $DockerMachine
    docker-machine env --shell powershell $DockerMachine | Invoke-Expression
    #see which is active
    docker-machine active
	
    docker load -i "$PSScriptRoot/prismadb-proxy-mssql.tar"
	
    docker-compose up -d --no-color prismadb prismaproxy
    docker-compose up --no-color --exit-code-from prismabenchmark --build prismabenchmark
}
finally {
    if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
    Pop-Location
}
