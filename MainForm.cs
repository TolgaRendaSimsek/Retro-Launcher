using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RetroLauncher
{
    public partial class MainForm : Form
    {
        private readonly GameLibraryManager _libraryManager = new();
        private Game? _selectedGame = null;
        private Control? _selectedCard = null;
        private bool _isGridView = true;

        public static event EventHandler<string>? GameProcessExited;
        private System.Windows.Forms.Timer? _mainSessionTimer;

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;
        private const int VIDEO_HOTKEY_ID = 9001;
        private const int WM_HOTKEY = 0x0312;

        private readonly string[] _consolesList = new[]
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

        public MainForm()
        {
            InitializeComponent();
            SetupFormEvents();
        }

        private void SetupFormEvents()
        {
            this.Load += MainForm_Load;

            // Session timer initialization
            _mainSessionTimer = new System.Windows.Forms.Timer { Interval = 1000 };
            _mainSessionTimer.Tick += MainSessionTimer_Tick;
            this.Load += (s, e) =>
            {
                _mainSessionTimer.Start();
                RegisterHotKey(this.Handle, HOTKEY_ID, 0, 0x7B); // F12
                RegisterHotKey(this.Handle, VIDEO_HOTKEY_ID, 0, 0x79); // F10
                ThemeManager.Instance.ThemeChanged += (sender, args) => ThemeManager.Instance.ApplyTheme(this);
                ThemeManager.Instance.ApplyTheme(this);
                LocalizationManager.Instance.LanguageChanged += (sender, args) => LocalizationManager.Instance.ApplyLanguage(this);
                LocalizationManager.Instance.ApplyLanguage(this);
            };
            this.FormClosed += (s, e) => {
                _mainSessionTimer.Stop();
                _mainSessionTimer.Dispose();
                UnregisterHotKey(this.Handle, HOTKEY_ID);
                UnregisterHotKey(this.Handle, VIDEO_HOTKEY_ID);
            };

            // Search box and console filter events
            tbSearch.TextChanged += (s, e) => RefreshGameList();
            lbConsoleFilter.SelectedIndexChanged += (s, e) => RefreshGameList();
            lbConsoleFilter.DrawItem += lbConsoleFilter_DrawItem;

            // Toolbar sorting, filtering, and view layout events
            btnGridView.Click += (s, e) => ToggleView(true);
            btnListView.Click += (s, e) => ToggleView(false);
            cbSort.SelectedIndexChanged += (s, e) => RefreshGameList();
            cbFilter.SelectedIndexChanged += (s, e) => RefreshGameList();

            // Action buttons
            btnAddGame.Click += btnAddGame_Click;
            btnManageEmulators.Click += btnManageEmulators_Click;
            btnProfile.Click += btnProfile_Click;
            btnPlay.Click += btnPlay_Click;
            btnEditPaths.Click += btnEditPaths_Click;
            btnDelete.Click += btnDelete_Click;
            btnManageSaves.Click += btnManageSaves_Click;
            btnManageScreenshots.Click += btnManageScreenshots_Click;
            btnManageVideos.Click += btnManageVideos_Click;
            btnManageControllers.Click += btnManageControllers_Click;
            btnAppearance.Click += btnAppearance_Click;
            btnLanguageSettings.Click += btnLanguageSettings_Click;

            romManagerToolStripMenuItem.Click += (s, e) => {
                MessageBox.Show("ROM Manager UI is under development.", "ROM Manager", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            biosManagerToolStripMenuItem.Click += (s, e) => {
                using (var form = new BiosManagerForm())
                {
                    form.ShowDialog(this);
                }
            };

            // Hover transitions
            SetupHoverEffect(btnPlay, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupHoverEffect(btnEditPaths, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupHoverEffect(btnDelete, Color.FromArgb(239, 68, 68), Color.FromArgb(220, 38, 38));
            SetupHoverEffect(btnAddGame, Color.FromArgb(16, 185, 129), Color.FromArgb(5, 150, 105));
            SetupHoverEffect(btnManageEmulators, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupHoverEffect(btnProfile, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupHoverEffect(btnManageSaves, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupHoverEffect(btnManageScreenshots, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupHoverEffect(btnManageVideos, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupHoverEffect(btnManageControllers, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupHoverEffect(btnAppearance, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupHoverEffect(btnLanguageSettings, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
        }

        private void SetupHoverEffect(Button btn, Color normalColor, Color hoverColor)
        {
            btn.BackColor = normalColor;
            btn.MouseEnter += (s, e) => btn.BackColor = hoverColor;
            btn.MouseLeave += (s, e) => btn.BackColor = normalColor;
        }

        private void MainForm_Load(object? sender, EventArgs e)
        {
            // Populate category sidebar
            lbConsoleFilter.Items.Clear();
            lbConsoleFilter.Items.Add("All Games");
            foreach (var console in _consolesList)
            {
                lbConsoleFilter.Items.Add(console);
            }
            lbConsoleFilter.SelectedIndex = 0;

            // Set initial sort and filter dropdown indexes
            cbSort.SelectedIndex = 0; // "Title A-Z"
            cbFilter.SelectedIndex = 0; // "All Games"

            RefreshGameList();

            // Check for updates asynchronously
            _ = UpdateManager.CheckForUpdatesAsync(this);
        }

        private void btnProfile_Click(object? sender, EventArgs e)
        {
            using (var profileForm = new UserProfileForm())
            {
                profileForm.ShowDialog(this);
            }
        }

        private void RefreshGameList()
        {
            // Clear current controls to prevent memory leaks
            foreach (Control ctrl in flpGamesGrid.Controls)
            {
                ctrl.Dispose();
            }
            flpGamesGrid.Controls.Clear();

            string selectedConsole = lbConsoleFilter.SelectedItem?.ToString() ?? "All Games";
            string searchQuery = tbSearch.Text.Trim();

            // Run library pipeline
            var searchResults = _libraryManager.SearchGames(searchQuery);
            var filteredResults = _libraryManager.FilterGames(searchResults, selectedConsole, cbFilter.SelectedItem?.ToString() ?? "All Games");
            var sortedResults = _libraryManager.SortGames(filteredResults, cbSort.SelectedItem?.ToString() ?? "Title A-Z");

            // Render controls based on active layout view mode
            foreach (var game in sortedResults)
            {
                if (_isGridView)
                {
                    GameCard card = new GameCard(game);
                    card.CardSelected += (s, e) =>
                    {
                        SelectGame(game, card);
                        OpenGameDetails(game);
                    };
                    flpGamesGrid.Controls.Add(card);
                }
                else
                {
                    GameListRow row = new GameListRow(game);
                    row.Width = flpGamesGrid.ClientSize.Width - 35; // Responsive width
                    row.RowSelected += (s, e) =>
                    {
                        SelectGame(game, row);
                        OpenGameDetails(game);
                    };
                    flpGamesGrid.Controls.Add(row);
                }
            }

            // Restore selection or load default
            if (sortedResults.Count > 0)
            {
                Control? matchCard = null;
                if (_isGridView)
                {
                    matchCard = flpGamesGrid.Controls.OfType<GameCard>()
                        .FirstOrDefault(card => card.Game.Id == _selectedGame?.Id);
                }
                else
                {
                    matchCard = flpGamesGrid.Controls.OfType<GameListRow>()
                        .FirstOrDefault(row => row.Game.Id == _selectedGame?.Id);
                }
                
                if (matchCard != null)
                {
                    SelectGame(sortedResults.First(g => g.Id == _selectedGame?.Id), matchCard);
                }
                else
                {
                    var firstCtrl = flpGamesGrid.Controls.Count > 0 ? flpGamesGrid.Controls[0] : null;
                    if (firstCtrl != null)
                    {
                        SelectGame(sortedResults[0], firstCtrl);
                    }
                }
            }
            else
            {
                ClearDetails();
            }
        }

        private void SelectGame(Game game, Control cardOrRow)
        {
            if (_selectedCard != null)
            {
                if (_selectedCard is GameCard oldCard) oldCard.IsSelected = false;
                else if (_selectedCard is GameListRow oldRow) oldRow.IsSelected = false;
            }

            _selectedCard = cardOrRow;
            _selectedGame = game;

            if (_selectedCard is GameCard card) card.IsSelected = true;
            else if (_selectedCard is GameListRow row) row.IsSelected = true;

            // Load right drawer details
            lblDetailsTitle.Text = _selectedGame.Title;
            lblDetailsConsole.Text = _selectedGame.Platform;

            if (pbDetailsCover.Image != null)
            {
                pbDetailsCover.Image.Dispose();
                pbDetailsCover.Image = null;
            }

            pbDetailsCover.Image = MediaManager.GetImageOrPlaceholder(_selectedGame.CoverImagePath, "cover");

            // BIOS/Firmware warning banner for PlayStation games
            if (_selectedGame.Platform.StartsWith("Sony PlayStation", StringComparison.OrdinalIgnoreCase))
            {
                lblDetailsStatus.ForeColor = Color.FromArgb(248, 113, 113); // Coral warning red
                lblDetailsStatus.Text = "⚠️ PlayStation emulators require BIOS files (PS1/PS2) or system firmware (PS3) to run. These must be legally obtained by the user.";
            }
            else
            {
                lblDetailsStatus.ForeColor = Color.FromArgb(156, 163, 175);
                lblDetailsStatus.Text = "Ready to play.";
            }
        }

        private void OpenGameDetails(Game game)
        {
            using (var detailForm = new GameDetailForm(game, _libraryManager))
            {
                detailForm.PlayClicked += (s, e) =>
                {
                    LaunchGame(game);
                };
                detailForm.ShowDialog(this);
            }
            RefreshGameList();
        }

        private void ToggleView(bool isGridView)
        {
            _isGridView = isGridView;

            btnGridView.BackColor = _isGridView ? Color.FromArgb(38, 38, 48) : Color.FromArgb(24, 24, 28);
            btnGridView.ForeColor = _isGridView ? Color.White : Color.FromArgb(156, 163, 175);

            btnListView.BackColor = !_isGridView ? Color.FromArgb(38, 38, 48) : Color.FromArgb(24, 24, 28);
            btnListView.ForeColor = !_isGridView ? Color.White : Color.FromArgb(156, 163, 175);

            RefreshGameList();
        }

        private void ClearDetails()
        {
            _selectedGame = null;
            _selectedCard = null;

            lblDetailsTitle.Text = "No Game Selected";
            lblDetailsConsole.Text = "Choose a game to play";
            lblDetailsStatus.ForeColor = Color.FromArgb(156, 163, 175);
            lblDetailsStatus.Text = "Select a game to begin.";

            if (pbDetailsCover.Image != null)
            {
                pbDetailsCover.Image.Dispose();
                pbDetailsCover.Image = null;
            }
        }

        private void lbConsoleFilter_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            Color bgColor = isSelected ? Color.FromArgb(38, 38, 48) : Color.FromArgb(19, 19, 22);
            Color textColor = isSelected ? Color.FromArgb(99, 102, 241) : Color.FromArgb(156, 163, 175);

            using (var brush = new SolidBrush(bgColor))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            string text = lbConsoleFilter.Items[e.Index]?.ToString() ?? "";
            using (var brush = new SolidBrush(textColor))
            {
                var textRect = new Rectangle(e.Bounds.Left + 12, e.Bounds.Top, e.Bounds.Width - 12, e.Bounds.Height);
                using (var sf = new StringFormat { LineAlignment = StringAlignment.Center })
                {
                    e.Graphics.DrawString(text, e.Font ?? this.Font, brush, textRect, sf);
                }
            }

            if (isSelected)
            {
                using (var indicatorBrush = new SolidBrush(Color.FromArgb(99, 102, 241)))
                {
                    e.Graphics.FillRectangle(indicatorBrush, e.Bounds.Left, e.Bounds.Top + 4, 3, e.Bounds.Height - 8);
                }
            }
        }

        private void btnAddGame_Click(object? sender, EventArgs e)
        {
            using (var addForm = new AddGameForm())
            {
                if (addForm.ShowDialog(this) == DialogResult.OK)
                {
                    Game newGame = addForm.CreatedGame;
                    _libraryManager.AddGame(newGame);

                    var fs = new MockFriendsService();
                    fs.LogActivity($"Added a new game to library: {newGame.Title}");

                    lbConsoleFilter.SelectedIndex = 0;
                    tbSearch.Clear();

                    RefreshGameList();

                    // Find and select the new game
                    var newCard = flpGamesGrid.Controls.OfType<GameCard>()
                        .FirstOrDefault(card => card.Game.Id == newGame.Id);
                    
                    if (newCard != null)
                    {
                        SelectGame(newGame, newCard);
                    }
                }
            }
        }

        private void btnManageEmulators_Click(object? sender, EventArgs e)
        {
            using (var manager = new EmulatorManagerForm())
            {
                manager.ShowDialog(this);
                // Reload list to sync any default mapping changes immediately
                RefreshGameList();
            }
        }

        private void btnEditPaths_Click(object? sender, EventArgs e)
        {
            if (_selectedGame == null) return;

            using (var editForm = new AddGameForm(_selectedGame))
            {
                if (editForm.ShowDialog(this) == DialogResult.OK)
                {
                    _libraryManager.UpdateGame(_selectedGame);
                    RefreshGameList();

                    Control? matchCard = null;
                    if (_isGridView)
                    {
                        matchCard = flpGamesGrid.Controls.OfType<GameCard>()
                            .FirstOrDefault(card => card.Game.Id == _selectedGame.Id);
                    }
                    else
                    {
                        matchCard = flpGamesGrid.Controls.OfType<GameListRow>()
                            .FirstOrDefault(row => row.Game.Id == _selectedGame.Id);
                    }
                    
                    if (matchCard != null)
                    {
                        SelectGame(_selectedGame, matchCard);
                    }
                }
            }
        }

        private void btnDelete_Click(object? sender, EventArgs e)
        {
            if (_selectedGame == null) return;

            var confirmResult = MessageBox.Show(
                $"Are you sure you want to delete '{_selectedGame.Title}'?",
                "Confirm Deletion",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirmResult == DialogResult.Yes)
            {
                _libraryManager.RemoveGame(_selectedGame);
                ClearDetails();
                RefreshGameList();
            }
        }

        private void btnPlay_Click(object? sender, EventArgs e)
        {
            if (_selectedGame == null)
            {
                MessageBox.Show(
                    "No game selected.\n\nPlease select a game from the grid library before attempting to launch.",
                    "Launch Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            LaunchGame(_selectedGame);
        }

        private void LaunchGame(Game game)
        {
            string emulator = ResolvePath(game.EmulatorId);
            string rom = ResolvePath(game.RomPath);

            if (string.IsNullOrEmpty(game.EmulatorId) || !File.Exists(emulator))
            {
                lblDetailsStatus.Text = "Error: Emulator executable not found.";
                MessageBox.Show(
                    $"Emulator executable not found at:\n'{game.EmulatorId}'\n\nPlease select a valid emulator path.",
                    "Missing Emulator",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            if (string.IsNullOrEmpty(game.RomPath) || (!File.Exists(rom) && !Directory.Exists(rom)))
            {
                lblDetailsStatus.Text = "Error: ROM file/folder not found.";
                MessageBox.Show(
                    $"ROM file or game folder not found at:\n'{game.RomPath}'\n\nPlease select a valid ROM path.",
                    "Missing ROM File",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
                return;
            }

            try
            {
                lblDetailsStatus.ForeColor = Color.FromArgb(165, 180, 252);
                lblDetailsStatus.Text = $"Launching {game.Title}... (external process)";

                // Minimize launcher window and hide from desktop taskbar
                this.WindowState = FormWindowState.Minimized;
                this.ShowInTaskbar = false;

                DateTime startTime = DateTime.Now;
                var friendsService = new MockFriendsService();
                friendsService.UpdateMyStatus(ActivityStatus.Online, game.Title);
                friendsService.LogActivity($"Started playing {game.Title}");

                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = emulator,
                    Arguments = $"\"{rom}\"", // Pass quoted ROM/folder path
                    UseShellExecute = true
                };

                Process? process = Process.Start(psi);
                if (process != null)
                {
                    PlaytimeManager.Instance.StartSession(game.Id, process.Id);
                    process.EnableRaisingEvents = true;
                    process.Exited += (s, ev) =>
                    {
                        int sessionMins = PlaytimeManager.Instance.EndSession(game.Id);

                        // Update playtime in library database
                        game.TotalPlaytimeMinutes = PlaytimeManager.Instance.GetTotalPlaytime(game.Id);
                        game.LastPlayed = PlaytimeManager.Instance.GetOrCreateRecord(game.Id).LastPlayed;
                        _libraryManager.UpdateGame(game);

                        // Update playtime in social system
                        var fs = new MockFriendsService();
                        var profile = fs.GetLocalProfile();
                        profile.TotalPlayTimeMinutes = _libraryManager.Games.Sum(g => g.TotalPlaytimeMinutes);
                        fs.SaveLocalProfile(profile);
                        fs.UpdateMyStatus(ActivityStatus.Online, "");
                        fs.LogActivity($"Finished playing {game.Title} (Session: {sessionMins} mins)");

                        // Notify details window or cards
                        GameProcessExited?.Invoke(null, game.Id);

                        if (!this.IsDisposed && this.IsHandleCreated)
                        {
                            this.Invoke((MethodInvoker)delegate
                            {
                                this.WindowState = FormWindowState.Normal;
                                this.ShowInTaskbar = true;
                                this.Focus();
                                
                                // Restore PlayStation banner or status text
                                if (game.Platform.StartsWith("Sony PlayStation", StringComparison.OrdinalIgnoreCase))
                                {
                                    lblDetailsStatus.ForeColor = Color.FromArgb(248, 113, 113);
                                    lblDetailsStatus.Text = "⚠️ PlayStation emulators require BIOS files (PS1/PS2) or system firmware (PS3) to run. These must be legally obtained by the user.";
                                }
                                else
                                {
                                    lblDetailsStatus.ForeColor = Color.FromArgb(156, 163, 175);
                                    lblDetailsStatus.Text = "Ready to play.";
                                }
                                
                                RefreshGameList(); // Refresh list to update play stats
                            });
                        }
                    };
                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                    this.ShowInTaskbar = true;
                    lblDetailsStatus.Text = "Started, monitor inactive.";
                }
            }
            catch (Exception ex)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                lblDetailsStatus.Text = $"Launch failed: {ex.Message}";
                MessageBox.Show(
                    $"Failed to start external emulator process:\n{ex.Message}",
                    "Launch Execution Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private Image? LoadImageFromFile(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;

            try
            {
                using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                {
                    return Image.FromStream(stream);
                }
            }
            catch
            {
                return null;
            }
        }

        private Image CreatePlaceholderImage(string title)
        {
            Bitmap bmp = new Bitmap(180, 240);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(
                    new Rectangle(0, 0, 180, 240),
                    Color.FromArgb(40, 40, 50),
                    Color.FromArgb(20, 20, 25),
                    45f))
                {
                    g.FillRectangle(brush, 0, 0, 180, 240);
                }

                using (var pen = new Pen(Color.FromArgb(70, 75, 95), 2))
                {
                    g.DrawRectangle(pen, 1, 1, 178, 238);
                }

                using (var accentBrush = new SolidBrush(Color.FromArgb(99, 102, 241)))
                {
                    g.FillRectangle(accentBrush, 20, 40, 140, 8);
                    g.FillRectangle(accentBrush, 20, 55, 140, 3);
                }

                using (Font font = new Font("Segoe UI", 11, FontStyle.Bold))
                using (Brush brush = new SolidBrush(Color.FromArgb(220, 225, 235)))
                {
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString(title, font, brush, new Rectangle(10, 80, 160, 100), sf);
                }

                using (Font miniFont = new Font("Segoe UI", 8, FontStyle.Bold))
                using (Brush badgeBg = new SolidBrush(Color.FromArgb(99, 102, 241)))
                using (Brush textBrush = new SolidBrush(Color.White))
                {
                    g.FillRectangle(badgeBg, 50, 200, 80, 20);
                    StringFormat sf = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    g.DrawString("RETRO", miniFont, textBrush, new Rectangle(50, 200, 80, 20), sf);
                }
            }
            return bmp;
        }

        private string ResolvePath(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (Path.IsPathRooted(path)) return path;

            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string testPath1 = Path.Combine(baseDir, path);
            if (File.Exists(testPath1) || Directory.Exists(testPath1)) return testPath1;

            string testPath2 = Path.Combine(Directory.GetCurrentDirectory(), path);
            if (File.Exists(testPath2) || Directory.Exists(testPath2)) return testPath2;

            return testPath1;
        }

        private void MainSessionTimer_Tick(object? sender, EventArgs e)
        {
            if (_selectedGame == null)
            {
                btnPlay.Text = "▶  PLAY";
                btnPlay.Enabled = true;
                return;
            }

            if (PlaytimeManager.Instance.IsSessionActive(_selectedGame.Id))
            {
                var sessionStart = PlaytimeManager.Instance.GetSessionStart(_selectedGame.Id);
                if (sessionStart.HasValue)
                {
                    var elapsed = DateTime.Now - sessionStart.Value;
                    btnPlay.Text = $"⏳ RUNNING ({elapsed.Minutes:D2}:{elapsed.Seconds:D2})";
                    btnPlay.Enabled = false;

                    if (VideoManager.Instance.IsRecording && VideoManager.Instance.ActiveGameId == _selectedGame.Id)
                    {
                        int recSecs = VideoManager.Instance.GetRecordingDurationSeconds();
                        int recMins = recSecs / 60;
                        int recRemainingSecs = recSecs % 60;
                        lblDetailsStatus.ForeColor = Color.FromArgb(248, 113, 113); // Red
                        lblDetailsStatus.Text = $"🔴 RECORDING GAMEPLAY ({recMins:D2}:{recRemainingSecs:D2}) - F10 to Stop";
                    }
                    else
                    {
                        lblDetailsStatus.ForeColor = Color.FromArgb(165, 180, 252);
                        lblDetailsStatus.Text = $"Playing {_selectedGame.Title} • Session: {elapsed.Minutes}m {elapsed.Seconds}s";
                    }
                }
            }
            else
            {
                btnPlay.Text = "▶  PLAY";
                btnPlay.Enabled = true;
                
                // Restore warning banner if status text still shows "Playing"
                if (lblDetailsStatus.Text.StartsWith("Playing", StringComparison.OrdinalIgnoreCase))
                {
                    if (_selectedGame.Platform.StartsWith("Sony PlayStation", StringComparison.OrdinalIgnoreCase))
                    {
                        lblDetailsStatus.ForeColor = Color.FromArgb(248, 113, 113);
                        lblDetailsStatus.Text = "⚠️ PlayStation emulators require BIOS files (PS1/PS2) or system firmware (PS3) to run. These must be legally obtained by the user.";
                    }
                    else
                    {
                        lblDetailsStatus.ForeColor = Color.FromArgb(156, 163, 175);
                        lblDetailsStatus.Text = "Ready to play.";
                    }
                }
            }
        }

        private void btnManageSaves_Click(object? sender, EventArgs e)
        {
            using (var savesForm = new SaveManagerForm())
            {
                savesForm.ShowDialog(this);
            }
            RefreshGameList();
        }

        private void btnManageScreenshots_Click(object? sender, EventArgs e)
        {
            using (var scForm = new ScreenshotManagerForm())
            {
                scForm.ShowDialog(this);
            }
            RefreshGameList();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY)
            {
                int hotkeyId = m.WParam.ToInt32();
                if (hotkeyId == HOTKEY_ID)
                {
                    CaptureRunningGameScreenshot();
                }
                else if (hotkeyId == VIDEO_HOTKEY_ID)
                {
                    ToggleVideoRecording();
                }
            }
            base.WndProc(ref m);
        }

        private void CaptureRunningGameScreenshot()
        {
            var activeId = PlaytimeManager.Instance.ActiveGameId;
            if (string.IsNullOrEmpty(activeId)) return;

            var sc = ScreenshotManager.Instance.CaptureScreenshot(activeId);
            if (sc != null)
            {
                var game = _libraryManager.Games.FirstOrDefault(g => g.Id == activeId);
                string title = game != null ? game.Title : activeId;

                if (this.InvokeRequired)
                {
                    this.BeginInvoke(new Action(() => {
                        lblDetailsStatus.ForeColor = Color.FromArgb(52, 211, 153);
                        lblDetailsStatus.Text = $"📸  Screenshot captured for {title}!";
                    }));
                }
                else
                {
                    lblDetailsStatus.ForeColor = Color.FromArgb(52, 211, 153);
                    lblDetailsStatus.Text = $"📸  Screenshot captured for {title}!";
                }
            }
        }

        private void ToggleVideoRecording()
        {
            if (VideoManager.Instance.IsRecording)
            {
                var clip = VideoManager.Instance.StopRecording();
                if (clip != null)
                {
                    var game = _libraryManager.Games.FirstOrDefault(g => g.Id == clip.GameId);
                    string title = game != null ? game.Title : clip.GameId;

                    if (this.InvokeRequired)
                    {
                        this.BeginInvoke(new Action(() => {
                            lblDetailsStatus.ForeColor = Color.FromArgb(52, 211, 153);
                            lblDetailsStatus.Text = $"🎥  Clip saved: {clip.Title} ({clip.Duration})!";
                        }));
                    }
                    else
                    {
                        lblDetailsStatus.ForeColor = Color.FromArgb(52, 211, 153);
                        lblDetailsStatus.Text = $"🎥  Clip saved: {clip.Title} ({clip.Duration})!";
                    }
                }
            }
            else
            {
                var activeId = PlaytimeManager.Instance.ActiveGameId;
                if (string.IsNullOrEmpty(activeId)) return;

                if (VideoManager.Instance.StartRecording(activeId))
                {
                    var game = _libraryManager.Games.FirstOrDefault(g => g.Id == activeId);
                    string title = game != null ? game.Title : activeId;

                    if (this.InvokeRequired)
                    {
                        this.BeginInvoke(new Action(() => {
                            lblDetailsStatus.ForeColor = Color.FromArgb(248, 113, 113);
                            lblDetailsStatus.Text = $"🔴  Recording gameplay for {title}...";
                        }));
                    }
                    else
                    {
                        lblDetailsStatus.ForeColor = Color.FromArgb(248, 113, 113);
                        lblDetailsStatus.Text = $"🔴  Recording gameplay for {title}...";
                    }
                }
            }
        }

        private void btnManageVideos_Click(object? sender, EventArgs e)
        {
            using (var vForm = new VideoManagerForm())
            {
                vForm.ShowDialog(this);
            }
            RefreshGameList();
        }

        private void btnManageControllers_Click(object? sender, EventArgs e)
        {
            using (var cForm = new ControllerManagerForm())
            {
                cForm.ShowDialog(this);
            }
            RefreshGameList();
        }

        private void btnAppearance_Click(object? sender, EventArgs e)
        {
            using (var appForm = new AppearanceSettingsForm())
            {
                appForm.ShowDialog(this);
            }
            RefreshGameList();
        }

        private void btnLanguageSettings_Click(object? sender, EventArgs e)
        {
            using (var langForm = new LanguageSettingsForm())
            {
                langForm.ShowDialog(this);
            }
            RefreshGameList();
        }
    }
}
