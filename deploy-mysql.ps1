if ($env:APPVEYOR -eq $true) { Switch-DockerWindows }
else { & $Env:ProgramFiles\Docker\Docker\DockerCli.exe -SwitchWindowsEngine }

docker images

docker push aprismatic.azurecr.io/prismadb-proxy-mysql:latest-win-sac2016
docker push aprismatic.azurecr.io/prismadb-proxy-mysql:latest-win-sac2016-debug

if ($env:APPVEYOR -eq $true) { Switch-DockerLinux }
else { & $Env:ProgramFiles\Docker\Docker\DockerCli.exe -SwitchLinuxEngine }

docker images

docker push aprismatic.azurecr.io/prismadb-proxy-mysql:latest-alpine
docker push aprismatic.azurecr.io/prismadb-proxy-mysql:latest-alpine-debug

if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }