using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using SideReport.Application.Auth.Commands;
using SideReport.Application.Interfaces;
using SideReport.Infrastructure.Identity;
using SideReport.Domain.Entities;
using SideReport.Domain.Exceptions;
using Xunit;

namespace SideReport.Tests.Auth;

public class AuthServiceTests
{
    [Fact]
    public void RegisterCommand_WithValidData_ShouldHaveCorrectProperties()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Name = "홍길동",
            Email = "hong@example.com",
            Password = "P@ssword123!"
        };

        // Assert
        command.Name.Should().Be("홍길동");
        command.Email.Should().Be("hong@example.com");
        command.Password.Should().Be("P@ssword123!");
    }

    [Fact]
    public void LoginCommand_WithValidData_ShouldHaveCorrectProperties()
    {
        // Arrange
        var command = new LoginCommand
        {
            Email = "hong@example.com",
            Password = "P@ssword123!"
        };

        // Assert
        command.Email.Should().Be("hong@example.com");
        command.Password.Should().Be("P@ssword123!");
    }

    [Fact]
    public void RefreshToken_WhenNotRevoked_ShouldBeActive()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "test-token",
            Expires = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        // Assert
        token.IsActive.Should().BeTrue();
    }

    [Fact]
    public void RefreshToken_WhenRevoked_ShouldNotBeActive()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "test-token",
            Expires = DateTime.UtcNow.AddDays(7),
            IsRevoked = true
        };

        // Assert
        token.IsActive.Should().BeFalse();
    }

    [Fact]
    public void RefreshToken_WhenExpired_ShouldNotBeActive()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "test-token",
            Expires = DateTime.UtcNow.AddDays(-1),
            IsRevoked = false
        };

        // Assert
        token.IsActive.Should().BeFalse();
    }
}
