using Microsoft.Extensions.Hosting;
using Microsoft.ApplicationInsights.WorkerService;
using Microsoft.Extensions.DependencyInjection;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()  // Use this for Azure Functions isolated worker
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();

        // Add any custom services or dependencies needed by the function
        services.AddSingleton<RegisterUserFunction>();  // Register the RegisterUserFunction
    })
    .Build();

host.Run();
