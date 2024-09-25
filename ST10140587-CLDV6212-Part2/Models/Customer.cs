using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ST10140587_CLDV6212_Part2.Models
{
    public class Customer : ITableEntity
    {
        [Key]
        public int Customer_Id { get; set; }

        public string Customer_Name { get; set; }

        public string Email { get; set; }  

        public string Password { get; set; }  

        public string Role { get; set; } 

        // ITableEntity properties
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
