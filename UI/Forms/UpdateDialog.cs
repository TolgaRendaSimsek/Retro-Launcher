using System;
using System.IO;
using System.Text.Json;
using System.Windows.Forms;
using RetroLauncher.Core.Utilities;
using RetroLauncher.Services.Updates;

namespace RetroLauncher.UI.Forms
{
    public partial class UpdateDialog : Form
    {
        private readonly ApplicationUpdateCheckResult _checkResult;

        public UpdateDialog(ApplicationUpdateCheckResult checkResult)
        {
            InitializeComponent();
            _checkResult = checkResult ?? throw new ArgumentNullException(nameof(checkResult));

            SetupDialogData();
            SetupEvents();
        }

        private void SetupDialogData()
        {
            string currentVer = _checkResult.CurrentVersion?.ToString() ?? ApplicationVersionProvider.Instance.SemanticVersionString;
            string latestVer = _checkResult.LatestVersion?.ToString() ?? _checkResult.ReleaseTag ?? "Unknown";

            lblVersionDetails.Text = $"Current: {currentVer}  ➔  Latest: {latestVer}";

            string notes = _checkResult.ReleaseNotes;
            rtbChangelog.Text = string.IsNullOrWhiteSpace(notes)
                ? "No release notes were provided."
                : notes.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        }

        private void SetupEvents()
        {
            btnUpdateNow.Click += btnUpdateNow_Click;
            btnSkipVersion.Click += btnSkipVersion_Click;
            btnRemindLater.Click += (s, e) => this.Close();
        }

        private async void btnUpdateNow_Click(object? sender, EventArgs e)
        {
            this.Close();
            try
            {
                await ApplicationUpdateService.Instance.DownloadUpdateAsync();

                var result = MessageBox.Show(
                    this.Owner,
                    "Update downloaded successfully. Retro Launcher will now restart to complete installation.",
                    "Update Ready",
                    MessageBoxButtons.OKCancel,
                    MessageBoxIcon.Information
                );

                if (result == DialogResult.OK)
                {
                    await ApplicationUpdateService.Instance.PrepareAndInstallUpdateAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.Owner, $"Failed to apply update: {ex.Message}", "Update Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSkipVersion_Click(object? sender, EventArgs e)
        {
            this.Close();
        }
    }
}
