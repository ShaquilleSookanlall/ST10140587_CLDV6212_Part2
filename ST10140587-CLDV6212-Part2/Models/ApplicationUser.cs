using Azure.Data.Tables;
using Azure;

public class ApplicationUser : ITableEntity
{
    public string User_Id { get; set; }
    public string User_Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; } // or use PasswordHash if hashed
    public string Role { get; set; }  // Role field

    // Profile picture field to store the URL/path of the uploaded profile picture
    public string ProfilePictureUrl { get; set; }

    // ITableEntity implementation
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public ETag ETag { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
}
