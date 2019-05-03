Push-Location $PSScriptRoot

$linrt = ($env:linrt, "linux-musl-x64" -ne $null)[0]

try {
  docker build  -t aprismatic.azurecr.io/prismadb-proxy-mysql:alpine `
                -f "Dockerfile-mysql-$linrt"  .
				
  docker save -o "$PSScriptRoot/prismadb-proxy-mysql.tar" "aprismatic.azurecr.io/prismadb-proxy-mysql:alpine"

  docker tag  aprismatic.azurecr.io/prismadb-proxy-mysql:alpine `
              aprismatic.azurecr.io/prismadb-proxy-mysql:latest
}
finally {
  if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
  Pop-Location
}
