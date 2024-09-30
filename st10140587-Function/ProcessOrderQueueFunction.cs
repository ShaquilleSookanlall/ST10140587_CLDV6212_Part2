using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using System;

public class ProcessOrderHttpFunction
{
    private readonly TableClient _orderTableClient;

    public ProcessOrderHttpFunction()
    {
        // Initialize the TableClient with the connection string
        string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        _orderTableClient = new TableClient(storageConnectionString, "Orders");
    }

    [Function("ProcessOrderHttp")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("ProcessOrderHttpFunction");
        logger.LogInformation("Processing order via HTTP request.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        Order order;

        try
        {
            order = JsonSerializer.Deserialize<Order>(requestBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        catch (JsonException ex)
        {
            logger.LogError($"Error deserializing order: {ex.Message}");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Invalid order data.");
            return badRequestResponse;
        }

        if (order == null || string.IsNullOrEmpty(order.CustomerName) || string.IsNullOrEmpty(order.ProductName))
        {
            logger.LogError("Invalid order: Missing required fields.");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Missing required fields.");
            return badRequestResponse;
        }

        order.PartitionKey = "Orders";
        order.RowKey = Guid.NewGuid().ToString();

        try
        {
            await _orderTableClient.AddEntityAsync(order);
            logger.LogInformation("Order added successfully.");
        }
        catch (RequestFailedException ex)
        {
            logger.LogError($"Error adding order to Table Storage: {ex.Message}");
            var internalServerErrorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await internalServerErrorResponse.WriteStringAsync("Error adding order to Table Storage.");
            return internalServerErrorResponse;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("Order processed successfully.");
        return response;
    }
}

public class Order : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string CustomerName { get; set; }
    public string ProductName { get; set; }
    public DateTime OrderDate { get; set; }
    public string OrderAddress { get; set; }
    public decimal TotalPrice { get; set; }
    public string OrderStatus { get; set; }
}
