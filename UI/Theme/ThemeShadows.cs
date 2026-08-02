using System.Drawing;

namespace RetroLauncher.UI.Theme
{
    public class ThemeShadows
    {
        public Color ShadowColor { get; set; } = Color.FromArgb(0, 0, 0);
        public int NormalAlpha { get; set; } = 25;
        public int HoverAlpha { get; set; } = 45;
        public int PressedAlpha { get; set; } = 12;
        public int NormalOffset { get; set; } = 2;
        public int PressedOffset { get; set; } = 1;
    }
}
