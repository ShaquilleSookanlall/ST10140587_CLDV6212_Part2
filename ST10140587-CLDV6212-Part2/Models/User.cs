using Azure;
using Azure.Data.Tables;
using System;

namespace ST10140587_CLDV6212_Part2.Models
{
    public class User : ITableEntity
    {
        // PartitionKey and RowKey
        public string PartitionKey { get; set; } = "Users"; // Default to Users partition
        public string RowKey { get; set; } // This will be the Email (unique identifier)

        // Properties
        public string User_Id { get; set; } // Unique ID for each user
        public string User_Name { get; set; } // User’s name
        public string Email { get; set; } // Email, also used as RowKey
        public string Password { get; set; } // User’s hashed password
        public string Role { get; set; }  // Customer or Admin

        // Metadata for TableEntity
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Constructors
        public User() { }

        public User(string userName, string email, string password, string role)
        {
            User_Id = Guid.NewGuid().ToString(); // Generate a new unique ID
            User_Name = userName;
            Email = email;
            Password = password;
            Role = role;
            PartitionKey = "Users"; // All users will be in the Users partition
            RowKey = email; // Use email as the unique RowKey
        }
    }
}
