namespace MySolarCells.Services;

public interface IThemeService
{
    AppTheme CurrentTheme { get; }
    AppTheme UserThemePreference { get; set; }
    void ApplyTheme(AppTheme theme);
    event EventHandler<AppTheme> ThemeChanged;
}

public class ThemeService : IThemeService
{
    private readonly ISettingsService settingsService;
    private AppTheme currentTheme;

    public event EventHandler<AppTheme> ThemeChanged = delegate { };

    public ThemeService(ISettingsService settingsService)
    {
        this.settingsService = settingsService;
        currentTheme = settingsService.UserTheme;
    }

    public AppTheme CurrentTheme => currentTheme;

    public AppTheme UserThemePreference
    {
        get => settingsService.UserTheme;
        set
        {
            if (settingsService.UserTheme != value)
            {
                settingsService.UserTheme = value;
                ApplyTheme(value);
            }
        }
    }

    public void ApplyTheme(AppTheme theme)
    {
        currentTheme = theme;

        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = theme;
        }

        ThemeChanged?.Invoke(this, theme);
    }
}
