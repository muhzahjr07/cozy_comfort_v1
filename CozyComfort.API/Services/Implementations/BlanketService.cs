using CozyComfort.API.Data;
using CozyComfort.API.Models.Domain;
using CozyComfort.API.Models.DTOs;
using CozyComfort.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CozyComfort.API.Services.Implementations
{
    public class BlanketService : IBlanketService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BlanketService> _logger;

        public BlanketService(ApplicationDbContext context, ILogger<BlanketService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PaginatedResponse<BlanketSummaryResponse>> GetBlanketsAsync(BlanketSearchRequest request)
        {
            var query = _context.Blankets.Where(b => b.IsActive).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Query))
                query = query.Where(b => b.Name.Contains(request.Query) || b.Description.Contains(request.Query) || b.Material.Contains(request.Query));

            if (!string.IsNullOrWhiteSpace(request.Material))
                query = query.Where(b => b.Material == request.Material);

            if (!string.IsNullOrWhiteSpace(request.Size))
                query = query.Where(b => b.Size == request.Size);

            var total = await query.CountAsync();
            var blankets = await query
                .OrderBy(b => b.Name)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(b => new BlanketSummaryResponse
                {
                    BlanketId = b.BlanketId,
                    Name = b.Name,
                    Description = b.Description,
                    Material = b.Material,
                    Size = b.Size,
                    BasePrice = b.BasePrice,
                    ImageUrl = b.ImageUrl
                })
                .ToListAsync();

            return PaginatedResponse<BlanketSummaryResponse>.Create(blankets, request.PageNumber, request.PageSize, total);
        }

        public async Task<BlanketResponse> GetBlanketByIdAsync(int id)
        {
            var blanket = await _context.Blankets
                .Include(b => b.ManufacturerStock)
                .FirstOrDefaultAsync(b => b.BlanketId == id)
                ?? throw new KeyNotFoundException($"Blanket {id} not found.");

            return MapToResponse(blanket);
        }

        public async Task<IEnumerable<string>> GetMaterialsAsync() =>
            await _context.Blankets.Where(b => b.IsActive).Select(b => b.Material).Distinct().ToListAsync();

        public async Task<IEnumerable<string>> GetSizesAsync() =>
            await _context.Blankets.Where(b => b.IsActive).Select(b => b.Size).Distinct().ToListAsync();

        public async Task<BlanketResponse> CreateBlanketAsync(CreateBlanketRequest request)
        {
            var blanket = new Blanket
            {
                Name = request.Name,
                Description = request.Description,
                Material = request.Material,
                Size = request.Size,
                BasePrice = request.BasePrice,
                ImageUrl = request.ImageUrl,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };
            _context.Blankets.Add(blanket);
            await _context.SaveChangesAsync();

            var mfgStock = new ManufacturerStock
            {
                BlanketId = blanket.BlanketId,
                StockQuantity = request.InitialStock,
                DailyProductionCapacity = request.DailyProductionCapacity,
                LeadTimeDays = request.LeadTimeDays,
                LastUpdated = DateTime.UtcNow
            };
            _context.ManufacturerStocks.Add(mfgStock);
            await _context.SaveChangesAsync();

            blanket.ManufacturerStock = mfgStock;
            return MapToResponse(blanket);
        }

        public async Task<BlanketResponse> UpdateBlanketAsync(int id, CreateBlanketRequest request)
        {
            var blanket = await _context.Blankets
                .Include(b => b.ManufacturerStock)
                .FirstOrDefaultAsync(b => b.BlanketId == id)
                ?? throw new KeyNotFoundException($"Blanket {id} not found.");

            blanket.Name = request.Name;
            blanket.Description = request.Description;
            blanket.Material = request.Material;
            blanket.Size = request.Size;
            blanket.BasePrice = request.BasePrice;
            blanket.ImageUrl = request.ImageUrl;
            blanket.ModifiedAt = DateTime.UtcNow;

            if (blanket.ManufacturerStock != null)
            {
                blanket.ManufacturerStock.DailyProductionCapacity = request.DailyProductionCapacity;
                blanket.ManufacturerStock.LeadTimeDays = request.LeadTimeDays;
                blanket.ManufacturerStock.LastUpdated = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return MapToResponse(blanket);
        }

        public async Task<bool> DeleteBlanketAsync(int id)
        {
            var blanket = await _context.Blankets.FindAsync(id)
                ?? throw new KeyNotFoundException($"Blanket {id} not found.");
            blanket.IsActive = false;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<BlanketResponse> UpdateManufacturerStockAsync(int blanketId, int stockQuantity, int dailyCapacity, int leadTimeDays)
        {
            var mfgStock = await _context.ManufacturerStocks.FirstOrDefaultAsync(ms => ms.BlanketId == blanketId)
                ?? throw new KeyNotFoundException($"Manufacturer stock for blanket {blanketId} not found.");

            mfgStock.StockQuantity = stockQuantity;
            mfgStock.DailyProductionCapacity = dailyCapacity;
            mfgStock.LeadTimeDays = leadTimeDays;
            mfgStock.LastUpdated = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetBlanketByIdAsync(blanketId);
        }

        private static BlanketResponse MapToResponse(Blanket b) => new()
        {
            BlanketId = b.BlanketId,
            Name = b.Name,
            Description = b.Description,
            Material = b.Material,
            Size = b.Size,
            BasePrice = b.BasePrice,
            ImageUrl = b.ImageUrl,
            IsActive = b.IsActive,
            CreatedAt = b.CreatedAt,
            ModifiedAt = b.ModifiedAt,
            StockQuantity = b.ManufacturerStock?.StockQuantity ?? 0,
            DailyProductionCapacity = b.ManufacturerStock?.DailyProductionCapacity ?? 0,
            LeadTimeDays = b.ManufacturerStock?.LeadTimeDays ?? 0
        };
    }
}
