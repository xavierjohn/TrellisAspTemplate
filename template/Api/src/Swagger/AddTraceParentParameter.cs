namespace BestWeatherForecast.Api;

using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

/// <summary>
/// Configure Swashbuckle with optional trace parent.
/// </summary>
public class AddTraceParentParameter : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters?.Add(new OpenApiParameter
        {
            Name = "traceparent",
            In = ParameterLocation.Header,
            Description = "HTTP header field identifies the incoming request in a tracing system",
            Required = false
        });
    }
}
