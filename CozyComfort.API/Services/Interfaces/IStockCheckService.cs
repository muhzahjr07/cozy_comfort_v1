using CozyComfort.API.Models.DTOs;

namespace CozyComfort.API.Services.Interfaces
{
    public interface IStockCheckService
    {
        Task<StockCheckResponse> CheckStockAsync(StockCheckRequest request);
        Task<IEnumerable<StockQueryHistoryResponse>> GetQueryHistoryAsync(int? sellerId = null);
    }
}
