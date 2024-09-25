using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using ST10140587_CLDV6212_Part2.Models;
using ST10140587_CLDV6212_Part2.ViewModels;

public class AccountController : Controller
{
    private readonly TableStorageService _tableStorageService;

    // Constructor for injecting the TableStorageService
    public AccountController(TableStorageService tableStorageService)
    {
        _tableStorageService = tableStorageService;
    }

    // GET: Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }

    // POST: Register
    [HttpPost]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            var customer = new Customer
            {
                Customer_Id = new Random().Next(1, 1000),
                Customer_Name = model.FullName,
                Email = model.Email,
                Password = HashPassword(model.Password),  // Hash the password before storing
                Role = "User",  // Default role for new registrations
                PartitionKey = "Customers",
                RowKey = model.Email  // Use email as the RowKey
            };

            try
            {
                // Store in Azure Table Storage
                await _tableStorageService.AddCustomerAsync(customer);
                return RedirectToAction("Login", "Account");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
        }

        return View(model);  // If model state is invalid, return the form with errors
    }

    // GET: Login
    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    // POST: Login
    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (ModelState.IsValid)
        {
            var customer = await _tableStorageService.GetCustomerAsync("Customers", model.Email);

            if (customer != null && VerifyPassword(model.Password, customer.Password))
            {
                // Create claims for the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, customer.Customer_Name),
                    new Claim(ClaimTypes.Email, customer.Email),
                    new Claim(ClaimTypes.Role, customer.Role)  // Add role as a claim
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // Sign in the user with cookie authentication
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Invalid email or password.");
        }

        return View(model);
    }

    // POST: Logout
    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }

    // Helper to hash the password using SHA256
    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    // Helper to verify the hashed password
    private bool VerifyPassword(string password, string storedHash)
    {
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hashedPassword = Convert.ToBase64String(bytes);
        return hashedPassword == storedHash;
    }
}
