using System;

namespace CozyComfort.API.Models.DTOs
{
    public class BlanketResponse
    {
        public int BlanketId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }

        // Manufacturer production/stock indicators
        public int StockQuantity { get; set; }
        public int DailyProductionCapacity { get; set; }
        public int LeadTimeDays { get; set; }
    }

    public class CreateBlanketRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        // Manufacturer starting properties
        public int InitialStock { get; set; } = 50;
        public int DailyProductionCapacity { get; set; } = 10;
        public int LeadTimeDays { get; set; } = 2;
    }

    public class BlanketSummaryResponse
    {
        public int BlanketId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Material { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }

    public class BlanketSearchRequest
    {
        public string? Query { get; set; }
        public string? Material { get; set; }
        public string? Size { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
