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
using RetroLauncher.Core.Utilities;
using RetroLauncher.UI.Controls;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Forms
{
    public class PackageManagerForm : Form
    {
        // Primary Layout Containers
        private TableLayoutPanel _tblRoot = null!;
        private TableLayoutPanel _tblHeader = null!;
        private TableLayoutPanel _tblMainSplit = null!;
        private TableLayoutPanel _tblBottomStatus = null!;

        private Panel _pnlLeftFilter = null!;
        private Panel _pnlPackageListHost = null!;
        private Panel _pnlDetails = null!;

        // Header & Search
        private Label _lblPageTitle = null!;
        private SearchBox _searchBox = null!;
        private ModernButton _btnCheckUpdates = null!;
        private ModernButton _btnInstallSelected = null!;

        // Filters
        private FlowLayoutPanel _flpCategories = null!;
        private CheckBox _chkInstalledOnly = null!;
        private CheckBox _chkUpdateAvailable = null!;
        private ModernButton _btnClearFilters = null!;

        // Package List & Cards
        private FlowLayoutPanel _flpCards = null!;
        private EmptyStatePanel _emptyStatePanel = null!;

        // Details Panel Controls
        private Label _lblDetailName = null!;
        private Label _lblDetailMeta = null!;
        private TextBox _txtDetailDesc = null!;
        private TextBox _txtDetailNotes = null!;
        private ModernButton _btnInstall = null!;
        private ModernButton _btnUpdate = null!;
        private ModernButton _btnRepair = null!;
        private ModernButton _btnRemove = null!;
        private ModernButton _btnOpenFolder = null!;
        private ModernButton _btnManualInstall = null!;

        // Bottom Status Bar
        private Label _lblQueueStatus = null!;
        private ProgressBar _pbQueueProgress = null!;
        private ModernButton _btnOpenLogs = null!;

        // Data Fields
        private List<PackageManifest> _remotePackages = new();
        private PackageManifest? _selectedPackage;
        private readonly List<string> _bulkSelectionIds = new();
        private CancellationTokenSource? _cts;
        private string _activeCategory = "All";
        private static readonly string ManifestPath = ApplicationPaths.ResolveWritablePath("packages_manifest.json");

        public PackageManagerForm()
        {
            this.Text = "Downloads & Package Manager";
            this.Size = new Size(1020, 680);
            this.MinimumSize = new Size(880, 580);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppTheme.Current.Colors.Background;
            this.ForeColor = AppTheme.Current.Colors.TextPrimary;

            InitializeStructuredLayout();
            _ = LoadManifestAsync();
        }

        private void InitializeStructuredLayout()
        {
            this.Controls.Clear();

            // -----------------------------------------------------------------
            // Root Container (1 Column, 3 Rows: Header 50px, MainSplit 100%, Status 42px)
            // -----------------------------------------------------------------
            _tblRoot = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 3,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = AppTheme.Current.Colors.Background
            };
            _tblRoot.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _tblRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            _tblRoot.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            _tblRoot.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));

            // -----------------------------------------------------------------
            // 1. Header (Page title, Search, Check for Updates, Install Selected)
            // -----------------------------------------------------------------
            var pnlHeaderHost = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = AppTheme.Current.Colors.TopBarBackground
            };

            _tblHeader = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(24, 8, 24, 8),
                BackColor = AppTheme.Current.Colors.TopBarBackground
            };
            _tblHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _tblHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _tblHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            _tblHeader.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            _lblPageTitle = new Label
            {
                Text = "📦 Package Downloads",
                Font = AppTheme.Current.Fonts.TitleMedium,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 0, 20, 0)
            };
            _tblHeader.Controls.Add(_lblPageTitle, 0, 0);

            _searchBox = new SearchBox
            {
                PlaceholderText = "Search packages...",
                Width = 280,
                MinimumSize = new Size(200, 34),
                MaximumSize = new Size(340, 34),
                Anchor = AnchorStyles.Right,
                Margin = new Padding(0, 0, 12, 0)
            };
            _searchBox.SearchTextChanged += (s, e) => ApplyFilters();
            _tblHeader.Controls.Add(_searchBox, 1, 0);

            _btnCheckUpdates = new ModernButton { Text = "🔄 Check Updates", Size = new Size(130, 34), IsPrimary = false, Anchor = AnchorStyles.Right, Margin = new Padding(0, 0, 8, 0) };
            _btnCheckUpdates.Click += btnCheckUpdates_Click;
            _tblHeader.Controls.Add(_btnCheckUpdates, 2, 0);

            _btnInstallSelected = new ModernButton { Text = "⬇️ Install Selected", Size = new Size(130, 34), IsPrimary = true, Anchor = AnchorStyles.Right, Margin = new Padding(0) };
            _btnInstallSelected.Click += btnInstallSelected_Click;
            _tblHeader.Controls.Add(_btnInstallSelected, 3, 0);

            pnlHeaderHost.Controls.Add(_tblHeader);
            _tblRoot.Controls.Add(pnlHeaderHost, 0, 0);

            // -----------------------------------------------------------------
            // 2. MainSplit (3 Columns: LeftFilter 200px, PackageList 100%, Details 280px / Collapsed)
            // -----------------------------------------------------------------
            _tblMainSplit = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = AppTheme.Current.Colors.Background
            };
            _tblMainSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));
            _tblMainSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _tblMainSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0F));

            // Left Filter Panel
            _pnlLeftFilter = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Padding = new Padding(12),
                BackColor = AppTheme.Current.Colors.SidebarBackground
            };

            var lblFilterHeader = new Label
            {
                Text = "CATEGORIES",
                Font = AppTheme.Current.Fonts.BodySmall,
                ForeColor = AppTheme.Current.Colors.TextMuted,
                Dock = DockStyle.Top,
                Height = 24
            };
            _pnlLeftFilter.Controls.Add(lblFilterHeader);

            _flpCategories = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Margin = new Padding(0, 4, 0, 16)
            };

            string[] categories = new[] { "All", "Core", "Emulator", "Plugin", "Bios", "Mod" };
            foreach (var cat in categories)
            {
                var btnCat = new ModernButton { Text = cat, Size = new Size(176, 30), IsPrimary = (cat == "All"), Margin = new Padding(0, 2, 0, 2) };
                btnCat.Click += (s, e) =>
                {
                    _activeCategory = cat;
                    foreach (ModernButton b in _flpCategories.Controls.OfType<ModernButton>())
                    {
                        b.IsPrimary = (b.Text == cat);
                    }
                    ApplyFilters();
                };
                _flpCategories.Controls.Add(btnCat);
            }
            _pnlLeftFilter.Controls.Add(_flpCategories);

            var pnlOptions = new Panel { Dock = DockStyle.Top, Height = 120, Padding = new Padding(0, 8, 0, 0) };
            _chkInstalledOnly = new CheckBox { Text = "Installed Only", ForeColor = AppTheme.Current.Colors.TextPrimary, Font = AppTheme.Current.Fonts.BodySmall, Dock = DockStyle.Top, Height = 28 };
            _chkInstalledOnly.CheckedChanged += (s, e) => ApplyFilters();

            _chkUpdateAvailable = new CheckBox { Text = "Updates Only", ForeColor = AppTheme.Current.Colors.TextPrimary, Font = AppTheme.Current.Fonts.BodySmall, Dock = DockStyle.Top, Height = 28 };
            _chkUpdateAvailable.CheckedChanged += (s, e) => ApplyFilters();

            _btnClearFilters = new ModernButton { Text = "Clear Filters", Size = new Size(176, 30), IsPrimary = false, Dock = DockStyle.Bottom };
            _btnClearFilters.Click += (s, e) =>
            {
                _activeCategory = "All";
                _chkInstalledOnly.Checked = false;
                _chkUpdateAvailable.Checked = false;
                _searchBox.SearchText = "";
                foreach (ModernButton b in _flpCategories.Controls.OfType<ModernButton>())
                {
                    b.IsPrimary = (b.Text == "All");
                }
                ApplyFilters();
            };

            pnlOptions.Controls.Add(_btnClearFilters);
            pnlOptions.Controls.Add(_chkUpdateAvailable);
            pnlOptions.Controls.Add(_chkInstalledOnly);
            _pnlLeftFilter.Controls.Add(pnlOptions);

            _tblMainSplit.Controls.Add(_pnlLeftFilter, 0, 0);

            // Package List Panel
            _pnlPackageListHost = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Padding = new Padding(12),
                AutoScroll = true,
                BackColor = AppTheme.Current.Colors.Background
            };

            _flpCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0)
            };
            _pnlPackageListHost.Controls.Add(_flpCards);

            _emptyStatePanel = new EmptyStatePanel { Visible = false, Dock = DockStyle.Top };
            _emptyStatePanel.Configure("📦", "No Packages Found", "No packages match your search filters.", "Clear Filters");
            _emptyStatePanel.ActionClicked += (s, e) => _btnClearFilters.PerformClick();
            _pnlPackageListHost.Controls.Add(_emptyStatePanel);

            _tblMainSplit.Controls.Add(_pnlPackageListHost, 1, 0);

            // Right Details Panel
            _pnlDetails = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Padding = new Padding(12),
                Visible = false,
                BackColor = AppTheme.Current.Colors.SidebarBackground
            };

            var tlpDetailsInner = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Margin = new Padding(0)
            };
            tlpDetailsInner.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            _lblDetailName = new Label { Text = "Package Details", Font = AppTheme.Current.Fonts.TitleSmall, ForeColor = AppTheme.Current.Colors.TextPrimary, Dock = DockStyle.Top, AutoSize = true };
            _lblDetailMeta = new Label { Text = "Select a package to view options.", Font = AppTheme.Current.Fonts.BodySmall, ForeColor = AppTheme.Current.Colors.TextSecondary, Dock = DockStyle.Top, AutoSize = true, Margin = new Padding(0, 4, 0, 8) };

            _txtDetailDesc = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, BackColor = AppTheme.Current.Colors.Surface, ForeColor = AppTheme.Current.Colors.TextPrimary, BorderStyle = BorderStyle.None };
            _txtDetailNotes = new TextBox { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, BackColor = AppTheme.Current.Colors.Surface, ForeColor = AppTheme.Current.Colors.TextSecondary, BorderStyle = BorderStyle.None };

            var flpActionButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
            _btnInstall = new ModernButton { Text = "⬇️ Install Package", Size = new Size(250, 34), IsPrimary = true, Margin = new Padding(0, 2, 0, 2) };
            _btnInstall.Click += btnInstall_Click;

            _btnUpdate = new ModernButton { Text = "🔄 Update Package", Size = new Size(250, 34), IsPrimary = true, Margin = new Padding(0, 2, 0, 2) };
            _btnUpdate.Click += btnUpdate_Click;

            _btnRepair = new ModernButton { Text = "🛠️ Repair Package", Size = new Size(250, 34), IsPrimary = false, Margin = new Padding(0, 2, 0, 2) };
            _btnRepair.Click += btnRepair_Click;

            _btnRemove = new ModernButton { Text = "🗑️ Uninstall", Size = new Size(250, 34), IsPrimary = false, Margin = new Padding(0, 2, 0, 2) };
            _btnRemove.Click += btnRemove_Click;

            _btnOpenFolder = new ModernButton { Text = "📁 Open Folder", Size = new Size(250, 34), IsPrimary = false, Margin = new Padding(0, 2, 0, 2) };
            _btnOpenFolder.Click += btnOpenFolder_Click;

            _btnManualInstall = new ModernButton { Text = "📦 Manual Install Archive...", Size = new Size(250, 34), IsPrimary = false, Margin = new Padding(0, 6, 0, 2) };
            _btnManualInstall.Click += btnManualInstall_Click;

            flpActionButtons.Controls.Add(_btnInstall);
            flpActionButtons.Controls.Add(_btnUpdate);
            flpActionButtons.Controls.Add(_btnRepair);
            flpActionButtons.Controls.Add(_btnRemove);
            flpActionButtons.Controls.Add(_btnOpenFolder);
            flpActionButtons.Controls.Add(_btnManualInstall);

            tlpDetailsInner.Controls.Add(_lblDetailName, 0, 0);
            tlpDetailsInner.Controls.Add(_lblDetailMeta, 0, 1);
            tlpDetailsInner.Controls.Add(_txtDetailDesc, 0, 2);
            tlpDetailsInner.Controls.Add(_txtDetailNotes, 0, 3);
            tlpDetailsInner.Controls.Add(flpActionButtons, 0, 4);

            _pnlDetails.Controls.Add(tlpDetailsInner);
            _tblMainSplit.Controls.Add(_pnlDetails, 2, 0);

            _tblRoot.Controls.Add(_tblMainSplit, 0, 1);

            // -----------------------------------------------------------------
            // 3. Bottom Status Bar (Catalog status, progress, Open Logs button)
            // -----------------------------------------------------------------
            var pnlBottomHost = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(0),
                Padding = new Padding(0),
                BackColor = AppTheme.Current.Colors.TopBarBackground
            };

            _tblBottomStatus = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Margin = new Padding(0),
                Padding = new Padding(16, 4, 16, 4),
                BackColor = AppTheme.Current.Colors.TopBarBackground
            };
            _tblBottomStatus.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            _tblBottomStatus.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 180F));
            _tblBottomStatus.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            _lblQueueStatus = new Label
            {
                Text = "Catalog up to date • Ready",
                Font = AppTheme.Current.Fonts.BodySmall,
                ForeColor = AppTheme.Current.Colors.TextSecondary,
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            _tblBottomStatus.Controls.Add(_lblQueueStatus, 0, 0);

            _pbQueueProgress = new ProgressBar
            {
                Height = 16,
                Dock = DockStyle.Fill,
                Anchor = AnchorStyles.Right,
                Value = 0
            };
            _tblBottomStatus.Controls.Add(_pbQueueProgress, 1, 0);

            _btnOpenLogs = new ModernButton
            {
                Text = "📄 Open Logs",
                Size = new Size(110, 28),
                IsPrimary = false,
                Anchor = AnchorStyles.Right,
                Margin = new Padding(8, 0, 0, 0)
            };
            _btnOpenLogs.Click += (s, e) => ViewLogsFile();
            _tblBottomStatus.Controls.Add(_btnOpenLogs, 2, 0);

            pnlBottomHost.Controls.Add(_tblBottomStatus);
            _tblRoot.Controls.Add(pnlBottomHost, 0, 2);

            this.Controls.Add(_tblRoot);
        }

        private async Task LoadManifestAsync()
        {
            _lblQueueStatus.Text = "Loading package catalog...";
            try
            {
                if (File.Exists(ManifestPath))
                {
                    string json = await File.ReadAllTextAsync(ManifestPath);
                    var list = JsonSerializer.Deserialize<List<PackageManifest>>(json);
                    _remotePackages = list ?? new List<PackageManifest>();
                }
                else
                {
                    _remotePackages = new List<PackageManifest>();
                }
                _lblQueueStatus.Text = $"Catalog loaded ({_remotePackages.Count} packages).";
            }
            catch (Exception ex)
            {
                _lblQueueStatus.Text = $"Failed to load catalog: {ex.Message}";
                _remotePackages = new List<PackageManifest>();
            }

            BuildCardsList();
            UpdateDetailPanel();
        }

        private void BuildCardsList()
        {
            _flpCards.SuspendLayout();
            _flpCards.Controls.Clear();

            int visibleCount = 0;
            foreach (var package in _remotePackages)
            {
                var local = PackageManagerService.Instance.Repository.GetById(package.id);
                if (!MatchesFilters(package, local)) continue;

                visibleCount++;
                Panel card = CreatePackageCardControl(package, local);
                _flpCards.Controls.Add(card);
            }

            _emptyStatePanel.Visible = (visibleCount == 0);
            _flpCards.ResumeLayout(true);
        }

        private Panel CreatePackageCardControl(PackageManifest package, InstalledPackage? local)
        {
            bool isSelected = (_selectedPackage != null && _selectedPackage.id == package.id);

            Panel card = new Panel
            {
                Size = new Size(330, 130),
                Margin = new Padding(6),
                Padding = new Padding(12),
                BackColor = isSelected ? AppTheme.Current.Colors.SurfaceCardSelected : AppTheme.Current.Colors.SurfaceCard,
                Cursor = Cursors.Hand
            };

            var chkSelect = new CheckBox
            {
                Location = new Point(8, 8),
                Size = new Size(18, 18),
                Checked = _bulkSelectionIds.Contains(package.id)
            };
            chkSelect.CheckedChanged += (s, e) =>
            {
                if (chkSelect.Checked && !_bulkSelectionIds.Contains(package.id))
                    _bulkSelectionIds.Add(package.id);
                else if (!chkSelect.Checked)
                    _bulkSelectionIds.Remove(package.id);
            };
            card.Controls.Add(chkSelect);

            Label lblName = new Label
            {
                Text = package.name,
                Font = AppTheme.Current.Fonts.TitleSmall,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Location = new Point(32, 8),
                Size = new Size(180, 22),
                AutoEllipsis = true
            };
            lblName.Click += (s, e) => SelectPackage(package);
            card.Controls.Add(lblName);

            StatusType chipStatus = StatusType.Info;
            string statusLabel = "Not Installed";
            string installedVer = local != null ? local.installedVersion : "—";

            if (local != null)
            {
                bool healthy = PackageManagerService.Instance.VerifyHealth(package.id);
                if (!healthy)
                {
                    chipStatus = StatusType.Error;
                    statusLabel = "Failed";
                }
                else if (PackageManagerService.Instance.UpdateService.IsUpdateAvailable(local.installedVersion, package.version))
                {
                    chipStatus = StatusType.Warning;
                    statusLabel = "Update Available";
                }
                else
                {
                    chipStatus = StatusType.Success;
                    statusLabel = "Installed";
                }
            }

            var chip = new StatusChip
            {
                Text = statusLabel,
                StatusType = chipStatus,
                Location = new Point(216, 8),
                Size = new Size(100, 22)
            };
            chip.Click += (s, e) => SelectPackage(package);
            card.Controls.Add(chip);

            Label lblDesc = new Label
            {
                Text = package.description,
                Font = AppTheme.Current.Fonts.BodySmall,
                ForeColor = AppTheme.Current.Colors.TextSecondary,
                Location = new Point(32, 34),
                Size = new Size(284, 40),
                AutoEllipsis = true
            };
            lblDesc.Click += (s, e) => SelectPackage(package);
            card.Controls.Add(lblDesc);

            string sizeStr = $"{package.downloadSize / 1024.0 / 1024.0:F1} MB";
            Label lblFooter = new Label
            {
                Text = $"{package.packageType} • Size: {sizeStr} • Local: {installedVer} | Remote: {package.version}",
                Font = AppTheme.Current.Fonts.BodySmall,
                ForeColor = AppTheme.Current.Colors.TextMuted,
                Location = new Point(32, 80),
                Size = new Size(284, 38),
                AutoEllipsis = true
            };
            lblFooter.Click += (s, e) => SelectPackage(package);
            card.Controls.Add(lblFooter);

            card.Click += (s, e) => SelectPackage(package);
            return card;
        }

        private bool MatchesFilters(PackageManifest package, InstalledPackage? local)
        {
            if (_activeCategory != "All" && !string.Equals(package.packageType.ToString(), _activeCategory, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string searchVal = _searchBox.SearchText.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchVal))
            {
                if (!package.name.ToLower().Contains(searchVal) && !package.description.ToLower().Contains(searchVal))
                {
                    return false;
                }
            }

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
            _tblMainSplit.ColumnStyles[2].Width = 280F;
            _pnlDetails.Visible = true;
            UpdateDetailPanel();
            BuildCardsList();
        }

        private void UpdateDetailPanel()
        {
            if (_selectedPackage == null)
            {
                _tblMainSplit.ColumnStyles[2].Width = 0F;
                _pnlDetails.Visible = false;
                return;
            }

            _lblDetailName.Text = _selectedPackage.name;
            string sizeStr = $"{_selectedPackage.downloadSize / 1024.0 / 1024.0:F1} MB";
            _lblDetailMeta.Text = $"{_selectedPackage.packageType} • {sizeStr}\nAuthor: {_selectedPackage.author}";
            _txtDetailDesc.Text = _selectedPackage.description;
            _txtDetailNotes.Text = !string.IsNullOrEmpty(_selectedPackage.releaseNotes) ? _selectedPackage.releaseNotes : "No release notes available.";

            ToggleButtons(true);
        }

        private void ToggleButtons(bool hasSelection)
        {
            _btnInstall.Visible = false;
            _btnUpdate.Visible = false;
            _btnRepair.Visible = false;
            _btnRemove.Visible = false;
            _btnOpenFolder.Visible = false;

            if (!hasSelection || _selectedPackage == null) return;

            var local = PackageManagerService.Instance.Repository.GetById(_selectedPackage.id);
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
                    _btnRepair.Visible = true;
                    _btnRemove.Visible = true;
                }

                _btnOpenFolder.Visible = true;
            }
        }

        // --- Operations & Event Handlers ---
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
                _selectedPackage = null;
                BuildCardsList();
                UpdateDetailPanel();
            }
        }

        private void btnOpenFolder_Click(object? sender, EventArgs e)
        {
            if (_selectedPackage == null) return;
            var local = PackageManagerService.Instance.Repository.GetById(_selectedPackage.id);
            if (local != null)
            {
                string path = ApplicationPaths.ResolveWritablePath(local.installedPath);
                if (Directory.Exists(path))
                {
                    try
                    {
                        Process.Start("explorer.exe", $"\"{path}\"");
                    }
                    catch (Exception ex)
                    {
                        ToastNotification.ShowToast(this, $"Could not open folder: {ex.Message}", StatusType.Error);
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

        private async void btnInstallSelected_Click(object? sender, EventArgs e)
        {
            if (_bulkSelectionIds.Count == 0)
            {
                ToastNotification.ShowToast(this, "No packages selected for bulk installation.", StatusType.Warning);
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
            ToastNotification.ShowToast(this, "Package catalog successfully updated.", StatusType.Success);
        }

        private async Task RunOperationAsync(PackageManifest package, Func<IProgress<int>, CancellationToken, Task> action)
        {
            this.Enabled = false;
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
                _pbQueueProgress.Value = 100;
                BuildCardsList();
                UpdateDetailPanel();
            }
        }

        private void ViewLogsFile()
        {
            string logs = Path.Combine(ApplicationPaths.LogsDir, "package_manager.log");
            if (File.Exists(logs))
            {
                try
                {
                    Process.Start("notepad.exe", $"\"{logs}\"");
                }
                catch (Exception ex)
                {
                    ToastNotification.ShowToast(this, $"Could not open logs: {ex.Message}", StatusType.Error);
                }
            }
            else
            {
                ToastNotification.ShowToast(this, "No package logs found.", StatusType.Info);
            }
        }
    }
}
