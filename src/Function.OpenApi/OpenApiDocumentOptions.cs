// BY COPILOT
using Microsoft.OpenApi;
using System.Reflection;

namespace Function.OpenApi;

public class OpenApiDocumentOptions
{
    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string RoutePrefix { get; set; } = "api";
    public IList<string> ServerUrls { get; set; } = [];
    public IList<Assembly> Assemblies { get; set; } = [];

    /// <summary>
    /// The OpenAPI specification version to generate. Defaults to OpenApi3_0.
    /// </summary>
    public OpenApiSpecVersion SpecVersion { get; set; } = OpenApiSpecVersion.OpenApi3_0;
}
