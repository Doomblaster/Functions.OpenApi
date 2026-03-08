// BY COPILOT
using Microsoft.Extensions.DependencyInjection;
using Function.OpenApi.Builders;

namespace Function.OpenApi;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddOpenApi(
        this IServiceCollection services,
        Action<OpenApiDocumentOptions>? configure = null)
    {
        var options = new OpenApiDocumentOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddSingleton<OpenApiDocumentBuilder>();
        return services;
    }
}
