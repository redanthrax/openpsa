using FluentValidation;

namespace OpenPsa.Modules.Authentication.Features.Auth.Login;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest> {
    public LoginRequestValidator() {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid address.")
            .MaximumLength(320);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MaximumLength(256);
    }
}
