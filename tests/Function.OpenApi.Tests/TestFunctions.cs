using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

using Function.OpenApi;
using Function.OpenApi.Tests;

public class TestFunctions
{
    private readonly ILogger<TestFunctions> _logger;

    public TestFunctions(ILogger<TestFunctions> logger)
    {
        _logger = logger;
    }

    [Function("Function1")]
    [OpenApiResponse("application/json", typeof(ResponseBody), System.Net.HttpStatusCode.OK)]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult(new ResponseBody { Name = "Welcome to Azure Functions!", ListOfInts = [.. Enumerable.Range(0, 10)], NestedClass = new NestedClass() });
    }

    [Function("Function2")]
    [OpenApiResponse("application/json", typeof(ResponseBody), System.Net.HttpStatusCode.OK)]
    [OpenApiResponse("application/json", typeof(ValidationProblemDetails), System.Net.HttpStatusCode.BadRequest)]
    [OpenApiRequestBody("application/json", typeof(RequestBody))]
    [OpenApiHeader("x-correlation-id", typeof(string), OpenApiHeaderDirection.In, true)]
    [OpenApiHeader("x-response-id", typeof(Guid), OpenApiHeaderDirection.Out, true, ResponseStatusCode = 200)]
    public IActionResult Run2([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "function2/{id}")] HttpRequest req, Guid id, [Microsoft.Azure.Functions.Worker.Http.FromBody] RequestBody body, CancellationToken token)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        return new OkObjectResult(new ResponseBody { Name = "Welcome to Azure Functions with route parameter!", ListOfInts = [.. Enumerable.Range(0, 10)], NestedClass = new NestedClass() });
    }
}
