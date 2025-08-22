using Authentication.Application.Interfaces;
using System.Text.RegularExpressions;

namespace Authentication.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    public string HashPassword(string password) => BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));

    public bool VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);

    public bool IsPasswordValid(string password)
    {
        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            return false;

        var hasUpperCase = Regex.IsMatch(password, @"[A-Z]");
        var hasLowerCase = Regex.IsMatch(password, @"[a-z]");
        var hasDigit = Regex.IsMatch(password, @"[0-9]");
        var hasSpecialChar = Regex.IsMatch(password, @"[!@#$%^&*(),.?""{}|<>]");

        return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
    }

    public string GenerateRandomPassword(int length = 12)
    {
        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        var password = new char[length];

        for (int i = 0; i < length; i++)
        {
            password[i] = validChars[random.Next(validChars.Length)];
        }

        return new string(password);
    }
}