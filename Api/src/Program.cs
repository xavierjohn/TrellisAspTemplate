using BestWeatherForecast.AntiCorruptionLayer;
using BestWeatherForecast.Api;
using BestWeatherForecast.Api.Middleware;
using BestWeatherForecast.Application;
using ServiceLevelIndicators;

var builder = WebApplication.CreateBuilder(args);

// Load EnvironmentOptions early for use during setup
Program.EnvironmentOptions = builder.Configuration
    .GetSection(nameof(EnvironmentOptions))
    .Get<EnvironmentOptions>() ?? new EnvironmentOptions();

// Add services to the container.

builder.Services
    .AddPresentation(Program.EnvironmentOptions)
    .AddApplication()
    .AddAntiCorruptionLayer(Program.EnvironmentOptions);

var app = builder.Build();

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
app.UseServiceLevelIndicator();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.MapControllers();

app.Run();

/// <summary>
/// Main entry point for the application.
/// </summary>
public partial class Program
{
    internal static EnvironmentOptions EnvironmentOptions { get; set; } = new();
}
