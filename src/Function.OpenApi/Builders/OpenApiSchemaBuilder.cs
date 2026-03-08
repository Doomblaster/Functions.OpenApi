// BY COPILOT
using Microsoft.OpenApi;
using System.Collections;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace Function.OpenApi.Builders;

internal sealed class OpenApiSchemaBuilder
{
    private readonly OpenApiDocument _document;
    private readonly Dictionary<(Type Type, bool Nullable), OpenApiSchema> _cache = [];

    public OpenApiSchemaBuilder(OpenApiDocument document)
    {
        _document = document;
    }

    public OpenApiSchema BuildComponentSchema(Type type)
    {
        var schema = BuildSchemaFromType(type);
        _cache.TryAdd((type, false), schema);
        return schema;
    }

    public OpenApiSchema BuildSchemaFromPropertyInfo(PropertyInfo property)
    {
        var isNullable = IsPropertyNullable(property);
        if (_cache.TryGetValue((property.PropertyType, isNullable), out var cachedSchema))
        {
            return cachedSchema;
        }

        var schema = BuildSchemaFromType(property.PropertyType);
        if (!isNullable)
        {
            _cache.TryAdd((property.PropertyType, false), schema);
            return schema;
        }

        var nullableSchema = new OpenApiSchema
        {
            Type = schema.Type | JsonSchemaType.Null,
            Id = schema.Id,
            Format = schema.Format,
            Enum = schema.Enum,
            Properties = schema.Properties,
            Items = schema.Items
        };
        if (!property.PropertyType.IsValueType)
        {
            nullableSchema.Id = $"Nullable_{schema.Id}";
        }
        _cache.TryAdd((property.PropertyType, true), nullableSchema);
        return nullableSchema;
    }

    public void FlushToDocument()
    {
        var components = _document.Components
            ?? throw new InvalidOperationException("OpenApi document components are not initialized.");
        components.Schemas ??= new Dictionary<string, IOpenApiSchema>();
        foreach (var ((_, _), schema) in _cache)
        {
            components.Schemas.Add(schema.Id!, schema);
        }
    }

    private OpenApiSchema BuildSchemaFromType(Type type)
    {
        if (_cache.TryGetValue((type, false), out var cachedSchema))
        {
            return cachedSchema;
        }

        var schema = new OpenApiSchema();
        switch (type)
        {
            case var _ when Nullable.GetUnderlyingType(type) is not null:
                var underlyingType = Nullable.GetUnderlyingType(type) ?? typeof(object);
                var underlyingSchema = BuildSchemaFromType(underlyingType);
                schema.Type = underlyingSchema.Type | JsonSchemaType.Null;
                schema.Id = GetFriendlyFullName(type);
                schema.Format = underlyingSchema.Format;
                schema.Enum = underlyingSchema.Enum;
                schema.Example = underlyingSchema.Example;
                schema.Properties = underlyingSchema.Properties;
                schema.Items = underlyingSchema.Items;
                break;
            case var _ when type.IsEnum:
                schema.Type = JsonSchemaType.String;
                schema.Id = GetFriendlyFullName(type);
                schema.Enum = [.. Enum.GetNames(type).Select(name => JsonValue.Create(name)!)];
                break;
            case var _ when type == typeof(string):
                schema.Type = JsonSchemaType.String;
                schema.Id = "System.String";
                break;
            case var _ when type == typeof(short) || type == typeof(int):
                schema.Type = JsonSchemaType.Integer;
                schema.Format = "int32";
                schema.Id = "System.Int32";
                break;
            case var _ when type == typeof(long):
                schema.Type = JsonSchemaType.Integer;
                schema.Format = "int64";
                schema.Id = "System.Int64";
                break;
            case var _ when type == typeof(float):
                schema.Type = JsonSchemaType.Number;
                schema.Format = "float";
                schema.Id = "System.Single";
                break;
            case var _ when type == typeof(double) || type == typeof(decimal):
                schema.Type = JsonSchemaType.Number;
                schema.Format = "double";
                schema.Id = "double";
                break;
            case var _ when type == typeof(byte) || type == typeof(byte[]):
                schema.Type = JsonSchemaType.String;
                schema.Format = "byte";
                schema.Id = "byte";
                break;
            case var _ when type == typeof(bool):
                schema.Type = JsonSchemaType.Boolean;
                schema.Id = "bool";
                break;
            case var _ when type == typeof(DateOnly):
                schema.Type = JsonSchemaType.String;
                schema.Format = "date";
                schema.Id = "System.DateOnly";
                schema.Example = JsonValue.Create(DateOnly.FromDateTime(DateTime.UtcNow).ToString("O"));
                break;
            case var _ when type == typeof(DateTime) || type == typeof(DateTimeOffset):
                schema.Type = JsonSchemaType.String;
                schema.Format = "date-time";
                schema.Id = "date-time";
                break;
            case var _ when type == typeof(Guid):
                schema.Type = JsonSchemaType.String;
                schema.Format = "uuid";
                schema.Id = "System.Guid";
                break;
            case var _ when IsCollectionType(type):
                schema.Type = JsonSchemaType.Array;
                var itemType = GetCollectionItemType(type);
                _ = BuildComponentSchema(itemType);
                var friendlyName = GetFriendlyFullName(type);
                var friendlyItemName = GetFriendlyFullName(itemType);
                var reference = new OpenApiSchemaReference(friendlyItemName, _document);
                schema.Items = reference;
                schema.Id = friendlyName;
                break;
            case var _ when type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>):
                schema.Type = JsonSchemaType.Object;
                schema.Id = GetFriendlyFullName(type);
                var valueType = type.GetGenericArguments()[1];
                var valueTypeSchema = BuildComponentSchema(valueType);
                schema.AdditionalProperties ??= new OpenApiSchemaReference(valueTypeSchema.Id!, _document);
                break;
            default:
                schema.Type = JsonSchemaType.Object;
                schema.Properties ??= new Dictionary<string, IOpenApiSchema>();
                schema.Id = GetFriendlyFullName(type);
                foreach (var property in type.GetProperties())
                {
                    var name = GetJsonPropertyName(property, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    var propertySchema = BuildSchemaFromPropertyInfo(property);
                    var propertyReference = new OpenApiSchemaReference(propertySchema.Id!, _document);
                    schema.Properties.Add(name, propertyReference);
                }
                break;
        }
        return schema;
    }

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

    private static bool IsCollectionType(Type type)
    {
        var isDictionary = (type.IsInterface && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IDictionary<,>))
            || type.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
        return type != typeof(string)
            && !isDictionary
            && typeof(IEnumerable).IsAssignableFrom(type)
            && GetCollectionItemType(type) != typeof(object);
    }

    private static Type GetCollectionItemType(Type type)
    {
        if (type.IsArray)
            return type.GetElementType() ?? typeof(object);

        if (type.IsGenericType)
            return type.GetGenericArguments()[0];

        var enumerableInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        return enumerableInterface?.GetGenericArguments()[0] ?? typeof(object);
    }

    private static string GetJsonPropertyName(PropertyInfo property, JsonSerializerOptions options)
    {
        var attr = property.GetCustomAttribute<JsonPropertyNameAttribute>();
        if (attr != null)
            return attr.Name;

        if (options?.PropertyNamingPolicy != null)
            return options.PropertyNamingPolicy.ConvertName(property.Name);

        return property.Name;
    }
}
