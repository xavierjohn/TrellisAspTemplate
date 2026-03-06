using BestWeatherForecast.AntiCorruptionLayer;
using BestWeatherForecast.Api;
using BestWeatherForecast.Api.Middleware;
using BestWeatherForecast.Application;
using ServiceLevelIndicators;
using Trellis.Asp;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services
    .AddPresentation()
    .AddApplication()
    .AddAntiCorruptionLayer();

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
app.UseScalarValueValidation();
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
