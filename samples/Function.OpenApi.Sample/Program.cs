using Function.OpenApi;
using Function.OpenApi.Sample;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddOpenApi(options =>
    {
        options.Title = "Function OpenAPI Sample";
        options.Version = "1.0.0";
        options.ServerUrls.Add("http://localhost:7014");
        options.Assemblies.Add(typeof(SampleFunctions).Assembly);
    });

builder.Build().Run();
