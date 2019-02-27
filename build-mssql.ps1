Push-Location $PSScriptRoot

$winrt = ($env:winrt, "win-x64" -ne $null)[0]
$linrt = ($env:linrt, "linux-musl-x64" -ne $null)[0]

try {
  if ($env:APPVEYOR -eq $true) { Switch-DockerWindows }
  else { & $Env:ProgramFiles\Docker\Docker\DockerCli.exe -SwitchWindowsEngine }

  docker build  --platform=windows `
                -t aprismatic.azurecr.io/prismadb-proxy-mssql:latest-win-sac2016 `
                -f "Dockerfile-mssql-$winrt"  .

  docker build  --platform=windows `
                -t aprismatic.azurecr.io/prismadb-proxy-mssql:latest-win-sac2016-debug `
                -f "Dockerfile-mssql-$winrt-debug"  .

  if ($env:APPVEYOR -eq $true) { Switch-DockerLinux }
  else { & $Env:ProgramFiles\Docker\Docker\DockerCli.exe -SwitchLinuxEngine }

  docker build  --platform=linux `
                -t aprismatic.azurecr.io/prismadb-proxy-mssql:latest-alpine `
                -f "Dockerfile-mssql-$linrt"  .

  docker build  --platform=linux `
                -t aprismatic.azurecr.io/prismadb-proxy-mssql:latest-alpine-debug `
                -f "Dockerfile-mssql-$linrt-debug"  .
				
  docker save -o prismadb-proxy-mssql.tar aprismatic.azurecr.io/prismadb-proxy-mssql:latest-alpine
}
finally {
  if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
  Pop-Location
}
