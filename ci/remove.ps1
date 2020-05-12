Push-Location $PSScriptRoot

try {
    $dockerMachine = "prismadb-benchmark-test"

    docker-machine stop $dockerMachine
    Write-Output "y" | docker-machine rm $dockerMachine
}
catch { }
finally {
    Pop-Location
}
