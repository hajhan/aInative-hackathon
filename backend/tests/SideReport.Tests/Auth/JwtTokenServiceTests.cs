using FluentAssertions;
using Microsoft.Extensions.Configuration;
using SideReport.Infrastructure.Services;
using Xunit;

namespace SideReport.Tests.Auth;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _sut;

    public JwtTokenServiceTests()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = "test_secret_key_at_least_32_characters_long!",
                ["Jwt:Issuer"] = "SideReport",
                ["Jwt:Audience"] = "SideReportUsers",
                ["Jwt:ExpiryMinutes"] = "15"
            })
            .Build();

        _sut = new JwtTokenService(config);
    }

    [Fact]
    public void GenerateAccessToken_ShouldReturnNonEmptyString()
    {
        // Act
        var token = _sut.GenerateAccessToken(
            Guid.NewGuid().ToString(), "test@example.com", "테스트");

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT는 3개 부분으로 구성
    }

    [Fact]
    public void GenerateRefreshToken_ShouldReturnNonEmptyString()
    {
        // Act
        var token = _sut.GenerateRefreshToken();

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Length.Should().BeGreaterThan(20);
    }

    [Fact]
    public void GenerateRefreshToken_CalledTwice_ShouldReturnDifferentTokens()
    {
        // Act
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void GetUserIdFromExpiredToken_WithValidToken_ShouldReturnUserId()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var token = _sut.GenerateAccessToken(userId, "test@example.com", "테스트");

        // Act
        var extractedId = _sut.GetUserIdFromExpiredToken(token);

        // Assert
        extractedId.Should().Be(userId);
    }
}
