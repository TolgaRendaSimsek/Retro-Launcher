using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RetroLauncher
{
    public partial class BiosManagerForm : Form
    {
        private readonly string[] _consoles = new[]
        {
            "Sony PlayStation 1",
            "Sony PlayStation 2",
            "Sony PlayStation 3"
        };

        public BiosManagerForm()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Load += BiosManagerForm_Load;
            lbConsoles.SelectedIndexChanged += lbConsoles_SelectedIndexChanged;

            btnLocate.Click += btnLocate_Click;
            btnImport.Click += btnImport_Click;
            btnDownload.Click += btnDownload_Click;
            btnOpenFolder.Click += btnOpenFolder_Click;
            btnClose.Click += btnClose_Click;

            // Hover effects
            SetupHover(btnLocate, Color.FromArgb(44, 44, 52), Color.FromArgb(55, 55, 65));
            SetupHover(btnImport, Color.FromArgb(44, 44, 52), Color.FromArgb(55, 55, 65));
            SetupHover(btnDownload, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupHover(btnOpenFolder, Color.FromArgb(44, 44, 52), Color.FromArgb(55, 55, 65));
            SetupHover(btnClose, Color.FromArgb(239, 68, 68), Color.FromArgb(220, 38, 38));
        }

        private void SetupHover(Button btn, Color baseColor, Color hoverColor)
        {
            btn.BackColor = baseColor;
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = baseColor;
        }

        private void BiosManagerForm_Load(object? sender, EventArgs e)
        {
            ThemeManager.Instance.ThemeChanged += (s, ev) => ThemeManager.Instance.ApplyTheme(this);
            ThemeManager.Instance.ApplyTheme(this);
            LocalizationManager.Instance.LanguageChanged += (s, ev) => LocalizationManager.Instance.ApplyLanguage(this);
            LocalizationManager.Instance.ApplyLanguage(this);

            RefreshList();
        }

        private void RefreshList()
        {
            lbConsoles.Items.Clear();
            foreach (var console in _consoles)
            {
                lbConsoles.Items.Add(console);
            }

            if (lbConsoles.Items.Count > 0)
            {
                lbConsoles.SelectedIndex = 0;
            }
        }

        private void lbConsoles_SelectedIndexChanged(object? sender, EventArgs e)
        {
            string? console = lbConsoles.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(console))
            {
                lblStatusVal.Text = "";
                tbPath.Text = "";
                ToggleDetailsControls(false);
                return;
            }

            ToggleDetailsControls(true);

            // Fetch state from manager
            var item = BiosManager.Instance.BiosItems.FirstOrDefault(b => string.Equals(b.Console, console, StringComparison.OrdinalIgnoreCase));
            if (item != null)
            {
                if (item.Status == "Ready")
                {
                    lblStatusVal.Text = "READY";
                    lblStatusVal.ForeColor = Color.FromArgb(16, 185, 129); // Green
                    tbPath.Text = item.Path;
                }
                else
                {
                    lblStatusVal.Text = "MISSING";
                    lblStatusVal.ForeColor = Color.FromArgb(239, 68, 68); // Red
                    tbPath.Text = "";
                }
            }
        }

        private void btnLocate_Click(object? sender, EventArgs e)
        {
            string? console = lbConsoles.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(console)) return;

            using (var fd = new OpenFileDialog())
            {
                fd.Title = $"Locate BIOS for {console}";
                fd.Filter = GetFileDialogFilter(console);

                if (fd.ShowDialog(this) == DialogResult.OK)
                {
                    bool result = BiosManager.Instance.LocateBiosManually(console, fd.FileName);
                    if (result)
                    {
                        lbConsoles_SelectedIndexChanged(null, EventArgs.Empty);
                        MessageBox.Show($"BIOS file path mapped successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void btnImport_Click(object? sender, EventArgs e)
        {
            string? console = lbConsoles.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(console)) return;

            using (var fd = new OpenFileDialog())
            {
                fd.Title = $"Import BIOS File for {console}";
                fd.Filter = GetFileDialogFilter(console);

                if (fd.ShowDialog(this) == DialogResult.OK)
                {
                    bool result = BiosManager.Instance.ImportBiosFile(console, fd.FileName);
                    if (result)
                    {
                        lbConsoles_SelectedIndexChanged(null, EventArgs.Empty);
                        MessageBox.Show($"BIOS file imported successfully into emulator folder!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to copy or import the BIOS file.", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void btnDownload_Click(object? sender, EventArgs e)
        {
            string? console = lbConsoles.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(console)) return;

            // Legal Warning
            string warningMsg = "LEGAL WARNING:\n\nDownloading emulator BIOS/firmware files is only permitted if you legally own the console hardware and/or own the original system files.\n\nBy continuing, you agree that you are downloading legally authorized files for personal backup use only.\n\nDo you wish to proceed with the download?";
            var confirm = MessageBox.Show(warningMsg, "Legal Compliance Check", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm != DialogResult.Yes)
            {
                return;
            }

            string apiEndpoint = "http://localhost:5000/api/bios/latest";

            // Disable controls
            ToggleDetailsControls(false);
            lbConsoles.Enabled = false;
            btnClose.Enabled = false;

            pbProgress.Value = 0;
            pbProgress.Visible = true;
            lblDownloadStatus.Text = "Connecting to API for BIOS info...";

            try
            {
                bool result = await BiosManager.Instance.DownloadBiosFromApiAsync(console, apiEndpoint, (progress) =>
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        pbProgress.Value = Math.Min(100, Math.Max(0, progress));
                        lblDownloadStatus.Text = $"Downloading package... {progress}%";
                    }));
                });

                if (result)
                {
                    lblDownloadStatus.Text = "BIOS downloaded and installed!";
                    MessageBox.Show($"Successfully downloaded and registered BIOS/firmware for {console}!", "Download Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    lbConsoles_SelectedIndexChanged(null, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                lblDownloadStatus.Text = "Download failed.";
                MessageBox.Show($"Failed to download BIOS from API:\n{ex.Message}", "API Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                ToggleDetailsControls(true);
                lbConsoles.Enabled = true;
                btnClose.Enabled = true;
                pbProgress.Visible = false;
            }
        }

        private void btnOpenFolder_Click(object? sender, EventArgs e)
        {
            string? console = lbConsoles.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(console)) return;

            string folderName = console switch
            {
                "Sony PlayStation 1" => "Emulators/PS1/bios",
                "Sony PlayStation 2" => "Emulators/PS2/bios",
                "Sony PlayStation 3" => "Emulators/PS3/dev_flash",
                _ => "Emulators/Common/bios"
            };

            string resolved = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, folderName);
            if (!Directory.Exists(resolved))
            {
                try { Directory.CreateDirectory(resolved); } catch { }
            }

            try
            {
                Process.Start("explorer.exe", resolved);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open folder:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object? sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void ToggleDetailsControls(bool enabled)
        {
            btnLocate.Enabled = enabled;
            btnImport.Enabled = enabled;
            btnDownload.Enabled = enabled;
            btnOpenFolder.Enabled = enabled;
        }

        private string GetFileDialogFilter(string console)
        {
            return console switch
            {
                "Sony PlayStation 3" => "PS3 Firmware Update (*.pup)|*.pup|All files (*.*)|*.*",
                _ => "BIOS files (*.bin;*.rom;*.img)|*.bin;*.rom;*.img|All files (*.*)|*.*"
            };
        }
    }
}
