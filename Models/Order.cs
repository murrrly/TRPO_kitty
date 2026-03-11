using System;
using System.Collections.Generic;

namespace RPM.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public DateTime OrderTime { get; set; }
        public string DeliveryType { get; set; }
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public string DishName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}