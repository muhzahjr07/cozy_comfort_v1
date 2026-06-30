using CozyComfort.API.Models.DTOs;

namespace CozyComfort.API.Services.Interfaces
{
    public interface IBlanketService
    {
        Task<PaginatedResponse<BlanketSummaryResponse>> GetBlanketsAsync(BlanketSearchRequest request);
        Task<BlanketResponse> GetBlanketByIdAsync(int id);
        Task<IEnumerable<string>> GetMaterialsAsync();
        Task<IEnumerable<string>> GetSizesAsync();
        Task<BlanketResponse> CreateBlanketAsync(CreateBlanketRequest request);
        Task<BlanketResponse> UpdateBlanketAsync(int id, CreateBlanketRequest request);
        Task<bool> DeleteBlanketAsync(int id);
        Task<BlanketResponse> UpdateManufacturerStockAsync(int blanketId, int stockQuantity, int dailyCapacity, int leadTimeDays);
    }
}
