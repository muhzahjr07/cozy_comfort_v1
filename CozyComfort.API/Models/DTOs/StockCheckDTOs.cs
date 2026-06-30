using System;

namespace CozyComfort.API.Models.DTOs
{
    public class StockCheckRequest
    {
        public int SellerId { get; set; }
        public int BlanketId { get; set; }
        public int Quantity { get; set; }
    }

    public class StockCheckResponse
    {
        public int SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public int BlanketId { get; set; }
        public string BlanketName { get; set; } = string.Empty;
        public int QuantityRequested { get; set; }

        // Tier 1: Seller
        public int SellerStockLevel { get; set; }
        public string SellerStockStatus { get; set; } = string.Empty; // "InStock", "OutOfStock"

        // Tier 2: Distributor
        public int? DistributorId { get; set; }
        public string DistributorName { get; set; } = string.Empty;
        public int DistributorStockLevel { get; set; }
        public string DistributorStockStatus { get; set; } = string.Empty; // "InStock", "OutOfStock", "NoDistributor"

        // Tier 3: Manufacturer (Cozy Comfort)
        public int ManufacturerStockLevel { get; set; }
        public string ManufacturerStockStatus { get; set; } = string.Empty; // "InStock", "OutOfStock"
        public int DailyProductionCapacity { get; set; }

        // Results
        public int CalculatedLeadTimeDays { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
        public decimal PriceQuoted { get; set; } // Unit price
        public string RouteTaken { get; set; } = string.Empty; // "SellerStock", "DistributorStock", "ManufacturerStock", "ProductionRequired"
    }

    public class StockQueryHistoryResponse
    {
        public int SupplyChainQueryId { get; set; }
        public int SellerId { get; set; }
        public string SellerName { get; set; } = string.Empty;
        public int BlanketId { get; set; }
        public string BlanketName { get; set; } = string.Empty;
        public int QuantityRequested { get; set; }
        public int SellerStockLevel { get; set; }
        public int DistributorStockLevel { get; set; }
        public int ManufacturerStockLevel { get; set; }
        public int CalculatedLeadTimeDays { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
        public decimal DistributorCostQuoted { get; set; }
        public string RouteTaken { get; set; } = string.Empty;
        public DateTime QueryDate { get; set; }
    }
}
