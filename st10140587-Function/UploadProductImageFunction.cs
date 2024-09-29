using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

public class UploadProductImageFunction
{
    private readonly BlobServiceClient _blobServiceClient;

    public UploadProductImageFunction()
    {
        // Retrieve the AzureWebJobsStorage connection string from environment variables
        string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        _blobServiceClient = new BlobServiceClient(storageConnectionString);
    }

    [Function("UploadProductImage")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequestData req,
        FunctionContext executionContext)
    {
        var log = executionContext.GetLogger("UploadProductImageFunction");
        log.LogInformation("Processing image upload request.");

        HttpResponseData response;

        try
        {
            // Step 1: Read the base64 string from the request body
            var base64String = await req.ReadAsStringAsync();
            if (string.IsNullOrEmpty(base64String))
            {
                log.LogError("No content found in the request body.");
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Request body is empty. Please send a base64 encoded string.");
                return response;
            }

            // Step 2: Convert the base64 string into a byte array (file content)
            byte[] fileBytes = Convert.FromBase64String(base64String);

            // Step 3: Create or get the Blob container client for "products"
            var containerClient = _blobServiceClient.GetBlobContainerClient("products");
            await containerClient.CreateIfNotExistsAsync();  // Ensure the container exists

            // Step 4: Generate a unique name for the uploaded file (e.g., uploaded_image_<timestamp>.jpg)
            string fileName = $"uploaded_image_{DateTime.UtcNow:yyyyMMddHHmmss}.jpg";
            var blobClient = containerClient.GetBlobClient(fileName);

            // Step 5: Upload the byte array to Blob Storage as a new file
            using (var stream = new MemoryStream(fileBytes))
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            log.LogInformation($"File successfully uploaded as {fileName}");

            // Step 6: Respond with the file name and success message
            response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync($"Image {fileName} uploaded successfully.");
            return response;
        }
        catch (FormatException ex)
        {
            log.LogError($"Invalid base64 format: {ex.Message}");
            response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync("Invalid base64 format. Please check the input.");
            return response;
        }
        catch (Exception ex)
        {
            log.LogError($"Error uploading image: {ex.Message}");
            response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync($"Error uploading image: {ex.Message}");
            return response;
        }
    }
}
