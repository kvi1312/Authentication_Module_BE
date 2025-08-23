using Authentication.Application.Dtos.Request;
using Authentication.Application.Dtos.Response;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;
using System.Text;
using Xunit;

namespace Authentication.Tests.IntegrationTests;

public class TokenExpirationIntegrationTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public TokenExpirationIntegrationTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task AccessToken_Should_BeValidatedByAuthenticatedEndpoints()
    {
        var username = GenerateUniqueUsername("tokenuser");
        var email = GenerateUniqueEmail("token");
        await RegisterTestUser(username, email, "TokenTest123!");
        var loginResponse = await LoginTestUser(username, "TokenTest123!", false);

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonConvert.DeserializeObject<LoginResponse>(loginContent);
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.AccessToken);

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        var response = await _client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task InvalidAccessToken_Should_Return401()
    {
        var invalidToken = "invalid.access.token";

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", invalidToken);

        var response = await _client.PostAsync("/api/auth/logout", null);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_Should_FailWithInvalidToken()
    {
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "invalid_refresh_token"
        };

        var json = JsonConvert.SerializeObject(refreshRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/refresh-token", content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RefreshToken_Should_FailAfterLogout()
    {
        var username = GenerateUniqueUsername("logoutexpiryuser");
        var email = GenerateUniqueEmail("logoutexpiry");
        await RegisterTestUser(username, email, "LogoutExpiryTest123!");
        var loginResponse = await LoginTestUser(username, "LogoutExpiryTest123!", false);

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonConvert.DeserializeObject<LoginResponse>(loginContent);
        Assert.NotNull(loginResult);
        Assert.NotNull(loginResult.AccessToken);

        var refreshTokenCookie = loginResponse.Headers.GetValues("Set-Cookie")
            .FirstOrDefault(c => c.Contains("refreshToken"));
        Assert.NotNull(refreshTokenCookie);
        var refreshToken = ExtractCookieValue(refreshTokenCookie, "refreshToken");

        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult.AccessToken);

        var logoutResponse = await _client.PostAsync("/api/auth/logout", null);
        logoutResponse.EnsureSuccessStatusCode();

        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = refreshToken
        };

        var json = JsonConvert.SerializeObject(refreshRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/refresh-token", content);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task RememberMeToken_Should_AllowLongerSessionDuration()
    {
        var username = GenerateUniqueUsername("rememberexpiryuser");
        var email = GenerateUniqueEmail("rememberexpiry");
        await RegisterTestUser(username, email, "RememberExpiryTest123!");

        var configSection = _factory.Services.GetRequiredService<IConfiguration>().GetSection("JwtSettings");
        var rememberMeExpiry = configSection["RememberMeTokenExpiryDays"];
        Console.WriteLine($"Configuration RememberMeTokenExpiryDays: {rememberMeExpiry}");

        var loginResponse = await LoginTestUser(username, "RememberExpiryTest123!", true);

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonConvert.DeserializeObject<LoginResponse>(loginContent);
        Assert.NotNull(loginResult);
        Assert.True(loginResult.Success);
        Assert.NotNull(loginResult.RememberMeTokenExpiresAt);

        var now = DateTime.UtcNow;
        var sevenDaysFromNow = now.AddDays(7);
        var rememberMeExpiry2 = loginResult.RememberMeTokenExpiresAt.Value;

        Console.WriteLine($"RememberMeToken expires at {rememberMeExpiry2}, but should be > {sevenDaysFromNow} (now: {now})");

        Assert.True(rememberMeExpiry2 > sevenDaysFromNow,
            $"RememberMeToken expires at {rememberMeExpiry2}, but should be > {sevenDaysFromNow} (now: {now})");

        var cookies = loginResponse.Headers.GetValues("Set-Cookie");
        var rememberMeCookie = cookies.FirstOrDefault(c => c.Contains("rememberMe"));
        Assert.NotNull(rememberMeCookie);
        Assert.Contains("expires=", rememberMeCookie.ToLower());
    }

    [Fact]
    public async Task RefreshTokenRotation_Should_ProvideNewTokens()
    {
        var username = GenerateUniqueUsername("rotationuser");
        var email = GenerateUniqueEmail("rotation");
        await RegisterTestUser(username, email, "RotationTest123!");
        var loginResponse = await LoginTestUser(username, "RotationTest123!", false);

        var refreshTokenCookie = loginResponse.Headers.GetValues("Set-Cookie")
            .FirstOrDefault(c => c.Contains("refreshToken"));
        var originalRefreshToken = ExtractCookieValue(refreshTokenCookie, "refreshToken");

        var refreshRequest1 = new RefreshTokenRequest
        {
            RefreshToken = originalRefreshToken
        };

        var json1 = JsonConvert.SerializeObject(refreshRequest1);
        var content1 = new StringContent(json1, Encoding.UTF8, "application/json");

        var response1 = await _client.PostAsync("/api/auth/refresh-token", content1);
        response1.EnsureSuccessStatusCode();

        var refreshContent1 = await response1.Content.ReadAsStringAsync();
        var refreshResult1 = JsonConvert.DeserializeObject<RefreshTokenResponse>(refreshContent1);

        var newRefreshTokenCookie = response1.Headers.GetValues("Set-Cookie")
            .FirstOrDefault(c => c.Contains("refreshToken"));
        var newRefreshToken = ExtractCookieValue(newRefreshTokenCookie, "refreshToken");

        var refreshRequest2 = new RefreshTokenRequest
        {
            RefreshToken = newRefreshToken
        };

        var json2 = JsonConvert.SerializeObject(refreshRequest2);
        var content2 = new StringContent(json2, Encoding.UTF8, "application/json");

        var response2 = await _client.PostAsync("/api/auth/refresh-token", content2);

        response2.EnsureSuccessStatusCode();

        var refreshContent2 = await response2.Content.ReadAsStringAsync();
        var refreshResult2 = JsonConvert.DeserializeObject<RefreshTokenResponse>(refreshContent2);

        Assert.True(refreshResult1.Success);
        Assert.True(refreshResult2.Success);
        Assert.NotEqual(refreshResult1.AccessToken, refreshResult2.AccessToken);
    }

    [Fact]
    public async Task UsedRefreshToken_Should_BeInvalidForSubsequentRequests()
    {
        var username = GenerateUniqueUsername("usedtokenuser");
        var email = GenerateUniqueEmail("usedtoken");
        await RegisterTestUser(username, email, "UsedTokenTest123!");
        var loginResponse = await LoginTestUser(username, "UsedTokenTest123!", false);

        var refreshTokenCookie = loginResponse.Headers.GetValues("Set-Cookie")
            .FirstOrDefault(c => c.Contains("refreshToken"));
        var refreshToken = ExtractCookieValue(refreshTokenCookie, "refreshToken");

        var refreshRequest1 = new RefreshTokenRequest
        {
            RefreshToken = refreshToken
        };

        var json1 = JsonConvert.SerializeObject(refreshRequest1);
        var content1 = new StringContent(json1, Encoding.UTF8, "application/json");

        var response1 = await _client.PostAsync("/api/auth/refresh-token", content1);
        response1.EnsureSuccessStatusCode();

        var refreshRequest2 = new RefreshTokenRequest
        {
            RefreshToken = refreshToken // Same token as before
        };

        var json2 = JsonConvert.SerializeObject(refreshRequest2);
        var content2 = new StringContent(json2, Encoding.UTF8, "application/json");

        var response2 = await _client.PostAsync("/api/auth/refresh-token", content2);

        Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
    }

    [Fact]
    public async Task TokenExpiration_Should_RequireReauthentication()
    {
        var username = GenerateUniqueUsername("expirationuser");
        var email = GenerateUniqueEmail("expiration");
        await RegisterTestUser(username, email, "ExpirationTest123!");

        var loginResponse = await LoginTestUser(username, "ExpirationTest123!", false);

        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonConvert.DeserializeObject<LoginResponse>(loginContent);

        Assert.True(loginResult.Success);
        Assert.NotNull(loginResult.AccessTokenExpiresAt);
        Assert.NotNull(loginResult.RefreshTokenExpiresAt);
        Assert.True(loginResult.AccessTokenExpiresAt > DateTime.UtcNow);
        Assert.True(loginResult.RefreshTokenExpiresAt > DateTime.UtcNow);
        Assert.True(loginResult.RefreshTokenExpiresAt > loginResult.AccessTokenExpiresAt);
    }

    private async Task RegisterTestUser(string username, string email, string password)
    {
        var registerRequest = new RegisterRequest
        {
            Username = username,
            Email = email,
            Password = password,
            ConfirmPassword = password,
            FirstName = "Test",
            LastName = "User"
        };

        var json = JsonConvert.SerializeObject(registerRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/register", content);
        response.EnsureSuccessStatusCode();
    }

    private async Task<HttpResponseMessage> LoginTestUser(string username, string password, bool rememberMe)
    {
        var loginRequest = new LoginRequest
        {
            Username = username,
            Password = password,
            RememberMe = rememberMe
        };

        var json = JsonConvert.SerializeObject(loginRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);
        response.EnsureSuccessStatusCode();
        return response;
    }

    private static string GenerateUniqueUsername(string baseUsername)
    {
        return $"{baseUsername}_{Guid.NewGuid():N}";
    }

    private static string GenerateUniqueEmail(string baseEmail)
    {
        return $"{baseEmail}_{Guid.NewGuid():N}@test.com";
    }

    private static string ExtractCookieValue(string cookieHeader, string cookieName)
    {
        var parts = cookieHeader.Split(';');
        var cookiePart = parts.FirstOrDefault(p => p.Trim().StartsWith($"{cookieName}="));
        return cookiePart?.Split('=')[1]?.Trim() ?? string.Empty;
    }
}
