# Dev Container Configuration

This directory contains the configuration for GitHub Codespaces and VS Code Dev Containers.

## What's Included

- **.NET 10 SDK** - Based on the global.json configuration
- **Docker-in-Docker** - For running the OpenTelemetry docker-compose stack
- **GitHub CLI** - For GitHub operations
- **VS Code Extensions**:
  - C# Dev Kit
  - C# extension
  - Docker extension
  - REST Client (for testing API endpoints)
  - EditorConfig

## Getting Started in Codespaces

1. After the codespace is created, it will automatically restore NuGet packages
2. Build the solution: `dotnet build`
3. Run tests: `./runtests.cmd` or `dotnet test`
4. Run the API: `dotnet run --project Api/src/Api.csproj`
5. (Optional) Start OpenTelemetry services: `cd DockerOpenTelemetry && docker-compose up -d`

## Port Forwarding

The following ports are configured to be forwarded:
- **5000** - API (HTTP)
- **5001** - API (HTTPS)
- **9090** - Prometheus (for metrics)
- **9411** - Zipkin (for traces)

## Testing the API

Once the API is running, you can:
- Use the `.http` files in the Api/src directory with the REST Client extension
- Access the Swagger UI at the forwarded port 5000 or 5001
- View metrics at Prometheus (port 9090)
- View traces at Zipkin (port 9411)
