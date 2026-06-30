using System;

namespace CozyComfort.API.Models.Domain
{
    public class SupplyChainQuery
    {
        public int SupplyChainQueryId { get; set; }
        public int SellerId { get; set; }
        public int BlanketId { get; set; }
        public int QuantityRequested { get; set; }
        public int SellerStockLevel { get; set; }
        public int DistributorStockLevel { get; set; }
        public int ManufacturerStockLevel { get; set; }
        public int DailyProductionCapacity { get; set; }
        public int CalculatedLeadTimeDays { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
        public decimal DistributorCostQuoted { get; set; }
        public DateTime QueryDate { get; set; }

        // Navigation
        public virtual Seller Seller { get; set; } = null!;
        public virtual Blanket Blanket { get; set; } = null!;
    }
}
