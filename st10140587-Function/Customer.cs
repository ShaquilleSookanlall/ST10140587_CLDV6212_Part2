using Azure;
using Azure.Data.Tables;
using System;

namespace ST10140587Function.Models
{
    public class Customer : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        public int Customer_Id { get; set; }
        public string Customer_Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; }
    }
}
