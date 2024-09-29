using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ST10140587_CLDV6212_Part2.Models;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TableStorageService _tableStorageService; // To retrieve and display users

    public AccountController(IHttpClientFactory httpClientFactory, TableStorageService tableStorageService)
    {
        _httpClientFactory = httpClientFactory;
        _tableStorageService = tableStorageService;
    }

    // ---------------------
    // Register a new user (either customer or admin, depending on the role)
    // ---------------------
    [HttpPost]
    public async Task<IActionResult> Register(User user)
    {
        var httpClient = _httpClientFactory.CreateClient();

        // Convert user data to JSON
        var jsonContent = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");

        // Send POST request to the Azure Function (or your local function endpoint)
        var response = await httpClient.PostAsync("http://localhost:7267/api/RegisterUser", jsonContent);

        if (response.IsSuccessStatusCode)
        {
            // Redirect to login page after successful registration
            return RedirectToAction("Login", "Account");
        }

        // Handle registration failure (e.g., show error message)
        ModelState.AddModelError("", "Error registering user.");
        return View(user);
    }

    public IActionResult Register()
    {
        return View();
    }

    // ---------------------
    // User Login
    // ---------------------
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password)
    {
        // Retrieve user from Azure Table Storage using the email and password
        var user = await _tableStorageService.GetUserByEmailAndPasswordAsync(email, password);

        if (user != null)
        {
            // Create claims for the logged-in user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.User_Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)  // Admin or Customer
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // Sign the user in
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal);

            // Redirect to home page or admin page based on role
            if (user.Role == "Admin")
            {
                return RedirectToAction("Index", "Home"); // Redirect admins to an admin page
            }

            return RedirectToAction("Index", "Home"); // Redirect customers to the home page
        }

        // If the user is not found, show an error message
        ModelState.AddModelError("", "Invalid login attempt.");
        return View();
    }

    // ---------------------
    // User Logout
    // ---------------------
    public async Task<IActionResult> Logout()
    {
        // Sign out the user
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login", "Account");
    }

    // ---------------------
    // List users for admin view
    // ---------------------
    [HttpGet]
    public async Task<IActionResult> Users()
    {
        // Only admins can view this page
        if (User.IsInRole("Admin"))
        {
            var users = await _tableStorageService.GetAllUsersAsync(); // Fetch all users
            return View(users);
        }

        return Unauthorized(); // Redirect or show error if unauthorized access
    }
}