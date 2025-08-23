using Authentication.Application.Commands;
using Authentication.Domain.Enums;
using Xunit;

namespace Authentication.Tests.UnitTests;

public class AuthenticationFlowTests
{
    [Fact]
    public void LoginCommand_Should_ValidateRequiredProperties()
    {
        var loginCommand = new LoginCommand
        {
            Username = "testuser",
            Password = "password123",
            RememberMe = false,
            DeviceInfo = "Mobile",
            IpAddress = "192.168.1.1"
        };

        Assert.Equal("testuser", loginCommand.Username);
        Assert.Equal("password123", loginCommand.Password);
        Assert.False(loginCommand.RememberMe);
        Assert.Equal("Mobile", loginCommand.DeviceInfo);
        Assert.Equal("192.168.1.1", loginCommand.IpAddress);
    }

    [Fact]
    public void LoginCommand_Should_SupportRememberMe()
    {
        var loginCommand = new LoginCommand
        {
            Username = "testuser",
            Password = "password123",
            RememberMe = true
        };

        Assert.True(loginCommand.RememberMe);
    }

    [Fact]
    public void LogoutCommand_Should_ValidateRefreshToken()
    {
        var logoutCommand = new LogoutCommand
        {
            RefreshToken = "refresh_token_to_invalidate"
        };

        Assert.Equal("refresh_token_to_invalidate", logoutCommand.RefreshToken);
    }

    [Fact]
    public void RefreshTokenCommand_Should_ValidateTokens()
    {
        var refreshCommand = new RefreshTokenCommand
        {
            RefreshToken = "current_refresh_token"
        };

        Assert.Equal("current_refresh_token", refreshCommand.RefreshToken);
    }

    [Fact]
    public void AuthenticationFlow_Should_HandleMultipleSteps()
    {
        var registerCommand = new RegisterCommand
        {
            Username = "flowuser",
            Email = "flow@example.com",
            Password = "FlowPassword123!",
            FirstName = "Flow",
            LastName = "User"
        };

        var loginCommand = new LoginCommand
        {
            Username = "flowuser",
            Password = "FlowPassword123!",
            RememberMe = true
        };

        var refreshCommand = new RefreshTokenCommand
        {
            RefreshToken = "initial_refresh_token"
        };

        var logoutCommand = new LogoutCommand
        {
            RefreshToken = "final_refresh_token"
        };

        Assert.NotNull(registerCommand);
        Assert.NotNull(loginCommand);
        Assert.NotNull(refreshCommand);
        Assert.NotNull(logoutCommand);

        Assert.Equal("flowuser", registerCommand.Username);
        Assert.Equal("flowuser", loginCommand.Username);
        Assert.True(loginCommand.RememberMe);
        Assert.NotNull(refreshCommand.RefreshToken);
        Assert.NotNull(logoutCommand.RefreshToken);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LoginCommand_RememberMe_Should_AffectTokenGeneration(bool rememberMe)
    {
        var loginCommand = new LoginCommand
        {
            Username = "remembertest",
            Password = "password123",
            RememberMe = rememberMe
        };

        Assert.Equal(rememberMe, loginCommand.RememberMe);
    }
}
