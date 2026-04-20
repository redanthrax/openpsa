namespace OpenPsa.Modules.Email.Templates;

public interface IEmailTemplateService {
    Task<EmailTemplateResult> RenderAsync(string templateName, object model);
}
