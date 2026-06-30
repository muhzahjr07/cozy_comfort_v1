using CozyComfort.API.Models.DTOs;

namespace CozyComfort.API.Services.Interfaces
{
    public interface IOrderService
    {
        Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
        Task<OrderResponse> GetOrderByIdAsync(int id);
        Task<IEnumerable<OrderResponse>> GetOrdersBySellerAsync(int sellerId);
        Task<IEnumerable<OrderResponse>> GetOrdersByDistributorAsync(int distributorId);
        Task<IEnumerable<OrderResponse>> GetAllOrdersAsync();
        Task<OrderResponse> UpdateOrderStatusAsync(int id, UpdateOrderStatusRequest request);
        Task<IEnumerable<ProductionOrderResponse>> GetProductionOrdersAsync(int? distributorId = null);
        Task<ProductionOrderResponse> UpdateProductionOrderStatusAsync(int id, string status);
        Task<DashboardStatsResponse> GetDashboardStatsAsync();
    }
}
