# Windows containers paused until AppVeyor updates to Windows Server newer than 2016
if ($env:APPVEYOR -ne $true) {
  if ($env:APPVEYOR -eq $true) { Switch-DockerWindows }
  else { & $Env:ProgramFiles\Docker\Docker\DockerCli.exe -SwitchWindowsEngine }

  docker images

  docker push aprismatic.azurecr.io/sqlpeek-mssql:latest-win-1803
  docker push aprismatic.azurecr.io/sqlpeek-mysql:latest-win-1803
}

if ($env:APPVEYOR -eq $true) { Switch-DockerLinux }
else { & $Env:ProgramFiles\Docker\Docker\DockerCli.exe -SwitchLinuxEngine }

docker images

docker push aprismatic.azurecr.io/sqlpeek-mssql:latest-alpine
docker push aprismatic.azurecr.io/sqlpeek-mysql:latest-alpine

if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }