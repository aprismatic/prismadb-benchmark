docker push aprismatic.azurecr.io/prismadb-benchmark:alpine

if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }