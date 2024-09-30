using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cors.Infrastructure;
using ST10140587_CLDV6212_Part2.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// Register HttpClientFactory to enable HttpClient usage in controllers
builder.Services.AddHttpClient();

// Register HttpClientFactory
builder.Services.AddHttpClient();

// Register TableStorageService with configuration
builder.Services.AddSingleton<TableStorageService>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("AzureStorage");
    return new TableStorageService(connectionString);
});

// Register BlobService for handling Blob Storage operations
builder.Services.AddSingleton<BlobService>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("AzureStorage");
    return new BlobService(connectionString);
});

// Register CartService
builder.Services.AddScoped<CartService>();

// Register IHttpContextAccessor
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// ** Add session services **
builder.Services.AddDistributedMemoryCache(); // Required for session state
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
    options.Cookie.HttpOnly = true; // Makes the session cookie accessible only via HTTP (not JavaScript)
    options.Cookie.IsEssential = true; // Required for GDPR compliance
});

// Configure cookie authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ** Add session middleware to the pipeline **
app.UseSession(); // <-- This is required to enable session state

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
