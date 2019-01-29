Push-Location $PSScriptRoot

try {
  docker-compose up -d --build prismadb prismaproxy
  docker-compose up --build prismabenchmark
}
finally {
  if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }
  Pop-Location
}
