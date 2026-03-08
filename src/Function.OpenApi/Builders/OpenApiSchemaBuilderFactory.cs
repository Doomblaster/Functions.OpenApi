// BY COPILOT
using Microsoft.OpenApi;

namespace Function.OpenApi.Builders;

internal static class OpenApiSchemaBuilderFactory
{
    public static IOpenApiSchemaBuilder Create(OpenApiDocument document, OpenApiSpecVersion specVersion)
    {
        return specVersion switch
        {
            OpenApiSpecVersion.OpenApi3_0 => new OpenApi30SchemaBuilder(document),
            OpenApiSpecVersion.OpenApi3_1 => new OpenApi31SchemaBuilder(document),
            _ => throw new NotSupportedException($"OpenAPI spec version {specVersion} is not supported.")
        };
    }
}
