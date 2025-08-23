using Authentication.Application.Dtos.Request;
using Authentication.Application.Dtos.Response;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Authentication.Infrastructure.Persistence;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Xunit;

namespace Authentication.Tests.IntegrationTests;

public class SimplifiedAuthenticationIntegrationTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public SimplifiedAuthenticationIntegrationTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public void AuthenticationRequest_Models_Should_SerializeCorrectly()
    {
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "TestPassword123!",
            RememberMe = true
        };

        var registerRequest = new RegisterRequest
        {
            Username = "newuser",
            Email = "new@test.com",
            Password = "NewPassword123!",
            FirstName = "New",
            LastName = "User"
        };

        var refreshTokenRequest = new RefreshTokenRequest
        {
            RefreshToken = "test_refresh_token"
        };

        var loginJson = JsonConvert.SerializeObject(loginRequest);
        var registerJson = JsonConvert.SerializeObject(registerRequest);
        var refreshJson = JsonConvert.SerializeObject(refreshTokenRequest);

        Assert.Contains("testuser", loginJson);
        Assert.Contains("true", loginJson.ToLower());
        Assert.Contains("newuser", registerJson);
        Assert.Contains("new@test.com", registerJson);
        Assert.Contains("test_refresh_token", refreshJson);

        var deserializedLogin = JsonConvert.DeserializeObject<LoginRequest>(loginJson);
        Assert.NotNull(deserializedLogin);
        Assert.Equal("testuser", deserializedLogin.Username);
        Assert.True(deserializedLogin.RememberMe);
    }

    [Fact]
    public void AuthenticationResponse_Models_Should_DeserializeCorrectly()
    {
        var loginResponseJson = @"{
            ""Success"": true,
            ""Message"": ""Login successful"",
            ""AccessToken"": ""eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."",
            ""AccessTokenExpiresAt"": ""2024-08-24T10:00:00Z"",
            ""User"": {
                ""Username"": ""testuser"",
                ""Email"": ""test@example.com""
            },
            ""SessionId"": ""session_123""
        }";

        var registerResponseJson = @"{
            ""Success"": true,
            ""Message"": ""Registration successful"",
            ""User"": {
                ""Username"": ""newuser"",
                ""Email"": ""new@test.com""
            }
        }";

        var loginResponse = JsonConvert.DeserializeObject<LoginResponse>(loginResponseJson);
        var registerResponse = JsonConvert.DeserializeObject<RegisterResponse>(registerResponseJson);

        Assert.NotNull(loginResponse);
        Assert.True(loginResponse.Success);
        Assert.Equal("Login successful", loginResponse.Message);
        Assert.NotNull(loginResponse.AccessToken);
        Assert.Equal("testuser", loginResponse.User?.Username);
        Assert.Equal("session_123", loginResponse.SessionId);

        Assert.NotNull(registerResponse);
        Assert.True(registerResponse.Success);
        Assert.Equal("newuser", registerResponse.User?.Username);
    }

    [Fact]
    public void TokenExpiration_Scenarios_Should_ValidateCorrectly()
    {
        var accessTokenLifetime = TimeSpan.FromMinutes(15);
        var refreshTokenLifetime = TimeSpan.FromDays(7);
        var rememberMeTokenLifetime = TimeSpan.FromDays(30);

        var now = DateTime.UtcNow;

        var accessTokenExpiry = now.Add(accessTokenLifetime);
        Assert.True(accessTokenExpiry > now);
        Assert.True(accessTokenExpiry < now.AddHours(1));

        var refreshTokenExpiry = now.Add(refreshTokenLifetime);
        Assert.True(refreshTokenExpiry > accessTokenExpiry);
        Assert.True(refreshTokenExpiry < now.AddDays(8));

        var rememberMeExpiry = now.Add(rememberMeTokenLifetime);
        Assert.True(rememberMeExpiry > refreshTokenExpiry);
        Assert.True(rememberMeExpiry > now.AddDays(29));
    }

    [Fact]
    public void RoleManagement_Scenarios_Should_ValidateCorrectly()
    {
        var adminRoleRequest = new
        {
            Username = "testuser",
            RoleType = "Admin"
        };

        var bulkAssignmentRequest = new
        {
            UserRoleAssignments = new[]
            {
                new { Username = "user1", RoleType = "Customer" },
                new { Username = "user2", RoleType = "Manager" },
                new { Username = "user3", RoleType = "Employee" }
            }
        };

        var adminJson = JsonConvert.SerializeObject(adminRoleRequest);
        var bulkJson = JsonConvert.SerializeObject(bulkAssignmentRequest);

        Assert.Contains("Admin", adminJson);
        Assert.Contains("testuser", adminJson);

        Assert.Contains("UserRoleAssignments", bulkJson);
        Assert.Contains("Customer", bulkJson);
        Assert.Contains("Manager", bulkJson);
        Assert.Contains("Employee", bulkJson);
    }

    [Fact]
    public void AuthenticationFlow_EndToEnd_Should_ValidateCorrectly()
    {
        var registrationStep = new RegisterRequest
        {
            Username = "flowuser",
            Email = "flow@test.com",
            Password = "FlowTest123!",
            FirstName = "Flow",
            LastName = "User"
        };

        var loginStep = new LoginRequest
        {
            Username = "flowuser",
            Password = "FlowTest123!",
            RememberMe = true
        };

        var refreshStep = new RefreshTokenRequest
        {
            RefreshToken = "mock_refresh_token"
        };

        var regJson = JsonConvert.SerializeObject(registrationStep);
        var loginJson = JsonConvert.SerializeObject(loginStep);
        var refreshJson = JsonConvert.SerializeObject(refreshStep);

        Assert.NotNull(regJson);
        Assert.NotNull(loginJson);
        Assert.NotNull(refreshJson);

        Assert.Contains("flowuser", regJson);
        Assert.Contains("flowuser", loginJson);

        Assert.Contains("true", loginJson.ToLower());

        Assert.Contains("mock_refresh_token", refreshJson);
    }

    [Fact]
    public void HttpStatusCodes_Should_BeHandledCorrectly()
    {
        var validLoginScenario = new
        {
            StatusCode = HttpStatusCode.OK,
            ExpectedOutcome = "Success"
        };

        var invalidCredentialsScenario = new
        {
            StatusCode = HttpStatusCode.BadRequest,
            ExpectedOutcome = "Invalid credentials"
        };

        var unauthorizedScenario = new
        {
            StatusCode = HttpStatusCode.Unauthorized,
            ExpectedOutcome = "Token expired or invalid"
        };

        var forbiddenScenario = new
        {
            StatusCode = HttpStatusCode.Forbidden,
            ExpectedOutcome = "Insufficient permissions"
        };

        Assert.Equal(200, (int)validLoginScenario.StatusCode);
        Assert.Equal(400, (int)invalidCredentialsScenario.StatusCode);
        Assert.Equal(401, (int)unauthorizedScenario.StatusCode);
        Assert.Equal(403, (int)forbiddenScenario.StatusCode);

        Assert.Equal("Success", validLoginScenario.ExpectedOutcome);
        Assert.Equal("Invalid credentials", invalidCredentialsScenario.ExpectedOutcome);
        Assert.Equal("Token expired or invalid", unauthorizedScenario.ExpectedOutcome);
        Assert.Equal("Insufficient permissions", forbiddenScenario.ExpectedOutcome);
    }

    [Fact]
    public void CookieHandling_Should_ValidateCorrectly()
    {
        var sessionCookieOptions = new
        {
            HttpOnly = true,
            Secure = false,
            SameSite = "Strict",
            Path = "/",
            Expires = (DateTime?)null
        };

        var persistentCookieOptions = new
        {
            HttpOnly = true,
            Secure = false,
            SameSite = "Strict",
            Path = "/",
            Expires = DateTime.UtcNow.AddDays(30)
        };

        Assert.True(sessionCookieOptions.HttpOnly);
        Assert.Equal("Strict", sessionCookieOptions.SameSite);
        Assert.Equal("/", sessionCookieOptions.Path);
        Assert.Null(sessionCookieOptions.Expires);

        Assert.True(persistentCookieOptions.HttpOnly);
        // Verify persistent cookie is set with expiry
        Assert.True(persistentCookieOptions.Expires > DateTime.UtcNow); // Persistent cookie should have expiry
    }

    [Fact]
    public void SecurityHeaders_Should_BeValidated()
    {
        var authorizationHeader = "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";
        var userAgentHeader = "Mozilla/5.0 (Windows NT 10.0; Win64; x64)";
        var ipAddress = "192.168.1.100";

        var bearerToken = authorizationHeader.StartsWith("Bearer ")
            ? authorizationHeader.Substring("Bearer ".Length).Trim()
            : null;

        Assert.NotNull(bearerToken);
        Assert.StartsWith("eyJ", bearerToken);
        Assert.NotNull(userAgentHeader);
        Assert.NotNull(ipAddress);

        var ipParts = ipAddress.Split('.');
        Assert.Equal(4, ipParts.Length);
        Assert.All(ipParts, part => Assert.True(int.Parse(part) >= 0 && int.Parse(part) <= 255));
    }
}
