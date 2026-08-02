using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RetroLauncher.UI.Controls;
using RetroLauncher.UI.Theme;

namespace RetroLauncher.UI.Forms
{
    public class GlobalControllerSettingsForm : Form
    {
        private ComboBox cbPlayerSelect = null!;
        private ComboBox cbControllerType = null!;
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
        private CheckBox chkAutoSyncOnLaunch = null!;

        // Hotkeys
        private TextBox tbHotkeyPause = null!;
        private TextBox tbHotkeySaveState = null!;
        private TextBox tbHotkeyLoadState = null!;
        private TextBox tbHotkeyFastForward = null!;
        private TextBox tbHotkeyScreenshot = null!;
        private TextBox tbHotkeyMenu = null!;

        // Mapping Inputs Cache
        private Dictionary<string, TextBox> _mappingInputs = new(StringComparer.OrdinalIgnoreCase);

        private ModernButton btnSave = null!;
        private ModernButton btnSyncAll = null!;

        private int _selectedPlayerIndex = 1;

        public GlobalControllerSettingsForm()
        {
            InitializeComponent();
            SetupFormLayout();
            LoadConfigToUI();
        }

        private void InitializeComponent()
        {
            this.Text = "Global Controller Settings";
            this.Size = new Size(880, 640);
            this.MinimumSize = new Size(800, 560);
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = AppTheme.Current.Colors.Background;
            this.ForeColor = AppTheme.Current.Colors.TextPrimary;
            this.Padding = new Padding(24, 16, 24, 16);
            this.AutoScroll = true;
        }

        private void SetupFormLayout()
        {
            this.Controls.Clear();

            var tlpMain = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 1,
                RowCount = 5,
                Margin = new Padding(0)
            };
            tlpMain.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Header Section
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 44, Margin = new Padding(0, 0, 0, 16) };
            Label lblTitle = new Label
            {
                Text = "🕹️ Master Controller Configuration",
                Font = AppTheme.Current.Fonts.TitleSmall,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Location = new Point(0, 8),
                AutoSize = true
            };
            chkAutoSyncOnLaunch = new CheckBox
            {
                Text = "Automatically sync global profile to emulators on launch",
                Font = AppTheme.Current.Fonts.BodySmall,
                ForeColor = AppTheme.Current.Colors.TextSecondary,
                Location = new Point(360, 10),
                AutoSize = true
            };
            pnlHeader.Controls.Add(lblTitle);
            pnlHeader.Controls.Add(chkAutoSyncOnLaunch);
            tlpMain.Controls.Add(pnlHeader, 0, 0);

            // Player Selector Row
            var pnlPlayer = new Panel { Dock = DockStyle.Top, Height = 36, Margin = new Padding(0, 0, 0, 16) };
            Label lblPlayer = new Label { Text = "Select Player Profile:", Font = AppTheme.Current.Fonts.BodyMedium, ForeColor = AppTheme.Current.Colors.TextPrimary, Location = new Point(0, 6), AutoSize = true };
            cbPlayerSelect = new ComboBox
            {
                Location = new Point(160, 4),
                Size = new Size(180, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = AppTheme.Current.Colors.Surface,
                ForeColor = AppTheme.Current.Colors.TextPrimary
            };
            cbPlayerSelect.Items.AddRange(new object[] { "Player 1", "Player 2", "Player 3", "Player 4" });
            cbPlayerSelect.SelectedIndex = 0;
            cbPlayerSelect.SelectedIndexChanged += cbPlayerSelect_SelectedIndexChanged;
            pnlPlayer.Controls.Add(lblPlayer);
            pnlPlayer.Controls.Add(cbPlayerSelect);
            tlpMain.Controls.Add(pnlPlayer, 0, 1);

            // Split Grid (Left: Calibration & Hotkeys, Right: Button Mappings)
            var tlpSplit = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 0, 0, 16)
            };
            tlpSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpSplit.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            // Left Stack: Calibration + Hotkeys
            var pnlLeftStack = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                Margin = new Padding(0, 0, 8, 0)
            };

            GroupBox gbCalibration = new GroupBox
            {
                Text = "Controller & Calibration",
                Size = new Size(380, 270),
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Font = AppTheme.Current.Fonts.BodySmall,
                Margin = new Padding(0, 0, 0, 12)
            };

            Label lblType = new Label { Text = "Type:", Location = new Point(15, 25), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            cbControllerType = new ComboBox { Location = new Point(140, 22), Size = new Size(210, 23), DropDownStyle = ComboBoxStyle.DropDownList, FlatStyle = FlatStyle.Flat, BackColor = AppTheme.Current.Colors.Surface, ForeColor = AppTheme.Current.Colors.TextPrimary };
            cbControllerType.Items.AddRange(new object[] { "XInput", "DirectInput", "Keyboard", "Disabled" });

            Label lblDevice = new Label { Text = "Device Name:", Location = new Point(15, 55), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            tbDeviceGuid = new TextBox { Location = new Point(140, 52), Size = new Size(210, 23), BackColor = AppTheme.Current.Colors.Surface, ForeColor = AppTheme.Current.Colors.TextPrimary };

            Label lblDeadzone = new Label { Text = "Deadzone:", Location = new Point(15, 85), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            nudDeadzone = new NumericUpDown { Location = new Point(140, 82), Size = new Size(90, 23), DecimalPlaces = 2, Increment = 0.05m, Minimum = 0.00m, Maximum = 0.50m, BackColor = AppTheme.Current.Colors.Surface, ForeColor = AppTheme.Current.Colors.TextPrimary };

            Label lblSensitivity = new Label { Text = "Sensitivity:", Location = new Point(15, 115), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            nudSensitivity = new NumericUpDown { Location = new Point(140, 112), Size = new Size(90, 23), DecimalPlaces = 2, Increment = 0.10m, Minimum = 0.50m, Maximum = 2.50m, BackColor = AppTheme.Current.Colors.Surface, ForeColor = AppTheme.Current.Colors.TextPrimary };

            Label lblTrigger = new Label { Text = "Trigger Threshold:", Location = new Point(15, 145), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            nudTriggerThreshold = new NumericUpDown { Location = new Point(140, 142), Size = new Size(90, 23), DecimalPlaces = 2, Increment = 0.05m, Minimum = 0.00m, Maximum = 0.50m, BackColor = AppTheme.Current.Colors.Surface, ForeColor = AppTheme.Current.Colors.TextPrimary };

            chkInvertLX = new CheckBox { Text = "Invert Left X", Location = new Point(15, 175), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            chkInvertLY = new CheckBox { Text = "Invert Left Y", Location = new Point(140, 175), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            chkInvertRX = new CheckBox { Text = "Invert Right X", Location = new Point(15, 200), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            chkInvertRY = new CheckBox { Text = "Invert Right Y", Location = new Point(140, 200), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };

            chkEnableRumble = new CheckBox { Text = "Enable Rumble", Location = new Point(15, 230), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            Label lblRumbleStr = new Label { Text = "Strength:", Location = new Point(140, 230), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary };
            nudRumbleStrength = new NumericUpDown { Location = new Point(210, 227), Size = new Size(90, 23), DecimalPlaces = 2, Increment = 0.10m, Minimum = 0.00m, Maximum = 1.00m, BackColor = AppTheme.Current.Colors.Surface, ForeColor = AppTheme.Current.Colors.TextPrimary };

            gbCalibration.Controls.Add(lblType); gbCalibration.Controls.Add(cbControllerType);
            gbCalibration.Controls.Add(lblDevice); gbCalibration.Controls.Add(tbDeviceGuid);
            gbCalibration.Controls.Add(lblDeadzone); gbCalibration.Controls.Add(nudDeadzone);
            gbCalibration.Controls.Add(lblSensitivity); gbCalibration.Controls.Add(nudSensitivity);
            gbCalibration.Controls.Add(lblTrigger); gbCalibration.Controls.Add(nudTriggerThreshold);
            gbCalibration.Controls.Add(chkInvertLX); gbCalibration.Controls.Add(chkInvertLY);
            gbCalibration.Controls.Add(chkInvertRX); gbCalibration.Controls.Add(chkInvertRY);
            gbCalibration.Controls.Add(chkEnableRumble); gbCalibration.Controls.Add(lblRumbleStr); gbCalibration.Controls.Add(nudRumbleStrength);
            pnlLeftStack.Controls.Add(gbCalibration);

            GroupBox gbHotkeys = new GroupBox
            {
                Text = "Global Hotkeys",
                Size = new Size(380, 210),
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Font = AppTheme.Current.Fonts.BodySmall
            };

            tbHotkeyPause = AddHotkeyRow(gbHotkeys, "Pause:", "P", 25);
            tbHotkeySaveState = AddHotkeyRow(gbHotkeys, "Save State:", "F1", 55);
            tbHotkeyLoadState = AddHotkeyRow(gbHotkeys, "Load State:", "F3", 85);
            tbHotkeyFastForward = AddHotkeyRow(gbHotkeys, "Fast Forward:", "Tab", 115);
            tbHotkeyScreenshot = AddHotkeyRow(gbHotkeys, "Screenshot:", "F12", 145);
            tbHotkeyMenu = AddHotkeyRow(gbHotkeys, "Toggle Menu:", "Escape", 175);
            pnlLeftStack.Controls.Add(gbHotkeys);

            tlpSplit.Controls.Add(pnlLeftStack, 0, 0);

            // Right Stack: Button Mappings
            GroupBox gbMappings = new GroupBox
            {
                Text = "Button & Direction Mappings",
                Dock = DockStyle.Fill,
                ForeColor = AppTheme.Current.Colors.TextPrimary,
                Font = AppTheme.Current.Fonts.BodySmall,
                Margin = new Padding(8, 0, 0, 0)
            };

            Panel pnlMapScroll = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(8),
                AutoScroll = true
            };

            var defaultMappings = PlayerControllerConfig.GetDefaultButtonMappings();
            int yPos = 5;
            foreach (var kvp in defaultMappings)
            {
                Label lblKey = new Label { Text = kvp.Key.Replace("_", " "), Location = new Point(5, yPos + 3), Size = new Size(130, 20), ForeColor = AppTheme.Current.Colors.TextPrimary, Font = AppTheme.Current.Fonts.BodySmall };
                TextBox tbInput = new TextBox { Text = kvp.Value, Location = new Point(140, yPos), Size = new Size(180, 22), BackColor = AppTheme.Current.Colors.Surface, ForeColor = AppTheme.Current.Colors.TextPrimary };

                pnlMapScroll.Controls.Add(lblKey);
                pnlMapScroll.Controls.Add(tbInput);
                _mappingInputs[kvp.Key] = tbInput;

                yPos += 28;
            }
            gbMappings.Controls.Add(pnlMapScroll);
            tlpSplit.Controls.Add(gbMappings, 1, 0);

            tlpMain.Controls.Add(tlpSplit, 0, 2);

            // Action Buttons Row
            var flpActionButtons = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 8, 0, 0)
            };

            btnSave = new ModernButton { Text = "💾 Save & Apply Profile", Size = new Size(180, 36), IsPrimary = true, Margin = new Padding(0, 0, 12, 0) };
            btnSave.Click += btnSave_Click;

            btnSyncAll = new ModernButton { Text = "🔄 Sync All Emulators", Size = new Size(180, 36), IsPrimary = false, Margin = new Padding(0) };
            btnSyncAll.Click += btnSyncAll_Click;

            flpActionButtons.Controls.Add(btnSave);
            flpActionButtons.Controls.Add(btnSyncAll);
            tlpMain.Controls.Add(flpActionButtons, 0, 3);

            this.Controls.Add(tlpMain);
        }

        private TextBox AddHotkeyRow(GroupBox gb, string labelText, string defaultValue, int y)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(15, y + 3), AutoSize = true, ForeColor = AppTheme.Current.Colors.TextPrimary, Font = AppTheme.Current.Fonts.BodySmall };
            TextBox tb = new TextBox { Text = defaultValue, Location = new Point(140, y), Size = new Size(210, 22), BackColor = AppTheme.Current.Colors.Surface, ForeColor = AppTheme.Current.Colors.TextPrimary };
            gb.Controls.Add(lbl);
            gb.Controls.Add(tb);
            return tb;
        }

        private void LoadConfigToUI()
        {
            var config = GlobalControllerConfigManager.Instance.Config;
            chkAutoSyncOnLaunch.Checked = config.AutoSyncOnLaunch;

            tbHotkeyPause.Text = config.Hotkeys.Pause;
            tbHotkeySaveState.Text = config.Hotkeys.SaveState;
            tbHotkeyLoadState.Text = config.Hotkeys.LoadState;
            tbHotkeyFastForward.Text = config.Hotkeys.FastForward;
            tbHotkeyScreenshot.Text = config.Hotkeys.Screenshot;
            tbHotkeyMenu.Text = config.Hotkeys.ToggleMenu;

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

            foreach (var kvp in _mappingInputs)
            {
                if (player.ButtonMappings.TryGetValue(kvp.Key, out string? val))
                {
                    kvp.Value.Text = val;
                }
            }
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

            player.ControllerType = cbControllerType.SelectedItem?.ToString() ?? "XInput";
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

            foreach (var kvp in _mappingInputs)
            {
                player.ButtonMappings[kvp.Key] = kvp.Value.Text;
            }
        }

        private void cbPlayerSelect_SelectedIndexChanged(object? sender, EventArgs e)
        {
            SavePlayerConfig(_selectedPlayerIndex);
            _selectedPlayerIndex = cbPlayerSelect.SelectedIndex + 1;
            LoadPlayerConfig(_selectedPlayerIndex);
        }

        private async void btnSave_Click(object? sender, EventArgs e)
        {
            SavePlayerConfig(_selectedPlayerIndex);

            var config = GlobalControllerConfigManager.Instance.Config;
            config.AutoSyncOnLaunch = chkAutoSyncOnLaunch.Checked;

            config.Hotkeys.Pause = tbHotkeyPause.Text;
            config.Hotkeys.SaveState = tbHotkeySaveState.Text;
            config.Hotkeys.LoadState = tbHotkeyLoadState.Text;
            config.Hotkeys.FastForward = tbHotkeyFastForward.Text;
            config.Hotkeys.Screenshot = tbHotkeyScreenshot.Text;
            config.Hotkeys.ToggleMenu = tbHotkeyMenu.Text;

            GlobalControllerConfigManager.Instance.Save();

            btnSave.Enabled = false;
            try
            {
                var syncResults = await ControllerSyncService.Instance.SyncAllEmulatorsAsync(this);
                int successCount = syncResults.Count(r => r.Success);
                ToastNotification.ShowToast(
                    this,
                    $"Global controller settings saved & applied to {successCount} / {syncResults.Count} emulators.",
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
                var sb = new System.Text.StringBuilder();
                sb.AppendLine("Global Controller Sync Summary:");
                sb.AppendLine("-----------------------------");

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
