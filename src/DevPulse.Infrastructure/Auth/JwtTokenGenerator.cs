using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DevPulse.Application.Contracts;
using DevPulse.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DevPulse.Infrastructure.Auth;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(User user)
    {
        var secret = _configuration["Jwt:SecretKey"]
            ?? "DevPulseSuperSecureKeyForJwtTokens_MustBeAtLeast32BytesLong!";
        var issuer = _configuration["Jwt:Issuer"] ?? "DevPulse";
        var audience = _configuration["Jwt:Audience"] ?? "DevPulseApp";
        var expiryDays = int.TryParse(_configuration["Jwt:ExpiryDays"], out var days) ? days : 7;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(JwtRegisteredClaimNames.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(expiryDays),
            Issuer = issuer,
            Audience = audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
