using DevPulse.Application.Common;
using DevPulse.Domain.Exceptions;
using FluentAssertions;
using Xunit;

namespace DevPulse.UnitTests;

public class GitHubQueryParserTests
{
    [Theory]
    [InlineData("dotnet/aspnetcore", "dotnet", "aspnetcore")]
    [InlineData("https://github.com/dotnet/aspnetcore", "dotnet", "aspnetcore")]
    [InlineData("https://github.com/dotnet/aspnetcore.git", "dotnet", "aspnetcore")]
    [InlineData("http://github.com/torvalds/linux", "torvalds", "linux")]
    [InlineData("github.com/facebook/react", "facebook", "react")]
    [InlineData("facebook/react/", "facebook", "react")]
    [InlineData("  dotnet/runtime  ", "dotnet", "runtime")]
    public void Parse_ValidInputs_ReturnsExpectedOwnerAndRepo(string input, string expectedOwner, string expectedRepo)
    {
        // Act
        var (owner, repo) = GitHubQueryParser.Parse(input);

        // Assert
        Assert.Equal(expectedOwner, owner);
        Assert.Equal(expectedRepo, repo);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("not-a-repo")]
    [InlineData("https://gitlab.com/owner/repo")]
    [InlineData("https://random-site.org")]
    public void Parse_InvalidInputs_ThrowsValidationException(string? input)
    {
        // Act & Assert
        Assert.Throws<ValidationException>(() => GitHubQueryParser.Parse(input));
    }
}
