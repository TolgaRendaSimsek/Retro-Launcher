using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RetroLauncher.UI.Forms
{
    public partial class BiosManagerForm : Form
    {
        private readonly string[] _consoles = new[]
        {
            "Sony PlayStation 1",
            "Sony PlayStation 2",
            "Sony PlayStation 3",
            "Sony PlayStation Portable",
            "Nintendo GameCube",
            "Nintendo Wii",
            "RetroArch"
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

            btnImport.Click += btnImport_Click;
            btnOpenFolder.Click += btnOpenFolder_Click;
            btnVerify.Click += btnVerify_Click;
            btnRemove.Click += btnRemove_Click;
            btnSync.Click += btnSync_Click;
            btnClose.Click += btnClose_Click;

            // Hover effects
            SetupHover(btnImport, Color.FromArgb(44, 44, 52), Color.FromArgb(55, 55, 65));
            SetupHover(btnOpenFolder, Color.FromArgb(44, 44, 52), Color.FromArgb(55, 55, 65));
            SetupHover(btnVerify, Color.FromArgb(44, 44, 52), Color.FromArgb(55, 55, 65));
            SetupHover(btnRemove, Color.FromArgb(44, 44, 52), Color.FromArgb(55, 55, 65));
            SetupHover(btnSync, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
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
                    lblStatusVal.Text = $"READY ({item.FileName})";
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
                        MessageBox.Show($"BIOS file imported successfully into centralized BIOS folder!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Failed to copy or import the BIOS file.", "Import Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnVerify_Click(object? sender, EventArgs e)
        {
            string? console = lbConsoles.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(console)) return;

            BiosManager.Instance.DetectBiosStatus();
            lbConsoles_SelectedIndexChanged(null, EventArgs.Empty);

            var item = BiosManager.Instance.BiosItems.FirstOrDefault(b => string.Equals(b.Console, console, StringComparison.OrdinalIgnoreCase));
            if (item != null && item.Status == "Ready")
            {
                MessageBox.Show($"Verification successful! BIOS is ready in centralized directory.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show($"BIOS file not detected in centralized folder.", "Verification Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnRemove_Click(object? sender, EventArgs e)
        {
            string? console = lbConsoles.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(console)) return;

            var confirm = MessageBox.Show($"Are you sure you want to delete the BIOS files for {console} from the centralized folder?", "Confirm Deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
            {
                bool result = BiosManager.Instance.RemoveBiosFile(console);
                if (result)
                {
                    lbConsoles_SelectedIndexChanged(null, EventArgs.Empty);
                    MessageBox.Show("BIOS files removed successfully from centralized folder.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to remove BIOS files.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnSync_Click(object? sender, EventArgs e)
        {
            string? console = lbConsoles.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(console)) return;

            var item = BiosManager.Instance.BiosItems.FirstOrDefault(b => string.Equals(b.Console, console, StringComparison.OrdinalIgnoreCase));
            if (item == null || item.Status != "Ready")
            {
                MessageBox.Show("BIOS must be imported and verified before syncing.", "Sync Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool result = BiosManager.Instance.SyncBiosToEmulator(item);
            if (result)
            {
                MessageBox.Show($"BIOS files successfully synchronized to the expected emulator folder!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Failed to synchronize BIOS files to the emulator expected folder.", "Sync Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnOpenFolder_Click(object? sender, EventArgs e)
        {
            string? console = lbConsoles.SelectedItem?.ToString();
            if (string.IsNullOrEmpty(console)) return;

            string folderName = BiosManager.Instance.GetDefaultFolderForConsole(console);
            string resolved = Path.Combine(AppContext.BaseDirectory, folderName);

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
            btnImport.Enabled = enabled;
            btnOpenFolder.Enabled = enabled;
            btnVerify.Enabled = enabled;
            btnRemove.Enabled = enabled;
            btnSync.Enabled = enabled;
        }

        private string GetFileDialogFilter(string console)
        {
            return console switch
            {
                "Sony PlayStation 3" => "PS3 Firmware Update (*.pup)|*.pup|All files (*.*)|*.*",
                "Sony PlayStation Portable" => "PSP System Files (*.prx;*.bin)|*.prx;*.bin|All files (*.*)|*.*",
                _ => "BIOS files (*.bin;*.rom;*.img)|*.bin;*.rom;*.img|All files (*.*)|*.*"
            };
        }
    }
}
