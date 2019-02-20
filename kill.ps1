Push-Location $PSScriptRoot

docker-machine stop $DockerMachine
echo "y" | docker-machine rm $DockerMachine