using Authentication.Application.Dtos.Response;
using Xunit;

namespace Authentication.Tests.UnitTests;

public class ResponseDtoTests
{
    [Fact]
    public void LoginResponse_Should_IndicateSuccess_When_LoginSuccessful()
    {
        // Arrange & Act
        var response = new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            AccessToken = "access_token_123",
            RefreshToken = "refresh_token_123"
        };

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Login successful", response.Message);
        Assert.Equal("access_token_123", response.AccessToken);
        Assert.Equal("refresh_token_123", response.RefreshToken);
    }

    [Fact]
    public void LoginResponse_Should_IndicateFailure_When_LoginFailed()
    {
        // Arrange & Act
        var response = new LoginResponse
        {
            Success = false,
            Message = "Invalid credentials"
        };

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Invalid credentials", response.Message);
        Assert.Null(response.AccessToken);
        Assert.Null(response.RefreshToken);
    }

    [Fact]
    public void RegisterResponse_Should_IndicateSuccess_When_RegistrationSuccessful()
    {
        // Arrange & Act
        var response = new RegisterResponse
        {
            Success = true,
            Message = "Registration successful. You can now login."
        };

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Registration successful. You can now login.", response.Message);
    }

    [Fact]
    public void RegisterResponse_Should_IndicateFailure_When_RegistrationFailed()
    {
        // Arrange & Act
        var response = new RegisterResponse
        {
            Success = false,
            Message = "Username or email already exists"
        };

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Username or email already exists", response.Message);
    }

    [Fact]
    public void RefreshTokenResponse_Should_IndicateSuccess_When_TokenRefreshSuccessful()
    {
        // Arrange & Act
        var response = new RefreshTokenResponse
        {
            Success = true,
            Message = "Token refreshed successfully",
            AccessToken = "new_access_token",
            RefreshToken = "new_refresh_token",
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15),
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Token refreshed successfully", response.Message);
        Assert.Equal("new_access_token", response.AccessToken);
        Assert.Equal("new_refresh_token", response.RefreshToken);
        Assert.NotNull(response.AccessTokenExpiresAt);
        Assert.NotNull(response.RefreshTokenExpiresAt);
    }

    [Fact]
    public void RefreshTokenResponse_Should_IndicateFailure_When_TokenExpired()
    {
        // Arrange & Act
        var response = new RefreshTokenResponse
        {
            Success = false,
            Message = "Invalid or expired refresh token"
        };

        // Assert
        Assert.False(response.Success);
        Assert.Equal("Invalid or expired refresh token", response.Message);
        Assert.Null(response.AccessToken);
        Assert.Null(response.RefreshToken);
    }

    [Fact]
    public void UserManagementResponse_Should_IndicateSuccess_When_RoleAdded()
    {
        // Arrange & Act
        var response = new UserManagementResponse
        {
            Success = true,
            Message = "Roles added successfully"
        };

        // Assert
        Assert.True(response.Success);
        Assert.Equal("Roles added successfully", response.Message);
    }

    [Fact]
    public void UserManagementResponse_Should_IndicateFailure_When_UserNotFound()
    {
        // Arrange & Act
        var response = new UserManagementResponse
        {
            Success = false,
            Message = "User not found"
        };

        // Assert
        Assert.False(response.Success);
        Assert.Equal("User not found", response.Message);
    }

    [Fact]
    public void LoginResponse_Should_SupportRememberMeToken()
    {
        // Arrange & Act
        var response = new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            RememberMeToken = "remember_me_token"
        };

        // Assert
        Assert.True(response.Success);
        Assert.Equal("remember_me_token", response.RememberMeToken);
    }

    [Fact]
    public void LoginResponse_Should_SupportSessionId()
    {
        // Arrange & Act
        var response = new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            AccessToken = "access_token",
            RefreshToken = "refresh_token",
            SessionId = "session_123"
        };

        // Assert
        Assert.True(response.Success);
        Assert.Equal("session_123", response.SessionId);
    }

    [Theory]
    [InlineData(true, "Operation successful")]
    [InlineData(false, "Operation failed")]
    public void Response_Should_ReflectSuccessStatus(bool success, string message)
    {
        // Arrange & Act
        var response = new UserManagementResponse
        {
            Success = success,
            Message = message
        };

        // Assert
        Assert.Equal(success, response.Success);
        Assert.Equal(message, response.Message);
    }
}
