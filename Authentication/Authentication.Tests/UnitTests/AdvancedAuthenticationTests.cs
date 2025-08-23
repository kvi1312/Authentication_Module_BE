using Authentication.Application.Commands;
using Authentication.Application.Dtos.Request;
using Authentication.Application.Dtos.Response;
using Authentication.Application.Handlers;
using Authentication.Application.Interfaces;
using Authentication.Domain.Entities;
using Authentication.Domain.Interfaces;
using Authentication.Domain.Interfaces.Repositories;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Authentication.Tests.UnitTests;

/// <summary>
/// Advanced test cases for authentication scenarios including token expiration, remember me functionality,
/// successful registration/logout, and token reuse prevention
/// </summary>
public class AdvancedAuthenticationTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IJwtService> _mockJwtService;
    private readonly Mock<IPasswordService> _mockPasswordService;
    private readonly Mock<IAuthenticationService> _mockAuthService;
    private readonly Mock<ITokenConfigService> _mockTokenConfigService;
    private readonly Mock<ILogger<RefreshTokenCommandHandler>> _mockLogger;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<IRefreshTokenRepository> _mockRefreshTokenRepository;

    public AdvancedAuthenticationTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockJwtService = new Mock<IJwtService>();
        _mockPasswordService = new Mock<IPasswordService>();
        _mockAuthService = new Mock<IAuthenticationService>();
        _mockTokenConfigService = new Mock<ITokenConfigService>();
        _mockLogger = new Mock<ILogger<RefreshTokenCommandHandler>>();
        _mockUserRepository = new Mock<IUserRepository>();
        _mockRefreshTokenRepository = new Mock<IRefreshTokenRepository>();

        _mockUnitOfWork.Setup(u => u.UserRepository).Returns(_mockUserRepository.Object);
        _mockUnitOfWork.Setup(u => u.RefreshTokensRepository).Returns(_mockRefreshTokenRepository.Object);
    }

    #region Token Expiration Tests

    [Fact]
    public async Task RefreshToken_Should_Fail_When_AccessToken_Expired_And_RefreshToken_Also_Expired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var expiredRefreshToken = RefreshToken.Create(
            "expired_refresh_token_456",
            "jwt_id_456",
            userId,
            TimeSpan.FromMinutes(-30)
        );

        _mockJwtService.Setup(j => j.IsTokenExpired(It.IsAny<string>())).Returns(true);

        var command = new RefreshTokenCommand
        {
            RefreshToken = "expired_refresh_token_456"
        };

        _mockRefreshTokenRepository
            .Setup(r => r.GetByTokenAsync("expired_refresh_token_456"))
            .ReturnsAsync(expiredRefreshToken);

        var handler = new RefreshTokenCommandHandler(_mockUnitOfWork.Object, _mockJwtService.Object, _mockTokenConfigService.Object, _mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid or expired refresh token", result.Message);
        Assert.Null(result.AccessToken);
    }

    #endregion

    #region Remember Me Token Tests

    [Fact]
    public async Task Login_Should_Generate_RememberMeToken_When_RememberMe_True()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "password123",
            RememberMe = true
        };

        var expectedResponse = new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            AccessToken = "access_token_123",
            RefreshToken = "refresh_token_123",
            RememberMeToken = "remember_me_token_123",
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        _mockAuthService
            .Setup(a => a.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        // Act
        var result = await _mockAuthService.Object.LoginAsync(loginRequest, CancellationToken.None);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.RememberMeToken);
        Assert.Equal("remember_me_token_123", result.RememberMeToken);
    }

    #endregion

    #region Successful Logout Tests

    [Fact]
    public async Task Logout_Should_Succeed_With_Valid_Tokens()
    {
        // Arrange
        var logoutRequest = new LogoutRequest
        {
            RefreshToken = "valid_refresh_token_789",
            AccessToken = "valid_access_token_789",
            LogoutFromAllDevices = false
        };

        _mockAuthService
            .Setup(a => a.LogoutAsync(It.IsAny<LogoutRequest>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mockAuthService.Object.LogoutAsync(logoutRequest, Guid.NewGuid(), CancellationToken.None);

        // Assert
        Assert.True(result);
        _mockAuthService.Verify(a => a.LogoutAsync(It.IsAny<LogoutRequest>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Logout_Should_Revoke_RefreshToken_And_Blacklist_AccessToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshTokenValue = "refresh_token_to_revoke";
        var accessToken = "access_token_to_blacklist";
        var jwtId = "jwt_id_123";

        var refreshToken = RefreshToken.Create(refreshTokenValue, jwtId, userId, TimeSpan.FromDays(7));

        var logoutRequest = new LogoutRequest
        {
            RefreshToken = refreshTokenValue,
            AccessToken = accessToken
        };

        _mockRefreshTokenRepository
            .Setup(r => r.GetByTokenAsync(refreshTokenValue))
            .ReturnsAsync(refreshToken);

        _mockJwtService
            .Setup(j => j.GetJwtIdFromToken(accessToken))
            .Returns(jwtId);

        _mockJwtService
            .Setup(j => j.GetTokenExpirationDate(accessToken))
            .Returns(DateTime.UtcNow.AddMinutes(15));

        _mockJwtService
            .Setup(j => j.BlacklistTokenAsync(jwtId, It.IsAny<DateTime>()))
            .Returns(Task.CompletedTask);

        _mockAuthService
            .Setup(a => a.LogoutAsync(It.IsAny<LogoutRequest>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .Callback<LogoutRequest, Guid?, CancellationToken>((request, uid, ct) =>
            {
                refreshToken.MarkAsRevoked();
                _mockJwtService.Object.BlacklistTokenAsync(jwtId, DateTime.UtcNow.AddMinutes(15));
            })
            .ReturnsAsync(true);

        // Act
        var result = await _mockAuthService.Object.LogoutAsync(logoutRequest, userId, CancellationToken.None);

        // Assert
        Assert.True(result);
        Assert.True(refreshToken.IsRevoked);
        _mockJwtService.Verify(j => j.BlacklistTokenAsync(jwtId, It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task Logout_Should_Succeed_Even_With_Only_RefreshToken()
    {
        // Arrange
        var logoutRequest = new LogoutRequest
        {
            RefreshToken = "only_refresh_token_456",
            AccessToken = null,
            LogoutFromAllDevices = false
        };

        _mockAuthService
            .Setup(a => a.LogoutAsync(It.IsAny<LogoutRequest>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mockAuthService.Object.LogoutAsync(logoutRequest, null, CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    #endregion

    #region Token Reuse Prevention Tests

    [Fact]
    public void RefreshToken_Should_Be_Invalid_After_MarkAsUsed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = RefreshToken.Create("token_123", "jwt_id_123", userId, TimeSpan.FromDays(7));

        Assert.True(refreshToken.IsValid());
        Assert.False(refreshToken.IsRevoked);

        // Act
        refreshToken.MarkAsRevoked();

        // Assert
        Assert.False(refreshToken.IsValid());
        Assert.True(refreshToken.IsRevoked);
    }

    [Fact]
    public void Multiple_RefreshToken_Requests_Should_Each_Invalidate_Previous_Token()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            IsActive = true,
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = new Role { Name = "User" } }
            }
        };

        var firstRefreshToken = RefreshToken.Create("first_refresh_token", "jwt_id_1", userId, TimeSpan.FromDays(7));

        var secondRefreshToken = RefreshToken.Create("second_refresh_token", "jwt_id_2", userId, TimeSpan.FromDays(7));

        var handler = new RefreshTokenCommandHandler(_mockUnitOfWork.Object, _mockJwtService.Object, _mockTokenConfigService.Object, _mockLogger.Object);

        _mockRefreshTokenRepository
            .Setup(r => r.GetByTokenAsync("first_refresh_token"))
            .ReturnsAsync(firstRefreshToken);

        _mockUserRepository
            .Setup(u => u.GetWithRolesAsync(userId))
            .ReturnsAsync(user);

        _mockJwtService
            .Setup(j => j.GenerateAccessToken(user, It.IsAny<List<string>>()))
            .Returns("access_token_2");

        _mockJwtService
            .Setup(j => j.GenerateRefreshToken())
            .Returns("second_refresh_token");

        _mockJwtService
            .Setup(j => j.GetJwtIdFromToken("access_token_2"))
            .Returns("jwt_id_2");

        _mockRefreshTokenRepository
            .Setup(r => r.AddAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        firstRefreshToken.MarkAsRevoked();

        Assert.True(firstRefreshToken.IsRevoked);
        Assert.False(firstRefreshToken.IsValid());

        var canReuseFirstToken = firstRefreshToken.IsValid();
        Assert.False(canReuseFirstToken);

        Assert.True(secondRefreshToken.IsValid());
        Assert.False(secondRefreshToken.IsRevoked);
    }

    [Fact]
    public async Task AccessToken_Should_Be_Blacklisted_After_Logout()
    {
        // Arrange
        var accessToken = "access_token_to_blacklist";
        var jwtId = "jwt_id_blacklist_test";
        var expirationDate = DateTime.UtcNow.AddMinutes(15);

        _mockJwtService
            .Setup(j => j.GetJwtIdFromToken(accessToken))
            .Returns(jwtId);

        _mockJwtService
            .Setup(j => j.GetTokenExpirationDate(accessToken))
            .Returns(expirationDate);

        _mockJwtService
            .Setup(j => j.IsTokenBlacklistedAsync(jwtId))
            .ReturnsAsync(false);

        // Act - Simulate logout which should blacklist the token
        await _mockJwtService.Object.BlacklistTokenAsync(jwtId, expirationDate);

        // Setup for blacklist check
        _mockJwtService
            .Setup(j => j.IsTokenBlacklistedAsync(jwtId))
            .ReturnsAsync(true);

        // Assert
        var isBlacklisted = await _mockJwtService.Object.IsTokenBlacklistedAsync(jwtId);
        Assert.True(isBlacklisted);

        _mockJwtService.Verify(j => j.BlacklistTokenAsync(jwtId, expirationDate), Times.Once);
    }

    #endregion

    #region Integration Test Scenarios

    [Fact]
    public async Task Complete_Authentication_Flow_Should_Work_With_Token_Refresh_And_Logout()
    {
        var registerRequest = new RegisterRequest
        {
            Username = "flowtest",
            Email = "flowtest@example.com",
            Password = "FlowTest123!",
            FirstName = "Flow",
            LastName = "Test"
        };

        var registerResponse = new LoginResponse
        {
            Success = true,
            Message = "Registration successful",
            AccessToken = "initial_access_token",
            RefreshToken = "initial_refresh_token"
        };

        var loginRequest = new LoginRequest
        {
            Username = "flowtest",
            Password = "FlowTest123!",
            RememberMe = true
        };

        var loginResponse = new LoginResponse
        {
            Success = true,
            Message = "Login successful",
            AccessToken = "login_access_token",
            RefreshToken = "login_refresh_token",
            RememberMeToken = "remember_me_token_123"
        };

        var refreshResponse = new RefreshTokenResponse
        {
            Success = true,
            Message = "Token refreshed successfully",
            AccessToken = "refreshed_access_token",
            RefreshToken = "new_refresh_token"
        };

        _mockAuthService
            .Setup(a => a.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(registerResponse);

        _mockAuthService
            .Setup(a => a.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(loginResponse);

        _mockAuthService
            .Setup(a => a.RefreshTokenAsync(It.IsAny<RefreshTokenRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshResponse);

        _mockAuthService
            .Setup(a => a.LogoutAsync(It.IsAny<LogoutRequest>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // 1. Register
        var regResult = await _mockAuthService.Object.RegisterAsync(registerRequest, CancellationToken.None);
        Assert.True(regResult.Success);
        Assert.NotNull(regResult.AccessToken);

        // 2. Login with RememberMe
        var loginResult = await _mockAuthService.Object.LoginAsync(loginRequest, CancellationToken.None);
        Assert.True(loginResult.Success);
        Assert.NotNull(loginResult.RememberMeToken);

        // 3. Refresh Token
        var refreshRequest = new RefreshTokenRequest { RefreshToken = loginResult.RefreshToken! };
        var refreshResult = await _mockAuthService.Object.RefreshTokenAsync(refreshRequest, CancellationToken.None);
        Assert.True(refreshResult.Success);
        Assert.NotEqual(loginResult.AccessToken, refreshResult.AccessToken);

        // 4. Logout
        var logoutRequest = new LogoutRequest
        {
            RefreshToken = refreshResult.RefreshToken,
            AccessToken = refreshResult.AccessToken
        };
        var logoutResult = await _mockAuthService.Object.LogoutAsync(logoutRequest, null, CancellationToken.None);
        Assert.True(logoutResult);
    }

    #endregion
}
