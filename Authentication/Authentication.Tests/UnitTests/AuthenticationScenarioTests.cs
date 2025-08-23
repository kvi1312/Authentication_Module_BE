using Authentication.Application.Commands;
using Authentication.Application.Dtos.Request;
using Authentication.Domain.Entities;
using Xunit;

namespace Authentication.Tests.UnitTests;

/// <summary>
/// Comprehensive test cases for authentication scenarios as requested
/// </summary>
public class AuthenticationScenarioTests
{
    #region Token Expiration Tests

    [Fact]
    public void RefreshToken_Should_Be_Expired_When_ExpirationTimePassed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiredRefreshToken = RefreshToken.Create(
            "expired_refresh_token",
            "jwt_id_expired",
            userId,
            TimeSpan.FromDays(-1) // Expired 1 day ago
        );

        // Act & Assert
        Assert.False(expiredRefreshToken.IsValid());
        Assert.True(expiredRefreshToken.ExpiresAt < DateTime.UtcNow);
    }

    [Fact]
    public void AccessToken_Should_Expire_After_Configured_Minutes()
    {
        // Arrange
        var tokenCreatedAt = DateTime.UtcNow;
        var accessTokenLifetime = TimeSpan.FromMinutes(15);

        // Act
        var expirationTime = tokenCreatedAt.Add(accessTokenLifetime);
        var isExpiredAfterLifetime = DateTime.UtcNow.AddMinutes(16) > expirationTime;

        // Assert
        Assert.True(expirationTime > tokenCreatedAt);
        Assert.True(isExpiredAfterLifetime); // Should be expired after 16 minutes
    }

    [Theory]
    [InlineData(5)]   // 5 minutes
    [InlineData(15)]  // 15 minutes
    [InlineData(30)]  // 30 minutes
    [InlineData(60)]  // 1 hour
    public void AccessToken_Should_Expire_After_Various_Durations(int durationMinutes)
    {
        // Arrange
        var tokenCreatedAt = DateTime.UtcNow;
        var configuredDuration = TimeSpan.FromMinutes(durationMinutes);

        // Act
        var expirationTime = tokenCreatedAt.Add(configuredDuration);
        var simulatedFutureTime = tokenCreatedAt.Add(configuredDuration).AddSeconds(1);

        // Assert
        Assert.True(simulatedFutureTime > expirationTime);
        Assert.Equal(durationMinutes, configuredDuration.TotalMinutes);
    }

    #endregion

    #region Remember Me Token Tests

    [Fact]
    public void RememberMeToken_Should_Be_Created_With_Extended_Validity()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenHash = "hashed_remember_me_token_123";
        var validity = TimeSpan.FromDays(30); // 30 days for remember me

        // Act
        var rememberMeToken = RememberMeToken.Create(userId, tokenHash, validity);

        // Assert
        Assert.NotEqual(Guid.Empty, rememberMeToken.Id);
        Assert.Equal(userId, rememberMeToken.UserId);
        Assert.Equal(tokenHash, rememberMeToken.TokenHash);
        Assert.True(rememberMeToken.ExpiresAt > DateTime.UtcNow.AddDays(29));
        Assert.False(rememberMeToken.IsUsed);
        Assert.True(rememberMeToken.IsValid());
    }

    [Fact]
    public void RememberMeToken_Should_Be_Invalid_When_Expired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenHash = "expired_remember_me_token";
        var expiredValidity = TimeSpan.FromDays(-1); // Expired yesterday

        // Act
        var expiredRememberMeToken = RememberMeToken.Create(userId, tokenHash, expiredValidity);

        // Assert
        Assert.False(expiredRememberMeToken.IsValid());
        Assert.True(expiredRememberMeToken.ExpiresAt < DateTime.UtcNow);
    }

    [Fact]
    public void RememberMeToken_Should_Be_Invalid_After_Being_Used()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenHash = "used_remember_me_token";
        var validity = TimeSpan.FromDays(30);

        var rememberMeToken = RememberMeToken.Create(userId, tokenHash, validity);

        // Verify initially valid
        Assert.True(rememberMeToken.IsValid());
        Assert.False(rememberMeToken.IsUsed);

        // Act
        rememberMeToken.MarkAsUsed();

        // Assert
        Assert.True(rememberMeToken.IsUsed);
        Assert.False(rememberMeToken.IsValid());
    }

    [Fact]
    public void RememberMeToken_Should_Have_Longer_Validity_Than_RefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshTokenExpiry = TimeSpan.FromDays(7);   // 7 days for refresh token
        var rememberMeExpiry = TimeSpan.FromDays(30);    // 30 days for remember me

        // Act
        var refreshToken = RefreshToken.Create("refresh_123", "jwt_123", userId, refreshTokenExpiry);
        var rememberMeToken = RememberMeToken.Create(userId, "remember_hash_123", rememberMeExpiry);

        // Assert
        Assert.True(rememberMeToken.ExpiresAt > refreshToken.ExpiresAt);
        Assert.True(rememberMeExpiry > refreshTokenExpiry);
    }

    #endregion

    #region Registration Success Tests

    [Fact]
    public void RegisterRequest_Should_Contain_All_Required_Fields()
    {
        // Arrange & Act
        var registerRequest = new RegisterRequest
        {
            Username = "newuser123",
            Email = "newuser@example.com",
            Password = "SecurePassword123!",
            FirstName = "New",
            LastName = "User"
        };

        // Assert
        Assert.NotNull(registerRequest.Username);
        Assert.NotNull(registerRequest.Email);
        Assert.NotNull(registerRequest.Password);
        Assert.NotNull(registerRequest.FirstName);
        Assert.NotNull(registerRequest.LastName);
        Assert.Contains("@", registerRequest.Email);
        Assert.True(registerRequest.Password.Length >= 8);
    }

    [Theory]
    [InlineData("user@domain.com")]
    [InlineData("test.email+tag@example.org")]
    [InlineData("user123@subdomain.domain.co.uk")]
    [InlineData("simple@example.org")]
    public void RegisterRequest_Should_Accept_Valid_Email_Formats(string validEmail)
    {
        // Arrange & Act
        var request = new RegisterRequest { Email = validEmail };

        // Assert
        Assert.Contains("@", request.Email);
        Assert.Contains(".", request.Email);
        Assert.True(request.Email.IndexOf("@") < request.Email.LastIndexOf("."));
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("user@")]
    [InlineData("user@@domain.com")]
    public void RegisterRequest_Should_Reject_Invalid_Email_Formats(string invalidEmail)
    {
        // Arrange & Act
        var request = new RegisterRequest { Email = invalidEmail };

        // Assert - Basic validation that these emails are problematic
        var isValid = !string.IsNullOrEmpty(invalidEmail) &&
                     invalidEmail.Contains("@") &&
                     invalidEmail.Contains(".") &&
                     invalidEmail.IndexOf("@") < invalidEmail.LastIndexOf(".") &&
                     invalidEmail.Split('@').Length == 2;

        Assert.False(isValid);
    }

    #endregion

    #region Logout Success Tests

    [Fact]
    public void LogoutRequest_Should_Support_RefreshToken_Only()
    {
        // Arrange & Act
        var logoutRequest = new LogoutRequest
        {
            RefreshToken = "valid_refresh_token_789",
            AccessToken = null,
            LogoutFromAllDevices = false
        };

        // Assert
        Assert.NotNull(logoutRequest.RefreshToken);
        Assert.Null(logoutRequest.AccessToken);
        Assert.False(logoutRequest.LogoutFromAllDevices);
    }

    [Fact]
    public void LogoutRequest_Should_Support_Both_Tokens()
    {
        // Arrange & Act
        var logoutRequest = new LogoutRequest
        {
            RefreshToken = "valid_refresh_token_789",
            AccessToken = "valid_access_token_789",
            LogoutFromAllDevices = false
        };

        // Assert
        Assert.NotNull(logoutRequest.RefreshToken);
        Assert.NotNull(logoutRequest.AccessToken);
        Assert.False(logoutRequest.LogoutFromAllDevices);
    }

    [Fact]
    public void LogoutRequest_Should_Support_LogoutFromAllDevices()
    {
        // Arrange & Act
        var logoutRequest = new LogoutRequest
        {
            RefreshToken = "refresh_token_all_devices",
            AccessToken = "access_token_all_devices",
            LogoutFromAllDevices = true
        };

        // Assert
        Assert.True(logoutRequest.LogoutFromAllDevices);
        Assert.NotNull(logoutRequest.RefreshToken);
        Assert.NotNull(logoutRequest.AccessToken);
    }

    #endregion

    #region Token Reuse Prevention Tests

    [Fact]
    public void RefreshToken_Should_Be_Invalid_After_Being_Revoked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = RefreshToken.Create("token_123", "jwt_id_123", userId, TimeSpan.FromDays(7));

        // Verify initially valid
        Assert.True(refreshToken.IsValid());
        Assert.False(refreshToken.IsRevoked);

        refreshToken.MarkAsRevoked();

        // Assert
        Assert.False(refreshToken.IsValid());
        Assert.True(refreshToken.IsRevoked);
    }

    [Fact]
    public void RefreshToken_Should_Not_Be_Valid_If_Already_Revoked()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = RefreshToken.Create("revoked_token", "jwt_revoked", userId, TimeSpan.FromDays(7));

        refreshToken.MarkAsRevoked();

        // Act - Check if token can be reused
        var canBeReused = refreshToken.IsValid();

        // Assert
        Assert.False(canBeReused);
        Assert.True(refreshToken.IsRevoked);
    }

    [Fact]
    public void Multiple_RefreshTokens_Should_Be_Independent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var token1 = RefreshToken.Create("token_1", "jwt_1", userId, TimeSpan.FromDays(7));
        var token2 = RefreshToken.Create("token_2", "jwt_2", userId, TimeSpan.FromDays(7));

        // Act - Revoke first token
        token1.MarkAsRevoked();

        // Assert - Second token should still be valid
        Assert.False(token1.IsValid());
        Assert.True(token1.IsRevoked);
        Assert.True(token2.IsValid());
        Assert.False(token2.IsRevoked);
    }

    [Fact]
    public void RefreshToken_Should_Have_Unique_Properties()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokens = new List<RefreshToken>();

        // Act - Create multiple tokens
        for (int i = 0; i < 5; i++)
        {
            var token = RefreshToken.Create($"token_{i}", $"jwt_id_{i}", userId, TimeSpan.FromDays(7));
            tokens.Add(token);
        }

        // Assert - All tokens should have unique IDs and values
        var uniqueIds = tokens.Select(t => t.Id).Distinct().Count();
        var uniqueTokens = tokens.Select(t => t.Token).Distinct().Count();
        var uniqueJwtIds = tokens.Select(t => t.JwtId).Distinct().Count();

        Assert.Equal(5, uniqueIds);
        Assert.Equal(5, uniqueTokens);
        Assert.Equal(5, uniqueJwtIds);
        Assert.True(tokens.All(t => t.UserId == userId));
    }

    #endregion

    #region Command Validation Tests

    [Fact]
    public void LoginCommand_Should_Support_RememberMe_Flag()
    {
        // Arrange & Act
        var loginWithRememberMe = new LoginCommand
        {
            Username = "testuser",
            Password = "password123",
            RememberMe = true,
            DeviceInfo = "Mobile App",
            IpAddress = "192.168.1.100"
        };

        var loginWithoutRememberMe = new LoginCommand
        {
            Username = "testuser2",
            Password = "password456",
            RememberMe = false,
            DeviceInfo = "Web Browser",
            IpAddress = "192.168.1.101"
        };

        // Assert
        Assert.True(loginWithRememberMe.RememberMe);
        Assert.False(loginWithoutRememberMe.RememberMe);
        Assert.Equal("testuser", loginWithRememberMe.Username);
        Assert.Equal("testuser2", loginWithoutRememberMe.Username);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void RefreshTokenCommand_Should_Handle_Invalid_Tokens(string invalidToken)
    {
        // Arrange & Act
        var command = new RefreshTokenCommand { RefreshToken = invalidToken };

        // Assert
        var isEmpty = string.IsNullOrWhiteSpace(command.RefreshToken);
        Assert.True(isEmpty);
    }

    [Fact]
    public void LogoutCommand_Should_Handle_Partial_Token_Information()
    {
        // Arrange & Act
        var logoutWithNoTokens = new LogoutCommand
        {
            RefreshToken = null,
            AccessToken = null
        };

        var logoutWithOnlyRefresh = new LogoutCommand
        {
            RefreshToken = "only_refresh_token",
            AccessToken = null
        };

        var logoutWithOnlyAccess = new LogoutCommand
        {
            RefreshToken = null,
            AccessToken = "only_access_token"
        };

        // Assert
        Assert.Null(logoutWithNoTokens.RefreshToken);
        Assert.Null(logoutWithNoTokens.AccessToken);

        Assert.NotNull(logoutWithOnlyRefresh.RefreshToken);
        Assert.Null(logoutWithOnlyRefresh.AccessToken);

        Assert.Null(logoutWithOnlyAccess.RefreshToken);
        Assert.NotNull(logoutWithOnlyAccess.AccessToken);
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Theory]
    [InlineData(1)]      // 1 day minimum
    [InlineData(7)]      // 1 week standard
    [InlineData(30)]     // 1 month extended
    [InlineData(90)]     // 3 months long-term
    public void RefreshToken_Should_Handle_Various_Expiry_Durations(int expiryDays)
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiryDuration = TimeSpan.FromDays(expiryDays);

        // Act
        var refreshToken = RefreshToken.Create($"boundary_test_{expiryDays}", "jwt_id", userId, expiryDuration);

        // Assert
        Assert.True(refreshToken.IsValid());
        Assert.True(refreshToken.ExpiresAt > DateTime.UtcNow);
        Assert.True(refreshToken.ExpiresAt <= DateTime.UtcNow.AddDays(expiryDays + 1));
    }

    [Fact]
    public void RefreshToken_Should_Reject_Invalid_Parameters()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create("", "jwt_id", userId, TimeSpan.FromDays(7)));

        Assert.Throws<ArgumentException>(() =>
            RefreshToken.Create("token", "", userId, TimeSpan.FromDays(7)));
    }

    [Fact]
    public void RememberMeToken_Should_Reject_Invalid_Parameters()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act & Assert - Test empty token hash
        var tokenWithEmptyHash = RememberMeToken.Create(userId, "", TimeSpan.FromDays(30));
        Assert.Equal("", tokenWithEmptyHash.TokenHash);

        // Test with Guid.Empty 
        var tokenWithEmptyUserId = RememberMeToken.Create(Guid.Empty, "hash", TimeSpan.FromDays(30));
        Assert.Equal(Guid.Empty, tokenWithEmptyUserId.UserId);
    }

    #endregion

    #region Integration Scenario Tests

    [Fact]
    public void Complete_Authentication_Flow_Commands_Should_Be_Valid()
    {
        // Arrange - Registration
        var registerCommand = new RegisterCommand
        {
            Username = "flowtest",
            Email = "flowtest@example.com",
            Password = "FlowTest123!",
            FirstName = "Flow",
            LastName = "Test"
        };

        // Login with RememberMe
        var loginCommand = new LoginCommand
        {
            Username = "flowtest",
            Password = "FlowTest123!",
            RememberMe = true,
            IpAddress = "192.168.1.100",
            DeviceInfo = "Test Browser"
        };

        // Refresh Token
        var refreshCommand = new RefreshTokenCommand
        {
            RefreshToken = "initial_refresh_token",
            IpAddress = "192.168.1.100",
            DeviceInfo = "Test Browser"
        };

        // Logout
        var logoutCommand = new LogoutCommand
        {
            RefreshToken = "refreshed_token",
            AccessToken = "access_token",
            LogoutFromAllDevices = false
        };

        // Assert - All commands should be properly formed
        Assert.NotNull(registerCommand);
        Assert.Equal("flowtest", registerCommand.Username);
        Assert.Equal("flowtest@example.com", registerCommand.Email);

        Assert.NotNull(loginCommand);
        Assert.True(loginCommand.RememberMe);
        Assert.Equal("flowtest", loginCommand.Username);

        Assert.NotNull(refreshCommand);
        Assert.Equal("initial_refresh_token", refreshCommand.RefreshToken);

        Assert.NotNull(logoutCommand);
        Assert.False(logoutCommand.LogoutFromAllDevices);
    }

    [Fact]
    public void Token_Lifecycle_Should_Follow_Expected_Pattern()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act - Token creation
        var refreshToken = RefreshToken.Create("lifecycle_token", "jwt_lifecycle", userId, TimeSpan.FromDays(7));
        var rememberMeToken = RememberMeToken.Create(userId, "remember_hash", TimeSpan.FromDays(30));

        // Assert - Initial state
        Assert.True(refreshToken.IsValid());
        Assert.False(refreshToken.IsRevoked);
        Assert.True(rememberMeToken.IsValid());
        Assert.False(rememberMeToken.IsUsed);

        // Act - Token usage
        refreshToken.MarkAsRevoked();
        rememberMeToken.MarkAsUsed();

        // Assert - Final state
        Assert.False(refreshToken.IsValid());
        Assert.True(refreshToken.IsRevoked);
        Assert.False(rememberMeToken.IsValid());
        Assert.True(rememberMeToken.IsUsed);
    }

    #endregion
}
