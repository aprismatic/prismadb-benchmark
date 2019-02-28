docker push aprismatic.azurecr.io/prismadb-benchmark-test:latest-alpine

if ($LastExitCode -ne 0) { $host.SetShouldExit($LastExitCode) }