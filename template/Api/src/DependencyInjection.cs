namespace BestWeatherForecast.Api;

using BestWeatherForecast.Api.Middleware;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using ServiceLevelIndicators;
using Scalar.AspNetCore;
using Trellis.Asp;

internal static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.ConfigureOpenTelemetry();
        services.ConfigureServiceLevelIndicators();
        services.AddProblemDetails();
        services.AddControllers().AddScalarValueValidation();
        services.AddApiVersioning()
                .AddMvc()
                .AddApiExplorer()
                .AddOpenApi(options => options.Document.AddScalarTransformers());

        // Register OpenAPI keyed services in the standard DI container.
        // Asp.Versioning.OpenApi's WithDocumentPerVersion() uses an internal KeyedServiceContainer
        // whose fallback path throws when a key isn't found (preview bug). Registering documents
        // here lets MapOpenApi() resolve directly from the standard DI container instead.
        // Add one call per API version; the versioning PostConfigure adds per-version transformers.
        services.AddOpenApi("2023-06-06");

        services.AddScoped<ErrorHandlingMiddleware>();
        return services;
    }

    private static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services)
    {
        static void configureResource(ResourceBuilder r) => r.AddService(
            serviceName: "BestWeatherForecastService",
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
                builder.AddResultsInstrumentation();
                builder.AddPrimitiveValueObjectInstrumentation();
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
            options.LocationId = ServiceLevelIndicator.CreateLocationId("public", "westus3");
        })
        .AddMvc()
        .AddApiVersion();

        return services;
    }
}
