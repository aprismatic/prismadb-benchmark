Push-Location $PSScriptRoot

$DigitalOceanToken = $env:DO_TOKEN
$dockerMachine = "prismadb-benchmark-test"

docker-machine create --driver digitalocean --digitalocean-access-token $DigitalOceanToken `
    --digitalocean-region='sfo2' --digitalocean-size='c-32' $dockerMachine
	
docker-machine ls
docker-machine env --shell powershell $dockerMachine
docker-machine env --shell powershell $dockerMachine | Invoke-Expression
#see which is active
docker-machine active
	
docker-compose -f docker-compose.yml up -d prismadb prismaproxy
Start-Sleep -s 300
docker-compose -f docker-compose.yml up --exit-code-from prismabenchmark prismabenchmark

Pop-Location
