Push-Location $PSScriptRoot

try {

	$DigitalOceanToken = $env:DOTokenSecure
	$DockerMachine = "BenchmarkTest"

	docker-machine create --driver digitalocean --digitalocean-access-token $DigitalOceanToken `
						--digitalocean-region sgp1 --digitalocean-size s-4vcpu-8gb $DockerMachine

	docker-machine env $DockerMachine | Invoke-Expression

	docker-compose up -d --build prismadb prismaproxy
	docker-compose up --build prismabenchmark
	
	docker-machine stop $DockerMachine
	echo "y" | docker-machine rm $DockerMachine
	
}
finally {
	if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
	Pop-Location
}
