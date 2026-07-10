using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;

namespace RetroLauncher
{
    public partial class UpdateDialog : Form
    {
        private readonly UpdateInfo _updateInfo;
        private readonly UpdaterSettings _settings;
        private readonly string _settingsPath;

        public UpdateDialog(UpdateInfo updateInfo, UpdaterSettings settings, string settingsPath)
        {
            InitializeComponent();
            _updateInfo = updateInfo;
            _settings = settings;
            _settingsPath = settingsPath;

            SetupDialogData();
            SetupEvents();
        }

        private void SetupDialogData()
        {
            lblVersionDetails.Text = $"Current: {UpdateManager.CurrentVersion}  ➔  Latest: {_updateInfo.Version}";
            rtbChangelog.Text = string.IsNullOrEmpty(_updateInfo.Changelog) 
                ? "No release notes available for this release." 
                : _updateInfo.Changelog.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        }

        private void SetupEvents()
        {
            btnUpdateNow.Click += btnUpdateNow_Click;
            btnSkipVersion.Click += btnSkipVersion_Click;
            btnRemindLater.Click += (s, e) => this.Close();
        }

        private void btnUpdateNow_Click(object? sender, EventArgs e)
        {
            this.Close();
            // Start download and progress screen
            using (var progressForm = new UpdateProgressForm(_updateInfo))
            {
                progressForm.ShowDialog(this.Owner);
            }
        }

        private void btnSkipVersion_Click(object? sender, EventArgs e)
        {
            try
            {
                _settings.SkippedVersion = _updateInfo.Version;
                string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);
                
                MessageBox.Show($"Version {_updateInfo.Version} will be skipped until the next release is published.", 
                    "Update Skipped", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to write skipped version: {ex.Message}");
            }
            
            this.Close();
        }
    }
}
