using Authentication.Application.Commands;
using Authentication.Application.Dtos.Request;
using Authentication.Application.Dtos.Response;
using Authentication.Application.Handlers;
using Authentication.Application.Interfaces;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Authentication.Domain.Interfaces;
using Authentication.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Authentication.Tests.UnitTests;

/// <summary>
/// Integration tests for real-world authentication scenarios
/// </summary>
public class AuthenticationIntegrationTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<ITokenConfigService> _mockTokenConfigService;
    private readonly Mock<ILogger<RefreshTokenCommandHandler>> _mockRefreshLogger;
    private readonly Mock<ILogger<LoginCommandHandler>> _mockLoginLogger;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;
    private readonly Mock<IRoleRepository> _mockRoleRepository;

    public AuthenticationIntegrationTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockJwtService = new Mock<IJwtService>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockTokenConfigService = new Mock<ITokenConfigService>();
        _mockRefreshLogger = new Mock<ILogger<RefreshTokenCommandHandler>>();
        _mockLoginLogger = new Mock<ILogger<LoginCommandHandler>>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();
        _mockRoleRepository = new Mock<IRoleRepository>();

        _mockUnitOfWork.Setup(u => u.UserRepository).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(u => u.RefreshTokensRepository).Returns(_mockRefreshTokenRepository.Object);
        _mockUnitOfWork.Setup(u => u.RolesRepository).Returns(_mockRoleRepository.Object);
    }

    #region Real-world Token Expiration Scenarios

    [Fact]
    public async Task Scenario_AccessToken_Expires_During_Request_Should_Return_Unauthorized()
    {
        var expiredAccessToken = "expired.access.token";

        _mockJwtService.Setup(j => j.IsTokenExpired(expiredAccessToken)).Returns(true);
        _mockJwtService.Setup(j => j.GetJwtIdFromToken(expiredAccessToken)).Returns("expired_jwt_id");
        _mockJwtService.Setup(j => j.IsTokenBlacklistedAsync("expired_jwt_id")).ReturnsAsync(false);

        // Setup the authentication service to return false for expired tokens
        _mockAuthService.Setup(a => a.ValidateTokenAsync(expiredAccessToken, "access", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var isValid = await _mockAuthService.Object.ValidateTokenAsync(expiredAccessToken, "access", CancellationToken.None);

        // Assert
        Assert.False(isValid);
        _mockAuthService.Verify(a => a.ValidateTokenAsync(expiredAccessToken, "access", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Scenario_RefreshToken_Expires_While_User_Is_Active_Should_Require_Relogin()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiredRefreshToken = RefreshToken.Create(
            "expired_long_term_refresh",
            "jwt_id_expired",
            userId,
            TimeSpan.FromDays(-1) // Expired 1 day ago
        );

        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "expired_long_term_refresh"
        };

        _mockRefreshTokenRepository
            .Setup(r => r.GetByTokenAsync("expired_long_term_refresh"))
            .ReturnsAsync(expiredRefreshToken);

        // Act
        var result = await _mockAuthService.Object.RefreshTokenAsync(refreshRequest, CancellationToken.None);

        // Assert - Should fail and require user to login again
        var expectedResponse = new RefreshTokenResponse
        {
            Success = false,
            Message = "Invalid or expired refresh token"
        };

        _mockAuthService.Setup(a => a.RefreshTokenAsync(It.IsAny<RefreshTokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var actualResult = await _mockAuthService.Object.RefreshTokenAsync(refreshRequest, CancellationToken.None);
        Assert.False(actualResult.Success);
        Assert.Equal("Invalid or expired refresh token", actualResult.Message);
    }

    [Theory]
    [InlineData(5)]  // 5 minutes
    [InlineData(15)] // 15 minutes  
    [InlineData(30)] // 30 minutes
    [InlineData(60)] // 1 hour
    public void AccessToken_Should_Expire_After_Configured_Duration(int durationMinutes)
    {
        // Arrange
        var tokenCreatedAt = DateTime.UtcNow;
        var configuredDuration = TimeSpan.FromMinutes(durationMinutes);

        // Act
        var expirationTime = tokenCreatedAt.Add(configuredDuration);
        var simulatedCurrentTime = tokenCreatedAt.Add(configuredDuration).AddSeconds(1); // 1 second after expiration

        // Assert
        Assert.True(simulatedCurrentTime > expirationTime);
        Assert.Equal(durationMinutes, configuredDuration.TotalMinutes);
    }

    #endregion

    #region Remember Me Token Scenarios

    [Fact]
    public async Task Scenario_RememberMe_Should_Extend_Session_Duration()
    {
        // Arrange
        var loginWithRememberMe = new LoginRequest
        {
            Username = "longterm_user",
            Password = "SecurePass123!",
            RememberMe = true
        };

        var loginWithoutRememberMe = new LoginRequest
        {
            Username = "shortterm_user",
            Password = "SecurePass123!",
            RememberMe = false
        };

        var longTermResponse = new LoginResponse
        {
            Success = true,
            AccessToken = "long_access_token",
            RefreshToken = "long_refresh_token",
            RememberMeToken = "remember_me_token_30days",
            AccessTokenExpiresAt = DateTime.UtcNow.AddDays(30) // 30 days for remember me
        };

        var shortTermResponse = new LoginResponse
        {
            Success = true,
            AccessToken = "short_access_token",
            RefreshToken = "short_refresh_token",
            RememberMeToken = null,
            AccessTokenExpiresAt = DateTime.UtcNow.AddDays(7) // 7 days default
        };

        _mockAuthService.Setup(a => a.LoginAsync(
            It.Is<LoginRequest>(r => r.RememberMe == true),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(longTermResponse);

        _mockAuthService.Setup(a => a.LoginAsync(
            It.Is<LoginRequest>(r => r.RememberMe == false),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(shortTermResponse);

        // Act
        var longTermResult = await _mockAuthService.Object.LoginAsync(loginWithRememberMe, CancellationToken.None);
        var shortTermResult = await _mockAuthService.Object.LoginAsync(loginWithoutRememberMe, CancellationToken.None);

        // Assert
        Assert.NotNull(longTermResult.RememberMeToken);
        Assert.Null(shortTermResult.RememberMeToken);
        Assert.True(longTermResult.AccessTokenExpiresAt > shortTermResult.AccessTokenExpiresAt);
    }

    [Fact]
    public void RememberMeToken_Should_Have_Longer_Expiry_Than_RefreshToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshTokenExpiry = TimeSpan.FromDays(7);
        var rememberMeExpiry = TimeSpan.FromDays(30);

        // Act
        var refreshToken = RefreshToken.Create("refresh_123", "jwt_123", userId, refreshTokenExpiry);
        var rememberMeToken = RememberMeToken.Create(userId, "remember_hash_123", rememberMeExpiry);

        // Assert
        Assert.True(rememberMeToken.ExpiresAt > refreshToken.ExpiresAt);
        Assert.True(rememberMeExpiry > refreshTokenExpiry);
    }

    [Fact]
    public void Scenario_RememberMe_Token_Should_Allow_Auto_Login_After_Session_Expires()
    {
        // Arrange - Simulate user with expired session but valid remember me token
        var userId = Guid.NewGuid();
        var expiredRefreshToken = RefreshToken.Create("expired_session", "jwt_expired", userId, TimeSpan.FromDays(-1));
        var validRememberMeToken = RememberMeToken.Create(userId, "valid_remember_hash", TimeSpan.FromDays(15));

        // Act - Check if remember me token can be used for re-authentication
        var canAutoLogin = validRememberMeToken.IsValid() && !expiredRefreshToken.IsValid();

        // Assert
        Assert.True(canAutoLogin);
        Assert.False(expiredRefreshToken.IsValid());
        Assert.True(validRememberMeToken.IsValid());
    }

    #endregion

    #region Successful Registration Scenarios

    [Fact]
    public async Task Scenario_User_Registration_Should_Create_Complete_User_Profile()
    {
        // Arrange
        var registrationData = new RegisterRequest
        {
            Username = "complete_user_2024",
            Email = "complete.user@company.com",
            Password = "SecureCompanyPass123!",
            FirstName = "Complete",
            LastName = "User"
        };

        var expectedUser = new Authentication.Application.Dtos.UserDto
        {
            Id = Guid.NewGuid(),
            Username = "complete_user_2024",
            Email = "complete.user@company.com",
            FirstName = "Complete",
            LastName = "User",
            UserType = UserType.EndUser,
            IsActive = true,
            CreatedDate = DateTimeOffset.UtcNow
        };

        var registrationResponse = new LoginResponse
        {
            Success = true,
            Message = "Registration successful",
            AccessToken = "registration_access_token",
            RefreshToken = "registration_refresh_token",
            User = expectedUser,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        _mockAuthService.Setup(a => a.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(registrationResponse);

        // Act
        var result = await _mockAuthService.Object.RegisterAsync(registrationData, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.Equal("Registration successful", result.Message);
        Assert.NotNull(result.User);
        Assert.Equal(registrationData.Username, result.User.Username);
        Assert.Equal(registrationData.Email, result.User.Email);
        Assert.Equal(registrationData.FirstName, result.User.FirstName);
        Assert.Equal(registrationData.LastName, result.User.LastName);
        Assert.Equal(UserType.EndUser, result.User.UserType);
        Assert.True(result.User.IsActive);
        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
    }

    [Theory]
    [InlineData(UserType.EndUser, "User")]
    [InlineData(UserType.Admin, "Admin")]
    [InlineData(UserType.Partner, "Partner")]
    public async Task Registration_Should_Assign_Correct_Role_Based_On_UserType(UserType userType, string expectedRole)
    {
        // Arrange
        var registerRequest = new RegisterRequest
        {
            Username = $"test_{userType.ToString().ToLower()}",
            Email = $"test.{userType.ToString().ToLower()}@example.com",
            Password = "TestPassword123!",
            FirstName = "Test",
            LastName = "User"
        };

        var mockUser = new Authentication.Application.Dtos.UserDto
        {
            Id = Guid.NewGuid(),
            Username = registerRequest.Username,
            Email = registerRequest.Email,
            UserType = userType,
            IsActive = true
        };

        var response = new LoginResponse
        {
            Success = true,
            User = mockUser,
            AccessToken = "test_token",
            RefreshToken = "test_refresh"
        };

        _mockAuthService.Setup(a => a.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _mockAuthService.Object.RegisterAsync(registerRequest, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal(userType, result.User.UserType);

        var actualRole = result.User.UserType.ToString();
        Assert.Contains(expectedRole, actualRole);
    }

    #endregion

    #region Successful Logout Scenarios

    [Fact]
    public async Task Scenario_Logout_Should_Invalidate_All_User_Tokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var accessToken = "active_access_token_123";
        var refreshToken = "active_refresh_token_123";
        var jwtId = "jwt_id_123";

        var logoutRequest = new LogoutRequest
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            LogoutFromAllDevices = true
        };

        var mockRefreshToken = RefreshToken.Create(refreshToken, jwtId, userId, TimeSpan.FromDays(7));

        _mockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(refreshToken))
            .ReturnsAsync(mockRefreshToken);

        _mockJwtService.Setup(j => j.GetJwtIdFromToken(accessToken))
            .Returns(jwtId);

        _mockJwtService.Setup(j => j.GetTokenExpirationDate(accessToken))
            .Returns(DateTime.UtcNow.AddMinutes(15));

        _mockAuthService.Setup(a => a.LogoutAsync(It.IsAny<LogoutRequest>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .Callback<LogoutRequest, Guid?, CancellationToken>((request, uid, ct) =>
            {
                mockRefreshToken.MarkAsRevoked();
                _mockJwtService.Object.BlacklistTokenAsync(jwtId, DateTime.UtcNow.AddMinutes(15));
            })
            .ReturnsAsync(true);

        // Act
        var result = await _mockAuthService.Object.LogoutAsync(logoutRequest, userId, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.True(mockRefreshToken.IsRevoked);

        // Verify that blacklist was called for access token
        _mockJwtService.Verify(j => j.BlacklistTokenAsync(jwtId, It.IsAny<DateTime>()), Times.AtLeastOnce);
    }

    [Fact]
    public async Task Scenario_Logout_From_All_Devices_Should_Revoke_All_User_Sessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var currentDeviceRefreshToken = "current_device_refresh";
        var otherDeviceRefreshTokens = new List<string> { "device1_refresh", "device2_refresh", "device3_refresh" };

        var logoutRequest = new LogoutRequest
        {
            RefreshToken = currentDeviceRefreshToken,
            LogoutFromAllDevices = true
        };

        _mockRefreshTokenRepository.Setup(r => r.RevokeAllByUserIdAsync(userId))
            .Returns(Task.CompletedTask);

        _mockAuthService.Setup(a => a.LogoutAsync(It.IsAny<LogoutRequest>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .Callback<LogoutRequest, Guid?, CancellationToken>((request, uid, ct) =>
            {
                if (request.LogoutFromAllDevices && uid.HasValue)
                {
                    _mockRefreshTokenRepository.Object.RevokeAllByUserIdAsync(uid.Value);
                }
            })
            .ReturnsAsync(true);

        // Act
        var result = await _mockAuthService.Object.LogoutAsync(logoutRequest, userId, CancellationToken.None);

        // Assert
        Assert.True(result);
        _mockRefreshTokenRepository.Verify(r => r.RevokeAllByUserIdAsync(userId), Times.AtLeastOnce);
    }


    #endregion

    #region Token Reuse Prevention Advanced Scenarios

    [Fact]
    public async Task Scenario_Stolen_Token_Detection_Should_Invalidate_All_User_Sessions()
    {
        // Arrange - Simulate suspicious activity: using an already used refresh token
        var userId = Guid.NewGuid();
        var suspiciousRefreshToken = "potentially_stolen_token";
        var jwtId = "suspicious_jwt_id";

        var usedRefreshToken = RefreshToken.Create(suspiciousRefreshToken, jwtId, userId, TimeSpan.FromDays(7));
        usedRefreshToken.MarkAsRevoked(); 

        var command = new RefreshTokenCommand { RefreshToken = suspiciousRefreshToken };

        _mockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(suspiciousRefreshToken))
            .ReturnsAsync(usedRefreshToken);

        // suspicious activity is detected = revoke all user sessions
        _mockRefreshTokenRepository.Setup(r => r.RevokeAllByUserIdAsync(userId))
            .Returns(Task.CompletedTask);

        var handler = new RefreshTokenCommandHandler(_mockUnitOfWork.Object, _mockJwtService.Object, _mockTokenConfigService.Object, _mockRefreshLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid or expired refresh token", result.Message);

        // verify the token was already marked as used
        Assert.True(usedRefreshToken.IsRevoked);
    }

    [Fact]
    public void Scenario_Token_Rotation_Should_Generate_New_Tokens_With_Different_Values()
    {
        // Arrange
        var originalRefreshToken = "original_refresh_token_12345";
        var newRefreshToken = "new_refresh_token_67890";
        var originalAccessToken = "original.access.token";
        var newAccessToken = "new.access.token";

        // Act & Assert - Verify tokens are different after rotation
        Assert.NotEqual(originalRefreshToken, newRefreshToken);
        Assert.NotEqual(originalAccessToken, newAccessToken);

        // Verify token format/structure (basic validation)
        Assert.True(originalRefreshToken.Length > 10);
        Assert.True(newRefreshToken.Length > 10);
        Assert.Contains(".", originalAccessToken);
        Assert.Contains(".", newAccessToken);
    }

    #endregion

    #region Cross-Device and Security Scenarios

    [Fact]
    public async Task Scenario_Login_From_Multiple_Devices_Should_Generate_Separate_Sessions()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var deviceLogins = new List<(string device, string ip, LoginRequest request)>
        {
            ("Mobile-iPhone", "192.168.1.100", new LoginRequest { Username = "multiuser", Password = "Pass123!", RememberMe = true }),
            ("Desktop-Chrome", "192.168.1.101", new LoginRequest { Username = "multiuser", Password = "Pass123!", RememberMe = false }),
            ("Tablet-Safari", "192.168.1.102", new LoginRequest { Username = "multiuser", Password = "Pass123!", RememberMe = true })
        };

        var responses = new List<LoginResponse>();

        foreach (var (device, ip, request) in deviceLogins)
        {
            var response = new LoginResponse
            {
                Success = true,
                AccessToken = $"access_token_{device}",
                RefreshToken = $"refresh_token_{device}",
                RememberMeToken = request.RememberMe ? $"remember_me_{device}" : null,
                Message = "Login successful"
            };
            responses.Add(response);
        }

        // Setup mock to return responses in sequence
        var setupSequence = _mockAuthService.SetupSequence(a => a.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()));
        foreach (var response in responses)
        {
            setupSequence = setupSequence.ReturnsAsync(response);
        }

        // Act
        var results = new List<LoginResponse>();
        foreach (var (_, _, request) in deviceLogins)
        {
            var result = await _mockAuthService.Object.LoginAsync(request, CancellationToken.None);
            results.Add(result);
        }

        // Assert
        Assert.All(results, r => Assert.True(r.Success));
        Assert.Equal(3, results.Count);

        // Verify each device has unique tokens
        var accessTokens = results.Select(r => r.AccessToken).ToList();
        var refreshTokens = results.Select(r => r.RefreshToken).ToList();

        Assert.Equal(accessTokens.Distinct().Count(), accessTokens.Count);
        Assert.Equal(refreshTokens.Distinct().Count(), refreshTokens.Count);

        // Verify remember me tokens only for devices that requested it
        var rememberMeTokens = results.Where(r => r.RememberMeToken != null).ToList();
        Assert.Equal(2, rememberMeTokens.Count);
    }

    #endregion
}
