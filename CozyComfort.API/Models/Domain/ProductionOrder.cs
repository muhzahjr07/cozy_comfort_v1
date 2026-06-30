using System;

namespace CozyComfort.API.Models.Domain
{
    public class ProductionOrder
    {
        public int ProductionOrderId { get; set; }
        public int OrderId { get; set; }
        public int DistributorId { get; set; }
        public int BlanketId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; } = string.Empty; // Scheduled, InProduction, Completed
        public DateTime ScheduledDate { get; set; }
        public DateTime EstimatedCompletionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        // Navigation
        public virtual Order Order { get; set; } = null!;
        public virtual Distributor Distributor { get; set; } = null!;
        public virtual Blanket Blanket { get; set; } = null!;
    }
}
