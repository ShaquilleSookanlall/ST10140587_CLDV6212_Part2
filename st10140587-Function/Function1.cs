using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Azure.Storage.Blobs;
using Azure.Storage.Files.Shares;
using Azure.Data.Tables;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using ST10140587Function.Models;

public class Function1
{
    // 1. Process Transaction Function
    [Function("ProcessTransactionFunction")]
    public void ProcessTransaction(
        [QueueTrigger("transactions", Connection = "AzureWebJobsStorage")] string queueMessage,
        ILogger log)
    {
        log.LogInformation($"Processing transaction message: {queueMessage}");

        // Deserialize the queue message into an Order object
        var order = Order.FromJson(queueMessage);

        log.LogInformation($"Order processed: ID = {order.Order_Id}, Total Cost = {order.TotalCost}");
        // Further processing can be added here
    }

    // 2. Store to Table Storage Function
    [Function("StoreToTableFunction")]
    public async Task StoreToTable(
        [QueueTrigger("products", Connection = "AzureWebJobsStorage")] string queueMessage,
        ILogger log)
    {
        log.LogInformation($"Queue message for table storage: {queueMessage}");

        var tableClient = new TableClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "Orders");

        // Deserialize the queue message to Order object
        var order = JsonSerializer.Deserialize<Order>(queueMessage);

        // Store the order in Azure Table Storage
        await tableClient.AddEntityAsync(order);
        log.LogInformation($"Order stored: {order.Order_Id}, TotalCost: {order.TotalCost}");
    }

    // 3. Write to Blob Storage Function
    [Function("WriteToBlobStorage")]
    public async Task WriteToBlobStorage(
        [BlobTrigger("products/{name}", Connection = "AzureWebJobsStorage")] byte[] blobContent,
        string name,
        ILogger log)
    {
        log.LogInformation($"Blob trigger for blob: {name}, Size: {blobContent.Length} Bytes");

        // Example: Write the blob data to another blob container or process the content
        var blobClient = new BlobClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "processedproducts", name);
        using (var stream = new MemoryStream(blobContent))
        {
            await blobClient.UploadAsync(stream, overwrite: true);
        }
        log.LogInformation($"Blob processed and uploaded: {name}");
    }

    // 4. Write to File Share Function
    [Function("WriteToFileShareFunction")]
    public async Task WriteToFileShare(
        [QueueTrigger("abcsales", Connection = "AzureWebJobsStorage")] string queueMessage,
        ILogger log)
    {
        log.LogInformation($"Queue trigger for writing to file share: {queueMessage}");

        var shareClient = new ShareClient(Environment.GetEnvironmentVariable("AzureWebJobsStorage"), "customershare");
        var directoryClient = shareClient.GetDirectoryClient("salesdata");
        await directoryClient.CreateIfNotExistsAsync();

        var fileClient = directoryClient.GetFileClient("sales.txt");
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(queueMessage));

        // Write to file in the Azure file share
        await fileClient.CreateAsync(stream.Length);
        await fileClient.UploadRangeAsync(new HttpRange(0, stream.Length), stream);

        log.LogInformation("File uploaded successfully to file share.");
    }
}
