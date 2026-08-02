using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RetroLauncher.UI.Forms
{
    public partial class LanguageSettingsForm : Form
    {
        private readonly List<LangComboItem> _languages = new()
        {
            new LangComboItem { Code = "en", Name = "English (en)" },
            new LangComboItem { Code = "tr", Name = "Türkçe (tr)" },
            new LangComboItem { Code = "de", Name = "Deutsch (de)" },
            new LangComboItem { Code = "fr", Name = "Français (fr)" },
            new LangComboItem { Code = "es", Name = "Español (es)" },
            new LangComboItem { Code = "ja", Name = "日本語 (ja)" }
        };

        public LanguageSettingsForm()
        {
            InitializeComponent();
            SetupCustomEvents();
        }

        private void SetupCustomEvents()
        {
            this.Load += LanguageSettingsForm_Load;
            btnApply.Click += btnApply_Click;
            btnClose.Click += (s, e) => this.Close();

            // Bind events for real-time repaints
            ThemeManager.Instance.ThemeChanged += ThemeManager_ThemeChanged;
            LocalizationManager.Instance.LanguageChanged += LocalizationManager_LanguageChanged;

            // Setup hover styles
            SetupButtonHover(btnApply, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupButtonHover(btnClose, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
        }

        private void SetupButtonHover(Button btn, Color normal, Color hover)
        {
            btn.BackColor = normal;
            btn.MouseEnter += (s, e) => btn.BackColor = hover;
            btn.MouseLeave += (s, e) => btn.BackColor = normal;
        }

        private void LanguageSettingsForm_Load(object? sender, EventArgs e)
        {
            PopulateLanguages();
            ThemeManager.Instance.ApplyTheme(this);
            LocalizationManager.Instance.ApplyLanguage(this);
        }

        private void ThemeManager_ThemeChanged(object? sender, EventArgs e)
        {
            ThemeManager.Instance.ApplyTheme(this);
        }

        private void LocalizationManager_LanguageChanged(object? sender, EventArgs e)
        {
            LocalizationManager.Instance.ApplyLanguage(this);
        }

        private void PopulateLanguages()
        {
            cbLanguage.Items.Clear();
            foreach (var lang in _languages)
            {
                cbLanguage.Items.Add(lang);
            }

            string currentLang = LocalizationManager.Instance.CurrentLanguage;
            int selectIdx = _languages.FindIndex(l => l.Code == currentLang);
            cbLanguage.SelectedIndex = selectIdx >= 0 ? selectIdx : 0;
        }

        private void btnApply_Click(object? sender, EventArgs e)
        {
            var selectedItem = cbLanguage.SelectedItem as LangComboItem;
            if (selectedItem != null)
            {
                LocalizationManager.Instance.LoadLanguage(selectedItem.Code);
                MessageBox.Show(
                    LocalizationManager.Instance.GetText("language_applied_msg") ?? "Language updated successfully!",
                    LocalizationManager.Instance.GetText("language_applied_title") ?? "Language Settings",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            ThemeManager.Instance.ThemeChanged -= ThemeManager_ThemeChanged;
            LocalizationManager.Instance.LanguageChanged -= LocalizationManager_LanguageChanged;
        }

        private class LangComboItem
        {
            public string Code { get; set; } = "en";
            public string Name { get; set; } = "";

            public override string ToString()
            {
                return Name;
            }
        }
    }
}
