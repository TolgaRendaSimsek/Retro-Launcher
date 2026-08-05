namespace RetroLauncher.UI.Forms
{
    partial class MainForm
    {
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
            this.btnAddGame = new System.Windows.Forms.Button();
            this.btnManageEmulators = new System.Windows.Forms.Button();
            this.btnProfile = new System.Windows.Forms.Button();
            this.pnlLibraryToolbar = new System.Windows.Forms.Panel();
            this.btnGridView = new System.Windows.Forms.Button();
            this.btnListView = new System.Windows.Forms.Button();
            this.lblSortBy = new System.Windows.Forms.Label();
            this.cbSort = new System.Windows.Forms.ComboBox();
            this.lblFilterBy = new System.Windows.Forms.Label();
            this.cbFilter = new System.Windows.Forms.ComboBox();
            this.pnlSidebar = new System.Windows.Forms.Panel();
            this.lblSidebarHeader = new System.Windows.Forms.Label();
            this.lbConsoleFilter = new System.Windows.Forms.ListBox();
            this.pnlDetails = new System.Windows.Forms.Panel();
            this.pbDetailsCover = new System.Windows.Forms.PictureBox();
            this.lblDetailsTitle = new System.Windows.Forms.Label();
            this.lblDetailsConsole = new System.Windows.Forms.Label();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnEditPaths = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.lblDetailsStatus = new System.Windows.Forms.Label();
            this.flpGamesGrid = new System.Windows.Forms.FlowLayoutPanel();
            this.btnManageSaves = new System.Windows.Forms.Button();
            this.btnManageScreenshots = new System.Windows.Forms.Button();
            this.btnManageVideos = new System.Windows.Forms.Button();
            this.btnManageControllers = new System.Windows.Forms.Button();
            this.btnAppearance = new System.Windows.Forms.Button();
            this.btnLanguageSettings = new System.Windows.Forms.Button();
            this.pnlSidebar.SuspendLayout();
            this.pnlDetails.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbDetailsCover)).BeginInit();
            this.SuspendLayout();
            // 
            // btnAddGame
            // 
            this.btnAddGame.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddGame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.btnAddGame.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAddGame.FlatAppearance.BorderSize = 0;
            this.btnAddGame.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAddGame.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnAddGame.ForeColor = System.Drawing.Color.White;
            this.btnAddGame.Location = new System.Drawing.Point(965, 12);
            this.btnAddGame.Name = "btnAddGame";
            this.btnAddGame.Size = new System.Drawing.Size(120, 35);
            this.btnAddGame.TabIndex = 2;
            this.btnAddGame.Text = "➕  Add Game";
            this.btnAddGame.UseVisualStyleBackColor = false;
            // 
            // btnManageEmulators
            // 
            this.btnManageEmulators.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnManageEmulators.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnManageEmulators.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnManageEmulators.FlatAppearance.BorderSize = 0;
            this.btnManageEmulators.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManageEmulators.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnManageEmulators.ForeColor = System.Drawing.Color.White;
            this.btnManageEmulators.Location = new System.Drawing.Point(810, 12);
            this.btnManageEmulators.Name = "btnManageEmulators";
            this.btnManageEmulators.Size = new System.Drawing.Size(145, 35);
            this.btnManageEmulators.TabIndex = 3;
            this.btnManageEmulators.Text = "⚙️  Manage Emulators";
            this.btnManageEmulators.UseVisualStyleBackColor = false;
            // 
            // btnProfile
            // 
            this.btnProfile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnProfile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnProfile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnProfile.FlatAppearance.BorderSize = 0;
            this.btnProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProfile.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnProfile.ForeColor = System.Drawing.Color.White;
            this.btnProfile.Location = new System.Drawing.Point(515, 12);
            this.btnProfile.Name = "btnProfile";
            this.btnProfile.Size = new System.Drawing.Size(150, 35);
            this.btnProfile.TabIndex = 4;
            this.btnProfile.Text = "👤  Profile && Friends";
            this.btnProfile.UseVisualStyleBackColor = false;
            // 
            // btnAppearance
            // 
            this.btnAppearance.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAppearance.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnAppearance.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAppearance.FlatAppearance.BorderSize = 0;
            this.btnAppearance.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAppearance.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnAppearance.ForeColor = System.Drawing.Color.White;
            this.btnAppearance.Location = new System.Drawing.Point(675, 12);
            this.btnAppearance.Name = "btnAppearance";
            this.btnAppearance.Size = new System.Drawing.Size(125, 35);
            this.btnAppearance.TabIndex = 6;
            this.btnAppearance.Text = "🎨  Theme";
            this.btnAppearance.UseVisualStyleBackColor = false;
            // 
            // pnlSidebar
            // 
            this.pnlSidebar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(19)))), ((int)(((byte)(22)))));
            this.pnlSidebar.Controls.Add(this.lblSidebarHeader);
            this.pnlSidebar.Controls.Add(this.lbConsoleFilter);
            this.pnlSidebar.Controls.Add(this.btnManageSaves);
            this.pnlSidebar.Controls.Add(this.btnManageScreenshots);
            this.pnlSidebar.Controls.Add(this.btnManageVideos);
            this.pnlSidebar.Controls.Add(this.btnManageControllers);
            this.pnlSidebar.Controls.Add(this.btnLanguageSettings);
            this.pnlSidebar.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlSidebar.Location = new System.Drawing.Point(0, 60);
            this.pnlSidebar.Name = "pnlSidebar";
            this.pnlSidebar.Size = new System.Drawing.Size(200, 540);
            this.pnlSidebar.TabIndex = 1;
            // 
            // lblSidebarHeader
            // 
            this.lblSidebarHeader.AutoSize = true;
            this.lblSidebarHeader.Font = new System.Drawing.Font("Segoe UI Black", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSidebarHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblSidebarHeader.Location = new System.Drawing.Point(15, 15);
            this.lblSidebarHeader.Name = "lblSidebarHeader";
            this.lblSidebarHeader.Size = new System.Drawing.Size(64, 13);
            this.lblSidebarHeader.TabIndex = 0;
            this.lblSidebarHeader.Text = "CONSOLES";
            // 
            // lbConsoleFilter
            // 
            this.lbConsoleFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(19)))), ((int)(((byte)(22)))));
            this.lbConsoleFilter.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lbConsoleFilter.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lbConsoleFilter.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lbConsoleFilter.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(223)))));
            this.lbConsoleFilter.FormattingEnabled = true;
            this.lbConsoleFilter.ItemHeight = 32;
            this.lbConsoleFilter.Location = new System.Drawing.Point(10, 40);
            this.lbConsoleFilter.Name = "lbConsoleFilter";
            this.lbConsoleFilter.Size = new System.Drawing.Size(180, 260);
            this.lbConsoleFilter.TabIndex = 1;
            // 
            // btnManageSaves
            // 
            this.btnManageSaves.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnManageSaves.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnManageSaves.FlatAppearance.BorderSize = 0;
            this.btnManageSaves.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManageSaves.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnManageSaves.ForeColor = System.Drawing.Color.White;
            this.btnManageSaves.Location = new System.Drawing.Point(10, 320);
            this.btnManageSaves.Name = "btnManageSaves";
            this.btnManageSaves.Size = new System.Drawing.Size(180, 32);
            this.btnManageSaves.TabIndex = 2;
            this.btnManageSaves.Text = "💾  Manage Saves";
            this.btnManageSaves.UseVisualStyleBackColor = false;
            // 
            // btnManageScreenshots
            // 
            this.btnManageScreenshots.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnManageScreenshots.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnManageScreenshots.FlatAppearance.BorderSize = 0;
            this.btnManageScreenshots.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManageScreenshots.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnManageScreenshots.ForeColor = System.Drawing.Color.White;
            this.btnManageScreenshots.Location = new System.Drawing.Point(10, 360);
            this.btnManageScreenshots.Name = "btnManageScreenshots";
            this.btnManageScreenshots.Size = new System.Drawing.Size(180, 32);
            this.btnManageScreenshots.TabIndex = 3;
            this.btnManageScreenshots.Text = "📸  Manage Screenshots";
            this.btnManageScreenshots.UseVisualStyleBackColor = false;
            // 
            // btnManageVideos
            // 
            this.btnManageVideos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnManageVideos.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnManageVideos.FlatAppearance.BorderSize = 0;
            this.btnManageVideos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManageVideos.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnManageVideos.ForeColor = System.Drawing.Color.White;
            this.btnManageVideos.Location = new System.Drawing.Point(10, 400);
            this.btnManageVideos.Name = "btnManageVideos";
            this.btnManageVideos.Size = new System.Drawing.Size(180, 32);
            this.btnManageVideos.TabIndex = 4;
            this.btnManageVideos.Text = "📹  Manage Videos";
            this.btnManageVideos.UseVisualStyleBackColor = false;
            // 
            // btnManageControllers
            // 
            this.btnManageControllers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnManageControllers.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnManageControllers.FlatAppearance.BorderSize = 0;
            this.btnManageControllers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnManageControllers.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnManageControllers.ForeColor = System.Drawing.Color.White;
            this.btnManageControllers.Location = new System.Drawing.Point(10, 440);
            this.btnManageControllers.Name = "btnManageControllers";
            this.btnManageControllers.Size = new System.Drawing.Size(180, 32);
            this.btnManageControllers.TabIndex = 5;
            this.btnManageControllers.Text = "🎮  Manage Controllers";
            this.btnManageControllers.UseVisualStyleBackColor = false;
            // 
            // btnLanguageSettings
            // 
            this.btnLanguageSettings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnLanguageSettings.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnLanguageSettings.FlatAppearance.BorderSize = 0;
            this.btnLanguageSettings.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLanguageSettings.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnLanguageSettings.ForeColor = System.Drawing.Color.White;
            this.btnLanguageSettings.Location = new System.Drawing.Point(10, 480);
            this.btnLanguageSettings.Name = "btnLanguageSettings";
            this.btnLanguageSettings.Size = new System.Drawing.Size(180, 32);
            this.btnLanguageSettings.TabIndex = 7;
            this.btnLanguageSettings.Text = "🌐  Language";
            this.btnLanguageSettings.UseVisualStyleBackColor = false;
            // 
            // pnlDetails
            // 
            this.pnlDetails.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(19)))), ((int)(((byte)(22)))));
            this.pnlDetails.Controls.Add(this.pbDetailsCover);
            this.pnlDetails.Controls.Add(this.lblDetailsTitle);
            this.pnlDetails.Controls.Add(this.lblDetailsConsole);
            this.pnlDetails.Controls.Add(this.btnPlay);
            this.pnlDetails.Controls.Add(this.btnEditPaths);
            this.pnlDetails.Controls.Add(this.btnDelete);
            this.pnlDetails.Controls.Add(this.lblDetailsStatus);
            this.pnlDetails.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlDetails.Location = new System.Drawing.Point(800, 60);
            this.pnlDetails.Name = "pnlDetails";
            this.pnlDetails.Size = new System.Drawing.Size(300, 540);
            this.pnlDetails.TabIndex = 2;
            // 
            // pbDetailsCover
            // 
            this.pbDetailsCover.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(34)))));
            this.pbDetailsCover.Location = new System.Drawing.Point(40, 15);
            this.pbDetailsCover.Name = "pbDetailsCover";
            this.pbDetailsCover.Size = new System.Drawing.Size(220, 280);
            this.pbDetailsCover.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbDetailsCover.TabIndex = 0;
            this.pbDetailsCover.TabStop = false;
            // 
            // lblDetailsTitle
            // 
            this.lblDetailsTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblDetailsTitle.ForeColor = System.Drawing.Color.White;
            this.lblDetailsTitle.Location = new System.Drawing.Point(15, 305);
            this.lblDetailsTitle.Name = "lblDetailsTitle";
            this.lblDetailsTitle.Size = new System.Drawing.Size(270, 40);
            this.lblDetailsTitle.TabIndex = 1;
            this.lblDetailsTitle.Text = "No Game Selected";
            this.lblDetailsTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblDetailsConsole
            // 
            this.lblDetailsConsole.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblDetailsConsole.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblDetailsConsole.Location = new System.Drawing.Point(15, 345);
            this.lblDetailsConsole.Name = "lblDetailsConsole";
            this.lblDetailsConsole.Size = new System.Drawing.Size(270, 20);
            this.lblDetailsConsole.TabIndex = 2;
            this.lblDetailsConsole.Text = "Choose a game to play";
            this.lblDetailsConsole.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnPlay
            // 
            this.btnPlay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.btnPlay.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPlay.FlatAppearance.BorderSize = 0;
            this.btnPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlay.Font = new System.Drawing.Font("Segoe UI Black", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnPlay.ForeColor = System.Drawing.Color.White;
            this.btnPlay.Location = new System.Drawing.Point(15, 375);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(270, 45);
            this.btnPlay.TabIndex = 3;
            this.btnPlay.Text = "▶   PLAY";
            this.btnPlay.UseVisualStyleBackColor = false;
            // 
            // btnEditPaths
            // 
            this.btnEditPaths.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnEditPaths.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditPaths.FlatAppearance.BorderSize = 0;
            this.btnEditPaths.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditPaths.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnEditPaths.ForeColor = System.Drawing.Color.White;
            this.btnEditPaths.Location = new System.Drawing.Point(15, 430);
            this.btnEditPaths.Name = "btnEditPaths";
            this.btnEditPaths.Size = new System.Drawing.Size(130, 35);
            this.btnEditPaths.TabIndex = 4;
            this.btnEditPaths.Text = "⚙️  Edit Paths";
            this.btnEditPaths.UseVisualStyleBackColor = false;
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(155, 430);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(130, 35);
            this.btnDelete.TabIndex = 5;
            this.btnDelete.Text = "❌  Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            // 
            // lblDetailsStatus
            // 
            this.lblDetailsStatus.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblDetailsStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblDetailsStatus.Location = new System.Drawing.Point(15, 475);
            this.lblDetailsStatus.Name = "lblDetailsStatus";
            this.lblDetailsStatus.Size = new System.Drawing.Size(270, 50);
            this.lblDetailsStatus.TabIndex = 6;
            this.lblDetailsStatus.Text = "Select a game to begin.";
            this.lblDetailsStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pnlLibraryToolbar
            // 
            this.pnlLibraryToolbar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.pnlLibraryToolbar.Controls.Add(this.btnGridView);
            this.pnlLibraryToolbar.Controls.Add(this.btnListView);
            this.pnlLibraryToolbar.Controls.Add(this.lblSortBy);
            this.pnlLibraryToolbar.Controls.Add(this.cbSort);
            this.pnlLibraryToolbar.Controls.Add(this.lblFilterBy);
            this.pnlLibraryToolbar.Controls.Add(this.cbFilter);
            this.pnlLibraryToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlLibraryToolbar.Location = new System.Drawing.Point(200, 60);
            this.pnlLibraryToolbar.Name = "pnlLibraryToolbar";
            this.pnlLibraryToolbar.Size = new System.Drawing.Size(600, 45);
            this.pnlLibraryToolbar.TabIndex = 4;
            // 
            // btnGridView
            // 
            this.btnGridView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(38)))), ((int)(((byte)(38)))), ((int)(((byte)(48)))));
            this.btnGridView.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGridView.FlatAppearance.BorderSize = 0;
            this.btnGridView.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGridView.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnGridView.ForeColor = System.Drawing.Color.White;
            this.btnGridView.Location = new System.Drawing.Point(15, 7);
            this.btnGridView.Name = "btnGridView";
            this.btnGridView.Size = new System.Drawing.Size(80, 30);
            this.btnGridView.TabIndex = 0;
            this.btnGridView.Text = "⊞ Grid";
            this.btnGridView.UseVisualStyleBackColor = false;
            // 
            // btnListView
            // 
            this.btnListView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.btnListView.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnListView.FlatAppearance.BorderSize = 0;
            this.btnListView.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnListView.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F);
            this.btnListView.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.btnListView.Location = new System.Drawing.Point(100, 7);
            this.btnListView.Name = "btnListView";
            this.btnListView.Size = new System.Drawing.Size(80, 30);
            this.btnListView.TabIndex = 1;
            this.btnListView.Text = "☰ List";
            this.btnListView.UseVisualStyleBackColor = false;
            // 
            // lblSortBy
            // 
            this.lblSortBy.AutoSize = true;
            this.lblSortBy.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblSortBy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblSortBy.Location = new System.Drawing.Point(200, 15);
            this.lblSortBy.Name = "lblSortBy";
            this.lblSortBy.Size = new System.Drawing.Size(47, 15);
            this.lblSortBy.TabIndex = 2;
            this.lblSortBy.Text = "Sort By:";
            // 
            // cbSort
            // 
            this.cbSort.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.cbSort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbSort.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbSort.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.cbSort.ForeColor = System.Drawing.Color.White;
            this.cbSort.FormattingEnabled = true;
            this.cbSort.Items.AddRange(new object[] {
            "Title A-Z",
            "Last Played",
            "Most Played",
            "Recently Added"});
            this.cbSort.Location = new System.Drawing.Point(253, 11);
            this.cbSort.Name = "cbSort";
            this.cbSort.Size = new System.Drawing.Size(120, 21);
            this.cbSort.TabIndex = 3;
            // 
            // lblFilterBy
            // 
            this.lblFilterBy.AutoSize = true;
            this.lblFilterBy.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lblFilterBy.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblFilterBy.Location = new System.Drawing.Point(395, 15);
            this.lblFilterBy.Name = "lblFilterBy";
            this.lblFilterBy.Size = new System.Drawing.Size(36, 15);
            this.lblFilterBy.TabIndex = 4;
            this.lblFilterBy.Text = "Filter:";
            // 
            // cbFilter
            // 
            this.cbFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.cbFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFilter.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbFilter.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.cbFilter.ForeColor = System.Drawing.Color.White;
            this.cbFilter.FormattingEnabled = true;
            this.cbFilter.Items.AddRange(new object[] {
            "All Games",
            "Favorites Only",
            "Installed Only",
            "Missing ROMs"});
            this.cbFilter.Location = new System.Drawing.Point(437, 11);
            this.cbFilter.Name = "cbFilter";
            this.cbFilter.Size = new System.Drawing.Size(120, 21);
            this.cbFilter.TabIndex = 5;
            // 
            // flpGamesGrid
            // 
            this.flpGamesGrid.AutoScroll = true;
            this.flpGamesGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            this.flpGamesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flpGamesGrid.Location = new System.Drawing.Point(200, 105);
            this.flpGamesGrid.Name = "flpGamesGrid";
            this.flpGamesGrid.Padding = new System.Windows.Forms.Padding(15);
            this.flpGamesGrid.Size = new System.Drawing.Size(600, 495);
            this.flpGamesGrid.TabIndex = 3;
            // 
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(27)))), ((int)(((byte)(27)))), ((int)(((byte)(31)))));
            this.ClientSize = new System.Drawing.Size(1100, 624);
            this.Controls.Add(this.flpGamesGrid);
            this.Controls.Add(this.pnlLibraryToolbar);
            this.Controls.Add(this.pnlDetails);
            this.Controls.Add(this.pnlSidebar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RetroLauncher";
            this.pnlSidebar.ResumeLayout(false);
            this.pnlSidebar.PerformLayout();
            this.pnlDetails.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbDetailsCover)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.ComponentModel.IContainer components = null!;
        private System.Windows.Forms.Button btnAddGame;
        private System.Windows.Forms.Button btnManageEmulators;
        private System.Windows.Forms.Button btnProfile;
        private System.Windows.Forms.Panel pnlSidebar;
        private System.Windows.Forms.Label lblSidebarHeader;
        private System.Windows.Forms.ListBox lbConsoleFilter;
        private System.Windows.Forms.Panel pnlDetails;
        private System.Windows.Forms.PictureBox pbDetailsCover;
        private System.Windows.Forms.Label lblDetailsTitle;
        private System.Windows.Forms.Label lblDetailsConsole;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnEditPaths;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Label lblDetailsStatus;
        private System.Windows.Forms.FlowLayoutPanel flpGamesGrid;
        private System.Windows.Forms.Panel pnlLibraryToolbar;
        private System.Windows.Forms.Button btnGridView;
        private System.Windows.Forms.Button btnListView;
        private System.Windows.Forms.Label lblSortBy;
        private System.Windows.Forms.ComboBox cbSort;
        private System.Windows.Forms.Label lblFilterBy;
        private System.Windows.Forms.ComboBox cbFilter;
        private System.Windows.Forms.Button btnManageSaves;
        private System.Windows.Forms.Button btnManageScreenshots;
        private System.Windows.Forms.Button btnManageVideos;
        private System.Windows.Forms.Button btnManageControllers;
        private System.Windows.Forms.Button btnAppearance;
        private System.Windows.Forms.Button btnLanguageSettings;
    }
}
