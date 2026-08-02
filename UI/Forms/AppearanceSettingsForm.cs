using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace RetroLauncher.UI.Forms
{
    public partial class AppearanceSettingsForm : Form
    {
        private Color _selectedAccent;
        private string? _selectedBgPath;

        public AppearanceSettingsForm()
        {
            InitializeComponent();
            SetupCustomEvents();
        }

        private void SetupCustomEvents()
        {
            this.Load += AppearanceSettingsForm_Load;
            
            btnPickAccent.Click += btnPickAccent_Click;
            btnBrowseBg.Click += btnBrowseBg_Click;
            btnClearBg.Click += btnClearBg_Click;
            
            btnApply.Click += btnApply_Click;
            btnReset.Click += btnReset_Click;
            btnClose.Click += (s, e) => this.Close();

            // Bind theme changing notification to repaint this settings window immediately
            ThemeManager.Instance.ThemeChanged += ThemeManager_ThemeChanged;

            // Setup hover styles
            SetupButtonHover(btnPickAccent, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupButtonHover(btnBrowseBg, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupButtonHover(btnClearBg, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupButtonHover(btnApply, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupButtonHover(btnReset, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupButtonHover(btnClose, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
        }

        private void SetupButtonHover(Button btn, Color normal, Color hover)
        {
            btn.BackColor = normal;
            btn.MouseEnter += (s, e) => btn.BackColor = hover;
            btn.MouseLeave += (s, e) => btn.BackColor = normal;
        }

        private void AppearanceSettingsForm_Load(object? sender, EventArgs e)
        {
            PopulateControls();
            LoadCurrentSettings();
            ThemeManager.Instance.ApplyTheme(this);
        }

        private void ThemeManager_ThemeChanged(object? sender, EventArgs e)
        {
            ThemeManager.Instance.ApplyTheme(this);
            pnlAccentPreview.BackColor = _selectedAccent;
        }

        private void PopulateControls()
        {
            // Themes
            cbTheme.Items.Clear();
            cbTheme.Items.AddRange(new[] { "Dark", "OLED", "Light", "Retro", "PlayStation", "Xbox", "Nintendo" });
            cbTheme.SelectedIndex = 0;

            // Font Sizes
            cbFontSize.Items.Clear();
            cbFontSize.Items.AddRange(new[] { "Small", "Medium", "Large" });
            cbFontSize.SelectedIndex = 1; // Medium
        }

        private void LoadCurrentSettings()
        {
            var s = ThemeManager.Instance.Settings;

            // Theme name
            int themeIdx = cbTheme.FindStringExact(s.ActiveTheme);
            cbTheme.SelectedIndex = themeIdx >= 0 ? themeIdx : 0;

            // Font size
            int fontIdx = cbFontSize.FindStringExact(s.FontSizeName);
            cbFontSize.SelectedIndex = fontIdx >= 0 ? fontIdx : 1;

            // Accent preview
            try
            {
                _selectedAccent = ColorTranslator.FromHtml(s.AccentColorHtml);
            }
            catch
            {
                _selectedAccent = Color.FromArgb(99, 102, 241);
            }
            pnlAccentPreview.BackColor = _selectedAccent;

            // Background path
            _selectedBgPath = s.BackgroundImagePath;
            tbBackgroundPath.Text = string.IsNullOrEmpty(_selectedBgPath) ? "None" : _selectedBgPath;
        }

        private void btnPickAccent_Click(object? sender, EventArgs e)
        {
            using (var cd = new ColorDialog())
            {
                cd.Color = _selectedAccent;
                if (cd.ShowDialog(this) == DialogResult.OK)
                {
                    _selectedAccent = cd.Color;
                    pnlAccentPreview.BackColor = _selectedAccent;
                }
            }
        }

        private void btnBrowseBg_Click(object? sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";
                ofd.Title = "Select Custom Background Image";

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    _selectedBgPath = ofd.FileName;
                    tbBackgroundPath.Text = _selectedBgPath;
                }
            }
        }

        private void btnClearBg_Click(object? sender, EventArgs e)
        {
            _selectedBgPath = null;
            tbBackgroundPath.Text = "None";
        }

        private void btnApply_Click(object? sender, EventArgs e)
        {
            var s = ThemeManager.Instance.Settings;
            s.ActiveTheme = cbTheme.SelectedItem?.ToString() ?? "Dark";
            s.FontSizeName = cbFontSize.SelectedItem?.ToString() ?? "Medium";
            s.AccentColorHtml = ColorTranslator.ToHtml(_selectedAccent);
            s.BackgroundImagePath = _selectedBgPath;

            ThemeManager.Instance.SaveThemeSettings();
            ThemeManager.Instance.OnThemeChanged();

            MessageBox.Show("Theme settings saved and applied globally!", "Theme Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnReset_Click(object? sender, EventArgs e)
        {
            var confirm = MessageBox.Show("Are you sure you want to reset appearance settings to default values?", "Reset Theme", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            ThemeManager.Instance.ResetToDefaultTheme();
            LoadCurrentSettings();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            ThemeManager.Instance.ThemeChanged -= ThemeManager_ThemeChanged;
        }
    }
}
