using Authentication.Domain.Enums;
using Xunit;

namespace Authentication.Tests.UnitTests;

public class EnumTests
{
    [Theory]
    [InlineData(UserType.EndUser, 1)]
    [InlineData(UserType.Admin, 2)]
    [InlineData(UserType.Partner, 3)]
    public void UserType_Should_HaveCorrectValues(UserType userType, int expectedValue)
    {
        // Act & Assert
        Assert.Equal(expectedValue, (int)userType);
    }

    [Theory]
    [InlineData(RoleType.Customer, 1)]
    [InlineData(RoleType.Admin, 2)]
    [InlineData(RoleType.Manager, 3)]
    [InlineData(RoleType.SuperAdmin, 4)]
    [InlineData(RoleType.Employee, 5)]
    [InlineData(RoleType.Partner, 6)]
    [InlineData(RoleType.Guest, 7)]
    public void RoleType_Should_HaveCorrectValues(RoleType roleType, int expectedValue)
    {
        // Act & Assert
        Assert.Equal(expectedValue, (int)roleType);
    }

    [Fact]
    public void UserType_Should_HaveAllRequiredValues()
    {
        // Arrange
        var expectedUserTypes = new[] { UserType.EndUser, UserType.Admin, UserType.Partner };

        // Act
        var allUserTypes = Enum.GetValues<UserType>();

        // Assert
        Assert.Equal(expectedUserTypes.Length, allUserTypes.Length);
        foreach (var expectedType in expectedUserTypes)
        {
            Assert.Contains(expectedType, allUserTypes);
        }
    }

    [Fact]
    public void RoleType_Should_HaveAllRequiredValues()
    {
        // Arrange
        var expectedRoleTypes = new[]
        {
            RoleType.Customer,
            RoleType.Admin,
            RoleType.Manager,
            RoleType.SuperAdmin,
            RoleType.Employee,
            RoleType.Partner,
            RoleType.Guest
        };

        // Act
        var allRoleTypes = Enum.GetValues<RoleType>();

        // Assert
        Assert.Equal(expectedRoleTypes.Length, allRoleTypes.Length);
        foreach (var expectedType in expectedRoleTypes)
        {
            Assert.Contains(expectedType, allRoleTypes);
        }
    }

    [Theory]
    [InlineData("EndUser")]
    [InlineData("Admin")]
    [InlineData("Partner")]
    public void UserType_Should_ParseFromString(string userTypeString)
    {
        // Act
        var success = Enum.TryParse<UserType>(userTypeString, out var userType);

        // Assert
        Assert.True(success);
        Assert.Equal(userTypeString, userType.ToString());
    }

    [Theory]
    [InlineData("Customer")]
    [InlineData("Admin")]
    [InlineData("Manager")]
    [InlineData("SuperAdmin")]
    [InlineData("Employee")]
    [InlineData("Partner")]
    [InlineData("Guest")]
    public void RoleType_Should_ParseFromString(string roleTypeString)
    {
        // Act
        var success = Enum.TryParse<RoleType>(roleTypeString, out var roleType);

        // Assert
        Assert.True(success);
        Assert.Equal(roleTypeString, roleType.ToString());
    }

    [Fact]
    public void UserType_Should_NotParseInvalidString()
    {
        // Act
        var success = Enum.TryParse<UserType>("InvalidUserType", out _);

        // Assert
        Assert.False(success);
    }

    [Fact]
    public void RoleType_Should_NotParseInvalidString()
    {
        // Act
        var success = Enum.TryParse<RoleType>("InvalidRoleType", out _);

        // Assert
        Assert.False(success);
    }
}
