@echo off
echo Starting Aspire Dashboard...
echo.
echo Dashboard UI:  http://localhost:18888
echo OTLP endpoint: http://localhost:4317 (gRPC)
echo.
docker run --rm -it -d -p 18888:18888 -p 4317:18889 -e ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true --name aspire-dashboard mcr.microsoft.com/dotnet/aspire-dashboard:latest
echo.
echo Dashboard started. Open http://localhost:18888 in your browser.
