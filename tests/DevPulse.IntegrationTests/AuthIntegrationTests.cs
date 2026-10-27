using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using DevPulse.Application.Common;
using DevPulse.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DevPulse.IntegrationTests;

public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public AuthIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task RegisterAndLoginFlow_CreatesUserAndAuthenticatesSuccessfully()
    {
        // 1. Register a new user
        var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
        var registerPayload = new RegisterRequestDto
        {
            Username = $"user_{uniqueSuffix}",
            Email = $"dev_{uniqueSuffix}@test.com",
            Password = "SecurePassword123!"
        };

        var regContent = new StringContent(JsonSerializer.Serialize(registerPayload), Encoding.UTF8, "application/json");
        var regResponse = await _client.PostAsync("/api/auth/register", regContent);

        Assert.Equal(HttpStatusCode.OK, regResponse.StatusCode);
        var regJson = await regResponse.Content.ReadAsStringAsync();
        var regResult = JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(regJson, JsonOpts);

        Assert.NotNull(regResult);
        Assert.True(regResult.Success);
        Assert.NotNull(regResult.Data.Token);
        Assert.Equal(registerPayload.Username, regResult.Data.User.Username);

        // 2. Login with registered credentials
        var loginPayload = new LoginRequestDto
        {
            UsernameOrEmail = registerPayload.Username,
            Password = "SecurePassword123!"
        };

        var loginContent = new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json");
        var loginResponse = await _client.PostAsync("/api/auth/login", loginContent);

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loginJson = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<ApiResponse<AuthResponseDto>>(loginJson, JsonOpts);

        Assert.NotNull(loginResult);
        Assert.True(loginResult.Success);
        var jwtToken = loginResult.Data.Token;
        Assert.NotNull(jwtToken);

        // 3. Access protected /api/auth/me with Bearer token
        using var authRequest = new HttpRequestMessage(HttpMethod.Get, "/api/auth/me");
        authRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        var meResponse = await _client.SendAsync(authRequest);

        Assert.Equal(HttpStatusCode.OK, meResponse.StatusCode);
        var meJson = await meResponse.Content.ReadAsStringAsync();
        var meResult = JsonSerializer.Deserialize<ApiResponse<UserDto>>(meJson, JsonOpts);

        Assert.NotNull(meResult);
        Assert.Equal(registerPayload.Username, meResult.Data.Username);

        // 4. Access protected /api/saved-repositories with Bearer token
        using var savedRequest = new HttpRequestMessage(HttpMethod.Get, "/api/saved-repositories");
        savedRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
        var savedResponse = await _client.SendAsync(savedRequest);

        Assert.Equal(HttpStatusCode.OK, savedResponse.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoints_WithoutToken_ReturnsUnauthorized()
    {
        // Act: try accessing protected saved repositories without authorization header
        var response = await _client.GetAsync("/api/saved-repositories");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithIncorrectPassword_ReturnsBadRequestProblemDetails()
    {
        // Arrange
        var payload = new LoginRequestDto
        {
            UsernameOrEmail = "non_existent_user",
            Password = "WrongPassword99!"
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
