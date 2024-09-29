using Azure;
using Azure.Data.Tables;
using System;

namespace ST10140587_CLDV6212_Part2.Models
{
    public class Order : ITableEntity
    {
        public int Order_Id { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        // Existing properties
        public string CustomerName { get; set; }
        public string ProductName { get; set; }
        public DateTime OrderDate { get; set; }
        public string Order_Address { get; set; }
        public decimal TotalPrice { get; set; }
        public string OrderStatus { get; set; }  // e.g., Pending, Completed, Cancelled

        // Foreign keys for Product and Customer
        public string Product_Id { get; set; }  // Foreign key to Product table
        public string Customer_Id { get; set; }  // Foreign key to Customer table
    }
}
