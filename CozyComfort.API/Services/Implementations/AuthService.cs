using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using CozyComfort.API.Data;
using CozyComfort.API.Models.Domain;
using CozyComfort.API.Models.DTOs;
using CozyComfort.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace CozyComfort.API.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationDbContext context, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
                return new AuthResponse { IsSuccess = false, Message = "Email is already registered." };

            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            if (request.Role == "Seller")
            {
                var seller = new Seller
                {
                    UserId = user.UserId,
                    Name = request.SellerName ?? $"{request.FirstName}'s Store",
                    ContactEmail = request.Email,
                    ContactPhone = "",
                    ShippingAddress = request.SellerShippingAddress ?? "",
                    AssignedDistributorId = request.AssignedDistributorId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };
                _context.Sellers.Add(seller);
                await _context.SaveChangesAsync();
            }
            else if (request.Role == "Distributor")
            {
                var distributor = new Distributor
                {
                    UserId = user.UserId,
                    Name = request.DistributorName ?? $"{request.FirstName} Distribution",
                    ContactEmail = request.Email,
                    ContactPhone = request.DistributorContactPhone ?? "",
                    ShippingCost = 8.00m,
                    LogisticsSpeedDays = 3,
                    SuccessRate = 0.90m,
                    AvgResponseTimeMs = 1000,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow
                };
                _context.Distributors.Add(distributor);
                await _context.SaveChangesAsync();
            }

            return await LoginAsync(new LoginRequest { Email = request.Email, Password = request.Password });
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Seller)
                .Include(u => u.Distributor)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return new AuthResponse { IsSuccess = false, Message = "Invalid email or password." };

            if (!user.IsActive)
                return new AuthResponse { IsSuccess = false, Message = "Account is deactivated." };

            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return new AuthResponse
            {
                IsSuccess = true,
                Token = token,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                SellerId = user.Seller?.SellerId,
                DistributorId = user.Distributor?.DistributorId,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Message = "Login successful."
            };
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"] ?? "CozyComfortSuperSecretKeyThatIsAtLeast32Chars!";

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("SellerId", user.Seller?.SellerId.ToString() ?? ""),
                new Claim("DistributorId", user.Distributor?.DistributorId.ToString() ?? "")
            };

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"] ?? "CozyComfortAPI",
                audience: jwtSettings["Audience"] ?? "CozyComfortUsers",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
