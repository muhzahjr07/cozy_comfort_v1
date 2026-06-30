using System;
using System.Collections.Generic;

namespace CozyComfort.API.Models.Domain
{
    public class Order
    {
        public int OrderId { get; set; }
        public string OrderReference { get; set; } = string.Empty;
        public int SellerId { get; set; }
        public int? DistributorId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty; // CheckingStock, Processing, AwaitingProduction, ShippedToDistributor, ShippedToSeller, Delivered
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        // Navigation
        public virtual Seller Seller { get; set; } = null!;
        public virtual Distributor? Distributor { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<ProductionOrder> ProductionOrders { get; set; } = new List<ProductionOrder>();
    }
}
