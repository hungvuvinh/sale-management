using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using SaleManagement.Api.Models;

namespace SaleManagement.Api.Services;

public interface IJwtService
{
    (string Token, int ExpiresInSeconds) GenerateToken(UserProfileDto user);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _config;

    public JwtService(IConfiguration config)
    {
        _config = config;
    }

    public (string Token, int ExpiresInSeconds) GenerateToken(UserProfileDto user)
    {
        var secret = _config["Jwt:Secret"] ?? "your_super_secret_jwt_key_change_in_production_2026";
        var issuer = _config["Jwt:Issuer"] ?? "SaleManagement.Api";
        var audience = _config["Jwt:Audience"] ?? "SaleManagement.Client";
        var expiryHours = int.TryParse(_config["Jwt:ExpiryHours"], out var hours) ? hours : 8;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role),
            new("user_id", user.Id.ToString()),
            new("role", user.Role)
        };

        if (user.StoreId.HasValue)
        {
            claims.Add(new Claim("store_id", user.StoreId.Value.ToString()));
        }

        var expires = DateTime.UtcNow.AddHours(expiryHours);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        var expiresInSeconds = (int)(expires - DateTime.UtcNow).TotalSeconds;

        return (tokenString, expiresInSeconds);
    }
}
