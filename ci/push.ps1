Push-Location $PSScriptRoot

$ver = $env:VERSION

docker tag prismadb.azurecr.io/benchmark:latest prismadb.azurecr.io/benchmark:alpine
docker tag prismadb.azurecr.io/benchmark:latest prismadb.azurecr.io/benchmark:$ver

docker push prismadb.azurecr.io/benchmark

Pop-Location
