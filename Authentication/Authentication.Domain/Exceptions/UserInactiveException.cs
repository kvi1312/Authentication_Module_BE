namespace Authentication.Domain.Exceptions;

public class UserInactiveException() : AuthenticationException("USER_INACTIVE", "User account is inactive");