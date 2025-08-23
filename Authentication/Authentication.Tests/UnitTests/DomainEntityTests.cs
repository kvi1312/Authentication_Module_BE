using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using Xunit;

namespace Authentication.Tests.UnitTests;

public class DomainEntityTests
{

    [Fact]
    public void RefreshToken_Should_BeInvalid_When_Expired()
    {
        // Arrange
        var token = RefreshToken.Create("test_token", "jwt_id", Guid.NewGuid(), TimeSpan.FromDays(-1));

        // Act & Assert
        Assert.False(token.IsValid());
        Assert.True(DateTime.UtcNow >= token.ExpiresAt);
    }



    [Fact]
    public void RefreshToken_Should_BeInvalid_When_Revoked()
    {
        // Arrange
        var token = RefreshToken.Create("test_token", "jwt_id", Guid.NewGuid(), TimeSpan.FromDays(7));

        // Act
        token.MarkAsRevoked();

        // Assert
        Assert.False(token.IsValid());
        Assert.True(token.IsRevoked);
    }
}
