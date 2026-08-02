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

        private SplitContainer splitContainer = null!;
        private Button btnCancel = null!;
        private Button btnInstall = null!;
        private Button btnReinstall = null!;
        private Button btnRepair = null!;
        private Button btnUpdate = null!;
        private Button btnUninstall = null!;
        private Button btnOpenFolder = null!;
        private Button btnSyncBios = null!;
        private Button btnSyncAllBios = null!;
        private Button btnApplyControllerProfile = null!;
        private CheckBox chkAutoSyncController = null!;
        private Button btnImportControllerSettings = null!;
        private Button btnExportControllerSettings = null!;
        private Button btnSyncAllControllers = null!;
        private Button btnDuckStationApi = null!;
        private Label lblChannel = null!;
        private ComboBox cbChannel = null!;
        private Label lblBiosHeader = null!;
        private Label lblBiosStatusVal = null!;
        private Label lblLastUpdateHeader = null!;
        private Label lblLastUpdateVal = null!;
        private TableLayoutPanel pnlProgress = null!;
        private FlowLayoutPanel flpActionButtons = null!;
        private FlowLayoutPanel flpControllerButtons = null!;
        private FlowLayoutPanel flpFooterButtons = null!;
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
            this.ClientSize = new Size(820, 600);
            this.MinimumSize = new Size(760, 560);
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(20, 20, 25);
            this.ForeColor = Color.White;

            this.Controls.Clear();

            // Root layout: SplitContainer dividing Left List/Global buttons and Right Detail panel
            splitContainer = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 230,
                SplitterWidth = 6,
                BackColor = Color.FromArgb(30, 30, 38)
            };
            this.Controls.Add(splitContainer);

            // -------------------------------------------------------------
            // LEFT PANEL SETUP
            // -------------------------------------------------------------
            var pnlLeft = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(8),
                BackColor = Color.FromArgb(20, 20, 25)
            };
            pnlLeft.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlLeft.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlLeft.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlLeft.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlLeft.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var lblEmulatorsHeader = new Label
            {
                Text = "Emulators",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 6)
            };
            pnlLeft.Controls.Add(lblEmulatorsHeader, 0, 0);

            lbEmulators.Dock = DockStyle.Fill;
            lbEmulators.DrawMode = DrawMode.OwnerDrawFixed;
            lbEmulators.ItemHeight = 44;
            lbEmulators.DrawItem += lbEmulators_DrawItem;
            lbEmulators.SelectedIndexChanged += lbEmulators_SelectedIndexChanged;
            pnlLeft.Controls.Add(lbEmulators, 0, 1);

            var flpLeftAddRemove = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0, 6, 0, 6)
            };

            btnAdd = new Button
            {
                Text = "➕ Add",
                Size = new Size(100, 32),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(16, 185, 129),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Margin = new Padding(0, 0, 6, 0)
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += btnAdd_Click;

            btnRemove = new Button
            {
                Text = "❌ Remove",
                Size = new Size(100, 32),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(239, 68, 68),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Margin = new Padding(0)
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += btnRemove_Click;

            flpLeftAddRemove.Controls.Add(btnAdd);
            flpLeftAddRemove.Controls.Add(btnRemove);
            pnlLeft.Controls.Add(flpLeftAddRemove, 0, 2);

            var flpLeftGlobals = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0)
            };

            Button btnHealthCheck = new Button
            {
                Text = "🔍 Health",
                Size = new Size(66, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(79, 70, 229),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Margin = new Padding(0, 0, 4, 4)
            };
            btnHealthCheck.FlatAppearance.BorderSize = 0;
            btnHealthCheck.Click += btnHealthCheck_Click;

            btnSyncAllBios = new Button
            {
                Text = "🔄 BIOS",
                Size = new Size(66, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(16, 185, 129),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Margin = new Padding(0, 0, 4, 4)
            };
            btnSyncAllBios.FlatAppearance.BorderSize = 0;
            btnSyncAllBios.Click += btnSyncAllBios_Click;

            btnSyncAllControllers = new Button
            {
                Text = "🎮 Controls",
                Size = new Size(72, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(99, 102, 241),
                Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 4)
            };
            btnSyncAllControllers.FlatAppearance.BorderSize = 0;
            btnSyncAllControllers.Click += btnSyncAllControllers_Click;

            flpLeftGlobals.Controls.Add(btnHealthCheck);
            flpLeftGlobals.Controls.Add(btnSyncAllBios);
            flpLeftGlobals.Controls.Add(btnSyncAllControllers);
            pnlLeft.Controls.Add(flpLeftGlobals, 0, 3);

            splitContainer.Panel1.Controls.Add(pnlLeft);

            // -------------------------------------------------------------
            // RIGHT PANEL SETUP
            // -------------------------------------------------------------
            var pnlRightMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                ColumnCount = 1,
                RowCount = 8,
                Padding = new Padding(12),
                BackColor = Color.FromArgb(24, 24, 30)
            };
            pnlRightMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            pnlRightMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlRightMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlRightMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlRightMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlRightMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlRightMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            pnlRightMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            pnlRightMain.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // --- Row 0: Details Grid ---
            var tblDetailsGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 7,
                Margin = new Padding(0, 0, 0, 8)
            };
            tblDetailsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 115F));
            tblDetailsGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            for (int r = 0; r < 7; r++)
                tblDetailsGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            // Name
            lblName = new Label
            {
                Text = "Name:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175),
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(0, 6, 4, 6)
            };
            tbName = new TextBox
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(44, 44, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(0, 4, 0, 6)
            };
            tbName.TextChanged += (s, e) => SaveSelectedFields();
            tblDetailsGrid.Controls.Add(lblName, 0, 0);
            tblDetailsGrid.Controls.Add(tbName, 1, 0);

            // Path + Browse
            lblPath = new Label
            {
                Text = "Executable:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175),
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(0, 6, 4, 6)
            };

            var pnlPathBrowse = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 6)
            };
            pnlPathBrowse.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlPathBrowse.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            tbPath = new TextBox
            {
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(44, 44, 52),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9.5F),
                Margin = new Padding(0, 0, 4, 0)
            };
            tbPath.TextChanged += (s, e) => {
                if (lbEmulators.SelectedItem is EmulatorItem selectedEmu)
                {
                    UpdateDetectedStatus(selectedEmu);
                }
                SaveSelectedFields();
            };

            btnBrowse = new Button
            {
                Text = "...",
                Size = new Size(38, 26),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(0)
            };
            btnBrowse.FlatAppearance.BorderSize = 0;
            btnBrowse.Click += btnBrowse_Click;

            pnlPathBrowse.Controls.Add(tbPath, 0, 0);
            pnlPathBrowse.Controls.Add(btnBrowse, 1, 0);

            tblDetailsGrid.Controls.Add(lblPath, 0, 1);
            tblDetailsGrid.Controls.Add(pnlPathBrowse, 1, 1);

            // Version
            lblVersionHeader = new Label
            {
                Text = "Version Info:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175),
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(0, 6, 4, 6)
            };
            lblVersion = new Label
            {
                Text = "Unknown",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.White,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(0, 6, 0, 6)
            };
            tblDetailsGrid.Controls.Add(lblVersionHeader, 0, 2);
            tblDetailsGrid.Controls.Add(lblVersion, 1, 2);

            // BIOS Status & Sync
            lblBiosHeader = new Label
            {
                Text = "BIOS/Firmware:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175),
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(0, 6, 4, 6)
            };

            var flpBios = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0, 2, 0, 6)
            };

            lblBiosStatusVal = new Label
            {
                Text = "Checking...",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Margin = new Padding(0, 4, 12, 0)
            };

            btnSyncBios = new Button
            {
                Text = "🔄 Sync BIOS",
                Size = new Size(110, 28),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(79, 70, 229),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Margin = new Padding(0)
            };
            btnSyncBios.FlatAppearance.BorderSize = 0;
            btnSyncBios.Click += btnSyncBios_Click;

            flpBios.Controls.Add(lblBiosStatusVal);
            flpBios.Controls.Add(btnSyncBios);

            tblDetailsGrid.Controls.Add(lblBiosHeader, 0, 3);
            tblDetailsGrid.Controls.Add(flpBios, 1, 3);

            // Default Console
            lblDefaultHeader = new Label
            {
                Text = "Default For:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175),
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(0, 6, 4, 6)
            };
            cbDefaultConsole = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(44, 44, 52),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 4, 0, 6)
            };
            cbDefaultConsole.SelectedIndexChanged += cbDefaultConsole_SelectedIndexChanged;

            tblDetailsGrid.Controls.Add(lblDefaultHeader, 0, 4);
            tblDetailsGrid.Controls.Add(cbDefaultConsole, 1, 4);

            // Channel
            lblChannel = new Label
            {
                Text = "Release Channel:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175),
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(0, 6, 4, 6)
            };
            cbChannel = new ComboBox
            {
                Dock = DockStyle.Top,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(44, 44, 52),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Margin = new Padding(0, 4, 0, 6)
            };
            cbChannel.Items.Add("Stable");
            cbChannel.Items.Add("Nightly");
            cbChannel.SelectedIndexChanged += cbChannel_SelectedIndexChanged;

            tblDetailsGrid.Controls.Add(lblChannel, 0, 5);
            tblDetailsGrid.Controls.Add(cbChannel, 1, 5);

            // Last Checked
            lblLastUpdateHeader = new Label
            {
                Text = "Last Checked:",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175),
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(0, 6, 4, 6)
            };
            lblLastUpdateVal = new Label
            {
                Text = "Never",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Regular),
                ForeColor = Color.White,
                AutoSize = true,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(0, 6, 0, 6)
            };
            tblDetailsGrid.Controls.Add(lblLastUpdateHeader, 0, 6);
            tblDetailsGrid.Controls.Add(lblLastUpdateVal, 1, 6);

            pnlRightMain.Controls.Add(tblDetailsGrid, 0, 0);

            // --- Row 1: Actions Header ---
            var lblActionsHeader = new Label
            {
                Text = "Installation & Maintenance",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(229, 231, 235),
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 4)
            };
            pnlRightMain.Controls.Add(lblActionsHeader, 0, 1);

            // --- Row 2: Action Buttons FlowLayoutPanel ---
            flpActionButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0, 0, 0, 8)
            };

            btnTestLaunch = new Button
            {
                Text = "🚀  Launch",
                Size = new Size(115, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(99, 102, 241),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(0, 0, 6, 6)
            };
            btnTestLaunch.FlatAppearance.BorderSize = 0;
            btnTestLaunch.Click += btnTestLaunch_Click;

            btnInstall = new Button
            {
                Text = "⬇️  Install",
                Size = new Size(125, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(16, 185, 129),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(0, 0, 6, 6)
            };
            btnInstall.FlatAppearance.BorderSize = 0;
            btnInstall.Click += async (s, e) => {
                if (btnInstall.Text.Contains("Update"))
                    await ExecuteOperationAsync(EmulatorInstallationOperation.Update);
                else if (btnInstall.Text.Contains("Reinstall"))
                    await ExecuteOperationAsync(EmulatorInstallationOperation.Reinstall);
                else
                    await ExecuteOperationAsync(EmulatorInstallationOperation.Install);
            };

            btnReinstall = new Button
            {
                Text = "🔄  Reinstall",
                Size = new Size(115, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(99, 102, 241),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(0, 0, 6, 6)
            };
            btnReinstall.FlatAppearance.BorderSize = 0;
            btnReinstall.Click += async (s, e) => await ExecuteOperationAsync(EmulatorInstallationOperation.Reinstall);

            btnRepair = new Button
            {
                Text = "🛠️  Repair",
                Size = new Size(105, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(79, 70, 229),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(0, 0, 6, 6)
            };
            btnRepair.FlatAppearance.BorderSize = 0;
            btnRepair.Click += async (s, e) => await ExecuteOperationAsync(EmulatorInstallationOperation.Repair);

            btnUpdate = new Button
            {
                Text = "⬆️  Update",
                Size = new Size(105, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(245, 158, 11),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(0, 0, 6, 6)
            };
            btnUpdate.FlatAppearance.BorderSize = 0;
            btnUpdate.Click += async (s, e) => await ExecuteOperationAsync(EmulatorInstallationOperation.Update);

            btnUninstall = new Button
            {
                Text = "🗑️  Uninstall",
                Size = new Size(115, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(239, 68, 68),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(0, 0, 6, 6)
            };
            btnUninstall.FlatAppearance.BorderSize = 0;
            btnUninstall.Click += btnUninstall_Click;

            btnDuckStationApi = new Button
            {
                Text = "🔌 DuckStation API",
                Size = new Size(150, 34),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(79, 70, 229),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Margin = new Padding(0, 0, 6, 6),
                Visible = false
            };
            btnDuckStationApi.FlatAppearance.BorderSize = 0;
            btnDuckStationApi.Click += async (s, e) => {
                try
                {
                    var apiClient = new ApiClient();
                    var info = await apiClient.GetDuckStationPackageAsync("https://api.github.com/repos/stenzek/duckstation/releases/latest");
                    if (info != null)
                    {
                        MessageBox.Show($"DuckStation API Info:\nVersion: {info.Version}\nURL: {info.DownloadUrl}", "DuckStation API", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"DuckStation API check failed: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            flpActionButtons.Controls.Add(btnTestLaunch);
            flpActionButtons.Controls.Add(btnInstall);
            flpActionButtons.Controls.Add(btnReinstall);
            flpActionButtons.Controls.Add(btnRepair);
            flpActionButtons.Controls.Add(btnUpdate);
            flpActionButtons.Controls.Add(btnUninstall);
            flpActionButtons.Controls.Add(btnDuckStationApi);

            pnlRightMain.Controls.Add(flpActionButtons, 0, 2);

            // --- Row 3: Controller Header ---
            var lblControllerHeader = new Label
            {
                Text = "Controller Profile & Sync",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(229, 231, 235),
                AutoSize = true,
                Margin = new Padding(0, 10, 0, 4)
            };
            pnlRightMain.Controls.Add(lblControllerHeader, 0, 3);

            // --- Row 4: Controller Buttons FlowLayoutPanel ---
            flpControllerButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                WrapContents = true,
                Margin = new Padding(0, 0, 0, 8)
            };

            btnApplyControllerProfile = new Button
            {
                Text = "🎮 Apply Global Profile",
                Size = new Size(160, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(99, 102, 241),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Margin = new Padding(0, 0, 6, 6)
            };
            btnApplyControllerProfile.FlatAppearance.BorderSize = 0;
            btnApplyControllerProfile.Click += btnApplyControllerProfile_Click;

            btnImportControllerSettings = new Button
            {
                Text = "📥 Import",
                Size = new Size(90, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Margin = new Padding(0, 0, 6, 6)
            };
            btnImportControllerSettings.FlatAppearance.BorderSize = 0;
            btnImportControllerSettings.Click += btnImportControllerSettings_Click;

            btnExportControllerSettings = new Button
            {
                Text = "📤 Export",
                Size = new Size(90, 30),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(55, 65, 81),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Margin = new Padding(0, 0, 6, 6)
            };
            btnExportControllerSettings.FlatAppearance.BorderSize = 0;
            btnExportControllerSettings.Click += btnExportControllerSettings_Click;

            chkAutoSyncController = new CheckBox
            {
                Text = "Auto Sync Controller on Launch",
                Font = new Font("Segoe UI", 8.5F),
                ForeColor = Color.FromArgb(209, 213, 219),
                AutoSize = true,
                Margin = new Padding(0, 4, 0, 6)
            };
            chkAutoSyncController.CheckedChanged += chkAutoSyncController_CheckedChanged;

            flpControllerButtons.Controls.Add(btnApplyControllerProfile);
            flpControllerButtons.Controls.Add(btnImportControllerSettings);
            flpControllerButtons.Controls.Add(btnExportControllerSettings);
            flpControllerButtons.Controls.Add(chkAutoSyncController);

            pnlRightMain.Controls.Add(flpControllerButtons, 0, 4);

            // --- Row 5: Dedicated Progress TableLayoutPanel ---
            pnlProgress = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 3,
                RowCount = 2,
                Margin = new Padding(0, 6, 0, 10),
                Visible = false
            };
            pnlProgress.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            pnlProgress.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            pnlProgress.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            pbProgress = new ProgressBar
            {
                Dock = DockStyle.Top,
                Height = 18,
                Margin = new Padding(0, 0, 8, 4)
            };

            btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(70, 26),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(107, 114, 128),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Margin = new Padding(0)
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += btnCancel_Click;

            lblStatus = new Label
            {
                Text = "Ready",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(156, 163, 175),
                AutoSize = true,
                Margin = new Padding(0, 2, 0, 0)
            };

            pnlProgress.Controls.Add(pbProgress, 0, 0);
            pnlProgress.SetColumnSpan(pbProgress, 2);
            pnlProgress.Controls.Add(btnCancel, 2, 0);
            pnlProgress.Controls.Add(lblStatus, 0, 1);
            pnlProgress.SetColumnSpan(lblStatus, 3);

            pnlRightMain.Controls.Add(pnlProgress, 0, 5);

            // --- Row 7: Footer Bar ---
            flpFooterButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                FlowDirection = FlowDirection.RightToLeft,
                AutoSize = true,
                Margin = new Padding(0, 12, 0, 0)
            };

            btnSaveClose = new Button
            {
                Text = "💾 Save & Close",
                Size = new Size(125, 36),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(16, 185, 129),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(6, 0, 0, 0)
            };
            btnSaveClose.FlatAppearance.BorderSize = 0;
            btnSaveClose.Click += btnSaveClose_Click;

            btnOpenFolder = new Button
            {
                Text = "📂 Folder",
                Size = new Size(95, 36),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(107, 114, 128),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Margin = new Padding(0)
            };
            btnOpenFolder.FlatAppearance.BorderSize = 0;
            btnOpenFolder.Click += btnOpenFolder_Click;

            flpFooterButtons.Controls.Add(btnSaveClose);
            flpFooterButtons.Controls.Add(btnOpenFolder);

            pnlRightMain.Controls.Add(flpFooterButtons, 0, 7);

            splitContainer.Panel2.Controls.Add(pnlRightMain);

            // Hover styles
            SetupHover(btnSaveClose, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105));
            SetupHover(btnTestLaunch, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupHover(btnInstall, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105));
            SetupHover(btnReinstall, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupHover(btnRepair, Color.FromArgb(79, 70, 229), Color.FromArgb(67, 56, 202));
            SetupHover(btnUpdate, Color.FromArgb(245, 158, 11), Color.FromArgb(217, 119, 6));
            SetupHover(btnAdd, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105));
            SetupHover(btnRemove, Color.FromArgb(239, 68, 68), Color.FromArgb(220, 38, 38));
            SetupHover(btnCancel, Color.FromArgb(107, 114, 128), Color.FromArgb(75, 85, 99));
            SetupHover(btnUninstall, Color.FromArgb(239, 68, 68), Color.FromArgb(220, 38, 38));
            SetupHover(btnOpenFolder, Color.FromArgb(107, 114, 128), Color.FromArgb(75, 85, 99));
            SetupHover(btnHealthCheck, Color.FromArgb(79, 70, 229), Color.FromArgb(67, 56, 202));
            SetupHover(btnSyncBios, Color.FromArgb(79, 70, 229), Color.FromArgb(67, 56, 202));
            SetupHover(btnSyncAllBios, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105));
            SetupHover(btnApplyControllerProfile, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupHover(btnImportControllerSettings, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupHover(btnExportControllerSettings, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupHover(btnSyncAllControllers, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupHover(btnDuckStationApi, Color.FromArgb(79, 70, 229), Color.FromArgb(67, 56, 202));
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
            var provider = new JsonEmulatorPackageDefinitionProvider();
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
            var provider = new JsonEmulatorPackageDefinitionProvider();
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
                        var listRes = await githubService.GetReleasesAsync(definition.GitHubOwner, definition.GitHubRepository, CancellationToken.None);
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
                        var latestRes = await githubService.GetLatestReleaseAsync(definition.GitHubOwner, definition.GitHubRepository, CancellationToken.None);
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

            chkAutoSyncController.Checked = emu.AutoSyncController;

            _isUpdatingSelection = false;
        }

        private void ClearDetails()
        {
            _isUpdatingSelection = true;
            chkAutoSyncController.Checked = false;
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
                btnSyncBios.Visible = (biosStatus == "Present");
            }
            else
            {
                lblBiosStatusVal.ForeColor = Color.FromArgb(245, 158, 11); // Yellow/Orange
                btnSyncBios.Visible = true;
            }

            // Emulator-specific buttons (Requirement 8)
            bool isDuckStation = string.Equals(emu.Id, "duckstation", StringComparison.OrdinalIgnoreCase);
            btnDuckStationApi.Visible = isDuckStation;

            // Last Update Check
            lblLastUpdateVal.Text = _lastUpdateCheck == DateTime.MinValue ? "Never" : _lastUpdateCheck.ToString("g");

            UpdateButtonActions(emu, status);
        }

        private void UpdateButtonActions(EmulatorItem emu, string status)
        {
            bool isInstalled = EmulatorManager.IsEmulatorInstalled(emu);
            bool isUpdateAvailable = isInstalled && !string.IsNullOrEmpty(emu.LatestVersion) && !string.IsNullOrEmpty(emu.InstalledVersion)
                && EmulatorManager.IsUpdateAvailable(emu.Id, emu.InstalledVersion, emu.LatestVersion)
                && emu.LatestVersion != "Update check unavailable";

            if (_isInstalling)
            {
                btnInstall.Text = "⏳  Installing...";
                btnInstall.Enabled = false;
                btnReinstall.Enabled = false;
                btnRepair.Enabled = false;
                btnUpdate.Enabled = false;
                btnUninstall.Enabled = false;
                btnTestLaunch.Enabled = false;
                btnBrowse.Enabled = false;
                btnOpenFolder.Enabled = false;

                pnlProgress.Visible = true;
                btnCancel.Visible = true;
                return;
            }

            pnlProgress.Visible = false;
            btnCancel.Visible = false;
            btnBrowse.Enabled = true;

            if (!isInstalled)
            {
                // NOT INSTALLED: Show Install button dynamically ("⬇️  Install")
                btnInstall.Text = "⬇️  Install";
                btnInstall.Visible = true;
                btnInstall.Enabled = true;

                btnTestLaunch.Visible = false;
                btnReinstall.Visible = false;
                btnRepair.Visible = false;
                btnUpdate.Visible = false;
                btnUninstall.Visible = false;
                btnOpenFolder.Enabled = false;
            }
            else
            {
                // INSTALLED: Show Launch, Repair, Reinstall, Uninstall.
                btnTestLaunch.Visible = true;
                btnTestLaunch.Enabled = true;

                btnReinstall.Visible = true;
                btnReinstall.Enabled = true;

                btnRepair.Visible = true;
                btnRepair.Enabled = true;

                btnUninstall.Visible = true;
                btnUninstall.Enabled = true;

                btnOpenFolder.Enabled = true;

                if (isUpdateAvailable)
                {
                    btnInstall.Text = "⬆️  Update";
                    btnInstall.Visible = true;
                    btnInstall.Enabled = true;
                    btnUpdate.Visible = true;
                    btnUpdate.Enabled = true;
                }
                else
                {
                    btnInstall.Text = "✅  Installed";
                    btnInstall.Visible = true;
                    btnInstall.Enabled = false;
                    btnUpdate.Visible = false;
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

        private async void btnUninstall_Click(object? sender, EventArgs e)
        {
            if (lbEmulators.SelectedItem is not EmulatorItem selectedEmu) return;

            var confirmResult = MessageBox.Show(
                $"Are you sure you want to uninstall application files for '{selectedEmu.Name}'?",
                "Confirm Uninstall",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (confirmResult != DialogResult.Yes) return;

            var keepResult = MessageBox.Show(
                "Do you want to keep your user configuration and save data (e.g. saves, memory cards, screenshots, config files)?",
                "Keep Settings & Saves?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question
            );

            if (keepResult == DialogResult.Cancel) return;

            bool keepData = (keepResult == DialogResult.Yes);

            btnUninstall.Enabled = false;
            lblStatus.Text = "Uninstalling...";

            try
            {
                var req = new EmulatorInstallationRequest
                {
                    EmulatorId = selectedEmu.Id,
                    Operation = EmulatorInstallationOperation.Uninstall,
                    UninstallKeepUserData = keepData,
                    CancellationToken = CancellationToken.None
                };

                var uninstallResult = await _installationService.UninstallAsync(req);
                if (uninstallResult.Success)
                {
                    selectedEmu.InstalledVersion = "";
                    selectedEmu.Status = "Missing";
                    selectedEmu.Path = "";

                    // Reload settings
                    _config = EmulatorManager.LoadConfig();
                    
                    var updatedEmu = _config.Emulators.FirstOrDefault(x => string.Equals(x.Id, selectedEmu.Id, StringComparison.OrdinalIgnoreCase));
                    if (updatedEmu != null)
                    {
                        selectedEmu = updatedEmu;
                    }

                    UpdateDetectedStatus(selectedEmu);
                    RefreshList();
                    MessageBox.Show($"{selectedEmu.Name} files uninstalled successfully.", "Uninstall Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Uninstall failed:\n{uninstallResult.ErrorMessage}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Uninstall failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnUninstall.Enabled = true;
                lblStatus.Text = "";
                UpdateDetectedStatus(selectedEmu);
                RefreshList();
            }
        }

        private async Task ExecuteOperationAsync(EmulatorInstallationOperation operation)
        {
            if (lbEmulators.SelectedItem is not EmulatorItem selectedEmu) return;
            if (_isInstalling) return;
            _isInstalling = true;

            // Check if manually configured when performing clean Install
            string resolved = ResolvePath(selectedEmu.Path);
            if (File.Exists(resolved) && operation == EmulatorInstallationOperation.Install)
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

            // Disable action buttons
            btnInstall.Enabled = false;
            btnReinstall.Enabled = false;
            btnRepair.Enabled = false;
            btnUpdate.Enabled = false;
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
                    Operation = operation,
                    Progress = progress,
                    CancellationToken = _cts.Token
                };

                var installResult = await _installationService.InstallAsync(installReq);

                if (installResult.Success)
                {
                    lblStatus.Text = "Operation complete!";
                    string actionCompleted = operation.ToString();
                    MessageBox.Show($"{selectedEmu.Name} operation {actionCompleted} completed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    _config = EmulatorManager.LoadConfig();
                    
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
                    lblStatus.Text = "Operation failed.";
                    var ex = new Exception(installResult.ErrorMessage ?? "Deployment verification check failed.");
                    ShowDetailedError("Operation Error", $"Failed to execute {operation} for {selectedEmu.Name}.", ex);
                }
            }
            catch (OperationCanceledException)
            {
                lblStatus.Text = "Operation cancelled.";
                MessageBox.Show("Operation was cancelled.", "Cancelled", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                lblStatus.Text = "Operation failed.";
                ShowDetailedError("Operation Failure", $"An error occurred while executing {operation} for {selectedEmu.Name}.", ex);
            }
            finally
            {
                _cts?.Dispose();
                _cts = null;
                _isInstalling = false;

                btnSaveClose.Enabled = true;
                btnAdd.Enabled = true;
                btnRemove.Enabled = true;
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

        private async void btnSyncBios_Click(object? sender, EventArgs e)
        {
            if (lbEmulators.SelectedItem is not EmulatorItem selectedEmu) return;

            btnSyncBios.Enabled = false;
            btnSyncAllBios.Enabled = false;
            lblBiosStatusVal.Text = "Scanning BIOS files...";
            lblBiosStatusVal.ForeColor = Color.FromArgb(245, 158, 11);

            var progress = new Progress<BiosSyncProgress>(p =>
            {
                switch (p.State)
                {
                    case BiosSyncState.Scanning:
                        lblBiosStatusVal.Text = "Scanning BIOS files...";
                        lblBiosStatusVal.ForeColor = Color.FromArgb(245, 158, 11);
                        break;
                    case BiosSyncState.Syncing:
                        lblBiosStatusVal.Text = "Syncing...";
                        lblBiosStatusVal.ForeColor = Color.FromArgb(59, 130, 246);
                        break;
                    case BiosSyncState.SyncedSuccessfully:
                        lblBiosStatusVal.Text = "Synced successfully";
                        lblBiosStatusVal.ForeColor = Color.FromArgb(16, 185, 129);
                        break;
                    case BiosSyncState.NoCompatibleBiosFound:
                        lblBiosStatusVal.Text = "No compatible BIOS found";
                        lblBiosStatusVal.ForeColor = Color.FromArgb(245, 158, 11);
                        break;
                    case BiosSyncState.EmulatorNotInstalled:
                        lblBiosStatusVal.Text = "Emulator not installed";
                        lblBiosStatusVal.ForeColor = Color.FromArgb(239, 68, 68);
                        break;
                    case BiosSyncState.Failed:
                        lblBiosStatusVal.Text = "Sync failed";
                        lblBiosStatusVal.ForeColor = Color.FromArgb(239, 68, 68);
                        break;
                }
            });

            _cts = new CancellationTokenSource();

            try
            {
                var result = await BiosSynchronizationService.Instance.SyncEmulatorBiosAsync(selectedEmu.Id, progress, _cts.Token);
                UpdateBiosStatusUI(result);
                ShowSingleSyncSummaryDialog(result);
            }
            catch (OperationCanceledException)
            {
                lblBiosStatusVal.Text = "Sync cancelled";
                lblBiosStatusVal.ForeColor = Color.FromArgb(239, 68, 68);
            }
            catch (Exception ex)
            {
                lblBiosStatusVal.Text = "Sync failed";
                lblBiosStatusVal.ForeColor = Color.FromArgb(239, 68, 68);
                ShowDetailedError("BIOS Synchronization Error", $"Failed to synchronize BIOS for '{selectedEmu.Name}'.", ex);
            }
            finally
            {
                btnSyncBios.Enabled = true;
                btnSyncAllBios.Enabled = true;
                _cts?.Dispose();
                _cts = null;
            }
        }

        private async void btnSyncAllBios_Click(object? sender, EventArgs e)
        {
            btnSyncBios.Enabled = false;
            btnSyncAllBios.Enabled = false;
            
            _cts = new CancellationTokenSource();

            var progress = new Progress<BiosSyncProgress>(p =>
            {
                lblBiosStatusVal.Text = $"Syncing {p.EmulatorName}...";
            });

            try
            {
                var results = await BiosSynchronizationService.Instance.SyncAllEmulatorsBiosAsync(progress, _cts.Token);
                ShowGlobalSyncSummaryDialog(results);
                if (lbEmulators.SelectedItem is EmulatorItem current)
                {
                    var matched = results.FirstOrDefault(r => string.Equals(r.EmulatorId, current.Id, StringComparison.OrdinalIgnoreCase));
                    if (matched != null)
                    {
                        UpdateBiosStatusUI(matched);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                lblBiosStatusVal.Text = "Global sync cancelled";
            }
            catch (Exception ex)
            {
                ShowDetailedError("Global BIOS Synchronization Error", "Failed to complete global BIOS sync operation.", ex);
            }
            finally
            {
                btnSyncBios.Enabled = true;
                btnSyncAllBios.Enabled = true;
                _cts?.Dispose();
                _cts = null;
            }
        }

        private void UpdateBiosStatusUI(BiosSyncResult result)
        {
            switch (result.State)
            {
                case BiosSyncState.SyncedSuccessfully:
                    lblBiosStatusVal.Text = "Synced successfully";
                    lblBiosStatusVal.ForeColor = Color.FromArgb(16, 185, 129);
                    break;
                case BiosSyncState.NoCompatibleBiosFound:
                    lblBiosStatusVal.Text = "No compatible BIOS found";
                    lblBiosStatusVal.ForeColor = Color.FromArgb(245, 158, 11);
                    break;
                case BiosSyncState.EmulatorNotInstalled:
                    lblBiosStatusVal.Text = "Emulator not installed";
                    lblBiosStatusVal.ForeColor = Color.FromArgb(239, 68, 68);
                    break;
                case BiosSyncState.Failed:
                    lblBiosStatusVal.Text = "Sync failed";
                    lblBiosStatusVal.ForeColor = Color.FromArgb(239, 68, 68);
                    break;
                case BiosSyncState.Scanning:
                    lblBiosStatusVal.Text = "Scanning BIOS files...";
                    lblBiosStatusVal.ForeColor = Color.FromArgb(245, 158, 11);
                    break;
                case BiosSyncState.Syncing:
                    lblBiosStatusVal.Text = "Syncing...";
                    lblBiosStatusVal.ForeColor = Color.FromArgb(59, 130, 246);
                    break;
            }
        }

        private void ShowSingleSyncSummaryDialog(BiosSyncResult result)
        {
            string statusText = result.State switch
            {
                BiosSyncState.SyncedSuccessfully => "Synced successfully",
                BiosSyncState.NoCompatibleBiosFound => "No compatible BIOS found",
                BiosSyncState.EmulatorNotInstalled => "Emulator not installed",
                BiosSyncState.Failed => "Sync failed",
                _ => result.State.ToString()
            };

            string msg = $"Emulator: {result.EmulatorName}\n" +
                         $"Status: {statusText}\n" +
                         $"Copied Files: {result.CopiedCount}\n" +
                         $"Skipped Files: {result.SkippedCount}\n" +
                         $"Destination Path: {(string.IsNullOrEmpty(result.DestinationPath) ? "N/A" : result.DestinationPath)}";

            if (!string.IsNullOrEmpty(result.ErrorMessage))
            {
                msg += $"\n\nError: {result.ErrorMessage}";
            }

            MessageBoxIcon icon = result.State switch
            {
                BiosSyncState.SyncedSuccessfully => MessageBoxIcon.Information,
                BiosSyncState.NoCompatibleBiosFound => MessageBoxIcon.Warning,
                _ => MessageBoxIcon.Error
            };

            MessageBox.Show(msg, $"{result.EmulatorName} BIOS Sync Summary", MessageBoxButtons.OK, icon);
        }

        private void ShowGlobalSyncSummaryDialog(List<BiosSyncResult> results)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("Global BIOS Synchronization Summary:");
            sb.AppendLine("------------------------------------");

            foreach (var res in results)
            {
                string statusText = res.State switch
                {
                    BiosSyncState.SyncedSuccessfully => "Synced successfully",
                    BiosSyncState.NoCompatibleBiosFound => "No compatible BIOS found",
                    BiosSyncState.EmulatorNotInstalled => "Emulator not installed",
                    BiosSyncState.Failed => "Sync failed",
                    _ => res.State.ToString()
                };

                sb.AppendLine($"• {res.EmulatorName}: {statusText}");
                sb.AppendLine($"  - Copied: {res.CopiedCount}, Skipped: {res.SkippedCount}");
                sb.AppendLine($"  - Destination: {(string.IsNullOrEmpty(res.DestinationPath) ? "N/A" : res.DestinationPath)}");
                if (!string.IsNullOrEmpty(res.ErrorMessage))
                {
                    sb.AppendLine($"  - Error: {res.ErrorMessage}");
                }
                sb.AppendLine();
            }

            MessageBox.Show(sb.ToString().TrimEnd(), "Global BIOS Sync Summary", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private async void btnApplyControllerProfile_Click(object? sender, EventArgs e)
        {
            if (lbEmulators.SelectedItem is not EmulatorItem selectedEmu) return;
            btnApplyControllerProfile.Enabled = false;
            try
            {
                var res = await ControllerSyncService.Instance.ApplyGlobalProfileToEmulatorAsync(selectedEmu.Id, false, this);
                MessageBox.Show(this, res.Message, $"{selectedEmu.Name} Controller Sync", MessageBoxButtons.OK, res.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            finally
            {
                btnApplyControllerProfile.Enabled = true;
            }
        }

        private void chkAutoSyncController_CheckedChanged(object? sender, EventArgs e)
        {
            if (_isUpdatingSelection) return;
            if (lbEmulators.SelectedItem is EmulatorItem selectedEmu)
            {
                selectedEmu.AutoSyncController = chkAutoSyncController.Checked;
                EmulatorManager.SaveConfig(_config);
            }
        }

        private async void btnImportControllerSettings_Click(object? sender, EventArgs e)
        {
            if (lbEmulators.SelectedItem is not EmulatorItem selectedEmu) return;
            btnImportControllerSettings.Enabled = false;
            try
            {
                var res = await ControllerSyncService.Instance.ImportFromEmulatorAsync(selectedEmu.Id);
                MessageBox.Show(this, res.Message, "Import Controller Settings", MessageBoxButtons.OK, res.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            finally
            {
                btnImportControllerSettings.Enabled = true;
            }
        }

        private async void btnExportControllerSettings_Click(object? sender, EventArgs e)
        {
            if (lbEmulators.SelectedItem is not EmulatorItem selectedEmu) return;
            btnExportControllerSettings.Enabled = false;
            try
            {
                var res = await ControllerSyncService.Instance.ExportToEmulatorAsync(selectedEmu.Id, this);
                MessageBox.Show(this, res.Message, "Export Controller Settings", MessageBoxButtons.OK, res.Success ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            }
            finally
            {
                btnExportControllerSettings.Enabled = true;
            }
        }

        private async void btnSyncAllControllers_Click(object? sender, EventArgs e)
        {
            btnSyncAllControllers.Enabled = false;
            try
            {
                var syncResults = await ControllerSyncService.Instance.SyncAllEmulatorsAsync(this);
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Global Controller Synchronization Summary:");
                sb.AppendLine("------------------------------------------");
                foreach (var res in syncResults)
                {
                    string status = res.Success ? "✅ Success" : "❌ Failed / Skipped";
                    sb.AppendLine($"• {res.EmulatorName}: {status}");
                    if (!string.IsNullOrEmpty(res.Message)) sb.AppendLine($"  - {res.Message}");
                }
                MessageBox.Show(this, sb.ToString().TrimEnd(), "Sync All Controllers", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                btnSyncAllControllers.Enabled = true;
            }
        }
    }
}
