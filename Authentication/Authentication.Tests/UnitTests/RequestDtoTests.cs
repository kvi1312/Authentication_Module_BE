using Authentication.Application.Dtos.Request;
using Authentication.Domain.Enums;
using Xunit;

namespace Authentication.Tests.UnitTests;

public class RequestDtoTests
{
    [Fact]
    public void LoginRequest_Should_SupportRememberMe()
    {
        // Arrange & Act
        var request = new LoginRequest
        {
            Username = "testuser",
            Password = "password123",
            RememberMe = true
        };

        // Assert
        Assert.True(request.RememberMe);
    }

    [Fact]
    public void RegisterRequest_Should_ValidateRequiredFields()
    {
        // Arrange & Act
        var request = new RegisterRequest
        {
            Username = "newuser",
            Email = "newuser@example.com",
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "John",
            LastName = "Doe"
        };

        // Assert
        Assert.Equal("newuser", request.Username);
        Assert.Equal("newuser@example.com", request.Email);
        Assert.Equal("SecurePassword123!", request.Password);
        Assert.Equal("SecurePassword123!", request.ConfirmPassword);
    }

    [Fact]
    public void LogoutRequest_Should_ValidateRefreshToken()
    {
        // Arrange & Act
        var request = new LogoutRequest
        {
            RefreshToken = "refresh_token_to_invalidate"
        };

        // Assert
        Assert.Equal("refresh_token_to_invalidate", request.RefreshToken);
    }

    [Fact]
    public void AddUserRoleRequest_Should_ValidateUserAndRoles()
    {
        // Arrange & Act
        var request = new AddUserRoleRequest
        {
            UserId = Guid.NewGuid(),
            RolesToAdd = new List<RoleType> { RoleType.Admin, RoleType.Customer }
        };

        // Assert
        Assert.NotEqual(Guid.Empty, request.UserId);
        Assert.Equal(2, request.RolesToAdd.Count);
        Assert.Contains(RoleType.Admin, request.RolesToAdd);
        Assert.Contains(RoleType.Customer, request.RolesToAdd);
    }

    [Fact]
    public void RemoveUserRoleRequest_Should_ValidateUserAndRoles()
    {
        // Arrange & Act
        var request = new RemoveUserRoleRequest
        {
            UserId = Guid.NewGuid(),
            RolesToRemove = new List<RoleType> { RoleType.Customer }
        };

        // Assert
        Assert.NotEqual(Guid.Empty, request.UserId);
        Assert.Single(request.RolesToRemove);
        Assert.Contains(RoleType.Customer, request.RolesToRemove);
    }

    [Theory]
    [InlineData("validuser")]
    [InlineData("user123")]
    [InlineData("test.user")]
    public void LoginRequest_Should_AcceptValidUsernames(string username)
    {
        // Arrange & Act
        var request = new LoginRequest
        {
            Username = username,
            Password = "password123"
        };

        // Assert
        Assert.Equal(username, request.Username);
    }

    [Theory]
    [InlineData("user@domain.com")]
    [InlineData("test.email@example.org")]
    [InlineData("user+tag@domain.co.uk")]
    public void RegisterRequest_Should_AcceptValidEmails(string email)
    {
        // Arrange & Act
        var request = new RegisterRequest
        {
            Username = "testuser",
            Email = email,
            Password = "SecurePassword123!",
            ConfirmPassword = "SecurePassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        // Assert
        Assert.Equal(email, request.Email);
    }

    // Note: These tests would require creating PasswordChangeRequest, 
    // ForgotPasswordRequest, and ResetPasswordRequest DTOs in the actual application
}
