using Microsoft.AspNetCore.Mvc;
using ST10140587_CLDV6212_Part2.Services;
using ST10140587_CLDV6212_Part2.Models;
using System.Threading.Tasks;

public class ProductController : Controller
{
    private readonly TableStorageService _tableStorageService;
    private readonly BlobService _blobService;

    public ProductController(TableStorageService tableStorageService, BlobService blobService)
    {
        _tableStorageService = tableStorageService;
        _blobService = blobService;
    }

    // GET: Product
    public async Task<IActionResult> Index()
    {
        var products = await _tableStorageService.GetAllProductsAsync();
        return View(products);
    }

    // GET: Product/Create
    public async Task<IActionResult> AddProduct()
    {
        var blobImages = await _blobService.GetProductImagesAsync();  // Get images from Blob Storage
        ViewBag.Images = blobImages;  // Pass images to the view
        return View();
    }

    // POST: Product/Create
    [HttpPost]
    public async Task<IActionResult> AddProduct(Product product, string selectedImage)
    {
        try
        {
            // Step 1: Ensure the selected image is provided
            if (string.IsNullOrEmpty(selectedImage))
            {
                TempData["ErrorMessage"] = "Please select a product image.";
                return View(product);  // Reroute back to the form with an error message
            }

            // Step 2: Save product details to Azure Table Storage
            product.PartitionKey = "ProductsPartition";
            product.RowKey = Guid.NewGuid().ToString();
            product.ImageUrl = selectedImage;  // Assign selected image

            await _tableStorageService.AddProductAsync(product);

            // Redirect to the product index with a success notification
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

    // GET: Product/DeleteProduct (confirmation page)
    public async Task<IActionResult> DeleteProduct(string partitionKey, string rowKey)
    {
        var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);  // Render the confirmation view
    }

    // POST: Product/DeleteProduct (delete product)
    [HttpPost]
    public async Task<IActionResult> ConfirmDelete(string partitionKey, string rowKey)
    {
        await _tableStorageService.DeleteProductAsync(partitionKey, rowKey);
        TempData["SuccessMessage"] = "Product deleted successfully!";
        return RedirectToAction("Index");
    }

    // GET: Product/Details
    public async Task<IActionResult> Details(string partitionKey, string rowKey)
    {
        var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);
    }
}
