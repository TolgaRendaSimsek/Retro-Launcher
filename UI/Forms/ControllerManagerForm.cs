using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RetroLauncher.UI.Forms
{
    public partial class ControllerManagerForm : Form
    {
        private GameLibraryManager _libraryManager = new();
        private ControllerProfile? _selectedProfile;

        // Cache map to easily extract selected mapping values from the dynamically added ComboBoxes
        private Dictionary<string, ComboBox> _mappingDropdowns = new();

        private readonly string[] _physicalInputsList = new[]
        {
            "Button 0", "Button 1", "Button 2", "Button 3",
            "Button 4", "Button 5", "Button 6", "Button 7",
            "Button 8", "Button 9", "Button 10", "Button 11",
            "Button 12", "Button 13", "Button 14", "Button 15",
            "Axis X+", "Axis X-", "Axis Y+", "Axis Y-",
            "Axis Z+", "Axis Z-", "Axis R+", "Axis R-"
        };

        public ControllerManagerForm()
        {
            InitializeComponent();
            SetupCustomEvents();
        }

        private void SetupCustomEvents()
        {
            this.Load += ControllerManagerForm_Load;
            lbProfiles.SelectedIndexChanged += lbProfiles_SelectedIndexChanged;
            lbProfiles.DrawItem += lbProfiles_DrawItem;

            btnScan.Click += btnScan_Click;
            btnCreate.Click += btnCreate_Click;
            btnDelete.Click += btnDelete_Click;
            btnTestInput.Click += btnTestInput_Click;
            btnSaveProfile.Click += btnSaveProfile_Click;
            btnClose.Click += (s, e) => this.Close();

            // Setup hover styles
            SetupButtonHover(btnScan, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupButtonHover(btnCreate, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105));
            SetupButtonHover(btnDelete, Color.FromArgb(239, 68, 68), Color.FromArgb(220, 38, 38));
            SetupButtonHover(btnTestInput, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupButtonHover(btnSaveProfile, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupButtonHover(btnClose, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
        }

        private void SetupButtonHover(Button btn, Color normal, Color hover)
        {
            btn.BackColor = normal;
            btn.MouseEnter += (s, e) => btn.BackColor = hover;
            btn.MouseLeave += (s, e) => btn.BackColor = normal;
        }

        private void ControllerManagerForm_Load(object? sender, EventArgs e)
        {
            ThemeManager.Instance.ThemeChanged += (s, ev) => ThemeManager.Instance.ApplyTheme(this);
            ThemeManager.Instance.ApplyTheme(this);
            LocalizationManager.Instance.LanguageChanged += (s, ev) => LocalizationManager.Instance.ApplyLanguage(this);
            LocalizationManager.Instance.ApplyLanguage(this);
            PopulateDropdowns();
            ScanDevices();
            RefreshProfilesList();
            BuildMappingGrid();
        }

        private void PopulateDropdowns()
        {
            // Controller Types
            cbControllerType.Items.Clear();
            foreach (var type in Enum.GetNames(typeof(ControllerType)))
            {
                cbControllerType.Items.Add(type);
            }
            cbControllerType.SelectedIndex = 0;

            // Target Emulators
            cbTargetEmulator.Items.Clear();
            cbTargetEmulator.Items.Add("None");
            cbTargetEmulator.Items.Add("DuckStation");
            cbTargetEmulator.Items.Add("PCSX2");
            cbTargetEmulator.Items.Add("RPCS3");
            cbTargetEmulator.SelectedIndex = 0;

            // Target Games
            cbTargetGame.Items.Clear();
            cbTargetGame.Items.Add(new GameComboItem { Id = null, Title = "None" });
            foreach (var game in _libraryManager.Games)
            {
                cbTargetGame.Items.Add(new GameComboItem { Id = game.Id, Title = game.Title });
            }
            cbTargetGame.SelectedIndex = 0;
        }

        private void ScanDevices()
        {
            lvDevices.Items.Clear();
            var devices = ControllerManager.Instance.DetectConnectedControllers();

            if (devices.Count == 0)
            {
                var item = new ListViewItem("-");
                item.SubItems.Add("No active controllers detected. Connect a gamepad and click Scan.");
                item.SubItems.Add("-");
                item.SubItems.Add("-");
                lvDevices.Items.Add(item);
                return;
            }

            foreach (var dev in devices)
            {
                var item = new ListViewItem(dev.Id.ToString());
                item.SubItems.Add(dev.ProductName);
                item.SubItems.Add(dev.Type.ToString());
                item.SubItems.Add(dev.Status);
                lvDevices.Items.Add(item);
            }
        }

        private void RefreshProfilesList()
        {
            lbProfiles.Items.Clear();
            foreach (var p in ControllerManager.Instance.Profiles)
            {
                lbProfiles.Items.Add(p);
            }

            if (lbProfiles.Items.Count > 0)
            {
                lbProfiles.SelectedIndex = 0;
            }
            else
            {
                ClearMappingConfigPanel();
            }
        }

        private void BuildMappingGrid()
        {
            flpMappings.Controls.Clear();
            _mappingDropdowns.Clear();

            // The virtual buttons we want to map
            string[] virtualButtons = new[]
            {
                "Dpad_Up", "Dpad_Down", "Dpad_Left", "Dpad_Right",
                "A_Button", "B_Button", "X_Button", "Y_Button",
                "L1_Shoulder", "R1_Shoulder", "L2_Trigger", "R2_Trigger",
                "L3_Stick", "R3_Stick", "Start", "Select"
            };

            foreach (var btnName in virtualButtons)
            {
                Panel row = new Panel
                {
                    Size = new Size(260, 30),
                    Margin = new Padding(3)
                };

                Label lblName = new Label
                {
                    Text = btnName.Replace("_", " "),
                    Location = new Point(5, 5),
                    Size = new Size(110, 20),
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Regular)
                };

                ComboBox cbInput = new ComboBox
                {
                    Location = new Point(125, 2),
                    Size = new Size(125, 23),
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(36, 36, 42),
                    ForeColor = Color.White
                };

                cbInput.Items.AddRange(_physicalInputsList);
                cbInput.SelectedIndex = 0;

                row.Controls.Add(lblName);
                row.Controls.Add(cbInput);
                flpMappings.Controls.Add(row);

                _mappingDropdowns[btnName] = cbInput;
            }
        }

        private void lbProfiles_SelectedIndexChanged(object? sender, EventArgs e)
        {
            _selectedProfile = lbProfiles.SelectedItem as ControllerProfile;
            LoadProfileToConfigPanel();
        }

        private void LoadProfileToConfigPanel()
        {
            if (_selectedProfile == null)
            {
                ClearMappingConfigPanel();
                return;
            }

            tbProfileName.Text = _selectedProfile.Name;

            // Set Device Type
            int typeIdx = cbControllerType.FindStringExact(_selectedProfile.ControllerTypeName);
            cbControllerType.SelectedIndex = typeIdx >= 0 ? typeIdx : 0;

            // Set Target Emulator
            if (string.IsNullOrEmpty(_selectedProfile.TargetEmulatorId))
            {
                cbTargetEmulator.SelectedIndex = 0; // None
            }
            else
            {
                int emuIdx = cbTargetEmulator.FindStringExact(_selectedProfile.TargetEmulatorId);
                cbTargetEmulator.SelectedIndex = emuIdx >= 0 ? emuIdx : 0;
            }

            // Set Target Game
            if (string.IsNullOrEmpty(_selectedProfile.TargetGameId))
            {
                cbTargetGame.SelectedIndex = 0; // None
            }
            else
            {
                bool found = false;
                for (int i = 0; i < cbTargetGame.Items.Count; i++)
                {
                    var item = cbTargetGame.Items[i] as GameComboItem;
                    if (item != null && item.Id == _selectedProfile.TargetGameId)
                    {
                        cbTargetGame.SelectedIndex = i;
                        found = true;
                        break;
                    }
                }
                if (!found) cbTargetGame.SelectedIndex = 0;
            }

            // Bind Mappings
            foreach (var kv in _mappingDropdowns)
            {
                if (_selectedProfile.Mappings.TryGetValue(kv.Key, out string? physicalInput))
                {
                    int inputIdx = kv.Value.FindStringExact(physicalInput);
                    kv.Value.SelectedIndex = inputIdx >= 0 ? inputIdx : 0;
                }
                else
                {
                    kv.Value.SelectedIndex = 0;
                }
            }
        }

        private void ClearMappingConfigPanel()
        {
            tbProfileName.Clear();
            cbControllerType.SelectedIndex = 0;
            cbTargetEmulator.SelectedIndex = 0;
            cbTargetGame.SelectedIndex = 0;

            foreach (var kv in _mappingDropdowns)
            {
                kv.Value.SelectedIndex = 0;
            }
        }

        private void btnScan_Click(object? sender, EventArgs e)
        {
            ScanDevices();
            MessageBox.Show("Controller scanning completed!", "Scan Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnCreate_Click(object? sender, EventArgs e)
        {
            string pName = InputPrompt.Show("Enter a name for the new controller profile:", "Create Profile", "My New Profile");
            if (string.IsNullOrWhiteSpace(pName)) return;

            var newProfile = ControllerManager.Instance.CreateControllerProfile(pName, "Generic");
            RefreshProfilesList();

            // Select new profile
            for (int i = 0; i < lbProfiles.Items.Count; i++)
            {
                if ((lbProfiles.Items[i] as ControllerProfile)?.Id == newProfile.Id)
                {
                    lbProfiles.SelectedIndex = i;
                    break;
                }
            }
        }

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            if (_selectedProfile == null)
            {
                MessageBox.Show("Please select a profile to delete.", "Delete Profile", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show($"Are you sure you want to delete profile '{_selectedProfile.Name}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            if (ControllerManager.Instance.DeleteControllerProfile(_selectedProfile.Id))
            {
                RefreshProfilesList();
            }
        }

        private void btnSaveProfile_Click(object? sender, EventArgs e)
        {
            if (_selectedProfile == null)
            {
                MessageBox.Show("No active profile selected to save.", "Save Config", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string updatedName = tbProfileName.Text.Trim();
            if (string.IsNullOrEmpty(updatedName))
            {
                MessageBox.Show("Profile Name cannot be empty.", "Save Config", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _selectedProfile.Name = updatedName;
            _selectedProfile.ControllerTypeName = cbControllerType.SelectedItem?.ToString() ?? "Generic";

            // Set Target Emulator
            string selectedEmu = cbTargetEmulator.SelectedItem?.ToString() ?? "None";
            _selectedProfile.TargetEmulatorId = (selectedEmu == "None") ? null : selectedEmu;

            // Set Target Game
            var selectedGameItem = cbTargetGame.SelectedItem as GameComboItem;
            _selectedProfile.TargetGameId = selectedGameItem?.Id;

            // Mutually exclusive: if target game is assigned, clear emulator target, and vice-versa
            if (_selectedProfile.TargetGameId != null)
            {
                _selectedProfile.TargetEmulatorId = null;
            }

            // Extract dropdown mappings
            foreach (var kv in _mappingDropdowns)
            {
                _selectedProfile.Mappings[kv.Key] = kv.Value.SelectedItem?.ToString() ?? "Button 0";
            }

            // Save database changes
            if (ControllerManager.Instance.EditControllerProfile(_selectedProfile))
            {
                // Synchronize assignments
                if (_selectedProfile.TargetGameId != null)
                {
                    ControllerManager.Instance.AssignProfileToGame(_selectedProfile.TargetGameId, _selectedProfile.Id);
                }
                else if (_selectedProfile.TargetEmulatorId != null)
                {
                    ControllerManager.Instance.AssignProfileToEmulator(_selectedProfile.TargetEmulatorId, _selectedProfile.Id);
                }
                else
                {
                    // Clear assignments if set to None
                    ControllerManager.Instance.AssignProfileToGame(_selectedProfile.Id, null);
                }

                MessageBox.Show("Profile configuration saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                int activeIndex = lbProfiles.SelectedIndex;
                RefreshProfilesList();
                if (lbProfiles.Items.Count > activeIndex)
                {
                    lbProfiles.SelectedIndex = activeIndex;
                }
            }
        }

        private void lbProfiles_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            e.DrawBackground();

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bgColor = isSelected ? Color.FromArgb(99, 102, 241) : Color.FromArgb(31, 31, 35);
            Color fgColor = Color.White;

            using (Brush bgBrush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(bgBrush, e.Bounds);
            }

            var profile = lbProfiles.Items[e.Index] as ControllerProfile;
            if (profile != null)
            {
                // Display assignment badges
                string displayLabel = profile.Name;
                if (!string.IsNullOrEmpty(profile.TargetEmulatorId))
                {
                    displayLabel += $" [Emu: {profile.TargetEmulatorId}]";
                }
                else if (!string.IsNullOrEmpty(profile.TargetGameId))
                {
                    var targetGame = _libraryManager.Games.FirstOrDefault(g => g.Id == profile.TargetGameId);
                    displayLabel += $" [Game: {(targetGame != null ? targetGame.Title : "Assigning")}]";
                }

                Rectangle textBounds = new Rectangle(e.Bounds.Left + 10, e.Bounds.Top, e.Bounds.Width - 10, e.Bounds.Height);
                TextRenderer.DrawText(
                    e.Graphics,
                    displayLabel,
                    e.Font ?? this.Font,
                    textBounds,
                    fgColor,
                    bgColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter
                );
            }

            e.DrawFocusRectangle();
        }

        private void btnTestInput_Click(object? sender, EventArgs e)
        {
            using (var testForm = new ControllerTestInputForm())
            {
                testForm.ShowDialog(this);
            }
        }
    }

    public class GameComboItem
    {
        public string? Id { get; set; }
        public string Title { get; set; } = "";

        public override string ToString()
        {
            return Title;
        }
    }
}
