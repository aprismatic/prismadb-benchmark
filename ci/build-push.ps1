Push-Location $PSScriptRoot

$ver = $env:VERSION

docker build -t prismadb.azurecr.io/benchmark:$ver-alpine -f Dockerfile-Benchmark ../publish

docker tag prismadb.azurecr.io/benchmark:$ver-alpine prismadb.azurecr.io/benchmark:latest
docker tag prismadb.azurecr.io/benchmark:$ver-alpine prismadb.azurecr.io/benchmark:alpine
docker tag prismadb.azurecr.io/benchmark:$ver-alpine prismadb.azurecr.io/benchmark:$ver

docker push prismadb.azurecr.io/benchmark

Pop-Location
