using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RetroLauncher.UI.Forms
{
    public class PackageManagerForm : Form
    {
        // UI Layout Elements
        private Panel _pnlSidebar = null!;
        private Panel _pnlDetails = null!;
        private Panel _pnlHeader = null!;
        private Panel _pnlQueue = null!;
        private FlowLayoutPanel _flpCards = null!;

        // Controls
        private TextBox _txtSearch = null!;
        private CheckBox _chkInstalledOnly = null!;
        private CheckBox _chkUpdateAvailable = null!;
        private ProgressBar _pbQueueProgress = null!;
        private Label _lblQueueStatus = null!;
        
        // Top-level buttons
        private Button _btnUpdateAll = null!;
        private Button _btnInstallSelected = null!;
        private Button _btnCheckUpdates = null!;
        private Button _btnClearCompleted = null!;

        // Right details panel controls
        private PictureBox _pbDetailIcon = null!;
        private Label _lblDetailName = null!;
        private Label _lblDetailMeta = null!;
        private TextBox _txtDetailDesc = null!;
        private TextBox _txtDetailNotes = null!;
        private Button _btnInstall = null!;
        private Button _btnUpdate = null!;
        private Button _btnRepair = null!;
        private Button _btnRemove = null!;
        private Button _btnCancel = null!;
        private Button _btnOpenFolder = null!;
        private Button _btnManualInstall = null!;
        private Button _btnViewNotes = null!;

        // Data Fields
        private List<PackageManifest> _remotePackages = new();
        private PackageManifest? _selectedPackage;
        private readonly List<string> _bulkSelectionIds = new();
        private CancellationTokenSource? _cts;

        private string _activeCategory = "All";
        private static readonly string MockManifestPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "packages_manifest.json");

        // Theme colors
        private static readonly Color SteamBg = Color.FromArgb(17, 26, 39);
        private static readonly Color SteamSidebarBg = Color.FromArgb(20, 24, 30);
        private static readonly Color SteamCardBg = Color.FromArgb(28, 38, 52);
        private static readonly Color SteamCardBgSelected = Color.FromArgb(41, 56, 76);
        private static readonly Color SteamAccent = Color.FromArgb(102, 192, 244);
        private static readonly Color SteamGreen = Color.FromArgb(102, 192, 92);
        private static readonly Color SteamRed = Color.FromArgb(217, 83, 79);
        private static readonly Color SteamOrange = Color.FromArgb(240, 173, 78);

        public PackageManagerForm()
        {
            this.Text = "RetroLauncher Package Manager";
            this.Size = new Size(1000, 680);
            this.MinimumSize = new Size(850, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = SteamBg;

            InitializeLayout();
            SeedMockManifestIfNeeded();
            _ = LoadManifestAsync();
        }

        private void InitializeLayout()
        {
            // 1. Header Top Bar Panel
            _pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(23, 29, 37),
                Padding = new Padding(10)
            };

            _txtSearch = new TextBox
            {
                Location = new Point(15, 18),
                Width = 180,
                BackColor = Color.FromArgb(31, 41, 55),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5F)
            };
            _txtSearch.Text = "Search packages...";
            _txtSearch.Enter += (s, e) => { if (_txtSearch.Text == "Search packages...") _txtSearch.Text = ""; };
            _txtSearch.Leave += (s, e) => { if (string.IsNullOrWhiteSpace(_txtSearch.Text)) _txtSearch.Text = "Search packages..."; };
            _txtSearch.TextChanged += (s, e) => ApplyFilters();

            _btnUpdateAll = CreateHeaderButton("Update All", new Point(210, 15), SteamOrange, btnUpdateAll_Click);
            _btnInstallSelected = CreateHeaderButton("Install Selected", new Point(320, 15), SteamGreen, btnInstallSelected_Click);
            _btnCheckUpdates = CreateHeaderButton("Check for Updates", new Point(445, 15), SteamAccent, btnCheckUpdates_Click);
            _btnClearCompleted = CreateHeaderButton("Clear Completed", new Point(590, 15), Color.Gray, btnClearCompleted_Click);

            _pnlHeader.Controls.AddRange(new Control[] { _txtSearch, _btnUpdateAll, _btnInstallSelected, _btnCheckUpdates, _btnClearCompleted });
            this.Controls.Add(_pnlHeader);

            // 2. Left Sidebar Panel
            _pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 190,
                BackColor = SteamSidebarBg,
                Padding = new Padding(10, 15, 10, 15)
            };

            Label lblCategories = new Label
            {
                Text = "CATEGORIES",
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Location = new Point(12, 10),
                AutoSize = true
            };
            _pnlSidebar.Controls.Add(lblCategories);

            int startY = 32;
            string[] categories = new[] { "All", "Emulator", "Theme", "Shader", "Mod", "LanguagePack", "Plugin", "Tool", "Firmware" };
            foreach (var cat in categories)
            {
                Button btnCat = new Button
                {
                    Text = cat,
                    Location = new Point(10, startY),
                    Size = new Size(170, 26),
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.White,
                    BackColor = Color.Transparent,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Font = new Font("Segoe UI", 9F)
                };
                btnCat.FlatAppearance.BorderSize = 0;
                btnCat.FlatAppearance.MouseOverBackColor = Color.FromArgb(31, 41, 55);
                btnCat.Click += (s, e) =>
                {
                    _activeCategory = cat;
                    ApplyFilters();
                };
                _pnlSidebar.Controls.Add(btnCat);
                startY += 28;
            }

            startY += 10;
            Label lblFilters = new Label
            {
                Text = "FILTERS",
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Location = new Point(12, startY),
                AutoSize = true
            };
            _pnlSidebar.Controls.Add(lblFilters);
            startY += 20;

            _chkInstalledOnly = new CheckBox
            {
                Text = "Installed Only",
                ForeColor = Color.White,
                Location = new Point(15, startY),
                Size = new Size(160, 20),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _chkInstalledOnly.CheckedChanged += (s, e) => ApplyFilters();
            _pnlSidebar.Controls.Add(_chkInstalledOnly);
            startY += 24;

            _chkUpdateAvailable = new CheckBox
            {
                Text = "Updates Only",
                ForeColor = Color.White,
                Location = new Point(15, startY),
                Size = new Size(160, 20),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F)
            };
            _chkUpdateAvailable.CheckedChanged += (s, e) => ApplyFilters();
            _pnlSidebar.Controls.Add(_chkUpdateAvailable);

            this.Controls.Add(_pnlSidebar);

            // 3. Bottom Queue / Progress Bar Panel
            _pnlQueue = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(23, 29, 37),
                Padding = new Padding(15, 10, 15, 10)
            };

            _lblQueueStatus = new Label
            {
                Text = "Queue Empty - Idle",
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                Location = new Point(15, 8),
                Size = new Size(400, 18)
            };

            _pbQueueProgress = new ProgressBar
            {
                Location = new Point(15, 28),
                Size = new Size(500, 16),
                Style = ProgressBarStyle.Continuous
            };

            Button btnLogs = new Button
            {
                Text = "📜 Open Logs",
                Location = new Point(530, 20),
                Size = new Size(100, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(31, 41, 55),
                ForeColor = Color.White
            };
            btnLogs.Click += (s, e) => ViewLogsFile();

            _pnlQueue.Controls.AddRange(new Control[] { _lblQueueStatus, _pbQueueProgress, btnLogs });
            this.Controls.Add(_pnlQueue);

            // 4. Right Details Panel
            _pnlDetails = new Panel
            {
                Dock = DockStyle.Right,
                Width = 280,
                BackColor = Color.FromArgb(24, 32, 45),
                Padding = new Padding(15)
            };

            _pbDetailIcon = new PictureBox
            {
                Size = new Size(48, 48),
                Location = new Point(15, 15),
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent
            };

            _lblDetailName = new Label
            {
                Text = "No Package Selected",
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(70, 15),
                Size = new Size(195, 24)
            };

            _lblDetailMeta = new Label
            {
                Text = "Select a card to view actions.",
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(70, 40),
                Size = new Size(195, 30)
            };

            // Description Label & Value
            _txtDetailDesc = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(24, 32, 45),
                ForeColor = Color.FromArgb(209, 213, 219),
                BorderStyle = BorderStyle.None,
                Location = new Point(15, 80),
                Size = new Size(250, 80),
                Font = new Font("Segoe UI", 9F)
            };

            _txtDetailNotes = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                BackColor = Color.FromArgb(17, 24, 39),
                ForeColor = Color.FromArgb(156, 163, 175),
                BorderStyle = BorderStyle.FixedSingle,
                Location = new Point(15, 170),
                Size = new Size(250, 140),
                Font = new Font("Segoe UI", 8.5F),
                ScrollBars = ScrollBars.Vertical
            };

            // Operation Actions Buttons Stack
            int btnY = 325;
            _btnInstall = CreateDetailButton("Install", ref btnY, SteamGreen, btnInstall_Click);
            _btnUpdate = CreateDetailButton("Update", ref btnY, SteamOrange, btnUpdate_Click);
            _btnRepair = CreateDetailButton("Repair", ref btnY, SteamAccent, btnRepair_Click);
            _btnRemove = CreateDetailButton("Remove", ref btnY, SteamRed, btnRemove_Click);
            _btnCancel = CreateDetailButton("Cancel Download", ref btnY, Color.FromArgb(55, 65, 81), btnCancel_Click);
            _btnOpenFolder = CreateDetailButton("Open Install Folder", ref btnY, Color.FromArgb(55, 65, 81), btnOpenFolder_Click);
            _btnManualInstall = CreateDetailButton("Manual Install Archive", ref btnY, Color.FromArgb(55, 65, 81), btnManualInstall_Click);
            _btnViewNotes = CreateDetailButton("View Release Notes", ref btnY, Color.FromArgb(55, 65, 81), btnViewNotes_Click);

            _pnlDetails.Controls.AddRange(new Control[]
            {
                _pbDetailIcon, _lblDetailName, _lblDetailMeta, _txtDetailDesc, _txtDetailNotes,
                _btnInstall, _btnUpdate, _btnRepair, _btnRemove, _btnCancel, _btnOpenFolder, _btnManualInstall, _btnViewNotes
            });

            this.Controls.Add(_pnlDetails);

            // 5. Scrollable FlowLayoutPanel for Package Cards
            _flpCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = SteamBg,
                Padding = new Padding(15)
            };

            this.Controls.Add(_flpCards);
            UpdateDetailPanel();
        }

        private Button CreateHeaderButton(string text, Point location, Color backColor, EventHandler onClick)
        {
            Button btn = new Button
            {
                Text = text,
                Location = location,
                Size = new Size(105, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            return btn;
        }

        private Button CreateDetailButton(string text, ref int startY, Color backColor, EventHandler onClick)
        {
            Button btn = new Button
            {
                Text = text,
                Location = new Point(15, startY),
                Size = new Size(250, 26),
                FlatStyle = FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Visible = false
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.Click += onClick;
            startY += 30;
            return btn;
        }

        private void SeedMockManifestIfNeeded()
        {
            if (File.Exists(MockManifestPath)) return;

            var mockPackages = new List<PackageManifest>
            {
                new PackageManifest
                {
                    id = "neon_theme",
                    name = "Neon Dark Theme",
                    description = "Vibrant neon glow borders with dark transparent panels.",
                    packageType = PackageType.Theme,
                    version = "1.2.0",
                    downloadUrl = "https://github.com/stenzek/duckstation/releases/download/latest/duckstation-windows-x64-release.zip",
                    archiveType = "zip",
                    installFolder = "NeonDark",
                    executablePath = "theme.json",
                    downloadSize = 34500000,
                    preservedPaths = new List<string> { "custom_configs.txt" },
                    releaseNotes = "v1.2.0: Added dynamic neon presets. Fixed grid layout scaling.\nv1.1.0: Initial dark transparent panel setup."
                },
                new PackageManifest
                {
                    id = "crt_scanlines",
                    name = "CRT Scanlines Shader",
                    description = "Realistic CRT aperture grille shadow mask shader.",
                    packageType = PackageType.Shader,
                    version = "1.0.5",
                    downloadUrl = "https://github.com/stenzek/duckstation/releases/download/latest/duckstation-windows-x64-release.zip",
                    archiveType = "zip",
                    installFolder = "CrtScanlines",
                    executablePath = "crt.slang",
                    downloadSize = 1200000,
                    releaseNotes = "v1.0.5: Added curved phosphor distortion toggles.\nv1.0.0: Seeded CRT scanline overlays."
                },
                new PackageManifest
                {
                    id = "tr_lang_pack",
                    name = "Turkish Language Pack",
                    description = "Full UI and configuration localization translations.",
                    packageType = PackageType.LanguagePack,
                    version = "1.0.0",
                    downloadUrl = "https://github.com/stenzek/duckstation/releases/download/latest/duckstation-windows-x64-release.zip",
                    archiveType = "zip",
                    installFolder = "TR",
                    executablePath = "tr.json",
                    downloadSize = 45000,
                    releaseNotes = "v1.0.0: Full keyboard character maps and layout files added."
                }
            };

            try
            {
                string json = JsonSerializer.Serialize(mockPackages, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(MockManifestPath, json);
            }
            catch { }
        }

        private async Task LoadManifestAsync()
        {
            _lblQueueStatus.Text = "Querying packages catalog...";
            try
            {
                var provider = new JsonPackageCatalogProvider();
                _remotePackages = await provider.GetCatalogAsync(MockManifestPath, CancellationToken.None);
                BuildCardsList();
                _lblQueueStatus.Text = "Catalog loaded successfully.";
            }
            catch (Exception ex)
            {
                _lblQueueStatus.Text = $"Failed to load packages: {ex.Message}";
            }
        }

        private void BuildCardsList()
        {
            _flpCards.SuspendLayout();
            _flpCards.Controls.Clear();

            PackageManagerService.Instance.Repository.Load();

            foreach (var package in _remotePackages)
            {
                // Local matches
                var local = PackageManagerService.Instance.Repository.GetById(package.id);
                bool matchesFilters = MatchesFilters(package, local);

                if (!matchesFilters) continue;

                Panel card = new Panel
                {
                    Size = new Size(490, 80),
                    BackColor = (_selectedPackage?.id == package.id) ? SteamCardBgSelected : SteamCardBg,
                    Margin = new Padding(0, 0, 10, 10),
                    Padding = new Padding(10),
                    Cursor = Cursors.Hand
                };

                // Click selectors
                card.Click += (s, e) => SelectPackage(package);

                // Checkbox for bulk installs
                CheckBox chkBulk = new CheckBox
                {
                    Location = new Point(10, 30),
                    Size = new Size(20, 20),
                    Checked = _bulkSelectionIds.Contains(package.id)
                };
                chkBulk.CheckedChanged += (s, e) =>
                {
                    if (chkBulk.Checked)
                    {
                        if (!_bulkSelectionIds.Contains(package.id)) _bulkSelectionIds.Add(package.id);
                    }
                    else
                    {
                        _bulkSelectionIds.Remove(package.id);
                    }
                };
                card.Controls.Add(chkBulk);

                // Type Icon Generator Box
                PictureBox pbIcon = new PictureBox
                {
                    Location = new Point(35, 16),
                    Size = new Size(48, 48),
                    BackColor = Color.FromArgb(17, 24, 39),
                    SizeMode = PictureBoxSizeMode.CenterImage
                };
                // Paint mock placeholder icon based on package type
                pbIcon.Paint += (s, e) =>
                {
                    string initial = package.name.Length > 0 ? package.name[0].ToString().ToUpper() : "P";
                    using (Brush b = new SolidBrush(SteamAccent))
                    {
                        e.Graphics.DrawString(initial, new Font("Segoe UI", 16F, FontStyle.Bold), b, 12, 10);
                    }
                };
                pbIcon.Click += (s, e) => SelectPackage(package);
                card.Controls.Add(pbIcon);

                // Metadata details
                Label lblName = new Label
                {
                    Text = package.name,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
                    Location = new Point(95, 10),
                    Size = new Size(240, 20),
                    AutoSize = false
                };
                lblName.Click += (s, e) => SelectPackage(package);
                card.Controls.Add(lblName);

                Label lblDesc = new Label
                {
                    Text = package.description,
                    ForeColor = Color.FromArgb(156, 163, 175),
                    Font = new Font("Segoe UI", 8.5F),
                    Location = new Point(95, 30),
                    Size = new Size(240, 20),
                    AutoEllipsis = true
                };
                lblDesc.Click += (s, e) => SelectPackage(package);
                card.Controls.Add(lblDesc);

                // Size & Types
                string sizeStr = $"{package.downloadSize / 1024.0 / 1024.0:F1} MB";
                Label lblSub = new Label
                {
                    Text = $"{package.packageType} • {sizeStr}",
                    ForeColor = Color.FromArgb(156, 163, 175),
                    Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                    Location = new Point(95, 52),
                    Size = new Size(240, 18)
                };
                lblSub.Click += (s, e) => SelectPackage(package);
                card.Controls.Add(lblSub);

                // Version comparison and status badges
                string installedVer = local != null ? local.installedVersion : "—";
                string statusText = "Not Installed";
                Color statusColor = Color.FromArgb(156, 163, 175);

                if (local != null)
                {
                    bool healthy = PackageManagerService.Instance.VerifyHealth(package.id);
                    if (!healthy)
                    {
                        statusText = "Broken";
                        statusColor = SteamRed;
                    }
                    else if (PackageManagerService.Instance.UpdateService.IsUpdateAvailable(local.installedVersion, package.version))
                    {
                        statusText = "Update Ready";
                        statusColor = SteamOrange;
                    }
                    else
                    {
                        statusText = "Ready";
                        statusColor = SteamGreen;
                    }
                }

                Label lblStatus = new Label
                {
                    Text = statusText,
                    ForeColor = statusColor,
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    Location = new Point(340, 15),
                    Size = new Size(135, 18),
                    TextAlign = ContentAlignment.MiddleRight
                };
                lblStatus.Click += (s, e) => SelectPackage(package);
                card.Controls.Add(lblStatus);

                Label lblVersions = new Label
                {
                    Text = $"Local: {installedVer}\nLatest: {package.version}",
                    ForeColor = Color.FromArgb(156, 163, 175),
                    Font = new Font("Segoe UI", 8F),
                    Location = new Point(340, 36),
                    Size = new Size(135, 30),
                    TextAlign = ContentAlignment.TopRight
                };
                lblVersions.Click += (s, e) => SelectPackage(package);
                card.Controls.Add(lblVersions);

                _flpCards.Controls.Add(card);
            }

            _flpCards.ResumeLayout();
            _flpCards.Refresh();
        }

        private bool MatchesFilters(PackageManifest package, InstalledPackage? local)
        {
            // Category check
            if (_activeCategory != "All" && !string.Equals(package.packageType.ToString(), _activeCategory, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Search filter
            string searchVal = _txtSearch.Text.Trim().ToLower();
            if (searchVal != "search packages..." && !string.IsNullOrEmpty(searchVal))
            {
                if (!package.name.ToLower().Contains(searchVal) && !package.description.ToLower().Contains(searchVal))
                {
                    return false;
                }
            }

            // State checkbox filters
            if (_chkInstalledOnly.Checked && local == null) return false;

            if (_chkUpdateAvailable.Checked)
            {
                if (local == null) return false;
                bool hasUpdate = PackageManagerService.Instance.UpdateService.IsUpdateAvailable(local.installedVersion, package.version);
                if (!hasUpdate) return false;
            }

            return true;
        }

        private void ApplyFilters()
        {
            BuildCardsList();
        }

        private void SelectPackage(PackageManifest package)
        {
            _selectedPackage = package;
            UpdateDetailPanel();
            
            // Re-render highlight selection borders
            foreach (Control card in _flpCards.Controls)
            {
                if (card is Panel p)
                {
                    p.BackColor = SteamCardBg;
                }
            }

            // Redraw highlighting
            BuildCardsList();
        }

        private void UpdateDetailPanel()
        {
            if (_selectedPackage == null)
            {
                _lblDetailName.Text = "No Selection";
                _lblDetailMeta.Text = "Select a card to view actions.";
                _txtDetailDesc.Text = "";
                _txtDetailNotes.Text = "";
                
                // Hide action buttons
                ToggleButtons(false);
                return;
            }

            _lblDetailName.Text = _selectedPackage.name;
            string sizeStr = $"{_selectedPackage.downloadSize / 1024.0 / 1024.0:F1} MB";
            _lblDetailMeta.Text = $"{_selectedPackage.packageType} • {sizeStr}\nAuthor: {_selectedPackage.author}";
            _txtDetailDesc.Text = _selectedPackage.description;
            _txtDetailNotes.Text = !string.IsNullOrEmpty(_selectedPackage.releaseNotes) ? _selectedPackage.releaseNotes : "No release notes available.";

            // Show relevant actions based on installation status
            ToggleButtons(true);
        }

        private void ToggleButtons(bool hasSelection)
        {
            // Default hide all
            _btnInstall.Visible = false;
            _btnUpdate.Visible = false;
            _btnRepair.Visible = false;
            _btnRemove.Visible = false;
            _btnCancel.Visible = false;
            _btnOpenFolder.Visible = false;
            _btnManualInstall.Visible = true; // Always visible
            _btnViewNotes.Visible = hasSelection;

            if (!hasSelection) return;

            var local = PackageManagerService.Instance.Repository.GetById(_selectedPackage!.id);
            if (local == null)
            {
                _btnInstall.Visible = true;
            }
            else
            {
                bool healthy = PackageManagerService.Instance.VerifyHealth(_selectedPackage.id);
                if (!healthy)
                {
                    _btnRepair.Visible = true;
                    _btnRemove.Visible = true;
                }
                else if (PackageManagerService.Instance.UpdateService.IsUpdateAvailable(local.installedVersion, _selectedPackage.version))
                {
                    _btnUpdate.Visible = true;
                    _btnRepair.Visible = true;
                    _btnRemove.Visible = true;
                }
                else
                {
                    _btnRepair.Visible = true; // Reinstall option
                    _btnRemove.Visible = true;
                }

                _btnOpenFolder.Visible = true;
            }
        }

        // --- Operation Buttons ---
        private async void btnInstall_Click(object? sender, EventArgs e)
        {
            if (_selectedPackage == null) return;
            await RunOperationAsync(_selectedPackage, async (progress, token) =>
            {
                _lblQueueStatus.Text = $"Downloading {_selectedPackage.name}...";
                bool ok = await PackageManagerService.Instance.InstallPackageAsync(_selectedPackage, progress, token);
                _lblQueueStatus.Text = ok ? $"{_selectedPackage.name} installed successfully." : $"{_selectedPackage.name} installation failed.";
            });
        }

        private async void btnUpdate_Click(object? sender, EventArgs e)
        {
            if (_selectedPackage == null) return;
            await RunOperationAsync(_selectedPackage, async (progress, token) =>
            {
                _lblQueueStatus.Text = $"Updating {_selectedPackage.name}...";
                bool ok = await PackageManagerService.Instance.InstallPackageAsync(_selectedPackage, progress, token);
                _lblQueueStatus.Text = ok ? $"{_selectedPackage.name} updated successfully." : $"{_selectedPackage.name} update failed.";
            });
        }

        private async void btnRepair_Click(object? sender, EventArgs e)
        {
            if (_selectedPackage == null) return;
            await RunOperationAsync(_selectedPackage, async (progress, token) =>
            {
                _lblQueueStatus.Text = $"Repairing {_selectedPackage.name}...";
                bool ok = await PackageManagerService.Instance.RepairPackageAsync(_selectedPackage, progress, token);
                _lblQueueStatus.Text = ok ? $"{_selectedPackage.name} repaired successfully." : $"{_selectedPackage.name} repair failed.";
            });
        }

        private async void btnRemove_Click(object? sender, EventArgs e)
        {
            if (_selectedPackage == null) return;
            var res = MessageBox.Show($"Are you sure you want to remove {_selectedPackage.name}?", "Remove Package", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (res == DialogResult.Yes)
            {
                _lblQueueStatus.Text = $"Removing {_selectedPackage.name}...";
                bool ok = await PackageManagerService.Instance.RemovePackageAsync(_selectedPackage.id, CancellationToken.None);
                _lblQueueStatus.Text = ok ? $"{_selectedPackage.name} removed successfully." : $"{_selectedPackage.name} removal failed.";
                BuildCardsList();
                UpdateDetailPanel();
            }
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            _cts?.Cancel();
            _lblQueueStatus.Text = "Operation cancelled by user.";
        }

        private void btnOpenFolder_Click(object? sender, EventArgs e)
        {
            if (_selectedPackage == null) return;
            var local = PackageManagerService.Instance.Repository.GetById(_selectedPackage.id);
            if (local != null)
            {
                string path = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, local.installedPath));
                if (Directory.Exists(path))
                {
                    try
                    {
                        Process.Start("explorer.exe", $"\"{path}\"");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Could not open folder: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private async void btnManualInstall_Click(object? sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Supported Archives (*.zip; *.7z)|*.zip;*.7z";
                ofd.Title = "Select Local Archive File";

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    string filename = Path.GetFileNameWithoutExtension(ofd.FileName);
                    var mockMetadata = new PackageManifest
                    {
                        id = filename.ToLower().Replace(" ", "_"),
                        name = filename,
                        packageType = PackageType.Mod,
                        version = "1.0.0",
                        installFolder = filename.Replace(" ", ""),
                        archiveType = ofd.FileName.EndsWith(".7z") ? "7z" : "zip"
                    };

                    await RunOperationAsync(mockMetadata, async (progress, token) =>
                    {
                        _lblQueueStatus.Text = $"Manually installing {mockMetadata.name}...";
                        bool ok = await PackageManagerService.Instance.InstallManualPackageAsync(ofd.FileName, mockMetadata, progress, token);
                        _lblQueueStatus.Text = ok ? $"Manual install of {mockMetadata.name} completed." : "Manual installation failed.";
                    });
                }
            }
        }

        private void btnViewNotes_Click(object? sender, EventArgs e)
        {
            if (_selectedPackage == null) return;
            MessageBox.Show(_selectedPackage.releaseNotes, $"{_selectedPackage.name} Release Notes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // --- Top Header Actions ---
        private async void btnUpdateAll_Click(object? sender, EventArgs e)
        {
            var updateManifests = new List<PackageManifest>();
            foreach (var package in _remotePackages)
            {
                var local = PackageManagerService.Instance.Repository.GetById(package.id);
                if (local != null && PackageManagerService.Instance.UpdateService.IsUpdateAvailable(local.installedVersion, package.version))
                {
                    updateManifests.Add(package);
                }
            }

            if (updateManifests.Count == 0)
            {
                MessageBox.Show("All packages are already up to date.", "Updates Checked", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int count = 0;
            foreach (var package in updateManifests)
            {
                count++;
                _lblQueueStatus.Text = $"Bulk updating ({count}/{updateManifests.Count}): {package.name}...";
                await RunOperationAsync(package, async (progress, token) =>
                {
                    await PackageManagerService.Instance.InstallPackageAsync(package, progress, token);
                });
            }
            _lblQueueStatus.Text = "Bulk updates completed.";
        }

        private async void btnInstallSelected_Click(object? sender, EventArgs e)
        {
            if (_bulkSelectionIds.Count == 0)
            {
                MessageBox.Show("No packages selected for bulk installation. Check card checkboxes first.", "Install Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectionManifests = _remotePackages.Where(p => _bulkSelectionIds.Contains(p.id)).ToList();
            int count = 0;

            foreach (var package in selectionManifests)
            {
                count++;
                _lblQueueStatus.Text = $"Bulk installing ({count}/{selectionManifests.Count}): {package.name}...";
                await RunOperationAsync(package, async (progress, token) =>
                {
                    await PackageManagerService.Instance.InstallPackageAsync(package, progress, token);
                });
            }

            _bulkSelectionIds.Clear();
            _lblQueueStatus.Text = "Bulk installations completed.";
        }

        private async void btnCheckUpdates_Click(object? sender, EventArgs e)
        {
            _lblQueueStatus.Text = "Checking remote package feeds...";
            await LoadManifestAsync();
            MessageBox.Show("Package manifests successfully loaded and updated.", "Check Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnClearCompleted_Click(object? sender, EventArgs e)
        {
            _pbQueueProgress.Value = 0;
            _lblQueueStatus.Text = "Queue cleared - Idle";
        }

        // --- Core Async Helper ---
        private async Task RunOperationAsync(PackageManifest package, Func<IProgress<int>, CancellationToken, Task> action)
        {
            this.Enabled = false;
            _btnCancel.Visible = true;
            _pbQueueProgress.Value = 0;
            _cts = new CancellationTokenSource();

            var progress = new Progress<int>(percent =>
            {
                _pbQueueProgress.Value = Math.Min(100, Math.Max(0, percent));
            });

            try
            {
                await action(progress, _cts.Token);
            }
            catch (Exception ex)
            {
                _lblQueueStatus.Text = $"Error: {ex.Message}";
            }
            finally
            {
                this.Enabled = true;
                _btnCancel.Visible = false;
                _pbQueueProgress.Value = 100;
                BuildCardsList();
                UpdateDetailPanel();
            }
        }

        private void ViewLogsFile()
        {
            string logs = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "package_manager.log");
            if (File.Exists(logs))
            {
                try
                {
                    Process.Start("notepad.exe", $"\"{logs}\"");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open logs: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("No package logs found.", "Logs", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
