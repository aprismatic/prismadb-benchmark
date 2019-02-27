Push-Location $PSScriptRoot

$winrt = ($env:winrt, "win-x64" -ne $null)[0]
$linrt = ($env:linrt, "linux-musl-x64" -ne $null)[0]

try {
  if ($env:APPVEYOR -eq $true) { Switch-DockerWindows }
  else { & $Env:ProgramFiles\Docker\Docker\DockerCli.exe -SwitchWindowsEngine }

  docker build  --platform=windows `
                -t aprismatic.azurecr.io/prismadb-proxy-mysql:latest-win-sac2016 `
                -f "Dockerfile-mysql-$winrt"  .

  docker build  --platform=windows `
                -t aprismatic.azurecr.io/prismadb-proxy-mysql:latest-win-sac2016-debug `
                -f "Dockerfile-mysql-$winrt-debug"  .
  
  if ($env:APPVEYOR -eq $true) { Switch-DockerLinux }
  else { & $Env:ProgramFiles\Docker\Docker\DockerCli.exe -SwitchLinuxEngine }

  docker build  --platform=linux `
                -t aprismatic.azurecr.io/prismadb-proxy-mysql:latest-alpine `
                -f "Dockerfile-mysql-$linrt"  .
  
  docker build  --platform=linux `
                -t aprismatic.azurecr.io/prismadb-proxy-mysql:latest-alpine-debug `
                -f "Dockerfile-mysql-$linrt-debug"  .
}
finally {
  if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
  Pop-Location
}
