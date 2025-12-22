namespace BestWeatherForecast.Api;

using Azure.Core;
using BestWeatherForecast.AntiCorruptionLayer;
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
    public static IServiceCollection AddPresentation(this IServiceCollection services, EnvironmentOptions environmentOptions)
    {
        services.ConfigureOpenTelemetry(environmentOptions);
        services.ConfigureServiceLevelIndicators(environmentOptions);
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
        _2023_06_06.Models.ConfigureMapster.Config();
        return services;
    }

    private static IServiceCollection ConfigureOpenTelemetry(this IServiceCollection services, EnvironmentOptions environmentOptions)
    {
        void configureResource(ResourceBuilder r) => r.AddService(
            serviceName: environmentOptions.ServiceName,
            serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString() ?? "unknown");

        services.AddOpenTelemetry()
            .ConfigureResource(configureResource)
            .WithMetrics(builder =>
            {
                builder.AddAspNetCoreInstrumentation();
                builder.AddMeter(
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

        return services;
    }

    private static IServiceCollection ConfigureServiceLevelIndicators(this IServiceCollection services, EnvironmentOptions environmentOptions)
    {
        services.AddServiceLevelIndicator(options =>
        {
            options.LocationId = ServiceLevelIndicator.CreateLocationId("public", environmentOptions.Region);
        })
        .AddMvc()
        .AddApiVersion();

        return services;
    }
}
