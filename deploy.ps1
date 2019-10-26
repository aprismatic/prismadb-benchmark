docker push prismadb.azurecr.io/benchmark

if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }