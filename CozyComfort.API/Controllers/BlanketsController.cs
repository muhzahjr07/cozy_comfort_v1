using CozyComfort.API.Models.DTOs;
using CozyComfort.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CozyComfort.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlanketsController : ControllerBase
    {
        private readonly IBlanketService _blanketService;
        private readonly ILogger<BlanketsController> _logger;

        public BlanketsController(IBlanketService blanketService, ILogger<BlanketsController> logger)
        {
            _blanketService = blanketService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<BlanketSummaryResponse>>>> GetBlankets([FromQuery] BlanketSearchRequest request)
        {
            try
            {
                var result = await _blanketService.GetBlanketsAsync(request);
                return Ok(ApiResponse<PaginatedResponse<BlanketSummaryResponse>>.SuccessResult(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting blankets");
                return BadRequest(ApiResponse<PaginatedResponse<BlanketSummaryResponse>>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<BlanketResponse>>> GetBlanket(int id)
        {
            try
            {
                var result = await _blanketService.GetBlanketByIdAsync(id);
                return Ok(ApiResponse<BlanketResponse>.SuccessResult(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<BlanketResponse>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("materials")]
        public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetMaterials()
        {
            var result = await _blanketService.GetMaterialsAsync();
            return Ok(ApiResponse<IEnumerable<string>>.SuccessResult(result));
        }

        [HttpGet("sizes")]
        public async Task<ActionResult<ApiResponse<IEnumerable<string>>>> GetSizes()
        {
            var result = await _blanketService.GetSizesAsync();
            return Ok(ApiResponse<IEnumerable<string>>.SuccessResult(result));
        }

        [HttpPost]
        [Authorize(Roles = "Manufacturer")]
        public async Task<ActionResult<ApiResponse<BlanketResponse>>> CreateBlanket([FromBody] CreateBlanketRequest request)
        {
            try
            {
                var result = await _blanketService.CreateBlanketAsync(request);
                return CreatedAtAction(nameof(GetBlanket), new { id = result.BlanketId },
                    ApiResponse<BlanketResponse>.SuccessResult(result, "Blanket created successfully."));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<BlanketResponse>.ErrorResult(ex.Message));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Manufacturer")]
        public async Task<ActionResult<ApiResponse<BlanketResponse>>> UpdateBlanket(int id, [FromBody] CreateBlanketRequest request)
        {
            try
            {
                var result = await _blanketService.UpdateBlanketAsync(id, request);
                return Ok(ApiResponse<BlanketResponse>.SuccessResult(result, "Blanket updated successfully."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<BlanketResponse>.ErrorResult(ex.Message));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Manufacturer")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteBlanket(int id)
        {
            try
            {
                await _blanketService.DeleteBlanketAsync(id);
                return Ok(ApiResponse<bool>.SuccessResult(true, "Blanket deactivated."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<bool>.ErrorResult(ex.Message));
            }
        }

        [HttpPut("{id}/manufacturer-stock")]
        [Authorize(Roles = "Manufacturer")]
        public async Task<ActionResult<ApiResponse<BlanketResponse>>> UpdateManufacturerStock(
            int id, [FromBody] UpdateManufacturerStockRequest request)
        {
            try
            {
                var result = await _blanketService.UpdateManufacturerStockAsync(id, request.StockQuantity, request.DailyProductionCapacity, request.LeadTimeDays);
                return Ok(ApiResponse<BlanketResponse>.SuccessResult(result, "Manufacturer stock updated."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<BlanketResponse>.ErrorResult(ex.Message));
            }
        }
    }

    public class UpdateManufacturerStockRequest
    {
        public int StockQuantity { get; set; }
        public int DailyProductionCapacity { get; set; }
        public int LeadTimeDays { get; set; }
    }
}
