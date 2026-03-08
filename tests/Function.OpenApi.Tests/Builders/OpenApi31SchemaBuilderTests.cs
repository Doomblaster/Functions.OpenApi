// BY COPILOT
using Function.OpenApi.Builders;
using Microsoft.OpenApi;

namespace Function.OpenApi.Tests.Builders;

public class OpenApi31SchemaBuilderTests
{
    private static OpenApi31SchemaBuilder CreateBuilder()
    {
        var document = new OpenApiDocument
        {
            Components = new()
            {
                Schemas = new Dictionary<string, IOpenApiSchema>()
            }
        };
        return new OpenApi31SchemaBuilder(document);
    }

    #region DateOnly - Examples vs Example

    [Fact]
    public void BuildComponentSchema_DateOnly_UsesExamplesArrayInsteadOfExample()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(DateOnly));

        Assert.Equal(JsonSchemaType.String, schema.Type);
        Assert.Equal("date", schema.Format);
        Assert.Equal("System.DateOnly", schema.Id);
        // 3.1 uses Examples (array) instead of Example (singular)
        Assert.NotNull(schema.Examples);
        Assert.NotEmpty(schema.Examples);
    }

    [Fact]
    public void BuildComponentSchema_DateOnly_ExampleShouldBeNullIn31()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(DateOnly));

        // In 3.1, singular Example should not be set; Examples array is used instead
        Assert.Null(schema.Example);
    }

    #endregion

    #region Primitive Types Match 3.0

    [Fact]
    public void BuildComponentSchema_String_MatchesOpenApi30()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(string));

        Assert.Equal(JsonSchemaType.String, schema.Type);
        Assert.Equal("System.String", schema.Id);
    }

    [Fact]
    public void BuildComponentSchema_Int_MatchesOpenApi30()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(int));

        Assert.Equal(JsonSchemaType.Integer, schema.Type);
        Assert.Equal("int32", schema.Format);
    }

    [Fact]
    public void BuildComponentSchema_Guid_MatchesOpenApi30()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(Guid));

        Assert.Equal(JsonSchemaType.String, schema.Type);
        Assert.Equal("uuid", schema.Format);
        Assert.Equal("System.Guid", schema.Id);
    }

    [Fact]
    public void BuildComponentSchema_Bool_MatchesOpenApi30()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(bool));

        Assert.Equal(JsonSchemaType.Boolean, schema.Type);
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

    [Fact]
    public void BuildComponentSchema_Enum_MatchesOpenApi30()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(RequestKind));

        Assert.Equal(JsonSchemaType.String, schema.Type);
        Assert.NotNull(schema.Enum);
        Assert.Equal(2, schema.Enum.Count);
    }

    #endregion

    #region Complex Types Match 3.0

    [Fact]
    public void BuildComponentSchema_ComplexObject_HasSamePropertiesAs30()
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
    public void BuildComponentSchema_Array_HasSameStructureAs30()
    {
        var builder = CreateBuilder();

        var schema = builder.BuildComponentSchema(typeof(List<string>));

        Assert.Equal(JsonSchemaType.Array, schema.Type);
        Assert.NotNull(schema.Items);
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

    #region Nullable Handling (same schema construction, different serialization)

    [Fact]
    public void BuildSchemaFromPropertyInfo_NullableGuid_SetsNullTypeFlag()
    {
        var builder = CreateBuilder();
        var property = typeof(NestedClass).GetProperty(nameof(NestedClass.Id))!;

        var schema = builder.BuildSchemaFromPropertyInfo(property);

        // Schema construction is the same; Microsoft.OpenApi handles 3.1 serialization
        Assert.True(schema.Type!.Value.HasFlag(JsonSchemaType.Null));
    }

    [Fact]
    public void BuildSchemaFromPropertyInfo_NullableString_SetsNullTypeFlag()
    {
        var builder = CreateBuilder();
        var property = typeof(NestedClass).GetProperty(nameof(NestedClass.Name))!;

        var schema = builder.BuildSchemaFromPropertyInfo(property);

        Assert.True(schema.Type!.Value.HasFlag(JsonSchemaType.Null));
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
    public void FlushToDocument_NoDuplicateSchemaKeys()
    {
        var document = new OpenApiDocument
        {
            Components = new()
            {
                Schemas = new Dictionary<string, IOpenApiSchema>()
            }
        };
        var builder = new OpenApi31SchemaBuilder(document);

        builder.BuildComponentSchema(typeof(Guid));
        builder.BuildComponentSchema(typeof(Guid));
        builder.FlushToDocument();

        var guidSchemas = document.Components!.Schemas!
            .Where(s => s.Key.Contains("Guid"))
            .ToList();
        Assert.Single(guidSchemas);
    }

    #endregion

    #region SpecVersion

    [Fact]
    public void SpecVersion_ReturnsOpenApi31()
    {
        var builder = CreateBuilder();

        Assert.Equal(OpenApiSpecVersion.OpenApi3_1, builder.SpecVersion);
    }

    #endregion
}
