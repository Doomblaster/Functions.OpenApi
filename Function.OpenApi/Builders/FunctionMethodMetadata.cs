// BY COPILOT
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Reflection;

namespace Function.OpenApi.Builders;

internal sealed record FunctionMethodMetadata(
    MethodInfo Method,
    FunctionAttribute FunctionAttribute,
    HttpTriggerAttribute HttpTriggerAttribute,
    IReadOnlyCollection<string> HttpMethods,
    IReadOnlyCollection<OpenApiResponseAttribute> ResponseAttributes,
    IReadOnlyCollection<OpenApiHeaderAttribute> HeaderAttributes,
    IReadOnlyList<ParameterInfo> RouteParameters);
