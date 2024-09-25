using Microsoft.AspNetCore.Mvc;
using ST10140587_CLDV6212_Part2.Models;
using System.Threading.Tasks;

public class CustomerController : Controller
{
    private readonly TableStorageService _tableStorageService;

    public CustomerController(TableStorageService tableStorageService)
    {
        _tableStorageService = tableStorageService;
    }

    public async Task<IActionResult> Index()
    {
        var customers = await _tableStorageService.GetAllCustomersAsync();
        return View(customers);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Customer customer)
    {
        customer.PartitionKey = "BirdersPartition";
        customer.RowKey = Guid.NewGuid().ToString();

        await _tableStorageService.AddCustomerAsync(customer);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Delete(string partitionKey, string rowKey)
    {
        await _tableStorageService.DeleteCustomerAsync(partitionKey, rowKey);
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Details(string partitionKey, string rowKey)
    {
        var customer = await _tableStorageService.GetCustomerAsync(partitionKey, rowKey);
        if (customer == null)
        {
            return NotFound();
        }
        return View(customer);
    }
}
