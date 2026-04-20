using System.Collections.Concurrent;
using System.Reflection;
using Scriban;
using Scriban.Runtime;

namespace OpenPsa.Modules.Email.Templates;

public class EmailTemplateService : IEmailTemplateService {
    private static readonly ConcurrentDictionary<string, (Template subject, Template html, Template text)> Cache = new();

    public async Task<EmailTemplateResult> RenderAsync(string templateName, object model) {
        var templates = Cache.GetOrAdd(templateName, LoadTemplates);

        var scriptObject = new ScriptObject();
        scriptObject.Import(model);

        var context = new TemplateContext();
        context.PushGlobal(scriptObject);

        var subject = await templates.subject.RenderAsync(context);
        context.Reset();
        context.PushGlobal(scriptObject);
        var html = await templates.html.RenderAsync(context);
        context.Reset();
        context.PushGlobal(scriptObject);
        var text = await templates.text.RenderAsync(context);

        return new EmailTemplateResult(subject.Trim(), html.Trim(), text.Trim());
    }

    private static (Template subject, Template html, Template text) LoadTemplates(string name) {
        var assembly = Assembly.GetExecutingAssembly();
        var prefix = $"Email.Templates.Content.{name}";

        var subjectSrc = ReadEmbeddedResource(assembly, $"{prefix}.subject.scriban") ?? "{{ ticket_number }} - {{ ticket_subject }}";
        var htmlSrc = ReadEmbeddedResource(assembly, $"{prefix}.html.scriban") ?? "<p>{{ body }}</p>";
        var textSrc = ReadEmbeddedResource(assembly, $"{prefix}.text.scriban") ?? "{{ body }}";

        return (Template.Parse(subjectSrc), Template.Parse(htmlSrc), Template.Parse(textSrc));
    }

    private static string? ReadEmbeddedResource(Assembly assembly, string name) {
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(name, StringComparison.OrdinalIgnoreCase));
        if (resourceName is null) return null;
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null) return null;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
