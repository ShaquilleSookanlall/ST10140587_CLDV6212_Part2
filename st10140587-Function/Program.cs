using Microsoft.Extensions.Hosting;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()  // Use this for Azure Functions isolated worker
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        // Add any other services you need here
    })
    .Build();

host.Run();
