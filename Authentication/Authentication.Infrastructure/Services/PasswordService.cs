using Authentication.Application.Interfaces;

namespace Authentication.Infrastructure.Services;

public class PasswordService : IPasswordService
{
    public string HashPassword(string password) =>  BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));


    public bool VerifyPassword(string password, string hash) => BCrypt.Net.BCrypt.Verify(password, hash);

    public bool IsPasswordValid(string password)
    {
        throw new NotImplementedException();
    }

    public string GenerateRandomPassword(int lenght = 12)
    {
        throw new NotImplementedException();
    }
}