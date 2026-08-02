using System.Drawing;

namespace RetroLauncher.UI.Theme
{
    public class ThemeColors
    {
        // Dark Theme Default Palette (Deep Navy / Charcoal + Purple/Blue Accents)
        public Color Background { get; set; } = Color.FromArgb(15, 17, 23);         // #0F1117
        public Color Surface { get; set; } = Color.FromArgb(24, 27, 36);            // #181B24
        public Color SurfaceCard { get; set; } = Color.FromArgb(30, 34, 48);        // #1E2230
        public Color SurfaceCardHover { get; set; } = Color.FromArgb(39, 44, 61);   // #272C3D
        public Color SurfaceCardSelected { get; set; } = Color.FromArgb(49, 56, 77);

        public Color AccentPrimary { get; set; } = Color.FromArgb(99, 102, 241);     // #6366F1
        public Color AccentHover { get; set; } = Color.FromArgb(79, 70, 229);        // #4F46E5
        public Color AccentSecondary { get; set; } = Color.FromArgb(139, 92, 246);   // #8B5CF6
        public Color AccentGradientEnd { get; set; } = Color.FromArgb(124, 58, 237);  // #7C3AED

        public Color TextPrimary { get; set; } = Color.FromArgb(249, 250, 251);     // #F9FAFB
        public Color TextSecondary { get; set; } = Color.FromArgb(156, 163, 175);   // #9CA3AF
        public Color TextMuted { get; set; } = Color.FromArgb(107, 114, 128);       // #6B7280
        public Color TextDisabled { get; set; } = Color.FromArgb(75, 85, 99);       // #4B5563

        public Color Border { get; set; } = Color.FromArgb(46, 53, 72);             // #2E3548
        public Color BorderHover { get; set; } = Color.FromArgb(75, 85, 99);        // #4B5563
        public Color BorderFocus { get; set; } = Color.FromArgb(99, 102, 241);       // #6366F1

        public Color StatusSuccess { get; set; } = Color.FromArgb(16, 185, 129);    // #10B981
        public Color StatusSuccessBg { get; set; } = Color.FromArgb(6, 78, 59);
        public Color StatusWarning { get; set; } = Color.FromArgb(245, 158, 11);    // #F59E0B
        public Color StatusWarningBg { get; set; } = Color.FromArgb(120, 53, 15);
        public Color StatusError { get; set; } = Color.FromArgb(239, 68, 68);       // #EF4444
        public Color StatusErrorBg { get; set; } = Color.FromArgb(127, 29, 29);
        public Color StatusInfo { get; set; } = Color.FromArgb(59, 130, 246);       // #3B82F6
        public Color StatusInfoBg { get; set; } = Color.FromArgb(30, 58, 138);

        public Color SidebarBackground { get; set; } = Color.FromArgb(11, 13, 18);
        public Color SidebarItemHover { get; set; } = Color.FromArgb(24, 27, 36);
        public Color SidebarItemSelected { get; set; } = Color.FromArgb(39, 44, 61);

        public Color TopBarBackground { get; set; } = Color.FromArgb(18, 20, 28);
    }
}
