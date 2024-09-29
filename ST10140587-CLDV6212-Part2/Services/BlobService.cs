using Azure.Storage.Blobs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ST10140587_CLDV6212_Part2.Services
{
    public class BlobService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly string _containerName = "products";

        public BlobService(string connectionString)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
        }

        // Uploads a file to the blob storage
        public async Task<string> UploadAsync(Stream fileStream, string fileName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(fileName);
            await blobClient.UploadAsync(fileStream);
            return blobClient.Uri.ToString();
        }

        // Lists all images in the blob storage container
        public async Task<List<string>> GetProductImagesAsync()
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var imageList = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                imageList.Add(blobItem.Name);
            }

            return imageList;
        }

        // Deletes a blob from the storage
        public async Task DeleteBlobAsync(string blobUri)
        {
            Uri uri = new Uri(blobUri);
            string blobName = uri.Segments[^1];
            var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
            var blobClient = containerClient.GetBlobClient(blobName);
            await blobClient.DeleteIfExistsAsync();
        }
    }
}
