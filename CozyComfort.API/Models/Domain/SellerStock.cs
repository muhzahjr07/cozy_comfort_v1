using System;

namespace CozyComfort.API.Models.Domain
{
    public class SellerStock
    {
        public int SellerStockId { get; set; }
        public int SellerId { get; set; }
        public int BlanketId { get; set; }
        public int StockQuantity { get; set; }
        public decimal RetailPrice { get; set; }
        public DateTime LastUpdated { get; set; }

        // Navigation
        public virtual Seller Seller { get; set; } = null!;
        public virtual Blanket Blanket { get; set; } = null!;
    }
}
