namespace Authentication.Domain.Exceptions;

public class TokenExpiredException() : AuthenticationException("TOKEN_EXPIRED", "Token has expired");
