using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10140587_CLDV6212_Part2.Models;
using ST10140587_CLDV6212_Part2.Services;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using System.Linq;

[Authorize]
public class OrderController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;  // For making HTTP requests to the Azure Function

    public OrderController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET: Orders
    public async Task<IActionResult> Index()
    {
        List<Order> orders;

        // Check if the user is an admin
        if (User.IsInRole("Admin"))
        {
            // Admins can see all orders, retrieve them from the function
            orders = await FetchAllOrdersFromTableAsync();  // Implemented separately to retrieve all orders from the table
        }
        else
        {
            // Regular users can only see their own orders
            var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;

            if (userEmail == null)
            {
                // Handle case where user email is not found in claims
                TempData["ErrorMessage"] = "User email not found in session.";
                return RedirectToAction("Index", "Home");
            }

            orders = await FetchAllOrdersFromTableAsync(); // Implemented separately to retrieve all orders from the table
            orders = orders.Where(o => o.CustomerName == userEmail).ToList();
        }

        return View(orders);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Order order)
    {
        try
        {
            // Step 1: Prepare the HTTP client
            var httpClient = _httpClientFactory.CreateClient();

            // Step 2: Set default order information
            order.PartitionKey = "Orders";  // Azure Function should handle this
            order.RowKey = Guid.NewGuid().ToString();  // Unique identifier
            order.OrderDate = DateTime.UtcNow;  // Set current UTC time for order date
            order.OrderStatus = "Pending";  // Default status for a new order

            // Step 3: Serialize the order to JSON
            string serializedOrder = JsonConvert.SerializeObject(order);

            // Step 4: Create StringContent with the correct media type
            var content = new StringContent(serializedOrder, Encoding.UTF8, "application/json");

            // Step 5: Send the request to the Azure Function at the correct URL
            var response = await httpClient.PostAsync("http://localhost:7267/api/ProcessOrderHttp", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Order created and processed successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to process the order.";
            }
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
        }

        return RedirectToAction("Index");
    }

    // Fetches all orders from Table Storage (used for both admin and regular users)
    private async Task<List<Order>> FetchAllOrdersFromTableAsync()
    {
        try
        {
            // Prepare the HTTP client
            var httpClient = _httpClientFactory.CreateClient();

            // Send a GET request to the Azure Function or fetch from Table Storage (depending on your setup)
            // Note: This assumes you have another API/function endpoint that returns all orders, adjust if necessary
            var response = await httpClient.GetAsync("http://localhost:7267/api/GetAllOrders");

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<Order>>(responseData);
            }

            return new List<Order>();  // Return empty list if the request fails
        }
        catch (Exception ex)
        {
            // Handle error in fetching orders
            TempData["ErrorMessage"] = $"Error fetching orders: {ex.Message}";
            return new List<Order>();
        }
    }

    // GET: Order/Delete/{partitionKey}/{rowKey}
    public async Task<IActionResult> Delete(string partitionKey, string rowKey)
    {
        var order = await FetchOrderByKeysAsync(partitionKey, rowKey);
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
        await DeleteOrderFromTableAsync(partitionKey, rowKey); // Implement deletion logic via the function
        TempData["SuccessMessage"] = "Order deleted successfully!";
        return RedirectToAction("Index");
    }

    // GET: Order/Details/{partitionKey}/{rowKey}
    public async Task<IActionResult> Details(string partitionKey, string rowKey)
    {
        var order = await FetchOrderByKeysAsync(partitionKey, rowKey);
        if (order == null)
        {
            return NotFound();
        }
        return View(order);
    }

    // Fetches a specific order based on PartitionKey and RowKey (used in Delete and Details actions)
    private async Task<Order> FetchOrderByKeysAsync(string partitionKey, string rowKey)
    {
        try
        {
            // Use HttpClient to fetch an individual order from the Azure Function
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync($"http://localhost:7267/api/GetOrder/{partitionKey}/{rowKey}");

            if (response.IsSuccessStatusCode)
            {
                string responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Order>(responseData);
            }

            return null;
        }
        catch (Exception ex)
        {
            // Handle error in fetching the order
            TempData["ErrorMessage"] = $"Error fetching order: {ex.Message}";
            return null;
        }
    }

    // Deletes an order from Table Storage (used in DeleteConfirmed action)
    private async Task DeleteOrderFromTableAsync(string partitionKey, string rowKey)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.DeleteAsync($"http://localhost:7267/api/DeleteOrder/{partitionKey}/{rowKey}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Failed to delete the order.";
            }
        }
        catch (Exception ex)
        {
            // Handle error in deleting the order
            TempData["ErrorMessage"] = $"Error deleting order: {ex.Message}";
        }
    }
}