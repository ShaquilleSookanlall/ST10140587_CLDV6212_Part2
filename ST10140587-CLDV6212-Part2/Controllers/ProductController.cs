using Microsoft.AspNetCore.Mvc;
using ST10140587_CLDV6212_Part2.Models;
using System.Threading.Tasks;

public class ProductController : Controller
{
    private readonly TableStorageService _tableStorageService;

    public ProductController(TableStorageService tableStorageService)
    {
        _tableStorageService = tableStorageService;
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
    public async Task<IActionResult> AddProduct(Product product)
    {
        product.PartitionKey = "ProductsPartition";
        product.RowKey = Guid.NewGuid().ToString();

        await _tableStorageService.AddProductAsync(product);
        return RedirectToAction("Index");
    }

    // GET: Product/Delete/5
    public async Task<IActionResult> Delete(string partitionKey, string rowKey)
    {
        await _tableStorageService.DeleteProductAsync(partitionKey, rowKey);
        return RedirectToAction("Index");
    }

    // GET: Product/Details/5
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
