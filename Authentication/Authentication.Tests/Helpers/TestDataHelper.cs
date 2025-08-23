using Authentication.Application.Dtos.Response;
using Authentication.Domain.Entities;
using Authentication.Domain.Enums;
using System;
using System.Collections.Generic;

namespace Authentication.Tests.Helpers;

public static class TestDataHelper
{
    public static User CreateTestUser(
        string username = "testuser",
        string email = "test@test.com",
        string firstName = "Test",
        string lastName = "User",
        bool isActive = true)
    {
        var user = User.Create(username, email, "hashedpassword", firstName, lastName);

        if (!isActive)
        {
            user.Deactivate();
        }

        return user;
    }

    public static User CreateTestUserWithRoles(
        List<RoleType> roleTypes,
        string username = "testuser",
        string email = "test@test.com",
        string firstName = "Test",
        string lastName = "User")
    {
        var user = CreateTestUser(username, email, firstName, lastName);

        foreach (var roleType in roleTypes)
        {
            var role = CreateTestRole(roleType);
            user.UserRoles.Add(new UserRole
            {
                UserId = user.Id,
                RoleId = role.Id,
                User = user,
                Role = role,
                AssignedDate = DateTimeOffset.UtcNow
            });
        }

        return user;
    }

    public static Role CreateTestRole(RoleType roleType)
    {
        var (name, description, userType) = GetRoleDetails(roleType);
        return Role.Create(name, description, userType);
    }

    public static RefreshToken CreateTestRefreshToken(
        Guid userId,
        string token = "test_refresh_token",
        string jwtId = "test_jwt_id",
        TimeSpan? validity = null,
        bool isRevoked = false)
    {
        var refreshToken = RefreshToken.Create(
            token,
            jwtId,
            userId,
            validity ?? TimeSpan.FromDays(7)
        );

        if (isRevoked)
        {
            refreshToken.MarkAsRevoked();
        }

        return refreshToken;
    }

    public static RefreshToken CreateExpiredRefreshToken(
        Guid userId,
        string token = "expired_refresh_token",
        string jwtId = "expired_jwt_id")
    {
        return RefreshToken.Create(
            token,
            jwtId,
            userId,
            TimeSpan.FromDays(-1) // Expired 1 day ago
        );
    }

    private static (string name, string description, UserType userType) GetRoleDetails(RoleType roleType)
    {
        return roleType switch
        {
            RoleType.Customer => ("Customer", "Standard customer access", UserType.EndUser),
            RoleType.Guest => ("Guest", "Guest user access", UserType.EndUser),
            RoleType.Admin => ("Admin", "Standard administrative access", UserType.Admin),
            RoleType.SuperAdmin => ("SuperAdmin", "Full system administrative access", UserType.Admin),
            RoleType.Manager => ("Manager", "Management level access", UserType.Partner),
            RoleType.Partner => ("Partner", "Standard partner access", UserType.Partner),
            RoleType.Employee => ("Employee", "Employee level access", UserType.Partner),
            _ => ("Unknown", "Unknown role", UserType.EndUser)
        };
    }

    public static class MockResponses
    {
        public static class Successful
        {
            public static LoginResponse Login(string accessToken = "access_token", string refreshToken = "refresh_token")
            {
                return new LoginResponse
                {
                    Success = true,
                    Message = "Login successful",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AccessTokenExpiresAt = DateTime.UtcNow.AddHours(1)
                };
            }

            public static RegisterResponse Register()
            {
                return new RegisterResponse
                {
                    Success = true,
                    Message = "Registration successful. You can now login."
                };
            }

            public static RefreshTokenResponse RefreshToken(string accessToken = "new_access_token", string refreshToken = "new_refresh_token")
            {
                return new RefreshTokenResponse
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    AccessTokenExpiresAt = DateTime.UtcNow.AddHours(1),
                    RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7)
                };
            }
        }

        public static class Failed
        {
            public static LoginResponse InvalidCredentials()
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid credentials"
                };
            }

            public static LoginResponse AccountInactive()
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = "Account is inactive"
                };
            }

            public static RegisterResponse UsernameExists()
            {
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Username or email already exists"
                };
            }

            public static RefreshTokenResponse ExpiredToken()
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = "Invalid or expired refresh token"
                };
            }

            public static RefreshTokenResponse UserNotFound()
            {
                return new RefreshTokenResponse
                {
                    Success = false,
                    Message = "User not found or inactive"
                };
            }
        }
    }

    public static class Constants
    {
        public const string ValidPassword = "password123";
        public const string InvalidPassword = "wrongpassword";
        public const string HashedPassword = "hashed_password_123";

        public const string ValidUsername = "testuser";
        public const string InvalidUsername = "nonexistentuser";
        public const string ExistingUsername = "existinguser";

        public const string ValidEmail = "test@test.com";
        public const string InvalidEmail = "invalid@test.com";
        public const string ExistingEmail = "existing@test.com";

        public const string ValidRefreshToken = "valid_refresh_token";
        public const string InvalidRefreshToken = "invalid_refresh_token";
        public const string ExpiredRefreshToken = "expired_refresh_token";
        public const string UsedRefreshToken = "used_refresh_token";
        public const string RevokedRefreshToken = "revoked_refresh_token";

        public const string ValidAccessToken = "valid_access_token";
        public const string ExpiredAccessToken = "expired_access_token";

        public const string ValidJwtId = "valid_jwt_id";
        public const string ExpiredJwtId = "expired_jwt_id";

        public const string RememberMeToken = "remember_me_token";
        public const string HashedRememberMeToken = "hashed_remember_me_token";
    }
}
