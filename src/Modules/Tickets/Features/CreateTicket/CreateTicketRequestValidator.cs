using Contracts.Tickets;
using FluentValidation;

namespace OpenPsa.Modules.Tickets.Features.CreateTicket;

public sealed class CreateTicketRequestValidator : AbstractValidator<CreateTicketRequest> {
    public CreateTicketRequestValidator() {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be 200 characters or fewer.");

        RuleFor(x => x.Description)
            .MaximumLength(8000)
            .When(x => x.Description is not null);

        RuleFor(x => x.ClientId)
            .NotEqual(Guid.Empty).WithMessage("ClientId is required.");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow.AddMinutes(-5)).WithMessage("DueDate cannot be in the past.")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.AssignedToUserId)
            .Must(id => Guid.TryParse(id, out _))
            .WithMessage("AssignedToUserId must be a valid GUID.")
            .When(x => !string.IsNullOrEmpty(x.AssignedToUserId));
    }
}
