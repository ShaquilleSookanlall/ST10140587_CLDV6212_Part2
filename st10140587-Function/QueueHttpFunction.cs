using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

public class QueueHttpFunction
{
    private readonly QueueClient _queueClient;

    public QueueHttpFunction()
    {
        string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        _queueClient = new QueueClient(storageConnectionString, "orders");

        // Ensure the queue exists, create it if not
        _queueClient.CreateIfNotExists();
    }

    [Function("AddOrderToQueue")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext executionContext)
    {
        var logger = executionContext.GetLogger("AddOrderToQueue");
        logger.LogInformation("Received request to add order to the queue.");

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

        try
        {
            var queueMessage = JsonSerializer.Serialize(new QueueOrderMessage
            {
                CustomerName = order.CustomerName,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice
            });

            await _queueClient.SendMessageAsync(Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(queueMessage)));
            logger.LogInformation("Message sent to Queue Storage.");
        }
        catch (Exception ex)
        {
            logger.LogError($"Error sending message to the queue: {ex.Message}");
            var internalServerErrorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await internalServerErrorResponse.WriteStringAsync("Error sending the message to the queue.");
            return internalServerErrorResponse;
        }

        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync("Order successfully added to the queue.");
        return response;
    }

    public class QueueOrderMessage
    {
        public string CustomerName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
    }
}

// Order class representing the incoming order object
