using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CozyComfort.API.Models.Domain
{
    public class Distributor
    {
        public int DistributorId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public decimal ShippingCost { get; set; }
        public int LogisticsSpeedDays { get; set; }
        public decimal SuccessRate { get; set; }
        public int AvgResponseTimeMs { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        // Navigation
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
        [JsonIgnore]
        public virtual ICollection<Seller> Sellers { get; set; } = new List<Seller>();
        [JsonIgnore]
        public virtual ICollection<DistributorStock> DistributorStocks { get; set; } = new List<DistributorStock>();
        [JsonIgnore]
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        [JsonIgnore]
        public virtual ICollection<ProductionOrder> ProductionOrders { get; set; } = new List<ProductionOrder>();
    }
}
