using Microsoft.JSInterop;

namespace OpenPsa.Web.Services;

public enum ThemePreset { Professional, Cyberpunk }

public sealed class ThemeService {
    private bool _isDarkMode;
    private ThemePreset _preset = ThemePreset.Professional;

    public bool IsDarkMode {
        get => _isDarkMode;
        set {
            if (_isDarkMode == value) return;
            _isDarkMode = value;
            OnThemeChanged?.Invoke();
        }
    }

    public ThemePreset Preset {
        get => _preset;
        set {
            if (_preset == value) return;
            _preset = value;
            OnThemeChanged?.Invoke();
        }
    }

    public event Action? OnThemeChanged;

    public void Initialize(bool isDarkMode, ThemePreset preset) {
        _isDarkMode = isDarkMode;
        _preset = preset;
    }
}
