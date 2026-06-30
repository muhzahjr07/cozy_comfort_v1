using System;

namespace CozyComfort.API.Models.Domain
{
    public class ManufacturerStock
    {
        public int ManufacturerStockId { get; set; }
        public int BlanketId { get; set; }
        public int StockQuantity { get; set; }
        public int DailyProductionCapacity { get; set; } // e.g. 10 blankets per day
        public int LeadTimeDays { get; set; } // basic setup lead time
        public DateTime LastUpdated { get; set; }

        // Navigation
        public virtual Blanket Blanket { get; set; } = null!;
    }
}
