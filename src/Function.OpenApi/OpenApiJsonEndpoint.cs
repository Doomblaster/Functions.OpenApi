using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.OpenApi;
using Function.OpenApi.Builders;

namespace Function.OpenApi;

internal class OpenApiJsonEndpoint
{
    private readonly OpenApiDocumentBuilder _builder;
    private readonly OpenApiDocumentOptions _options;

    public OpenApiJsonEndpoint(OpenApiDocumentBuilder builder, OpenApiDocumentOptions options)
    {
        _builder = builder;
        _options = options;
    }

    [Function("openapi")]
    public async Task<HttpResponseData> GetSwagger([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "openapi.json")] HttpRequestData data, CancellationToken ct)
    {
        var document = _builder.BuildDocument();
        var response = data.CreateResponse();
        response.Headers.Add("Content-Type", "application/json");
        using var ms = new MemoryStream();
        await document.SerializeAsJsonAsync(ms, specVersion: _options.SpecVersion, ct);
        await response.WriteBytesAsync(ms.ToArray(), cancellationToken: ct);
        return response;
    }
}
