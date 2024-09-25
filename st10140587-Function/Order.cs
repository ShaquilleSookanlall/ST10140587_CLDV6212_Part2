using Azure;
using Azure.Data.Tables;
using System;

namespace ST10140587Function.Models
{
    public class Order : ITableEntity
    {
        public int Order_Id { get; set; }
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public int Customer_ID { get; set; }
        public int Product_ID { get; set; }
        public DateTime Order_Date { get; set; }
        public string? Order_Address { get; set; }
        public string? CustomerName { get; set; }
        public string? ProductName { get; set; }
        public decimal TotalCost { get; set; } // The new property for Total Cost

        // Optional helper method to deserialize Order from JSON
        public static Order FromJson(string jsonString)
        {
            return System.Text.Json.JsonSerializer.Deserialize<Order>(jsonString);
        }
    }
}
