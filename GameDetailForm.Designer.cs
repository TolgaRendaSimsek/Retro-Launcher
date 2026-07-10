namespace RetroLauncher
{
    partial class GameDetailForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.PictureBox pbHero;
        private System.Windows.Forms.PictureBox pbLogo;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.PictureBox pbCover;
        private System.Windows.Forms.Button btnPlay;
        private System.Windows.Forms.Button btnFavorite;
        private System.Windows.Forms.Button btnWatchTrailer;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblPlatform;
        private System.Windows.Forms.Label lblPlaytime;
        private System.Windows.Forms.Label lblMetadata;
        private System.Windows.Forms.RichTextBox rtbDescription;
        private System.Windows.Forms.Label lblScreenshotsHeader;
        private System.Windows.Forms.FlowLayoutPanel flpScreenshots;
        private System.Windows.Forms.Label lblVideosHeader;
        private System.Windows.Forms.FlowLayoutPanel flpVideos;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnEditPaths;
        private System.Windows.Forms.Button btnEditMedia;
        private System.Windows.Forms.Button btnEditMetadata;
        private System.Windows.Forms.Panel pnlAchievements;
        private System.Windows.Forms.Label lblAchievementsTitle;
        private System.Windows.Forms.Label lblProgressCount;
        private System.Windows.Forms.ProgressBar pbProgress;
        private System.Windows.Forms.FlowLayoutPanel flpAchievementsList;
        private System.Windows.Forms.LinkLabel lnkViewAll;

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
            this.pbHero = new System.Windows.Forms.PictureBox();
            this.pbLogo = new System.Windows.Forms.PictureBox();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.pbCover = new System.Windows.Forms.PictureBox();
            this.btnPlay = new System.Windows.Forms.Button();
            this.btnFavorite = new System.Windows.Forms.Button();
            this.btnWatchTrailer = new System.Windows.Forms.Button();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblPlatform = new System.Windows.Forms.Label();
            this.lblPlaytime = new System.Windows.Forms.Label();
            this.lblMetadata = new System.Windows.Forms.Label();
            this.rtbDescription = new System.Windows.Forms.RichTextBox();
            this.lblScreenshotsHeader = new System.Windows.Forms.Label();
            this.flpScreenshots = new System.Windows.Forms.FlowLayoutPanel();
            this.lblVideosHeader = new System.Windows.Forms.Label();
            this.flpVideos = new System.Windows.Forms.FlowLayoutPanel();
            this.btnClose = new System.Windows.Forms.Button();
            this.btnEditPaths = new System.Windows.Forms.Button();
            this.btnEditMedia = new System.Windows.Forms.Button();
            this.btnEditMetadata = new System.Windows.Forms.Button();
            this.pnlAchievements = new System.Windows.Forms.Panel();
            this.lblAchievementsTitle = new System.Windows.Forms.Label();
            this.lblProgressCount = new System.Windows.Forms.Label();
            this.pbProgress = new System.Windows.Forms.ProgressBar();
            this.flpAchievementsList = new System.Windows.Forms.FlowLayoutPanel();
            this.lnkViewAll = new System.Windows.Forms.LinkLabel();
            ((System.ComponentModel.ISupportInitialize)(this.pbHero)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).BeginInit();
            this.pnlLeft.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbCover)).BeginInit();
            this.pnlRight.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbHero
            // 
            this.pbHero.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(22)))));
            this.pbHero.Dock = System.Windows.Forms.DockStyle.Top;
            this.pbHero.Location = new System.Drawing.Point(0, 0);
            this.pbHero.Name = "pbHero";
            this.pbHero.Size = new System.Drawing.Size(820, 200);
            this.pbHero.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pbHero.TabIndex = 0;
            this.pbHero.TabStop = false;
            // 
            // pbLogo
            // 
            this.pbLogo.BackColor = System.Drawing.Color.Transparent;
            this.pbLogo.Location = new System.Drawing.Point(20, 110);
            this.pbLogo.Name = "pbLogo";
            this.pbLogo.Size = new System.Drawing.Size(300, 80);
            this.pbLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbLogo.TabIndex = 1;
            this.pbLogo.TabStop = false;
            // 
            // pnlLeft
            // 
            this.pnlLeft.Controls.Add(this.btnEditMetadata);
            this.pnlLeft.Controls.Add(this.btnEditMedia);
            this.pnlLeft.Controls.Add(this.btnEditPaths);
            this.pnlLeft.Controls.Add(this.btnWatchTrailer);
            this.pnlLeft.Controls.Add(this.btnFavorite);
            this.pnlLeft.Controls.Add(this.btnPlay);
            this.pnlLeft.Controls.Add(this.pbCover);
            this.pnlLeft.Location = new System.Drawing.Point(20, 215);
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Size = new System.Drawing.Size(230, 450);
            this.pnlLeft.TabIndex = 2;
            // 
            // pbCover
            // 
            this.pbCover.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(34)))));
            this.pbCover.Location = new System.Drawing.Point(0, 0);
            this.pbCover.Name = "pbCover";
            this.pbCover.Size = new System.Drawing.Size(220, 250);
            this.pbCover.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbCover.TabIndex = 0;
            this.pbCover.TabStop = false;
            // 
            // btnPlay
            // 
            this.btnPlay.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.btnPlay.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPlay.FlatAppearance.BorderSize = 0;
            this.btnPlay.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlay.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnPlay.ForeColor = System.Drawing.Color.White;
            this.btnPlay.Location = new System.Drawing.Point(0, 260);
            this.btnPlay.Name = "btnPlay";
            this.btnPlay.Size = new System.Drawing.Size(220, 40);
            this.btnPlay.TabIndex = 1;
            this.btnPlay.Text = "▶  PLAY GAME";
            this.btnPlay.UseVisualStyleBackColor = false;
            // 
            // btnFavorite
            // 
            this.btnFavorite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnFavorite.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFavorite.FlatAppearance.BorderSize = 0;
            this.btnFavorite.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFavorite.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F);
            this.btnFavorite.ForeColor = System.Drawing.Color.White;
            this.btnFavorite.Location = new System.Drawing.Point(0, 307);
            this.btnFavorite.Name = "btnFavorite";
            this.btnFavorite.Size = new System.Drawing.Size(105, 30);
            this.btnFavorite.TabIndex = 2;
            this.btnFavorite.Text = "★  Favorite";
            this.btnFavorite.UseVisualStyleBackColor = false;
            // 
            // btnWatchTrailer
            // 
            this.btnWatchTrailer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnWatchTrailer.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnWatchTrailer.FlatAppearance.BorderSize = 0;
            this.btnWatchTrailer.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnWatchTrailer.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F);
            this.btnWatchTrailer.ForeColor = System.Drawing.Color.White;
            this.btnWatchTrailer.Location = new System.Drawing.Point(115, 307);
            this.btnWatchTrailer.Name = "btnWatchTrailer";
            this.btnWatchTrailer.Size = new System.Drawing.Size(105, 30);
            this.btnWatchTrailer.TabIndex = 3;
            this.btnWatchTrailer.Text = "🎬  Trailer";
            this.btnWatchTrailer.UseVisualStyleBackColor = false;
            // 
            // pnlRight
            // 
            this.pnlRight.Controls.Add(this.flpVideos);
            this.pnlRight.Controls.Add(this.lblVideosHeader);
            this.pnlRight.Controls.Add(this.flpScreenshots);
            this.pnlRight.Controls.Add(this.lblScreenshotsHeader);
            this.pnlRight.Controls.Add(this.rtbDescription);
            this.pnlRight.Controls.Add(this.lblMetadata);
            this.pnlRight.Controls.Add(this.lblPlaytime);
            this.pnlRight.Controls.Add(this.lblPlatform);
            this.pnlRight.Controls.Add(this.lblTitle);
            this.pnlRight.Location = new System.Drawing.Point(265, 215);
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Size = new System.Drawing.Size(535, 415);
            this.pnlRight.TabIndex = 3;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(3, 0);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(129, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Game Title";
            // 
            // lblPlatform
            // 
            this.lblPlatform.AutoSize = true;
            this.lblPlatform.Font = new System.Drawing.Font("Segoe UI Symbol", 9.5F, System.Drawing.FontStyle.Italic);
            this.lblPlatform.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.lblPlatform.Location = new System.Drawing.Point(4, 32);
            this.lblPlatform.Name = "lblPlatform";
            this.lblPlatform.Size = new System.Drawing.Size(56, 17);
            this.lblPlatform.TabIndex = 1;
            this.lblPlatform.Text = "Platform";
            // 
            // lblPlaytime
            // 
            this.lblPlaytime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPlaytime.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblPlaytime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblPlaytime.Location = new System.Drawing.Point(265, 2);
            this.lblPlaytime.Name = "lblPlaytime";
            this.lblPlaytime.Size = new System.Drawing.Size(265, 54);
            this.lblPlaytime.TabIndex = 2;
            this.lblPlaytime.Text = "Playtime details";
            this.lblPlaytime.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // lblMetadata
            // 
            this.lblMetadata.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblMetadata.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(223)))));
            this.lblMetadata.Location = new System.Drawing.Point(4, 57);
            this.lblMetadata.Name = "lblMetadata";
            this.lblMetadata.Size = new System.Drawing.Size(528, 48);
            this.lblMetadata.TabIndex = 3;
            this.lblMetadata.Text = "Developer: Unknown  |  Publisher: Unknown\nGenre: Unknown  |  Released: 0000";
            // 
            // rtbDescription
            // 
            this.rtbDescription.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(22)))));
            this.rtbDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbDescription.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.rtbDescription.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(223)))));
            this.rtbDescription.Location = new System.Drawing.Point(4, 115);
            this.rtbDescription.Name = "rtbDescription";
            this.rtbDescription.ReadOnly = true;
            this.rtbDescription.Size = new System.Drawing.Size(528, 90);
            this.rtbDescription.TabIndex = 4;
            this.rtbDescription.Text = "Game Description...";
            // 
            // lblScreenshotsHeader
            // 
            this.lblScreenshotsHeader.AutoSize = true;
            this.lblScreenshotsHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblScreenshotsHeader.ForeColor = System.Drawing.Color.White;
            this.lblScreenshotsHeader.Location = new System.Drawing.Point(3, 218);
            this.lblScreenshotsHeader.Name = "lblScreenshotsHeader";
            this.lblScreenshotsHeader.Size = new System.Drawing.Size(71, 15);
            this.lblScreenshotsHeader.TabIndex = 5;
            this.lblScreenshotsHeader.Text = "Screenshots";
            // 
            // flpScreenshots
            // 
            this.flpScreenshots.AutoScroll = true;
            this.flpScreenshots.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(22)))));
            this.flpScreenshots.Location = new System.Drawing.Point(4, 238);
            this.flpScreenshots.Name = "flpScreenshots";
            this.flpScreenshots.Size = new System.Drawing.Size(528, 95);
            this.flpScreenshots.TabIndex = 6;
            this.flpScreenshots.WrapContents = false;
            // 
            // lblVideosHeader
            // 
            this.lblVideosHeader.AutoSize = true;
            this.lblVideosHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblVideosHeader.ForeColor = System.Drawing.Color.White;
            this.lblVideosHeader.Location = new System.Drawing.Point(3, 338);
            this.lblVideosHeader.Name = "lblVideosHeader";
            this.lblVideosHeader.Size = new System.Drawing.Size(91, 15);
            this.lblVideosHeader.TabIndex = 7;
            this.lblVideosHeader.Text = "Gameplay Clips";
            // 
            // flpVideos
            // 
            this.flpVideos.AutoScroll = true;
            this.flpVideos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(22)))));
            this.flpVideos.Location = new System.Drawing.Point(4, 355);
            this.flpVideos.Name = "flpVideos";
            this.flpVideos.Size = new System.Drawing.Size(528, 55);
            this.flpVideos.TabIndex = 8;
            this.flpVideos.WrapContents = false;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(940, 650);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 30);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Back to Library";
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // btnEditPaths
            // 
            this.btnEditPaths.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnEditPaths.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditPaths.FlatAppearance.BorderSize = 0;
            this.btnEditPaths.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditPaths.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F);
            this.btnEditPaths.ForeColor = System.Drawing.Color.White;
            this.btnEditPaths.Location = new System.Drawing.Point(0, 345);
            this.btnEditPaths.Name = "btnEditPaths";
            this.btnEditPaths.Size = new System.Drawing.Size(220, 30);
            this.btnEditPaths.TabIndex = 4;
            this.btnEditPaths.Text = "⚙️  Configure Launch Path";
            this.btnEditPaths.UseVisualStyleBackColor = false;
            // 
            // btnEditMedia
            // 
            this.btnEditMedia.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnEditMedia.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditMedia.FlatAppearance.BorderSize = 0;
            this.btnEditMedia.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditMedia.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F);
            this.btnEditMedia.ForeColor = System.Drawing.Color.White;
            this.btnEditMedia.Location = new System.Drawing.Point(0, 380);
            this.btnEditMedia.Name = "btnEditMedia";
            this.btnEditMedia.Size = new System.Drawing.Size(220, 30);
            this.btnEditMedia.TabIndex = 5;
            this.btnEditMedia.Text = "🎨  Edit Game Media";
            this.btnEditMedia.UseVisualStyleBackColor = false;
            // 
            // btnEditMetadata
            // 
            this.btnEditMetadata.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnEditMetadata.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnEditMetadata.FlatAppearance.BorderSize = 0;
            this.btnEditMetadata.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEditMetadata.Font = new System.Drawing.Font("Segoe UI Semibold", 8.5F);
            this.btnEditMetadata.ForeColor = System.Drawing.Color.White;
            this.btnEditMetadata.Location = new System.Drawing.Point(0, 415);
            this.btnEditMetadata.Name = "btnEditMetadata";
            this.btnEditMetadata.Size = new System.Drawing.Size(220, 30);
            this.btnEditMetadata.TabIndex = 6;
            this.btnEditMetadata.Text = "📝  Edit Metadata";
            this.btnEditMetadata.UseVisualStyleBackColor = false;
            // 
            // pnlAchievements
            // 
            this.pnlAchievements.Controls.Add(this.lnkViewAll);
            this.pnlAchievements.Controls.Add(this.flpAchievementsList);
            this.pnlAchievements.Controls.Add(this.pbProgress);
            this.pnlAchievements.Controls.Add(this.lblProgressCount);
            this.pnlAchievements.Controls.Add(this.lblAchievementsTitle);
            this.pnlAchievements.Location = new System.Drawing.Point(800, 215);
            this.pnlAchievements.Name = "pnlAchievements";
            this.pnlAchievements.Size = new System.Drawing.Size(260, 415);
            this.pnlAchievements.TabIndex = 4;
            // 
            // lblAchievementsTitle
            // 
            this.lblAchievementsTitle.AutoSize = true;
            this.lblAchievementsTitle.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblAchievementsTitle.ForeColor = System.Drawing.Color.White;
            this.lblAchievementsTitle.Location = new System.Drawing.Point(0, 0);
            this.lblAchievementsTitle.Name = "lblAchievementsTitle";
            this.lblAchievementsTitle.Size = new System.Drawing.Size(121, 20);
            this.lblAchievementsTitle.Text = "ACHIEVEMENTS";
            // 
            // lblProgressCount
            // 
            this.lblProgressCount.AutoSize = true;
            this.lblProgressCount.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.lblProgressCount.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblProgressCount.Location = new System.Drawing.Point(0, 25);
            this.lblProgressCount.Name = "lblProgressCount";
            this.lblProgressCount.Size = new System.Drawing.Size(120, 15);
            this.lblProgressCount.Text = "0 / 0 Unlocked (0%)";
            // 
            // pbProgress
            // 
            this.pbProgress.Location = new System.Drawing.Point(0, 48);
            this.pbProgress.Name = "pbProgress";
            this.pbProgress.Size = new System.Drawing.Size(250, 10);
            this.pbProgress.TabIndex = 2;
            // 
            // flpAchievementsList
            // 
            this.flpAchievementsList.AutoScroll = true;
            this.flpAchievementsList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(22)))));
            this.flpAchievementsList.Location = new System.Drawing.Point(0, 70);
            this.flpAchievementsList.Name = "flpAchievementsList";
            this.flpAchievementsList.Size = new System.Drawing.Size(260, 310);
            this.flpAchievementsList.TabIndex = 3;
            // 
            // lnkViewAll
            // 
            this.lnkViewAll.ActiveLinkColor = System.Drawing.Color.FromArgb(129, 140, 248);
            this.lnkViewAll.BackColor = System.Drawing.Color.Transparent;
            this.lnkViewAll.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lnkViewAll.LinkColor = System.Drawing.Color.FromArgb(99, 102, 241);
            this.lnkViewAll.Location = new System.Drawing.Point(0, 385);
            this.lnkViewAll.Name = "lnkViewAll";
            this.lnkViewAll.Size = new System.Drawing.Size(260, 25);
            this.lnkViewAll.TabIndex = 4;
            this.lnkViewAll.TabStop = true;
            this.lnkViewAll.Text = "View All Achievements";
            this.lnkViewAll.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lnkViewAll.VisitedLinkColor = System.Drawing.Color.FromArgb(99, 102, 241);
            // 
            // GameDetailForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.ClientSize = new System.Drawing.Size(1080, 695);
            this.Controls.Add(this.pnlAchievements);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.pnlRight);
            this.Controls.Add(this.pnlLeft);
            this.Controls.Add(this.pbLogo);
            this.Controls.Add(this.pbHero);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GameDetailForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Game Store Page";
            ((System.ComponentModel.ISupportInitialize)(this.pbHero)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbLogo)).EndInit();
            this.pnlLeft.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pbCover)).EndInit();
            this.pnlRight.ResumeLayout(false);
            this.pnlRight.PerformLayout();
            this.pnlAchievements.ResumeLayout(false);
            this.pnlAchievements.PerformLayout();
            this.ResumeLayout(false);

        }
    }
}
