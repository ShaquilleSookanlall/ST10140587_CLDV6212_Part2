using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10140587_CLDV6212_Part2.Models;
using ST10140587_CLDV6212_Part2.Services;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

[Authorize(Roles = "Admin")]  // Restrict access to Admins
public class OrderController : Controller
{
    private readonly TableStorageService _tableStorageService;
    private readonly IHttpClientFactory _httpClientFactory;  // For making HTTP requests to the Azure Function

    public OrderController(TableStorageService tableStorageService, IHttpClientFactory httpClientFactory)
    {
        _tableStorageService = tableStorageService;
        _httpClientFactory = httpClientFactory;
    }

    // GET: Order/Index
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
        // Step 1: Set partition key and row key for Table Storage
        order.PartitionKey = "Orders";
        order.RowKey = Guid.NewGuid().ToString();

        // Step 2: Prepare the HTTP client to call the Azure Function
        var httpClient = _httpClientFactory.CreateClient();

        // Serialize the order object to JSON
        var jsonContent = new StringContent(JsonConvert.SerializeObject(order), Encoding.UTF8, "application/json");

        // Step 3: Send a POST request to the ProcessOrder Azure Function
        var response = await httpClient.PostAsync("http://localhost:7267/api/ProcessOrder", jsonContent);

        // Step 4: Handle the response
        if (response.IsSuccessStatusCode)
        {
            TempData["SuccessMessage"] = "Order processed and added to Table Storage.";
        }
        else
        {
            TempData["ErrorMessage"] = $"Error processing order. Status code: {response.StatusCode}";
        }

        return RedirectToAction("Index");
    }

    // GET: Order/Delete/{partitionKey}/{rowKey}
    public async Task<IActionResult> Delete(string partitionKey, string rowKey)
    {
        var order = await _tableStorageService.GetOrderAsync(partitionKey, rowKey);
        if (order == null)
        {
            return NotFound();
        }
        return View(order);  // Show delete confirmation view
    }

    // POST: Order/DeleteConfirmed
    [HttpPost]
    public async Task<IActionResult> DeleteConfirmed(string partitionKey, string rowKey)
    {
        await _tableStorageService.DeleteOrderAsync(partitionKey, rowKey);
        TempData["SuccessMessage"] = "Order deleted successfully!";
        return RedirectToAction("Index");
    }


    // GET: Order/Details/{partitionKey}/{rowKey}
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
