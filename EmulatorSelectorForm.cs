using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RetroLauncher
{
    public class EmulatorSelectionItem
    {
        public string Name { get; set; } = "";
        public string Path { get; set; } = "";

        public override string ToString()
        {
            return $"{Name} [{Path}]";
        }
    }

    public partial class EmulatorSelectorForm : Form
    {
        public string SelectedEmulatorPath { get; private set; } = "";
        
        private readonly string _consoleCategory;

        // Keyword map to check emulator compatibility based on file path name
        private static readonly Dictionary<string, string[]> ConsoleCompatibilityKeywords = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Sony PlayStation 1", new[] { "ps1", "playstation 1", "duckstation", "pcsx" } },
            { "Sony PlayStation 2", new[] { "ps2", "playstation 2", "pcsx2" } },
            { "Sony PlayStation 3", new[] { "ps3", "playstation 3", "rpcs3" } },
            { "Nintendo Entertainment System (NES)", new[] { "nes", "fceux", "nestopia", "retroarch" } },
            { "Super Nintendo (SNES)", new[] { "snes", "sfc", "smc", "snes9x", "bsnes", "retroarch" } },
            { "Nintendo 64", new[] { "n64", "z64", "project64", "mupen", "retroarch" } },
            { "Sega Genesis", new[] { "genesis", "sega", "megadrive", "fusion", "gens", "retroarch" } },
            { "Game Boy Advance", new[] { "gba", "gameboy", "mgb", "visualboy", "retroarch" } }
        };

        public EmulatorSelectorForm(string consoleCategory)
        {
            _consoleCategory = consoleCategory;
            InitializeComponent();
            SetupForm();
        }

        private void SetupForm()
        {
            this.Text = $"Select Emulator for {_consoleCategory}";
            lbEmulators.SelectedIndexChanged += lbEmulators_SelectedIndexChanged;
            btnOk.Click += btnOk_Click;
            btnCancel.Click += (s, e) => this.Close();

            // Hover transitions
            SetupHover(btnOk, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105));
            SetupHover(btnCancel, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));

            LoadEmulators();
        }

        private void SetupHover(Button btn, Color baseColor, Color hoverColor)
        {
            btn.BackColor = baseColor;
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = baseColor;
        }

        private void LoadEmulators()
        {
            lbEmulators.Items.Clear();
            var addedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // 1. Add registered emulators from emulators.json
            var emuConfig = EmulatorManager.LoadConfig();
            foreach (var emu in emuConfig.Emulators)
            {
                lbEmulators.Items.Add(new EmulatorSelectionItem { Name = emu.Name, Path = emu.Path });
                addedPaths.Add(emu.Path);
            }

            // 2. Scan Emulators directory recursively for .exe files
            string emuFolder = Path.Combine(AppContext.BaseDirectory, "Emulators");
            if (!Directory.Exists(emuFolder))
            {
                emuFolder = Path.Combine(Directory.GetCurrentDirectory(), "Emulators");
            }

            if (Directory.Exists(emuFolder))
            {
                try
                {
                    string[] exes = Directory.GetFiles(emuFolder, "*.exe", SearchOption.AllDirectories);
                    foreach (var exe in exes)
                    {
                        string relative = MakeRelativePath(exe);
                        if (!addedPaths.Contains(relative))
                        {
                            string name = Path.GetFileNameWithoutExtension(exe);
                            lbEmulators.Items.Add(new EmulatorSelectionItem { Name = $"{name} (Discovered)", Path = relative });
                            addedPaths.Add(relative);
                        }
                    }
                }
                catch { }
            }

            if (lbEmulators.Items.Count > 0)
            {
                lbEmulators.SelectedIndex = 0;
            }
            else
            {
                lblWarning.Text = "No emulators found on disk. Click Cancel and use the Emulator Manager to add one.";
            }
        }

        private void lbEmulators_SelectedIndexChanged(object? sender, EventArgs e)
        {
            lblWarning.Text = "";

            if (lbEmulators.SelectedItem is EmulatorSelectionItem selectedEmu)
            {
                // Run compatibility validation check
                bool compatible = CheckCompatibility(selectedEmu.Path);
                if (!compatible)
                {
                    lblWarning.Text = "⚠️ Warning: The selected emulator may not support this ROM format. Ensure you map the correct executable.";
                }
            }
        }

        private bool CheckCompatibility(string path)
        {
            if (string.IsNullOrEmpty(path)) return true;

            string normalizedPath = path.Replace('\\', '/').ToLower();
            string fileName = Path.GetFileName(normalizedPath);

            if (ConsoleCompatibilityKeywords.TryGetValue(_consoleCategory, out string[]? keywords))
            {
                // If the path or file name contains any of the console keywords, we assume it's compatible
                return keywords.Any(keyword => normalizedPath.Contains(keyword) || fileName.Contains(keyword));
            }

            return true;
        }

        private void btnOk_Click(object? sender, EventArgs e)
        {
            if (lbEmulators.SelectedItem is not EmulatorSelectionItem selectedEmu)
            {
                MessageBox.Show("Please select an emulator from the list.", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SelectedEmulatorPath = selectedEmu.Path;

            // Save default setting preference if requested
            if (chkRemember.Checked)
            {
                var settings = SettingsManager.LoadSettings();
                settings.DefaultEmulators[_consoleCategory] = selectedEmu.Path;
                SettingsManager.SaveSettings(settings);
            }

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

            return Path.Combine(Directory.GetCurrentDirectory(), path);
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
    }
}
