Push-Location $PSScriptRoot

$linrt = ($env:linrt, "linux-musl-x64" -ne $null)[0]

try {
  docker build  --platform=linux `
                -t aprismatic.azurecr.io/prismadb-proxy-mssql:latest-alpine `
                -f "Dockerfile-mssql-$linrt"  .
}
finally {
  if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
  Pop-Location
}
