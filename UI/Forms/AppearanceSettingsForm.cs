using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using RetroLauncher.Core.Utilities;
using RetroLauncher.Services.Updates;
using RetroLauncher.UI.Controls;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Forms
{
    public partial class AppearanceSettingsForm : Form
    {
        private Color _selectedAccent;
        private string? _selectedBgPath;

        // Updates UI Controls
        private Label _lblAppVersion = null!;
        private Label _lblRemoteVersion = null!;
        private Label _lblUpdateStatus = null!;
        private TextBox _txtReleaseNotes = null!;
        private ModernButton _btnCheckAppUpdates = null!;
        private ModernButton _btnDownloadAppUpdate = null!;
        private ModernButton _btnInstallAppUpdate = null!;
        private ModernButton _btnOpenUpdateLogs = null!;
        private CheckBox _chkAutoCheckUpdates = null!;

        public AppearanceSettingsForm()
        {
            InitializeComponent();
            SetupUpdatesSection();
            SetupCustomEvents();
        }

        private void SetupUpdatesSection()
        {
            this.Size = new Size(560, 620);
            this.AutoScroll = true;

            GroupBox gbUpdates = new GroupBox
            {
                Text = "🚀 Application Updates (Retro Launcher)",
                Location = new Point(15, 360),
                Size = new Size(480, 210),
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Font = AppTheme.Current.Fonts.BodySmall
            };

            _lblAppVersion = new Label
            {
                Text = $"Current Installed Version: {ApplicationVersionProvider.Instance.SemanticVersionString}",
                Location = new Point(15, 25),
                AutoSize = true,
                ForeColor = AppTheme.Current.Colors.TextPrimary
            };
            gbUpdates.Controls.Add(_lblAppVersion);

            _lblRemoteVersion = new Label
            {
                Text = "Latest Remote Release: —",
                Location = new Point(15, 45),
                AutoSize = true,
                ForeColor = AppTheme.Current.Colors.TextSecondary
            };
            gbUpdates.Controls.Add(_lblRemoteVersion);

            _lblUpdateStatus = new Label
            {
                Text = "Status: Not checked",
                Location = new Point(15, 65),
                AutoSize = true,
                ForeColor = AppTheme.Current.Colors.TextSecondary
            };
            gbUpdates.Controls.Add(_lblUpdateStatus);

            _btnCheckAppUpdates = new ModernButton
            {
                Text = "🔄 Check for Updates",
                Location = new Point(15, 90),
                Size = new Size(140, 32),
                IsPrimary = false
            };
            _btnCheckAppUpdates.Click += btnCheckAppUpdates_Click;
            gbUpdates.Controls.Add(_btnCheckAppUpdates);

            _btnDownloadAppUpdate = new ModernButton
            {
                Text = "⬇️ Download Update",
                Location = new Point(165, 90),
                Size = new Size(140, 32),
                IsPrimary = true,
                Enabled = false
            };
            _btnDownloadAppUpdate.Click += btnDownloadAppUpdate_Click;
            gbUpdates.Controls.Add(_btnDownloadAppUpdate);

            _btnInstallAppUpdate = new ModernButton
            {
                Text = "⚡ Install & Restart",
                Location = new Point(315, 90),
                Size = new Size(140, 32),
                IsPrimary = true,
                Enabled = false
            };
            _btnInstallAppUpdate.Click += btnInstallAppUpdate_Click;
            gbUpdates.Controls.Add(_btnInstallAppUpdate);

            _txtReleaseNotes = new TextBox
            {
                Location = new Point(15, 130),
                Size = new Size(330, 68),
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = AppTheme.Current.Colors.Surface,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Text = "No release notes available."
            };
            gbUpdates.Controls.Add(_txtReleaseNotes);

            _btnOpenUpdateLogs = new ModernButton
            {
                Text = "📄 Logs",
                Location = new Point(355, 130),
                Size = new Size(100, 30),
                IsPrimary = false
            };
            _btnOpenUpdateLogs.Click += (s, e) => ViewUpdateLogs();
            gbUpdates.Controls.Add(_btnOpenUpdateLogs);

            this.Controls.Add(gbUpdates);
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

            ThemeManager.Instance.ThemeChanged += ThemeManager_ThemeChanged;
        }

        private void AppearanceSettingsForm_Load(object? sender, EventArgs e)
        {
            PopulateControls();
            LoadCurrentSettings();
            ThemeManager.Instance.ApplyTheme(this);
            RefreshUpdateStatusUI();
        }

        private void ThemeManager_ThemeChanged(object? sender, EventArgs e)
        {
            ThemeManager.Instance.ApplyTheme(this);
            pnlAccentPreview.BackColor = _selectedAccent;
        }

        private void PopulateControls()
        {
            cbTheme.Items.Clear();
            cbTheme.Items.AddRange(new[] { "Dark", "OLED", "Light", "Retro", "PlayStation", "Xbox", "Nintendo" });
            cbTheme.SelectedIndex = 0;

            cbFontSize.Items.Clear();
            cbFontSize.Items.AddRange(new[] { "Small", "Medium", "Large" });
            cbFontSize.SelectedIndex = 1;
        }

        private void LoadCurrentSettings()
        {
            var s = ThemeManager.Instance.Settings;

            int themeIdx = cbTheme.FindStringExact(s.ActiveTheme);
            cbTheme.SelectedIndex = themeIdx >= 0 ? themeIdx : 0;

            int fontIdx = cbFontSize.FindStringExact(s.FontSizeName);
            cbFontSize.SelectedIndex = fontIdx >= 0 ? fontIdx : 1;

            try
            {
                _selectedAccent = ColorTranslator.FromHtml(s.AccentColorHtml);
            }
            catch
            {
                _selectedAccent = Color.FromArgb(99, 102, 241);
            }
            pnlAccentPreview.BackColor = _selectedAccent;

            _selectedBgPath = s.BackgroundImagePath;
            tbBackgroundPath.Text = string.IsNullOrEmpty(_selectedBgPath) ? "None" : _selectedBgPath;
        }

        private void RefreshUpdateStatusUI()
        {
            var service = ApplicationUpdateService.Instance;
            _lblUpdateStatus.Text = $"Status: {service.Status}";

            if (service.LastCheckResult != null)
            {
                _lblRemoteVersion.Text = $"Latest Remote Release: {service.LastCheckResult.ReleaseTag ?? "—"}";
                if (!string.IsNullOrWhiteSpace(service.LastCheckResult.ReleaseNotes))
                {
                    _txtReleaseNotes.Text = service.LastCheckResult.ReleaseNotes;
                }
            }

            _btnDownloadAppUpdate.Enabled = (service.Status == ApplicationUpdateStatus.UpdateAvailable);
            _btnInstallAppUpdate.Enabled = (service.Status == ApplicationUpdateStatus.ReadyToInstall);
        }

        private async void btnCheckAppUpdates_Click(object? sender, EventArgs e)
        {
            _btnCheckAppUpdates.Enabled = false;
            _lblUpdateStatus.Text = "Status: Checking GitHub Releases...";
            try
            {
                var result = await ApplicationUpdateService.Instance.CheckForUpdatesAsync(forceRefresh: true);
                RefreshUpdateStatusUI();

                if (!result.CheckSucceeded)
                {
                    ToastNotification.ShowToast(this, $"Update check failed: {result.ErrorMessage}", StatusType.Error);
                }
                else if (result.UpdateAvailable)
                {
                    ToastNotification.ShowToast(this, $"New release available: {result.ReleaseTag}!", StatusType.Success);
                }
                else
                {
                    ToastNotification.ShowToast(this, "Application is up to date.", StatusType.Info);
                }
            }
            finally
            {
                _btnCheckAppUpdates.Enabled = true;
            }
        }

        private async void btnDownloadAppUpdate_Click(object? sender, EventArgs e)
        {
            _btnDownloadAppUpdate.Enabled = false;
            _lblUpdateStatus.Text = "Status: Downloading update package...";
            try
            {
                var progress = new Progress<int>(pct => _lblUpdateStatus.Text = $"Status: Downloading update... ({pct}%)");
                await ApplicationUpdateService.Instance.DownloadUpdateAsync(progress);
                RefreshUpdateStatusUI();
                ToastNotification.ShowToast(this, "Download complete! Click Install & Restart.", StatusType.Success);
            }
            catch (Exception ex)
            {
                _lblUpdateStatus.Text = $"Status: Download failed ({ex.Message})";
                ToastNotification.ShowToast(this, $"Download failed: {ex.Message}", StatusType.Error);
            }
            finally
            {
                RefreshUpdateStatusUI();
            }
        }

        private async void btnInstallAppUpdate_Click(object? sender, EventArgs e)
        {
            var confirm = MessageBox.Show(this, "The application will restart to apply the update. Continue?", "Install Update", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            try
            {
                await ApplicationUpdateService.Instance.PrepareAndInstallUpdateAsync();
            }
            catch (Exception ex)
            {
                ToastNotification.ShowToast(this, $"Update installation failed: {ex.Message}", StatusType.Error);
            }
        }

        private void ViewUpdateLogs()
        {
            string logPath = Path.Combine(ApplicationPaths.LogsDir, "ApplicationUpdates", "app_updates.log");
            if (File.Exists(logPath))
            {
                try
                {
                    Process.Start("notepad.exe", $"\"{logPath}\"");
                }
                catch (Exception ex)
                {
                    ToastNotification.ShowToast(this, $"Could not open log file: {ex.Message}", StatusType.Error);
                }
            }
            else
            {
                ToastNotification.ShowToast(this, "No update logs found yet.", StatusType.Info);
            }
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

            ToastNotification.ShowToast(this, "Theme settings saved globally!", StatusType.Success);
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
