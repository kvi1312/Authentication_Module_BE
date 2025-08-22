namespace Authentication.Application.Interfaces;

public interface IPasswordService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    bool IsPasswordValid(string password);
    string GenerateRandomPassword(int lenght = 12);
}