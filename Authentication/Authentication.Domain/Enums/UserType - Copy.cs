namespace Authentication.Domain.Enums;

public enum AuthenticationResult
{
    Success,
    InvalidCredentials,
    AccountLocked,
    AccountInactive,
    UserTypeNotAllowed,
    InvalidUserType
}
