using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CozyComfort.API.Models.Domain
{
    public class Blanket
    {
        public int BlanketId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty; // Wool, Cotton, Sherpa, Fleece, etc.
        public string Size { get; set; } = string.Empty; // Single, Double, Queen, King
        public decimal BasePrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        // Navigation
        [JsonIgnore]
        public virtual ICollection<DistributorStock> DistributorStocks { get; set; } = new List<DistributorStock>();
        [JsonIgnore]
        public virtual ICollection<SellerStock> SellerStocks { get; set; } = new List<SellerStock>();
        [JsonIgnore]
        public virtual ManufacturerStock? ManufacturerStock { get; set; }
    }
}
