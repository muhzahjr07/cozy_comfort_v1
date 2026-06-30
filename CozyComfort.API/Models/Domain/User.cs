using System;
using System.Text.Json.Serialization;

namespace CozyComfort.API.Models.Domain
{
    public class User
    {
        public int UserId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // Manufacturer, Distributor, Seller
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? LastLoginAt { get; set; }

        // Navigation
        [JsonIgnore]
        public virtual Seller? Seller { get; set; }
        [JsonIgnore]
        public virtual Distributor? Distributor { get; set; }
    }
}
