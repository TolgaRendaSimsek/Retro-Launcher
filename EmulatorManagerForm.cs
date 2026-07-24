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

        private Button btnCancel = null!;
        private Button btnUninstall = null!;
        private Button btnOpenFolder = null!;
        private Label lblChannel = null!;
        private ComboBox cbChannel = null!;
        private Label lblBiosHeader = null!;
        private Label lblBiosStatusVal = null!;
        private Label lblLastUpdateHeader = null!;
        private Label lblLastUpdateVal = null!;
        private DateTime _lastUpdateCheck = DateTime.MinValue;
        private bool _isInstalling = false;
        private CancellationTokenSource? _cts;
        private readonly IEmulatorInstallationService _installationService = new EmulatorInstallationService();

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
                if (lbEmulators.SelectedItem is EmulatorItem selectedEmu)
                {
                    UpdateDetectedStatus(selectedEmu);
                }
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

            // Adjust Form size to give us more vertical room
            this.ClientSize = new System.Drawing.Size(664, 465);

            // Left ListBox sizing adjustments
            lbEmulators.Height = 330;
            btnAdd.Location = new Point(20, 365);
            btnRemove.Location = new Point(135, 365);

            // Setup owner draw on ListBox
            lbEmulators.DrawMode = DrawMode.OwnerDrawFixed;
            lbEmulators.ItemHeight = 44;
            lbEmulators.DrawItem += lbEmulators_DrawItem;

            // Reposition Right Panel fields
            lblName.Location = new Point(260, 23);
            tbName.Location = new Point(370, 20);
            tbName.Width = 270;

            lblPath.Location = new Point(260, 63);
            tbPath.Location = new Point(370, 60);
            tbPath.Width = 225;
            btnBrowse.Location = new Point(600, 60);
            btnBrowse.Width = 40;

            lblVersionHeader.Location = new Point(260, 103);
            lblVersionHeader.Text = "Version Info:";
            lblVersion.Location = new Point(370, 103);
            lblVersion.Width = 270;

            // Initialize new controls programmatically
            lblBiosHeader = new Label
            {
                Text = "BIOS/Firmware:",
                Location = new Point(260, 138),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175),
                AutoSize = true
            };
            this.Controls.Add(lblBiosHeader);

            lblBiosStatusVal = new Label
            {
                Text = "Checking...",
                Location = new Point(370, 138),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true
            };
            this.Controls.Add(lblBiosStatusVal);

            lblDefaultHeader.Location = new Point(260, 173);
            lblDefaultHeader.Text = "Default For:";
            lblDefaultHeader.Width = 100;
            cbDefaultConsole.Location = new Point(370, 170);
            cbDefaultConsole.Width = 270;

            // Channel selection controls
            lblChannel = new Label
            {
                Text = "Release Channel:",
                Location = new Point(260, 208),
                Size = new Size(105, 20),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175),
                AutoSize = true
            };
            this.Controls.Add(lblChannel);

            cbChannel = new ComboBox
            {
                Location = new Point(370, 205),
                Size = new Size(270, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(44, 44, 52),
                ForeColor = Color.White
            };
            cbChannel.Items.Add("Stable");
            cbChannel.Items.Add("Nightly");
            cbChannel.SelectedIndexChanged += cbChannel_SelectedIndexChanged;
            this.Controls.Add(cbChannel);

            lblLastUpdateHeader = new Label
            {
                Text = "Last Checked:",
                Location = new Point(260, 243),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175),
                AutoSize = true
            };
            this.Controls.Add(lblLastUpdateHeader);

            lblLastUpdateVal = new Label
            {
                Text = "Never",
                Location = new Point(370, 243),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.White,
                AutoSize = true
            };
            this.Controls.Add(lblLastUpdateVal);

            btnTestLaunch.Location = new Point(260, 280);
            btnTestLaunch.Width = 380;
            btnTestLaunch.Height = 35;

            btnInstallDuckStationApi.Location = new Point(260, 320);
            btnInstallDuckStationApi.Width = 380;
            btnInstallDuckStationApi.Height = 35;

            pbProgress.Location = new Point(260, 368);
            pbProgress.Width = 300;
            pbProgress.Height = 15;

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(570, 363),
                Size = new Size(70, 25),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(107, 114, 128),
                Visible = false
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += btnCancel_Click;
            this.Controls.Add(btnCancel);

            btnUninstall = new Button
            {
                Text = "🗑️ Uninstall",
                Location = new Point(260, 415),
                Size = new Size(120, 35),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(239, 68, 68),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnUninstall.FlatAppearance.BorderSize = 0;
            btnUninstall.Click += btnUninstall_Click;
            this.Controls.Add(btnUninstall);

            btnOpenFolder = new Button
            {
                Text = "📂 Open Folder",
                Location = new Point(390, 415),
                Size = new Size(120, 35),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(107, 114, 128),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnOpenFolder.FlatAppearance.BorderSize = 0;
            btnOpenFolder.Click += btnOpenFolder_Click;
            this.Controls.Add(btnOpenFolder);

            Button btnHealthCheck = new Button
            {
                Text = "🔍 Health Check",
                Location = new Point(20, 415),
                Size = new Size(220, 35),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(79, 70, 229),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnHealthCheck.FlatAppearance.BorderSize = 0;
            btnHealthCheck.Click += btnHealthCheck_Click;
            this.Controls.Add(btnHealthCheck);

            btnSaveClose.Location = new Point(520, 415);
            btnSaveClose.Size = new Size(120, 35);

            // Hover styles
            SetupHover(btnSaveClose, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105));
            SetupHover(btnTestLaunch, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupHover(btnInstallDuckStationApi, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupHover(btnAdd, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105));
            SetupHover(btnRemove, Color.FromArgb(239, 68, 68), Color.FromArgb(220, 38, 38));
            SetupHover(btnCancel, Color.FromArgb(107, 114, 128), Color.FromArgb(75, 85, 99));
            SetupHover(btnUninstall, Color.FromArgb(239, 68, 68), Color.FromArgb(220, 38, 38));
            SetupHover(btnOpenFolder, Color.FromArgb(107, 114, 128), Color.FromArgb(75, 85, 99));
            SetupHover(btnHealthCheck, Color.FromArgb(79, 70, 229), Color.FromArgb(67, 56, 202));
        }

        private void SetupHover(Button btn, Color baseColor, Color hoverColor)
        {
            btn.BackColor = baseColor;
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = baseColor;
        }

        private void lbEmulators_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= lbEmulators.Items.Count) return;

            var emu = (EmulatorItem)lbEmulators.Items[e.Index];
            if (emu == null) return;

            // Draw background
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bgColor = isSelected ? Color.FromArgb(49, 46, 129) : Color.FromArgb(28, 28, 34);
            using (var brush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            // Draw status indicator circle
            string resolved = ResolvePath(emu.Path);
            bool isInstalled = File.Exists(resolved);
            Color statusColor = Color.FromArgb(239, 68, 68); // Red for missing
            if (isInstalled)
            {
                statusColor = Color.FromArgb(16, 185, 129); // Green for installed
                
                // Check if manually configured
                if (!string.IsNullOrEmpty(emu.InstallFolder))
                {
                    string standardPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, emu.InstallFolder));
                    string resolvedDir = Path.GetDirectoryName(Path.GetFullPath(resolved)) ?? "";
                    if (!resolvedDir.StartsWith(standardPath, StringComparison.OrdinalIgnoreCase))
                    {
                        statusColor = Color.FromArgb(59, 130, 246); // Blue for manual
                    }
                }
                
                // Check if update available
                if (!string.IsNullOrEmpty(emu.LatestVersion) && !string.IsNullOrEmpty(emu.InstalledVersion) && 
                    EmulatorManager.IsUpdateAvailable(emu.Id, emu.InstalledVersion, emu.LatestVersion) && emu.LatestVersion != "Update check unavailable")
                {
                    statusColor = Color.FromArgb(245, 158, 11); // Yellow for update available
                }
            }

            int dotSize = 10;
            int dotX = e.Bounds.Left + 10;
            int dotY = e.Bounds.Top + (e.Bounds.Height - dotSize) / 2;
            using (var dotBrush = new SolidBrush(statusColor))
            {
                e.Graphics.FillEllipse(dotBrush, dotX, dotY, dotSize, dotSize);
            }

            // Draw emulator name
            Font nameFont = new Font("Segoe UI", 10F, FontStyle.Bold);
            Color nameColor = Color.White;
            using (var nameBrush = new SolidBrush(nameColor))
            {
                e.Graphics.DrawString(emu.Name, nameFont, nameBrush, e.Bounds.Left + 28, e.Bounds.Top + 4);
            }

            // Draw console name
            Font consoleFont = new Font("Segoe UI", 8F, FontStyle.Regular);
            Color consoleColor = Color.FromArgb(156, 163, 175);
            using (var consoleBrush = new SolidBrush(consoleColor))
            {
                string consoleText = emu.SupportedPlatforms.FirstOrDefault() ?? "Console";
                e.Graphics.DrawString(consoleText, consoleFont, consoleBrush, e.Bounds.Left + 28, e.Bounds.Top + 22);
            }

            // Draw border separator
            using (var pen = new Pen(Color.FromArgb(44, 44, 52)))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }
        }

        private string GetBiosOrFirmwareStatus(EmulatorItem emu)
        {
            var provider = new JsonEmulatorDefinitionProvider();
            var definition = provider.GetById(emu.Id);
            if (definition == null) return "Not Required";

            if (definition.RequiresBios)
            {
                bool exists = BiosManager.Instance.CheckRealBiosExists(definition.ConsoleName);
                return exists ? "Present" : "BIOS Missing";
            }
            else if (definition.RequiresFirmware)
            {
                bool exists = BiosManager.Instance.CheckRealBiosExists(definition.ConsoleName);
                return exists ? "Present" : "Firmware Missing";
            }

            return "Not Required";
        }

        private void ShowDetailedError(string title, string message, Exception ex)
        {
            // Log stack trace internally
            RetroLogger.Log($"Error: {message}. Details: {ex}", "ERROR");

            // Build simplified message
            string simplifiedMsg = $"{message}\n\nError: {ex.Message}";

            // Custom error dialog with expandable section
            using (Form errorForm = new Form())
            {
                errorForm.Text = title;
                errorForm.Size = new Size(450, 200);
                errorForm.FormBorderStyle = FormBorderStyle.FixedDialog;
                errorForm.MaximizeBox = false;
                errorForm.MinimizeBox = false;
                errorForm.StartPosition = FormStartPosition.CenterParent;
                errorForm.BackColor = Color.FromArgb(20, 20, 25);
                errorForm.ForeColor = Color.White;

                Label lblMsg = new Label
                {
                    Text = simplifiedMsg,
                    Location = new Point(20, 20),
                    Size = new Size(410, 60),
                    Font = new Font("Segoe UI", 9.5F)
                };
                errorForm.Controls.Add(lblMsg);

                Button btnDetails = new Button
                {
                    Text = "Show Technical Details",
                    Location = new Point(20, 95),
                    Size = new Size(150, 30),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(44, 44, 52),
                    ForeColor = Color.White
                };
                errorForm.Controls.Add(btnDetails);

                Button btnOk = new Button
                {
                    Text = "OK",
                    Location = new Point(330, 95),
                    Size = new Size(80, 30),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(16, 185, 129),
                    ForeColor = Color.White
                };
                errorForm.Controls.Add(btnOk);
                btnOk.Click += (s, ev) => errorForm.Close();

                TextBox txtDetails = new TextBox
                {
                    Multiline = true,
                    ReadOnly = true,
                    ScrollBars = ScrollBars.Vertical,
                    Text = ex.ToString(),
                    Location = new Point(20, 140),
                    Size = new Size(390, 150),
                    BackColor = Color.FromArgb(28, 28, 34),
                    ForeColor = Color.FromArgb(239, 68, 68),
                    Font = new Font("Consolas", 8.5F),
                    Visible = false
                };
                errorForm.Controls.Add(txtDetails);

                btnDetails.Click += (s, ev) =>
                {
                    if (txtDetails.Visible)
                    {
                        txtDetails.Visible = false;
                        errorForm.Size = new Size(450, 200);
                        btnDetails.Text = "Show Technical Details";
                    }
                    else
                    {
                        txtDetails.Visible = true;
                        errorForm.Size = new Size(450, 360);
                        btnDetails.Text = "Hide Technical Details";
                    }
                };

                errorForm.ShowDialog(this);
            }
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

            // Check remote updates in the background after the main UI is visible
            this.Shown += (s, ev) =>
            {
                _ = Task.Run(async () =>
                {
                    await CheckRemoteUpdatesAsync();
                });
            };
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

        private async Task CheckRemoteUpdatesAsync()
        {
            var provider = new JsonEmulatorDefinitionProvider();
            var githubService = new GitHubReleaseService();

            foreach (var emu in _config.Emulators)
            {
                try
                {
                    var definition = provider.GetById(emu.Id);
                    if (definition == null) continue;

                    // Sync user's selected release channel
                    if (!string.IsNullOrEmpty(emu.ReleaseChannel))
                    {
                        if (Enum.TryParse<EmulatorReleaseChannel>(emu.ReleaseChannel, out var parsedChannel))
                        {
                            definition.ReleaseChannel = parsedChannel;
                        }
                    }

                    // Query latest release using rate-limit-friendly methods
                    GitHubRelease? latestRelease = null;
                    if (definition.ReleaseSourceType == EmulatorReleaseSourceType.GitHubBinaryRepository ||
                        definition.ReleaseSourceType == EmulatorReleaseSourceType.GitHubReleaseList)
                    {
                        var listRes = await githubService.GetReleasesAsync(definition.RepositoryOwner, definition.RepositoryName, CancellationToken.None);
                        if (listRes.Success && listRes.Data != null && listRes.Data.Any())
                        {
                            var selector = new ReleaseAssetSelector();
                            var selectResult = selector.SelectAsset(definition, listRes.Data);
                            if (selectResult.Status == SelectionStatus.Success && selectResult.SelectedAsset != null)
                            {
                                latestRelease = listRes.Data.FirstOrDefault(r => r.Assets != null && r.Assets.Contains(selectResult.SelectedAsset));
                            }
                        }
                    }
                    else
                    {
                        var latestRes = await githubService.GetLatestReleaseAsync(definition.RepositoryOwner, definition.RepositoryName, CancellationToken.None);
                        if (latestRes.Success) latestRelease = latestRes.Data;
                    }

                    if (latestRelease != null)
                    {
                        emu.LatestVersion = latestRelease.TagName;
                    }
                    else
                    {
                        emu.LatestVersion = "Update check unavailable";
                    }
                }
                catch
                {
                    emu.LatestVersion = "Update check unavailable";
                }
            }

            _lastUpdateCheck = DateTime.Now;

            // Safe UI Refresh
            if (this.IsHandleCreated)
            {
                this.BeginInvoke(new Action(() =>
                {
                    EmulatorManager.SaveConfig(_config);
                    if (lbEmulators.SelectedItem is EmulatorItem selectedEmu)
                    {
                        UpdateDetectedStatus(selectedEmu);
                    }
                    RefreshList();
                }));
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
            
            // Resolve version and status
            UpdateDetectedStatus(emu);

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

            // Set channel ComboBox index
            string channel = emu.ReleaseChannel ?? "Stable";
            int channelIdx = cbChannel.Items.IndexOf(channel);
            cbChannel.SelectedIndex = channelIdx >= 0 ? channelIdx : 0;

            _isUpdatingSelection = false;
        }

        private void ClearDetails()
        {
            _isUpdatingSelection = true;
            tbName.Clear();
            tbPath.Clear();
            lblVersion.Text = "Not Detected";
            cbDefaultConsole.SelectedIndex = 0;
            cbChannel.SelectedIndex = -1;
            _isUpdatingSelection = false;
            btnUninstall.Enabled = false;
            btnOpenFolder.Enabled = false;
        }

        private void UpdateDetectedStatus(EmulatorItem emu)
        {
            string resolved = ResolvePath(emu.Path);
            string status = "Missing";
            string version = "Not Detected";

            if (File.Exists(resolved))
            {
                try
                {
                    FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(resolved);
                    version = versionInfo.ProductVersion ?? versionInfo.FileVersion ?? emu.InstalledVersion;
                    if (string.IsNullOrEmpty(version)) version = "Detected";
                }
                catch
                {
                    version = "Detected";
                }

                // Determine if manual or standard installation path
                if (!string.IsNullOrEmpty(emu.InstallFolder))
                {
                    string standardPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, emu.InstallFolder));
                    string resolvedDir = Path.GetDirectoryName(Path.GetFullPath(resolved)) ?? "";

                    if (resolvedDir.StartsWith(standardPath, StringComparison.OrdinalIgnoreCase))
                    {
                        status = "Installed";
                        if (!string.IsNullOrEmpty(emu.LatestVersion) && !string.IsNullOrEmpty(emu.InstalledVersion) && 
                            EmulatorManager.IsUpdateAvailable(emu.Id, emu.InstalledVersion, emu.LatestVersion) && emu.LatestVersion != "Update check unavailable")
                        {
                            status = "Update available";
                        }
                    }
                    else
                    {
                        status = "Manually configured";
                    }
                }
                else
                {
                    status = "Manually configured";
                }
            }

            lblVersion.Text = $"Installed: {version} | Available: {emu.LatestVersion ?? "Checking..."}";

            // BIOS/Firmware Status Check
            string biosStatus = GetBiosOrFirmwareStatus(emu);
            lblBiosStatusVal.Text = biosStatus;
            if (biosStatus == "Present" || biosStatus == "Not Required")
            {
                lblBiosStatusVal.ForeColor = Color.FromArgb(16, 185, 129); // Green
            }
            else
            {
                lblBiosStatusVal.ForeColor = Color.FromArgb(245, 158, 11); // Yellow/Orange
            }

            // Last Update Check
            lblLastUpdateVal.Text = _lastUpdateCheck == DateTime.MinValue ? "Never" : _lastUpdateCheck.ToString("g");

            UpdateButtonActions(emu, status);
        }

        private void UpdateButtonActions(EmulatorItem emu, string status)
        {
            if (status == "Installing")
            {
                btnInstallDuckStationApi.Text = "⏳  INSTALLING...";
                btnInstallDuckStationApi.Enabled = false;
                btnBrowse.Enabled = false;
                btnUninstall.Enabled = false;
                btnCancel.Visible = true;
                btnOpenFolder.Enabled = false;
            }
            else
            {
                btnCancel.Visible = false;
                btnBrowse.Enabled = true;

                if (status == "Missing" || status == "Installation failed")
                {
                    btnInstallDuckStationApi.Text = "⬇️  INSTALL EMULATOR";
                    btnInstallDuckStationApi.Enabled = true;
                    btnUninstall.Enabled = false;
                    btnOpenFolder.Enabled = false;
                }
                else if (status == "Update available")
                {
                    btnInstallDuckStationApi.Text = "🔄  UPDATE EMULATOR";
                    btnInstallDuckStationApi.Enabled = true;
                    btnUninstall.Enabled = true;
                    btnOpenFolder.Enabled = true;
                }
                else // Installed or Manually configured
                {
                    btnInstallDuckStationApi.Text = "🛠️  REPAIR / REINSTALL";
                    btnInstallDuckStationApi.Enabled = true;
                    btnUninstall.Enabled = true;
                    btnOpenFolder.Enabled = true;
                }
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
            }
        }

        private void cbChannel_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingSelection) return;

            if (lbEmulators.SelectedItem is EmulatorItem selectedEmu)
            {
                string selectedChannel = cbChannel.SelectedItem?.ToString() ?? "Stable";
                selectedEmu.ReleaseChannel = selectedChannel;

                // Save config immediately
                EmulatorManager.SaveConfig(_config);

                // Update detected status based on new channel setting
                UpdateDetectedStatus(selectedEmu);
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

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            _cts?.Cancel();
        }

        private void btnOpenFolder_Click(object? sender, EventArgs e)
        {
            if (lbEmulators.SelectedItem is not EmulatorItem selectedEmu) return;
            string path = ResolvePath(selectedEmu.InstallFolder);
            if (Directory.Exists(path))
            {
                try
                {
                    Process.Start("explorer.exe", $"\"{path}\"");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open directory:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Installation directory does not exist.", "Folder Missing", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnUninstall_Click(object? sender, EventArgs e)
        {
            if (lbEmulators.SelectedItem is not EmulatorItem selectedEmu) return;

            var result = MessageBox.Show(
                $"Are you sure you want to uninstall application files for '{selectedEmu.Name}'?\n\nThis will NOT delete your BIOS, ROMs, Screenshots, or Save files.",
                "Confirm Uninstall",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                try
                {
                    SafeUninstallEmulatorFiles(selectedEmu.InstallFolder);
                    selectedEmu.InstalledVersion = "";
                    selectedEmu.Status = "Missing";
                    selectedEmu.Path = "";

                    EmulatorManager.SaveConfig(_config);
                    
                    UpdateDetectedStatus(selectedEmu);
                    RefreshList();
                    MessageBox.Show($"{selectedEmu.Name} files uninstalled successfully.", "Uninstall Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Uninstall failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SafeUninstallEmulatorFiles(string installFolder)
        {
            if (string.IsNullOrEmpty(installFolder)) return;
            string resolved = ResolvePath(installFolder);
            if (!Directory.Exists(resolved)) return;

            foreach (string file in Directory.GetFiles(resolved))
            {
                try { File.Delete(file); } catch { }
            }

            foreach (string subDir in Directory.GetDirectories(resolved))
            {
                string name = Path.GetFileName(subDir).ToLower();
                if (name == "bios" || name == "saves" || name == "configs" || 
                    name == "screenshots" || name == "games" || name == "roms")
                {
                    continue;
                }
                try { Directory.Delete(subDir, true); } catch { }
            }
        }

        private async void btnInstallDuckStationApi_Click(object? sender, EventArgs e)
        {
            if (lbEmulators.SelectedItem is not EmulatorItem selectedEmu) return;
            if (_isInstalling) return;
            _isInstalling = true;

            // Check if manually configured
            string resolved = ResolvePath(selectedEmu.Path);
            if (File.Exists(resolved))
            {
                string standardPath = string.IsNullOrEmpty(selectedEmu.InstallFolder) 
                    ? "" 
                    : Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, selectedEmu.InstallFolder));
                string resolvedDir = Path.GetDirectoryName(Path.GetFullPath(resolved)) ?? "";

                if (!string.IsNullOrEmpty(standardPath) && !resolvedDir.StartsWith(standardPath, StringComparison.OrdinalIgnoreCase))
                {
                    var manualPrompt = MessageBox.Show(
                        $"You have configured a manual installation of {selectedEmu.Name} at '{selectedEmu.Path}'.\n\nDo you want to replace it with the automatic standard installation?",
                        "Manual Installation Detected",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );
                    if (manualPrompt == DialogResult.No)
                    {
                        _isInstalling = false;
                        return;
                    }
                }
            }

            // Disable conflicting buttons
            btnInstallDuckStationApi.Enabled = false;
            btnSaveClose.Enabled = false;
            btnAdd.Enabled = false;
            btnRemove.Enabled = false;
            btnTestLaunch.Enabled = false;
            btnUninstall.Enabled = false;
            btnOpenFolder.Enabled = false;
            btnBrowse.Enabled = false;

            pbProgress.Value = 0;
            pbProgress.Visible = true;
            btnCancel.Visible = true;
            lblStatus.Text = "Initializing...";

            _cts = new CancellationTokenSource();

            try
            {
                var progress = new Progress<EmulatorInstallationProgress>(p =>
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        pbProgress.Value = Math.Min(100, Math.Max(0, p.Percentage));
                        lblStatus.Text = $"{p.CurrentStep}... {p.Percentage}%";
                    }));
                });

                var installReq = new EmulatorInstallationRequest
                {
                    EmulatorId = selectedEmu.Id,
                    Progress = progress,
                    CancellationToken = _cts.Token
                };

                var installResult = await _installationService.InstallAsync(installReq);

                if (installResult.Success && installResult.InstalledInfo != null)
                {
                    lblStatus.Text = "Installation complete!";
                    MessageBox.Show($"{selectedEmu.Name} installed successfully to version {installResult.InstalledInfo.InstalledVersion}!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    // Reload settings
                    _config = EmulatorManager.LoadConfig();
                    
                    // Sync fields
                    var updatedEmu = _config.Emulators.FirstOrDefault(x => string.Equals(x.Id, selectedEmu.Id, StringComparison.OrdinalIgnoreCase));
                    if (updatedEmu != null)
                    {
                        selectedEmu = updatedEmu;
                    }

                    RefreshList();
                    lbEmulators.SelectedItem = selectedEmu;
                }
                else
                {
                    lblStatus.Text = "Installation failed.";
                    var ex = new Exception(installResult.ErrorMessage ?? "Deployment verification check failed.");
                    ShowDetailedError("Installation Error", $"Failed to deploy {selectedEmu.Name}.", ex);
                }
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "Installation cancelled.";
                MessageBox.Show("Installation was cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Installation failed.";
                ShowDetailedError("Installation Failure", $"An error occurred while installing {selectedEmu.Name}.", ex);
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                _isInstalling = false;

                btnInstallDuckStationApi.Enabled = true;
                btnSaveClose.Enabled = true;
                btnAdd.Enabled = true;
                btnRemove.Enabled = true;
                btnTestLaunch.Enabled = true;
                btnBrowse.Enabled = true;
                pbProgress.Visible = false;
                btnCancel.Visible = false;

                UpdateDetectedStatus(selectedEmu);
            }
        }

        private void btnHealthCheck_Click(object? sender, EventArgs e)
        {
            using (var healthForm = new HealthCheckForm())
            {
                healthForm.ShowDialog(this);
            }
            if (lbEmulators.SelectedItem is EmulatorItem selectedEmu)
            {
                UpdateDetectedStatus(selectedEmu);
            }
            RefreshList();
        }
    }
}
