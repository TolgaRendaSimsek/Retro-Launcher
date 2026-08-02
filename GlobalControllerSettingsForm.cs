using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace RetroLauncher
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

        private Button btnSave = null!;
        private Button btnSyncAll = null!;
        private Button btnClose = null!;

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
            this.Size = new Size(820, 680);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(24, 24, 28);
            this.ForeColor = Color.White;
        }

        private void SetupFormLayout()
        {
            // Title Header
            Label lblTitle = new Label
            {
                Text = "🎮 Master Controller Configuration",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(20, 15),
                AutoSize = true,
                ForeColor = Color.White
            };
            this.Controls.Add(lblTitle);

            // Auto-Sync Toggle
            chkAutoSyncOnLaunch = new CheckBox
            {
                Text = "Automatically sync global profile to emulators on launch",
                Location = new Point(420, 20),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(209, 213, 219)
            };
            this.Controls.Add(chkAutoSyncOnLaunch);

            // Player Selection Bar
            Label lblPlayer = new Label
            {
                Text = "Select Player:",
                Location = new Point(20, 55),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
            };
            this.Controls.Add(lblPlayer);

            cbPlayerSelect = new ComboBox
            {
                Location = new Point(120, 52),
                Size = new Size(180, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(44, 44, 52),
                ForeColor = Color.White
            };
            cbPlayerSelect.Items.AddRange(new object[] { "Player 1", "Player 2", "Player 3", "Player 4" });
            cbPlayerSelect.SelectedIndex = 0;
            cbPlayerSelect.SelectedIndexChanged += cbPlayerSelect_SelectedIndexChanged;
            this.Controls.Add(cbPlayerSelect);

            // Group 1: Controller & Calibration
            GroupBox gbCalibration = new GroupBox
            {
                Text = "Controller & Calibration",
                Location = new Point(20, 90),
                Size = new Size(370, 290),
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            Label lblType = new Label { Text = "Type:", Location = new Point(15, 25), AutoSize = true, ForeColor = Color.White };
            cbControllerType = new ComboBox
            {
                Location = new Point(140, 22),
                Size = new Size(200, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(44, 44, 52),
                ForeColor = Color.White
            };
            cbControllerType.Items.AddRange(new object[] { "XInput", "DirectInput", "Keyboard", "Disabled" });

            Label lblDevice = new Label { Text = "Device Name:", Location = new Point(15, 55), AutoSize = true, ForeColor = Color.White };
            tbDeviceGuid = new TextBox { Location = new Point(140, 52), Size = new Size(200, 23), BackColor = Color.FromArgb(44, 44, 52), ForeColor = Color.White };

            Label lblDeadzone = new Label { Text = "Deadzone:", Location = new Point(15, 85), AutoSize = true, ForeColor = Color.White };
            nudDeadzone = new NumericUpDown { Location = new Point(140, 82), Size = new Size(80, 23), DecimalPlaces = 2, Increment = 0.05m, Minimum = 0.00m, Maximum = 0.50m, BackColor = Color.FromArgb(44, 44, 52), ForeColor = Color.White };

            Label lblSensitivity = new Label { Text = "Sensitivity:", Location = new Point(15, 115), AutoSize = true, ForeColor = Color.White };
            nudSensitivity = new NumericUpDown { Location = new Point(140, 112), Size = new Size(80, 23), DecimalPlaces = 2, Increment = 0.10m, Minimum = 0.50m, Maximum = 2.50m, BackColor = Color.FromArgb(44, 44, 52), ForeColor = Color.White };

            Label lblTrigger = new Label { Text = "Trigger Threshold:", Location = new Point(15, 145), AutoSize = true, ForeColor = Color.White };
            nudTriggerThreshold = new NumericUpDown { Location = new Point(140, 142), Size = new Size(80, 23), DecimalPlaces = 2, Increment = 0.05m, Minimum = 0.00m, Maximum = 0.50m, BackColor = Color.FromArgb(44, 44, 52), ForeColor = Color.White };

            chkInvertLX = new CheckBox { Text = "Invert Left X", Location = new Point(15, 175), AutoSize = true, ForeColor = Color.White };
            chkInvertLY = new CheckBox { Text = "Invert Left Y", Location = new Point(140, 175), AutoSize = true, ForeColor = Color.White };
            chkInvertRX = new CheckBox { Text = "Invert Right X", Location = new Point(15, 200), AutoSize = true, ForeColor = Color.White };
            chkInvertRY = new CheckBox { Text = "Invert Right Y", Location = new Point(140, 200), AutoSize = true, ForeColor = Color.White };

            chkEnableRumble = new CheckBox { Text = "Enable Rumble", Location = new Point(15, 230), AutoSize = true, ForeColor = Color.White };
            Label lblRumbleStr = new Label { Text = "Strength:", Location = new Point(140, 230), AutoSize = true, ForeColor = Color.White };
            nudRumbleStrength = new NumericUpDown { Location = new Point(210, 227), Size = new Size(80, 23), DecimalPlaces = 2, Increment = 0.10m, Minimum = 0.00m, Maximum = 1.00m, BackColor = Color.FromArgb(44, 44, 52), ForeColor = Color.White };

            gbCalibration.Controls.Add(lblType); gbCalibration.Controls.Add(cbControllerType);
            gbCalibration.Controls.Add(lblDevice); gbCalibration.Controls.Add(tbDeviceGuid);
            gbCalibration.Controls.Add(lblDeadzone); gbCalibration.Controls.Add(nudDeadzone);
            gbCalibration.Controls.Add(lblSensitivity); gbCalibration.Controls.Add(nudSensitivity);
            gbCalibration.Controls.Add(lblTrigger); gbCalibration.Controls.Add(nudTriggerThreshold);
            gbCalibration.Controls.Add(chkInvertLX); gbCalibration.Controls.Add(chkInvertLY);
            gbCalibration.Controls.Add(chkInvertRX); gbCalibration.Controls.Add(chkInvertRY);
            gbCalibration.Controls.Add(chkEnableRumble); gbCalibration.Controls.Add(lblRumbleStr); gbCalibration.Controls.Add(nudRumbleStrength);
            this.Controls.Add(gbCalibration);

            // Group 2: Hotkeys
            GroupBox gbHotkeys = new GroupBox
            {
                Text = "Global Hotkeys",
                Location = new Point(20, 390),
                Size = new Size(370, 220),
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            tbHotkeyPause = AddHotkeyRow(gbHotkeys, "Pause:", "P", 25);
            tbHotkeySaveState = AddHotkeyRow(gbHotkeys, "Save State:", "F1", 55);
            tbHotkeyLoadState = AddHotkeyRow(gbHotkeys, "Load State:", "F3", 85);
            tbHotkeyFastForward = AddHotkeyRow(gbHotkeys, "Fast Forward:", "Tab", 115);
            tbHotkeyScreenshot = AddHotkeyRow(gbHotkeys, "Screenshot:", "F12", 145);
            tbHotkeyMenu = AddHotkeyRow(gbHotkeys, "Toggle Menu:", "Escape", 175);
            this.Controls.Add(gbHotkeys);

            // Group 3: Button Mappings
            GroupBox gbMappings = new GroupBox
            {
                Text = "Button & Direction Mappings",
                Location = new Point(410, 55),
                Size = new Size(380, 555),
                ForeColor = Color.FromArgb(156, 163, 175),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };

            Panel pnlMapScroll = new Panel
            {
                Location = new Point(10, 20),
                Size = new Size(360, 525),
                AutoScroll = true
            };

            var defaultMappings = PlayerControllerConfig.GetDefaultButtonMappings();
            int yPos = 5;
            foreach (var kvp in defaultMappings)
            {
                Label lblKey = new Label { Text = kvp.Key.Replace("_", " "), Location = new Point(5, yPos + 3), Size = new Size(140, 20), ForeColor = Color.White, Font = new Font("Segoe UI", 8.5F, FontStyle.Regular) };
                TextBox tbInput = new TextBox { Text = kvp.Value, Location = new Point(150, yPos), Size = new Size(180, 22), BackColor = Color.FromArgb(44, 44, 52), ForeColor = Color.White };
                
                pnlMapScroll.Controls.Add(lblKey);
                pnlMapScroll.Controls.Add(tbInput);
                _mappingInputs[kvp.Key] = tbInput;

                yPos += 28;
            }

            gbMappings.Controls.Add(pnlMapScroll);
            this.Controls.Add(gbMappings);

            // Bottom Command Buttons
            btnSave = new Button
            {
                Text = "💾 Save & Apply Profile",
                Location = new Point(20, 620),
                Size = new Size(180, 35),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(16, 185, 129),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += btnSave_Click;
            this.Controls.Add(btnSave);

            btnSyncAll = new Button
            {
                Text = "🔄 Sync All Emulators",
                Location = new Point(210, 620),
                Size = new Size(180, 35),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(79, 70, 229),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnSyncAll.FlatAppearance.BorderSize = 0;
            btnSyncAll.Click += btnSyncAll_Click;
            this.Controls.Add(btnSyncAll);

            btnClose = new Button
            {
                Text = "Close",
                Location = new Point(690, 620),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.FromArgb(107, 114, 128),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private TextBox AddHotkeyRow(GroupBox gb, string labelText, string defaultValue, int y)
        {
            Label lbl = new Label { Text = labelText, Location = new Point(15, y + 3), AutoSize = true, ForeColor = Color.White, Font = new Font("Segoe UI", 8.5F, FontStyle.Regular) };
            TextBox tb = new TextBox { Text = defaultValue, Location = new Point(140, y), Size = new Size(200, 22), BackColor = Color.FromArgb(44, 44, 52), ForeColor = Color.White };
            gb.Controls.Add(lbl);
            gb.Controls.Add(tb);
            return tb;
        }

        private void LoadConfigToUI()
        {
            var config = GlobalControllerConfigManager.Instance.Config;
            chkAutoSyncOnLaunch.Checked = config.AutoSyncOnLaunch;

            // Load Hotkeys
            tbHotkeyPause.Text = config.Hotkeys.Pause;
            tbHotkeySaveState.Text = config.Hotkeys.SaveState;
            tbHotkeyLoadState.Text = config.Hotkeys.LoadState;
            tbHotkeyFastForward.Text = config.Hotkeys.FastForward;
            tbHotkeyScreenshot.Text = config.Hotkeys.Screenshot;
            tbHotkeyMenu.Text = config.Hotkeys.ToggleMenu;

            // Load Player Config
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
                MessageBox.Show(
                    this,
                    $"Global controller settings saved successfully!\nApplied to {successCount} / {syncResults.Count} emulators.",
                    "Controller Profile Saved",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
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
