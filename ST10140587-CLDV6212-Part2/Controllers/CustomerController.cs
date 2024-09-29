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
        var users = await _tableStorageService.GetAllUsersAsync(); // Changed to GetAllUsersAsync
        return View(users);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(User user)
    {
        user.PartitionKey = "UsersPartition";
        user.RowKey = Guid.NewGuid().ToString();

        await _tableStorageService.AddUserAsync(user); // Changed to AddUserAsync
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Delete(string partitionKey, string rowKey)
    {
        await _tableStorageService.DeleteUserAsync(partitionKey, rowKey); // Changed to DeleteUserAsync
        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Details(string partitionKey, string rowKey)
    {
        var user = await _tableStorageService.GetUserAsync(partitionKey, rowKey); // Changed to GetUserAsync
        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }
}