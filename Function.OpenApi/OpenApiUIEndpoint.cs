using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Function.OpenApi;

internal class OpenApiUIEndpoint
{
    [Function("openapi-ui")]
    public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "openapi")] HttpRequestData data, CancellationToken ct)
    {
        var response = data.CreateResponse();
        response.Headers.Add("Content-Type", "text/html; charset=utf-8");
        await response.WriteStringAsync("""
<!doctype html>
<html>
  <head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <title>API Docs</title>
  </head>
  <body>
    <div id="app"></div>

    <script src="https://cdn.jsdelivr.net/npm/@scalar/api-reference"></script>
    <script>
      Scalar.createApiReference('#app', {
        url: '/api/openapi.json'
      })
    </script>
  </body>
</html>
""", cancellationToken: ct);
        return response;
    }
}