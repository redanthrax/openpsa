using Common.Database;
using IntegrationEvents.Contacts;
using Microsoft.EntityFrameworkCore;
using OpenPsa.Modules.Contacts.Models;

namespace OpenPsa.Modules.Contacts.Features.Integration;

public class CreateContactFromEmailHandler {
    public static async Task<CreateContactFromEmailResponse> Handle(
        CreateContactFromEmailCommand command, OpenPsaDbContext db) {
        var existing = await db.Set<Contact>()
            .FirstOrDefaultAsync(c => c.Email == command.EmailAddress);

        if (existing != null)
            return new CreateContactFromEmailResponse(existing.Id);

        var nameParts = (command.DisplayName ?? command.EmailAddress).Split(' ', 2);

        var contact = new Contact {
            ClientId = command.ClientId,
            FirstName = nameParts[0],
            LastName = nameParts.Length > 1 ? nameParts[1] : string.Empty,
            Email = command.EmailAddress
        };

        db.Set<Contact>().Add(contact);
        await db.SaveChangesAsync();

        return new CreateContactFromEmailResponse(contact.Id);
    }
}
