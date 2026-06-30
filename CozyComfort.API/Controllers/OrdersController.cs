using CozyComfort.API.Models.DTOs;
using CozyComfort.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CozyComfort.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponse>>>> GetAllOrders()
        {
            var result = await _orderService.GetAllOrdersAsync();
            return Ok(ApiResponse<IEnumerable<OrderResponse>>.SuccessResult(result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<OrderResponse>>> GetOrder(int id)
        {
            try
            {
                var result = await _orderService.GetOrderByIdAsync(id);
                return Ok(ApiResponse<OrderResponse>.SuccessResult(result));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OrderResponse>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("seller/{sellerId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponse>>>> GetOrdersBySeller(int sellerId)
        {
            var result = await _orderService.GetOrdersBySellerAsync(sellerId);
            return Ok(ApiResponse<IEnumerable<OrderResponse>>.SuccessResult(result));
        }

        [HttpGet("distributor/{distributorId}")]
        public async Task<ActionResult<ApiResponse<IEnumerable<OrderResponse>>>> GetOrdersByDistributor(int distributorId)
        {
            var result = await _orderService.GetOrdersByDistributorAsync(distributorId);
            return Ok(ApiResponse<IEnumerable<OrderResponse>>.SuccessResult(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<OrderResponse>>> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var result = await _orderService.CreateOrderAsync(request);
                return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId },
                    ApiResponse<OrderResponse>.SuccessResult(result, "Order created successfully."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return BadRequest(ApiResponse<OrderResponse>.ErrorResult(ex.Message));
            }
        }

        [HttpPut("{id}/status")]
        public async Task<ActionResult<ApiResponse<OrderResponse>>> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var result = await _orderService.UpdateOrderStatusAsync(id, request);
                return Ok(ApiResponse<OrderResponse>.SuccessResult(result, "Order status updated."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<OrderResponse>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("production")]
        public async Task<ActionResult<ApiResponse<IEnumerable<ProductionOrderResponse>>>> GetProductionOrders([FromQuery] int? distributorId)
        {
            var result = await _orderService.GetProductionOrdersAsync(distributorId);
            return Ok(ApiResponse<IEnumerable<ProductionOrderResponse>>.SuccessResult(result));
        }

        [HttpPut("production/{id}/status")]
        public async Task<ActionResult<ApiResponse<ProductionOrderResponse>>> UpdateProductionStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var result = await _orderService.UpdateProductionOrderStatusAsync(id, request.Status);
                return Ok(ApiResponse<ProductionOrderResponse>.SuccessResult(result, "Production order updated."));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<ProductionOrderResponse>.ErrorResult(ex.Message));
            }
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<DashboardStatsResponse>>> GetDashboard()
        {
            var result = await _orderService.GetDashboardStatsAsync();
            return Ok(ApiResponse<DashboardStatsResponse>.SuccessResult(result));
        }

        /// <summary>
        /// Public guest checkout — no authentication required.
        /// Automatically routes through the first seeded seller (ID = 1).
        /// </summary>
        [HttpPost("guest")]
        [AllowAnonymous]
        public async Task<ActionResult<ApiResponse<OrderResponse>>> CreateGuestOrder([FromBody] GuestOrderRequest request)
        {
            try
            {
                var orderRequest = new CreateOrderRequest
                {
                    SellerId = 1, // Default seller: Nest & Home (seeded)
                    CustomerName = request.CustomerName,
                    CustomerEmail = request.CustomerEmail,
                    DeliveryAddress = request.DeliveryAddress,
                    Notes = request.Notes,
                    Items = request.Items
                };
                var result = await _orderService.CreateOrderAsync(orderRequest);
                return CreatedAtAction(nameof(GetOrder), new { id = result.OrderId },
                    ApiResponse<OrderResponse>.SuccessResult(result, "Your order has been placed successfully!"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating guest order");
                return BadRequest(ApiResponse<OrderResponse>.ErrorResult(ex.Message));
            }
        }
    }
}
