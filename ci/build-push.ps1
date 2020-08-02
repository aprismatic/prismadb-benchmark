Push-Location $PSScriptRoot

$ver = $env:VERSION

docker build -t aprismatic.azurecr.io/prismadb-benchmark:$ver-alpine -f Dockerfile-Benchmark ../publish

docker tag aprismatic.azurecr.io/prismadb-benchmark:$ver-alpine aprismatic.azurecr.io/prismadb-benchmark:latest
docker tag aprismatic.azurecr.io/prismadb-benchmark:$ver-alpine aprismatic.azurecr.io/prismadb-benchmark:alpine
docker tag aprismatic.azurecr.io/prismadb-benchmark:$ver-alpine aprismatic.azurecr.io/prismadb-benchmark:$ver

docker push aprismatic.azurecr.io/prismadb-benchmark

Pop-Location
