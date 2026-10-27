using DevPulse.Application.Contracts;
using DevPulse.Application.DTOs;
using DevPulse.Domain.Enums;
using DevPulse.Infrastructure.Persistence;
using DevPulse.Infrastructure.Services;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace DevPulse.UnitTests;

public class AnalyticsServiceTests
{
    private readonly Mock<IGitHubService> _gitHubMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly DevPulseDbContext _dbContext;
    private readonly AnalyticsService _sut;

    public AnalyticsServiceTests()
    {
        _gitHubMock = new Mock<IGitHubService>();
        _cacheMock = new Mock<ICacheService>();

        var options = new DbContextOptionsBuilder<DevPulseDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new DevPulseDbContext(options);

        _sut = new AnalyticsService(
            _gitHubMock.Object,
            _cacheMock.Object,
            _dbContext,
            NullLogger<AnalyticsService>.Instance);
    }

    [Fact]
    public async Task AnalyzeRepositoryAsync_WhenCacheMiss_FetchesDataAndComputesHealthyMetrics()
    {
        // Arrange
        var owner = "dotnet";
        var repo = "aspnetcore";

        _cacheMock.Setup(c => c.GetAsync<RepositoryAnalysisDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((RepositoryAnalysisDto?)null);

        _gitHubMock.Setup(g => g.GetRepositoryAsync(owner, repo, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new GitHubRepositoryDto
                   {
                       Owner = owner,
                       Name = repo,
                       FullName = $"{owner}/{repo}",
                       Stars = 35000,
                       Forks = 10000,
                       OpenIssues = 400,
                       PrimaryLanguage = "C#",
                       Description = "ASP.NET Core Web Framework",
                       License = "MIT",
                       IsArchived = false,
                       PushedAt = DateTime.UtcNow.AddHours(-2)
                   });

        _gitHubMock.Setup(g => g.GetCommitsAsync(owner, repo, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<CommitSummaryDto>
                   {
                       new() { Sha = "abc1234", Message = "feat: optimize pipeline", Date = DateTime.UtcNow.AddDays(-1), AuthorName = "Dev" },
                       new() { Sha = "def5678", Message = "fix: memory leak", Date = DateTime.UtcNow.AddDays(-3), AuthorName = "Dev" }
                   });

        _gitHubMock.Setup(g => g.GetPullRequestsAsync(owner, repo, ItemStateFilter.All, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<PullRequestSummaryDto>
                   {
                       new() { Number = 101, State = "closed", IsMerged = true, Title = "PR 1", CreatedAt = DateTime.UtcNow.AddDays(-5) },
                       new() { Number = 102, State = "open", Title = "PR 2", CreatedAt = DateTime.UtcNow.AddDays(-2) }
                   });

        _gitHubMock.Setup(g => g.GetIssuesAsync(owner, repo, ItemStateFilter.All, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<IssueSummaryDto>
                   {
                       new() { Number = 1, State = "closed", Title = "Issue 1", CreatedAt = DateTime.UtcNow.AddDays(-10) },
                       new() { Number = 2, State = "open", Title = "Issue 2", CreatedAt = DateTime.UtcNow.AddDays(-1) }
                   });

        _gitHubMock.Setup(g => g.GetLanguagesAsync(owner, repo, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<LanguageDistributionDto>
                   {
                       new() { Language = "C#", Bytes = 80000, Percentage = 80.0, ColorHex = "#178600" },
                       new() { Language = "TypeScript", Bytes = 20000, Percentage = 20.0, ColorHex = "#3178c6" }
                   });

        // Act
        var result = await _sut.AnalyzeRepositoryAsync(owner, repo);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("dotnet/aspnetcore", result.Repository.FullName);
        Assert.True(result.Metrics.HealthScore >= 70, $"Expected >= 70 but was {result.Metrics.HealthScore}");
        Assert.Contains(result.Metrics.HealthGrade, new[] { "A+", "A", "B" });
        Assert.Equal("Very Active", result.Metrics.ActivityStatus);
        Assert.True(result.Metrics.DaysSinceLastCommit <= 2);
        Assert.Equal(6, result.ActivityTrend.Count);
        Assert.False(result.IsFromCache);

        // Verify result was stored in cache
        _cacheMock.Verify(c => c.SetAsync(
            It.Is<string>(k => k.Contains("dotnet_aspnetcore")),
            It.IsAny<RepositoryAnalysisDto>(),
            It.IsAny<TimeSpan?>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CompareRepositoriesAsync_ReturnsComparisonMatrixAndIdentifiesWinner()
    {
        // Arrange
        _cacheMock.Setup(c => c.GetAsync<RepositoryAnalysisDto>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((RepositoryAnalysisDto?)null);

        // Repo 1: High stars
        _gitHubMock.Setup(g => g.GetRepositoryAsync("org1", "repo1", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new GitHubRepositoryDto { FullName = "org1/repo1", Owner = "org1", Name = "repo1", Stars = 50000, Forks = 8000, PrimaryLanguage = "TypeScript", License = "MIT", Description = "Popular project" });
        _gitHubMock.Setup(g => g.GetCommitsAsync("org1", "repo1", It.IsAny<int>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<CommitSummaryDto> { new() { Date = DateTime.UtcNow.AddDays(-2) } });
        _gitHubMock.Setup(g => g.GetPullRequestsAsync("org1", "repo1", It.IsAny<ItemStateFilter>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<PullRequestSummaryDto> { new() { State = "closed", IsMerged = true } });
        _gitHubMock.Setup(g => g.GetIssuesAsync("org1", "repo1", It.IsAny<ItemStateFilter>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<IssueSummaryDto> { new() { State = "closed" } });
        _gitHubMock.Setup(g => g.GetLanguagesAsync("org1", "repo1", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<LanguageDistributionDto>());

        // Repo 2: Lower stars
        _gitHubMock.Setup(g => g.GetRepositoryAsync("org2", "repo2", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new GitHubRepositoryDto { FullName = "org2/repo2", Owner = "org2", Name = "repo2", Stars = 10000, Forks = 2000, PrimaryLanguage = "C#", License = "MIT", Description = "Alternative project" });
        _gitHubMock.Setup(g => g.GetCommitsAsync("org2", "repo2", It.IsAny<int>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<CommitSummaryDto> { new() { Date = DateTime.UtcNow.AddDays(-1) } });
        _gitHubMock.Setup(g => g.GetPullRequestsAsync("org2", "repo2", It.IsAny<ItemStateFilter>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<PullRequestSummaryDto> { new() { State = "closed", IsMerged = true } });
        _gitHubMock.Setup(g => g.GetIssuesAsync("org2", "repo2", It.IsAny<ItemStateFilter>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<IssueSummaryDto> { new() { State = "closed" } });
        _gitHubMock.Setup(g => g.GetLanguagesAsync("org2", "repo2", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(new List<LanguageDistributionDto>());

        // Act
        var result = await _sut.CompareRepositoriesAsync(new[] { ("org1", "repo1"), ("org2", "repo2") });

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Repositories.Count);
        Assert.NotEmpty(result.ComparisonMatrix);

        var starsRow = result.ComparisonMatrix.FirstOrDefault(r => r.MetricKey == "Stars");
        Assert.NotNull(starsRow);
        Assert.Equal("org1/repo1", starsRow!.WinnerRepositoryFullName);
        Assert.False(string.IsNullOrWhiteSpace(result.OverallVerdict));
    }
}
