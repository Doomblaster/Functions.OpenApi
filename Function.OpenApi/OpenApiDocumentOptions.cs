// BY COPILOT
using System.Reflection;

namespace Function.OpenApi;

public class OpenApiDocumentOptions
{
    public string Title { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string RoutePrefix { get; set; } = "api";
    public IList<string> ServerUrls { get; set; } = [];
    public IList<Assembly> Assemblies { get; set; } = [];
}
