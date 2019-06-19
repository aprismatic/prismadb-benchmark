Push-Location $PSScriptRoot

try {
    $DockerMachine = 'PrismaDB-Benchmark-Test'

    docker-machine stop $DockerMachine
    Write-Output "y" | docker-machine rm $DockerMachine
}
finally {
    if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
    Pop-Location
}
