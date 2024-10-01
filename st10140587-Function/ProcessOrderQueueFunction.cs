using Azure.Data.Tables;
using Azure.Storage.Queues;
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
    private readonly QueueClient _queueClient;

    public ProcessOrderHttpFunction()
    {
        // Initialize the TableClient with the connection string
        string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        _orderTableClient = new TableClient(storageConnectionString, "Orders");

        // Initialize the QueueClient with the connection string and queue name
        _queueClient = new QueueClient(storageConnectionString, "orders");

        // Ensure the queue exists, create it if not
        _queueClient.CreateIfNotExists();
    }

    [Function("ProcessOrderHttp")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("ProcessOrderHttpFunction");
        logger.LogInformation("Processing order via HTTP request.");

        // Read the request body and deserialize into an Order object
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

        // Check for missing required fields
        if (order == null || string.IsNullOrEmpty(order.CustomerName) || string.IsNullOrEmpty(order.ProductName))
        {
            logger.LogError("Invalid order: Missing required fields.");
            var badRequestResponse = req.CreateResponse(HttpStatusCode.BadRequest);
            await badRequestResponse.WriteStringAsync("Missing required fields.");
            return badRequestResponse;
        }

        // Set PartitionKey and RowKey for the order
        order.PartitionKey = "Orders";
        order.RowKey = Guid.NewGuid().ToString();

        try
        {
            // Add the new order to Table Storage
            await _orderTableClient.AddEntityAsync(order);
            logger.LogInformation("Order added successfully to Table Storage.");

            // Prepare the queue message
            string queueMessage = JsonSerializer.Serialize(new
            {
                CustomerName = order.CustomerName,
                ProductName = order.ProductName,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice
            });

            // Send the message to Queue Storage
            await _queueClient.SendMessageAsync(queueMessage);
            logger.LogInformation("Message sent to Queue Storage.");
        }
        catch (RequestFailedException ex)
        {
            logger.LogError($"Error adding order to Table Storage or sending message to Queue: {ex.Message}");
            var internalServerErrorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await internalServerErrorResponse.WriteStringAsync("Error processing the order.");
            return internalServerErrorResponse;
        }

        // Return success response
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("Order processed successfully and message sent to queue.");
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
