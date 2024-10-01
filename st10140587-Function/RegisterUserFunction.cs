using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

public class RegisterUserFunction
{
    private readonly TableClient _usersTableClient;

    public RegisterUserFunction()
    {
        string storageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
        _usersTableClient = new TableClient(storageConnectionString, "Users");
    }

    [Function("RegisterUser")]
    public async Task<HttpResponseData> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
        FunctionContext executionContext)
    {
        var log = executionContext.GetLogger("RegisterUserFunction");
        log.LogInformation("Processing new user registration");

        HttpResponseData response;

        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var user = JsonSerializer.Deserialize<User>(requestBody);
            if (user == null)
            {
                log.LogError("Request body deserialization failed.");
                response = req.CreateResponse(System.Net.HttpStatusCode.BadRequest);
                await response.WriteStringAsync("Invalid user data.");
                return response;
            }

            user.PartitionKey = "Users";
            user.RowKey = user.Email; 

            await _usersTableClient.AddEntityAsync(user);
            log.LogInformation($"User successfully added: {user.User_Name}, Role: {user.Role}");

            response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync("User registered successfully.");
        }
        catch (Exception ex)
        {
            log.LogError($"Error processing user registration: {ex.Message}");
            response = req.CreateResponse(System.Net.HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("Error registering user.");
        }

        return response;
    }
}

// User model example
public class User : ITableEntity
{
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }

    public string User_Id { get; set; }
    public string User_Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string Role { get; set; }  // Customer or Admin
}
