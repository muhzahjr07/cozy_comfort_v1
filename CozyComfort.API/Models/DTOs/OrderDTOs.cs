using System;
using System.Collections.Generic;

namespace CozyComfort.API.Models.DTOs
{
    public class CreateOrderRequest
    {
        public int SellerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }

    public class CreateOrderItemRequest
    {
        public int BlanketId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    /// <summary>Guest checkout request — mirrors CreateOrderRequest but omits SellerId.</summary>
    public class GuestOrderRequest
    {
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<CreateOrderItemRequest> Items { get; set; } = new();
    }

    public class OrderResponse
    {
        public int OrderId { get; set; }
        public string OrderReference { get; set; } = string.Empty;
        public int SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public int? DistributorId { get; set; }
        public string DistributorName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string DeliveryAddress { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
        public List<OrderItemResponse> OrderItems { get; set; } = new();
        public List<ProductionOrderResponse> ProductionOrders { get; set; } = new();
    }

    public class OrderItemResponse
    {
        public int OrderItemId { get; set; }
        public int BlanketId { get; set; }
        public string BlanketName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }

    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; } = string.Empty; // Processing, AwaitingProduction, ShippedToDistributor, ShippedToSeller, Delivered
    }

    public class ProductionOrderResponse
    {
        public int ProductionOrderId { get; set; }
        public int OrderId { get; set; }
        public string OrderReference { get; set; } = string.Empty;
        public int DistributorId { get; set; }
        public string DistributorName { get; set; } = string.Empty;
        public int BlanketId { get; set; }
        public string BlanketName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty; // Scheduled, InProduction, Completed
        public DateTime ScheduledDate { get; set; }
        public DateTime EstimatedCompletionDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
