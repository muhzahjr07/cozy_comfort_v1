using CozyComfort.API.Data;
using CozyComfort.API.Models.Domain;
using CozyComfort.API.Models.DTOs;
using CozyComfort.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CozyComfort.API.Services.Implementations
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<OrderService> _logger;

        public OrderService(ApplicationDbContext context, ILogger<OrderService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            var seller = await _context.Sellers.Include(s => s.AssignedDistributor)
                .FirstOrDefaultAsync(s => s.SellerId == request.SellerId)
                ?? throw new KeyNotFoundException($"Seller {request.SellerId} not found.");

            var orderRef = $"CC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
            decimal total = request.Items.Sum(i => i.Quantity * i.UnitPrice);

            var order = new Order
            {
                OrderReference = orderRef,
                SellerId = request.SellerId,
                DistributorId = seller.AssignedDistributorId,
                CustomerName = request.CustomerName,
                CustomerEmail = request.CustomerEmail,
                DeliveryAddress = request.DeliveryAddress,
                OrderDate = DateTime.UtcNow,
                Status = "Processing",
                TotalAmount = total,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow,
                ModifiedAt = DateTime.UtcNow
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Add order items and deduct seller stock, trigger production if needed
            foreach (var item in request.Items)
            {
                var orderItem = new OrderItem
                {
                    OrderId = order.OrderId,
                    BlanketId = item.BlanketId,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.Quantity * item.UnitPrice
                };
                _context.OrderItems.Add(orderItem);

                // Deduct from seller stock
                var sellerStock = await _context.SellerStocks
                    .FirstOrDefaultAsync(ss => ss.SellerId == request.SellerId && ss.BlanketId == item.BlanketId);
                if (sellerStock != null && sellerStock.StockQuantity >= item.Quantity)
                {
                    sellerStock.StockQuantity -= item.Quantity;
                    sellerStock.LastUpdated = DateTime.UtcNow;
                }
                else
                {
                    // Stock not at seller — check distributor and manufacturer
                    var distStock = seller.AssignedDistributorId.HasValue
                        ? await _context.DistributorStocks
                            .FirstOrDefaultAsync(ds => ds.DistributorId == seller.AssignedDistributorId && ds.BlanketId == item.BlanketId)
                        : null;

                    if (distStock != null && distStock.StockQuantity >= item.Quantity)
                    {
                        distStock.StockQuantity -= item.Quantity;
                        distStock.LastUpdated = DateTime.UtcNow;
                        order.Status = "Processing";
                        order.EstimatedDeliveryDate = DateTime.UtcNow.AddDays(seller.AssignedDistributor?.LogisticsSpeedDays ?? 3);
                    }
                    else
                    {
                        // Trigger production order
                        var mfgStock = await _context.ManufacturerStocks
                            .FirstOrDefaultAsync(ms => ms.BlanketId == item.BlanketId);

                        int dailyCap = mfgStock?.DailyProductionCapacity ?? 10;
                        int mfgLead = mfgStock?.LeadTimeDays ?? 2;
                        int distSpeed = seller.AssignedDistributor?.LogisticsSpeedDays ?? 3;
                        int productionDays = (int)Math.Ceiling((double)item.Quantity / dailyCap);
                        int totalDays = productionDays + mfgLead + distSpeed;

                        var productionOrder = new ProductionOrder
                        {
                            OrderId = order.OrderId,
                            DistributorId = seller.AssignedDistributorId ?? 0,
                            BlanketId = item.BlanketId,
                            Quantity = item.Quantity,
                            Status = "Scheduled",
                            ScheduledDate = DateTime.UtcNow.AddDays(mfgLead),
                            EstimatedCompletionDate = DateTime.UtcNow.AddDays(productionDays + mfgLead),
                            CreatedAt = DateTime.UtcNow,
                            ModifiedAt = DateTime.UtcNow
                        };
                        _context.ProductionOrders.Add(productionOrder);
                        order.Status = "AwaitingProduction";
                        order.EstimatedDeliveryDate = DateTime.UtcNow.AddDays(totalDays);
                    }
                }
            }

            order.ModifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return await GetOrderByIdAsync(order.OrderId);
        }

        public async Task<OrderResponse> GetOrderByIdAsync(int id)
        {
            var order = await _context.Orders
                .Include(o => o.Seller)
                .Include(o => o.Distributor)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Blanket)
                .Include(o => o.ProductionOrders).ThenInclude(po => po.Blanket)
                .Include(o => o.ProductionOrders).ThenInclude(po => po.Distributor)
                .FirstOrDefaultAsync(o => o.OrderId == id)
                ?? throw new KeyNotFoundException($"Order {id} not found.");

            return MapToResponse(order);
        }

        public async Task<IEnumerable<OrderResponse>> GetOrdersBySellerAsync(int sellerId) =>
            await GetOrdersQuery().Where(o => o.SellerId == sellerId)
                .OrderByDescending(o => o.CreatedAt).Select(o => MapToResponse(o)).ToListAsync();

        public async Task<IEnumerable<OrderResponse>> GetOrdersByDistributorAsync(int distributorId) =>
            await GetOrdersQuery().Where(o => o.DistributorId == distributorId)
                .OrderByDescending(o => o.CreatedAt).Select(o => MapToResponse(o)).ToListAsync();

        public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync() =>
            await GetOrdersQuery().OrderByDescending(o => o.CreatedAt).Select(o => MapToResponse(o)).ToListAsync();

        public async Task<OrderResponse> UpdateOrderStatusAsync(int id, UpdateOrderStatusRequest request)
        {
            var order = await _context.Orders.FindAsync(id)
                ?? throw new KeyNotFoundException($"Order {id} not found.");
            order.Status = request.Status;
            order.ModifiedAt = DateTime.UtcNow;
            if (request.Status == "Delivered") order.ActualDeliveryDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return await GetOrderByIdAsync(id);
        }

        public async Task<IEnumerable<ProductionOrderResponse>> GetProductionOrdersAsync(int? distributorId = null)
        {
            var query = _context.ProductionOrders
                .Include(po => po.Order)
                .Include(po => po.Blanket)
                .Include(po => po.Distributor)
                .AsQueryable();
            if (distributorId.HasValue)
                query = query.Where(po => po.DistributorId == distributorId.Value);
            return await query.OrderByDescending(po => po.CreatedAt).Select(po => MapProductionResponse(po)).ToListAsync();
        }

        public async Task<ProductionOrderResponse> UpdateProductionOrderStatusAsync(int id, string status)
        {
            var po = await _context.ProductionOrders
                .Include(p => p.Order)
                .Include(p => p.Blanket)
                .Include(p => p.Distributor)
                .FirstOrDefaultAsync(p => p.ProductionOrderId == id)
                ?? throw new KeyNotFoundException($"Production order {id} not found.");

            po.Status = status;
            po.ModifiedAt = DateTime.UtcNow;

            if (status == "Completed")
            {
                // Replenish manufacturer → deduct from mfg stock, add to distributor
                var mfgStock = await _context.ManufacturerStocks.FirstOrDefaultAsync(ms => ms.BlanketId == po.BlanketId);
                if (mfgStock != null) mfgStock.StockQuantity = Math.Max(0, mfgStock.StockQuantity - po.Quantity);

                var distStock = await _context.DistributorStocks
                    .FirstOrDefaultAsync(ds => ds.DistributorId == po.DistributorId && ds.BlanketId == po.BlanketId);
                if (distStock != null) distStock.StockQuantity += po.Quantity;

                // Advance parent order status
                var order = await _context.Orders.FindAsync(po.OrderId);
                if (order != null) { order.Status = "ShippedToDistributor"; order.ModifiedAt = DateTime.UtcNow; }
            }

            await _context.SaveChangesAsync();
            return MapProductionResponse(po);
        }

        public async Task<DashboardStatsResponse> GetDashboardStatsAsync()
        {
            var orders = await _context.Orders.Include(o => o.Distributor).ToListAsync();
            var distributors = await _context.Distributors.ToListAsync();

            return new DashboardStatsResponse
            {
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.Status is "Processing" or "CheckingStock"),
                AwaitingProductionOrders = orders.Count(o => o.Status == "AwaitingProduction"),
                CompletedOrders = orders.Count(o => o.Status == "Delivered"),
                TotalSellers = await _context.Sellers.CountAsync(),
                TotalDistributors = distributors.Count,
                TotalRevenue = orders.Where(o => o.Status == "Delivered").Sum(o => o.TotalAmount),
                OrdersByStatus = orders.GroupBy(o => o.Status)
                    .Select(g => new OrderStatusCount { Status = g.Key, Count = g.Count() }).ToList(),
                DailyOrders = orders.GroupBy(o => o.CreatedAt.Date)
                    .OrderByDescending(g => g.Key).Take(14)
                    .Select(g => new DailyOrderCount { Date = g.Key, Count = g.Count(), Revenue = g.Sum(o => o.TotalAmount) }).ToList(),
                DistributorPerformance = distributors.Select(d => new DistributorPerformanceSummary
                {
                    DistributorName = d.Name,
                    OrderCount = orders.Count(o => o.DistributorId == d.DistributorId),
                    SuccessRate = d.SuccessRate,
                    AverageResponseTime = d.AvgResponseTimeMs
                }).ToList()
            };
        }

        private IQueryable<Order> GetOrdersQuery() =>
            _context.Orders
                .Include(o => o.Seller).Include(o => o.Distributor)
                .Include(o => o.OrderItems).ThenInclude(oi => oi.Blanket)
                .Include(o => o.ProductionOrders).ThenInclude(po => po.Blanket)
                .Include(o => o.ProductionOrders).ThenInclude(po => po.Distributor);

        private static OrderResponse MapToResponse(Order o) => new()
        {
            OrderId = o.OrderId,
            OrderReference = o.OrderReference,
            SellerId = o.SellerId,
            SellerName = o.Seller?.Name ?? "",
            DistributorId = o.DistributorId,
            DistributorName = o.Distributor?.Name ?? "",
            CustomerName = o.CustomerName,
            CustomerEmail = o.CustomerEmail,
            DeliveryAddress = o.DeliveryAddress,
            OrderDate = o.OrderDate,
            Status = o.Status,
            TotalAmount = o.TotalAmount,
            Notes = o.Notes,
            EstimatedDeliveryDate = o.EstimatedDeliveryDate,
            ActualDeliveryDate = o.ActualDeliveryDate,
            CreatedAt = o.CreatedAt,
            ModifiedAt = o.ModifiedAt,
            OrderItems = o.OrderItems.Select(oi => new OrderItemResponse
            {
                OrderItemId = oi.OrderItemId,
                BlanketId = oi.BlanketId,
                BlanketName = oi.Blanket?.Name ?? "",
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.TotalPrice
            }).ToList(),
            ProductionOrders = o.ProductionOrders.Select(po => MapProductionResponse(po)).ToList()
        };

        private static ProductionOrderResponse MapProductionResponse(ProductionOrder po) => new()
        {
            ProductionOrderId = po.ProductionOrderId,
            OrderId = po.OrderId,
            OrderReference = po.Order?.OrderReference ?? "",
            DistributorId = po.DistributorId,
            DistributorName = po.Distributor?.Name ?? "",
            BlanketId = po.BlanketId,
            BlanketName = po.Blanket?.Name ?? "",
            Quantity = po.Quantity,
            Status = po.Status,
            ScheduledDate = po.ScheduledDate,
            EstimatedCompletionDate = po.EstimatedCompletionDate,
            CreatedAt = po.CreatedAt
        };
    }
}
