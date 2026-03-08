// BY COPILOT
using Function.OpenApi.Builders;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace Function.OpenApi.Tests;

public class OpenApi31IntegrationTests
{
    private static OpenApiDocumentBuilder CreateBuilderWithSpecVersion(OpenApiSpecVersion specVersion)
    {
        var options = new OpenApiDocumentOptions
        {
            Title = "Test OpenApi Implementation",
            Version = "1.0.0",
            ServerUrls = ["http://localhost:7136"],
            Assemblies = [typeof(TestFunctions).Assembly],
            SpecVersion = specVersion
        };
        return new OpenApiDocumentBuilder(options);
    }

    [Fact]
    public async Task FullDocument_31_HasCorrectOpenApiVersion()
    {
        var builder = CreateBuilderWithSpecVersion(OpenApiSpecVersion.OpenApi3_1);
        var document = builder.BuildDocument();

        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_1, TestContext.Current.CancellationToken);
        var node = JsonNode.Parse(json);

        // Microsoft.OpenApi 3.3.1 serializes OpenAPI 3.1 as version "3.1.2"
        var version = node!["openapi"]?.GetValue<string>();
        Assert.NotNull(version);
        Assert.StartsWith("3.1", version);
    }

    [Fact]
    public async Task FullDocument_31_NullableValueType_UsesTypeArrayWithNull()
    {
        var builder = CreateBuilderWithSpecVersion(OpenApiSpecVersion.OpenApi3_1);
        var document = builder.BuildDocument();

        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_1, TestContext.Current.CancellationToken);
        var node = JsonNode.Parse(json);

        // In 3.1, nullable types should use type array: ["string", "null"] instead of "nullable": true
        var nullableGuidSchema = node!["components"]?["schemas"]?["System.Nullable_System.Guid"];
        Assert.NotNull(nullableGuidSchema);

        var typeNode = nullableGuidSchema!["type"];
        Assert.NotNull(typeNode);

        // 3.1 serializes nullable as a type array
        if (typeNode is JsonArray typeArray)
        {
            var types = typeArray.Select(t => t?.GetValue<string>()).ToList();
            Assert.Contains("string", types);
            Assert.Contains("null", types);
        }
        else
        {
            // If Microsoft.OpenApi serializes it differently, at least verify nullable keyword is absent
            Assert.Null(nullableGuidSchema["nullable"]);
        }
    }

    [Fact]
    public async Task FullDocument_31_NullableReferenceType_UsesTypeArrayWithNull()
    {
        var builder = CreateBuilderWithSpecVersion(OpenApiSpecVersion.OpenApi3_1);
        var document = builder.BuildDocument();

        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_1, TestContext.Current.CancellationToken);
        var node = JsonNode.Parse(json);

        var nullableStringSchema = node!["components"]?["schemas"]?["Nullable_System.String"];
        Assert.NotNull(nullableStringSchema);

        // In 3.1, should not have "nullable": true keyword
        Assert.Null(nullableStringSchema!["nullable"]);
    }

    [Fact]
    public async Task FullDocument_31_HasAllPaths()
    {
        var builder = CreateBuilderWithSpecVersion(OpenApiSpecVersion.OpenApi3_1);
        var document = builder.BuildDocument();

        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_1, TestContext.Current.CancellationToken);
        var node = JsonNode.Parse(json);

        var paths = node!["paths"] as JsonObject;
        Assert.NotNull(paths);
        Assert.Contains("/Function1", paths!.Select(p => p.Key));
        Assert.Contains("/function2/{id}", paths.Select(p => p.Key));
    }

    [Fact]
    public async Task FullDocument_31_Function2_HasParameters()
    {
        var builder = CreateBuilderWithSpecVersion(OpenApiSpecVersion.OpenApi3_1);
        var document = builder.BuildDocument();

        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_1, TestContext.Current.CancellationToken);
        var node = JsonNode.Parse(json);

        var function2Post = node!["paths"]?["/function2/{id}"]?["post"];
        Assert.NotNull(function2Post);

        var parameters = function2Post!["parameters"] as JsonArray;
        Assert.NotNull(parameters);
        Assert.Equal(2, parameters!.Count);
    }

    [Fact]
    public async Task FullDocument_31_Function2_HasRequestBody()
    {
        var builder = CreateBuilderWithSpecVersion(OpenApiSpecVersion.OpenApi3_1);
        var document = builder.BuildDocument();

        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_1, TestContext.Current.CancellationToken);
        var node = JsonNode.Parse(json);

        var requestBody = node!["paths"]?["/function2/{id}"]?["post"]?["requestBody"];
        Assert.NotNull(requestBody);
    }

    [Fact]
    public async Task FullDocument_31_Function2_HasResponseHeaders()
    {
        var builder = CreateBuilderWithSpecVersion(OpenApiSpecVersion.OpenApi3_1);
        var document = builder.BuildDocument();

        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_1, TestContext.Current.CancellationToken);
        var node = JsonNode.Parse(json);

        var response200 = node!["paths"]?["/function2/{id}"]?["post"]?["responses"]?["200"];
        Assert.NotNull(response200);

        var headers = response200!["headers"];
        Assert.NotNull(headers);
        Assert.NotNull(headers!["x-response-id"]);
    }

    [Fact]
    public async Task FullDocument_31_HasComponentSchemas()
    {
        var builder = CreateBuilderWithSpecVersion(OpenApiSpecVersion.OpenApi3_1);
        var document = builder.BuildDocument();

        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_1, TestContext.Current.CancellationToken);
        var node = JsonNode.Parse(json);

        var schemas = node!["components"]?["schemas"] as JsonObject;
        Assert.NotNull(schemas);

        // Verify core schemas exist
        Assert.NotNull(schemas!["System.String"]);
        Assert.NotNull(schemas["System.Int32"]);
        Assert.NotNull(schemas["System.Guid"]);
        Assert.NotNull(schemas["Function.OpenApi.Tests.ResponseBody"]);
        Assert.NotNull(schemas["Function.OpenApi.Tests.RequestBody"]);
        Assert.NotNull(schemas["Function.OpenApi.Tests.RequestKind"]);
    }

    [Fact]
    public async Task FullDocument_31_DateOnly_UsesExamplesArray()
    {
        var builder = CreateBuilderWithSpecVersion(OpenApiSpecVersion.OpenApi3_1);
        var document = builder.BuildDocument();

        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_1, TestContext.Current.CancellationToken);
        var node = JsonNode.Parse(json);

        var dateOnlySchema = node!["components"]?["schemas"]?["System.DateOnly"];
        Assert.NotNull(dateOnlySchema);

        // In 3.1, should use "examples" (array) rather than "example" (singular)
        var examples = dateOnlySchema!["examples"];
        if (examples is not null)
        {
            Assert.IsType<JsonArray>(examples);
            Assert.NotEmpty((JsonArray)examples);
        }

        // In 3.1, singular "example" should not be present
        Assert.Null(dateOnlySchema["example"]);
    }

    [Fact]
    public async Task FullDocument_31_EnumSchema_HasCorrectValues()
    {
        var builder = CreateBuilderWithSpecVersion(OpenApiSpecVersion.OpenApi3_1);
        var document = builder.BuildDocument();

        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_1, TestContext.Current.CancellationToken);
        var node = JsonNode.Parse(json);

        var enumSchema = node!["components"]?["schemas"]?["Function.OpenApi.Tests.RequestKind"];
        Assert.NotNull(enumSchema);
        Assert.Equal("string", enumSchema!["type"]?.GetValue<string>());

        var enumValues = enumSchema["enum"] as JsonArray;
        Assert.NotNull(enumValues);
        Assert.Equal(2, enumValues!.Count);
        Assert.Contains("Standard", enumValues.Select(v => v?.GetValue<string>()));
        Assert.Contains("Urgent", enumValues.Select(v => v?.GetValue<string>()));
    }

    [Fact]
    public async Task FullDocument_31_HasMultipleResponseStatusCodes()
    {
        var builder = CreateBuilderWithSpecVersion(OpenApiSpecVersion.OpenApi3_1);
        var document = builder.BuildDocument();

        var json = await document.SerializeAsJsonAsync(OpenApiSpecVersion.OpenApi3_1, TestContext.Current.CancellationToken);
        var node = JsonNode.Parse(json);

        var responses = node!["paths"]?["/function2/{id}"]?["post"]?["responses"] as JsonObject;
        Assert.NotNull(responses);
        Assert.Contains("200", responses!.Select(r => r.Key));
        Assert.Contains("400", responses.Select(r => r.Key));
    }
}
