# Infrastructure & Setup Patterns

> **This document describes infrastructure setup patterns from the FunctionalDddAspTemplate.**
>
> Covers DependencyInjection, Program.cs, API versioning, OpenTelemetry, and Service Level Indicators.

---

## ?? **Dependency Injection Setup**

Each layer has a `DependencyInjection.cs` with extension methods for `IServiceCollection`.

### **Application Layer**

```csharp
namespace BestWeatherForecast.Application;

using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
        return services;
    }
}
```

**Pattern:**
- ? Static class with extension method
- ? Returns `IServiceCollection` for chaining
- ? Registers Mediator for CQRS

### **ACL Layer**

```csharp
namespace BestWeatherForecast.AntiCorruptionLayer;

using Microsoft.Extensions.DependencyInjection;
using BestWeatherForecast.Application.Abstractions;

public static class DependencyInjection
{
    public static IServiceCollection AddAntiCorruptionLayer(this IServiceCollection services)
    {
        services.AddScoped<IWeatherForecastService, WeatherForecastService>();
        // Add other repositories and services
        return services;
    }
}
```

**Pattern:**
- ? Registers implementations of Application abstractions
- ? Use `AddScoped` for repositories (per-request lifetime)
- ? Use `AddHttpClient` for external API clients

### **API Layer**

```csharp
namespace BestWeatherForecast.Api;

using Azure.Core;
using BestWeatherForecast.Api.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ServiceLevelIndicators;
using Swashbuckle.AspNetCore.SwaggerGen;

internal static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.ConfigureOpenTelemetry();
        services.ConfigureServiceLevelIndicators();
        services.AddProblemDetails();
        services.AddControllers();
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen(
            options =>
            {
                options.OperationFilter<AddApiVersionMetadata>();
                options.OperationFilter<AddTraceParentParameter>();

                var fileName = typeof(Program).Assembly.GetName().Name + ".xml";
                var filePath = Path.Combine(AppContext.BaseDirectory, fileName);

                // integrate xml comments
                options.IncludeXmlComments(filePath);
            });
        services.AddApiVersioning()
                .AddMvc()
                .AddApiExplorer();
        services.AddScoped<ErrorHandlingMiddleware>();
        
        // Configure Mapster for all API versions
        _2023_06_06.Models.ConfigureMapster.Config();
        
        return services;
    }

    private static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services)
    {
        static void configureResource(ResourceBuilder r) => r.AddService(
            serviceName: "BestWeatherForcastService",
            serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown");

        services.AddOpenTelemetry()
            .ConfigureResource(configureResource)
            .WithMetrics(builder =>
            {
                builder.AddAspNetCoreInstrumentation();
                builder.AddMeter(
                    ApiMeters.MeterName,
                    "Microsoft.AspNetCore.Hosting",
                    "Microsoft.AspNetCore.Server.Kestrel",
                    "System.Net.Http");
                builder.AddOtlpExporter();
            })
            .WithTracing(builder =>
            {
                builder.AddAspNetCoreInstrumentation();
                builder.AddOtlpExporter();
            });

        services.AddSingleton<ApiMeters>();
        return services;
    }

    private static IServiceCollection ConfigureServiceLevelIndicators(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<ServiceLevelIndicatorOptions>, ConfigureServiceLevelIndicatorOptions>());
        services.AddServiceLevelIndicator(options =>
        {
            options.LocationId = ServiceLevelIndicator.CreateLocationId("public", AzureLocation.WestUS3.Name);
        })
        .AddMvc()
        .AddApiVersion();

        return services;
    }
}
```

**Key Components:**

1. **AddPresentation** - Main registration method
   - OpenTelemetry configuration
   - Service Level Indicators
   - Problem Details support
   - Controllers
   - Swagger/OpenAPI
   - API Versioning
   - Error handling middleware
   - Mapster configuration

2. **ConfigureOpenTelemetry** - Observability setup
   - Service name and version
   - Metrics collection
   - Distributed tracing
   - OTLP exporter

3. **ConfigureServiceLevelIndicators** - SLI metrics
   - Location identification
   - MVC integration
   - API version tracking

---

## ?? **Program.cs Setup**

```csharp
using BestWeatherForecast.AntiCorruptionLayer;
using BestWeatherForecast.Api;
using BestWeatherForecast.Api.Middleware;
using BestWeatherForecast.Application;
using ServiceLevelIndicators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services
    .AddPresentation()      // API layer services
    .AddApplication()       // CQRS and Mediator
    .AddAntiCorruptionLayer();  // Repositories and external services

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(
        options =>
        {
            options.RoutePrefix = string.Empty; // make home page the swagger UI
            var descriptions = app.DescribeApiVersions();

            // build a swagger endpoint for each discovered API version
            foreach (var description in descriptions)
            {
                var url = $"/swagger/{description.GroupName}/swagger.json";
                var name = description.GroupName.ToUpperInvariant();
                options.SwaggerEndpoint(url, name);
            }
        });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseServiceLevelIndicator();  // SLI middleware
app.UseMiddleware<ErrorHandlingMiddleware>();  // Global error handling
app.MapControllers();

app.Run();

/// <summary>
/// Main entry point for the application.
/// </summary>
public partial class Program
{
}
```

**Pattern:**
- ? Chain `Add*` methods for each layer
- ? Swagger UI on root path in Development
- ? Dynamic Swagger endpoints for all API versions
- ? Service Level Indicator middleware
- ? Custom error handling middleware
- ? Expose `Program` class for integration tests

---

## ?? **API Versioning Configuration**

### **ConfigureSwaggerOptions.cs**

```csharp
namespace BestWeatherForecast.Api;

using System.Text;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider provider) 
        => this.provider = provider;

    public void Configure(SwaggerGenOptions options)
    {
        // add a swagger document for each discovered API version
        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
        }
    }

    private static OpenApiInfo CreateInfoForApiVersion(ApiVersionDescription description)
    {
        var text = new StringBuilder("API documentation with versioning.");
        var info = new OpenApiInfo()
        {
            Title = "My API",
            Version = description.ApiVersion.ToString(),
            Contact = new OpenApiContact() { Name = "Your Name", Email = "you@example.com" },
            License = new OpenApiLicense() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
        };

        if (description.IsDeprecated)
        {
            text.Append(" This API version has been deprecated.");
        }

        if (description.SunsetPolicy is SunsetPolicy policy)
        {
            if (policy.Date is DateTimeOffset when)
            {
                text.Append(" The API will be sunset on ")
                    .Append(when.Date.ToShortDateString())
                    .Append('.');
            }

            if (policy.HasLinks)
            {
                text.AppendLine();

                for (var i = 0; i < policy.Links.Count; i++)
                {
                    var link = policy.Links[i];

                    if (link.Type == "text/html")
                    {
                        text.AppendLine();

                        if (link.Title.HasValue)
                        {
                            text.Append(link.Title.Value).Append(": ");
                        }

                        text.Append(link.LinkTarget.OriginalString);
                    }
                }
            }
        }

        info.Description = text.ToString();

        return info;
    }
}
```

**Pattern:**
- ? Implements `IConfigureOptions<SwaggerGenOptions>`
- ? Creates one Swagger document per API version
- ? Handles deprecated versions and sunset policies
- ? Registered in DependencyInjection: `services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>()`

---

## ?? **Service Level Indicators**

### **ApiMeters.cs**

```csharp
namespace BestWeatherForecast.Api;

using System.Diagnostics.Metrics;

public sealed class ApiMeters
{
    public const string MeterName = "BestWeatherForecast.Api";

    public ApiMeters(IMeterFactory meterFactory)
    {
        Meter = meterFactory.Create(MeterName);
    }

    public Meter Meter { get; }
}
```

**Pattern:**
- ? Singleton service for metrics
- ? Meter name matches API assembly name
- ? Registered in DependencyInjection: `services.AddSingleton<ApiMeters>()`

### **ConfigureServiceLevelIndicatorOptions.cs**

```csharp
namespace BestWeatherForecast.Api;

using Microsoft.Extensions.Options;
using ServiceLevelIndicators;

internal sealed class ConfigureServiceLevelIndicatorOptions : IConfigureOptions<ServiceLevelIndicatorOptions>
{
    private readonly ApiMeters meters;

    public ConfigureServiceLevelIndicatorOptions(ApiMeters meters) => this.meters = meters;

    public void Configure(ServiceLevelIndicatorOptions options) => options.Meter = meters.Meter;
}
```

**Pattern:**
- ? Configures SLI to use custom meter
- ? Registered using `TryAddEnumerable`

---

## ?? **Swagger Operation Filters**

### **AddApiVersionMetadata.cs**

Operation filter to add API version metadata to Swagger documentation.

```csharp
namespace BestWeatherForecast.Api;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

public class AddApiVersionMetadata : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;

        operation.Deprecated |= apiDescription.IsDeprecated();

        // Add response type metadata
        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            var responseKey = responseType.IsDefaultResponse 
                ? "default" 
                : responseType.StatusCode.ToString(CultureInfo.InvariantCulture);
            var response = operation.Responses?[responseKey];
            if (response == null) continue;

            foreach (var contentType in response.Content?.Keys ?? Enumerable.Empty<string>())
            {
                if (!responseType.ApiResponseFormats.Any(x => x.MediaType == contentType))
                {
                    response.Content?.Remove(contentType);
                }
            }
        }

        // Add parameter metadata (default values, descriptions, etc.)
        if (operation.Parameters == null) return;

        for (int i = 0; i < operation.Parameters.Count; i++)
        {
            var parameter = operation.Parameters[i];
            var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

            var newDescription = parameter.Description ?? description.ModelMetadata?.Description;
            
            JsonNode? newDefault = parameter.Schema?.Default;
            if (parameter.Schema?.Default == null &&
                 description.DefaultValue != null &&
                 description.DefaultValue is not DBNull &&
                 description.ModelMetadata is ModelMetadata modelMetadata)
            {
                var json = JsonSerializer.Serialize(description.DefaultValue, modelMetadata.ModelType);
                newDefault = JsonNode.Parse(json);
            }

            var newRequired = parameter.Required || description.IsRequired;

            if (newDescription != parameter.Description ||
                !Equals(newDefault, parameter.Schema?.Default) ||
                newRequired != parameter.Required)
            {
                operation.Parameters[i] = new OpenApiParameter
                {
                    Name = parameter.Name,
                    In = parameter.In,
                    Description = newDescription,
                    Required = newRequired,
                    Schema = parameter.Schema == null ? null : new OpenApiSchema
                    {
                        Type = parameter.Schema.Type,
                        Format = parameter.Schema.Format,
                        Default = newDefault,
                        Enum = parameter.Schema.Enum
                    },
                    Style = parameter.Style,
                    Explode = parameter.Explode
                };
            }
        }
    }
}
```

**Pattern:**
- ? Fixes Swashbuckle bugs with API versioning
- ? Adds default values, descriptions, and required status
- ? Registered in Swagger configuration: `options.OperationFilter<AddApiVersionMetadata>()`

### **AddTraceParentParameter.cs**

Operation filter to add `traceparent` header for distributed tracing.

```csharp
namespace BestWeatherForecast.Api.Swagger;

using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public class AddTraceParentParameter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "traceparent",
            In = ParameterLocation.Header,
            Description = "W3C Trace Context traceparent header for distributed tracing",
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = "string",
                Example = new OpenApiString("00-0af7651916cd43dd8448eb211c80319c-b7ad6b7169203331-01")
            }
        });
    }
}
```

**Pattern:**
- ? Adds `traceparent` header to all operations
- ? Supports W3C Trace Context for distributed tracing
- ? Registered in Swagger configuration: `options.OperationFilter<AddTraceParentParameter>()`

---

## ??? **Error Handling Middleware**

```csharp
namespace BestWeatherForecast.Api.Middleware;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;

public class ErrorHandlingMiddleware : IMiddleware
{
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(ILogger<ErrorHandlingMiddleware> logger)
        => _logger = logger;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred");

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "An error occurred while processing your request",
                status = 500,
                traceId = context.TraceIdentifier
            });
        }
    }
}
```

**Pattern:**
- ? Implements `IMiddleware` (scoped lifetime)
- ? Catches unhandled exceptions
- ? Returns RFC 7807 Problem Details
- ? Logs errors with trace ID
- ? Registered in DependencyInjection: `services.AddScoped<ErrorHandlingMiddleware>()`
- ? Used in Program.cs: `app.UseMiddleware<ErrorHandlingMiddleware>()`

---

## ? **Checklist for New Projects**

### **DependencyInjection.cs Files**

- [ ] **Application**: `AddApplication()` with Mediator registration
- [ ] **ACL**: `AddAntiCorruptionLayer()` with repository registrations
- [ ] **API**: `AddPresentation()` with all infrastructure

### **Program.cs**

- [ ] Chain `Add*` methods for each layer
- [ ] Configure Swagger UI with dynamic versioning
- [ ] Add `UseServiceLevelIndicator()` middleware
- [ ] Add `UseMiddleware<ErrorHandlingMiddleware>()`
- [ ] Expose `Program` class for integration tests

### **Swagger Configuration**

- [ ] `ConfigureSwaggerOptions.cs` for version documentation
- [ ] `AddApiVersionMetadata` operation filter
- [ ] `AddTraceParentParameter` operation filter
- [ ] XML comments enabled

### **Observability**

- [ ] OpenTelemetry configured with metrics and tracing
- [ ] Service Level Indicators enabled
- [ ] `ApiMeters` class for custom metrics
- [ ] OTLP exporter configured

### **Error Handling**

- [ ] `ErrorHandlingMiddleware` implemented
- [ ] Problem Details support enabled
- [ ] Logging configured

---

## ?? **Reference Links**

- **ServiceLevelIndicators**: https://github.com/xavierjohn/ServiceLevelIndicators
- **OpenTelemetry .NET**: https://github.com/open-telemetry/opentelemetry-dotnet
- **Asp.Versioning**: https://github.com/dotnet/aspnet-api-versioning
- **RFC 7807 Problem Details**: https://tools.ietf.org/html/rfc7807
