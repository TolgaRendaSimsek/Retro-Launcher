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

        private bool _isFullscreen = false;
        private FormWindowState _prevWindowState;
        private FormBorderStyle _prevFormBorderStyle;
        private Rectangle _prevBounds;

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
            SetupResponsiveLayout();
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

            var setupWizardItem = new ToolStripMenuItem("First-Time Setup Wizard");
            setupWizardItem.Click += (s, e) => {
                using (var wizard = new SetupWizardForm())
                {
                    if (wizard.ShowDialog(this) == DialogResult.OK)
                    {
                        EmulatorManager.Instance.LoadEmulators();
                        RefreshGameList();
                    }
                }
            };
            toolsToolStripMenuItem.DropDownItems.Add(setupWizardItem);

            var packageManagerItem = new ToolStripMenuItem("Package Manager");
            packageManagerItem.Click += (s, e) => {
                using (var form = new PackageManagerForm())
                {
                    form.ShowDialog(this);
                }
            };
            toolsToolStripMenuItem.DropDownItems.Add(packageManagerItem);

            this.Shown += MainForm_Shown;

            GameLaunchService.Instance.GameStarted += (s, gameId) =>
            {
                if (this.IsDisposed || !this.IsHandleCreated) return;
                this.Invoke((MethodInvoker)delegate
                {
                    this.WindowState = FormWindowState.Minimized;
                    this.ShowInTaskbar = false;
                });
            };

            GameLaunchService.Instance.GameExited += (s, gameId) =>
            {
                if (this.IsDisposed || !this.IsHandleCreated) return;
                this.Invoke((MethodInvoker)delegate
                {
                    this.WindowState = FormWindowState.Normal;
                    this.ShowInTaskbar = true;
                    this.Focus();

                    GameProcessExited?.Invoke(null, gameId);

                    if (_selectedGame != null)
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
                    else
                    {
                        lblDetailsStatus.ForeColor = Color.FromArgb(156, 163, 175);
                        lblDetailsStatus.Text = "Ready to play.";
                    }

                    RefreshGameList();
                });
            };

            // Hover transitions
            btnPlay.BackColor = Color.FromArgb(99, 102, 241);
            btnPlay.MouseEnter += (s, e) => {
                if (_selectedGame != null && GameLaunchService.Instance.IsGameRunning(_selectedGame.Id))
                {
                    btnPlay.BackColor = Color.FromArgb(220, 38, 38); // Darker red on hover
                }
                else
                {
                    btnPlay.BackColor = Color.FromArgb(79, 70, 229); // Darker indigo on hover
                }
            };
            btnPlay.MouseLeave += (s, e) => {
                if (_selectedGame != null && GameLaunchService.Instance.IsGameRunning(_selectedGame.Id))
                {
                    btnPlay.BackColor = Color.FromArgb(239, 68, 68); // Red
                }
                else
                {
                    btnPlay.BackColor = Color.FromArgb(99, 102, 241); // Indigo
                }
            };
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

        private void SetupResponsiveLayout()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox = true;
            this.MinimumSize = new Size(850, 600);
            this.KeyPreview = true;

            // Load and restore previous window size and state
            var settings = SettingsManager.LoadSettings();
            if (settings.WindowWidth > 100 && settings.WindowHeight > 100)
            {
                this.Size = new Size(settings.WindowWidth, settings.WindowHeight);
                if (settings.WindowLeft >= 0 && settings.WindowTop >= 0)
                {
                    this.StartPosition = FormStartPosition.Manual;
                    this.Left = settings.WindowLeft;
                    this.Top = settings.WindowTop;
                }
                if (settings.IsMaximized)
                {
                    this.WindowState = FormWindowState.Maximized;
                }
            }

            // Hook KeyDown for fullscreen toggles (F11) and escape
            this.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.F11)
                {
                    ToggleFullscreen();
                    e.Handled = true;
                }
                else if (e.KeyCode == Keys.Escape)
                {
                    ExitFullscreen();
                    e.Handled = true;
                }
            };

            // FormClosing to save size and state
            this.FormClosing += (s, e) =>
            {
                var currentSettings = SettingsManager.LoadSettings();
                if (!_isFullscreen)
                {
                    currentSettings.IsMaximized = (this.WindowState == FormWindowState.Maximized);
                    if (this.WindowState == FormWindowState.Normal)
                    {
                        currentSettings.WindowWidth = this.Width;
                        currentSettings.WindowHeight = this.Height;
                        currentSettings.WindowLeft = this.Left;
                        currentSettings.WindowTop = this.Top;
                    }
                    else
                    {
                        currentSettings.WindowWidth = _prevBounds.Width > 0 ? _prevBounds.Width : this.RestoreBounds.Width;
                        currentSettings.WindowHeight = _prevBounds.Height > 0 ? _prevBounds.Height : this.RestoreBounds.Height;
                        currentSettings.WindowLeft = _prevBounds.Width > 0 ? _prevBounds.Left : this.RestoreBounds.Left;
                        currentSettings.WindowTop = _prevBounds.Height > 0 ? _prevBounds.Top : this.RestoreBounds.Top;
                    }
                }
                else
                {
                    currentSettings.IsMaximized = (_prevWindowState == FormWindowState.Maximized);
                    currentSettings.WindowWidth = _prevBounds.Width;
                    currentSettings.WindowHeight = _prevBounds.Height;
                    currentSettings.WindowLeft = _prevBounds.Left;
                    currentSettings.WindowTop = _prevBounds.Top;
                }
                SettingsManager.SaveSettings(currentSettings);
            };

            // Rebuild pnlTop right buttons flow
            pnlTop.Controls.Remove(btnProfile);
            pnlTop.Controls.Remove(btnAppearance);
            pnlTop.Controls.Remove(btnManageEmulators);
            pnlTop.Controls.Remove(btnAddGame);

            FlowLayoutPanel flpTopRight = new FlowLayoutPanel
            {
                Dock = DockStyle.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Width = 580,
                Height = 60,
                Padding = new Padding(0, 10, 10, 0),
                BackColor = Color.Transparent
            };
            
            flpTopRight.Controls.Add(btnProfile);
            flpTopRight.Controls.Add(btnAppearance);
            flpTopRight.Controls.Add(btnManageEmulators);
            flpTopRight.Controls.Add(btnAddGame);

            foreach (Control btn in flpTopRight.Controls)
            {
                btn.Margin = new Padding(5, 2, 5, 2);
                btn.Height = 35;
                btn.Anchor = AnchorStyles.None;
            }
            pnlTop.Controls.Add(flpTopRight);

            // Rebuild pnlSidebar using TableLayoutPanel
            pnlSidebar.Controls.Clear();
            TableLayoutPanel tlpSidebar = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 8,
                Padding = new Padding(10),
                BackColor = Color.Transparent
            };
            tlpSidebar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F)); // lblSidebarHeader
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // lbConsoleFilter
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // btnManageSaves
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // btnManageScreenshots
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // btnManageVideos
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // btnManageControllers
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F)); // btnLanguageSettings
            tlpSidebar.RowStyles.Add(new RowStyle(SizeType.Absolute, 10F)); // spacing

            lblSidebarHeader.Dock = DockStyle.Fill;
            lblSidebarHeader.Margin = new Padding(5, 5, 5, 0);

            lbConsoleFilter.Dock = DockStyle.Fill;
            lbConsoleFilter.Margin = new Padding(5, 5, 5, 10);

            btnManageSaves.Dock = DockStyle.Fill;
            btnManageSaves.Margin = new Padding(5, 2, 5, 2);

            btnManageScreenshots.Dock = DockStyle.Fill;
            btnManageScreenshots.Margin = new Padding(5, 2, 5, 2);

            btnManageVideos.Dock = DockStyle.Fill;
            btnManageVideos.Margin = new Padding(5, 2, 5, 2);

            btnManageControllers.Dock = DockStyle.Fill;
            btnManageControllers.Margin = new Padding(5, 2, 5, 2);

            btnLanguageSettings.Dock = DockStyle.Fill;
            btnLanguageSettings.Margin = new Padding(5, 2, 5, 2);

            tlpSidebar.Controls.Add(lblSidebarHeader, 0, 0);
            tlpSidebar.Controls.Add(lbConsoleFilter, 0, 1);
            tlpSidebar.Controls.Add(btnManageSaves, 0, 2);
            tlpSidebar.Controls.Add(btnManageScreenshots, 0, 3);
            tlpSidebar.Controls.Add(btnManageVideos, 0, 4);
            tlpSidebar.Controls.Add(btnManageControllers, 0, 5);
            tlpSidebar.Controls.Add(btnLanguageSettings, 0, 6);

            pnlSidebar.Controls.Add(tlpSidebar);

            // Rebuild pnlDetails using TableLayoutPanel
            pnlDetails.Controls.Clear();
            TableLayoutPanel tlpDetails = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 6,
                Padding = new Padding(15),
                BackColor = Color.Transparent
            };
            tlpDetails.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tlpDetails.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // pbDetailsCover
            tlpDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 45F)); // lblDetailsTitle
            tlpDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F)); // lblDetailsConsole
            tlpDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // btnPlay
            tlpDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); // sub buttons table
            tlpDetails.RowStyles.Add(new RowStyle(SizeType.Absolute, 55F)); // lblDetailsStatus

            pbDetailsCover.Dock = DockStyle.Fill;
            pbDetailsCover.Margin = new Padding(10);
            
            lblDetailsTitle.Dock = DockStyle.Fill;
            lblDetailsTitle.Margin = new Padding(5, 2, 5, 2);

            lblDetailsConsole.Dock = DockStyle.Fill;
            lblDetailsConsole.Margin = new Padding(5, 0, 5, 2);

            btnPlay.Dock = DockStyle.Fill;
            btnPlay.Margin = new Padding(5, 5, 5, 5);

            TableLayoutPanel tlpDetailsSubButtons = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0),
                BackColor = Color.Transparent
            };
            tlpDetailsSubButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpDetailsSubButtons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpDetailsSubButtons.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            btnEditPaths.Dock = DockStyle.Fill;
            btnEditPaths.Margin = new Padding(5, 2, 2, 2);

            btnDelete.Dock = DockStyle.Fill;
            btnDelete.Margin = new Padding(2, 2, 5, 2);

            tlpDetailsSubButtons.Controls.Add(btnEditPaths, 0, 0);
            tlpDetailsSubButtons.Controls.Add(btnDelete, 1, 0);

            lblDetailsStatus.Dock = DockStyle.Fill;
            lblDetailsStatus.Margin = new Padding(5);

            tlpDetails.Controls.Add(pbDetailsCover, 0, 0);
            tlpDetails.Controls.Add(lblDetailsTitle, 0, 1);
            tlpDetails.Controls.Add(lblDetailsConsole, 0, 2);
            tlpDetails.Controls.Add(btnPlay, 0, 3);
            tlpDetails.Controls.Add(tlpDetailsSubButtons, 0, 4);
            tlpDetails.Controls.Add(lblDetailsStatus, 0, 5);

            pnlDetails.Controls.Add(tlpDetails);

            // Rebuild pnlLibraryToolbar using FlowLayoutPanel
            pnlLibraryToolbar.Controls.Clear();
            FlowLayoutPanel flpToolbar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Padding = new Padding(10, 8, 10, 8),
                BackColor = Color.Transparent
            };

            btnGridView.Margin = new Padding(5, 0, 5, 0);
            btnGridView.Height = 30;
            btnGridView.Width = 80;

            btnListView.Margin = new Padding(5, 0, 15, 0);
            btnListView.Height = 30;
            btnListView.Width = 80;

            lblSortBy.Margin = new Padding(5, 8, 5, 0);
            lblSortBy.AutoSize = true;

            cbSort.Margin = new Padding(5, 4, 15, 0);
            cbSort.Height = 21;
            cbSort.Width = 120;

            lblFilterBy.Margin = new Padding(5, 8, 5, 0);
            lblFilterBy.AutoSize = true;

            cbFilter.Margin = new Padding(5, 4, 5, 0);
            cbFilter.Height = 21;
            cbFilter.Width = 120;

            flpToolbar.Controls.AddRange(new Control[]
            {
                btnGridView, btnListView, lblSortBy, cbSort, lblFilterBy, cbFilter
            });

            pnlLibraryToolbar.Controls.Add(flpToolbar);

            // Subscribe to grid size changes
            flpGamesGrid.SizeChanged += (s, e) => flpGamesGrid_SizeChanged();
        }

        private void ToggleFullscreen()
        {
            if (!_isFullscreen)
            {
                _prevWindowState = this.WindowState;
                _prevFormBorderStyle = this.FormBorderStyle;
                _prevBounds = this.Bounds;

                this.WindowState = FormWindowState.Normal;
                this.FormBorderStyle = FormBorderStyle.None;
                this.Bounds = Screen.FromControl(this).Bounds;
                _isFullscreen = true;
            }
            else
            {
                this.FormBorderStyle = _prevFormBorderStyle;
                this.Bounds = _prevBounds;
                this.WindowState = _prevWindowState;
                _isFullscreen = false;
            }
        }

        private void ExitFullscreen()
        {
            if (_isFullscreen)
            {
                ToggleFullscreen();
            }
        }

        private void flpGamesGrid_SizeChanged()
        {
            if (!_isGridView)
            {
                flpGamesGrid.SuspendLayout();
                foreach (Control ctrl in flpGamesGrid.Controls)
                {
                    if (ctrl is GameListRow row)
                    {
                        row.Width = flpGamesGrid.ClientSize.Width - 35;
                    }
                }
                flpGamesGrid.ResumeLayout();
                return;
            }

            int clientWidth = flpGamesGrid.ClientSize.Width - flpGamesGrid.Padding.Horizontal;
            int cardMinWith = 140;
            int cardMargin = 10;
            int colCount = Math.Max(1, clientWidth / (cardMinWith + cardMargin));
            int targetWidth = (clientWidth / colCount) - cardMargin;

            flpGamesGrid.SuspendLayout();
            foreach (Control ctrl in flpGamesGrid.Controls)
            {
                if (ctrl is GameCard card)
                {
                    card.Width = targetWidth;
                    card.Height = (int)(targetWidth * 1.53);
                }
            }
            flpGamesGrid.ResumeLayout();
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

        private void MainForm_Shown(object? sender, EventArgs e)
        {
            var settings = SettingsManager.LoadSettings();
            if (settings.IsFirstRun)
            {
                using (var wizard = new SetupWizardForm())
                {
                    if (wizard.ShowDialog(this) == DialogResult.OK)
                    {
                        EmulatorManager.Instance.LoadEmulators();
                        RefreshGameList();
                    }
                }
            }
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

            flpGamesGrid_SizeChanged();

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

            if (GameLaunchService.Instance.IsGameRunning(_selectedGame.Id))
            {
                btnPlay.Text = "🛑 STOP GAME";
                btnPlay.BackColor = Color.FromArgb(239, 68, 68);
                
                var sessionStart = PlaytimeManager.Instance.GetSessionStart(_selectedGame.Id);
                if (sessionStart.HasValue)
                {
                    var elapsed = DateTime.Now - sessionStart.Value;
                    lblDetailsStatus.ForeColor = Color.FromArgb(165, 180, 252);
                    lblDetailsStatus.Text = $"Playing {_selectedGame.Title} • Session: {elapsed.Minutes}m {elapsed.Seconds}s";
                }
            }
            else
            {
                btnPlay.Text = "▶  PLAY";
                btnPlay.BackColor = Color.FromArgb(99, 102, 241);

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

            if (GameLaunchService.Instance.IsGameRunning(_selectedGame.Id))
            {
                GameLaunchService.Instance.StopGame(_selectedGame.Id);
            }
            else
            {
                LaunchGame(_selectedGame);
            }
        }

        private async void LaunchGame(Game game)
        {
            try
            {
                lblDetailsStatus.ForeColor = Color.FromArgb(165, 180, 252);
                lblDetailsStatus.Text = $"Launching {game.Title}... (external process)";
                
                await GameLaunchService.Instance.LaunchGameAsync(game);
            }
            catch (Exception ex)
            {
                this.WindowState = FormWindowState.Normal;
                this.ShowInTaskbar = true;
                lblDetailsStatus.ForeColor = Color.FromArgb(239, 68, 68);
                lblDetailsStatus.Text = $"Launch failed: {ex.Message}";
                MessageBox.Show(
                    $"Failed to launch game:\n{ex.Message}",
                    "Launch Error",
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
                btnPlay.BackColor = Color.FromArgb(99, 102, 241); // Reset to Indigo
                return;
            }

            if (GameLaunchService.Instance.IsGameRunning(_selectedGame.Id))
            {
                var sessionStart = PlaytimeManager.Instance.GetSessionStart(_selectedGame.Id);
                if (sessionStart.HasValue)
                {
                    var elapsed = DateTime.Now - sessionStart.Value;
                    btnPlay.Text = $"🛑 STOP GAME ({elapsed.Minutes:D2}:{elapsed.Seconds:D2})";
                    btnPlay.Enabled = true;
                    btnPlay.BackColor = Color.FromArgb(239, 68, 68); // Red

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
                btnPlay.BackColor = Color.FromArgb(99, 102, 241); // Reset to Indigo
                
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
