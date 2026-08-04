using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RetroLauncher.Core.Enums;
using RetroLauncher.Core.Models;
using RetroLauncher.Services.Controllers;
using RetroLauncher.UI.Controls;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Forms
{
    public class GlobalControllerSettingsForm : Form
    {
        private ComboBox cbPlayerSelect = null!;
        private ComboBox cbControllerType = null!;
        private CheckBox chkAutoSyncOnLaunch = null!;

        // Preset UI
        private Panel pnlKeyboardPresetBar = null!;
        private ComboBox cbPresetSelect = null!;
        private ModernButton btnApplyPreset = null!;
        private ModernButton btnResetPreset = null!;
        private ModernButton btnClearMappings = null!;

        // Test Input Banner
        private Panel pnlTestModeBanner = null!;
        private Label lblTestInputStatus = null!;
        private ModernButton btnToggleTestMode = null!;
        private bool _isTestModeActive = false;

        // Conflict Warning Banner
        private Panel pnlWarningBanner = null!;
        private Label lblWarningText = null!;

        // Tabs
        private TabControl tcMain = null!;
        private TabPage tpKeyboard = null!;
        private TabPage tpGamepad = null!;
        private TabPage tpHotkeys = null!;

        // Key Capture Mapping Dictionary for Gameplay Actions
        private Dictionary<VirtualControllerAction, KeyCaptureControl> _keyboardControls = new();

        // Key Capture Mapping Dictionary for Hotkeys
        private Dictionary<VirtualControllerAction, KeyCaptureControl> _hotkeyControls = new();

        // Gamepad Controls
        private TextBox tbDeviceGuid = null!;
        private NumericUpDown nudDeadzone = null!;
        private NumericUpDown nudSensitivity = null!;
        private NumericUpDown nudTriggerThreshold = null!;
        private CheckBox chkInvertLX = null!;
        private CheckBox chkInvertLY = null!;
        private CheckBox chkInvertRX = null!;
        private CheckBox chkInvertRY = null!;
        private CheckBox chkEnableRumble = null!;
        private NumericUpDown nudRumbleStrength = null!;
        private Dictionary<string, TextBox> _gamepadMappingInputs = new(StringComparer.OrdinalIgnoreCase);

        // Action Buttons
        private ModernButton btnSave = null!;
        private ModernButton btnSyncAll = null!;

        private int _selectedPlayerIndex = 1;
        private bool _isDirty = false;

        public GlobalControllerSettingsForm()
        {
            InitializeComponent();
            SetupFormLayout();
            LoadConfigToUI();
            this.KeyPreview = true;
            this.KeyDown += GlobalControllerSettingsForm_KeyDown;
        }

        private void InitializeComponent()
        {
            this.Text = "🕹️ Master Controller & Keyboard Configuration";
            this.Size = new Size(960, 720);
            this.MinimumSize = new Size(840, 620);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppTheme.Current.Colors.Background;
            this.ForeColor = AppTheme.Current.Colors.TextPrimary;
            this.Padding = new Padding(16);
            this.AutoScroll = true;
        }

        private void SetupFormLayout()
        {
            this.Controls.Clear();

            var pnlMain = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 5,
                Margin = new Padding(0)
            };
            pnlMain.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header
            pnlMain.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Test Banner
            pnlMain.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Warning Banner
            pnlMain.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Tabs
            pnlMain.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Action buttons

            // 1. Top Header Row (Player Select, Controller Type, AutoSync, Test Mode Toggle)
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 50, Margin = new Padding(0, 0, 0, 8) };

            Label lblPlayer = new Label
            {
                Text = "Player:",
                Font = new Font(AppTheme.Current.Fonts.BodySmall, FontStyle.Bold),
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Location = new Point(0, 14),
                AutoSize = true
            };
            cbPlayerSelect = new ComboBox
            {
                Location = new Point(55, 10),
                Size = new Size(110, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppTheme.Current.Colors.Surface,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Font = AppTheme.Current.Fonts.BodySmall
            };
            cbPlayerSelect.Items.AddRange(new object[] { "Player 1", "Player 2", "Player 3", "Player 4" });
            cbPlayerSelect.SelectedIndex = 0;
            cbPlayerSelect.SelectedIndexChanged += cbPlayerSelect_SelectedIndexChanged;

            Label lblType = new Label
            {
                Text = "Type:",
                Font = new Font(AppTheme.Current.Fonts.BodySmall, FontStyle.Bold),
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Location = new Point(180, 14),
                AutoSize = true
            };
            cbControllerType = new ComboBox
            {
                Location = new Point(225, 10),
                Size = new Size(120, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppTheme.Current.Colors.Surface,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Font = AppTheme.Current.Fonts.BodySmall
            };
            cbControllerType.Items.AddRange(new object[] { "Keyboard", "XInput", "DirectInput", "Disabled" });
            cbControllerType.SelectedIndexChanged += cbControllerType_SelectedIndexChanged;

            chkAutoSyncOnLaunch = new CheckBox
            {
                Text = "Auto-sync to emulators on launch",
                Font = AppTheme.Current.Fonts.BodySmall,
                ForeColor = AppTheme.Current.Colors.TextSecondary,
                Location = new Point(365, 12),
                AutoSize = true
            };

            btnToggleTestMode = new ModernButton
            {
                Text = "🧪 Test Input Mode",
                Location = new Point(730, 8),
                Size = new Size(180, 32),
                IsPrimary = false
            };
            btnToggleTestMode.Click += btnToggleTestMode_Click;

            pnlHeader.Controls.Add(lblPlayer);
            pnlHeader.Controls.Add(cbPlayerSelect);
            pnlHeader.Controls.Add(lblType);
            pnlHeader.Controls.Add(cbControllerType);
            pnlHeader.Controls.Add(chkAutoSyncOnLaunch);
            pnlHeader.Controls.Add(btnToggleTestMode);
            pnlMain.Controls.Add(pnlHeader, 0, 0);

            // 2. Test Mode Banner (Hidden by default)
            pnlTestModeBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(40, 99, 102, 241),
                Margin = new Padding(0, 0, 0, 8),
                Visible = false
            };
            lblTestInputStatus = new Label
            {
                Text = "🎮 TEST MODE ACTIVE: Press physical keys on your keyboard. Press ESC or click Test Input Mode to exit.",
                Font = new Font(AppTheme.Current.Fonts.BodySmall, FontStyle.Bold),
                ForeColor = Color.FromArgb(165, 180, 252),
                Location = new Point(12, 8),
                AutoSize = true
            };
            pnlTestModeBanner.Controls.Add(lblTestInputStatus);
            pnlMain.Controls.Add(pnlTestModeBanner, 0, 1);

            // 3. Conflict Warning Banner (Hidden by default)
            pnlWarningBanner = new Panel
            {
                Dock = DockStyle.Top,
                Height = 32,
                BackColor = Color.FromArgb(40, 239, 68, 68),
                Margin = new Padding(0, 0, 0, 8),
                Visible = false
            };
            lblWarningText = new Label
            {
                Text = "⚠️ Duplicate key assignment detected!",
                Font = new Font(AppTheme.Current.Fonts.BodySmall, FontStyle.Bold),
                ForeColor = Color.FromArgb(248, 113, 113),
                Location = new Point(12, 6),
                AutoSize = true
            };
            pnlWarningBanner.Controls.Add(lblWarningText);
            pnlMain.Controls.Add(pnlWarningBanner, 0, 2);

            // 4. Tab Control for Keyboard, Gamepad, Hotkeys
            tcMain = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font(AppTheme.Current.Fonts.BodySmall, FontStyle.Bold)
            };

            tpKeyboard = new TabPage("⌨️ Keyboard Mappings")
            {
                BackColor = AppTheme.Current.Colors.Background,
                Padding = new Padding(12),
                AutoScroll = true
            };

            tpGamepad = new TabPage("🎮 Gamepad & Calibration")
            {
                BackColor = AppTheme.Current.Colors.Background,
                Padding = new Padding(12),
                AutoScroll = true
            };

            tpHotkeys = new TabPage("⚡ Global Hotkeys")
            {
                BackColor = AppTheme.Current.Colors.Background,
                Padding = new Padding(12),
                AutoScroll = true
            };

            SetupKeyboardTabPage();
            SetupGamepadTabPage();
            SetupHotkeysTabPage();

            tcMain.TabPages.Add(tpKeyboard);
            tcMain.TabPages.Add(tpGamepad);
            tcMain.TabPages.Add(tpHotkeys);

            pnlMain.Controls.Add(tcMain, 0, 3);

            // 5. Action Buttons Footer Row
            var pnlFooter = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 12, 0, 0),
                FlowDirection = FlowDirection.LeftToRight
            };

            btnSave = new ModernButton
            {
                Text = "💾 Save & Apply Profile",
                Size = new Size(190, 36),
                IsPrimary = true,
                Margin = new Padding(0, 0, 12, 0)
            };
            btnSave.Click += btnSave_Click;

            btnSyncAll = new ModernButton
            {
                Text = "🔄 Sync All Emulators",
                Size = new Size(180, 36),
                IsPrimary = false,
                Margin = new Padding(0)
            };
            btnSyncAll.Click += btnSyncAll_Click;

            pnlFooter.Controls.Add(btnSave);
            pnlFooter.Controls.Add(btnSyncAll);

            pnlMain.Controls.Add(pnlFooter, 0, 4);

            this.Controls.Add(pnlMain);
        }

        private void SetupKeyboardTabPage()
        {
            tpKeyboard.Controls.Clear();

            var pnlLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 2,
                Margin = new Padding(0)
            };
            pnlLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Preset Bar
            pnlKeyboardPresetBar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                Margin = new Padding(0, 0, 0, 12)
            };

            Label lblPreset = new Label
            {
                Text = "Keyboard Preset:",
                Font = new Font(AppTheme.Current.Fonts.BodySmall, FontStyle.Bold),
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Location = new Point(0, 12),
                AutoSize = true
            };

            cbPresetSelect = new ComboBox
            {
                Location = new Point(125, 8),
                Size = new Size(150, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppTheme.Current.Colors.Surface,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Font = AppTheme.Current.Fonts.BodySmall
            };
            cbPresetSelect.Items.AddRange(KeyboardPresetCatalog.GetPresetNames().ToArray());
            cbPresetSelect.SelectedIndex = 0;

            btnApplyPreset = new ModernButton
            {
                Text = "Apply Preset",
                Location = new Point(285, 6),
                Size = new Size(110, 30),
                IsPrimary = false
            };
            btnApplyPreset.Click += (s, e) => ApplySelectedPreset();

            btnResetPreset = new ModernButton
            {
                Text = "Reset Layout",
                Location = new Point(405, 6),
                Size = new Size(110, 30),
                IsPrimary = false
            };
            btnResetPreset.Click += (s, e) => ApplySelectedPreset();

            btnClearMappings = new ModernButton
            {
                Text = "Clear All",
                Location = new Point(525, 6),
                Size = new Size(90, 30),
                IsPrimary = false
            };
            btnClearMappings.Click += (s, e) => ClearAllKeyboardMappings();

            pnlKeyboardPresetBar.Controls.Add(lblPreset);
            pnlKeyboardPresetBar.Controls.Add(cbPresetSelect);
            pnlKeyboardPresetBar.Controls.Add(btnApplyPreset);
            pnlKeyboardPresetBar.Controls.Add(btnResetPreset);
            pnlKeyboardPresetBar.Controls.Add(btnClearMappings);

            pnlLayout.Controls.Add(pnlKeyboardPresetBar, 0, 0);

            // 2-Column Responsive Key Capture Grid for VirtualControllerActions
            var tlpGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                Margin = new Padding(0)
            };
            tlpGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            var actions = new[]
            {
                VirtualControllerAction.DPadUp, VirtualControllerAction.DPadDown,
                VirtualControllerAction.DPadLeft, VirtualControllerAction.DPadRight,
                VirtualControllerAction.LeftStickUp, VirtualControllerAction.LeftStickDown,
                VirtualControllerAction.LeftStickLeft, VirtualControllerAction.LeftStickRight,
                VirtualControllerAction.FaceSouth, VirtualControllerAction.FaceEast,
                VirtualControllerAction.FaceWest, VirtualControllerAction.FaceNorth,
                VirtualControllerAction.L1, VirtualControllerAction.R1,
                VirtualControllerAction.L2, VirtualControllerAction.R2,
                VirtualControllerAction.L3, VirtualControllerAction.R3,
                VirtualControllerAction.Start, VirtualControllerAction.Select
            };

            _keyboardControls.Clear();
            int col = 0, row = 0;

            foreach (var action in actions)
            {
                Panel pnlRow = new Panel
                {
                    Height = 34,
                    Dock = DockStyle.Top,
                    Margin = new Padding(4)
                };

                Label lblAction = new Label
                {
                    Text = KeyboardPresetCatalog.GetActionDisplayName(action) + ":",
                    Font = AppTheme.Current.Fonts.BodySmall,
                    ForeColor = AppTheme.Current.Colors.TextPrimary,
                    Location = new Point(0, 8),
                    Size = new Size(160, 20)
                };

                KeyCaptureControl kcc = new KeyCaptureControl
                {
                    Action = action,
                    Location = new Point(165, 3),
                    Size = new Size(170, 26)
                };
                kcc.KeyCaptured += (s, key) =>
                {
                    _isDirty = true;
                    ValidateDuplicateKeys();
                };

                _keyboardControls[action] = kcc;

                pnlRow.Controls.Add(lblAction);
                pnlRow.Controls.Add(kcc);

                tlpGrid.Controls.Add(pnlRow, col, row);

                col++;
                if (col >= 2)
                {
                    col = 0;
                    row++;
                }
            }

            pnlLayout.Controls.Add(tlpGrid, 0, 1);
            tpKeyboard.Controls.Add(pnlLayout);
        }

        private void SetupGamepadTabPage()
        {
            tpGamepad.Controls.Clear();

            var pnlMain = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Margin = new Padding(0)
            };

            GroupBox gbCalibration = new GroupBox
            {
                Text = "Controller Device & Calibration",
                Size = new Size(420, 250),
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Font = AppTheme.Current.Fonts.BodySmall,
                Margin = new Padding(0, 0, 0, 12)
            };

            Label lblDevice = new Label { Text = "Device Name:", Location = new Point(15, 30), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            tbDeviceGuid = new TextBox { Location = new Point(140, 27), Size = new Size(240, 23), BackColor = AppTheme.Current.Colors.Surface, ForeColor = AppTheme.Current.Colors.TextPrimary };

            Label lblDeadzone = new Label { Text = "Deadzone:", Location = new Point(15, 60), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            nudDeadzone = new NumericUpDown { Location = new Point(140, 57), Size = new Size(90, 23), DecimalPlaces = 2, Increment = 0.05m, Minimum = 0.00m, Maximum = 0.50m, BackColor = AppTheme.Current.Colors.Surface, ForeColor = AppTheme.Current.Colors.TextPrimary };

            Label lblSensitivity = new Label { Text = "Sensitivity:", Location = new Point(15, 90), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            nudSensitivity = new NumericUpDown { Location = new Point(140, 87), Size = new Size(90, 23), DecimalPlaces = 2, Increment = 0.10m, Minimum = 0.50m, Maximum = 2.50m, BackColor = AppTheme.Current.Colors.Surface, ForeColor = AppTheme.Current.Colors.TextPrimary };

            Label lblTrigger = new Label { Text = "Trigger Threshold:", Location = new Point(15, 120), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            nudTriggerThreshold = new NumericUpDown { Location = new Point(140, 117), Size = new Size(90, 23), DecimalPlaces = 2, Increment = 0.05m, Minimum = 0.00m, Maximum = 0.50m, BackColor = AppTheme.Current.Colors.Surface, ForeColor = AppTheme.Current.Colors.TextPrimary };

            chkInvertLX = new CheckBox { Text = "Invert Left X", Location = new Point(15, 155), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            chkInvertLY = new CheckBox { Text = "Invert Left Y", Location = new Point(140, 155), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            chkInvertRX = new CheckBox { Text = "Invert Right X", Location = new Point(15, 180), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            chkInvertRY = new CheckBox { Text = "Invert Right Y", Location = new Point(140, 180), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };

            chkEnableRumble = new CheckBox { Text = "Enable Rumble", Location = new Point(15, 210), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            Label lblRumbleStr = new Label { Text = "Strength:", Location = new Point(140, 210), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            nudRumbleStrength = new NumericUpDown { Location = new Point(210, 207), Size = new Size(90, 23), DecimalPlaces = 2, Increment = 0.10m, Minimum = 0.00m, Maximum = 1.00m, BackColor = AppTheme.Current.Colors.Surface, ForeColor = AppTheme.Current.Colors.TextPrimary };

            gbCalibration.Controls.Add(lblDevice); gbCalibration.Controls.Add(tbDeviceGuid);
            gbCalibration.Controls.Add(lblDeadzone); gbCalibration.Controls.Add(nudDeadzone);
            gbCalibration.Controls.Add(lblSensitivity); gbCalibration.Controls.Add(nudSensitivity);
            gbCalibration.Controls.Add(lblTrigger); gbCalibration.Controls.Add(nudTriggerThreshold);
            gbCalibration.Controls.Add(chkInvertLX); gbCalibration.Controls.Add(chkInvertLY);
            gbCalibration.Controls.Add(chkInvertRX); gbCalibration.Controls.Add(chkInvertRY);
            gbCalibration.Controls.Add(chkEnableRumble); gbCalibration.Controls.Add(lblRumbleStr); gbCalibration.Controls.Add(nudRumbleStrength);

            pnlMain.Controls.Add(gbCalibration);

            tpGamepad.Controls.Add(pnlMain);
        }

        private void SetupHotkeysTabPage()
        {
            tpHotkeys.Controls.Clear();

            var pnlMain = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Margin = new Padding(0)
            };

            GroupBox gbHotkeys = new GroupBox
            {
                Text = "Global Launcher Hotkeys",
                Size = new Size(450, 260),
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Font = AppTheme.Current.Fonts.BodySmall
            };

            var hotkeyActions = new[]
            {
                (VirtualControllerAction.Pause, "Pause Launcher:"),
                (VirtualControllerAction.SaveState, "Save State:"),
                (VirtualControllerAction.LoadState, "Load State:"),
                (VirtualControllerAction.FastForward, "Fast Forward:"),
                (VirtualControllerAction.Screenshot, "Screenshot:"),
                (VirtualControllerAction.ToggleMenu, "Toggle Menu:")
            };

            _hotkeyControls.Clear();
            int y = 25;

            foreach (var (action, label) in hotkeyActions)
            {
                Label lbl = new Label
                {
                    Text = label,
                    Location = new Point(15, y + 4),
                    AutoSize = true,
                    ForeColor = AppTheme.Current.Colors.TextPrimary,
                    Font = AppTheme.Current.Fonts.BodySmall
                };

                KeyCaptureControl kcc = new KeyCaptureControl
                {
                    Action = action,
                    Location = new Point(180, y),
                    Size = new Size(180, 26)
                };
                kcc.KeyCaptured += (s, k) =>
                {
                    _isDirty = true;
                    ValidateDuplicateKeys();
                };

                _hotkeyControls[action] = kcc;

                gbHotkeys.Controls.Add(lbl);
                gbHotkeys.Controls.Add(kcc);

                y += 36;
            }

            pnlMain.Controls.Add(gbHotkeys);
            tpHotkeys.Controls.Add(pnlMain);
        }

        private void LoadConfigToUI()
        {
            var config = GlobalControllerConfigManager.Instance.Config;
            chkAutoSyncOnLaunch.Checked = config.AutoSyncOnLaunch;

            // Load Hotkeys into Hotkey Controls
            if (_hotkeyControls.TryGetValue(VirtualControllerAction.Pause, out var kccPause) && Enum.TryParse(config.Hotkeys.Pause, out Keys kPause)) kccPause.SelectedKey = kPause;
            if (_hotkeyControls.TryGetValue(VirtualControllerAction.SaveState, out var kccSave) && Enum.TryParse(config.Hotkeys.SaveState, out Keys kSave)) kccSave.SelectedKey = kSave;
            if (_hotkeyControls.TryGetValue(VirtualControllerAction.LoadState, out var kccLoad) && Enum.TryParse(config.Hotkeys.LoadState, out Keys kLoad)) kccLoad.SelectedKey = kLoad;
            if (_hotkeyControls.TryGetValue(VirtualControllerAction.FastForward, out var kccFF) && Enum.TryParse(config.Hotkeys.FastForward, out Keys kFF)) kccFF.SelectedKey = kFF;
            if (_hotkeyControls.TryGetValue(VirtualControllerAction.Screenshot, out var kccSS) && Enum.TryParse(config.Hotkeys.Screenshot, out Keys kSS)) kccSS.SelectedKey = kSS;
            if (_hotkeyControls.TryGetValue(VirtualControllerAction.ToggleMenu, out var kccMenu) && Enum.TryParse(config.Hotkeys.ToggleMenu, out Keys kMenu)) kccMenu.SelectedKey = kMenu;

            LoadPlayerConfig(_selectedPlayerIndex);
        }

        private void LoadPlayerConfig(int playerIndex)
        {
            var config = GlobalControllerConfigManager.Instance.Config;
            var player = config.Players.FirstOrDefault(p => p.PlayerIndex == playerIndex) ?? new PlayerControllerConfig { PlayerIndex = playerIndex };

            int typeIdx = cbControllerType.FindStringExact(player.ControllerType);
            cbControllerType.SelectedIndex = typeIdx >= 0 ? typeIdx : 0;

            tbDeviceGuid.Text = player.DeviceGuidOrName;
            nudDeadzone.Value = (decimal)Math.Clamp(player.Deadzone, 0.0f, 0.50f);
            nudSensitivity.Value = (decimal)Math.Clamp(player.Sensitivity, 0.50f, 2.50f);
            nudTriggerThreshold.Value = (decimal)Math.Clamp(player.TriggerThreshold, 0.0f, 0.50f);

            chkInvertLX.Checked = player.InvertLeftStickX;
            chkInvertLY.Checked = player.InvertLeftStickY;
            chkInvertRX.Checked = player.InvertRightStickX;
            chkInvertRY.Checked = player.InvertRightStickY;

            chkEnableRumble.Checked = player.EnableRumble;
            nudRumbleStrength.Value = (decimal)Math.Clamp(player.RumbleStrength, 0.0f, 1.00f);

            // Load Keyboard Mappings
            var kbMappings = player.GetKeyboardMappings();
            if (!kbMappings.Any(m => m.Key.HasValue))
            {
                // Fallback to Modern WASD if empty
                kbMappings = KeyboardPresetCatalog.GetModernWASDPreset();
            }

            foreach (var mapping in kbMappings)
            {
                if (_keyboardControls.TryGetValue(mapping.Action, out var kcc))
                {
                    kcc.SelectedKey = mapping.Key;
                }
            }

            UpdateControllerTypeVisibility();
            ValidateDuplicateKeys();
            _isDirty = false;
        }

        private void SavePlayerConfig(int playerIndex)
        {
            var config = GlobalControllerConfigManager.Instance.Config;
            var player = config.Players.FirstOrDefault(p => p.PlayerIndex == playerIndex);
            if (player == null)
            {
                player = new PlayerControllerConfig { PlayerIndex = playerIndex };
                config.Players.Add(player);
            }

            player.ControllerType = cbControllerType.SelectedItem?.ToString() ?? "Keyboard";
            player.DeviceGuidOrName = tbDeviceGuid.Text;
            player.Deadzone = (float)nudDeadzone.Value;
            player.Sensitivity = (float)nudSensitivity.Value;
            player.TriggerThreshold = (float)nudTriggerThreshold.Value;

            player.InvertLeftStickX = chkInvertLX.Checked;
            player.InvertLeftStickY = chkInvertLY.Checked;
            player.InvertRightStickX = chkInvertRX.Checked;
            player.InvertRightStickY = chkInvertRY.Checked;

            player.EnableRumble = chkEnableRumble.Checked;
            player.RumbleStrength = (float)nudRumbleStrength.Value;

            // Save Keyboard Mappings
            var list = new List<KeyboardMapping>();
            foreach (var kvp in _keyboardControls)
            {
                list.Add(new KeyboardMapping { Action = kvp.Key, Key = kvp.Value.SelectedKey });
            }
            player.SetKeyboardMappings(list);
        }

        private void UpdateControllerTypeVisibility()
        {
            string selectedType = cbControllerType.SelectedItem?.ToString() ?? "Keyboard";
            bool isKeyboard = string.Equals(selectedType, "Keyboard", StringComparison.OrdinalIgnoreCase);

            pnlKeyboardPresetBar.Visible = isKeyboard;
            if (isKeyboard)
            {
                tcMain.SelectedTab = tpKeyboard;
            }
        }

        private void ApplySelectedPreset()
        {
            string selectedPreset = cbPresetSelect.SelectedItem?.ToString() ?? KeyboardPresetCatalog.ModernWASD;
            var presetMappings = KeyboardPresetCatalog.GetPresetMappings(selectedPreset);

            foreach (var pm in presetMappings)
            {
                if (_keyboardControls.TryGetValue(pm.Action, out var kcc))
                {
                    kcc.SelectedKey = pm.Key;
                }
            }

            ValidateDuplicateKeys();
            _isDirty = true;
            ToastNotification.ShowToast(this, $"Applied preset '{selectedPreset}'.", StatusType.Info);
        }

        private void ClearAllKeyboardMappings()
        {
            foreach (var kcc in _keyboardControls.Values)
            {
                kcc.SelectedKey = null;
            }

            ValidateDuplicateKeys();
            _isDirty = true;
        }

        private void ValidateDuplicateKeys()
        {
            var keyCounts = new Dictionary<Keys, List<VirtualControllerAction>>();

            foreach (var kvp in _keyboardControls)
            {
                if (kvp.Value.SelectedKey.HasValue)
                {
                    Keys k = kvp.Value.SelectedKey.Value;
                    if (!keyCounts.ContainsKey(k)) keyCounts[k] = new List<VirtualControllerAction>();
                    keyCounts[k].Add(kvp.Key);
                }
            }

            var duplicates = keyCounts.Where(kvp => kvp.Value.Count > 1).ToList();
            if (duplicates.Any())
            {
                var sb = new StringBuilder("⚠️ Warning: Duplicate key mappings detected: ");
                foreach (var dup in duplicates)
                {
                    sb.Append($"[{KeyCaptureControl.FormatKeyDisplay(dup.Key)}] (");
                    sb.Append(string.Join(", ", dup.Value.Select(a => KeyboardPresetCatalog.GetActionDisplayName(a))));
                    sb.Append(") ");
                }

                lblWarningText.Text = sb.ToString();
                pnlWarningBanner.Visible = true;
            }
            else
            {
                pnlWarningBanner.Visible = false;
            }
        }

        private void cbPlayerSelect_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_isDirty)
            {
                var result = MessageBox.Show(this, "Save changes to the current player profile before switching?", "Unsaved Changes", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel)
                {
                    cbPlayerSelect.SelectedIndex = _selectedPlayerIndex - 1;
                    return;
                }
                if (result == DialogResult.Yes)
                {
                    SavePlayerConfig(_selectedPlayerIndex);
                }
            }

            _selectedPlayerIndex = cbPlayerSelect.SelectedIndex + 1;
            LoadPlayerConfig(_selectedPlayerIndex);
        }

        private void cbControllerType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateControllerTypeVisibility();
            _isDirty = true;
        }

        private void btnToggleTestMode_Click(object? sender, EventArgs e)
        {
            _isTestModeActive = !_isTestModeActive;

            if (_isTestModeActive)
            {
                pnlTestModeBanner.Visible = true;
                btnToggleTestMode.Text = "🛑 Exit Test Mode";
                this.Focus();
            }
            else
            {
                pnlTestModeBanner.Visible = false;
                btnToggleTestMode.Text = "🧪 Test Input Mode";
            }
        }

        private void GlobalControllerSettingsForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (_isTestModeActive)
            {
                if (e.KeyCode == Keys.Escape)
                {
                    _isTestModeActive = false;
                    pnlTestModeBanner.Visible = false;
                    btnToggleTestMode.Text = "🧪 Test Input Mode";
                    return;
                }

                Keys pressedKey = e.KeyCode;
                var matchedControl = _keyboardControls.FirstOrDefault(kvp => kvp.Value.SelectedKey == pressedKey);

                if (matchedControl.Value != null)
                {
                    lblTestInputStatus.Text = $"🎮 TEST MODE: Last Key Pressed: [ {KeyCaptureControl.FormatKeyDisplay(pressedKey)} ] ➔ Mapped to: {KeyboardPresetCatalog.GetActionDisplayName(matchedControl.Key)}";
                    tcMain.SelectedTab = tpKeyboard;
                    matchedControl.Value.Focus();
                }
                else
                {
                    lblTestInputStatus.Text = $"🎮 TEST MODE: Last Key Pressed: [ {KeyCaptureControl.FormatKeyDisplay(pressedKey)} ] ➔ [ Unassigned ]";
                }
            }
        }

        private async void btnSave_Click(object? sender, EventArgs e)
        {
            SavePlayerConfig(_selectedPlayerIndex);

            var config = GlobalControllerConfigManager.Instance.Config;
            config.AutoSyncOnLaunch = chkAutoSyncOnLaunch.Checked;

            if (_hotkeyControls.TryGetValue(VirtualControllerAction.Pause, out var kccPause)) config.Hotkeys.Pause = kccPause.SelectedKey?.ToString() ?? "Escape";
            if (_hotkeyControls.TryGetValue(VirtualControllerAction.SaveState, out var kccSave)) config.Hotkeys.SaveState = kccSave.SelectedKey?.ToString() ?? "F1";
            if (_hotkeyControls.TryGetValue(VirtualControllerAction.LoadState, out var kccLoad)) config.Hotkeys.LoadState = kccLoad.SelectedKey?.ToString() ?? "F3";
            if (_hotkeyControls.TryGetValue(VirtualControllerAction.FastForward, out var kccFF)) config.Hotkeys.FastForward = kccFF.SelectedKey?.ToString() ?? "Tab";
            if (_hotkeyControls.TryGetValue(VirtualControllerAction.Screenshot, out var kccSS)) config.Hotkeys.Screenshot = kccSS.SelectedKey?.ToString() ?? "F12";
            if (_hotkeyControls.TryGetValue(VirtualControllerAction.ToggleMenu, out var kccMenu)) config.Hotkeys.ToggleMenu = kccMenu.SelectedKey?.ToString() ?? "F10";

            GlobalControllerConfigManager.Instance.Save();
            _isDirty = false;

            btnSave.Enabled = false;
            try
            {
                var syncResults = await ControllerSyncService.Instance.SyncAllEmulatorsAsync(this);
                int successCount = syncResults.Count(r => r.Success);
                ToastNotification.ShowToast(
                    this,
                    $"Global controller & keyboard profile saved & applied to {successCount} / {syncResults.Count} emulators.",
                    StatusType.Success);
            }
            finally
            {
                btnSave.Enabled = true;
            }
        }

        private async void btnSyncAll_Click(object? sender, EventArgs e)
        {
            btnSyncAll.Enabled = false;
            try
            {
                var syncResults = await ControllerSyncService.Instance.SyncAllEmulatorsAsync(this);
                var sb = new StringBuilder();
                sb.AppendLine("Global Controller & Keyboard Sync Summary:");
                sb.AppendLine("-----------------------------------------");

                foreach (var res in syncResults)
                {
                    string status = res.Success ? "✅ Success" : "❌ Failed / Skipped";
                    sb.AppendLine($"• {res.EmulatorName}: {status}");
                    if (!string.IsNullOrEmpty(res.Message))
                    {
                        sb.AppendLine($"  - {res.Message}");
                    }
                }

                MessageBox.Show(this, sb.ToString().TrimEnd(), "Global Controller Sync", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                btnSyncAll.Enabled = true;
            }
        }
    }
}
