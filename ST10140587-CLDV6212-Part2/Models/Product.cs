using Azure;
using Azure.Data.Tables;
using System.ComponentModel.DataAnnotations;

namespace ST10140587_CLDV6212_Part2.Models
{
    public class Product : ITableEntity
    {
        [Key]
        public int Product_Id { get; set; }
        public string? Product_Name { get; set; }
        public string? Product_Description { get; set; }
        public string? ImageUrl { get; set; }
        public Double Price { get; set; }
        
        // ITableEntity implementation
        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public ETag ETag { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
    }
}
