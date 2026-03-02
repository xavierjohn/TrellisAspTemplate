namespace BestWeatherForecast.Api;

using System.Globalization;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

/// <summary>
/// Configure Swashbuckle with default values from the API Explorer that it doesn't automatically do
/// </summary>
/// <remarks>This <see cref="IOperationFilter"/> is only required due to bugs in the <see cref="SwaggerGenerator"/>.
/// Once they are fixed and published, this class can be removed.</remarks>
public class AddApiVersionMetadata : IOperationFilter
{
    /// <inheritdoc />
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;

        operation.Deprecated |= apiDescription.IsDeprecated();

        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/b7cf75e7905050305b115dd96640ddd6e74c7ac9/src/Swashbuckle.AspNetCore.SwaggerGen/SwaggerGenerator/SwaggerGenerator.cs#L383-L387
            var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString(CultureInfo.InvariantCulture);
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

        if (operation.Parameters == null)
        {
            return;
        }

        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413
        for (int i = 0; i < operation.Parameters.Count; i++)
        {
            var parameter = operation.Parameters[i];
            var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

            var newDescription = parameter.Description;
            if (newDescription == null)
            {
                newDescription = description.ModelMetadata?.Description;
            }

            JsonNode? newDefault = parameter.Schema?.Default;
            if (parameter.Schema?.Default == null &&
                 description.DefaultValue != null &&
                 description.DefaultValue is not DBNull &&
                 description.ModelMetadata is ModelMetadata modelMetadata)
            {
                // REF: https://github.com/Microsoft/aspnet-api-versioning/issues/429#issuecomment-605402330
                var json = JsonSerializer.Serialize(description.DefaultValue, modelMetadata.ModelType);
                newDefault = JsonNode.Parse(json);
            }

            var newRequired = parameter.Required || description.IsRequired;

            // Replace parameter if any properties changed
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
