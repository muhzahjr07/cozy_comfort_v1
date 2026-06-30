using System;

namespace CozyComfort.API.Models.DTOs
{
    public class DistributorResponse
    {
        public int DistributorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public decimal ShippingCost { get; set; }
        public int LogisticsSpeedDays { get; set; }
        public decimal SuccessRate { get; set; }
        public int AvgResponseTimeMs { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateDistributorRequest
    {
        public string Name { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public decimal ShippingCost { get; set; }
        public int LogisticsSpeedDays { get; set; }
    }

    public class DistributorStockResponse
    {
        public int DistributorStockId { get; set; }
        public int DistributorId { get; set; }
        public int BlanketId { get; set; }
        public string BlanketName { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public decimal CostPrice { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class UpdateDistributorStockRequest
    {
        public int BlanketId { get; set; }
        public int StockQuantity { get; set; }
        public decimal CostPrice { get; set; }
    }
}
