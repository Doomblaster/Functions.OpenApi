// BY COPILOT
using Microsoft.OpenApi;
using System.Reflection;

namespace Function.OpenApi.Builders;

/// <summary>
/// Defines schema generation for a specific OpenAPI specification version.
/// </summary>
internal interface IOpenApiSchemaBuilder
{
    /// <summary>
    /// The OpenAPI spec version this builder targets.
    /// </summary>
    OpenApiSpecVersion SpecVersion { get; }

    /// <summary>
    /// Builds a schema for a type and adds it to the internal cache.
    /// </summary>
    OpenApiSchema BuildComponentSchema(Type type);

    /// <summary>
    /// Builds a schema for a property, respecting nullability.
    /// </summary>
    OpenApiSchema BuildSchemaFromPropertyInfo(PropertyInfo property);

    /// <summary>
    /// Flushes all cached schemas to the document's Components.Schemas.
    /// </summary>
    void FlushToDocument();
}
