using System;

namespace CozyComfort.API.Models.Domain
{
    public class DistributorStock
    {
        public int DistributorStockId { get; set; }
        public int DistributorId { get; set; }
        public int BlanketId { get; set; }
        public int StockQuantity { get; set; }
        public decimal CostPrice { get; set; }
        public DateTime LastUpdated { get; set; }

        // Navigation
        public virtual Distributor Distributor { get; set; } = null!;
        public virtual Blanket Blanket { get; set; } = null!;
    }
}
