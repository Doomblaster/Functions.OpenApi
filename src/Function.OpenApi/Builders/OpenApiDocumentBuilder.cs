// BY COPILOT
using Microsoft.OpenApi;
using System.Reflection;

namespace Function.OpenApi.Builders;

public partial class OpenApiDocumentBuilder
{
    private readonly OpenApiDocumentOptions _options;
    private OpenApiDocument _document = new();
    private OpenApiSchemaBuilder _schemaBuilder = null!;

    public OpenApiDocumentBuilder(OpenApiDocumentOptions options)
    {
        _options = options;
    }

    public OpenApiDocumentBuilder(params Assembly[] assemblies)
        : this(new OpenApiDocumentOptions
        {
            Title = "Test OpenApi Implementation",
            Version = "1.0.0",
            ServerUrls = ["http://localhost:7136"],
            Assemblies = [.. assemblies]
        })
    {
    }

    public OpenApiDocument BuildDocument()
    {
        InitializeDocument();

        var methods = GetFunctionMethods();
        foreach (var method in methods)
        {
            var path = GetPath(method);
            var pathItem = GetOrCreatePathItem(path);
            AddOperations(pathItem, method);
            AddRequestBody(pathItem, method.Method);
            AddRouteParameters(pathItem, method.RouteParameters);
            AddHeaderParameters(pathItem, method.HeaderAttributes);
        }

        _schemaBuilder.FlushToDocument();
        return _document;
    }

    private void InitializeDocument()
    {
        _document = new()
        {
            Info = new()
            {
                Version = _options.Version,
                Title = _options.Title
            },
            Servers = [.. _options.ServerUrls.Select(url => new OpenApiServer { Url = BuildServerUrl(url) })],
            Components = new()
            {
                Parameters = new Dictionary<string, IOpenApiParameter>(),
                RequestBodies = new Dictionary<string, IOpenApiRequestBody>(),
                Schemas = new Dictionary<string, IOpenApiSchema>()
            },
            Paths = []
        };
        _schemaBuilder = new OpenApiSchemaBuilder(_document);
    }

    private string BuildServerUrl(string baseUrl)
    {
        if (string.IsNullOrEmpty(_options.RoutePrefix))
            return baseUrl;
        return $"{baseUrl.TrimEnd('/')}/{_options.RoutePrefix}";
    }

    private OpenApiComponents GetComponents() =>
        _document.Components ?? throw new InvalidOperationException("OpenApi document components are not initialized.");

    private IDictionary<string, IOpenApiRequestBody> GetRequestBodies()
    {
        var components = GetComponents();
        components.RequestBodies ??= new Dictionary<string, IOpenApiRequestBody>();
        return components.RequestBodies;
    }

    private IDictionary<string, IOpenApiParameter> GetParameters()
    {
        var components = GetComponents();
        components.Parameters ??= new Dictionary<string, IOpenApiParameter>();
        return components.Parameters;
    }
}
