using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using ST10140587_CLDV6212_Part2.Models;
using System.Threading.Tasks;

public class OrderController : Controller
{
    private readonly TableStorageService _tableStorageService;

    public OrderController(TableStorageService tableStorageService)
    {
        _tableStorageService = tableStorageService;
    }

    // GET: Order
    public async Task<IActionResult> Index()
    {
        var orders = await _tableStorageService.GetAllOrdersAsync();
        return View(orders);
    }

    // GET: Order/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Order/Create
    [HttpPost]
    public async Task<IActionResult> Create(Order order)
    {
        var products = await _tableStorageService.GetAllProductsAsync();
        var customers = await _tableStorageService.GetAllCustomersAsync();

        // Debugging logs to check data
        Console.WriteLine($"Products count: {products.Count}");
        Console.WriteLine($"Customers count: {customers.Count}");

        ViewBag.Products = new SelectList(products, "RowKey", "Product_Name");
        ViewBag.Customers = new SelectList(customers, "RowKey", "Customer_Name");

        return View();
    }



    // GET: Order/Delete/5
    public async Task<IActionResult> Delete(string partitionKey, string rowKey)
    {
        await _tableStorageService.DeleteOrderAsync(partitionKey, rowKey);
        return RedirectToAction("Index");
    }

    // GET: Order/Details/5
    public async Task<IActionResult> Details(string partitionKey, string rowKey)
    {
        var order = await _tableStorageService.GetOrderAsync(partitionKey, rowKey);
        if (order == null)
        {
            return NotFound();
        }
        return View(order);
    }
}
