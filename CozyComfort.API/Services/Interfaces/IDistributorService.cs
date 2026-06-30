using CozyComfort.API.Models.DTOs;

namespace CozyComfort.API.Services.Interfaces
{
    public interface IDistributorService
    {
        Task<IEnumerable<DistributorResponse>> GetAllDistributorsAsync();
        Task<DistributorResponse> GetDistributorByIdAsync(int id);
        Task<DistributorResponse> UpdateDistributorAsync(int id, UpdateDistributorRequest request);
        Task<IEnumerable<DistributorStockResponse>> GetDistributorStockAsync(int distributorId);
        Task<DistributorStockResponse> UpdateDistributorStockAsync(int distributorId, UpdateDistributorStockRequest request);
    }
}
