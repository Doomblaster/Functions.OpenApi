// BY COPILOT
using Microsoft.OpenApi;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Function.OpenApi.Builders;

/// <summary>
/// Schema builder for OpenAPI 3.1 specification.
/// Uses JSON Schema 2020-12 semantics (type arrays for nullable, examples instead of example).
/// </summary>
internal sealed class OpenApi31SchemaBuilder : OpenApiSchemaBuilderBase
{
    public OpenApi31SchemaBuilder(OpenApiDocument document)
        : base(document, OpenApiSpecVersion.OpenApi3_1)
    {
    }

    public override OpenApiSchema BuildComponentSchema(Type type)
    {
        var schema = BuildSchemaFromType(type);
        Cache.TryAdd((type, false), schema);
        return schema;
    }

    public override OpenApiSchema BuildSchemaFromPropertyInfo(PropertyInfo property)
    {
        var isNullable = IsPropertyNullable(property);
        if (Cache.TryGetValue((property.PropertyType, isNullable), out var cachedSchema))
        {
            return cachedSchema;
        }

        var schema = BuildSchemaFromType(property.PropertyType);
        if (!isNullable)
        {
            Cache.TryAdd((property.PropertyType, false), schema);
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
        Cache.TryAdd((property.PropertyType, true), nullableSchema);
        return nullableSchema;
    }

    private OpenApiSchema BuildSchemaFromType(Type type)
    {
        if (Cache.TryGetValue((type, false), out var cachedSchema))
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
                schema.Examples = underlyingSchema.Examples;
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
                schema.Id = "System.Double";
                break;
            case var _ when type == typeof(byte[]):
                schema.Type = JsonSchemaType.String;
                schema.Format = "byte";
                schema.Id = "System.ByteArray";
                break;
            case var _ when type == typeof(byte):
                schema.Type = JsonSchemaType.Integer;
                schema.Format = "uint8";
                schema.Id = "System.Byte";
                break;
            case var _ when type == typeof(bool):
                schema.Type = JsonSchemaType.Boolean;
                schema.Id = "System.Boolean";
                break;
            case var _ when type == typeof(DateOnly):
                schema.Type = JsonSchemaType.String;
                schema.Format = "date";
                schema.Id = "System.DateOnly";
                // 3.1 uses `examples` (array) instead of `example` (singular)
                schema.Examples = [JsonValue.Create(DateOnly.FromDateTime(DateTime.UtcNow).ToString("O"))];
                break;
            case var _ when type == typeof(DateTime) || type == typeof(DateTimeOffset):
                schema.Type = JsonSchemaType.String;
                schema.Format = "date-time";
                schema.Id = "System.DateTime";
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
                var reference = new OpenApiSchemaReference(friendlyItemName, Document);
                schema.Items = reference;
                schema.Id = friendlyName;
                break;
            case var _ when IsDictionaryType(type):
                schema.Type = JsonSchemaType.Object;
                schema.Id = GetFriendlyFullName(type);
                var dictInterface = GetDictionaryInterface(type);
                var valueType = dictInterface.GetGenericArguments()[1];
                var valueTypeSchema = BuildComponentSchema(valueType);
                schema.AdditionalProperties ??= new OpenApiSchemaReference(valueTypeSchema.Id!, Document);
                break;
            default:
                schema.Type = JsonSchemaType.Object;
                schema.Properties ??= new Dictionary<string, IOpenApiSchema>();
                schema.Id = GetFriendlyFullName(type);
                foreach (var property in type.GetProperties())
                {
                    var name = GetJsonPropertyName(property, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                    var propertySchema = BuildSchemaFromPropertyInfo(property);
                    var propertyReference = new OpenApiSchemaReference(propertySchema.Id!, Document);
                    schema.Properties.Add(name, propertyReference);
                }
                break;
        }
        return schema;
    }
}
