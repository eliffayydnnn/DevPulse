using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using DevPulse.Domain.Entities;
using DevPulse.Infrastructure.Auth;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DevPulse.UnitTests;

public class AuthTests
{
    private readonly PasswordHasherService _hasher = new();
    private readonly JwtTokenGenerator _jwtGenerator;

    public AuthTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Jwt:SecretKey", "TestDevPulseSecretKeyThatIsAtLeast32BytesLong!" },
                { "Jwt:Issuer", "DevPulseTest" },
                { "Jwt:Audience", "DevPulseTestApp" },
                { "Jwt:ExpiryDays", "1" }
            })
            .Build();

        _jwtGenerator = new JwtTokenGenerator(config);
    }

    [Fact]
    public void HashPassword_GeneratesSaltedHash_AndVerifiesCorrectly()
    {
        // Arrange
        var password = "SecurePassword123!";

        // Act
        var hash = _hasher.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.Contains(":", hash);
        Assert.True(_hasher.VerifyPassword(password, hash));
        Assert.False(_hasher.VerifyPassword("WrongPassword123!", hash));
    }

    [Fact]
    public void HashPassword_ProducesDifferentHashes_ForSamePasswordDueToSalt()
    {
        // Arrange
        var password = "MySamePassword99!";

        // Act
        var hash1 = _hasher.HashPassword(password);
        var hash2 = _hasher.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2);
        Assert.True(_hasher.VerifyPassword(password, hash1));
        Assert.True(_hasher.VerifyPassword(password, hash2));
    }

    [Fact]
    public void GenerateToken_IncludesUserClaimsAndExpiresInFuture()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Username = "devpulse_user",
            Email = "user@devpulse.io"
        };

        // Act
        var tokenString = _jwtGenerator.GenerateToken(user);

        // Assert
        Assert.NotNull(tokenString);

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(tokenString);

        Assert.Equal("DevPulseTest", jwt.Issuer);
        Assert.Contains("DevPulseTestApp", jwt.Audiences);
        Assert.True(jwt.ValidTo > DateTime.UtcNow);

        var subClaim = jwt.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub || c.Type == "sub");
        var nameClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name || c.Type == "unique_name" || c.Type == "name");
        var emailClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email || c.Type == "email");

        Assert.NotNull(subClaim);
        Assert.Equal(user.Id.ToString(), subClaim!.Value);
        Assert.Equal("devpulse_user", nameClaim?.Value);
        Assert.Equal("user@devpulse.io", emailClaim?.Value);
    }
}
