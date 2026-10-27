using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DevPulse.IntegrationTests;

public class HealthEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetHealth_ReturnsOk_WithHealthyStatus()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var root = doc.RootElement;

        Assert.Equal("Healthy", root.GetProperty("status").GetString());
        Assert.Equal("DevPulse.Api", root.GetProperty("service").GetString());
        Assert.Equal("1.0.0", root.GetProperty("version").GetString());
    }

    [Fact]
    public async Task ParseEndpoint_WithValidQuery_ReturnsOk()
    {
        // Arrange
        var payload = new StringContent(
            JsonSerializer.Serialize(new { query = "https://github.com/dotnet/runtime" }),
            System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/repositories/parse", payload);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);
        var data = doc.RootElement.GetProperty("data");

        Assert.Equal("dotnet", data.GetProperty("owner").GetString());
        Assert.Equal("runtime", data.GetProperty("repo").GetString());
    }

    [Fact]
    public async Task ParseEndpoint_WithInvalidQuery_ReturnsProblemDetailsBadRequest()
    {
        // Arrange
        var payload = new StringContent(
            JsonSerializer.Serialize(new { query = "invalid!format" }),
            System.Text.Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/repositories/parse", payload);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(content);

        Assert.Equal(400, doc.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Validation Failed", doc.RootElement.GetProperty("title").GetString());
    }
}
