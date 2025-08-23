using Authentication.Application.Commands;
using Authentication.Application.Dtos.Response;
using Authentication.Domain.Enums;
using Xunit;

namespace Authentication.Tests.UnitTests;

public class CommandValidationTests
{
    [Fact]
    public void LoginCommand_Should_HaveCorrectProperties()
    {
        // Arrange & Act
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = "password123",
            RememberMe = true,
            DeviceInfo = "Mobile",
            IpAddress = "192.168.1.1"
        };

        // Assert
        Assert.Equal("testuser", command.Username);
        Assert.Equal("password123", command.Password);
        Assert.True(command.RememberMe);
        Assert.Equal("Mobile", command.DeviceInfo);
        Assert.Equal("192.168.1.1", command.IpAddress);
    }

    [Fact]
    public void RegisterCommand_Should_HaveCorrectProperties()
    {
        // Arrange & Act
        var command = new RegisterCommand
        {
            Username = "newuser",
            Email = "new@example.com",
            Password = "password123",
            FirstName = "New",
            LastName = "User",
            DeviceInfo = "Web",
            IpAddress = "192.168.1.2"
        };

        // Assert
        Assert.Equal("newuser", command.Username);
        Assert.Equal("new@example.com", command.Email);
        Assert.Equal("password123", command.Password);
        Assert.Equal("New", command.FirstName);
        Assert.Equal("User", command.LastName);
        Assert.Equal("Web", command.DeviceInfo);
        Assert.Equal("192.168.1.2", command.IpAddress);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void LoginCommand_RememberMe_Should_WorkCorrectly(bool rememberMe)
    {
        // Arrange & Act
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = "password123",
            RememberMe = rememberMe
        };

        // Assert
        Assert.Equal(rememberMe, command.RememberMe);
    }

    [Fact]
    public void LogoutCommand_Should_DefaultToSingleDeviceLogout()
    {
        // Arrange & Act
        var command = new LogoutCommand();

        // Assert
        Assert.False(command.LogoutFromAllDevices);
    }
}
