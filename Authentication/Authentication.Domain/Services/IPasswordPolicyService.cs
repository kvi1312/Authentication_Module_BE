namespace Authentication.Domain.Services;

public interface IPasswordPolicyService
{
    bool IsValidPassword(string password);
    string GetPasswordRequirements();
}