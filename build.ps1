Push-Location $PSScriptRoot

$linrt = ($env:linrt, "linux-musl-x64" -ne $null)[0]

try {
  docker build  --platform=linux `
                -t aprismatic.azurecr.io/prismadb-proxy-mssql:alpine `
                -f "Dockerfile-mssql-$linrt"  .
				
  docker save -o "$PSScriptRoot/prismadb-proxy-mssql.tar" "aprismatic.azurecr.io/prismadb-proxy-mssql:alpine"
}
finally {
  if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
  Pop-Location
}
