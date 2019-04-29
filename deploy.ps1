docker push aprismatic.azurecr.io/prismadb-benchmark

if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }