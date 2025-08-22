using Authentication.Application.Commands;
using Authentication.Domain.Enums;
using FluentValidation;

namespace Authentication.Application.Validators;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 50).WithMessage("Username must be between 3 and 50 characters")
            .Matches("^[a-zA-Z0-9._-]+$").WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }

    private bool BeValidUserType(UserType userType)
    {
        return userType == UserType.EndUser ||
               userType == UserType.Admin ||
               userType == UserType.Partner;
    }
}