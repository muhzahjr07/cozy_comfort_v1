using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CozyComfort.API.Models.Domain
{
    public class Seller
    {
        public int SellerId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string ContactPhone { get; set; } = string.Empty;
        public int? AssignedDistributorId { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        // Navigation
        [JsonIgnore]
        public virtual User User { get; set; } = null!;
        public virtual Distributor? AssignedDistributor { get; set; }
        [JsonIgnore]
        public virtual ICollection<SellerStock> SellerStocks { get; set; } = new List<SellerStock>();
        [JsonIgnore]
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
