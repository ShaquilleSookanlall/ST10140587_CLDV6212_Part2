using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ST10140587_CLDV6212_Part2.Models;
using ST10140587_CLDV6212_Part2.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

[Authorize]
public class OrderController : Controller
{
    private readonly TableStorageService _tableStorageService;

    public OrderController(TableStorageService tableStorageService)
    {
        _tableStorageService = tableStorageService;
    }

    // GET: Orders
    public async Task<IActionResult> Index()
    {
        List<Order> orders;

        // Check if the user is an admin
        if (User.IsInRole("Admin"))
        {
            // Admins can see all orders
            orders = await _tableStorageService.GetAllOrdersAsync();
        }
        else
        {
            // Regular users can only see their own orders by username
            var username = User.FindFirst(ClaimTypes.Name)?.Value;  // Get the username

            if (string.IsNullOrEmpty(username))
            {
                // Handle case where username is not found in claims
                TempData["ErrorMessage"] = "User name not found.";
                return RedirectToAction("Index", "Home");
            }

            // Fetch orders by the customer's username
            orders = await _tableStorageService.GetOrdersByCustomerAsync(username);
        }

        return View(orders);  // Return the orders to the view
    }

    // POST: Create Order (process and save new orders)
    [HttpPost]
    public async Task<IActionResult> Create(Order order)
    {
        try
        {
            // Set default order information
            order.PartitionKey = "Orders";
            order.RowKey = Guid.NewGuid().ToString();
            order.OrderDate = DateTime.UtcNow;  // Set current UTC time for order date
            order.OrderStatus = "Pending";  // Default status for a new order

            // Add the new order to Azure Table Storage
            await _tableStorageService.AddOrderAsync(order);

            TempData["SuccessMessage"] = "Order created successfully.";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Error: {ex.Message}";
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
