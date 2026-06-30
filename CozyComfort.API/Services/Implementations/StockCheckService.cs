using CozyComfort.API.Data;
using CozyComfort.API.Models.Domain;
using CozyComfort.API.Models.DTOs;
using CozyComfort.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CozyComfort.API.Services.Implementations
{
    public class StockCheckService : IStockCheckService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StockCheckService> _logger;

        public StockCheckService(ApplicationDbContext context, ILogger<StockCheckService> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Core SOC supply chain algorithm.
        /// Tier 1: Seller local stock      -> LeadTime = 0 days
        /// Tier 2: Distributor stock        -> LeadTime = distributor.LogisticsSpeedDays
        /// Tier 3: Manufacturer warehouse   -> LeadTime = mfgLeadTime + distributor.LogisticsSpeedDays
        /// Tier 4: Production required      -> LeadTime = ceil(qty/dailyCap) + mfgLead + distSpeed
        /// </summary>
        public async Task<StockCheckResponse> CheckStockAsync(StockCheckRequest request)
        {
            _logger.LogInformation("StockCheck: Seller={SellerId}, Blanket={BlanketId}, Qty={Qty}",
                request.SellerId, request.BlanketId, request.Quantity);

            // Load all entities in one round-trip
            var seller = await _context.Sellers
                .Include(s => s.AssignedDistributor)
                .FirstOrDefaultAsync(s => s.SellerId == request.SellerId)
                ?? throw new KeyNotFoundException($"Seller {request.SellerId} not found.");

            var blanket = await _context.Blankets.FindAsync(request.BlanketId)
                ?? throw new KeyNotFoundException($"Blanket {request.BlanketId} not found.");

            var sellerStock = await _context.SellerStocks
                .FirstOrDefaultAsync(ss => ss.SellerId == request.SellerId && ss.BlanketId == request.BlanketId);

            var distributorStock = seller.AssignedDistributorId.HasValue
                ? await _context.DistributorStocks
                    .FirstOrDefaultAsync(ds => ds.DistributorId == seller.AssignedDistributorId && ds.BlanketId == request.BlanketId)
                : null;

            var mfgStock = await _context.ManufacturerStocks
                .FirstOrDefaultAsync(ms => ms.BlanketId == request.BlanketId);

            var distributor = seller.AssignedDistributor;

            int sellerQty = sellerStock?.StockQuantity ?? 0;
            int distQty = distributorStock?.StockQuantity ?? 0;
            int mfgQty = mfgStock?.StockQuantity ?? 0;
            int dailyCap = mfgStock?.DailyProductionCapacity ?? 10;
            int mfgLead = mfgStock?.LeadTimeDays ?? 2;
            int distSpeed = distributor?.LogisticsSpeedDays ?? 3;
            decimal priceQuoted = sellerStock?.RetailPrice
                ?? distributorStock?.CostPrice
                ?? blanket.BasePrice;

            // ── Tier determination ─────────────────────────────────────────────
            int leadDays;
            string route;
            string sellerStatus = sellerQty >= request.Quantity ? "InStock" : "OutOfStock";
            string distStatus;
            string mfgStatus;

            if (sellerQty >= request.Quantity)
            {
                // Tier 1: Seller has sufficient stock
                leadDays = 0;
                route = "SellerStock";
                distStatus = distQty > 0 ? "InStock" : "OutOfStock";
                mfgStatus = mfgQty > 0 ? "InStock" : "OutOfStock";
            }
            else if (distributor != null && distQty >= request.Quantity)
            {
                // Tier 2: Distributor has sufficient stock
                leadDays = distSpeed;
                route = "DistributorStock";
                sellerStatus = "OutOfStock";
                distStatus = "InStock";
                mfgStatus = mfgQty > 0 ? "InStock" : "OutOfStock";
            }
            else if (mfgQty >= request.Quantity)
            {
                // Tier 3: Manufacturer warehouse has stock
                leadDays = mfgLead + distSpeed;
                route = "ManufacturerStock";
                sellerStatus = "OutOfStock";
                distStatus = distQty > 0 ? "InStock" : "OutOfStock";
                mfgStatus = "InStock";
            }
            else
            {
                // Tier 4: Production required
                int productionDays = (int)Math.Ceiling((double)request.Quantity / dailyCap);
                leadDays = productionDays + mfgLead + distSpeed;
                route = "ProductionRequired";
                sellerStatus = "OutOfStock";
                distStatus = distQty > 0 ? "InStock" : "OutOfStock";
                mfgStatus = "OutOfStock";
            }

            var response = new StockCheckResponse
            {
                SellerId = seller.SellerId,
                SellerName = seller.Name,
                BlanketId = blanket.BlanketId,
                BlanketName = blanket.Name,
                QuantityRequested = request.Quantity,
                SellerStockLevel = sellerQty,
                SellerStockStatus = sellerStatus,
                DistributorId = distributor?.DistributorId,
                DistributorName = distributor?.Name ?? "No Distributor Assigned",
                DistributorStockLevel = distQty,
                DistributorStockStatus = distributor == null ? "NoDistributor" : distStatus,
                ManufacturerStockLevel = mfgQty,
                ManufacturerStockStatus = mfgStatus,
                DailyProductionCapacity = dailyCap,
                CalculatedLeadTimeDays = leadDays,
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(leadDays),
                PriceQuoted = priceQuoted,
                RouteTaken = route
            };

            // Persist query history
            var queryRecord = new SupplyChainQuery
            {
                SellerId = request.SellerId,
                BlanketId = request.BlanketId,
                QuantityRequested = request.Quantity,
                SellerStockLevel = sellerQty,
                DistributorStockLevel = distQty,
                ManufacturerStockLevel = mfgQty,
                DailyProductionCapacity = dailyCap,
                CalculatedLeadTimeDays = leadDays,
                EstimatedDeliveryDate = response.EstimatedDeliveryDate,
                DistributorCostQuoted = priceQuoted,
                QueryDate = DateTime.UtcNow
            };
            _context.SupplyChainQueries.Add(queryRecord);
            await _context.SaveChangesAsync();

            return response;
        }

        public async Task<IEnumerable<StockQueryHistoryResponse>> GetQueryHistoryAsync(int? sellerId = null)
        {
            var query = _context.SupplyChainQueries
                .Include(q => q.Seller)
                .Include(q => q.Blanket)
                .AsQueryable();

            if (sellerId.HasValue)
                query = query.Where(q => q.SellerId == sellerId.Value);

            return await query
                .OrderByDescending(q => q.QueryDate)
                .Select(q => new StockQueryHistoryResponse
                {
                    SupplyChainQueryId = q.SupplyChainQueryId,
                    SellerId = q.SellerId,
                    SellerName = q.Seller.Name,
                    BlanketId = q.BlanketId,
                    BlanketName = q.Blanket.Name,
                    QuantityRequested = q.QuantityRequested,
                    SellerStockLevel = q.SellerStockLevel,
                    DistributorStockLevel = q.DistributorStockLevel,
                    ManufacturerStockLevel = q.ManufacturerStockLevel,
                    CalculatedLeadTimeDays = q.CalculatedLeadTimeDays,
                    EstimatedDeliveryDate = q.EstimatedDeliveryDate,
                    DistributorCostQuoted = q.DistributorCostQuoted,
                    QueryDate = q.QueryDate
                })
                .Take(100)
                .ToListAsync();
        }
    }
}
