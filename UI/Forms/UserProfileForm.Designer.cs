namespace RetroLauncher.UI.Forms
{
    partial class UserProfileForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlLeftSidebar;
        private System.Windows.Forms.Button btnTabProfile;
        private System.Windows.Forms.Button btnTabFriends;
        private System.Windows.Forms.Button btnTabPending;
        private System.Windows.Forms.Button btnTabAddFriend;
        private System.Windows.Forms.Button btnTabBlocked;
        private System.Windows.Forms.Panel pnlMainContent;
        
        // Profile Panel Controls
        private System.Windows.Forms.Panel pnlMyProfile;
        private System.Windows.Forms.Label lblProfileTitle;
        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox tbUsername;
        private System.Windows.Forms.Label lblFriendCodeLabel;
        private System.Windows.Forms.Label lblFriendCodeValue;
        private System.Windows.Forms.Label lblBio;
        private System.Windows.Forms.TextBox tbBio;
        private System.Windows.Forms.Label lblFavoriteConsole;
        private System.Windows.Forms.ComboBox cbFavoriteConsole;
        private System.Windows.Forms.Label lblThemeColor;
        private System.Windows.Forms.ComboBox cbThemeColor;
        private System.Windows.Forms.Label lblUpdateChannel;
        private System.Windows.Forms.ComboBox cbUpdateChannel;
        private System.Windows.Forms.Button btnSaveProfile;
        private System.Windows.Forms.Label lblStatsHeader;
        private System.Windows.Forms.Label lblStatTotalPlaytime;
        private System.Windows.Forms.Label lblStatTotalGames;
        private System.Windows.Forms.Label lblActivityHeader;
        private System.Windows.Forms.ListBox lbActivities;

        // Friends Panel Controls
        private System.Windows.Forms.Panel pnlFriendsList;
        private System.Windows.Forms.Label lblFriendsTitle;
        private System.Windows.Forms.ListBox lbFriends;
        private System.Windows.Forms.Button btnRemoveFriend;
        private System.Windows.Forms.Button btnBlockFriend;

        // Pending Panel Controls
        private System.Windows.Forms.Panel pnlPending;
        private System.Windows.Forms.Label lblPendingTitle;
        private System.Windows.Forms.FlowLayoutPanel flpPendingRequests;

        // Add Friend Panel Controls
        private System.Windows.Forms.Panel pnlAddFriend;
        private System.Windows.Forms.Label lblAddFriendTitle;
        private System.Windows.Forms.Label lblAddFriendInstructions;
        private System.Windows.Forms.TextBox tbAddFriendCode;
        private System.Windows.Forms.Button btnSendRequest;
        private System.Windows.Forms.Label lblAddFriendStatus;

        // Blocked Panel Controls
        private System.Windows.Forms.Panel pnlBlocked;
        private System.Windows.Forms.Label lblBlockedTitle;
        private System.Windows.Forms.ListBox lbBlockedUsers;
        private System.Windows.Forms.Button btnUnblockUser;
        private System.Windows.Forms.Label lblShowcaseHeader;
        private System.Windows.Forms.FlowLayoutPanel flpShowcase;
        private System.Windows.Forms.Button btnTabSaves;
        private System.Windows.Forms.Panel pnlSavesSync;
        private System.Windows.Forms.Label lblSavesTitle;
        private System.Windows.Forms.Label lblProvider;
        private System.Windows.Forms.ComboBox cbProvider;
        private System.Windows.Forms.Label lblSyncStatus;
        private System.Windows.Forms.Label lblLastSync;
        private System.Windows.Forms.Button btnSyncAll;
        private System.Windows.Forms.Label lblPathsHeader;
        private System.Windows.Forms.FlowLayoutPanel flpEmulatorSaves;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlLeftSidebar = new System.Windows.Forms.Panel();
            this.btnTabProfile = new System.Windows.Forms.Button();
            this.btnTabFriends = new System.Windows.Forms.Button();
            this.btnTabPending = new System.Windows.Forms.Button();
            this.btnTabAddFriend = new System.Windows.Forms.Button();
            this.btnTabBlocked = new System.Windows.Forms.Button();
            this.pnlMainContent = new System.Windows.Forms.Panel();
            
            // Profile Controls
            this.pnlMyProfile = new System.Windows.Forms.Panel();
            this.btnTabSaves = new System.Windows.Forms.Button();
            this.pnlSavesSync = new System.Windows.Forms.Panel();
            this.lblSavesTitle = new System.Windows.Forms.Label();
            this.lblProvider = new System.Windows.Forms.Label();
            this.cbProvider = new System.Windows.Forms.ComboBox();
            this.lblSyncStatus = new System.Windows.Forms.Label();
            this.lblLastSync = new System.Windows.Forms.Label();
            this.btnSyncAll = new System.Windows.Forms.Button();
            this.lblPathsHeader = new System.Windows.Forms.Label();
            this.flpEmulatorSaves = new System.Windows.Forms.FlowLayoutPanel();
            this.lblProfileTitle = new System.Windows.Forms.Label();
            this.lblUsername = new System.Windows.Forms.Label();
            this.tbUsername = new System.Windows.Forms.TextBox();
            this.lblFriendCodeLabel = new System.Windows.Forms.Label();
            this.lblFriendCodeValue = new System.Windows.Forms.Label();
            this.lblBio = new System.Windows.Forms.Label();
            this.tbBio = new System.Windows.Forms.TextBox();
            this.lblFavoriteConsole = new System.Windows.Forms.Label();
            this.cbFavoriteConsole = new System.Windows.Forms.ComboBox();
            this.lblThemeColor = new System.Windows.Forms.Label();
            this.cbThemeColor = new System.Windows.Forms.ComboBox();
            this.lblUpdateChannel = new System.Windows.Forms.Label();
            this.cbUpdateChannel = new System.Windows.Forms.ComboBox();
            this.btnSaveProfile = new System.Windows.Forms.Button();
            this.lblStatsHeader = new System.Windows.Forms.Label();
            this.lblStatTotalPlaytime = new System.Windows.Forms.Label();
            this.lblStatTotalGames = new System.Windows.Forms.Label();
            this.lblActivityHeader = new System.Windows.Forms.Label();
            this.lbActivities = new System.Windows.Forms.ListBox();
            this.lblShowcaseHeader = new System.Windows.Forms.Label();
            this.flpShowcase = new System.Windows.Forms.FlowLayoutPanel();

            // Friends Controls
            this.pnlFriendsList = new System.Windows.Forms.Panel();
            this.lblFriendsTitle = new System.Windows.Forms.Label();
            this.lbFriends = new System.Windows.Forms.ListBox();
            this.btnRemoveFriend = new System.Windows.Forms.Button();
            this.btnBlockFriend = new System.Windows.Forms.Button();

            // Pending Controls
            this.pnlPending = new System.Windows.Forms.Panel();
            this.lblPendingTitle = new System.Windows.Forms.Label();
            this.flpPendingRequests = new System.Windows.Forms.FlowLayoutPanel();

            // Add Friend Controls
            this.pnlAddFriend = new System.Windows.Forms.Panel();
            this.lblAddFriendTitle = new System.Windows.Forms.Label();
            this.lblAddFriendInstructions = new System.Windows.Forms.Label();
            this.tbAddFriendCode = new System.Windows.Forms.TextBox();
            this.btnSendRequest = new System.Windows.Forms.Button();
            this.lblAddFriendStatus = new System.Windows.Forms.Label();

            // Blocked Controls
            this.pnlBlocked = new System.Windows.Forms.Panel();
            this.lblBlockedTitle = new System.Windows.Forms.Label();
            this.lbBlockedUsers = new System.Windows.Forms.ListBox();
            this.btnUnblockUser = new System.Windows.Forms.Button();

            this.pnlLeftSidebar.SuspendLayout();
            this.pnlMainContent.SuspendLayout();
            this.pnlMyProfile.SuspendLayout();
            this.pnlFriendsList.SuspendLayout();
            this.pnlPending.SuspendLayout();
            this.pnlAddFriend.SuspendLayout();
            this.pnlBlocked.SuspendLayout();
            this.SuspendLayout();

            // 
            // pnlLeftSidebar
            // 
            this.pnlLeftSidebar.BackColor = System.Drawing.Color.FromArgb(19, 19, 22);
            this.pnlLeftSidebar.Controls.Add(this.btnTabProfile);
            this.pnlLeftSidebar.Controls.Add(this.btnTabFriends);
            this.pnlLeftSidebar.Controls.Add(this.btnTabPending);
            this.pnlLeftSidebar.Controls.Add(this.btnTabAddFriend);
            this.pnlLeftSidebar.Controls.Add(this.btnTabBlocked);
            this.pnlLeftSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlLeftSidebar.Location = new System.Drawing.Point(0, 0);
            this.pnlLeftSidebar.Name = "pnlLeftSidebar";
            this.pnlLeftSidebar.Size = new System.Drawing.Size(180, 620);
            this.pnlLeftSidebar.TabIndex = 0;

            // 
            // btnTabProfile
            // 
            this.btnTabProfile.BackColor = System.Drawing.Color.FromArgb(38, 38, 48);
            this.btnTabProfile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabProfile.FlatAppearance.BorderSize = 0;
            this.btnTabProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabProfile.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnTabProfile.ForeColor = System.Drawing.Color.White;
            this.btnTabProfile.Location = new System.Drawing.Point(0, 20);
            this.btnTabProfile.Name = "btnTabProfile";
            this.btnTabProfile.Size = new System.Drawing.Size(180, 45);
            this.btnTabProfile.TabIndex = 0;
            this.btnTabProfile.Text = "👤  My Profile";
            this.btnTabProfile.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabProfile.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnTabProfile.UseVisualStyleBackColor = false;

            // 
            // btnTabFriends
            // 
            this.btnTabFriends.BackColor = System.Drawing.Color.FromArgb(19, 19, 22);
            this.btnTabFriends.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabFriends.FlatAppearance.BorderSize = 0;
            this.btnTabFriends.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabFriends.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnTabFriends.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.btnTabFriends.Location = new System.Drawing.Point(0, 65);
            this.btnTabFriends.Name = "btnTabFriends";
            this.btnTabFriends.Size = new System.Drawing.Size(180, 45);
            this.btnTabFriends.TabIndex = 1;
            this.btnTabFriends.Text = "👥  Friends List";
            this.btnTabFriends.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabFriends.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnTabFriends.UseVisualStyleBackColor = true;

            // 
            // btnTabPending
            // 
            this.btnTabPending.BackColor = System.Drawing.Color.FromArgb(19, 19, 22);
            this.btnTabPending.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabPending.FlatAppearance.BorderSize = 0;
            this.btnTabPending.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabPending.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnTabPending.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.btnTabPending.Location = new System.Drawing.Point(0, 110);
            this.btnTabPending.Name = "btnTabPending";
            this.btnTabPending.Size = new System.Drawing.Size(180, 45);
            this.btnTabPending.TabIndex = 2;
            this.btnTabPending.Text = "📩  Pending Invites";
            this.btnTabPending.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabPending.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnTabPending.UseVisualStyleBackColor = true;

            // 
            // btnTabAddFriend
            // 
            this.btnTabAddFriend.BackColor = System.Drawing.Color.FromArgb(19, 19, 22);
            this.btnTabAddFriend.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabAddFriend.FlatAppearance.BorderSize = 0;
            this.btnTabAddFriend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabAddFriend.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnTabAddFriend.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.btnTabAddFriend.Location = new System.Drawing.Point(0, 155);
            this.btnTabAddFriend.Name = "btnTabAddFriend";
            this.btnTabAddFriend.Size = new System.Drawing.Size(180, 45);
            this.btnTabAddFriend.TabIndex = 3;
            this.btnTabAddFriend.Text = "➕  Add Friend";
            this.btnTabAddFriend.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabAddFriend.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnTabAddFriend.UseVisualStyleBackColor = true;

            // 
            // btnTabBlocked
            // 
            this.btnTabBlocked.BackColor = System.Drawing.Color.FromArgb(19, 19, 22);
            this.btnTabBlocked.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabBlocked.FlatAppearance.BorderSize = 0;
            this.btnTabBlocked.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabBlocked.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnTabBlocked.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.btnTabBlocked.Location = new System.Drawing.Point(0, 200);
            this.btnTabBlocked.Name = "btnTabBlocked";
            this.btnTabBlocked.Size = new System.Drawing.Size(180, 45);
            this.btnTabBlocked.TabIndex = 4;
            this.btnTabBlocked.Text = "🚫  Blocked Users";
            this.btnTabBlocked.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabBlocked.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnTabBlocked.UseVisualStyleBackColor = true;

            // 
            // pnlMainContent
            // 
            this.pnlMainContent.BackColor = System.Drawing.Color.FromArgb(24, 24, 28);
            this.pnlMainContent.Controls.Add(this.pnlMyProfile);
            this.pnlMainContent.Controls.Add(this.pnlFriendsList);
            this.pnlMainContent.Controls.Add(this.pnlPending);
            this.pnlMainContent.Controls.Add(this.pnlAddFriend);
            this.pnlMainContent.Controls.Add(this.pnlBlocked);
            this.pnlMainContent.Controls.Add(this.pnlSavesSync);
            this.pnlMainContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMainContent.Location = new System.Drawing.Point(180, 0);
            this.pnlMainContent.Name = "pnlMainContent";
            this.pnlMainContent.Size = new System.Drawing.Size(520, 620);
            this.pnlMainContent.TabIndex = 1;

            // 
            // pnlMyProfile
            // 
            this.pnlMyProfile.Controls.Add(this.lblProfileTitle);
            this.pnlMyProfile.Controls.Add(this.lblUsername);
            this.pnlMyProfile.Controls.Add(this.tbUsername);
            this.pnlMyProfile.Controls.Add(this.lblFriendCodeLabel);
            this.pnlMyProfile.Controls.Add(this.lblFriendCodeValue);
            this.pnlMyProfile.Controls.Add(this.lblBio);
            this.pnlMyProfile.Controls.Add(this.tbBio);
            this.pnlMyProfile.Controls.Add(this.lblFavoriteConsole);
            this.pnlMyProfile.Controls.Add(this.cbFavoriteConsole);
            this.pnlMyProfile.Controls.Add(this.lblThemeColor);
            this.pnlMyProfile.Controls.Add(this.cbThemeColor);
            this.pnlMyProfile.Controls.Add(this.lblUpdateChannel);
            this.pnlMyProfile.Controls.Add(this.cbUpdateChannel);
            this.pnlMyProfile.Controls.Add(this.btnSaveProfile);
            this.pnlMyProfile.Controls.Add(this.lblStatsHeader);
            this.pnlMyProfile.Controls.Add(this.lblStatTotalPlaytime);
            this.pnlMyProfile.Controls.Add(this.lblStatTotalGames);
            this.pnlMyProfile.Controls.Add(this.lblActivityHeader);
            this.pnlMyProfile.Controls.Add(this.lbActivities);
            this.pnlMyProfile.Controls.Add(this.lblShowcaseHeader);
            this.pnlMyProfile.Controls.Add(this.flpShowcase);
            this.pnlMyProfile.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlMyProfile.Location = new System.Drawing.Point(0, 0);
            this.pnlMyProfile.Name = "pnlMyProfile";
            this.pnlMyProfile.Size = new System.Drawing.Size(520, 620);
            this.pnlMyProfile.TabIndex = 0;

            // 
            // lblProfileTitle
            // 
            this.lblProfileTitle.AutoSize = true;
            this.lblProfileTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblProfileTitle.ForeColor = System.Drawing.Color.White;
            this.lblProfileTitle.Location = new System.Drawing.Point(20, 20);
            this.lblProfileTitle.Name = "lblProfileTitle";
            this.lblProfileTitle.Size = new System.Drawing.Size(123, 21);
            this.lblProfileTitle.Text = "Profile Settings";

            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            this.lblUsername.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.lblUsername.Location = new System.Drawing.Point(20, 60);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(60, 15);
            this.lblUsername.Text = "Username";

            // 
            // tbUsername
            // 
            this.tbUsername.BackColor = System.Drawing.Color.FromArgb(36, 36, 42);
            this.tbUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbUsername.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.tbUsername.ForeColor = System.Drawing.Color.White;
            this.tbUsername.Location = new System.Drawing.Point(20, 80);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.Size = new System.Drawing.Size(220, 24);

            // 
            // lblFriendCodeLabel
            // 
            this.lblFriendCodeLabel.AutoSize = true;
            this.lblFriendCodeLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            this.lblFriendCodeLabel.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.lblFriendCodeLabel.Location = new System.Drawing.Point(260, 60);
            this.lblFriendCodeLabel.Name = "lblFriendCodeLabel";
            this.lblFriendCodeLabel.Size = new System.Drawing.Size(95, 15);
            this.lblFriendCodeLabel.Text = "Your Friend Code";

            // 
            // lblFriendCodeValue
            // 
            this.lblFriendCodeValue.AutoSize = true;
            this.lblFriendCodeValue.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblFriendCodeValue.ForeColor = System.Drawing.Color.FromArgb(99, 102, 241);
            this.lblFriendCodeValue.Location = new System.Drawing.Point(260, 81);
            this.lblFriendCodeValue.Name = "lblFriendCodeValue";
            this.lblFriendCodeValue.Size = new System.Drawing.Size(89, 19);
            this.lblFriendCodeValue.Text = "0000-0000";

            // 
            // lblBio
            // 
            this.lblBio.AutoSize = true;
            this.lblBio.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            this.lblBio.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.lblBio.Location = new System.Drawing.Point(20, 115);
            this.lblBio.Name = "lblBio";
            this.lblBio.Size = new System.Drawing.Size(53, 15);
            this.lblBio.Text = "About Me";

            // 
            // tbBio
            // 
            this.tbBio.BackColor = System.Drawing.Color.FromArgb(36, 36, 42);
            this.tbBio.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbBio.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tbBio.ForeColor = System.Drawing.Color.White;
            this.tbBio.Location = new System.Drawing.Point(20, 135);
            this.tbBio.Multiline = true;
            this.tbBio.Name = "tbBio";
            this.tbBio.Size = new System.Drawing.Size(220, 50);

            // 
            // lblFavoriteConsole
            // 
            this.lblFavoriteConsole.AutoSize = true;
            this.lblFavoriteConsole.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            this.lblFavoriteConsole.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.lblFavoriteConsole.Location = new System.Drawing.Point(260, 115);
            this.lblFavoriteConsole.Name = "lblFavoriteConsole";
            this.lblFavoriteConsole.Size = new System.Drawing.Size(95, 15);
            this.lblFavoriteConsole.Text = "Favorite Console";

            // 
            // cbFavoriteConsole
            // 
            this.cbFavoriteConsole.BackColor = System.Drawing.Color.FromArgb(36, 36, 42);
            this.cbFavoriteConsole.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbFavoriteConsole.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbFavoriteConsole.ForeColor = System.Drawing.Color.White;
            this.cbFavoriteConsole.FormattingEnabled = true;
            this.cbFavoriteConsole.Location = new System.Drawing.Point(260, 135);
            this.cbFavoriteConsole.Name = "cbFavoriteConsole";
            this.cbFavoriteConsole.Size = new System.Drawing.Size(220, 23);

            // 
            // lblThemeColor
            // 
            this.lblThemeColor.AutoSize = true;
            this.lblThemeColor.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            this.lblThemeColor.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.lblThemeColor.Location = new System.Drawing.Point(20, 195);
            this.lblThemeColor.Name = "lblThemeColor";
            this.lblThemeColor.Size = new System.Drawing.Size(75, 15);
            this.lblThemeColor.Text = "Theme Color";

            // 
            // cbThemeColor
            // 
            this.cbThemeColor.BackColor = System.Drawing.Color.FromArgb(36, 36, 42);
            this.cbThemeColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbThemeColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbThemeColor.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbThemeColor.ForeColor = System.Drawing.Color.White;
            this.cbThemeColor.FormattingEnabled = true;
            this.cbThemeColor.Location = new System.Drawing.Point(20, 215);
            this.cbThemeColor.Name = "cbThemeColor";
            this.cbThemeColor.Size = new System.Drawing.Size(220, 23);

            // 
            // lblUpdateChannel
            // 
            this.lblUpdateChannel.AutoSize = true;
            this.lblUpdateChannel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F);
            this.lblUpdateChannel.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.lblUpdateChannel.Location = new System.Drawing.Point(260, 195);
            this.lblUpdateChannel.Name = "lblUpdateChannel";
            this.lblUpdateChannel.Size = new System.Drawing.Size(147, 15);
            this.lblUpdateChannel.Text = "Launcher Update Channel";

            // 
            // cbUpdateChannel
            // 
            this.cbUpdateChannel.BackColor = System.Drawing.Color.FromArgb(36, 36, 42);
            this.cbUpdateChannel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbUpdateChannel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbUpdateChannel.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbUpdateChannel.ForeColor = System.Drawing.Color.White;
            this.cbUpdateChannel.FormattingEnabled = true;
            this.cbUpdateChannel.Location = new System.Drawing.Point(260, 215);
            this.cbUpdateChannel.Name = "cbUpdateChannel";
            this.cbUpdateChannel.Size = new System.Drawing.Size(220, 23);

            // 
            // btnSaveProfile
            // 
            this.btnSaveProfile.BackColor = System.Drawing.Color.FromArgb(99, 102, 241);
            this.btnSaveProfile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSaveProfile.FlatAppearance.BorderSize = 0;
            this.btnSaveProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveProfile.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnSaveProfile.ForeColor = System.Drawing.Color.White;
            this.btnSaveProfile.Location = new System.Drawing.Point(20, 260);
            this.btnSaveProfile.Name = "btnSaveProfile";
            this.btnSaveProfile.Size = new System.Drawing.Size(460, 35);
            this.btnSaveProfile.Text = "Save Profile && Updater Config";
            this.btnSaveProfile.UseVisualStyleBackColor = false;

            // 
            // lblStatsHeader
            // 
            this.lblStatsHeader.AutoSize = true;
            this.lblStatsHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblStatsHeader.ForeColor = System.Drawing.Color.White;
            this.lblStatsHeader.Location = new System.Drawing.Point(20, 310);
            this.lblStatsHeader.Name = "lblStatsHeader";
            this.lblStatsHeader.Size = new System.Drawing.Size(89, 17);
            this.lblStatsHeader.Text = "My Statistics";

            // 
            // lblStatTotalPlaytime
            // 
            this.lblStatTotalPlaytime.AutoSize = true;
            this.lblStatTotalPlaytime.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatTotalPlaytime.ForeColor = System.Drawing.Color.FromArgb(209, 213, 223);
            this.lblStatTotalPlaytime.Location = new System.Drawing.Point(20, 335);
            this.lblStatTotalPlaytime.Name = "lblStatTotalPlaytime";
            this.lblStatTotalPlaytime.Size = new System.Drawing.Size(126, 15);
            this.lblStatTotalPlaytime.Text = "Total Playtime: 0 mins";

            // 
            // lblStatTotalGames
            // 
            this.lblStatTotalGames.AutoSize = true;
            this.lblStatTotalGames.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblStatTotalGames.ForeColor = System.Drawing.Color.FromArgb(209, 213, 223);
            this.lblStatTotalGames.Location = new System.Drawing.Point(260, 335);
            this.lblStatTotalGames.Name = "lblStatTotalGames";
            this.lblStatTotalGames.Size = new System.Drawing.Size(142, 15);
            this.lblStatTotalGames.Text = "Games in Library: 0 games";

            // 
            // lblActivityHeader
            // 
            this.lblActivityHeader.AutoSize = true;
            this.lblActivityHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblActivityHeader.ForeColor = System.Drawing.Color.White;
            this.lblActivityHeader.Location = new System.Drawing.Point(20, 365);
            this.lblActivityHeader.Name = "lblActivityHeader";
            this.lblActivityHeader.Size = new System.Drawing.Size(86, 17);
            this.lblActivityHeader.Text = "Activity Feed";

            // 
            // lbActivities
            // 
            this.lbActivities.BackColor = System.Drawing.Color.FromArgb(19, 19, 22);
            this.lbActivities.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.lbActivities.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lbActivities.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lbActivities.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.lbActivities.FormattingEnabled = true;
            this.lbActivities.ItemHeight = 15;
            this.lbActivities.Location = new System.Drawing.Point(20, 390);
            this.lbActivities.Name = "lbActivities";
            this.lbActivities.Size = new System.Drawing.Size(460, 90);
            // 
            // lblShowcaseHeader
            // 
            this.lblShowcaseHeader.AutoSize = true;
            this.lblShowcaseHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblShowcaseHeader.ForeColor = System.Drawing.Color.White;
            this.lblShowcaseHeader.Location = new System.Drawing.Point(20, 490);
            this.lblShowcaseHeader.Name = "lblShowcaseHeader";
            this.lblShowcaseHeader.Size = new System.Drawing.Size(160, 17);
            this.lblShowcaseHeader.Text = "Achievement Showcase";
            // 
            // flpShowcase
            // 
            this.flpShowcase.BackColor = System.Drawing.Color.FromArgb(19, 19, 22);
            this.flpShowcase.Location = new System.Drawing.Point(20, 515);
            this.flpShowcase.Name = "flpShowcase";
            this.flpShowcase.Size = new System.Drawing.Size(460, 60);
            this.flpShowcase.TabIndex = 20;

            // 
            // pnlFriendsList
            // 
            this.pnlFriendsList.Controls.Add(this.lblFriendsTitle);
            this.pnlFriendsList.Controls.Add(this.lbFriends);
            this.pnlFriendsList.Controls.Add(this.btnRemoveFriend);
            this.pnlFriendsList.Controls.Add(this.btnBlockFriend);
            this.pnlFriendsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlFriendsList.Location = new System.Drawing.Point(0, 0);
            this.pnlFriendsList.Name = "pnlFriendsList";
            this.pnlFriendsList.Size = new System.Drawing.Size(520, 500);
            this.pnlFriendsList.TabIndex = 1;

            // 
            // lblFriendsTitle
            // 
            this.lblFriendsTitle.AutoSize = true;
            this.lblFriendsTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblFriendsTitle.ForeColor = System.Drawing.Color.White;
            this.lblFriendsTitle.Location = new System.Drawing.Point(20, 20);
            this.lblFriendsTitle.Name = "lblFriendsTitle";
            this.lblFriendsTitle.Size = new System.Drawing.Size(96, 21);
            this.lblFriendsTitle.Text = "Friends List";

            // 
            // lbFriends
            // 
            this.lbFriends.BackColor = System.Drawing.Color.FromArgb(19, 19, 22);
            this.lbFriends.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbFriends.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lbFriends.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.lbFriends.ForeColor = System.Drawing.Color.White;
            this.lbFriends.FormattingEnabled = true;
            this.lbFriends.ItemHeight = 45;
            this.lbFriends.Location = new System.Drawing.Point(20, 60);
            this.lbFriends.Name = "lbFriends";
            this.lbFriends.Size = new System.Drawing.Size(480, 360);
            this.lbFriends.TabIndex = 1;

            // 
            // btnRemoveFriend
            // 
            this.btnRemoveFriend.BackColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.btnRemoveFriend.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRemoveFriend.FlatAppearance.BorderSize = 0;
            this.btnRemoveFriend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemoveFriend.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnRemoveFriend.ForeColor = System.Drawing.Color.White;
            this.btnRemoveFriend.Location = new System.Drawing.Point(20, 440);
            this.btnRemoveFriend.Name = "btnRemoveFriend";
            this.btnRemoveFriend.Size = new System.Drawing.Size(220, 35);
            this.btnRemoveFriend.Text = "Remove Friend";
            this.btnRemoveFriend.UseVisualStyleBackColor = false;

            // 
            // btnBlockFriend
            // 
            this.btnBlockFriend.BackColor = System.Drawing.Color.FromArgb(239, 68, 68);
            this.btnBlockFriend.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBlockFriend.FlatAppearance.BorderSize = 0;
            this.btnBlockFriend.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBlockFriend.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnBlockFriend.ForeColor = System.Drawing.Color.White;
            this.btnBlockFriend.Location = new System.Drawing.Point(260, 440);
            this.btnBlockFriend.Name = "btnBlockFriend";
            this.btnBlockFriend.Size = new System.Drawing.Size(240, 35);
            this.btnBlockFriend.Text = "🚫  Block User";
            this.btnBlockFriend.UseVisualStyleBackColor = false;

            // 
            // pnlPending
            // 
            this.pnlPending.Controls.Add(this.lblPendingTitle);
            this.pnlPending.Controls.Add(this.flpPendingRequests);
            this.pnlPending.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlPending.Location = new System.Drawing.Point(0, 0);
            this.pnlPending.Name = "pnlPending";
            this.pnlPending.Size = new System.Drawing.Size(520, 500);
            this.pnlPending.TabIndex = 2;

            // 
            // lblPendingTitle
            // 
            this.lblPendingTitle.AutoSize = true;
            this.lblPendingTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblPendingTitle.ForeColor = System.Drawing.Color.White;
            this.lblPendingTitle.Location = new System.Drawing.Point(20, 20);
            this.lblPendingTitle.Name = "lblPendingTitle";
            this.lblPendingTitle.Size = new System.Drawing.Size(132, 21);
            this.lblPendingTitle.Text = "Pending Invites";

            // 
            // flpPendingRequests
            // 
            this.flpPendingRequests.AutoScroll = true;
            this.flpPendingRequests.BackColor = System.Drawing.Color.FromArgb(19, 19, 22);
            this.flpPendingRequests.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flpPendingRequests.Location = new System.Drawing.Point(20, 60);
            this.flpPendingRequests.Name = "flpPendingRequests";
            this.flpPendingRequests.Size = new System.Drawing.Size(480, 415);
            this.flpPendingRequests.TabIndex = 1;
            this.flpPendingRequests.WrapContents = false;

            // 
            // pnlAddFriend
            // 
            this.pnlAddFriend.Controls.Add(this.lblAddFriendTitle);
            this.pnlAddFriend.Controls.Add(this.lblAddFriendInstructions);
            this.pnlAddFriend.Controls.Add(this.tbAddFriendCode);
            this.pnlAddFriend.Controls.Add(this.btnSendRequest);
            this.pnlAddFriend.Controls.Add(this.lblAddFriendStatus);
            this.pnlAddFriend.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlAddFriend.Location = new System.Drawing.Point(0, 0);
            this.pnlAddFriend.Name = "pnlAddFriend";
            this.pnlAddFriend.Size = new System.Drawing.Size(520, 500);
            this.pnlAddFriend.TabIndex = 3;

            // 
            // lblAddFriendTitle
            // 
            this.lblAddFriendTitle.AutoSize = true;
            this.lblAddFriendTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblAddFriendTitle.ForeColor = System.Drawing.Color.White;
            this.lblAddFriendTitle.Location = new System.Drawing.Point(20, 20);
            this.lblAddFriendTitle.Name = "lblAddFriendTitle";
            this.lblAddFriendTitle.Size = new System.Drawing.Size(97, 21);
            this.lblAddFriendTitle.Text = "Add Friend";

            // 
            // lblAddFriendInstructions
            // 
            this.lblAddFriendInstructions.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblAddFriendInstructions.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.lblAddFriendInstructions.Location = new System.Drawing.Point(20, 60);
            this.lblAddFriendInstructions.Name = "lblAddFriendInstructions";
            this.lblAddFriendInstructions.Size = new System.Drawing.Size(480, 45);
            this.lblAddFriendInstructions.Text = "Enter your friend\'s username or 8-digit Friend Code (e.g. 1111-2222) to send them a friend request.";

            // 
            // tbAddFriendCode
            // 
            this.tbAddFriendCode.BackColor = System.Drawing.Color.FromArgb(36, 36, 42);
            this.tbAddFriendCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbAddFriendCode.Font = new System.Drawing.Font("Segoe UI", 12F);
            this.tbAddFriendCode.ForeColor = System.Drawing.Color.White;
            this.tbAddFriendCode.Location = new System.Drawing.Point(20, 115);
            this.tbAddFriendCode.Name = "tbAddFriendCode";
            this.tbAddFriendCode.PlaceholderText = "Friend Code or Username...";
            this.tbAddFriendCode.Size = new System.Drawing.Size(480, 29);

            // 
            // btnSendRequest
            // 
            this.btnSendRequest.BackColor = System.Drawing.Color.FromArgb(16, 185, 129);
            this.btnSendRequest.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSendRequest.FlatAppearance.BorderSize = 0;
            this.btnSendRequest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSendRequest.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnSendRequest.ForeColor = System.Drawing.Color.White;
            this.btnSendRequest.Location = new System.Drawing.Point(20, 165);
            this.btnSendRequest.Name = "btnSendRequest";
            this.btnSendRequest.Size = new System.Drawing.Size(480, 40);
            this.btnSendRequest.Text = "Send Friend Request";
            this.btnSendRequest.UseVisualStyleBackColor = false;

            // 
            // lblAddFriendStatus
            // 
            this.lblAddFriendStatus.AutoSize = true;
            this.lblAddFriendStatus.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.lblAddFriendStatus.Location = new System.Drawing.Point(20, 220);
            this.lblAddFriendStatus.Name = "lblAddFriendStatus";
            this.lblAddFriendStatus.Size = new System.Drawing.Size(0, 17);

            // 
            // pnlBlocked
            // 
            this.pnlBlocked.Controls.Add(this.lblBlockedTitle);
            this.pnlBlocked.Controls.Add(this.lbBlockedUsers);
            this.pnlBlocked.Controls.Add(this.btnUnblockUser);
            this.pnlBlocked.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlBlocked.Location = new System.Drawing.Point(0, 0);
            this.pnlBlocked.Name = "pnlBlocked";
            this.pnlBlocked.Size = new System.Drawing.Size(520, 500);
            this.pnlBlocked.TabIndex = 4;

            // 
            // lblBlockedTitle
            // 
            this.lblBlockedTitle.AutoSize = true;
            this.lblBlockedTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblBlockedTitle.ForeColor = System.Drawing.Color.White;
            this.lblBlockedTitle.Location = new System.Drawing.Point(20, 20);
            this.lblBlockedTitle.Name = "lblBlockedTitle";
            this.lblBlockedTitle.Size = new System.Drawing.Size(117, 21);
            this.lblBlockedTitle.Text = "Blocked Users";

            // 
            // lbBlockedUsers
            // 
            this.lbBlockedUsers.BackColor = System.Drawing.Color.FromArgb(19, 19, 22);
            this.lbBlockedUsers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbBlockedUsers.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lbBlockedUsers.ForeColor = System.Drawing.Color.White;
            this.lbBlockedUsers.FormattingEnabled = true;
            this.lbBlockedUsers.ItemHeight = 17;
            this.lbBlockedUsers.Location = new System.Drawing.Point(20, 60);
            this.lbBlockedUsers.Name = "lbBlockedUsers";
            this.lbBlockedUsers.Size = new System.Drawing.Size(480, 360);
            this.lbBlockedUsers.TabIndex = 1;

            // 
            // btnUnblockUser
            // 
            this.btnUnblockUser.BackColor = System.Drawing.Color.FromArgb(99, 102, 241);
            this.btnUnblockUser.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUnblockUser.FlatAppearance.BorderSize = 0;
            this.btnUnblockUser.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUnblockUser.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnUnblockUser.ForeColor = System.Drawing.Color.White;
            this.btnUnblockUser.Location = new System.Drawing.Point(20, 440);
            this.btnUnblockUser.Name = "btnUnblockUser";
            this.btnUnblockUser.Size = new System.Drawing.Size(480, 35);
            this.btnUnblockUser.Text = "Unblock User";
            this.btnUnblockUser.UseVisualStyleBackColor = false;

            // 
            // btnTabSaves
            // 
            this.btnTabSaves.BackColor = System.Drawing.Color.FromArgb(19, 19, 22);
            this.btnTabSaves.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTabSaves.FlatAppearance.BorderSize = 0;
            this.btnTabSaves.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTabSaves.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F);
            this.btnTabSaves.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.btnTabSaves.Location = new System.Drawing.Point(0, 245);
            this.btnTabSaves.Name = "btnTabSaves";
            this.btnTabSaves.Size = new System.Drawing.Size(180, 45);
            this.btnTabSaves.TabIndex = 5;
            this.btnTabSaves.Text = "💾  Sync Saves";
            this.btnTabSaves.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnTabSaves.Padding = new System.Windows.Forms.Padding(15, 0, 0, 0);
            this.btnTabSaves.UseVisualStyleBackColor = true;

            // 
            // pnlSavesSync
            // 
            this.pnlSavesSync.Controls.Add(this.lblSavesTitle);
            this.pnlSavesSync.Controls.Add(this.lblProvider);
            this.pnlSavesSync.Controls.Add(this.cbProvider);
            this.pnlSavesSync.Controls.Add(this.lblSyncStatus);
            this.pnlSavesSync.Controls.Add(this.lblLastSync);
            this.pnlSavesSync.Controls.Add(this.btnSyncAll);
            this.pnlSavesSync.Controls.Add(this.lblPathsHeader);
            this.pnlSavesSync.Controls.Add(this.flpEmulatorSaves);
            this.pnlSavesSync.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSavesSync.Location = new System.Drawing.Point(0, 0);
            this.pnlSavesSync.Name = "pnlSavesSync";
            this.pnlSavesSync.Size = new System.Drawing.Size(520, 620);
            this.pnlSavesSync.TabIndex = 6;
            this.pnlSavesSync.Visible = false;

            // 
            // lblSavesTitle
            // 
            this.lblSavesTitle.AutoSize = true;
            this.lblSavesTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 14F, System.Drawing.FontStyle.Bold);
            this.lblSavesTitle.ForeColor = System.Drawing.Color.White;
            this.lblSavesTitle.Location = new System.Drawing.Point(20, 20);
            this.lblSavesTitle.Name = "lblSavesTitle";
            this.lblSavesTitle.Size = new System.Drawing.Size(185, 25);
            this.lblSavesTitle.TabIndex = 0;
            this.lblSavesTitle.Text = "Saves & Cloud Sync";

            // 
            // lblProvider
            // 
            this.lblProvider.AutoSize = true;
            this.lblProvider.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblProvider.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.lblProvider.Location = new System.Drawing.Point(20, 60);
            this.lblProvider.Name = "lblProvider";
            this.lblProvider.Size = new System.Drawing.Size(91, 15);
            this.lblProvider.TabIndex = 1;
            this.lblProvider.Text = "Active Provider:";

            // 
            // cbProvider
            // 
            this.cbProvider.BackColor = System.Drawing.Color.FromArgb(31, 31, 35);
            this.cbProvider.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbProvider.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbProvider.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbProvider.ForeColor = System.Drawing.Color.White;
            this.cbProvider.FormattingEnabled = true;
            this.cbProvider.Location = new System.Drawing.Point(20, 80);
            this.cbProvider.Name = "cbProvider";
            this.cbProvider.Size = new System.Drawing.Size(220, 23);
            this.cbProvider.TabIndex = 2;

            // 
            // lblSyncStatus
            // 
            this.lblSyncStatus.AutoSize = true;
            this.lblSyncStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSyncStatus.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.lblSyncStatus.Location = new System.Drawing.Point(260, 60);
            this.lblSyncStatus.Name = "lblSyncStatus";
            this.lblSyncStatus.Size = new System.Drawing.Size(117, 15);
            this.lblSyncStatus.TabIndex = 3;
            this.lblSyncStatus.Text = "Sync Status: Unknown";

            // 
            // lblLastSync
            // 
            this.lblLastSync.AutoSize = true;
            this.lblLastSync.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblLastSync.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.lblLastSync.Location = new System.Drawing.Point(260, 85);
            this.lblLastSync.Name = "lblLastSync";
            this.lblLastSync.Size = new System.Drawing.Size(95, 15);
            this.lblLastSync.TabIndex = 4;
            this.lblLastSync.Text = "Last Sync: Never";

            // 
            // btnSyncAll
            // 
            this.btnSyncAll.BackColor = System.Drawing.Color.FromArgb(99, 102, 241);
            this.btnSyncAll.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSyncAll.FlatAppearance.BorderSize = 0;
            this.btnSyncAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSyncAll.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnSyncAll.ForeColor = System.Drawing.Color.White;
            this.btnSyncAll.Location = new System.Drawing.Point(260, 110);
            this.btnSyncAll.Name = "btnSyncAll";
            this.btnSyncAll.Size = new System.Drawing.Size(220, 30);
            this.btnSyncAll.TabIndex = 5;
            this.btnSyncAll.Text = "🔄  Sync All Saves Now";
            this.btnSyncAll.UseVisualStyleBackColor = false;

            // 
            // lblPathsHeader
            // 
            this.lblPathsHeader.AutoSize = true;
            this.lblPathsHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 11F, System.Drawing.FontStyle.Bold);
            this.lblPathsHeader.ForeColor = System.Drawing.Color.White;
            this.lblPathsHeader.Location = new System.Drawing.Point(20, 160);
            this.lblPathsHeader.Name = "lblPathsHeader";
            this.lblPathsHeader.Size = new System.Drawing.Size(211, 20);
            this.lblPathsHeader.TabIndex = 6;
            this.lblPathsHeader.Text = "Emulator Save Configurations:";

            // 
            // flpEmulatorSaves
            // 
            this.flpEmulatorSaves.AutoScroll = true;
            this.flpEmulatorSaves.Location = new System.Drawing.Point(20, 190);
            this.flpEmulatorSaves.Name = "flpEmulatorSaves";
            this.flpEmulatorSaves.Size = new System.Drawing.Size(480, 410);
            this.flpEmulatorSaves.TabIndex = 7;

            // 
            // UserProfileForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(24, 24, 28);
            this.ClientSize = new System.Drawing.Size(700, 620);
            this.Controls.Add(this.pnlMainContent);
            this.Controls.Add(this.pnlLeftSidebar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UserProfileForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Profile & Friends System";
            this.pnlLeftSidebar.ResumeLayout(false);
            this.pnlMainContent.ResumeLayout(false);
            this.pnlMyProfile.ResumeLayout(false);
            this.pnlMyProfile.PerformLayout();
            this.pnlFriendsList.ResumeLayout(false);
            this.pnlFriendsList.PerformLayout();
            this.pnlPending.ResumeLayout(false);
            this.pnlPending.PerformLayout();
            this.pnlAddFriend.ResumeLayout(false);
            this.pnlAddFriend.PerformLayout();
            this.pnlBlocked.ResumeLayout(false);
            this.pnlBlocked.PerformLayout();
            this.pnlSavesSync.ResumeLayout(false);
            this.pnlSavesSync.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}
