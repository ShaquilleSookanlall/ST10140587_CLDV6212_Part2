using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10140587_CLDV6212_Part2.Models;
using ST10140587_CLDV6212_Part2.Services;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]  // Restrict access to Admins
public class OrderController : Controller
{
    private readonly TableStorageService _tableStorageService;

    public OrderController(TableStorageService tableStorageService)
    {
        _tableStorageService = tableStorageService;
    }

    public async Task<IActionResult> Index()
    {
        var orders = await _tableStorageService.GetAllOrdersAsync();
        return View(orders);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Order order)
    {
        order.PartitionKey = "Orders";
        order.RowKey = Guid.NewGuid().ToString();

        await _tableStorageService.AddOrderAsync(order);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Delete(string partitionKey, string rowKey)
    {
        await _tableStorageService.DeleteOrderAsync(partitionKey, rowKey);
        return RedirectToAction("Index");
    }

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
