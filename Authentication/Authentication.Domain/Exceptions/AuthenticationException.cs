namespace Authentication.Domain.Exceptions;

public class AuthenticationException : Exception
{
    public string Code { get; }

    public AuthenticationException(string code, string message) : base(message)
    {
        Code = code;
    }

    public AuthenticationException(string code, string message, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
    }
}