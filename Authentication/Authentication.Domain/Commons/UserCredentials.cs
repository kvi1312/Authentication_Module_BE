namespace Authentication.Domain.Commons;

public class UserCredentials : ValueObject
{
    public UserCredentials(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));
            
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        Username = username;
        Password = password;
    }

    public string Username { get; }
    public string Password { get; }
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Username;
        yield return Password;
    }
}