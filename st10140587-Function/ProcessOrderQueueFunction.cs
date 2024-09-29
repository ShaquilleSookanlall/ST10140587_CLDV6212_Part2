using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

public class ProcessOrderFunction
{
    private readonly TableClient _orderTableClient;

    public ProcessOrderFunction()
    {
        string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        _orderTableClient = new TableClient(storageConnectionString, "Orders"); // Assuming 'Orders' is the table name
    }

    [Function("ProcessOrder")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext executionContext)
    {
        var log = executionContext.GetLogger("ProcessOrderFunction");
        log.LogInformation("Processing new order from HTTP trigger.");

        HttpResponseData response;
        try
        {
            // Read the request body and deserialize the JSON into the Order object
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var order = JsonSerializer.Deserialize<Order>(requestBody, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

            if (order == null)
            {
                log.LogError("Order deserialization failed.");
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Invalid order data.");
                return response;
            }

            // Set partition key and row key for Table Storage
            order.PartitionKey = "Orders";
            order.RowKey = Guid.NewGuid().ToString();

            // Add the order to Azure Table Storage
            await _orderTableClient.AddEntityAsync(order);
            log.LogInformation($"Order added to Table Storage. Order ID: {order.RowKey}");

            // Respond with success
            response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync($"Order processed successfully. Order ID: {order.RowKey}");
        }
        catch (Exception ex)
        {
            log.LogError($"Error processing order: {ex.Message}");
            response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Error processing the order.");
        }

        return response;
    }
}

// The Order model
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
    public string OrderStatus { get; set; }  // e.g., Pending, Completed, Cancelled
}
