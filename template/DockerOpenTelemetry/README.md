# Aspire Dashboard (Standalone)

The [Aspire Dashboard](https://aspire.dev/dashboard/standalone/) provides a unified UI for traces, metrics, and structured logs — no configuration files needed.

## Start the Dashboard

```powershell
docker run --rm -it -d -p 18888:18888 -p 4317:18889 --name aspire-dashboard mcr.microsoft.com/dotnet/aspire-dashboard:latest
```

| Port | Purpose |
|------|---------|
| `18888` | Dashboard UI — open http://localhost:18888 |
| `4317` | OTLP gRPC receiver — apps send telemetry here |

## Login

The dashboard requires a token on first visit. Get it from the container logs:

```powershell
$loginLine = docker container logs aspire-dashboard | Select-String "login\?t="
$matches = [regex]::Match($loginLine, "(?<=login\?t=)(\S+)")
$matches.Value | Set-Clipboard
echo $matches.Value
```

Paste the token into the browser login page.

To skip authentication (local dev only):

```powershell
docker run --rm -it -d -p 18888:18888 -p 4317:18889 -e ASPIRE_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS=true --name aspire-dashboard mcr.microsoft.com/dotnet/aspire-dashboard:latest
```

## Configure Your App

Set these environment variables (or configure in `appsettings.Development.json`):

```
OTEL_EXPORTER_OTLP_ENDPOINT=http://localhost:4317
OTEL_EXPORTER_OTLP_PROTOCOL=grpc
```

## Stop

```powershell
docker stop aspire-dashboard
```
