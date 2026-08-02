using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Controls
{
    public class EmulatorCard : UserControl
    {
        public EmulatorItem Emulator { get; private set; }

        public event EventHandler<EmulatorInstallationOperation>? ActionRequested;
        public event EventHandler? SyncBiosRequested;
        public event EventHandler? SyncControllersRequested;
        public event EventHandler? OpenFolderRequested;
        public event EventHandler? DuckStationApiRequested;

        private readonly Label _lblName;
        private readonly Label _lblConsole;
        private readonly Label _lblPath;
        private readonly StatusChip _chipInstall;
        private readonly StatusChip _chipFirmware;
        private readonly StatusChip _chipHealth;

        private readonly FlowLayoutPanel _flpActions;
        private readonly ModernButton _btnPrimary;
        private readonly ModernButton _btnSecondary;
        private readonly ModernButton _btnSyncBios;
        private readonly ModernButton _btnSyncControllers;
        private readonly ModernButton _btnOpenFolder;
        private readonly ModernButton _btnDuckStationApi;

        public EmulatorCard(EmulatorItem emu)
        {
            Emulator = emu;

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
            Size = new Size(340, 220);
            Padding = new Padding(12);
            Margin = new Padding(8);
            BackColor = AppTheme.Current.Colors.SurfaceCard;

            var tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Margin = new Padding(0)
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header (Name & Console)
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Chips Row
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Executable path
            tlpMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Spacer
            tlpMain.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Actions Row

            // 1. Header Table (Icon + Name + Console)
            var tblHeader = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 2,
                Margin = new Padding(0, 0, 0, 8)
            };
            tblHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
            tblHeader.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            var lblIcon = new Label
            {
                Text = "🎮",
                Font = new Font("Segoe UI Emoji", 16F),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Margin = new Padding(0)
            };
            tblHeader.Controls.Add(lblIcon, 0, 0);
            tblHeader.SetRowSpan(lblIcon, 2);

            _lblName = new Label
            {
                Text = emu.Name,
                Font = AppTheme.Current.Fonts.TitleSmall,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(4, 0, 0, 2)
            };
            tblHeader.Controls.Add(_lblName, 1, 0);

            _lblConsole = new Label
            {
                Text = (emu.SupportedPlatforms != null && emu.SupportedPlatforms.Count > 0) ? string.Join(", ", emu.SupportedPlatforms) : "Multi-Platform Emulator",
                Font = AppTheme.Current.Fonts.BodySmall,
                ForeColor = AppTheme.Current.Colors.TextSecondary,
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(4, 0, 0, 0)
            };
            tblHeader.Controls.Add(_lblConsole, 1, 1);

            tlpMain.Controls.Add(tblHeader, 0, 0);

            // 2. Status Chips Row
            var flpChips = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0, 0, 0, 8)
            };

            _chipInstall = new StatusChip { Text = "Not Installed", StatusType = StatusType.Warning };
            _chipFirmware = new StatusChip { Text = "Firmware OK", StatusType = StatusType.Success };
            _chipHealth = new StatusChip { Text = "Healthy", StatusType = StatusType.Success };

            flpChips.Controls.Add(_chipInstall);
            flpChips.Controls.Add(_chipFirmware);
            flpChips.Controls.Add(_chipHealth);

            tlpMain.Controls.Add(flpChips, 0, 1);

            // 3. Executable Path
            _lblPath = new Label
            {
                Text = string.IsNullOrEmpty(emu.Path) ? "Path: Not Configured" : $"Exe: {Path.GetFileName(emu.Path)}",
                Font = AppTheme.Current.Fonts.BodySmall,
                ForeColor = AppTheme.Current.Colors.TextMuted,
                AutoSize = true,
                Dock = DockStyle.Top,
                Margin = new Padding(0, 0, 0, 8)
            };
            tlpMain.Controls.Add(_lblPath, 0, 2);

            // 4. Action Buttons Container
            _flpActions = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0)
            };

            _btnPrimary = new ModernButton { Text = "Install", IsPrimary = true, Size = new Size(90, 32) };
            _btnPrimary.Click += (s, e) => HandlePrimaryClick();

            _btnSecondary = new ModernButton { Text = "Reinstall", IsPrimary = false, Size = new Size(85, 32) };
            _btnSecondary.Click += (s, e) => ActionRequested?.Invoke(this, EmulatorInstallationOperation.Reinstall);

            _btnSyncBios = new ModernButton { Text = "🔄 BIOS", IsPrimary = false, Size = new Size(80, 32) };
            _btnSyncBios.Click += (s, e) => SyncBiosRequested?.Invoke(this, EventArgs.Empty);

            _btnSyncControllers = new ModernButton { Text = "🎮 Controls", IsPrimary = false, Size = new Size(95, 32) };
            _btnSyncControllers.Click += (s, e) => SyncControllersRequested?.Invoke(this, EventArgs.Empty);

            _btnOpenFolder = new ModernButton { Text = "📂 Folder", IsPrimary = false, Size = new Size(80, 32) };
            _btnOpenFolder.Click += (s, e) => OpenFolderRequested?.Invoke(this, EventArgs.Empty);

            _btnDuckStationApi = new ModernButton { Text = "🔌 API", IsPrimary = false, Size = new Size(70, 32), Visible = false };
            _btnDuckStationApi.Click += (s, e) => DuckStationApiRequested?.Invoke(this, EventArgs.Empty);

            _flpActions.Controls.Add(_btnPrimary);
            _flpActions.Controls.Add(_btnSecondary);
            _flpActions.Controls.Add(_btnSyncBios);
            _flpActions.Controls.Add(_btnSyncControllers);
            _flpActions.Controls.Add(_btnOpenFolder);
            _flpActions.Controls.Add(_btnDuckStationApi);

            tlpMain.Controls.Add(_flpActions, 0, 4);

            Controls.Add(tlpMain);

            UpdateState();
        }

        public void UpdateState()
        {
            bool isInstalled = EmulatorManager.IsEmulatorInstalled(Emulator);
            bool isUpdateAvailable = isInstalled && !string.IsNullOrEmpty(Emulator.LatestVersion) && !string.IsNullOrEmpty(Emulator.InstalledVersion)
                && EmulatorManager.IsUpdateAvailable(Emulator.Id, Emulator.InstalledVersion, Emulator.LatestVersion);

            if (isInstalled)
            {
                if (isUpdateAvailable)
                {
                    _chipInstall.Text = "Update Available";
                    _chipInstall.StatusType = StatusType.Warning;
                    _btnPrimary.Text = "Update";
                }
                else
                {
                    _chipInstall.Text = "Installed";
                    _chipInstall.StatusType = StatusType.Success;
                    _btnPrimary.Text = "Launch";
                }
                _btnSecondary.Visible = true;
                _btnSecondary.Text = "Reinstall";
                _btnOpenFolder.Visible = true;
            }
            else
            {
                _chipInstall.Text = "Not Installed";
                _chipInstall.StatusType = StatusType.Error;
                _btnPrimary.Text = "Install";
                _btnSecondary.Visible = false;
                _btnOpenFolder.Visible = false;
            }

            // DuckStation API button only visible for DuckStation
            _btnDuckStationApi.Visible = string.Equals(Emulator.Id, "duckstation", StringComparison.OrdinalIgnoreCase);
        }

        private void HandlePrimaryClick()
        {
            bool isInstalled = EmulatorManager.IsEmulatorInstalled(Emulator);
            bool isUpdateAvailable = isInstalled && !string.IsNullOrEmpty(Emulator.LatestVersion) && !string.IsNullOrEmpty(Emulator.InstalledVersion)
                && EmulatorManager.IsUpdateAvailable(Emulator.Id, Emulator.InstalledVersion, Emulator.LatestVersion);

            if (!isInstalled)
            {
                ActionRequested?.Invoke(this, EmulatorInstallationOperation.Install);
            }
            else if (isUpdateAvailable)
            {
                ActionRequested?.Invoke(this, EmulatorInstallationOperation.Update);
            }
            else
            {
                // Launch emulator
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, Emulator.Path)),
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not launch emulator: {ex.Message}", "Launch Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}
