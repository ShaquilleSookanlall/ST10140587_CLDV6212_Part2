using Azure.Storage.Blobs;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

public class UploadProductImageFunction
{
    private readonly BlobServiceClient _blobServiceClient;

    public UploadProductImageFunction()
    {
        string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        _blobServiceClient = new BlobServiceClient(storageConnectionString);
    }

    [Function("UploadProductImage")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var log = executionContext.GetLogger("UploadProductImageFunction");
        log.LogInformation("Processing image upload for new product.");

        HttpResponseData response;

        try
        {
            // Read the request content type to ensure it's a multipart/form-data request
            if (!req.Headers.TryGetValues("Content-Type", out var contentType) || !contentType.ToString().Contains("multipart/form-data"))
            {
                log.LogError("Invalid content type. Expected multipart/form-data.");
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Invalid content type. Expected multipart/form-data.");
                return response;
            }

            // Parse the form data manually by reading the request body
            var boundary = MultipartRequestHelper.GetBoundary(contentType.ToString(), 70);
            var reader = new MultipartReader(boundary, req.Body);
            var section = await reader.ReadNextSectionAsync();

            while (section != null)
            {
                // This example assumes you're only uploading one file.
                if (ContentDispositionHeaderValue.TryParse(section.ContentDisposition, out var contentDisposition) && contentDisposition.DispositionType.Equals("form-data") && contentDisposition.FileName.HasValue)
                {
                    var fileName = contentDisposition.FileName.Value;

                    // Get Blob container and create if it doesn't exist
                    var containerClient = _blobServiceClient.GetBlobContainerClient("products");
                    await containerClient.CreateIfNotExistsAsync();

                    // Upload the file to Blob Storage
                    var blobClient = containerClient.GetBlobClient(fileName);
                    using (var stream = section.Body)
                    {
                        await blobClient.UploadAsync(stream, true);
                    }

                    log.LogInformation($"File successfully uploaded: {fileName}");

                    // Respond with success message
                    response = req.CreateResponse(HttpStatusCode.OK);
                    await response.WriteStringAsync($"Image {fileName} uploaded successfully.");
                    return response;
                }

                section = await reader.ReadNextSectionAsync();
            }

            log.LogError("No file found in the request.");
            response = req.CreateResponse(HttpStatusCode.BadRequest);
            await response.WriteStringAsync("No file found.");
        }
        catch (Exception ex)
        {
            log.LogError($"Error uploading image: {ex.Message}");
            response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Error uploading image.");
        }

        return response;
    }
}

// Helper class for parsing multipart/form-data
public static class MultipartRequestHelper
{
    public static string GetBoundary(string contentType, int lengthLimit)
    {
        var boundary = HeaderUtilities.RemoveQuotes(MediaTypeHeaderValue.Parse(contentType).Boundary).Value;
        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidDataException("Missing content-type boundary.");
        }

        if (boundary.Length > lengthLimit)
        {
            throw new InvalidDataException($"Multipart boundary length limit {lengthLimit} exceeded.");
        }

        return boundary;
    }
}
