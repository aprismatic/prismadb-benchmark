Push-Location $PSScriptRoot

$ver = $env:VERSION

docker build -t aprismatic/prismadb-benchmark:$ver-alpine -f Dockerfile-Benchmark ../publish

docker tag aprismatic/prismadb-benchmark:$ver-alpine aprismatic/prismadb-benchmark:latest
docker tag aprismatic/prismadb-benchmark:$ver-alpine aprismatic/prismadb-benchmark:alpine
docker tag aprismatic/prismadb-benchmark:$ver-alpine aprismatic/prismadb-benchmark:$ver

docker push aprismatic/prismadb-benchmark

Pop-Location
