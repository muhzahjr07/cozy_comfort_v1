using CozyComfort.API.Models.DTOs;
using CozyComfort.API.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CozyComfort.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
        {
            try
            {
                var result = await _authService.RegisterAsync(request);
                if (!result.IsSuccess)
                    return BadRequest(ApiResponse<AuthResponse>.ErrorResult(result.Message));
                return Ok(ApiResponse<AuthResponse>.SuccessResult(result, "Registration successful."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return BadRequest(ApiResponse<AuthResponse>.ErrorResult(ex.Message));
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
        {
            try
            {
                var result = await _authService.LoginAsync(request);
                if (!result.IsSuccess)
                    return Unauthorized(ApiResponse<AuthResponse>.ErrorResult(result.Message));
                return Ok(ApiResponse<AuthResponse>.SuccessResult(result, "Login successful."));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging in");
                return BadRequest(ApiResponse<AuthResponse>.ErrorResult(ex.Message));
            }
        }
    }
}
