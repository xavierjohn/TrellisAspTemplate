using BestWeatherForecast.AntiCorruptionLayer;
using BestWeatherForecast.Api;
using BestWeatherForecast.Api.Middleware;
using BestWeatherForecast.Application;
using Scalar.AspNetCore;
using ServiceLevelIndicators;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddPresentation()
    .AddApplication()
    .AddAntiCorruptionLayer();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapSwagger("/openapi/{documentName}.json");
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Best Weather Forecast API");
        var descriptions = app.DescribeApiVersions();

        // Add all API version documents to Scalar
        foreach (var description in descriptions)
        {
            options.AddDocument(description.GroupName, $"/openapi/{description.GroupName}.json");
        }
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseServiceLevelIndicator();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapControllers();

app.Run();

/// <summary>
/// Main entry point for the application.
/// </summary>
public partial class Program
{
}
