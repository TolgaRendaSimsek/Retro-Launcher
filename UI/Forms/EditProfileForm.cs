using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace RetroLauncher.UI.Forms
{
    public partial class EditProfileForm : Form
    {
        private readonly IFriendsService _friendsService = new MockFriendsService();
        private readonly GameLibraryManager _libraryManager = new();
        private UserProfile _profile = new();
        
        private string? _tempAvatarPath;
        private string? _tempBannerPath;

        public EditProfileForm(UserProfile profile)
        {
            InitializeComponent();
            _profile = profile;
            SetupCustomEvents();
        }

        private void SetupCustomEvents()
        {
            this.Load += EditProfileForm_Load;
            btnBrowseAvatar.Click += btnBrowseAvatar_Click;
            btnBrowseBanner.Click += btnBrowseBanner_Click;
            clbFavoriteGames.ItemCheck += clbFavoriteGames_ItemCheck;
            
            btnSave.Click += btnSave_Click;
            btnCancel.Click += (s, e) => this.Close();

            // Hover styles
            SetupButtonHover(btnBrowseAvatar, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupButtonHover(btnBrowseBanner, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));
            SetupButtonHover(btnSave, Color.FromArgb(99, 102, 241), Color.FromArgb(79, 70, 229));
            SetupButtonHover(btnCancel, Color.FromArgb(55, 65, 81), Color.FromArgb(31, 41, 55));

            ThemeManager.Instance.ThemeChanged += (s, e) => ThemeManager.Instance.ApplyTheme(this);
            LocalizationManager.Instance.LanguageChanged += (s, e) => LocalizationManager.Instance.ApplyLanguage(this);
        }

        private void SetupButtonHover(Button btn, Color normal, Color hover)
        {
            btn.BackColor = normal;
            btn.MouseEnter += (s, e) => btn.BackColor = hover;
            btn.MouseLeave += (s, e) => btn.BackColor = normal;
        }

        private void EditProfileForm_Load(object? sender, EventArgs e)
        {
            PopulateDropdowns();
            PopulateGamesList();
            LoadCurrentProfileData();
            
            ThemeManager.Instance.ApplyTheme(this);
            LocalizationManager.Instance.ApplyLanguage(this);
        }

        private void PopulateDropdowns()
        {
            // Favorite Consoles
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
            cbFavoriteConsole.SelectedIndex = 0;

            // Theme Colors
            cbThemeColor.Items.Clear();
            cbThemeColor.Items.AddRange(new[]
            {
                "Indigo (#6366F1)",
                "Emerald (#10B981)",
                "Red (#EF4444)",
                "Amber (#F59E0B)",
                "Sky Blue (#0EA5E9)",
                "Rose (#F43F5E)",
                "Fuchsia (#D946EF)",
                "Violet (#8B5CF6)"
            });
            cbThemeColor.SelectedIndex = 0;
        }

        private void PopulateGamesList()
        {
            clbFavoriteGames.Items.Clear();
            foreach (var game in _libraryManager.Games)
            {
                clbFavoriteGames.Items.Add(game.Title);
            }
        }

        private void LoadCurrentProfileData()
        {
            tbUsername.Text = _profile.Username;
            tbBio.Text = _profile.Bio;

            // Console index
            int consoleIdx = cbFavoriteConsole.FindStringExact(_profile.FavoriteConsole);
            cbFavoriteConsole.SelectedIndex = consoleIdx >= 0 ? consoleIdx : 0;

            // Theme color index
            int colorIdx = cbThemeColor.Items.Cast<string>().ToList()
                .FindIndex(item => item.Contains(_profile.ThemeColor));
            cbThemeColor.SelectedIndex = colorIdx >= 0 ? colorIdx : 0;

            // Avatar & Banner previews
            _tempAvatarPath = _profile.AvatarPath;
            _tempBannerPath = _profile.BannerPath;
            UpdatePreviews();

            // Checked list box check matching
            for (int i = 0; i < clbFavoriteGames.Items.Count; i++)
            {
                string title = clbFavoriteGames.Items[i].ToString() ?? "";
                if (_profile.FavoriteGames.Contains(title))
                {
                    clbFavoriteGames.SetItemChecked(i, true);
                }
            }
        }

        private void UpdatePreviews()
        {
            // Avatar preview
            if (pbAvatarPreview.Image != null)
            {
                pbAvatarPreview.Image.Dispose();
                pbAvatarPreview.Image = null;
            }
            if (!string.IsNullOrEmpty(_tempAvatarPath) && File.Exists(_tempAvatarPath))
            {
                try
                {
                    using (var fs = new FileStream(_tempAvatarPath, FileMode.Open, FileAccess.Read))
                    {
                        pbAvatarPreview.Image = Image.FromStream(fs);
                    }
                }
                catch
                {
                    pbAvatarPreview.Image = null;
                }
            }

            // Banner preview
            if (pbBannerPreview.Image != null)
            {
                pbBannerPreview.Image.Dispose();
                pbBannerPreview.Image = null;
            }
            if (!string.IsNullOrEmpty(_tempBannerPath) && File.Exists(_tempBannerPath))
            {
                try
                {
                    using (var fs = new FileStream(_tempBannerPath, FileMode.Open, FileAccess.Read))
                    {
                        pbBannerPreview.Image = Image.FromStream(fs);
                    }
                }
                catch
                {
                    pbBannerPreview.Image = null;
                }
            }
        }

        private void btnBrowseAvatar_Click(object? sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";
                ofd.Title = "Select Profile Avatar Image";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    _tempAvatarPath = ofd.FileName;
                    UpdatePreviews();
                }
            }
        }

        private void btnBrowseBanner_Click(object? sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp";
                ofd.Title = "Select Profile Banner Image";
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    _tempBannerPath = ofd.FileName;
                    UpdatePreviews();
                }
            }
        }

        private void clbFavoriteGames_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            // Limit to max 5 favorites
            int checkedCount = clbFavoriteGames.CheckedItems.Count;
            if (e.NewValue == CheckState.Checked)
            {
                if (checkedCount >= 5)
                {
                    e.NewValue = CheckState.Unchecked;
                    MessageBox.Show("You can select a maximum of 5 favorite games.", "Limit Reached", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(tbUsername.Text))
            {
                MessageBox.Show("Username cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool avatarChanged = _profile.AvatarPath != (_tempAvatarPath ?? "");
            bool bannerChanged = _profile.BannerPath != (_tempBannerPath ?? "");

            _profile.Username = tbUsername.Text.Trim();
            _profile.Bio = tbBio.Text.Trim();
            _profile.FavoriteConsole = cbFavoriteConsole.SelectedItem?.ToString() ?? "All Consoles";
            _profile.AvatarPath = _tempAvatarPath ?? "";
            _profile.BannerPath = _tempBannerPath ?? "";

            // Parse hex color code
            string selectedColorText = cbThemeColor.SelectedItem?.ToString() ?? "Indigo (#6366F1)";
            int startIdx = selectedColorText.IndexOf('(');
            int endIdx = selectedColorText.IndexOf(')');
            if (startIdx >= 0 && endIdx > startIdx)
            {
                _profile.ThemeColor = selectedColorText.Substring(startIdx + 1, endIdx - startIdx - 1);
            }

            // Gather favorite games list
            _profile.FavoriteGames.Clear();
            foreach (var checkedItem in clbFavoriteGames.CheckedItems)
            {
                _profile.FavoriteGames.Add(checkedItem.ToString() ?? "");
            }

            _friendsService.SaveLocalProfile(_profile);

            if (avatarChanged)
            {
                _friendsService.LogActivity("Changed profile avatar picture.");
            }
            if (bannerChanged)
            {
                _friendsService.LogActivity("Changed profile banner image.");
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            if (pbAvatarPreview.Image != null) pbAvatarPreview.Image.Dispose();
            if (pbBannerPreview.Image != null) pbBannerPreview.Image.Dispose();
        }
    }
}
