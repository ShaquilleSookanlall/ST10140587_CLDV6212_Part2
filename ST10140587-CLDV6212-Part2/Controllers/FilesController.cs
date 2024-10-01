using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Authorization;

[Authorize(Roles = "Admin")]  // Restrict access to Admins


public class FilesController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    string uploadProductImageUrl = "https://st10140587functionapp.azurewebsites.net/api/UploadProductImage?code=peHZXlJH5bX5a5OL7OUrkuoSdaNn1PhBUDa9uR1rlt_mAzFuc4-bCg==";


    public FilesController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET: Files/Index
    public IActionResult Index()
    {
        // Load the page for uploading files
        return View();
    }

    // POST: Files/UploadImage
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["ErrorMessage"] = "Please select a .jpg file to upload.";
            return RedirectToAction("Index");
        }

        try
        {
            byte[] fileBytes;
            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                fileBytes = memoryStream.ToArray();
            }

            var base64String = Convert.ToBase64String(fileBytes);

            var httpClient = _httpClientFactory.CreateClient();
            var content = new StringContent(base64String);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/plain");

            var response = await httpClient.PostAsync(uploadProductImageUrl, content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "File uploaded successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Failed to upload the file. Status: {response.StatusCode}";
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
