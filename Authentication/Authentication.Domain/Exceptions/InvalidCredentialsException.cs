namespace Authentication.Domain.Exceptions;

public class InvalidCredentialsException()
    : AuthenticationException("INVALID_CREDENTIALS", "Invalid username or password");