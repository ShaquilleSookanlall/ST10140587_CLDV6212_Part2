using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using System;

public class FilesController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public FilesController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET: Files/Index
    public IActionResult Index()
    {
        // This will simply load the page
        return View();
    }

    // POST: Files/Upload
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a file to upload.";
            return RedirectToAction("Index");
        }

        try
        {
            // Step 1: Prepare the HTTP client
            var httpClient = _httpClientFactory.CreateClient();
            var content = new MultipartFormDataContent();

            // Step 2: Read the image file and add it to the request
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                var fileContent = new StreamContent(stream);
                content.Add(fileContent, "file", file.FileName);

                // Step 3: Make the POST request to the Azure Function to upload the image
                var response = await httpClient.PostAsync("http://localhost:7071/api/UploadProductImage", content);  // Update to Azure URL in production

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "File uploaded successfully.";
                    return RedirectToAction("Index");
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to upload the file.";
                }
            }

            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error uploading file: {ex.Message}";
            return RedirectToAction("Index");
        }
    }
}
