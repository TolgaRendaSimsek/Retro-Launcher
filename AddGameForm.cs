using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RetroLauncher
{
    public partial class AddGameForm : Form
    {
        public Game CreatedGame { get; private set; } = new();
        private readonly Game? _gameToEdit = null;
        
        // ... (rest of class variables)
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

        // Compatibility keywords mapping for warnings
        private static readonly System.Collections.Generic.Dictionary<string, string[]> ConsoleKeywords = new(StringComparer.OrdinalIgnoreCase)
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

        public AddGameForm(Game? gameToEdit = null)
        {
            _gameToEdit = gameToEdit;
            InitializeComponent();
            SetupForm();
            LoadEditValues();
        }

        private void SetupForm()
        {
            // Populate ComboBox categories
            cbConsole.Items.Clear();
            foreach (var console in _consoles)
            {
                cbConsole.Items.Add(console);
            }
            if (cbConsole.Items.Count > 0)
            {
                cbConsole.SelectedIndex = 0;
            }

            // Bind click handlers
            btnBrowseEmulator.Click += btnBrowseEmulator_Click;
            btnChooseManual.Click += btnChooseManual_Click;
            btnBrowseRom.Click += btnBrowseRom_Click;
            btnBrowseCover.Click += btnBrowseCover_Click;
            btnBrowseHero.Click += btnBrowseHero_Click;
            btnBrowseLogo.Click += btnBrowseLogo_Click;
            btnBrowseIcon.Click += btnBrowseIcon_Click;
            btnBrowseScreenshots.Click += btnBrowseScreenshots_Click;
            btnBrowseTrailer.Click += btnBrowseTrailer_Click;
            btnSave.Click += btnSave_Click;
            btnCancel.Click += (s, e) => this.Close();

            // Hover transitions
            SetupHover(btnSave, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105));
            SetupHover(btnCancel, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupHover(btnChooseManual, Color.FromArgb(44, 44, 52), Color.FromArgb(31, 31, 36));
            SetupHover(btnBrowseHero, Color.FromArgb(44, 44, 52), Color.FromArgb(31, 31, 36));
            SetupHover(btnBrowseLogo, Color.FromArgb(44, 44, 52), Color.FromArgb(31, 31, 36));
            SetupHover(btnBrowseIcon, Color.FromArgb(44, 44, 52), Color.FromArgb(31, 31, 36));
            SetupHover(btnBrowseScreenshots, Color.FromArgb(44, 44, 52), Color.FromArgb(31, 31, 36));
            SetupHover(btnBrowseTrailer, Color.FromArgb(44, 44, 52), Color.FromArgb(31, 31, 36));
        }

        private void LoadEditValues()
        {
            if (_gameToEdit != null)
            {
                this.Text = "Edit Game Details";
                btnSave.Text = "Save Changes";
                tbName.Text = _gameToEdit.Title;
                
                int index = cbConsole.Items.IndexOf(_gameToEdit.Platform);
                if (index >= 0)
                {
                    cbConsole.SelectedIndex = index;
                }
                
                var registeredEmu = EmulatorManager.Instance.FindEmulator(_gameToEdit.EmulatorId);
                tbEmulator.Text = registeredEmu != null ? registeredEmu.Path : _gameToEdit.EmulatorId;
                tbRom.Text = _gameToEdit.RomPath;
                tbCover.Text = _gameToEdit.CoverImagePath;
                tbHero.Text = _gameToEdit.HeroImagePath;
                tbLogo.Text = _gameToEdit.LogoImagePath;
                tbIcon.Text = _gameToEdit.IconImagePath;
                tbScreenshots.Text = string.Join("; ", _gameToEdit.ScreenshotPaths);
                tbTrailer.Text = _gameToEdit.TrailerVideoPath;
            }
        }

        private void SetupHover(Button btn, Color baseColor, Color hoverColor)
        {
            btn.BackColor = baseColor;
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = baseColor;
        }

        private void btnBrowseRom_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Game ROM File";
                
                // Allow directory selection for folder games (PlayStation 3) by using custom filters or letting them browse files
                ofd.Filter = "ROM / Game Files (*.cue;*.m3u;*.cso;*.pkg;*.rap;*.nes;*.sfc;*.smc;*.n64;*.z64;*.md;*.gba;*.iso;*.chd;*.bin)|*.cue;*.m3u;*.cso;*.pkg;*.rap;*.nes;*.sfc;*.smc;*.n64;*.z64;*.md;*.gba;*.iso;*.chd;*.bin|All Files (*.*)|*.*";
                ofd.InitialDirectory = ResolvePath(string.IsNullOrEmpty(tbRom.Text) ? "Games" : tbRom.Text);

                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    string filePath = ofd.FileName;
                    tbRom.Text = MakeRelativePath(filePath);

                    // If adding a new game, automatically set the name from the file name
                    if (string.IsNullOrEmpty(tbName.Text.Trim()))
                    {
                        tbName.Text = Path.GetFileNameWithoutExtension(filePath);
                    }

                    // Run automatic ROM detection logic
                    lblAutoDetect.Text = "Detecting console platform...";
                    RomDetector.DetectConsoleAndEmulator(filePath, this, out string detectedConsole, out string recommendedEmu);

                    if (!string.IsNullOrEmpty(detectedConsole))
                    {
                        // Auto-fill Console ComboBox
                        int idx = cbConsole.Items.IndexOf(detectedConsole);
                        if (idx >= 0)
                        {
                            cbConsole.SelectedIndex = idx;
                        }

                        lblAutoDetect.Text = $"Detected: {detectedConsole}";

                        if (!string.IsNullOrEmpty(recommendedEmu))
                        {
                            tbEmulator.Text = recommendedEmu;
                            lblAutoDetect.Text += $" | Recommended: {Path.GetFileName(recommendedEmu)}";
                        }
                        else
                        {
                            // Auto-detection succeeded for console, but NO emulator mapping was found.
                            // Automatically launch the manual emulator selection dialog!
                            lblAutoDetect.Text += " | No default emulator configured.";
                            MessageBox.Show(
                                $"Detected Platform: {detectedConsole}\n\nNo default emulator path is configured for this console. Please choose one from the selection menu.",
                                "Configure Emulator",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Information
                            );

                            using (var selector = new EmulatorSelectorForm(detectedConsole))
                            {
                                if (selector.ShowDialog(this) == DialogResult.OK)
                                {
                                    tbEmulator.Text = selector.SelectedEmulatorPath;
                                }
                            }
                        }
                    }
                    else
                    {
                        // Auto-detection failed (unrecognized extension)
                        lblAutoDetect.Text = "Detection failed. Please select category and emulator manually.";
                        
                        // Automatically open selection dialog for manually mapping
                        MessageBox.Show(
                            "The ROM file extension is unrecognized or ambiguous.\n\nPlease select the console category and emulator manually.",
                            "Platform Unrecognized",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                }
            }
        }

        private void btnChooseManual_Click(object? sender, EventArgs e)
        {
            string selectedConsole = cbConsole.SelectedItem?.ToString() ?? "";
            if (string.IsNullOrEmpty(selectedConsole))
            {
                MessageBox.Show("Please select a console category first.", "Console Category Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var selector = new EmulatorSelectorForm(selectedConsole))
            {
                if (selector.ShowDialog(this) == DialogResult.OK)
                {
                    tbEmulator.Text = selector.SelectedEmulatorPath;
                }
            }
        }

        private void btnBrowseEmulator_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Emulator Executable";
                ofd.Filter = "Executables (*.exe)|*.exe|All Files (*.*)|*.*";
                ofd.InitialDirectory = ResolvePath(string.IsNullOrEmpty(tbEmulator.Text) ? "Emulators" : tbEmulator.Text);

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    tbEmulator.Text = MakeRelativePath(ofd.FileName);
                }
            }
        }

        private void btnBrowseCover_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Cover Image";
                ofd.Filter = "Image Files (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp|All Files (*.*)|*.*";
                ofd.InitialDirectory = ResolvePath(string.IsNullOrEmpty(tbCover.Text) ? "Assets/Covers" : tbCover.Text);

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    tbCover.Text = MakeRelativePath(ofd.FileName);
                }
            }
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            string name = tbName.Text.Trim();
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Game Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                tbName.Focus();
                return;
            }

            string selectedConsole = cbConsole.SelectedItem?.ToString() ?? "";
            string emulatorPath = tbEmulator.Text.Trim();
            string logicalEmulatorId = EmulatorManager.Instance.ResolveAndRegisterEmulatorId(emulatorPath, selectedConsole);

            // Run GDI+ / File Path Compatibility warnings
            if (!string.IsNullOrEmpty(selectedConsole) && !string.IsNullOrEmpty(emulatorPath))
            {
                bool compatible = CheckCompatibility(selectedConsole, emulatorPath);
                if (!compatible)
                {
                    var result = MessageBox.Show(
                        $"Warning: The selected emulator executable path may not match the console format ({selectedConsole}).\n\nEmulator: '{emulatorPath}'\n\nDo you want to proceed and save this setting anyway?",
                        "Emulator Compatibility Warning",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning
                    );

                    if (result == DialogResult.No)
                    {
                        return; // Stay in dialog
                    }
                }
            }

            string gameId = _gameToEdit?.Id ?? Guid.NewGuid().ToString();

            // Process and copy media assets to the local media/ folder using relative path references
            string finalCover = ProcessMediaField(tbCover.Text.Trim(), gameId, "cover");
            string finalHero = ProcessMediaField(tbHero.Text.Trim(), gameId, "hero");
            string finalLogo = ProcessMediaField(tbLogo.Text.Trim(), gameId, "logo");
            string finalIcon = ProcessMediaField(tbIcon.Text.Trim(), gameId, "icon");
            var finalScreenshots = ProcessScreenshots(tbScreenshots.Text.Trim(), gameId);
            string finalTrailer = ProcessMediaField(tbTrailer.Text.Trim(), gameId, "trailer");

            if (_gameToEdit != null)
            {
                _gameToEdit.Title = name;
                _gameToEdit.Platform = selectedConsole;
                _gameToEdit.EmulatorId = logicalEmulatorId;
                _gameToEdit.RomPath = tbRom.Text.Trim();
                _gameToEdit.CoverImagePath = finalCover;
                _gameToEdit.HeroImagePath = finalHero;
                _gameToEdit.LogoImagePath = finalLogo;
                _gameToEdit.IconImagePath = finalIcon;
                _gameToEdit.ScreenshotPaths = finalScreenshots;
                _gameToEdit.TrailerVideoPath = finalTrailer;
                CreatedGame = _gameToEdit;
            }
            else
            {
                CreatedGame = new Game
                {
                    Id = gameId,
                    Title = name,
                    Platform = selectedConsole,
                    EmulatorId = logicalEmulatorId,
                    RomPath = tbRom.Text.Trim(),
                    CoverImagePath = finalCover,
                    HeroImagePath = finalHero,
                    LogoImagePath = finalLogo,
                    IconImagePath = finalIcon,
                    ScreenshotPaths = finalScreenshots,
                    TrailerVideoPath = finalTrailer
                };

                // Auto-fetch game metadata on creation using LocalMetadataProvider
                try
                {
                    var provider = new LocalMetadataProvider();
                    var results = provider.SearchGame(name, selectedConsole);
                    if (results != null && results.Count > 0)
                    {
                        var firstResult = results[0];
                        var meta = provider.GetGameDetails(firstResult.GameId);
                        if (meta != null)
                        {
                            CreatedGame.ReleaseDate = meta.ReleaseDate;
                            CreatedGame.ReleaseYear = meta.ReleaseYear;
                            CreatedGame.Genre = meta.Genre;
                            CreatedGame.Developer = meta.Developer;
                            CreatedGame.Publisher = meta.Publisher;
                            CreatedGame.Description = meta.Description;
                            CreatedGame.PlayerCount = meta.PlayerCount;
                            CreatedGame.Region = meta.Region;
                            CreatedGame.FileFormat = meta.FileFormat;
                            CreatedGame.GameId = meta.GameId;
                            CreatedGame.Tags = meta.Tags;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to auto-fetch metadata: {ex.Message}");
                }
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private string ProcessMediaField(string sourcePath, string gameId, string assetType)
        {
            if (string.IsNullOrWhiteSpace(sourcePath)) return "";
            string normalized = sourcePath.Replace('\\', '/');
            if (normalized.Contains($"media/{gameId}/{assetType}.")) return sourcePath; // already formatted/copied
            
            string resolved = ResolvePath(sourcePath);
            if (assetType == "cover") return MediaManager.AddCoverImage(gameId, resolved);
            if (assetType == "hero") return MediaManager.AddHeroImage(gameId, resolved);
            if (assetType == "logo") return MediaManager.AddLogoImage(gameId, resolved);
            if (assetType == "icon") return MediaManager.AddIconImage(gameId, resolved);
            
            return MediaManager.AddMediaFile(gameId, resolved, assetType);
        }

        private System.Collections.Generic.List<string> ProcessScreenshots(string input, string gameId)
        {
            var list = new System.Collections.Generic.List<string>();
            if (string.IsNullOrWhiteSpace(input)) return list;
            var paths = input.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var path in paths)
            {
                string trimmed = path.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;
                string normalized = trimmed.Replace('\\', '/');
                if (normalized.Contains($"media/{gameId}/screenshot_"))
                {
                    list.Add(trimmed);
                }
                else
                {
                    string copied = MediaManager.AddScreenshot(gameId, ResolvePath(trimmed));
                    if (!string.IsNullOrEmpty(copied))
                    {
                        list.Add(copied);
                    }
                }
            }
            return list;
        }

        private void btnBrowseHero_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Hero Banner Image";
                ofd.Filter = "Image Files (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp|All Files (*.*)|*.*";
                ofd.InitialDirectory = ResolvePath(string.IsNullOrEmpty(tbHero.Text) ? "Assets/Banners" : tbHero.Text);
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    tbHero.Text = MakeRelativePath(ofd.FileName);
                }
            }
        }

        private void btnBrowseLogo_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Logo Image";
                ofd.Filter = "Image Files (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp|All Files (*.*)|*.*";
                ofd.InitialDirectory = ResolvePath(string.IsNullOrEmpty(tbLogo.Text) ? "Assets/Logos" : tbLogo.Text);
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    tbLogo.Text = MakeRelativePath(ofd.FileName);
                }
            }
        }

        private void btnBrowseIcon_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Icon Image";
                ofd.Filter = "Image Files (*.jpg;*.jpeg;*.png;*.gif;*.ico)|*.jpg;*.jpeg;*.png;*.gif;*.ico|All Files (*.*)|*.*";
                ofd.InitialDirectory = ResolvePath(string.IsNullOrEmpty(tbIcon.Text) ? "Assets/Icons" : tbIcon.Text);
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    tbIcon.Text = MakeRelativePath(ofd.FileName);
                }
            }
        }

        private void btnBrowseScreenshots_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Screenshot Images";
                ofd.Filter = "Image Files (*.jpg;*.jpeg;*.png;*.webp)|*.jpg;*.jpeg;*.png;*.webp|All Files (*.*)|*.*";
                ofd.Multiselect = true;
                ofd.InitialDirectory = ResolvePath("Assets/Screenshots");
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    var relativePaths = ofd.FileNames.Select(MakeRelativePath);
                    tbScreenshots.Text = string.Join("; ", relativePaths);
                }
            }
        }

        private void btnBrowseTrailer_Click(object? sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Select Trailer Video File";
                ofd.Filter = "Video Files (*.mp4;*.mkv;*.avi;*.wmv)|*.mp4;*.mkv;*.avi;*.wmv|All Files (*.*)|*.*";
                ofd.InitialDirectory = ResolvePath(string.IsNullOrEmpty(tbTrailer.Text) ? "Assets/Videos" : tbTrailer.Text);
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    tbTrailer.Text = MakeRelativePath(ofd.FileName);
                }
            }
        }

        private bool CheckCompatibility(string console, string path)
        {
            string normalizedPath = path.Replace('\\', '/').ToLower();
            string fileName = Path.GetFileName(normalizedPath);

            if (ConsoleKeywords.TryGetValue(console, out string[]? keywords))
            {
                return keywords.Any(keyword => normalizedPath.Contains(keyword) || fileName.Contains(keyword));
            }
            return true;
        }

        private string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path)) return path;

            string baseDir = AppContext.BaseDirectory;
            string testPath1 = Path.Combine(baseDir, path);
            if (Directory.Exists(testPath1) || File.Exists(testPath1)) return testPath1;

            string testPath2 = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (Directory.Exists(testPath2) || File.Exists(testPath2)) return testPath2;

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
    }
}
