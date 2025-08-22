using Authentication.Domain.Enums;

namespace Authentication.Domain.Exceptions;

public class InvalidUserTypeException(UserType requestedUserType, UserType[] allowedUserTypes)
    : AuthenticationException("INVALID_USER_TYPE",
        $"User type {requestedUserType} is not allowed. Allowed types: {string.Join(", ", allowedUserTypes)}")
{
    public UserType RequestedUserType { get; } = requestedUserType;
    public UserType[] AllowedUserTypes { get; } = allowedUserTypes;
}