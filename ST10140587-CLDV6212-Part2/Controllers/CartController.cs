using Microsoft.AspNetCore.Mvc;
using ST10140587_CLDV6212_Part2.Models;
using ST10140587_CLDV6212_Part2.Services;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

public class CartController : Controller
{
    private readonly CartService _cartService;
    private readonly TableStorageService _tableStorageService;

    public CartController(CartService cartService, TableStorageService tableStorageService)
    {
        _cartService = cartService;
        _tableStorageService = tableStorageService;
    }

    // GET: Cart/Index
    public IActionResult Index()
    {
        var cartItems = _cartService.GetCartItems();
        ViewBag.TotalQuantity = cartItems.Sum(item => item.Quantity);
        ViewBag.TotalPrice = cartItems.Sum(item => item.Quantity * item.Price);
        return View(cartItems);
    }

    // POST: Cart/AddToCart
    [HttpPost]
    public async Task<IActionResult> AddToCart(string productId, int quantity)
    {
        // Fetch product details from Table Storage
        var product = await _tableStorageService.GetProductAsync("ProductsPartition", productId);

        if (product == null)
        {
            TempData["ErrorMessage"] = "Product not found.";
            return RedirectToAction("Index", "Product");
        }

        // Add product to cart
        var cartItem = new CartItem
        {
            ProductId = product.RowKey,
            ProductName = product.Product_Name,
            Quantity = quantity,
            Price = (decimal)product.Price
        };

        _cartService.AddToCart(cartItem);
        TempData["SuccessMessage"] = "Product added to cart.";
        return RedirectToAction("Index", "Product");
    }

    // POST: Cart/RemoveFromCart
    [HttpPost]
    public IActionResult RemoveFromCart(string productId)
    {
        _cartService.RemoveFromCart(productId);
        TempData["SuccessMessage"] = "Product removed from cart.";
        return RedirectToAction("Index");
    }

    // GET: Cart/Checkout
    public IActionResult Checkout()
    {
        var cartItems = _cartService.GetCartItems();
        ViewBag.TotalQuantity = cartItems.Sum(item => item.Quantity);
        ViewBag.TotalPrice = cartItems.Sum(item => item.Quantity * item.Price);
        return View(cartItems);
    }

    // POST: Cart/ConfirmOrder
    [HttpPost]
    public async Task<IActionResult> ConfirmOrder()
    {
        var cartItems = _cartService.GetCartItems();
        if (!cartItems.Any())
        {
            TempData["ErrorMessage"] = "Your cart is empty!";
            return RedirectToAction("Index");
        }

        // Create a new order
        var order = new Order
        {
            PartitionKey = "OrdersPartition",
            RowKey = Guid.NewGuid().ToString(),
            CustomerName = User.Identity.Name, // Assuming user is logged in
            OrderDate = DateTime.UtcNow,
            Order_Address = "Sample Address", // Replace with actual address collection
            TotalPrice = cartItems.Sum(item => item.Quantity * item.Price),
            OrderStatus = "Pending"
        };

        // Add order to Table Storage
        await _tableStorageService.AddOrderAsync(order);

        // Clear cart after confirming the order
        _cartService.ClearCart();

        TempData["SuccessMessage"] = "Order confirmed!";
        return RedirectToAction("Index", "Order"); // Redirect to orders page
    }
}
