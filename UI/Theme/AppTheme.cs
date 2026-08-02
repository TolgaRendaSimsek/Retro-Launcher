using System.Drawing;
using RetroLauncher.Resources.Localization;

namespace RetroLauncher.UI.Theme
{
    public class AppTheme
    {
        private static AppTheme? _current;
        public static AppTheme Current => _current ??= new AppTheme();

        public ThemeColors Colors { get; } = new ThemeColors();
        public ThemeFonts Fonts { get; } = new ThemeFonts();
        public ThemeSpacing Spacing { get; } = new ThemeSpacing();
        public ThemeDimensions Dimensions { get; } = new ThemeDimensions();
        public ThemeRadius Radius { get; } = new ThemeRadius();
        public ThemeShadows Shadows { get; } = new ThemeShadows();

        public bool EnableAnimations { get; set; } = true;
        public string Density { get; set; } = "Comfortable";

        public void ApplyAccentColor(Color accent)
        {
            Colors.AccentPrimary = accent;
            Colors.AccentHover = Color.FromArgb(
                Math.Max(0, accent.R - 20),
                Math.Max(0, accent.G - 20),
                Math.Max(0, accent.B - 20));
            Colors.BorderFocus = accent;
        }

        public static void Refresh()
        {
            var scheme = ThemeManager.Instance.CurrentThemeScheme;
            Current.Colors.Background = scheme.BackgroundColor;
            Current.Colors.Surface = scheme.PanelColor;
            Current.Colors.SurfaceCard = scheme.CardColor;
            Current.Colors.TextPrimary = scheme.TextColor;
            Current.Colors.TextSecondary = scheme.SubtextColor;
            Current.ApplyAccentColor(scheme.AccentColor);
        }
    }
}
