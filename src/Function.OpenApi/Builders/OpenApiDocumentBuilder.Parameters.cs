// BY COPILOT
using Microsoft.OpenApi;
using System.Reflection;

namespace Function.OpenApi.Builders;

public partial class OpenApiDocumentBuilder
{
    private void AddRouteParameters(OpenApiPathItem pathItem, IEnumerable<ParameterInfo> routeParameters)
    {
        var operations = pathItem.Operations ??= [];
        foreach (var parameter in routeParameters)
        {
            _ = _schemaBuilder.BuildComponentSchema(parameter.ParameterType);

            var friendlyName = OpenApiSchemaBuilderBase.GetFriendlyFullName(parameter.ParameterType);
            var parameterName = parameter.Name
                ?? throw new InvalidOperationException("Route parameter name cannot be null.");
            var componentParameterName = $"{friendlyName}_{parameterName}";
            var reference = new OpenApiSchemaReference(friendlyName, _document);
            var parameterReference = new OpenApiParameterReference(componentParameterName, _document);
            var openApiParameter = new OpenApiParameter
            {
                Name = parameterName,
                Required = true,
                In = ParameterLocation.Path,
                Schema = reference
            };
            foreach (var operation in operations.Values)
            {
                operation.Parameters ??= [];
                operation.Parameters.Add(parameterReference);
            }
            GetParameters().TryAdd(componentParameterName, openApiParameter);
        }
    }

    private void AddHeaderParameters(OpenApiPathItem pathItem, IEnumerable<OpenApiHeaderAttribute> headerAttributes)
    {
        var operations = pathItem.Operations ??= [];
        foreach (var headerAttribute in headerAttributes.Where(h => h.Direction == OpenApiHeaderDirection.In))
        {
            if (string.IsNullOrWhiteSpace(headerAttribute.Name))
                throw new InvalidOperationException("Header name cannot be null or empty.");

            _ = _schemaBuilder.BuildComponentSchema(headerAttribute.HeaderType);
            var friendlyName = OpenApiSchemaBuilderBase.GetFriendlyFullName(headerAttribute.HeaderType);
            var parameterName = $"{friendlyName}_{headerAttribute.Name}_header";
            var reference = new OpenApiSchemaReference(friendlyName, _document);
            var parameterReference = new OpenApiParameterReference(parameterName, _document);
            var openApiParameter = new OpenApiParameter
            {
                Name = headerAttribute.Name,
                Required = headerAttribute.Required,
                In = ParameterLocation.Header,
                Schema = reference
            };
            foreach (var operation in operations.Values)
            {
                operation.Parameters ??= [];
                operation.Parameters.Add(parameterReference);
            }
            GetParameters().TryAdd(parameterName, openApiParameter);
        }
    }
}
