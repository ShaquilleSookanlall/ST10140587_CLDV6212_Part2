using System;
using System.Collections.Generic;

namespace ST10140587_CLDV6212_Part2.Models
{
    public class CartItem
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalPrice => Quantity * Price;  // Calculated property
    }
}
