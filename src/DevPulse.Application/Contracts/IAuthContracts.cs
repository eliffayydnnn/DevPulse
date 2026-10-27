using DevPulse.Domain.Entities;

namespace DevPulse.Application.Contracts;

public interface IPasswordHasherService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
