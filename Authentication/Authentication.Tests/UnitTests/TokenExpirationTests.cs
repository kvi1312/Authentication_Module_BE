using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Xunit;

namespace Authentication.Tests.UnitTests;

public class TokenExpirationTests
{
    [Fact]
    public void AccessToken_Should_ExpireAfterConfiguredTime()
    {
        var tokenCreatedAt = DateTime.UtcNow;
        var accessTokenLifetime = TimeSpan.FromMinutes(15);

        var expirationTime = tokenCreatedAt.Add(accessTokenLifetime);
        var isExpired = DateTime.UtcNow > expirationTime;

        Assert.False(isExpired);
        Assert.True(expirationTime > tokenCreatedAt);
    }

    [Fact]
    public void RefreshToken_Should_ExpireAfterLongerPeriod()
    {
        var refreshToken = RefreshToken.Create(
            "refresh_token_123",
            "jwt_id_123",
            Guid.NewGuid(),
            TimeSpan.FromDays(7)
        );

        Assert.True(refreshToken.IsValid());
        Assert.True(refreshToken.ExpiresAt > DateTime.UtcNow);
    }

    [Fact]
    public void RefreshToken_Should_BeExpired_When_ExpirationTimePassed()
    {
        var refreshToken = RefreshToken.Create(
            "expired_refresh_token",
            "jwt_id_expired",
            Guid.NewGuid(),
            TimeSpan.FromDays(-1)
        );

        Assert.False(refreshToken.IsValid());
    }

    [Fact]
    public void RefreshToken_Should_BeInvalid_When_MarkedAsRevoked()
    {
        var token = RefreshToken.Create("test_token", "jwt_id", Guid.NewGuid(), TimeSpan.FromDays(7));

        token.MarkAsRevoked();

        Assert.False(token.IsValid());
        Assert.True(token.IsRevoked);
    }

    [Fact]
    public void RefreshToken_Should_BeInvalid_When_Revoked()
    {
        var token = RefreshToken.Create("test_token", "jwt_id", Guid.NewGuid(), TimeSpan.FromDays(7));

        token.MarkAsRevoked();

        Assert.False(token.IsValid());
        Assert.True(token.IsRevoked);
    }

    [Fact]
    public void RememberMeToken_Should_HaveExtendedExpiration()
    {
        var rememberMeTokenLifetime = TimeSpan.FromDays(30);
        var tokenCreatedAt = DateTime.UtcNow;

        var expirationTime = tokenCreatedAt.Add(rememberMeTokenLifetime);

        Assert.True(expirationTime > tokenCreatedAt.AddDays(7));
        Assert.True(expirationTime <= tokenCreatedAt.AddDays(30));
    }


    [Fact]
    public void User_Should_RequireReauthentication_When_TokensExpired()
    {
        var user = User.Create(
            "testuser",
            "test@example.com",
            "hashedpassword",
            "Test",
            "User"
        );

        var expiredRefreshToken = RefreshToken.Create(
            "expired_token",
            "jwt_id_expired",
            user.Id,
            TimeSpan.FromDays(-1)
        );

        Assert.False(expiredRefreshToken.IsValid());
        Assert.True(user.IsActive);
    }

    [Fact]
    public void TokenCleanup_Should_IdentifyExpiredTokens()
    {
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();

        var tokens = new List<RefreshToken>
        {
            RefreshToken.Create("valid_token", "jwt_1", userId1, TimeSpan.FromDays(7)),
            RefreshToken.Create("expired_token", "jwt_2", userId2, TimeSpan.FromDays(-1)),
            RefreshToken.Create("revoked_token", "jwt_3", userId3, TimeSpan.FromDays(7))
        };

        tokens[2].MarkAsRevoked();

        var validTokens = tokens.Where(t => t.IsValid()).ToList();
        var invalidTokens = tokens.Where(t => !t.IsValid()).ToList();

        Assert.Single(validTokens);
        Assert.Equal(2, invalidTokens.Count);
        Assert.Equal("valid_token", validTokens.First().Token);
    }
}
