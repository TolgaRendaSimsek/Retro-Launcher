using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;

namespace RetroLauncher.UI.Forms
{
    public partial class UserProfileForm : Form
    {
        private readonly IFriendsService _friendsService;
        private UserProfile _profile = new();
        private readonly GameLibraryManager _libraryManager = new();
        private UpdaterSettings _updaterSettings = new();
        private readonly string _settingsPath;

        // Steam-style user profile layout controls
        private PictureBox pbProfileBanner = null!;
        private PictureBox pbProfileAvatar = null!;
        private Label lblProfileName = null!;
        private Label lblStatus = null!;
        private Label lblFriendCodeDisplay = null!;
        private Label lblBioDisplay = null!;
        private Label lblFavConsoleHeader = null!;
        private Label lblFavConsoleDisplay = null!;
        private Label lblFavGamesHeader = null!;
        private FlowLayoutPanel flpFavGames = null!;
        private Label lblLastPlayedHeader = null!;
        private FlowLayoutPanel flpLastPlayed = null!;
        private Button btnEditProfile = null!;

        public UserProfileForm()
        {
            InitializeComponent();
            
            _friendsService = new MockFriendsService();
            _settingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updater_settings.json");

            InitializeSteamProfileLayout();
            SetupCustomEvents();
        }

        private void SetupCustomEvents()
        {
            this.Load += UserProfileForm_Load;

            // Tab switching navigation buttons
            btnTabProfile.Click += (s, e) => SwitchTab(pnlMyProfile, btnTabProfile);
            btnTabFriends.Click += (s, e) => SwitchTab(pnlFriendsList, btnTabFriends);
            btnTabPending.Click += (s, e) => SwitchTab(pnlPending, btnTabPending);
            btnTabAddFriend.Click += (s, e) => SwitchTab(pnlAddFriend, btnTabAddFriend);
            btnTabBlocked.Click += (s, e) => SwitchTab(pnlBlocked, btnTabBlocked);
            btnTabSaves.Click += (s, e) => SwitchTab(pnlSavesSync, btnTabSaves);

            btnSyncAll.Click += btnSyncAll_Click;
            cbProvider.SelectedIndexChanged += cbProvider_SelectedIndexChanged;

            // Active Profile Save
            btnSaveProfile.Click += btnSaveProfile_Click;

            // Friend actions
            btnSendRequest.Click += btnSendRequest_Click;
            btnRemoveFriend.Click += btnRemoveFriend_Click;
            btnBlockFriend.Click += btnBlockFriend_Click;
            btnUnblockUser.Click += btnUnblockUser_Click;

            // Owner Draw ListBox for Friends List
            lbFriends.DrawItem += lbFriends_DrawItem;

            // Color selection changes color theme
            cbThemeColor.SelectedIndexChanged += cbThemeColor_SelectedIndexChanged;
        }

        private void UserProfileForm_Load(object? sender, EventArgs e)
        {
            ThemeManager.Instance.ThemeChanged += (s, ev) => ThemeManager.Instance.ApplyTheme(this);
            ThemeManager.Instance.ApplyTheme(this);
            LocalizationManager.Instance.LanguageChanged += (s, ev) => LocalizationManager.Instance.ApplyLanguage(this);
            LocalizationManager.Instance.ApplyLanguage(this);
            LoadData();
            SwitchTab(pnlMyProfile, btnTabProfile);
        }

        private void LoadData()
        {
            _profile = _friendsService.GetLocalProfile();
            LoadUpdaterSettings();

            // Profile info panel binding
            tbUsername.Text = _profile.Username;
            lblFriendCodeValue.Text = _profile.FriendCode;
            tbBio.Text = _profile.Bio;

            // Populate consoles list and match selection
            cbFavoriteConsole.Items.Clear();
            cbFavoriteConsole.Items.Add("All Consoles");
            cbFavoriteConsole.Items.AddRange(new[]
            {
                "Sony PlayStation 1",
                "Sony PlayStation 2",
                "Sony PlayStation 3",
                "Nintendo Entertainment System (NES)",
                "Super Nintendo (SNES)",
                "Nintendo 64",
                "Sega Genesis",
                "Game Boy Advance"
            });
            int consoleIdx = cbFavoriteConsole.FindStringExact(_profile.FavoriteConsole);
            if (cbFavoriteConsole.Items.Count > 0)
            {
                cbFavoriteConsole.SelectedIndex = consoleIdx >= 0 ? consoleIdx : 0;
            }

            EnsureThemeColorItems();
            int colorIdx = cbThemeColor.Items.Cast<string>().ToList()
                .FindIndex(item => item.Contains(_profile.ThemeColor));
            if (cbThemeColor.Items.Count > 0)
            {
                cbThemeColor.SelectedIndex = colorIdx >= 0 ? colorIdx : 0;
            }

            EnsureUpdateChannelItems();
            int channelIdx = cbUpdateChannel.Items.Cast<string>().ToList()
                .FindIndex(item => item.Equals(_updaterSettings.UpdateChannel, StringComparison.OrdinalIgnoreCase));
            if (cbUpdateChannel.Items.Count > 0)
            {
                cbUpdateChannel.SelectedIndex = channelIdx >= 0 ? channelIdx : 0;
            }

            // Statistics display
            lblStatTotalGames.Text = $"Games in Library: {_libraryManager.Games.Count}";
            lblStatTotalPlaytime.Text = $"Total Playtime: {_profile.TotalPlayTimeMinutes} mins";

            // Activity Log display
            lbActivities.Items.Clear();
            foreach (var act in _profile.Activities)
            {
                lbActivities.Items.Add($"[{act.Timestamp}] {act.EventText}");
            }

            // Friends List display
            RefreshFriendsList();

            // Pending list display
            RefreshPendingRequests();

            // Blocked Users display
            RefreshBlockedList();

            ApplyThemeColor(_profile.ThemeColor);
            LoadShowcase();
            LoadSavesSyncData();
            PopulateSteamProfileData();
        }

        private void LoadUpdaterSettings()
        {
            try
            {
                if (File.Exists(_settingsPath))
                {
                    string json = File.ReadAllText(_settingsPath);
                    _updaterSettings = JsonSerializer.Deserialize<UpdaterSettings>(json) ?? new UpdaterSettings();
                }
            }
            catch
            {
                _updaterSettings = new UpdaterSettings();
            }
        }

        private void SaveUpdaterSettings()
        {
            try
            {
                string json = JsonSerializer.Serialize(_updaterSettings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsPath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error saving updater settings: {ex.Message}");
            }
        }

        private void EnsureThemeColorItems()
        {
            if (cbThemeColor.Items.Count > 0) return;

            cbThemeColor.Items.Clear();
            cbThemeColor.Items.AddRange(new[]
            {
                "Indigo (#6366F1)",
                "Emerald (#10B981)",
                "Rose (#FB7185)",
                "Amber (#F59E0B)",
                "Cyan (#06B6D4)",
                "Violet (#8B5CF6)"
            });
        }

        private void EnsureUpdateChannelItems()
        {
            if (cbUpdateChannel.Items.Count > 0) return;

            cbUpdateChannel.Items.Clear();
            cbUpdateChannel.Items.AddRange(new[]
            {
                "stable",
                "beta",
                "alpha"
            });
        }

        private void SwitchTab(Panel activePanel, Button activeTabBtn)
        {
            // Hide all tab panels
            pnlMyProfile.Visible = false;
            pnlFriendsList.Visible = false;
            pnlPending.Visible = false;
            pnlAddFriend.Visible = false;
            pnlBlocked.Visible = false;
            pnlSavesSync.Visible = false;

            // Reset navigation button highlights
            foreach (Control ctrl in pnlLeftSidebar.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.BackColor = Color.FromArgb(19, 19, 22);
                    btn.ForeColor = Color.FromArgb(156, 163, 175);
                }
            }

            // Show and highlight the active one
            activePanel.Visible = true;
            activeTabBtn.BackColor = Color.FromArgb(38, 38, 48);
            activeTabBtn.ForeColor = Color.White;
        }

        private void btnSaveProfile_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbUsername.Text))
            {
                MessageBox.Show("Username cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _profile.Username = tbUsername.Text.Trim();
            _profile.Bio = tbBio.Text.Trim();
            _profile.FavoriteConsole = cbFavoriteConsole.SelectedItem?.ToString() ?? "All Consoles";

            // Parse hex color from selected theme combo box item e.g. "Indigo (#6366F1)" -> "#6366F1"
            string selectedColorText = cbThemeColor.SelectedItem?.ToString() ?? "Indigo (#6366F1)";
            int startIdx = selectedColorText.IndexOf('(');
            int endIdx = selectedColorText.IndexOf(')');
            if (startIdx >= 0 && endIdx > startIdx)
            {
                _profile.ThemeColor = selectedColorText.Substring(startIdx + 1, endIdx - startIdx - 1);
            }

            // Save profile details
            _friendsService.SaveLocalProfile(_profile);

            // Save update channel choice
            _updaterSettings.UpdateChannel = cbUpdateChannel.SelectedItem?.ToString() ?? "stable";
            SaveUpdaterSettings();

            ApplyThemeColor(_profile.ThemeColor);

            MessageBox.Show("Profile and updater settings saved successfully!", "Settings Saved", MessageBoxButtons.OK, MessageBoxIcon.Information);
            
            LoadData(); // reload
        }

        private void ApplyThemeColor(string hexColor)
        {
            try
            {
                Color themeColor = ColorTranslator.FromHtml(hexColor);
                btnSaveProfile.BackColor = themeColor;
                btnSendRequest.BackColor = themeColor;
                btnUnblockUser.BackColor = themeColor;
                lblFriendCodeValue.ForeColor = themeColor;
            }
            catch
            {
                Color defaultColor = Color.FromArgb(99, 102, 241);
                btnSaveProfile.BackColor = defaultColor;
                btnSendRequest.BackColor = defaultColor;
                btnUnblockUser.BackColor = defaultColor;
                lblFriendCodeValue.ForeColor = defaultColor;
            }
        }

        private void cbThemeColor_SelectedIndexChanged(object? sender, EventArgs e)
        {
            string selectedColorText = cbThemeColor.SelectedItem?.ToString() ?? "";
            int startIdx = selectedColorText.IndexOf('(');
            int endIdx = selectedColorText.IndexOf(')');
            if (startIdx >= 0 && endIdx > startIdx)
            {
                string hexColor = selectedColorText.Substring(startIdx + 1, endIdx - startIdx - 1);
                ApplyThemeColor(hexColor);
            }
        }

        // Friends list helpers
        private void RefreshFriendsList()
        {
            lbFriends.Items.Clear();
            var friends = _friendsService.GetFriends();

            // Sort Online friends first, then Offline
            var sortedFriends = friends
                .OrderByDescending(f => f.Status != ActivityStatus.Offline)
                .ThenBy(f => f.Username)
                .ToList();

            foreach (var friend in sortedFriends)
            {
                lbFriends.Items.Add(friend);
            }
        }

        private void lbFriends_DrawItem(object? sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;

            Friend friend = (Friend)lbFriends.Items[e.Index];
            bool isSelected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;

            // Background
            Color bg = isSelected ? Color.FromArgb(38, 38, 48) : Color.FromArgb(19, 19, 22);
            using (var brush = new SolidBrush(bg))
            {
                e.Graphics.FillRectangle(brush, e.Bounds);
            }

            // Draw status dot
            Color statusColor;
            switch (friend.Status)
            {
                case ActivityStatus.Online:
                    statusColor = Color.FromArgb(16, 185, 129); // Emerald Green
                    break;
                case ActivityStatus.Away:
                    statusColor = Color.FromArgb(245, 158, 11); // Amber/Yellow
                    break;
                case ActivityStatus.Busy:
                    statusColor = Color.FromArgb(239, 68, 68); // Red
                    break;
                default:
                    statusColor = Color.FromArgb(107, 114, 128); // Slate/Gray
                    break;
            }

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var dotBrush = new SolidBrush(statusColor))
            {
                e.Graphics.FillEllipse(dotBrush, e.Bounds.Left + 15, e.Bounds.Top + (e.Bounds.Height / 2) - 6, 12, 12);
            }

            // Draw Username
            string fontName = this.Font.Name;
            using (var fontNameUser = new Font(fontName, 9.5F, FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.White))
            {
                e.Graphics.DrawString(friend.Username, fontNameUser, textBrush, e.Bounds.Left + 40, e.Bounds.Top + 6);
            }

            // Draw Subtext (Status & Playing Details)
            string subtext = friend.Status == ActivityStatus.Offline ? "Offline" : friend.Status.ToString();
            if (!string.IsNullOrEmpty(friend.CurrentlyPlaying))
            {
                subtext += $" - Playing: {friend.CurrentlyPlaying}";
            }

            using (var fontSub = new Font(fontName, 8F))
            using (var subBrush = new SolidBrush(Color.FromArgb(156, 163, 175)))
            {
                e.Graphics.DrawString(subtext, fontSub, subBrush, e.Bounds.Left + 40, e.Bounds.Top + 24);
            }

            // Draw bottom separator line
            using (var pen = new Pen(Color.FromArgb(28, 28, 34), 1))
            {
                e.Graphics.DrawLine(pen, e.Bounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);
            }
        }

        private void btnRemoveFriend_Click(object? sender, EventArgs e)
        {
            if (lbFriends.SelectedItem is Friend friend)
            {
                var result = MessageBox.Show($"Are you sure you want to remove '{friend.Username}' from your friends list?", 
                    "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _friendsService.RemoveFriend(friend.FriendCode);
                    RefreshFriendsList();
                }
            }
            else
            {
                MessageBox.Show("Please select a friend from the list.", "No Friend Selected", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnBlockFriend_Click(object? sender, EventArgs e)
        {
            if (lbFriends.SelectedItem is Friend friend)
            {
                var result = MessageBox.Show($"Are you sure you want to block '{friend.Username}'?", 
                    "Confirm Block", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    _friendsService.BlockUser(friend.FriendCode);
                    RefreshFriendsList();
                    RefreshBlockedList();
                }
            }
            else
            {
                MessageBox.Show("Please select a friend to block.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Pending Invitations List
        private void RefreshPendingRequests()
        {
            flpPendingRequests.Controls.Clear();
            var requests = _friendsService.GetPendingRequests();

            foreach (var req in requests)
            {
                Panel pnl = new Panel
                {
                    Size = new Size(450, 60),
                    BackColor = Color.FromArgb(28, 28, 34),
                    Margin = new Padding(0, 0, 0, 10),
                    Padding = new Padding(10)
                };

                Label lblInfo = new Label
                {
                    Text = req.Incoming 
                        ? $"{req.SenderName} sent you a friend request.\nCode: {req.SenderCode}  |  {req.Timestamp}"
                        : $"Sent request to {req.SenderName}.\nCode: {req.SenderCode}  |  {req.Timestamp}",
                    ForeColor = Color.White,
                    Font = new Font(this.Font.Name, 8.5F),
                    Location = new Point(10, 12),
                    Size = new Size(250, 38)
                };
                pnl.Controls.Add(lblInfo);

                if (req.Incoming)
                {
                    Button btnAccept = new Button
                    {
                        Text = "Accept",
                        BackColor = Color.FromArgb(16, 185, 129),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font(this.Font.Name, 8F, FontStyle.Bold),
                        Size = new Size(80, 30),
                        Location = new Point(270, 15),
                        Cursor = Cursors.Hand
                    };
                    btnAccept.FlatAppearance.BorderSize = 0;
                    btnAccept.Click += (s, e) =>
                    {
                        _friendsService.AcceptFriendRequest(req.Id);
                        RefreshPendingRequests();
                        RefreshFriendsList();
                    };
                    pnl.Controls.Add(btnAccept);

                    Button btnDecline = new Button
                    {
                        Text = "Decline",
                        BackColor = Color.FromArgb(239, 68, 68),
                        ForeColor = Color.White,
                        FlatStyle = FlatStyle.Flat,
                        Font = new Font(this.Font.Name, 8F, FontStyle.Bold),
                        Size = new Size(80, 30),
                        Location = new Point(360, 15),
                        Cursor = Cursors.Hand
                    };
                    btnDecline.FlatAppearance.BorderSize = 0;
                    btnDecline.Click += (s, e) =>
                    {
                        _friendsService.DeclineFriendRequest(req.Id);
                        RefreshPendingRequests();
                    };
                    pnl.Controls.Add(btnDecline);
                }
                else
                {
                    Label lblOutgoing = new Label
                    {
                        Text = "Outgoing",
                        ForeColor = Color.FromArgb(156, 163, 175),
                        Font = new Font(this.Font.Name, 8.5F, FontStyle.Italic),
                        Location = new Point(370, 20),
                        Size = new Size(70, 20)
                    };
                    pnl.Controls.Add(lblOutgoing);
                }

                flpPendingRequests.Controls.Add(pnl);
            }
        }

        // Add Friend Panel logic
        private void btnSendRequest_Click(object? sender, EventArgs e)
        {
            string value = tbAddFriendCode.Text.Trim();
            if (string.IsNullOrEmpty(value))
            {
                lblAddFriendStatus.ForeColor = Color.FromArgb(239, 68, 68);
                lblAddFriendStatus.Text = "Please enter a username or friend code.";
                return;
            }

            bool success = _friendsService.SendFriendRequest(value);
            if (success)
            {
                lblAddFriendStatus.ForeColor = Color.FromArgb(16, 185, 129);
                lblAddFriendStatus.Text = $"Friend request sent successfully to '{value}'!";
                tbAddFriendCode.Clear();
                RefreshPendingRequests();
            }
            else
            {
                lblAddFriendStatus.ForeColor = Color.FromArgb(239, 68, 68);
                lblAddFriendStatus.Text = "Request failed. Check if user is already added, blocked, or if code is invalid.";
            }
        }

        // Blocked list logic
        private void RefreshBlockedList()
        {
            lbBlockedUsers.Items.Clear();
            var blocked = _friendsService.GetBlockedUsers();
            foreach (var b in blocked)
            {
                lbBlockedUsers.Items.Add(b);
            }
            lbBlockedUsers.DisplayMember = "FriendCode";
        }

        private void LoadShowcase()
        {
            // Clear existing controls and dispose images to prevent memory/file leaks
            foreach (Control ctrl in flpShowcase.Controls)
            {
                if (ctrl is Panel pnl)
                {
                    foreach (Control sub in pnl.Controls)
                    {
                        if (sub is PictureBox pb && pb.Image != null)
                        {
                            var img = pb.Image;
                            pb.Image = null;
                            img.Dispose();
                        }
                    }
                }
            }
            flpShowcase.Controls.Clear();

            var achievementManager = new AchievementManager();
            var allUnlocked = achievementManager.Achievements.Where(a => a.IsUnlocked).ToList();

            for (int i = 0; i < 6; i++)
            {
                int index = i;
                string? achievementId = _profile.ShowcaseAchievementIds.ElementAtOrDefault(index);

                Panel slotPanel = new Panel
                {
                    Size = new Size(50, 50),
                    BackColor = Color.FromArgb(30, 30, 36),
                    Margin = new Padding(0, 0, 10, 0),
                    Cursor = Cursors.Hand
                };

                // Dotted border or normal border drawing
                slotPanel.Paint += (s, e) =>
                {
                    using (Pen pen = new Pen(Color.FromArgb(55, 65, 81), 1))
                    {
                        if (string.IsNullOrEmpty(achievementId))
                        {
                            pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                        }
                        else
                        {
                            // If rare achievement, paint a gold border around the slot
                            var ach = allUnlocked.FirstOrDefault(a => a.Id == achievementId);
                            if (ach != null && ach.Rarity.ToLower().Contains("rare"))
                            {
                                pen.Color = Color.FromArgb(245, 158, 11);
                                pen.Width = 2;
                            }
                        }
                        e.Graphics.DrawRectangle(pen, 0, 0, slotPanel.Width - 1, slotPanel.Height - 1);
                    }
                };

                PictureBox pb = new PictureBox
                {
                    Size = new Size(42, 42),
                    Location = new Point(4, 4),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    BackColor = Color.Transparent
                };

                if (!string.IsNullOrEmpty(achievementId))
                {
                    var ach = allUnlocked.FirstOrDefault(a => a.Id == achievementId);
                    if (ach != null)
                    {
                        pb.Image = MediaManager.GetImageOrPlaceholder(ach.IconPath, "icon");
                        
                        // Tooltip to show name/description of showcased achievement
                        ToolTip tt = new ToolTip();
                        tt.SetToolTip(pb, $"{ach.Title}\n{ach.Description}\n({ach.Rarity})");
                        tt.SetToolTip(slotPanel, $"{ach.Title}\n{ach.Description}\n({ach.Rarity})");

                        // Right-click menu to clear/remove the achievement from this slot
                        ContextMenuStrip cms = new ContextMenuStrip();
                        ToolStripMenuItem itemClear = new ToolStripMenuItem("Remove from Showcase");
                        itemClear.Click += (s, ev) =>
                        {
                            if (index < _profile.ShowcaseAchievementIds.Count)
                            {
                                _profile.ShowcaseAchievementIds[index] = "";
                                _friendsService.SaveLocalProfile(_profile);
                                LoadShowcase();
                            }
                        };
                        cms.Items.Add(itemClear);
                        pb.ContextMenuStrip = cms;
                        slotPanel.ContextMenuStrip = cms;
                    }
                }
                else
                {
                    // Empty slot, draw a plus sign
                    Label lblPlus = new Label
                    {
                        Text = "+",
                        ForeColor = Color.FromArgb(107, 114, 128),
                        Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                        TextAlign = ContentAlignment.MiddleCenter,
                        Dock = DockStyle.Fill,
                        Cursor = Cursors.Hand
                    };
                    lblPlus.Click += (s, ev) => OpenSelectorForSlot(index, allUnlocked);
                    slotPanel.Controls.Add(lblPlus);
                }

                // Click handler to select or swap achievement
                if (!string.IsNullOrEmpty(achievementId))
                {
                    pb.Click += (s, ev) => OpenSelectorForSlot(index, allUnlocked);
                    slotPanel.Click += (s, ev) => OpenSelectorForSlot(index, allUnlocked);
                    slotPanel.Controls.Add(pb);
                }
                else
                {
                    slotPanel.Click += (s, ev) => OpenSelectorForSlot(index, allUnlocked);
                }

                flpShowcase.Controls.Add(slotPanel);
            }
        }

        private void OpenSelectorForSlot(int index, List<Achievement> allUnlocked)
        {
            // Pass all showcased IDs except the one in the current slot
            List<string> otherShowcasedIds = _profile.ShowcaseAchievementIds
                .Where((id, idx) => idx != index && !string.IsNullOrEmpty(id))
                .ToList();

            using (var selector = new AchievementSelectorForm(allUnlocked, otherShowcasedIds))
            {
                if (selector.ShowDialog(this) == DialogResult.OK && selector.SelectedAchievement != null)
                {
                    while (_profile.ShowcaseAchievementIds.Count <= index)
                    {
                        _profile.ShowcaseAchievementIds.Add("");
                    }
                    _profile.ShowcaseAchievementIds[index] = selector.SelectedAchievement.Id;
                    
                    _friendsService.SaveLocalProfile(_profile);
                    LoadShowcase();
                }
            }
        }

        private void btnUnblockUser_Click(object? sender, EventArgs e)
        {
            if (lbBlockedUsers.SelectedItem is Friend blocked)
            {
                bool success = _friendsService.UnblockUser(blocked.FriendCode);
                if (success)
                {
                    RefreshBlockedList();
                    RefreshFriendsList();
                }
            }
            else
            {
                MessageBox.Show("Please select a blocked user from the list.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadSavesSyncData()
        {
            // Populate Providers
            cbProvider.Items.Clear();
            cbProvider.Items.AddRange(new object[] {
                "Local Backup",
                "Google Drive (Simulated)",
                "OneDrive (Simulated)",
                "Dropbox (Simulated)"
            });

            // Match active provider
            string activeProv = SaveManager.Instance.ActiveProvider;
            int idx = cbProvider.FindStringExact(activeProv);
            if (cbProvider.Items.Count > 0)
            {
                cbProvider.SelectedIndex = idx >= 0 ? idx : 0;
            }

            // Load last sync / status info
            UpdateSavesStatusUI();

            // Populate emulator saves list
            PopulateEmulatorSavesList();
        }

        private void UpdateSavesStatusUI()
        {
            string provider = SaveManager.Instance.ActiveProvider;
            var lib = new GameLibraryManager();
            var allMetas = lib.Games.Select(g => SaveManager.Instance.GetOrCreateMetadata(g.Id, g.EmulatorId)).ToList();
            
            DateTime? lastDate = null;
            bool anySynced = false;
            bool hasConflict = false;

            foreach (var meta in allMetas)
            {
                if (!string.IsNullOrEmpty(meta.LastBackupDate))
                {
                    if (DateTime.TryParse(meta.LastBackupDate, out DateTime dt))
                    {
                        if (!lastDate.HasValue || dt > lastDate.Value)
                        {
                            lastDate = dt;
                        }
                    }
                    anySynced = true;
                }

                var comp = SaveManager.Instance.CompareLocalAndBackupSaves(meta.GameId);
                if (comp == SaveComparisonResult.LocalNewer || comp == SaveComparisonResult.BackupNewer || comp == SaveComparisonResult.Different)
                {
                    hasConflict = true;
                }
            }

            lblLastSync.Text = lastDate.HasValue ? $"Last Sync: {lastDate.Value:yyyy-MM-dd HH:mm:ss}" : "Last Sync: Never";

            if (hasConflict)
            {
                lblSyncStatus.ForeColor = Color.FromArgb(248, 113, 113);
                lblSyncStatus.Text = "Sync Status: Conflicts Detected!";
            }
            else if (!anySynced)
            {
                lblSyncStatus.ForeColor = Color.FromArgb(156, 163, 175);
                lblSyncStatus.Text = "Sync Status: Not Synced";
            }
            else
            {
                lblSyncStatus.ForeColor = Color.FromArgb(52, 211, 153);
                lblSyncStatus.Text = "Sync Status: Up to date";
            }
        }

        private void PopulateEmulatorSavesList()
        {
            flpEmulatorSaves.Controls.Clear();

            var config = EmulatorManager.LoadConfig();
            foreach (var emu in config.Emulators)
            {
                Panel pnlRow = new Panel
                {
                    Size = new Size(450, 65),
                    Margin = new Padding(0, 0, 0, 10),
                    BackColor = Color.FromArgb(31, 31, 35)
                };

                Label lblName = new Label
                {
                    Text = emu.Name,
                    Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(10, 8),
                    AutoSize = true
                };

                TextBox tbPath = new TextBox
                {
                    Text = SaveManager.Instance.DetectSaveFolder(emu.Path),
                    Location = new Point(10, 32),
                    Size = new Size(260, 23),
                    ReadOnly = true,
                    BackColor = Color.FromArgb(20, 20, 24),
                    ForeColor = Color.FromArgb(200, 200, 200),
                    BorderStyle = BorderStyle.FixedSingle
                };

                Button btnBrowse = new Button
                {
                    Text = "...",
                    Size = new Size(30, 23),
                    Location = new Point(275, 32),
                    BackColor = Color.FromArgb(55, 65, 81),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btnBrowse.FlatAppearance.BorderSize = 0;

                Button btnBackup = new Button
                {
                    Text = "Backup",
                    Size = new Size(60, 23),
                    Location = new Point(315, 32),
                    BackColor = Color.FromArgb(99, 102, 241),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btnBackup.FlatAppearance.BorderSize = 0;

                Button btnRestore = new Button
                {
                    Text = "Restore",
                    Size = new Size(60, 23),
                    Location = new Point(380, 32),
                    BackColor = Color.FromArgb(55, 65, 81),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat,
                    Cursor = Cursors.Hand
                };
                btnRestore.FlatAppearance.BorderSize = 0;

                btnBrowse.Click += (s, e) =>
                {
                    using (var fbd = new FolderBrowserDialog())
                    {
                        fbd.Description = $"Select saves folder for {emu.Name}";
                        if (fbd.ShowDialog(this) == DialogResult.OK)
                        {
                            tbPath.Text = fbd.SelectedPath;
                            SaveManager.Instance.SetCustomSaveFolder(emu.Path, fbd.SelectedPath);
                        }
                    }
                };

                btnBackup.Click += (s, e) =>
                {
                    var lib = new GameLibraryManager();
                    var emuGames = lib.Games.Where(g => g.EmulatorId.Equals(emu.Path, StringComparison.OrdinalIgnoreCase) || g.EmulatorId.Equals(emu.Name, StringComparison.OrdinalIgnoreCase)).ToList();
                    
                    if (emuGames.Count == 0)
                    {
                        MessageBox.Show($"No games in library use the {emu.Name} emulator.", "Backup Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    int successCount = 0;
                    foreach (var game in emuGames)
                    {
                        if (SaveManager.Instance.BackupSaves(game.Id))
                        {
                            successCount++;
                        }
                    }

                    if (successCount > 0)
                    {
                        MessageBox.Show($"Successfully backed up {successCount} game saves for {emu.Name}!", "Backup Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        UpdateSavesStatusUI();
                    }
                    else
                    {
                        MessageBox.Show($"No save files found to backup for {emu.Name}.", "Backup Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                };

                btnRestore.Click += (s, e) =>
                {
                    var lib = new GameLibraryManager();
                    var emuGames = lib.Games.Where(g => g.EmulatorId.Equals(emu.Path, StringComparison.OrdinalIgnoreCase) || g.EmulatorId.Equals(emu.Name, StringComparison.OrdinalIgnoreCase)).ToList();

                    if (emuGames.Count == 0)
                    {
                        MessageBox.Show($"No games in library use the {emu.Name} emulator.", "Restore Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    int restoreCount = 0;
                    foreach (var game in emuGames)
                    {
                        if (SaveManager.Instance.RestoreSaves(game.Id))
                        {
                            restoreCount++;
                        }
                    }

                    if (restoreCount > 0)
                    {
                        MessageBox.Show($"Successfully restored {restoreCount} game saves for {emu.Name}!", "Restore Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        UpdateSavesStatusUI();
                    }
                };

                pnlRow.Controls.Add(lblName);
                pnlRow.Controls.Add(tbPath);
                pnlRow.Controls.Add(btnBrowse);
                pnlRow.Controls.Add(btnBackup);
                pnlRow.Controls.Add(btnRestore);

                flpEmulatorSaves.Controls.Add(pnlRow);
            }
        }

        private void btnSyncAll_Click(object? sender, EventArgs e)
        {
            var lib = new GameLibraryManager();
            if (lib.Games.Count == 0)
            {
                MessageBox.Show("No games in library to sync.", "Sync Saves", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int backupCount = 0;
            int conflictCount = 0;

            foreach (var game in lib.Games)
            {
                var comp = SaveManager.Instance.CompareLocalAndBackupSaves(game.Id);
                if (comp == SaveComparisonResult.LocalNewer || comp == SaveComparisonResult.BackupNewer || comp == SaveComparisonResult.Different)
                {
                    var result = SaveManager.Instance.ShowConflictDialog(game.Title, comp);
                    if (result == DialogResult.Yes)
                    {
                        if (SaveManager.Instance.BackupSaves(game.Id)) backupCount++;
                    }
                    else if (result == DialogResult.No)
                    {
                        SaveManager.Instance.RestoreSaves(game.Id);
                    }
                    else
                    {
                        conflictCount++;
                    }
                }
                else
                {
                    if (SaveManager.Instance.BackupSaves(game.Id))
                    {
                        backupCount++;
                    }
                }
            }

            string msg = $"Sync complete!\n\nSuccessfully backed up/updated {backupCount} game saves.";
            if (conflictCount > 0)
            {
                msg += $"\n\nNote: {conflictCount} game save syncs were aborted due to conflicts.";
            }

            MessageBox.Show(msg, "Saves Sync Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            UpdateSavesStatusUI();
        }

        private void cbProvider_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cbProvider.SelectedItem != null)
            {
                SaveManager.Instance.ActiveProvider = cbProvider.SelectedItem.ToString() ?? "Local Backup";
                SaveManager.Instance.SaveMetadata();
                UpdateSavesStatusUI();
            }
        }

        private void InitializeSteamProfileLayout()
        {
            // Hide old editing controls
            lblUsername.Visible = false;
            tbUsername.Visible = false;
            lblBio.Visible = false;
            tbBio.Visible = false;
            lblFavoriteConsole.Visible = false;
            cbFavoriteConsole.Visible = false;
            lblThemeColor.Visible = false;
            cbThemeColor.Visible = false;
            lblUpdateChannel.Visible = false;
            cbUpdateChannel.Visible = false;
            btnSaveProfile.Visible = false;

            // Reposition original stats and showcase controls
            lblShowcaseHeader.Location = new Point(20, 240);
            flpShowcase.Location = new Point(20, 265);
            flpShowcase.Size = new Size(220, 60);

            lblStatsHeader.Location = new Point(265, 240);
            lblStatTotalPlaytime.Location = new Point(265, 265);
            lblStatTotalGames.Location = new Point(265, 290);

            lblActivityHeader.Location = new Point(265, 330);
            lbActivities.Location = new Point(265, 355);
            lbActivities.Size = new Size(235, 245);
            lbActivities.BorderStyle = BorderStyle.None;

            // Create Steam controls
            pbProfileBanner = new PictureBox
            {
                Location = new Point(20, 50),
                Size = new Size(480, 120),
                SizeMode = PictureBoxSizeMode.CenterImage,
                BackColor = Color.FromArgb(31, 31, 35),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlMyProfile.Controls.Add(pbProfileBanner);

            pbProfileAvatar = new PictureBox
            {
                Location = new Point(35, 110),
                Size = new Size(50, 50),
                SizeMode = PictureBoxSizeMode.Zoom,
                BackColor = Color.FromArgb(19, 19, 22),
                BorderStyle = BorderStyle.FixedSingle
            };
            pnlMyProfile.Controls.Add(pbProfileAvatar);
            pbProfileAvatar.BringToFront();

            lblProfileName = new Label
            {
                Location = new Point(100, 110),
                AutoSize = true,
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            pnlMyProfile.Controls.Add(lblProfileName);
            lblProfileName.BringToFront();

            lblStatus = new Label
            {
                Location = new Point(100, 132),
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                ForeColor = Color.FromArgb(16, 185, 129),
                BackColor = Color.Transparent
            };
            pnlMyProfile.Controls.Add(lblStatus);
            lblStatus.BringToFront();

            lblFriendCodeDisplay = new Label
            {
                Location = new Point(100, 150),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 8F),
                ForeColor = Color.FromArgb(156, 163, 175),
                BackColor = Color.Transparent
            };
            pnlMyProfile.Controls.Add(lblFriendCodeDisplay);
            lblFriendCodeDisplay.BringToFront();

            lblBioDisplay = new Label
            {
                Location = new Point(20, 180),
                Size = new Size(480, 50),
                Font = new Font("Segoe UI", 9F, FontStyle.Italic),
                ForeColor = Color.FromArgb(209, 213, 223),
                BackColor = Color.Transparent
            };
            pnlMyProfile.Controls.Add(lblBioDisplay);

            btnEditProfile = new Button
            {
                Text = "⚙️  Edit Profile",
                Location = new Point(380, 15),
                Size = new Size(120, 28),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                BackColor = Color.FromArgb(55, 65, 81),
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 8.5F)
            };
            btnEditProfile.FlatAppearance.BorderSize = 0;
            btnEditProfile.Click += btnEditProfile_Click;
            SetupProfileButtonHover(btnEditProfile, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            pnlMyProfile.Controls.Add(btnEditProfile);

            // Favorite Console
            lblFavConsoleHeader = new Label
            {
                Text = "🕹️ FAVORITE CONSOLE",
                Location = new Point(20, 335),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175)
            };
            pnlMyProfile.Controls.Add(lblFavConsoleHeader);

            lblFavConsoleDisplay = new Label
            {
                Location = new Point(20, 360),
                AutoSize = true,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.White
            };
            pnlMyProfile.Controls.Add(lblFavConsoleDisplay);

            // Favorite Games
            lblFavGamesHeader = new Label
            {
                Text = "★ FAVORITE GAMES",
                Location = new Point(20, 395),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175)
            };
            pnlMyProfile.Controls.Add(lblFavGamesHeader);

            flpFavGames = new FlowLayoutPanel
            {
                Location = new Point(20, 420),
                Size = new Size(220, 85),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            pnlMyProfile.Controls.Add(flpFavGames);

            // Last Played Games
            lblLastPlayedHeader = new Label
            {
                Text = "🕒 LAST PLAYED GAMES",
                Location = new Point(20, 515),
                AutoSize = true,
                Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175)
            };
            pnlMyProfile.Controls.Add(lblLastPlayedHeader);

            flpLastPlayed = new FlowLayoutPanel
            {
                Location = new Point(20, 540),
                Size = new Size(220, 65),
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            pnlMyProfile.Controls.Add(flpLastPlayed);
        }

        private void PopulateSteamProfileData()
        {
            // Set dynamic Steam values
            lblProfileName.Text = _profile.Username;
            lblStatus.Text = _profile.Status.ToString();
            lblFriendCodeDisplay.Text = $"Friend Code: {_profile.FriendCode}";
            lblBioDisplay.Text = string.IsNullOrEmpty(_profile.Bio) ? "No bio provided." : _profile.Bio;
            lblFavConsoleDisplay.Text = _profile.FavoriteConsole;

            // Load Avatar Image
            if (pbProfileAvatar.Image != null)
            {
                pbProfileAvatar.Image.Dispose();
                pbProfileAvatar.Image = null;
            }
            if (!string.IsNullOrEmpty(_profile.AvatarPath) && File.Exists(_profile.AvatarPath))
            {
                try
                {
                    using (var fs = new FileStream(_profile.AvatarPath, FileMode.Open, FileAccess.Read))
                    {
                        pbProfileAvatar.Image = Image.FromStream(fs);
                    }
                }
                catch
                {
                    pbProfileAvatar.Image = null;
                }
            }
            else
            {
                // Silhouette placeholder
                pbProfileAvatar.Image = null;
            }

            // Load Banner Image
            if (pbProfileBanner.Image != null)
            {
                pbProfileBanner.Image.Dispose();
                pbProfileBanner.Image = null;
            }
            if (!string.IsNullOrEmpty(_profile.BannerPath) && File.Exists(_profile.BannerPath))
            {
                try
                {
                    using (var fs = new FileStream(_profile.BannerPath, FileMode.Open, FileAccess.Read))
                    {
                        pbProfileBanner.Image = Image.FromStream(fs);
                    }
                }
                catch
                {
                    pbProfileBanner.Image = null;
                }
            }
            else
            {
                // Gradient or solid color banner
                pbProfileBanner.Image = null;
            }

            // Populate Favorite Games
            flpFavGames.Controls.Clear();
            if (_profile.FavoriteGames.Count == 0)
            {
                Label lblNone = new Label
                {
                    Text = "None selected.",
                    ForeColor = Color.FromArgb(156, 163, 175),
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                    AutoSize = true
                };
                flpFavGames.Controls.Add(lblNone);
            }
            else
            {
                foreach (var game in _profile.FavoriteGames)
                {
                    Label lblGame = new Label
                    {
                        Text = $"• {game}",
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 9F),
                        AutoSize = true
                    };
                    flpFavGames.Controls.Add(lblGame);
                }
            }

            // Populate Last Played Games
            flpLastPlayed.Controls.Clear();
            var playtimeRecords = PlaytimeManager.Instance.GetAllRecords()
                .Where(r => r.TotalPlaytimeMinutes > 0 && !string.IsNullOrEmpty(r.LastPlayed))
                .OrderByDescending(r => r.LastPlayed)
                .Take(3)
                .ToList();

            if (playtimeRecords.Count == 0)
            {
                Label lblNone = new Label
                {
                    Text = "No games played yet.",
                    ForeColor = Color.FromArgb(156, 163, 175),
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                    AutoSize = true
                };
                flpLastPlayed.Controls.Add(lblNone);
            }
            else
            {
                foreach (var rec in playtimeRecords)
                {
                    // Find game name in library
                    var game = _libraryManager.Games.FirstOrDefault(g => g.Id == rec.GameId);
                    string gameTitle = game != null ? game.Title : rec.GameId;
                    
                    Label lblGame = new Label
                    {
                        Text = $"• {gameTitle} ({rec.TotalPlaytimeMinutes}m)",
                        ForeColor = Color.White,
                        Font = new Font("Segoe UI", 9F),
                        AutoSize = true
                    };
                    flpLastPlayed.Controls.Add(lblGame);
                }
            }
        }

        private void btnEditProfile_Click(object? sender, EventArgs e)
        {
            using (var editForm = new EditProfileForm(_profile))
            {
                if (editForm.ShowDialog(this) == DialogResult.OK)
                {
                    LoadData(); // Reload profile display
                }
            }
        }

        private void SetupProfileButtonHover(Button btn, Color normal, Color hover)
        {
            btn.BackColor = normal;
            btn.MouseEnter += (s, ev) => btn.BackColor = hover;
            btn.MouseLeave += (s, ev) => btn.BackColor = normal;
        }
    }

    public class UpdaterSettings
    {
        public string UpdateChannel { get; set; } = "stable";
        public string SkippedVersion { get; set; } = "";
        public string LastCheckTime { get; set; } = "";
    }
}
