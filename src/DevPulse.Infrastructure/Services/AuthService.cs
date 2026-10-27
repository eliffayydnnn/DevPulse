using DevPulse.Application.Contracts;
using DevPulse.Application.DTOs;
using DevPulse.Domain.Entities;
using DevPulse.Domain.Exceptions;
using DevPulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DevPulse.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly DevPulseDbContext _dbContext;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IJwtTokenGenerator _jwtGenerator;

    public AuthService(
        DevPulseDbContext dbContext,
        IPasswordHasherService passwordHasher,
        IJwtTokenGenerator jwtGenerator)
    {
        _dbContext = dbContext;
        _passwordHasher = passwordHasher;
        _jwtGenerator = jwtGenerator;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedUsername = request.Username.Trim().ToLowerInvariant();
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        // Check uniqueness
        var existingUser = await _dbContext.Users
            .AnyAsync(u => u.Username.ToLower() == normalizedUsername || u.Email.ToLower() == normalizedEmail, cancellationToken);

        if (existingUser)
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Account", new[] { "A user with the specified username or email already exists." } }
            });
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = new User
        {
            Username = request.Username.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = passwordHash,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var token = _jwtGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAtUtc = user.CreatedAtUtc
            }
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var query = request.UsernameOrEmail.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Username.ToLower() == query || u.Email.ToLower() == query, cancellationToken);

        if (user == null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                { "Credentials", new[] { "Invalid username/email or password." } }
            });
        }

        var token = _jwtGenerator.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7),
            User = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                CreatedAtUtc = user.CreatedAtUtc
            }
        };
    }

    public async Task<UserDto> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
        {
            throw new NotFoundException("User", userId);
        }

        return new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAtUtc = user.CreatedAtUtc
        };
    }
}
