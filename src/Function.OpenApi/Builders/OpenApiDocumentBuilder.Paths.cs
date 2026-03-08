// BY COPILOT
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.OpenApi;
using System.Reflection;

namespace Function.OpenApi.Builders;

public partial class OpenApiDocumentBuilder
{
    private IReadOnlyList<FunctionMethodMetadata> GetFunctionMethods()
    {
        return [.. _options.Assemblies
            .SelectMany(a => a.GetExportedTypes())
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            .Where(m => m.GetCustomAttributes<FunctionAttribute>().Any())
            .Select(CreateMethodMetadata)];
    }

    private static FunctionMethodMetadata CreateMethodMetadata(MethodInfo method)
    {
        var httpTriggerParameter = method.GetParameters()
            .FirstOrDefault(p => p.GetCustomAttribute<HttpTriggerAttribute>() is not null)
            ?? throw new InvalidOperationException($"No {nameof(HttpTriggerAttribute)} found for {method.Name}.");
        var httpTriggerAttribute = httpTriggerParameter.GetCustomAttribute<HttpTriggerAttribute>()
            ?? throw new InvalidOperationException($"No {nameof(HttpTriggerAttribute)} found for {method.Name}.");
        var functionAttribute = method.GetCustomAttribute<FunctionAttribute>()
            ?? throw new InvalidOperationException($"No {nameof(FunctionAttribute)} found for {method.Name}.");
        var httpMethods = httpTriggerAttribute.Methods
            ?? throw new InvalidOperationException($"No HTTP methods defined for {method.Name}.");

        return new FunctionMethodMetadata(
            method,
            functionAttribute,
            httpTriggerAttribute,
            httpMethods,
            [.. method.GetCustomAttributes<OpenApiResponseAttribute>()],
            [.. method.GetCustomAttributes<OpenApiHeaderAttribute>()],
            GetRouteParameters(method));
    }

    private static IReadOnlyList<ParameterInfo> GetRouteParameters(MethodInfo method)
    {
        return [.. method.GetParameters().Where(IsRouteParameter)];
    }

    private static bool IsRouteParameter(ParameterInfo parameter)
    {
        return parameter.ParameterType != typeof(CancellationToken)
            && parameter.ParameterType != typeof(HttpRequest)
            && parameter.ParameterType != typeof(HttpRequestData)
            && parameter.GetCustomAttribute<Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute>() is null
            && parameter.GetCustomAttribute<Microsoft.AspNetCore.Mvc.FromQueryAttribute>() is null;
    }

    private static string GetPath(FunctionMethodMetadata method)
    {
        var functionName = method.FunctionAttribute.Name
            ?? throw new InvalidOperationException("Function name cannot be null.");
        return method.HttpTriggerAttribute.Route is not null
            ? $"/{method.HttpTriggerAttribute.Route}"
            : $"/{functionName}";
    }

    private OpenApiPathItem GetOrCreatePathItem(string path)
    {
        if (_document.Paths.TryGetValue(path, out var existingPathItem))
        {
            if (existingPathItem is OpenApiPathItem openApiPathItem)
            {
                openApiPathItem.Operations ??= [];
                return openApiPathItem;
            }
            throw new InvalidOperationException($"Path item for '{path}' is not an OpenAPI path item.");
        }

        var pathItem = new OpenApiPathItem { Operations = [] };
        _document.Paths.Add(path, pathItem);
        return pathItem;
    }

    private void AddOperations(OpenApiPathItem pathItem, FunctionMethodMetadata method)
    {
        var operations = pathItem.Operations ??= [];
        foreach (var verb in method.HttpMethods)
        {
            if (string.IsNullOrWhiteSpace(verb))
                throw new InvalidOperationException("HTTP method cannot be null or empty.");

            var httpMethod = GetHttpMethod(verb);
            var operation = new OpenApiOperation
            {
                OperationId = BuildOperationId(method, verb)
            };
            AddResponses(operation, method.ResponseAttributes, method.HeaderAttributes);
            operations.Add(httpMethod, operation);
        }
    }

    private static string BuildOperationId(FunctionMethodMetadata method, string verb)
    {
        if (string.IsNullOrEmpty(verb))
            throw new InvalidOperationException("HTTP method cannot be null or empty.");

        var functionName = method.FunctionAttribute.Name
            ?? throw new InvalidOperationException("Function name cannot be null.");
        return $"{method.Method.DeclaringType?.Name}{char.ToUpper(verb[0])}{verb[1..]}{functionName}";
    }

    private static HttpMethod GetHttpMethod(string verb) => verb.ToLower() switch
    {
        "get" => HttpMethod.Get,
        "post" => HttpMethod.Post,
        "put" => HttpMethod.Put,
        "delete" => HttpMethod.Delete,
        _ => throw new InvalidOperationException($"Unsupported HTTP method: {verb}")
    };
}
