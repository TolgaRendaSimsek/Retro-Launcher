using System.Drawing;

namespace RetroLauncher.UI.Theme
{
    public class ThemeFonts
    {
        public Font TitleLarge { get; set; } = new Font("Segoe UI", 18F, FontStyle.Bold);
        public Font TitleMedium { get; set; } = new Font("Segoe UI", 14F, FontStyle.Bold);
        public Font TitleSmall { get; set; } = new Font("Segoe UI", 11F, FontStyle.Bold);

        public Font BodyLarge { get; set; } = new Font("Segoe UI", 10F, FontStyle.Regular);
        public Font BodyMedium { get; set; } = new Font("Segoe UI", 9F, FontStyle.Regular);
        public Font BodySmall { get; set; } = new Font("Segoe UI", 8.5F, FontStyle.Regular);

        public Font ButtonMedium { get; set; } = new Font("Segoe UI", 9F, FontStyle.Bold);
        public Font ButtonSmall { get; set; } = new Font("Segoe UI", 8.5F, FontStyle.Bold);

        public Font BadgeFont { get; set; } = new Font("Segoe UI", 8F, FontStyle.Bold);
        public Font CodeFont { get; set; } = new Font("Consolas", 8.5F, FontStyle.Regular);
    }
}
