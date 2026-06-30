using CozyComfort.API.Models.DTOs;
using CozyComfort.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CozyComfort.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DistributorsController : ControllerBase
    {
        private readonly IDistributorService _distributorService;

        public DistributorsController(IDistributorService distributorService)
        {
            _distributorService = distributorService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<DistributorResponse>>>> GetAll()
        {
            var result = await _distributorService.GetAllDistributorsAsync();
            return Ok(ApiResponse<IEnumerable<DistributorResponse>>.SuccessResult(result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<DistributorResponse>>> Get(int id)
        {
            try
            {
                var result = await _distributorService.GetDistributorByIdAsync(id);
                return Ok(ApiResponse<DistributorResponse>.SuccessResult(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<DistributorResponse>.ErrorResult(ex.Message));
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<DistributorResponse>>> Update(int id, [FromBody] UpdateDistributorRequest request)
        {
            try
            {
                var result = await _distributorService.UpdateDistributorAsync(id, request);
                return Ok(ApiResponse<DistributorResponse>.SuccessResult(result, "Distributor updated."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<DistributorResponse>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("{id}/stock")]
        public async Task<ActionResult<ApiResponse<IEnumerable<DistributorStockResponse>>>> GetStock(int id)
        {
            var result = await _distributorService.GetDistributorStockAsync(id);
            return Ok(ApiResponse<IEnumerable<DistributorStockResponse>>.SuccessResult(result));
        }

        [HttpPut("{id}/stock")]
        public async Task<ActionResult<ApiResponse<DistributorStockResponse>>> UpdateStock(int id, [FromBody] UpdateDistributorStockRequest request)
        {
            try
            {
                var result = await _distributorService.UpdateDistributorStockAsync(id, request);
                return Ok(ApiResponse<DistributorStockResponse>.SuccessResult(result, "Stock updated."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<DistributorStockResponse>.ErrorResult(ex.Message));
            }
        }
    }
}
