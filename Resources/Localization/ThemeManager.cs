using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Forms;

namespace RetroLauncher.Resources.Localization
{
    public class ThemeSettings
    {
        public string ActiveTheme { get; set; } = "Dark"; // Dark, OLED, Light, Retro, PlayStation, Xbox, Nintendo, Custom
        public string AccentColorHtml { get; set; } = "#6366F1"; // Indigo default
        public string? BackgroundImagePath { get; set; }
        public string FontSizeName { get; set; } = "Medium"; // Small, Medium, Large
    }

    public class ThemeScheme
    {
        public Color BackgroundColor { get; set; }
        public Color PanelColor { get; set; }
        public Color CardColor { get; set; }
        public Color TextColor { get; set; }
        public Color SubtextColor { get; set; }
        public Color AccentColor { get; set; }
        public Color HoverColor { get; set; }
    }

    public class ThemeManager
    {
        private static readonly string SettingsPath = ApplicationPaths.ThemeSettingsJson;
        private static readonly object LockObj = new();
        private static readonly ConditionalWeakTable<Control, StrongBox<float>> BaseFontSizes = new();

        private ThemeSettings _settings = new();
        private static ThemeManager? _instance;

        public event EventHandler? ThemeChanged;

        public static ThemeManager Instance
        {
            get
            {
                lock (LockObj)
                {
                    if (_instance == null)
                    {
                        _instance = new ThemeManager();
                    }
                    return _instance;
                }
            }
        }

        private ThemeManager()
        {
            LoadThemeSettings();
        }

        public ThemeSettings Settings => _settings;

        public ThemeScheme CurrentThemeScheme => GetThemeScheme(_settings.ActiveTheme);

        public void LoadThemeSettings()
        {
            lock (LockObj)
            {
                try
                {
                    if (File.Exists(SettingsPath))
                    {
                        string json = File.ReadAllText(SettingsPath);
                        var s = JsonSerializer.Deserialize<ThemeSettings>(json);
                        if (s != null)
                        {
                            _settings = s;
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading theme settings: {ex.Message}");
                }
                _settings = new ThemeSettings();
            }
        }

        public void SaveThemeSettings()
        {
            lock (LockObj)
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(_settings, options);
                    File.WriteAllText(SettingsPath, json);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error saving theme settings: {ex.Message}");
                }
            }
        }

        public void ResetToDefaultTheme()
        {
            _settings = new ThemeSettings();
            SaveThemeSettings();
            OnThemeChanged();
        }

        public void SetAccentColor(Color color)
        {
            _settings.AccentColorHtml = ColorTranslator.ToHtml(color);
            SaveThemeSettings();
            OnThemeChanged();
        }

        public void SetBackgroundImage(string? path)
        {
            _settings.BackgroundImagePath = path;
            SaveThemeSettings();
            OnThemeChanged();
        }

        public void ApplyTheme(Form form)
        {
            var scheme = CurrentThemeScheme;

            // Form Background
            form.BackColor = scheme.BackgroundColor;

            // Optional background image
            if (!string.IsNullOrEmpty(_settings.BackgroundImagePath) && File.Exists(_settings.BackgroundImagePath))
            {
                try
                {
                    form.BackgroundImage = Image.FromFile(_settings.BackgroundImagePath);
                    form.BackgroundImageLayout = ImageLayout.Stretch;
                }
                catch
                {
                    form.BackgroundImage = null;
                }
            }
            else
            {
                form.BackgroundImage = null;
            }

            // Recurse children controls
            foreach (Control control in form.Controls)
            {
                ApplyThemeToControl(control);
            }
        }

        public void ApplyThemeToControl(Control control)
        {
            var scheme = CurrentThemeScheme;
            float scale = GetFontScaleFactor(_settings.FontSizeName);

            // Save original base font size to prevent cumulative scaling loops
            if (!BaseFontSizes.TryGetValue(control, out var baseSize))
            {
                baseSize = new StrongBox<float>(control.Font.Size);
                BaseFontSizes.Add(control, baseSize);
            }

            // Apply scaled font size
            float targetSize = baseSize.Value * scale;
            if (Math.Abs(control.Font.Size - targetSize) > 0.01f)
            {
                control.Font = new Font(control.Font.FontFamily, targetSize, control.Font.Style);
            }

            // Paint control color scheme
            if (control is Panel pnl)
            {
                string pName = pnl.Name.ToLowerInvariant();
                if (pName.Contains("sidebar") || pName.Contains("details") || pName.Contains("top") || pName.Contains("topbar") || pName.Contains("header"))
                {
                    pnl.BackColor = scheme.PanelColor;
                }
                else if (pName.Contains("card") || pName.Contains("grid") || pName.Contains("placeholder") || pnl.Name.Contains("pnlConfig"))
                {
                    pnl.BackColor = scheme.CardColor;
                }
                else
                {
                    pnl.BackColor = scheme.BackgroundColor;
                }
            }
            else if (control is ListBox || control is ListView)
            {
                control.BackColor = scheme.CardColor;
                control.ForeColor = scheme.TextColor;
            }
            else if (control is TextBox)
            {
                control.BackColor = scheme.CardColor;
                control.ForeColor = scheme.TextColor;
            }
            else if (control is ComboBox cb)
            {
                cb.BackColor = scheme.CardColor;
                cb.ForeColor = scheme.TextColor;
            }
            else if (control is Label lbl)
            {
                string lName = lbl.Name.ToLowerInvariant();
                if (lName.Contains("header") || lName.Contains("label") || lName.Contains("console") || lName.Contains("choose") || lName.Contains("sub"))
                {
                    lbl.ForeColor = scheme.SubtextColor;
                }
                else
                {
                    lbl.ForeColor = scheme.TextColor;
                }
            }
            else if (control is Button btn)
            {
                string bName = btn.Name.ToLowerInvariant();
                if (bName.Contains("play") || bName.Contains("scan") || bName.Contains("save") || bName.Contains("create") || bName.Contains("apply"))
                {
                    btn.BackColor = scheme.AccentColor;
                    btn.ForeColor = Color.White;
                }
                else if (bName.Contains("delete") || bName.Contains("remove"))
                {
                    btn.BackColor = Color.FromArgb(239, 68, 68); // Soft Red remains Red
                    btn.ForeColor = Color.White;
                }
                else
                {
                    btn.BackColor = scheme.CardColor;
                    btn.ForeColor = scheme.TextColor;
                }
            }

            // Recurse descendants
            foreach (Control child in control.Controls)
            {
                ApplyThemeToControl(child);
            }
        }

        public void OnThemeChanged()
        {
            ThemeChanged?.Invoke(this, EventArgs.Empty);
        }

        private float GetFontScaleFactor(string fontName)
        {
            return fontName switch
            {
                "Small" => 0.85f,
                "Large" => 1.20f,
                _ => 1.00f
            };
        }

        private ThemeScheme GetThemeScheme(string themeName)
        {
            ThemeScheme scheme = new ThemeScheme();

            switch (themeName)
            {
                case "OLED":
                    scheme.BackgroundColor = Color.Black;
                    scheme.PanelColor = Color.Black;
                    scheme.CardColor = Color.FromArgb(18, 18, 18);
                    scheme.TextColor = Color.White;
                    scheme.SubtextColor = Color.FromArgb(156, 163, 175);
                    scheme.AccentColor = ColorTranslator.FromHtml(_settings.AccentColorHtml);
                    scheme.HoverColor = Color.FromArgb(55, 65, 81);
                    break;

                case "Light":
                    scheme.BackgroundColor = Color.FromArgb(243, 244, 246);
                    scheme.PanelColor = Color.White;
                    scheme.CardColor = Color.FromArgb(229, 231, 235);
                    scheme.TextColor = Color.FromArgb(17, 24, 39);
                    scheme.SubtextColor = Color.FromArgb(107, 114, 128);
                    scheme.AccentColor = ColorTranslator.FromHtml(_settings.AccentColorHtml);
                    scheme.HoverColor = Color.FromArgb(67, 56, 202);
                    break;

                case "Retro":
                    scheme.BackgroundColor = Color.FromArgb(224, 224, 224); // SNES Light
                    scheme.PanelColor = Color.FromArgb(189, 189, 189); // SNES Grey
                    scheme.CardColor = Color.FromArgb(245, 245, 245);
                    scheme.TextColor = Color.Black;
                    scheme.SubtextColor = Color.FromArgb(64, 64, 64);
                    scheme.AccentColor = Color.FromArgb(63, 81, 181); // Purple
                    scheme.HoverColor = Color.FromArgb(48, 63, 159);
                    break;

                case "PlayStation":
                    scheme.BackgroundColor = Color.FromArgb(10, 17, 40);
                    scheme.PanelColor = Color.FromArgb(3, 7, 30);
                    scheme.CardColor = Color.FromArgb(30, 41, 82);
                    scheme.TextColor = Color.White;
                    scheme.SubtextColor = Color.FromArgb(137, 194, 217);
                    scheme.AccentColor = Color.FromArgb(0, 112, 204);
                    scheme.HoverColor = Color.FromArgb(0, 90, 163);
                    break;

                case "Xbox":
                    scheme.BackgroundColor = Color.FromArgb(18, 18, 18);
                    scheme.PanelColor = Color.FromArgb(10, 10, 10);
                    scheme.CardColor = Color.FromArgb(26, 26, 26);
                    scheme.TextColor = Color.White;
                    scheme.SubtextColor = Color.FromArgb(163, 163, 163);
                    scheme.AccentColor = Color.FromArgb(16, 124, 16);
                    scheme.HoverColor = Color.FromArgb(12, 94, 12);
                    break;

                case "Nintendo":
                    scheme.BackgroundColor = Color.FromArgb(245, 245, 245);
                    scheme.PanelColor = Color.FromArgb(230, 0, 18);
                    scheme.CardColor = Color.White;
                    scheme.TextColor = Color.FromArgb(30, 30, 30);
                    scheme.SubtextColor = Color.FromArgb(100, 100, 100);
                    scheme.AccentColor = Color.FromArgb(230, 0, 18);
                    scheme.HoverColor = Color.FromArgb(195, 0, 15);
                    break;

                default: // Dark Default
                    scheme.BackgroundColor = Color.FromArgb(24, 24, 28);
                    scheme.PanelColor = Color.FromArgb(19, 19, 22);
                    scheme.CardColor = Color.FromArgb(31, 31, 35);
                    scheme.TextColor = Color.White;
                    scheme.SubtextColor = Color.FromArgb(156, 163, 175);
                    scheme.AccentColor = ColorTranslator.FromHtml(_settings.AccentColorHtml);
                    scheme.HoverColor = Color.FromArgb(79, 70, 229);
                    break;
            }

            return scheme;
        }
    }
}
