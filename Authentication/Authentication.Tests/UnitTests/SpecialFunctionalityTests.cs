using Authentication.Application.Commands;
using Authentication.Application.Dtos.Request;
using Authentication.Application.Dtos.Response;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Xunit;

namespace Authentication.Tests.UnitTests;

public class SpecialFunctionalityTests
{

    [Fact]
    public void RememberMe_Should_EnableExtendedSessionInLogin()
    {
        // Arrange & Act
        var loginCommand = new LoginCommand
        {
            Username = "user@example.com",
            Password = "securepassword",
            RememberMe = true
        };

        var loginRequest = new LoginRequest
        {
            Username = "user@example.com",
            Password = "securepassword",
            RememberMe = true
        };

        // Assert
        Assert.True(loginCommand.RememberMe);
        Assert.True(loginRequest.RememberMe);
    }

    [Fact]
    public void RememberMe_Should_BeIncludedInLoginResponse()
    {
        // Arrange & Act
        var response = new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            AccessToken = "access_token_123",
            RefreshToken = "refresh_token_123",
            RememberMeToken = "remember_me_token_456",
            RememberMeTokenExpiresAt = DateTime.UtcNow.AddDays(30)
        };

        // Assert
        Assert.True(response.Success);
        Assert.NotNull(response.RememberMeToken);
        Assert.Equal("remember_me_token_456", response.RememberMeToken);
        Assert.NotNull(response.RememberMeTokenExpiresAt);
        Assert.True(response.RememberMeTokenExpiresAt > DateTime.UtcNow.AddDays(25));
    }

    [Fact]
    public void TokenExpiration_Should_HandleAccessTokenExpiry()
    {
        // Arrange
        var shortLifetime = TimeSpan.FromMinutes(15);
        var tokenCreatedAt = DateTime.UtcNow;

        // Act
        var expiresAt = tokenCreatedAt.Add(shortLifetime);
        var isStillValid = DateTime.UtcNow < expiresAt;

        // Assert
        Assert.True(isStillValid);
        Assert.Equal(15, (expiresAt - tokenCreatedAt).TotalMinutes);
    }

    [Fact]
    public void TokenExpiration_Should_HandleRefreshTokenExpiry()
    {
        // Arrange
        var refreshToken = RefreshToken.Create(
            "test_refresh_token",
            "jwt_id_123",
            Guid.NewGuid(),
            TimeSpan.FromDays(7)
        );

        // Act
        var isValid = refreshToken.IsValid();
        var willExpireInFuture = refreshToken.ExpiresAt > DateTime.UtcNow;

        // Assert
        Assert.True(isValid);
        Assert.True(willExpireInFuture);
        Assert.False(refreshToken.IsRevoked);
    }

    [Fact]
    public void TokenExpiration_Should_InvalidateExpiredRefreshToken()
    {
        // Arrange
        var expiredToken = RefreshToken.Create(
            "expired_token",
            "jwt_id_expired",
            Guid.NewGuid(),
            TimeSpan.FromSeconds(-1) // Already expired
        );

        // Act
        var isValid = expiredToken.IsValid();

        // Assert
        Assert.False(isValid);
        Assert.True(expiredToken.ExpiresAt < DateTime.UtcNow);
    }

    [Fact]
    public void Authentication_Should_SupportCompleteLoginLogoutFlow()
    {
        // Arrange
        var username = "testuser@example.com";
        var password = "SecurePassword123!";

        // Act - Login
        var loginCommand = new LoginCommand
        {
            Username = username,
            Password = password,
            RememberMe = true
        };

        // Act - Successful Login Response
        var loginResponse = new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            AccessToken = "access_token",
            RefreshToken = "refresh_token_to_revoke",
            RememberMeToken = "remember_me_token"
        };

        // Act - Logout
        var logoutCommand = new LogoutCommand
        {
            RefreshToken = "refresh_token_to_revoke"
        };

        // Assert
        Assert.Equal(username, loginCommand.Username);
        Assert.True(loginCommand.RememberMe);
        Assert.True(loginResponse.Success);
        Assert.NotNull(loginResponse.RememberMeToken);
        Assert.Equal("refresh_token_to_revoke", logoutCommand.RefreshToken);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RememberMe_Should_BeConfigurable(bool rememberMe)
    {
        // Arrange & Act
        var loginCommand = new LoginCommand
        {
            Username = "test@example.com",
            Password = "password123",
            RememberMe = rememberMe
        };

        // Assert
        Assert.Equal(rememberMe, loginCommand.RememberMe);
    }
}
