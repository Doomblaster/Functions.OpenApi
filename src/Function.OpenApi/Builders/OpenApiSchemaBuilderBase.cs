// BY COPILOT
using Microsoft.OpenApi;
using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Function.OpenApi.Builders;

/// <summary>
/// Base class for OpenAPI schema builders. Provides common type handling and caching.
/// </summary>
internal abstract class OpenApiSchemaBuilderBase : IOpenApiSchemaBuilder
{
    protected readonly OpenApiDocument Document;
    protected readonly Dictionary<(Type Type, bool Nullable), OpenApiSchema> Cache = [];

    protected OpenApiSchemaBuilderBase(OpenApiDocument document, OpenApiSpecVersion specVersion)
    {
        Document = document;
        SpecVersion = specVersion;
    }

    public OpenApiSpecVersion SpecVersion { get; }

    public abstract OpenApiSchema BuildComponentSchema(Type type);
    public abstract OpenApiSchema BuildSchemaFromPropertyInfo(PropertyInfo property);

    public void FlushToDocument()
    {
        var components = Document.Components
            ?? throw new InvalidOperationException("OpenApi document components are not initialized.");
        components.Schemas ??= new Dictionary<string, IOpenApiSchema>();
        foreach (var ((_, _), schema) in Cache)
        {
            components.Schemas.TryAdd(schema.Id!, schema);
        }
    }

    /// <summary>
    /// Normalizes cache keys for nullable value types.
    /// For Nullable&lt;T&gt;, returns the underlying type T to prevent duplicate cache entries.
    /// </summary>
    protected static Type GetCacheKey(Type type) =>
        Nullable.GetUnderlyingType(type) ?? type;

    public static bool IsPropertyNullable(PropertyInfo property)
    {
        if (Nullable.GetUnderlyingType(property.PropertyType) != null)
            return true;

        if (!property.PropertyType.IsValueType)
        {
            var context = new NullabilityInfoContext();
            var nullability = context.Create(property);
            return nullability.WriteState == NullabilityState.Nullable;
        }

        return false;
    }

    public static string GetFriendlyFullName(Type type)
    {
        if (type.IsArray)
        {
            var elementType = type.GetElementType() ?? typeof(object);
            return $"{GetFriendlyFullName(elementType)}Array";
        }

        if (!type.IsGenericType)
            return type.FullName ?? type.Name;

        var genericTypeDef = type.GetGenericTypeDefinition();
        var genericArgs = type.GetGenericArguments();
        var args = string.Join("_", genericArgs.Select(GetFriendlyFullName));
        var genericTypeName = genericTypeDef.FullName ?? genericTypeDef.Name;
        return $"{genericTypeName.TrimEnd('`', '1', '2', '3', '4')}_{args}";
    }

    protected static bool IsCollectionType(Type type)
    {
        var isDictionary = (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        return type != typeof(string)
            && !isDictionary
            && typeof(IEnumerable).IsAssignableFrom(type)
            && GetCollectionItemType(type) != typeof(object);
    }

    protected static Type GetCollectionItemType(Type type)
    {
        if (type.IsArray)
            return type.GetElementType() ?? typeof(object);

        if (type.IsGenericType)
            return type.GetGenericArguments()[0];

        var enumerableInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return enumerableInterface?.GetGenericArguments()[0] ?? typeof(object);
    }

    protected static string GetJsonPropertyName(PropertyInfo property, JsonSerializerOptions options)
    {
        var attr = property.GetCustomAttribute<JsonPropertyNameAttribute>();
        if (attr != null)
            return attr.Name;

        if (options?.PropertyNamingPolicy != null)
            return options.PropertyNamingPolicy.ConvertName(property.Name);

        return property.Name;
    }
}
