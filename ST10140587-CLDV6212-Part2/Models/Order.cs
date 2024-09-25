using Azure;
using Azure.Data.Tables;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ST10140587_CLDV6212_Part2.Models
{
    public class Order : ITableEntity
    {
        [Key]
        public int Order_Id { get; set; }

        // ITableEntity properties
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }

        // Customer and product fields for Table Storage
        [Required(ErrorMessage = "Select a Customer")]
        public int Customer_ID { get; set; }

        [Required(ErrorMessage = "Select a Product")]
        public int Product_ID { get; set; }

        [Required(ErrorMessage = "Enter a valid Date")]
        public DateTime Order_Date { get; set; }

        [Required(ErrorMessage = "Enter the location")]
        public string? Order_Address { get; set; }

        // Optional names for easier reading in UI and function apps
        public string? CustomerName { get; set; }
        public string? ProductName { get; set; }

        // Add Quantity for product
        [Required(ErrorMessage = "Enter the quantity")]
        public int Quantity { get; set; }

        // Add Product Price (to calculate total cost)
        [Required(ErrorMessage = "Enter the product price")]
        public double ProductPrice { get; set; }

        // Total cost - this can either be calculated dynamically or stored directly
        public double TotalCost => Quantity * ProductPrice;

        // Add a parameterless constructor for serialization in the function app
        public Order() { }

        // Constructor for manually creating an Order object
        public Order(int orderId, int customerId, int productId, DateTime orderDate, string address, int quantity, double productPrice, string customerName = null, string productName = null)
        {
            Order_Id = orderId;
            Customer_ID = customerId;
            Product_ID = productId;
            Order_Date = orderDate;
            Order_Address = address;
            Quantity = quantity;
            ProductPrice = productPrice;
            CustomerName = customerName;
            ProductName = productName;

            // Setting PartitionKey and RowKey for Table Storage
            PartitionKey = customerId.ToString();
            RowKey = orderId.ToString();
        }

        // Optional method for easy JSON serialization/deserialization in function app
        public static Order FromJson(string json)
        {
            return JsonSerializer.Deserialize<Order>(json);
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
