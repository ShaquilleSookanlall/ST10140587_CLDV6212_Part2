using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using ST10140587_CLDV6212_Part2.Models;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System;

public class ProductController : Controller
{
    private readonly TableStorageService _tableStorageService;
    private readonly IHttpClientFactory _httpClientFactory;  // To make HTTP requests to Azure Functions

    public ProductController(TableStorageService tableStorageService, IHttpClientFactory httpClientFactory)
    {
        _tableStorageService = tableStorageService;
        _httpClientFactory = httpClientFactory;
    }

    // GET: Product
    public async Task<IActionResult> Index()
    {
        var products = await _tableStorageService.GetAllProductsAsync();
        return View(products);
    }

    // GET: Product/Create
    public IActionResult AddProduct()
    {
        return View();
    }

    // POST: Product/Create
    [HttpPost]
    public async Task<IActionResult> AddProduct(Product product, IFormFile productImage)
    {
        // Ensure an image is provided
        if (productImage == null || productImage.Length == 0)
        {
            TempData["ErrorMessage"] = "Please upload a product image.";
            return View(product);  // Reroute back to the form with an error message
        }

        try
        {
            // Step 1: Upload the image to Blob Storage via Azure Function
            var httpClient = _httpClientFactory.CreateClient();
            var content = new MultipartFormDataContent();

            using (var stream = new MemoryStream())
            {
                await productImage.CopyToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);
                var fileContent = new StreamContent(stream);
                content.Add(fileContent, "file", productImage.FileName);

                var response = await httpClient.PostAsync("http://localhost:7071/api/UploadProductImage", content);  // Azure Function URL

                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Failed to upload the product image. Please try again.";
                    return View(product);  // Reroute back to the form with an error message
                }
            }

            // Step 2: Save product details to Azure Table Storage
            product.PartitionKey = "ProductsPartition";
            product.RowKey = Guid.NewGuid().ToString();
            product.ImageUrl = productImage.FileName;  // Assuming the image file name is used to display the product image

            await _tableStorageService.AddProductAsync(product);

            // If everything is successful, redirect to the product index with success notification
            TempData["SuccessMessage"] = "Product added successfully!";
            return RedirectToAction("Index");

        }
        catch (Exception ex)
        {
            // Log the exception (optional)
            Console.WriteLine($"Error adding product: {ex.Message}");

            // Show an error message to the user and reroute to the form
            TempData["ErrorMessage"] = "An error occurred while adding the product. Please try again.";
            return View(product);
        }
    }
}
