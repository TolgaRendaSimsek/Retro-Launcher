using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RetroLauncher
{
    public partial class EmulatorManagerForm : Form
    {
        private EmulatorConfig _config = new();
        private bool _isUpdatingSelection = false;

        private readonly string[] _consoles = new[]
        {
            "Sony PlayStation 1",
            "Sony PlayStation 2",
            "Sony PlayStation 3",
            "Nintendo Entertainment System (NES)",
            "Super Nintendo (SNES)",
            "Nintendo 64",
            "Sega Genesis",
            "Game Boy Advance"
        };

        public EmulatorManagerForm()
        {
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Load += EmulatorManagerForm_Load;
            lbEmulators.SelectedIndexChanged += lbEmulators_SelectedIndexChanged;

            // Edit fields event
            tbName.TextChanged += (s, e) => SaveSelectedFields();
            tbPath.TextChanged += (s, e) => {
                UpdateDetectedVersion();
                SaveSelectedFields();
            };
            cbDefaultConsole.SelectedIndexChanged += cbDefaultConsole_SelectedIndexChanged;

            // Buttons
            btnBrowse.Click += btnBrowse_Click;
            btnAdd.Click += btnAdd_Click;
            btnRemove.Click += btnRemove_Click;
            btnTestLaunch.Click += btnTestLaunch_Click;
            btnInstallDuckStationApi.Click += btnInstallDuckStationApi_Click;
            btnSaveClose.Click += btnSaveClose_Click;

            // Hover styles
            SetupHover(btnSaveClose, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105));
            SetupHover(btnTestLaunch, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupHover(btnInstallDuckStationApi, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupHover(btnAdd, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105));
            SetupHover(btnRemove, Color.FromArgb(239, 68, 68), Color.FromArgb(220, 38, 38));
        }

        private void SetupHover(Button btn, Color baseColor, Color hoverColor)
        {
            btn.BackColor = baseColor;
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = baseColor;
        }

        private void EmulatorManagerForm_Load(object? sender, EventArgs e)
        {
            ThemeManager.Instance.ThemeChanged += (s, ev) => ThemeManager.Instance.ApplyTheme(this);
            ThemeManager.Instance.ApplyTheme(this);
            LocalizationManager.Instance.LanguageChanged += (s, ev) => LocalizationManager.Instance.ApplyLanguage(this);
            LocalizationManager.Instance.ApplyLanguage(this);
            _config = EmulatorManager.LoadConfig();

            // Populate default console ComboBox
            cbDefaultConsole.Items.Clear();
            cbDefaultConsole.Items.Add("(None)");
            foreach (var console in _consoles)
            {
                cbDefaultConsole.Items.Add(console);
            }

            RefreshList();
        }

        private void RefreshList()
        {
            _isUpdatingSelection = true;
            
            lbEmulators.Items.Clear();
            foreach (var emu in _config.Emulators)
            {
                lbEmulators.Items.Add(emu);
            }

            _isUpdatingSelection = false;

            if (lbEmulators.Items.Count > 0)
            {
                lbEmulators.SelectedIndex = 0;
            }
            else
            {
                ClearDetails();
            }
        }

        private void lbEmulators_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingSelection) return;

            if (lbEmulators.SelectedItem is EmulatorItem selectedEmu)
            {
                DisplayDetails(selectedEmu);
            }
            else
            {
                ClearDetails();
            }
        }

        private void DisplayDetails(EmulatorItem emu)
        {
            _isUpdatingSelection = true;

            tbName.Text = emu.Name;
            tbPath.Text = emu.Path;
            
            // Resolve version
            UpdateDetectedVersion();

            // Check default console association
            string mappedConsole = "(None)";
            foreach (var pair in _config.DefaultEmulators)
            {
                if (pair.Value.Equals(emu.Path, StringComparison.OrdinalIgnoreCase))
                {
                    mappedConsole = pair.Key;
                    break;
                }
            }

            int index = cbDefaultConsole.Items.IndexOf(mappedConsole);
            cbDefaultConsole.SelectedIndex = index >= 0 ? index : 0;

            _isUpdatingSelection = false;
        }

        private void ClearDetails()
        {
            _isUpdatingSelection = true;
            tbName.Clear();
            tbPath.Clear();
            lblVersion.Text = "Not Detected";
            cbDefaultConsole.SelectedIndex = 0;
            _isUpdatingSelection = false;
        }

        private void UpdateDetectedVersion()
        {
            string resolved = ResolvePath(tbPath.Text.Trim());
            if (File.Exists(resolved))
            {
                try
                {
                    FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(resolved);
                    lblVersion.Text = !string.IsNullOrEmpty(versionInfo.FileVersion) 
                        ? versionInfo.FileVersion 
                        : "Detected (No version string available)";
                }
                catch
                {
                    lblVersion.Text = "Unable to read version";
                }
            }
            else
            {
                lblVersion.Text = "Executable not found";
            }
        }

        private void SaveSelectedFields()
        {
            if (_isUpdatingSelection) return;

            if (lbEmulators.SelectedItem is EmulatorItem selectedEmu)
            {
                string oldPath = selectedEmu.Path;
                selectedEmu.Name = tbName.Text.Trim();
                selectedEmu.Path = tbPath.Text.Trim();
                selectedEmu.Version = lblVersion.Text.StartsWith("Executable") || lblVersion.Text.StartsWith("Unable") 
                    ? "" 
                    : lblVersion.Text;

                // Sync path changes with DefaultEmulators dictionary
                if (!string.Equals(oldPath, selectedEmu.Path, StringComparison.OrdinalIgnoreCase))
                {
                    List<string> keysToUpdate = _config.DefaultEmulators
                        .Where(pair => string.Equals(pair.Value, oldPath, StringComparison.OrdinalIgnoreCase))
                        .Select(pair => pair.Key)
                        .ToList();

                    foreach (var key in keysToUpdate)
                    {
                        _config.DefaultEmulators[key] = selectedEmu.Path;
                    }
                }

                // Refresh displayed ListBox text
                _isUpdatingSelection = true;
                int idx = lbEmulators.SelectedIndex;
                lbEmulators.Items[idx] = selectedEmu;
                _isUpdatingSelection = false;

                // Save config immediately
                EmulatorManager.SaveConfig(_config);
            }
        }

        private void cbDefaultConsole_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingSelection) return;

            if (lbEmulators.SelectedItem is EmulatorItem selectedEmu)
            {
                string selectedConsole = cbDefaultConsole.SelectedItem?.ToString() ?? "(None)";

                // Remove this emulator path from any existing associations in default emulators dictionary
                List<string> keysToRemove = _config.DefaultEmulators
                    .Where(pair => string.Equals(pair.Value, selectedEmu.Path, StringComparison.OrdinalIgnoreCase))
                    .Select(pair => pair.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _config.DefaultEmulators.Remove(key);
                }

                // Associate with new console if not "(None)"
                if (selectedConsole != "(None)")
                {
                    _config.DefaultEmulators[selectedConsole] = selectedEmu.Path;
                }

                // Save config immediately
                EmulatorManager.SaveConfig(_config);
            }
        }

        private void btnBrowse_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Emulator Executable";
                ofd.Filter = "Executables (*.exe)|*.exe|All Files (*.*)|*.*";
                
                string current = ResolvePath(tbPath.Text);
                if (File.Exists(current))
                {
                    ofd.InitialDirectory = Path.GetDirectoryName(current);
                    ofd.FileName = Path.GetFileName(current);
                }
                else
                {
                    ofd.InitialDirectory = ResolvePath("Emulators");
                }

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    tbPath.Text = MakeRelativePath(ofd.FileName);
                }
            }
        }

        private void btnAdd_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Emulator to Register";
                ofd.Filter = "Executables (*.exe)|*.exe|All Files (*.*)|*.*";
                ofd.InitialDirectory = ResolvePath("Emulators");

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string path = MakeRelativePath(ofd.FileName);
                    string name = Path.GetFileNameWithoutExtension(ofd.FileName);

                    // Prevent duplicate paths
                    if (_config.Emulators.Any(emu => string.Equals(emu.Path, path, StringComparison.OrdinalIgnoreCase)))
                    {
                        MessageBox.Show("This emulator is already registered.", "Duplicate Emulator", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    var newEmu = new EmulatorItem
                    {
                        Name = name,
                        Path = path
                    };

                    _config.Emulators.Add(newEmu);
                    RefreshList();

                    // Select the new emulator
                    lbEmulators.SelectedItem = newEmu;
                    tbName.Focus();
                    tbName.SelectAll();
                }
            }
        }

        private void btnRemove_Click(object? sender, EventArgs e)
        {
            if (lbEmulators.SelectedItem is not EmulatorItem selectedEmu) return;

            var result = MessageBox.Show(
                $"Are you sure you want to remove '{selectedEmu.Name}'?\n\nThis will remove its default mappings from your console configurations.",
                "Confirm Removal",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                // Remove default mappings pointing to this emulator
                List<string> keysToRemove = _config.DefaultEmulators
                    .Where(pair => string.Equals(pair.Value, selectedEmu.Path, StringComparison.OrdinalIgnoreCase))
                    .Select(pair => pair.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _config.DefaultEmulators.Remove(key);
                }

                _config.Emulators.Remove(selectedEmu);
                RefreshList();
            }
        }

        private void btnTestLaunch_Click(object? sender, EventArgs e)
        {
            if (lbEmulators.SelectedItem is not EmulatorItem selectedEmu) return;

            string resolvedPath = ResolvePath(selectedEmu.Path);
            if (!File.Exists(resolvedPath))
            {
                MessageBox.Show($"Emulator file not found at:\n'{selectedEmu.Path}'\n\nPlease select a valid path.", "Test Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = resolvedPath,
                    UseShellExecute = true
                };
                
                Process? proc = Process.Start(psi);
                if (proc != null)
                {
                    MessageBox.Show($"Successfully started '{selectedEmu.Name}' process!\n\nYou can close the emulator now.", "Test Launch Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Failed to attach test launch process for '{selectedEmu.Name}'.", "Test Launch Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to test launch emulator:\n{ex.Message}", "Launch Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSaveClose_Click(object? sender, EventArgs e)
        {
            EmulatorManager.SaveConfig(_config);
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path)) return path;

            string baseDir = AppContext.BaseDirectory;
            string testPath1 = Path.Combine(baseDir, path);
            if (File.Exists(testPath1) || Directory.Exists(testPath1)) return testPath1;

            string testPath2 = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (File.Exists(testPath2) || Directory.Exists(testPath2)) return testPath2;

            return testPath1;
        }

        private string MakeRelativePath(string fullPath)
        {
            if (string.IsNullOrEmpty(fullPath)) return "";

            string baseDir = AppContext.BaseDirectory;
            string workingDir = Directory.GetCurrentDirectory();

            if (fullPath.StartsWith(baseDir, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(baseDir.Length).TrimStart(Path.DirectorySeparatorChar);
            }
            if (fullPath.StartsWith(workingDir, StringComparison.OrdinalIgnoreCase))
            {
                return fullPath.Substring(workingDir.Length).TrimStart(Path.DirectorySeparatorChar);
            }

            return fullPath;
        }

        private async void btnInstallDuckStationApi_Click(object? sender, EventArgs e)
        {
            string apiEndpoint = "http://localhost:5000/api/duckstation/latest";

            btnInstallDuckStationApi.Enabled = false;
            btnSaveClose.Enabled = false;
            btnAdd.Enabled = false;
            btnRemove.Enabled = false;
            btnTestLaunch.Enabled = false;

            pbProgress.Value = 0;
            pbProgress.Visible = true;
            lblStatus.Text = "Querying API for package info...";

            try
            {
                var emuManager = EmulatorManager.Instance;
                
                bool result = await emuManager.InstallDuckStationFromApiAsync(apiEndpoint, (progress) =>
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        pbProgress.Value = Math.Min(100, Math.Max(0, progress));
                        lblStatus.Text = $"Downloading DuckStation Package... {progress}%";
                    }));
                });

                if (result)
                {
                    lblStatus.Text = "DuckStation installed successfully!";
                    MessageBox.Show("DuckStation has been successfully downloaded, verified, and installed!", "Installation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    _config = EmulatorManager.LoadConfig();
                    RefreshList();
                }
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Installation failed.";
                MessageBox.Show($"Failed to install DuckStation:\n{ex.Message}", "Installation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnInstallDuckStationApi.Enabled = true;
                btnSaveClose.Enabled = true;
                btnAdd.Enabled = true;
                btnRemove.Enabled = true;
                btnTestLaunch.Enabled = true;
                pbProgress.Visible = false;
            }
        }
    }
}
