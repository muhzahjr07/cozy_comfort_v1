using BCrypt.Net;
using CozyComfort.API.Models.Domain;
using Microsoft.EntityFrameworkCore;

namespace CozyComfort.API.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            try
            {
                if (await context.Users.AnyAsync())
                    return;

                // ── Users ──────────────────────────────────────────────────────────
                var manufacturer = new User
                {
                    Email = "admin@cozycomfort.com",
                    FirstName = "Cozy",
                    LastName = "Admin",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = "Manufacturer",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                var distUser1 = new User
                {
                    Email = "contact@warmroutes.com",
                    FirstName = "Warm",
                    LastName = "Routes",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = "Distributor",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                var distUser2 = new User
                {
                    Email = "info@softlogistics.com",
                    FirstName = "Soft",
                    LastName = "Logistics",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = "Distributor",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                var distUser3 = new User
                {
                    Email = "ops@cozydistrib.com",
                    FirstName = "Cozy",
                    LastName = "Distrib",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = "Distributor",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                var sellerUser1 = new User
                {
                    Email = "owner@nestandhome.com",
                    FirstName = "Nest",
                    LastName = "Home",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = "Seller",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                var sellerUser2 = new User
                {
                    Email = "sales@snugshop.com",
                    FirstName = "Snug",
                    LastName = "Shop",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = "Seller",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                var sellerUser3 = new User
                {
                    Email = "contact@cloudlinens.com",
                    FirstName = "Cloud",
                    LastName = "Linens",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123"),
                    Role = "Seller",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var users = new List<User> { manufacturer, distUser1, distUser2, distUser3, sellerUser1, sellerUser2, sellerUser3 };
                context.Users.AddRange(users);
                await context.SaveChangesAsync();

                // ── Blankets ───────────────────────────────────────────────────────
                var blankets = new List<Blanket>
                {
                    new Blanket
                    {
                        Name = "Arctic Wool Throw",
                        Description = "Ultra-soft merino wool blanket ideal for cold nights. Hypoallergenic and naturally temperature-regulating.",
                        Material = "Merino Wool",
                        Size = "Queen",
                        BasePrice = 89.99m,
                        ImageUrl = "https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=400",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow
                    },
                    new Blanket
                    {
                        Name = "Cloud Sherpa Blanket",
                        Description = "Double-sided sherpa fleece for maximum coziness. Machine washable and endlessly soft.",
                        Material = "Sherpa Fleece",
                        Size = "King",
                        BasePrice = 74.99m,
                        ImageUrl = "https://images.unsplash.com/photo-1567016432779-094069958ea5?w=400",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow
                    },
                    new Blanket
                    {
                        Name = "Summer Cotton Weave",
                        Description = "Breathable 100% organic cotton in a classic waffle weave. Perfect for year-round use.",
                        Material = "Organic Cotton",
                        Size = "Double",
                        BasePrice = 54.99m,
                        ImageUrl = "https://images.unsplash.com/photo-1620712943543-bcc4688e7485?w=400",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow
                    },
                    new Blanket
                    {
                        Name = "Velvet Plush Throw",
                        Description = "Luxurious velvet microfiber blanket with a high-pile plush finish. Lightweight yet warm.",
                        Material = "Microfiber Velvet",
                        Size = "Single",
                        BasePrice = 44.99m,
                        ImageUrl = "https://images.unsplash.com/photo-1584735935682-2f2b69dff9d2?w=400",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow
                    },
                    new Blanket
                    {
                        Name = "Heritage Knit Blanket",
                        Description = "Hand-inspired chunky knit design in premium acrylic yarn. Stylish and cozy for living rooms.",
                        Material = "Premium Acrylic",
                        Size = "Queen",
                        BasePrice = 64.99m,
                        ImageUrl = "https://images.unsplash.com/photo-1611486212557-88be5ff6f941?w=400",
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow,
                        ModifiedAt = DateTime.UtcNow
                    }
                };
                context.Blankets.AddRange(blankets);
                await context.SaveChangesAsync();

                // ── Manufacturer Stock ────────────────────────────────────────────
                var mfgStocks = new List<ManufacturerStock>
                {
                    new ManufacturerStock { BlanketId = blankets[0].BlanketId, StockQuantity = 120, DailyProductionCapacity = 15, LeadTimeDays = 2, LastUpdated = DateTime.UtcNow },
                    new ManufacturerStock { BlanketId = blankets[1].BlanketId, StockQuantity = 85, DailyProductionCapacity = 12, LeadTimeDays = 2, LastUpdated = DateTime.UtcNow },
                    new ManufacturerStock { BlanketId = blankets[2].BlanketId, StockQuantity = 200, DailyProductionCapacity = 20, LeadTimeDays = 1, LastUpdated = DateTime.UtcNow },
                    new ManufacturerStock { BlanketId = blankets[3].BlanketId, StockQuantity = 50, DailyProductionCapacity = 10, LeadTimeDays = 3, LastUpdated = DateTime.UtcNow },
                    new ManufacturerStock { BlanketId = blankets[4].BlanketId, StockQuantity = 75, DailyProductionCapacity = 10, LeadTimeDays = 3, LastUpdated = DateTime.UtcNow }
                };
                context.ManufacturerStocks.AddRange(mfgStocks);
                await context.SaveChangesAsync();

                // ── Distributors ───────────────────────────────────────────────────
                var dist1 = new Distributor
                {
                    UserId = distUser1.UserId,
                    Name = "Warm Routes Ltd",
                    ContactEmail = "contact@warmroutes.com",
                    ContactPhone = "+1-555-301-0001",
                    ShippingCost = 8.50m,
                    LogisticsSpeedDays = 2,
                    SuccessRate = 0.97m,
                    AvgResponseTimeMs = 800,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };
                var dist2 = new Distributor
                {
                    UserId = distUser2.UserId,
                    Name = "Soft Logistics Co",
                    ContactEmail = "info@softlogistics.com",
                    ContactPhone = "+1-555-302-0002",
                    ShippingCost = 6.00m,
                    LogisticsSpeedDays = 3,
                    SuccessRate = 0.92m,
                    AvgResponseTimeMs = 1200,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };
                var dist3 = new Distributor
                {
                    UserId = distUser3.UserId,
                    Name = "Cozy Distrib Inc",
                    ContactEmail = "ops@cozydistrib.com",
                    ContactPhone = "+1-555-303-0003",
                    ShippingCost = 10.00m,
                    LogisticsSpeedDays = 1,
                    SuccessRate = 0.99m,
                    AvgResponseTimeMs = 500,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };
                var distributors = new List<Distributor> { dist1, dist2, dist3 };
                context.Distributors.AddRange(distributors);
                await context.SaveChangesAsync();

                // ── Distributor Stock ──────────────────────────────────────────────
                var distStocks = new List<DistributorStock>
                {
                    new DistributorStock { DistributorId = dist1.DistributorId, BlanketId = blankets[0].BlanketId, StockQuantity = 30, CostPrice = 72.00m, LastUpdated = DateTime.UtcNow },
                    new DistributorStock { DistributorId = dist1.DistributorId, BlanketId = blankets[1].BlanketId, StockQuantity = 20, CostPrice = 60.00m, LastUpdated = DateTime.UtcNow },
                    new DistributorStock { DistributorId = dist1.DistributorId, BlanketId = blankets[2].BlanketId, StockQuantity = 0, CostPrice = 44.00m, LastUpdated = DateTime.UtcNow },
                    new DistributorStock { DistributorId = dist2.DistributorId, BlanketId = blankets[0].BlanketId, StockQuantity = 15, CostPrice = 71.00m, LastUpdated = DateTime.UtcNow },
                    new DistributorStock { DistributorId = dist2.DistributorId, BlanketId = blankets[2].BlanketId, StockQuantity = 40, CostPrice = 43.00m, LastUpdated = DateTime.UtcNow },
                    new DistributorStock { DistributorId = dist2.DistributorId, BlanketId = blankets[3].BlanketId, StockQuantity = 0, CostPrice = 36.00m, LastUpdated = DateTime.UtcNow },
                    new DistributorStock { DistributorId = dist3.DistributorId, BlanketId = blankets[1].BlanketId, StockQuantity = 25, CostPrice = 59.00m, LastUpdated = DateTime.UtcNow },
                    new DistributorStock { DistributorId = dist3.DistributorId, BlanketId = blankets[3].BlanketId, StockQuantity = 18, CostPrice = 35.00m, LastUpdated = DateTime.UtcNow },
                    new DistributorStock { DistributorId = dist3.DistributorId, BlanketId = blankets[4].BlanketId, StockQuantity = 22, CostPrice = 52.00m, LastUpdated = DateTime.UtcNow }
                };
                context.DistributorStocks.AddRange(distStocks);
                await context.SaveChangesAsync();

                // ── Sellers ────────────────────────────────────────────────────────
                var seller1 = new Seller
                {
                    UserId = sellerUser1.UserId,
                    Name = "Nest & Home Boutique",
                    ContactEmail = "owner@nestandhome.com",
                    ContactPhone = "+1-555-401-0001",
                    AssignedDistributorId = dist1.DistributorId,
                    ShippingAddress = "12 Maple Street, Portland, OR 97201",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };
                var seller2 = new Seller
                {
                    UserId = sellerUser2.UserId,
                    Name = "Snug Shop Online",
                    ContactEmail = "sales@snugshop.com",
                    ContactPhone = "+1-555-402-0002",
                    AssignedDistributorId = dist2.DistributorId,
                    ShippingAddress = "88 Commerce Ave, Austin, TX 78701",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };
                var seller3 = new Seller
                {
                    UserId = sellerUser3.UserId,
                    Name = "Cloud Linens Co",
                    ContactEmail = "contact@cloudlinens.com",
                    ContactPhone = "+1-555-403-0003",
                    AssignedDistributorId = dist3.DistributorId,
                    ShippingAddress = "5 Harbor View, Seattle, WA 98101",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };
                var sellers = new List<Seller> { seller1, seller2, seller3 };
                context.Sellers.AddRange(sellers);
                await context.SaveChangesAsync();

                // ── Seller Stock ───────────────────────────────────────────────────
                var sellerStocks = new List<SellerStock>
                {
                    new SellerStock { SellerId = seller1.SellerId, BlanketId = blankets[0].BlanketId, StockQuantity = 5, RetailPrice = 89.99m, LastUpdated = DateTime.UtcNow },
                    new SellerStock { SellerId = seller1.SellerId, BlanketId = blankets[1].BlanketId, StockQuantity = 0, RetailPrice = 74.99m, LastUpdated = DateTime.UtcNow },
                    new SellerStock { SellerId = seller2.SellerId, BlanketId = blankets[2].BlanketId, StockQuantity = 8, RetailPrice = 54.99m, LastUpdated = DateTime.UtcNow },
                    new SellerStock { SellerId = seller2.SellerId, BlanketId = blankets[3].BlanketId, StockQuantity = 0, RetailPrice = 44.99m, LastUpdated = DateTime.UtcNow },
                    new SellerStock { SellerId = seller3.SellerId, BlanketId = blankets[4].BlanketId, StockQuantity = 3, RetailPrice = 64.99m, LastUpdated = DateTime.UtcNow },
                    new SellerStock { SellerId = seller3.SellerId, BlanketId = blankets[1].BlanketId, StockQuantity = 0, RetailPrice = 74.99m, LastUpdated = DateTime.UtcNow }
                };
                context.SellerStocks.AddRange(sellerStocks);
                await context.SaveChangesAsync();

                Console.WriteLine("✅ Database seeded successfully with Cozy Comfort sample data!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding database: {ex.Message}");
                throw;
            }
        }
    }
}
