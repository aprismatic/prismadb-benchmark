Push-Location $PSScriptRoot

try {

	$DigitalOceanToken = $env:DOTokenSecure
	$DockerMachine = 'BenchmarkTest'

	docker-machine create --driver digitalocean --digitalocean-access-token $DigitalOceanToken `
						--digitalocean-region='sgp1' --digitalocean-size='s-4vcpu-8gb' $DockerMachine

	docker-machine env --shell powershell $DockerMachine
	docker-machine env --shell powershell $DockerMachine | Invoke-Expression

	$blockRdp = $true
	iex ((new-object net.webclient).DownloadString('https://raw.githubusercontent.com/appveyor/ci/master/scripts/enable-rdp.ps1'))
	
	docker-compose up -d --build prismadb prismaproxy
	docker-compose up --build prismabenchmark
	
	docker-machine stop $DockerMachine
	echo "y" | docker-machine rm $DockerMachine
	
}
finally {
	if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
	Pop-Location
}
