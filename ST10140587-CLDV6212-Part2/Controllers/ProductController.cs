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
    public async Task<IActionResult> AddProduct(Product product, IFormFile ImageUpload)
    {
        try
        {
            // Step 1: Ensure the image file is provided
            if (ImageUpload == null || ImageUpload.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a product image.";
                return View(product);  // Reroute back to the form with an error message
            }

            // Step 2: Upload the image to Blob Storage
            string imageUrl;
            using (var stream = ImageUpload.OpenReadStream())
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(ImageUpload.FileName);
                imageUrl = await _blobService.UploadAsync(stream, fileName);  // Assuming _blobService.UploadAsync saves the file to Blob and returns the URL
            }

            // Step 3: Save product details to Azure Table Storage
            product.PartitionKey = "ProductsPartition";
            product.RowKey = Guid.NewGuid().ToString();
            product.ImageUrl = imageUrl;  // Assign uploaded image URL

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
    public async Task<IActionResult> Delete(string partitionKey, string rowKey)
    {
        var product = await _tableStorageService.GetProductAsync(partitionKey, rowKey);
        if (product == null)
        {
            return NotFound();
        }
        return View(product);  // Render the confirmation view
    }

    // POST: Product/DeleteConfirmed (delete product)
    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
    {
        await _tableStorageService.DeleteProductAsync(partitionKey, rowKey);
        TempData["SuccessMessage"] = "Product deleted successfully!";
        return RedirectToAction("Index");
    }


    [HttpPost]
    public async Task<IActionResult> AddToCart(int Product_Id)
    {
        var product = await _tableStorageService.GetProductByIdAsync(Product_Id);

        if (product == null)
        {
            TempData["ErrorMessage"] = "Product not found.";
            return RedirectToAction("Index");
        }

        // Get the cart from the session
        List<Product> cart = HttpContext.Session.GetObjectFromJson<List<Product>>("Cart") ?? new List<Product>();

        // Add the product to the cart
        cart.Add(product);

        // Save the cart back to the session
        HttpContext.Session.SetObjectAsJson("Cart", cart);

        TempData["SuccessMessage"] = $"{product.Product_Name} has been added to your cart.";
        return RedirectToAction("Index");
    }
}
