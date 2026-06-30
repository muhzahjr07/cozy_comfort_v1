using CozyComfort.API.Models.DTOs;
using CozyComfort.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CozyComfort.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StockCheckController : ControllerBase
    {
        private readonly IStockCheckService _stockCheckService;
        private readonly ILogger<StockCheckController> _logger;

        public StockCheckController(IStockCheckService stockCheckService, ILogger<StockCheckController> logger)
        {
            _stockCheckService = stockCheckService;
            _logger = logger;
        }

        [HttpPost("query")]
        public async Task<ActionResult<ApiResponse<StockCheckResponse>>> CheckStock([FromBody] StockCheckRequest request)
        {
            try
            {
                var result = await _stockCheckService.CheckStockAsync(request);
                return Ok(ApiResponse<StockCheckResponse>.SuccessResult(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<StockCheckResponse>.ErrorResult(ex.Message));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in stock check");
                return BadRequest(ApiResponse<StockCheckResponse>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("history")]
        public async Task<ActionResult<ApiResponse<IEnumerable<StockQueryHistoryResponse>>>> GetHistory([FromQuery] int? sellerId)
        {
            var result = await _stockCheckService.GetQueryHistoryAsync(sellerId);
            return Ok(ApiResponse<IEnumerable<StockQueryHistoryResponse>>.SuccessResult(result));
        }
    }
}
