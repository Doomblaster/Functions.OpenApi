// BY COPILOT
using Function.OpenApi.Builders;
using Microsoft.OpenApi;

namespace Function.OpenApi.Tests.Builders;

public class OpenApiSchemaBuilderFactoryTests
{
    [Fact]
    public void Create_WithOpenApi30_ReturnsOpenApi30SchemaBuilder()
    {
        var document = new OpenApiDocument();

        var builder = OpenApiSchemaBuilderFactory.Create(document, OpenApiSpecVersion.OpenApi3_0);

        Assert.IsType<OpenApi30SchemaBuilder>(builder);
        Assert.Equal(OpenApiSpecVersion.OpenApi3_0, builder.SpecVersion);
    }

    [Fact]
    public void Create_WithOpenApi31_ReturnsOpenApi31SchemaBuilder()
    {
        var document = new OpenApiDocument();

        var builder = OpenApiSchemaBuilderFactory.Create(document, OpenApiSpecVersion.OpenApi3_1);

        Assert.IsType<OpenApi31SchemaBuilder>(builder);
        Assert.Equal(OpenApiSpecVersion.OpenApi3_1, builder.SpecVersion);
    }

    [Fact]
    public void Create_WithUnsupportedVersion_ThrowsNotSupportedException()
    {
        var document = new OpenApiDocument();
        var unsupportedVersion = (OpenApiSpecVersion)999;

        var exception = Assert.Throws<NotSupportedException>(
            () => OpenApiSchemaBuilderFactory.Create(document, unsupportedVersion));

        Assert.Contains("not supported", exception.Message);
    }

    [Fact]
    public void Create_ReturnedBuildersImplementIOpenApiSchemaBuilder()
    {
        var document = new OpenApiDocument();

        var builder30 = OpenApiSchemaBuilderFactory.Create(document, OpenApiSpecVersion.OpenApi3_0);
        var builder31 = OpenApiSchemaBuilderFactory.Create(document, OpenApiSpecVersion.OpenApi3_1);

        Assert.IsAssignableFrom<IOpenApiSchemaBuilder>(builder30);
        Assert.IsAssignableFrom<IOpenApiSchemaBuilder>(builder31);
    }

    [Fact]
    public void Create_EachCallReturnsNewInstance()
    {
        var document = new OpenApiDocument();

        var builder1 = OpenApiSchemaBuilderFactory.Create(document, OpenApiSpecVersion.OpenApi3_0);
        var builder2 = OpenApiSchemaBuilderFactory.Create(document, OpenApiSpecVersion.OpenApi3_0);

        Assert.NotSame(builder1, builder2);
    }
}
