using CozyComfort.API.Data;
using CozyComfort.API.Models.DTOs;
using CozyComfort.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CozyComfort.API.Services.Implementations
{
    public class DistributorService : IDistributorService
    {
        private readonly ApplicationDbContext _context;

        public DistributorService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DistributorResponse>> GetAllDistributorsAsync() =>
            await _context.Distributors.Where(d => d.IsActive)
                .Select(d => MapToResponse(d)).ToListAsync();

        public async Task<DistributorResponse> GetDistributorByIdAsync(int id)
        {
            var d = await _context.Distributors.FindAsync(id)
                ?? throw new KeyNotFoundException($"Distributor {id} not found.");
            return MapToResponse(d);
        }

        public async Task<DistributorResponse> UpdateDistributorAsync(int id, UpdateDistributorRequest request)
        {
            var d = await _context.Distributors.FindAsync(id)
                ?? throw new KeyNotFoundException($"Distributor {id} not found.");
            d.Name = request.Name;
            d.ContactEmail = request.ContactEmail;
            d.ContactPhone = request.ContactPhone;
            d.ShippingCost = request.ShippingCost;
            d.LogisticsSpeedDays = request.LogisticsSpeedDays;
            d.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return MapToResponse(d);
        }

        public async Task<IEnumerable<DistributorStockResponse>> GetDistributorStockAsync(int distributorId) =>
            await _context.DistributorStocks
                .Include(ds => ds.Blanket)
                .Where(ds => ds.DistributorId == distributorId)
                .Select(ds => new DistributorStockResponse
                {
                    DistributorStockId = ds.DistributorStockId,
                    DistributorId = ds.DistributorId,
                    BlanketId = ds.BlanketId,
                    BlanketName = ds.Blanket.Name,
                    StockQuantity = ds.StockQuantity,
                    CostPrice = ds.CostPrice,
                    LastUpdated = ds.LastUpdated
                }).ToListAsync();

        public async Task<DistributorStockResponse> UpdateDistributorStockAsync(int distributorId, UpdateDistributorStockRequest request)
        {
            var ds = await _context.DistributorStocks
                .Include(s => s.Blanket)
                .FirstOrDefaultAsync(s => s.DistributorId == distributorId && s.BlanketId == request.BlanketId);

            if (ds == null)
            {
                ds = new Models.Domain.DistributorStock
                {
                    DistributorId = distributorId,
                    BlanketId = request.BlanketId,
                    StockQuantity = request.StockQuantity,
                    CostPrice = request.CostPrice,
                    LastUpdated = DateTime.UtcNow
                };
                _context.DistributorStocks.Add(ds);
            }
            else
            {
                ds.StockQuantity = request.StockQuantity;
                ds.CostPrice = request.CostPrice;
                ds.LastUpdated = DateTime.UtcNow;
            }
            await _context.SaveChangesAsync();

            var blanket = await _context.Blankets.FindAsync(request.BlanketId);
            return new DistributorStockResponse
            {
                DistributorStockId = ds.DistributorStockId,
                DistributorId = ds.DistributorId,
                BlanketId = ds.BlanketId,
                BlanketName = blanket?.Name ?? "",
                StockQuantity = ds.StockQuantity,
                CostPrice = ds.CostPrice,
                LastUpdated = ds.LastUpdated
            };
        }

        private static DistributorResponse MapToResponse(Models.Domain.Distributor d) => new()
        {
            DistributorId = d.DistributorId,
            Name = d.Name,
            ContactEmail = d.ContactEmail,
            ContactPhone = d.ContactPhone,
            ShippingCost = d.ShippingCost,
            LogisticsSpeedDays = d.LogisticsSpeedDays,
            SuccessRate = d.SuccessRate,
            AvgResponseTimeMs = d.AvgResponseTimeMs,
            IsActive = d.IsActive
        };
    }
}
