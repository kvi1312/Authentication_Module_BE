namespace Authentication.Domain.Exceptions;

public class TokenRevokedException() : AuthenticationException("TOKEN_REVOKED", "Token has been revoked");