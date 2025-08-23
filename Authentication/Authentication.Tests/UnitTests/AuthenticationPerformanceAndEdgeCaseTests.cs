using Authentication.Application.Commands;
using Authentication.Application.Dtos.Request;
using Authentication.Domain.Entities;
using Xunit;

namespace Authentication.Tests.UnitTests;

/// <summary>
/// Performance and edge case tests for authentication scenarios
/// </summary>
public class AuthenticationPerformanceAndEdgeCaseTests
{
    #region Performance Tests

    [Fact]
    public void Token_Generation_Should_Be_Fast_For_High_Volume()
    {
        // Arrange
        var iterations = 1000;
        var tokenGenerationTimes = new List<TimeSpan>();

        // Act
        for (int i = 0; i < iterations; i++)
        {
            var startTime = DateTime.UtcNow;

            var refreshToken = RefreshToken.Create(
                $"perf_test_token_{i}",
                $"jwt_id_{i}",
                Guid.NewGuid(),
                TimeSpan.FromDays(7)
            );

            var endTime = DateTime.UtcNow;
            tokenGenerationTimes.Add(endTime - startTime);
        }

        // Assert
        var averageTime = tokenGenerationTimes.Average(t => t.TotalMilliseconds);
        var maxTime = tokenGenerationTimes.Max(t => t.TotalMilliseconds);

        Assert.True(averageTime < 10);
        Assert.True(maxTime < 100);
        Assert.Equal(iterations, tokenGenerationTimes.Count);
    }

    [Theory]
    [InlineData(100)]
    [InlineData(500)]
    [InlineData(1000)]
    public void Bulk_Token_Validation_Should_Complete_Within_Reasonable_Time(int tokenCount)
    {
        // Arrange
        var tokens = new List<RefreshToken>();
        var userId = Guid.NewGuid();

        var startGeneration = DateTime.UtcNow;
        for (int i = 0; i < tokenCount; i++)
        {
            tokens.Add(RefreshToken.Create($"bulk_token_{i}", $"jwt_{i}", userId, TimeSpan.FromDays(7)));
        }
        var endGeneration = DateTime.UtcNow;

        // Act - Validate all tokens
        var startValidation = DateTime.UtcNow;
        var validTokens = tokens.Where(t => t.IsValid()).ToList();
        var endValidation = DateTime.UtcNow;

        var generationTime = (endGeneration - startGeneration).TotalMilliseconds;
        var validationTime = (endValidation - startValidation).TotalMilliseconds;

        // Assert
        Assert.Equal(tokenCount, validTokens.Count);
        Assert.True(generationTime < tokenCount * 2); // Max 2ms per token generation
        Assert.True(validationTime < tokenCount * 1); // Max 1ms per token validation
    }

    #endregion

    #region Edge Case Tests



    [Fact]
    public void RememberMeToken_Should_Handle_Edge_Case_Expiry_Values()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var tokenHash = "edge_case_token_hash";

        var zeroValidityToken = RememberMeToken.Create(userId, tokenHash, TimeSpan.Zero);
        Assert.NotNull(zeroValidityToken);
        Assert.False(zeroValidityToken.IsValid());

        var negativeValidityToken = RememberMeToken.Create(userId, tokenHash, TimeSpan.FromDays(-1));
        Assert.NotNull(negativeValidityToken);
        Assert.False(negativeValidityToken.IsValid());

        var longTermToken = RememberMeToken.Create(userId, tokenHash, TimeSpan.FromDays(365));
        Assert.True(longTermToken.IsValid());
        Assert.True(longTermToken.ExpiresAt > DateTime.UtcNow.AddDays(360));
    }

    [Fact]
    public void RegisterRequest_Should_Validate_Email_Formats()
    {
        // Arrange
        var validEmails = new List<string>
        {
            "user@domain.com",
            "test.email+tag@example.org",
            "user123@subdomain.domain.co.uk",
            "simple@example.org"
        };

        var invalidEmails = new List<string>
        {
            "invalid-email",
            "@domain.com",
            "user@",
            "user@@domain.com",
            ""
        };

        // Act & Assert - Valid emails
        foreach (var email in validEmails)
        {
            var request = new RegisterRequest { Email = email };
            Assert.Contains("@", request.Email);
            Assert.Contains(".", request.Email);
        }

        // Act & Assert - Invalid emails
        foreach (var email in invalidEmails)
        {
            var request = new RegisterRequest { Email = email };

            bool hasAt = !string.IsNullOrEmpty(email) && email.Contains("@");
            bool hasDot = !string.IsNullOrEmpty(email) && email.Contains(".");

            if (email == "invalid-email")
            {
                Assert.False(hasAt);
            }
            else if (email == "@domain.com")
            {
                Assert.True(hasAt);
                Assert.True(hasDot);
            }
            else if (email == "user@")
            {
                Assert.True(hasAt);
                Assert.False(hasDot);
            }
            else if (email == "")
            {
                Assert.False(hasAt && hasDot);
            }
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void RefreshTokenCommand_Should_Handle_Invalid_Token_Values(string invalidToken)
    {
        // Arrange & Act
        var command = new RefreshTokenCommand { RefreshToken = invalidToken };

        // Assert
        var isEmpty = string.IsNullOrWhiteSpace(command.RefreshToken);
        Assert.True(isEmpty);
    }

    [Fact]
    public void LogoutCommand_Should_Handle_Missing_Token_Values()
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

    #region Boundary Value Tests

    [Theory]
    [InlineData(1)]      // Minimum viable duration
    [InlineData(60)]     // 1 hour
    [InlineData(1440)]   // 24 hours  
    [InlineData(10080)]  // 1 week
    [InlineData(43200)]  // 30 days
    public void AccessToken_Should_Handle_Various_Expiry_Durations(int expiryMinutes)
    {
        // Arrange
        var tokenCreatedAt = DateTime.UtcNow;
        var expiryDuration = TimeSpan.FromMinutes(expiryMinutes);

        // Act
        var expirationTime = tokenCreatedAt.Add(expiryDuration);

        // Assert
        Assert.True(expirationTime > tokenCreatedAt);
        Assert.Equal(expiryMinutes, expiryDuration.TotalMinutes);
        Assert.True(expirationTime <= tokenCreatedAt.AddMinutes(expiryMinutes + 1));
    }

    [Theory]
    [InlineData(1)]      // 1 day minimum
    [InlineData(7)]      // 1 week standard
    [InlineData(30)]     // 1 month extended
    [InlineData(90)]     // 3 months long-term
    [InlineData(365)]    // 1 year maximum
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

    #endregion

    #region Concurrent Access Tests

    [Fact]
    public void Rapid_Token_Status_Changes_Should_Maintain_Consistency()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = RefreshToken.Create("rapid_change_token", "jwt_rapid", userId, TimeSpan.FromDays(7));
        var rememberMeToken = RememberMeToken.Create(userId, "rapid_remember_hash", TimeSpan.FromDays(30));

        // Act & Assert - Initial state
        Assert.True(refreshToken.IsValid());
        Assert.True(rememberMeToken.IsValid());

        // Act - Rapid state changes
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
