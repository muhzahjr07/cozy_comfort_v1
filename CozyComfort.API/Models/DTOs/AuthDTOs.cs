using System;
using System.ComponentModel.DataAnnotations;

namespace CozyComfort.API.Models.DTOs
{
    public class RegisterRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty; // Manufacturer, Distributor, Seller

        // Extra details for registration
        public string? SellerName { get; set; }
        public string? SellerShippingAddress { get; set; }
        public int? AssignedDistributorId { get; set; }

        public string? DistributorName { get; set; }
        public string? DistributorContactPhone { get; set; }
    }

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? SellerId { get; set; }
        public int? DistributorId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
