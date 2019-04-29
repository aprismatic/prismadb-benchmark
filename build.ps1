Push-Location $PSScriptRoot

$linrt = ($env:linrt, "linux-musl-x64" -ne $null)[0]

try {
  docker build  -t aprismatic.azurecr.io/prismadb-proxy-mssql:alpine `
                -f "Dockerfile-mssql-$linrt"  .
				
  docker save -o "$PSScriptRoot/prismadb-proxy-mssql.tar" "aprismatic.azurecr.io/prismadb-proxy-mssql:alpine"

  docker tag  aprismatic.azurecr.io/prismadb-proxy-mssql:alpine `
              aprismatic.azurecr.io/prismadb-proxy-mssql:latest
}
finally {
  if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
  Pop-Location
}
