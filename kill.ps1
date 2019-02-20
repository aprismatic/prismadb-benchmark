Push-Location $PSScriptRoot

$DockerMachine = 'BenchmarkTest'

docker-machine stop $DockerMachine
echo "y" | docker-machine rm $DockerMachine