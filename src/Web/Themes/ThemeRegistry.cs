using OpenPsa.Web.Services;

namespace OpenPsa.Web.Themes;

public static class ThemeRegistry {
    public static MudBlazor.MudTheme GetTheme(ThemePreset preset) => preset switch {
        ThemePreset.Cyberpunk => CyberpunkTheme.Create(),
        _ => ProfessionalTheme.Create()
    };
}
