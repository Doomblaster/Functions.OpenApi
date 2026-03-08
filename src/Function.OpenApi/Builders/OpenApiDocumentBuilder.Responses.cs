// BY COPILOT
using Microsoft.OpenApi;
using System.Reflection;

namespace Function.OpenApi.Builders;

public partial class OpenApiDocumentBuilder
{
    private void AddResponses(
        OpenApiOperation operation,
        IEnumerable<OpenApiResponseAttribute> responseAttributes,
        IEnumerable<OpenApiHeaderAttribute> headerAttributes)
    {
        foreach (var responseAttribute in responseAttributes)
        {
            if (string.IsNullOrWhiteSpace(responseAttribute.ContentType))
                throw new InvalidOperationException("Response content type cannot be null or empty.");

            var responseHeaders = headerAttributes
                .Where(h => h.Direction == OpenApiHeaderDirection.Out)
                .Where(h => h.ResponseStatusCode == default || h.ResponseStatusCode == (int)responseAttribute.HttpStatusCode)
                .ToList();

            _ = _schemaBuilder.BuildComponentSchema(responseAttribute.BodyType);

            var mediaType = new OpenApiMediaType
            {
                Schema = new OpenApiSchemaReference(OpenApiSchemaBuilderBase.GetFriendlyFullName(responseAttribute.BodyType), _document)
            };
            var response = new OpenApiResponse
            {
                Description = string.Empty,
                Content = new Dictionary<string, IOpenApiMediaType>()
            };

            if (responseHeaders.Count > 0)
            {
                var responseHeadersMap = response.Headers ?? new Dictionary<string, IOpenApiHeader>();
                foreach (var headerAttribute in responseHeaders)
                {
                    if (string.IsNullOrWhiteSpace(headerAttribute.Name))
                        throw new InvalidOperationException("Header name cannot be null or empty.");

                    _ = _schemaBuilder.BuildComponentSchema(headerAttribute.HeaderType);
                    var friendlyName = OpenApiSchemaBuilderBase.GetFriendlyFullName(headerAttribute.HeaderType);
                    var schemaReference = new OpenApiSchemaReference(friendlyName, _document);
                    responseHeadersMap[headerAttribute.Name] = new OpenApiHeader
                    {
                        Schema = schemaReference,
                        Required = headerAttribute.Required
                    };
                }
                response.Headers = responseHeadersMap;
            }

            response.Content.Add(responseAttribute.ContentType, mediaType);
            operation.Responses ??= [];
            operation.Responses.Add(((int)responseAttribute.HttpStatusCode).ToString(), response);
        }
    }

    private void AddRequestBody(OpenApiPathItem pathItem, MethodInfo method)
    {
        var requestBodyAttribute = method.GetCustomAttribute<OpenApiRequestBodyAttribute>();
        if (requestBodyAttribute is null)
            return;

        if (string.IsNullOrWhiteSpace(requestBodyAttribute.ContentType))
            throw new InvalidOperationException("Request body content type cannot be null or empty.");

        _ = _schemaBuilder.BuildComponentSchema(requestBodyAttribute.BodyType);

        var requestBodyName = OpenApiSchemaBuilderBase.GetFriendlyFullName(requestBodyAttribute.BodyType);
        var mediaType = new OpenApiMediaType
        {
            Schema = new OpenApiSchemaReference(requestBodyName, _document)
        };
        var openapiRequestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, IOpenApiMediaType>()
        };
        openapiRequestBody.Content.Add(requestBodyAttribute.ContentType, mediaType);
        GetRequestBodies().TryAdd(requestBodyName, openapiRequestBody);

        var operations = pathItem.Operations ??= [];
        foreach (var operation in operations.Values)
        {
            operation.RequestBody = new OpenApiRequestBodyReference(requestBodyName, _document);
        }
    }
}
