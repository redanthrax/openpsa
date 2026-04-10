namespace OpenPsa.Web.Services;

public sealed class ThemeService {
    private bool _isDarkMode;

    public bool IsDarkMode {
        get => _isDarkMode;
        set {
            if (_isDarkMode == value) return;
            _isDarkMode = value;
            OnThemeChanged?.Invoke();
        }
    }

    public event Action? OnThemeChanged;

    public void Initialize(bool isDarkMode) => _isDarkMode = isDarkMode;
}
