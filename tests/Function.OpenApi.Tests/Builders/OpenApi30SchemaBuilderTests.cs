// BY COPILOT
using Function.OpenApi.Builders;
using Microsoft.OpenApi;
using System.Text.Json;

namespace Function.OpenApi.Tests.Builders;

public class OpenApi30SchemaBuilderTests
{
    private static OpenApi30SchemaBuilder CreateBuilder()
    {
        var document = new OpenApiDocument
        {
            Components = new()
            {
                Schemas = new Dictionary<string, IOpenApiSchema>()
            }
        };
        return new OpenApi30SchemaBuilder(document);
    }

    #region Primitive Type Schemas

    [Fact]
    public void BuildComponentSchema_String_ReturnsStringSchema()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(string));

        Assert.Equal(JsonSchemaType.String, schema.Type);
        Assert.Equal("System.String", schema.Id);
        Assert.Null(schema.Format);
    }

    [Fact]
    public void BuildComponentSchema_Int_ReturnsIntegerSchema()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(int));

        Assert.Equal(JsonSchemaType.Integer, schema.Type);
        Assert.Equal("int32", schema.Format);
        Assert.Equal("System.Int32", schema.Id);
    }

    [Fact]
    public void BuildComponentSchema_Long_ReturnsInt64Schema()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(long));

        Assert.Equal(JsonSchemaType.Integer, schema.Type);
        Assert.Equal("int64", schema.Format);
        Assert.Equal("System.Int64", schema.Id);
    }

    [Fact]
    public void BuildComponentSchema_Bool_ReturnsBooleanSchema()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(bool));

        Assert.Equal(JsonSchemaType.Boolean, schema.Type);
    }

    [Fact]
    public void BuildComponentSchema_Guid_ReturnsUuidSchema()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(Guid));

        Assert.Equal(JsonSchemaType.String, schema.Type);
        Assert.Equal("uuid", schema.Format);
        Assert.Equal("System.Guid", schema.Id);
    }

    [Fact]
    public void BuildComponentSchema_DateOnly_ReturnsDateSchemaWithExample()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(DateOnly));

        Assert.Equal(JsonSchemaType.String, schema.Type);
        Assert.Equal("date", schema.Format);
        Assert.Equal("System.DateOnly", schema.Id);
        Assert.NotNull(schema.Example);
    }

    [Fact]
    public void BuildComponentSchema_DateTime_ReturnsDateTimeSchema()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(DateTime));

        Assert.Equal(JsonSchemaType.String, schema.Type);
        Assert.Equal("date-time", schema.Format);
    }

    [Fact]
    public void BuildComponentSchema_Float_ReturnsFloatSchema()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(float));

        Assert.Equal(JsonSchemaType.Number, schema.Type);
        Assert.Equal("float", schema.Format);
    }

    [Fact]
    public void BuildComponentSchema_Double_ReturnsDoubleSchema()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(double));

        Assert.Equal(JsonSchemaType.Number, schema.Type);
        Assert.Equal("double", schema.Format);
    }

    [Fact]
    public void BuildComponentSchema_Short_ReturnsInt32Schema()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(short));

        Assert.Equal(JsonSchemaType.Integer, schema.Type);
        Assert.Equal("int32", schema.Format);
    }

    [Fact]
    public void BuildComponentSchema_Byte_ReturnsIntegerUint8Schema()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(byte));

        Assert.Equal(JsonSchemaType.Integer, schema.Type);
        Assert.Equal("uint8", schema.Format);
        Assert.Equal("System.Byte", schema.Id);
    }

    [Fact]
    public void BuildComponentSchema_ByteArray_ReturnsStringByteSchema()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(byte[]));

        Assert.Equal(JsonSchemaType.String, schema.Type);
        Assert.Equal("byte", schema.Format);
        Assert.Equal("System.ByteArray", schema.Id);
    }

    [Fact]
    public void BuildComponentSchema_Byte_DoesNotMapToStringByte()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(byte));

        // Verify this is NOT string/byte (that was the bug)
        Assert.NotEqual(JsonSchemaType.String, schema.Type);
        Assert.NotEqual("byte", schema.Format);
    }

    #endregion

    #region Complex Type Schemas

    [Fact]
    public void BuildComponentSchema_Enum_ReturnsStringSchemaWithEnumValues()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(RequestKind));

        Assert.Equal(JsonSchemaType.String, schema.Type);
        Assert.NotNull(schema.Enum);
        Assert.Equal(2, schema.Enum.Count);
    }

    [Fact]
    public void BuildComponentSchema_ComplexObject_ReturnsObjectSchemaWithProperties()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(ResponseBody));

        Assert.Equal(JsonSchemaType.Object, schema.Type);
        Assert.NotNull(schema.Properties);
        Assert.Contains("name", schema.Properties.Keys);
        Assert.Contains("listOfInts", schema.Properties.Keys);
        Assert.Contains("nestedClass", schema.Properties.Keys);
    }

    [Fact]
    public void BuildComponentSchema_Array_ReturnsArraySchemaWithItems()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(List<string>));

        Assert.Equal(JsonSchemaType.Array, schema.Type);
        Assert.NotNull(schema.Items);
    }

    [Fact]
    public void BuildComponentSchema_NestedObject_HasAllProperties()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(NestedClass));

        Assert.Equal(JsonSchemaType.Object, schema.Type);
        Assert.NotNull(schema.Properties);
        Assert.Contains("id", schema.Properties.Keys);
        Assert.Contains("name", schema.Properties.Keys);
        Assert.Contains("age", schema.Properties.Keys);
        Assert.Contains("dateOnly", schema.Properties.Keys);
        Assert.Contains("longInteger", schema.Properties.Keys);
        Assert.Contains("floatNumber", schema.Properties.Keys);
    }

    [Fact]
    public void BuildComponentSchema_ByteTestModel_HasDistinctSchemaIdsForByteAndByteArray()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(ByteTestModel));

        Assert.Equal(JsonSchemaType.Object, schema.Type);
        Assert.NotNull(schema.Properties);
        Assert.Contains("singleByte", schema.Properties.Keys);
        Assert.Contains("binaryData", schema.Properties.Keys);

        // Build byte and byte[] individually to verify distinct IDs
        var byteSchema = builder.BuildComponentSchema(typeof(byte));
        var byteArraySchema = builder.BuildComponentSchema(typeof(byte[]));

        Assert.NotEqual(byteSchema.Id, byteArraySchema.Id);
        Assert.Equal("System.Byte", byteSchema.Id);
        Assert.Equal("System.ByteArray", byteArraySchema.Id);
    }

    [Fact]
    public void BuildComponentSchema_ByteTestModel_SingleByteProperty_IsIntegerUint8()
    {
        var builder = CreateBuilder();
        var property = typeof(ByteTestModel).GetProperty(nameof(ByteTestModel.SingleByte))!;

        var schema = builder.BuildSchemaFromPropertyInfo(property);

        Assert.Equal(JsonSchemaType.Integer, schema.Type);
        Assert.Equal("uint8", schema.Format);
    }

    [Fact]
    public void BuildComponentSchema_ByteTestModel_BinaryDataProperty_IsStringByte()
    {
        var builder = CreateBuilder();
        var property = typeof(ByteTestModel).GetProperty(nameof(ByteTestModel.BinaryData))!;

        var schema = builder.BuildSchemaFromPropertyInfo(property);

        // Should be string/byte for base64-encoded content
        Assert.True(schema.Type!.Value.HasFlag(JsonSchemaType.String));
        Assert.Equal("byte", schema.Format);
    }

    #endregion

    #region Nullable Handling

    [Fact]
    public void BuildSchemaFromPropertyInfo_NullableGuid_ReturnsNullableSchema()
    {
        var builder = CreateBuilder();
        var property = typeof(NestedClass).GetProperty(nameof(NestedClass.Id))!;

        var schema = builder.BuildSchemaFromPropertyInfo(property);

        Assert.True(schema.Type!.Value.HasFlag(JsonSchemaType.Null));
    }

    [Fact]
    public void BuildSchemaFromPropertyInfo_NullableString_ReturnsNullableSchema()
    {
        var builder = CreateBuilder();
        var property = typeof(NestedClass).GetProperty(nameof(NestedClass.Name))!;

        var schema = builder.BuildSchemaFromPropertyInfo(property);

        Assert.True(schema.Type!.Value.HasFlag(JsonSchemaType.Null));
    }

    [Fact]
    public void BuildSchemaFromPropertyInfo_NullableInt_ReturnsNullableSchema()
    {
        var builder = CreateBuilder();
        var property = typeof(NestedClass).GetProperty(nameof(NestedClass.Age))!;

        var schema = builder.BuildSchemaFromPropertyInfo(property);

        Assert.True(schema.Type!.Value.HasFlag(JsonSchemaType.Null));
    }

    [Fact]
    public void BuildSchemaFromPropertyInfo_NonNullableProperty_DoesNotHaveNullType()
    {
        var builder = CreateBuilder();
        var property = typeof(NestedClass).GetProperty(nameof(NestedClass.LongInteger))!;

        var schema = builder.BuildSchemaFromPropertyInfo(property);

        Assert.False(schema.Type!.Value.HasFlag(JsonSchemaType.Null));
    }

    [Fact]
    public void BuildSchemaFromPropertyInfo_NullableByte_ReturnsNullableIntegerSchema()
    {
        var builder = CreateBuilder();
        var property = typeof(ByteTestModel).GetProperty(nameof(ByteTestModel.NullableByte))!;

        var schema = builder.BuildSchemaFromPropertyInfo(property);

        Assert.True(schema.Type!.Value.HasFlag(JsonSchemaType.Null));
        Assert.True(schema.Type!.Value.HasFlag(JsonSchemaType.Integer));
        Assert.Equal("uint8", schema.Format);
    }

    [Fact]
    public void BuildSchemaFromPropertyInfo_NullableByteArray_ReturnsNullableStringSchema()
    {
        var builder = CreateBuilder();
        var property = typeof(ByteTestModel).GetProperty(nameof(ByteTestModel.NullableBinaryData))!;

        var schema = builder.BuildSchemaFromPropertyInfo(property);

        Assert.True(schema.Type!.Value.HasFlag(JsonSchemaType.Null));
        Assert.True(schema.Type!.Value.HasFlag(JsonSchemaType.String));
        Assert.Equal("byte", schema.Format);
    }

    #endregion

    #region Cache Behavior

    [Fact]
    public void BuildComponentSchema_SameTypeTwice_ReturnsCachedInstance()
    {
        var builder = CreateBuilder();

        var schema1 = builder.BuildComponentSchema(typeof(Guid));
        var schema2 = builder.BuildComponentSchema(typeof(Guid));

        Assert.Same(schema1, schema2);
    }

    [Fact]
    public void FlushToDocument_AddsAllSchemasToDocument()
    {
        var document = new OpenApiDocument
        {
            Components = new()
            {
                Schemas = new Dictionary<string, IOpenApiSchema>()
            }
        };
        var builder = new OpenApi30SchemaBuilder(document);

        builder.BuildComponentSchema(typeof(string));
        builder.BuildComponentSchema(typeof(int));
        builder.BuildComponentSchema(typeof(Guid));
        builder.FlushToDocument();

        Assert.True(document.Components!.Schemas!.Count >= 3);
    }

    [Fact]
    public void FlushToDocument_NoDuplicateSchemaKeys()
    {
        var document = new OpenApiDocument
        {
            Components = new()
            {
                Schemas = new Dictionary<string, IOpenApiSchema>()
            }
        };
        var builder = new OpenApi30SchemaBuilder(document);

        // Build same type multiple times
        builder.BuildComponentSchema(typeof(Guid));
        builder.BuildComponentSchema(typeof(Guid));
        builder.BuildComponentSchema(typeof(Guid));
        builder.FlushToDocument();

        // Should not throw duplicate key exception
        var guidSchemas = document.Components!.Schemas!
            .Where(s => s.Key.Contains("Guid"))
            .ToList();
        Assert.Single(guidSchemas);
    }

    #endregion

    #region Full Document Serialization (3.0)

    [Fact]
    public async Task FullDocument_30_SerializesAsOpenApi304()
    {
        var document = new OpenApiDocumentBuilder(typeof(TestFunctions).Assembly).BuildDocument();

        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_0, TestContext.Current.CancellationToken);

        Assert.Contains("\"openapi\": \"3.0.4\"", json);
    }

    #endregion
}
