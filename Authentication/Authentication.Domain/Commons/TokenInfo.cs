namespace Authentication.Domain.Commons;

public class TokenInfo : ValueObject
{
    public string AccessToken { get; }
    public string RefreshToken { get; }
    public DateTime ExpiresAt { get; }

    public TokenInfo(string accessToken, string refreshToken, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
            throw new ArgumentException("Access token cannot be empty", nameof(accessToken));

        if (string.IsNullOrWhiteSpace(refreshToken))
            throw new ArgumentException("Refresh token cannot be empty", nameof(refreshToken));

        AccessToken = accessToken;
        RefreshToken = refreshToken;
        ExpiresAt = expiresAt;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return AccessToken;
        yield return RefreshToken;
        yield return ExpiresAt;
    }

    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;
}