namespace RetroLauncher.UI.Forms
{
    partial class EditProfileForm
    {
        private System.ComponentModel.IContainer components = null;

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
            this.lblUsername = new System.Windows.Forms.Label();
            this.tbUsername = new System.Windows.Forms.TextBox();
            this.lblBio = new System.Windows.Forms.Label();
            this.tbBio = new System.Windows.Forms.TextBox();
            this.lblFavoriteConsole = new System.Windows.Forms.Label();
            this.cbFavoriteConsole = new System.Windows.Forms.ComboBox();
            this.lblThemeColor = new System.Windows.Forms.Label();
            this.cbThemeColor = new System.Windows.Forms.ComboBox();
            this.lblAvatar = new System.Windows.Forms.Label();
            this.pbAvatarPreview = new System.Windows.Forms.PictureBox();
            this.btnBrowseAvatar = new System.Windows.Forms.Button();
            this.lblBanner = new System.Windows.Forms.Label();
            this.pbBannerPreview = new System.Windows.Forms.PictureBox();
            this.btnBrowseBanner = new System.Windows.Forms.Button();
            this.lblFavoriteGames = new System.Windows.Forms.Label();
            this.clbFavoriteGames = new System.Windows.Forms.CheckedListBox();
            this.btnSave = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatarPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBannerPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // lblUsername
            // 
            this.lblUsername.AutoSize = true;
            this.lblUsername.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblUsername.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblUsername.Location = new System.Drawing.Point(20, 15);
            this.lblUsername.Name = "lblUsername";
            this.lblUsername.Size = new System.Drawing.Size(63, 15);
            this.lblUsername.TabIndex = 0;
            this.lblUsername.Text = "Username:";
            // 
            // tbUsername
            // 
            this.tbUsername.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.tbUsername.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbUsername.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.tbUsername.ForeColor = System.Drawing.Color.White;
            this.tbUsername.Location = new System.Drawing.Point(20, 35);
            this.tbUsername.Name = "tbUsername";
            this.tbUsername.Size = new System.Drawing.Size(220, 24);
            this.tbUsername.TabIndex = 1;
            // 
            // lblBio
            // 
            this.lblBio.AutoSize = true;
            this.lblBio.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblBio.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblBio.Location = new System.Drawing.Point(20, 75);
            this.lblBio.Name = "lblBio";
            this.lblBio.Size = new System.Drawing.Size(62, 15);
            this.lblBio.TabIndex = 2;
            this.lblBio.Text = "Bio / About:";
            // 
            // tbBio
            // 
            this.tbBio.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.tbBio.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbBio.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tbBio.ForeColor = System.Drawing.Color.White;
            this.tbBio.Location = new System.Drawing.Point(20, 95);
            this.tbBio.Multiline = true;
            this.tbBio.Name = "tbBio";
            this.tbBio.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbBio.Size = new System.Drawing.Size(220, 60);
            this.tbBio.TabIndex = 3;
            // 
            // lblFavoriteConsole
            // 
            this.lblFavoriteConsole.AutoSize = true;
            this.lblFavoriteConsole.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblFavoriteConsole.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblFavoriteConsole.Location = new System.Drawing.Point(20, 170);
            this.lblFavoriteConsole.Name = "lblFavoriteConsole";
            this.lblFavoriteConsole.Size = new System.Drawing.Size(98, 15);
            this.lblFavoriteConsole.TabIndex = 4;
            this.lblFavoriteConsole.Text = "Favorite Console:";
            // 
            // cbFavoriteConsole
            // 
            this.cbFavoriteConsole.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.cbFavoriteConsole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFavoriteConsole.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbFavoriteConsole.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbFavoriteConsole.ForeColor = System.Drawing.Color.White;
            this.cbFavoriteConsole.FormattingEnabled = true;
            this.cbFavoriteConsole.Location = new System.Drawing.Point(20, 190);
            this.cbFavoriteConsole.Name = "cbFavoriteConsole";
            this.cbFavoriteConsole.Size = new System.Drawing.Size(220, 23);
            this.cbFavoriteConsole.TabIndex = 5;
            // 
            // lblThemeColor
            // 
            this.lblThemeColor.AutoSize = true;
            this.lblThemeColor.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblThemeColor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblThemeColor.Location = new System.Drawing.Point(20, 230);
            this.lblThemeColor.Name = "lblThemeColor";
            this.lblThemeColor.Size = new System.Drawing.Size(78, 15);
            this.lblThemeColor.TabIndex = 6;
            this.lblThemeColor.Text = "Theme Color:";
            // 
            // cbThemeColor
            // 
            this.cbThemeColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.cbThemeColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbThemeColor.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbThemeColor.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbThemeColor.ForeColor = System.Drawing.Color.White;
            this.cbThemeColor.FormattingEnabled = true;
            this.cbThemeColor.Location = new System.Drawing.Point(20, 250);
            this.cbThemeColor.Name = "cbThemeColor";
            this.cbThemeColor.Size = new System.Drawing.Size(220, 23);
            this.cbThemeColor.TabIndex = 7;
            // 
            // lblAvatar
            // 
            this.lblAvatar.AutoSize = true;
            this.lblAvatar.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblAvatar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblAvatar.Location = new System.Drawing.Point(270, 15);
            this.lblAvatar.Name = "lblAvatar";
            this.lblAvatar.Size = new System.Drawing.Size(81, 15);
            this.lblAvatar.TabIndex = 8;
            this.lblAvatar.Text = "Profile Avatar:";
            // 
            // pbAvatarPreview
            // 
            this.pbAvatarPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(19)))), ((int)(((byte)(22)))));
            this.pbAvatarPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbAvatarPreview.Location = new System.Drawing.Point(270, 35);
            this.pbAvatarPreview.Name = "pbAvatarPreview";
            this.pbAvatarPreview.Size = new System.Drawing.Size(64, 64);
            this.pbAvatarPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbAvatarPreview.TabIndex = 9;
            this.pbAvatarPreview.TabStop = false;
            // 
            // btnBrowseAvatar
            // 
            this.btnBrowseAvatar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnBrowseAvatar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowseAvatar.FlatAppearance.BorderSize = 0;
            this.btnBrowseAvatar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseAvatar.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold);
            this.btnBrowseAvatar.ForeColor = System.Drawing.Color.White;
            this.btnBrowseAvatar.Location = new System.Drawing.Point(345, 35);
            this.btnBrowseAvatar.Name = "btnBrowseAvatar";
            this.btnBrowseAvatar.Size = new System.Drawing.Size(135, 25);
            this.btnBrowseAvatar.TabIndex = 10;
            this.btnBrowseAvatar.Text = "Choose Avatar...";
            this.btnBrowseAvatar.UseVisualStyleBackColor = false;
            // 
            // lblBanner
            // 
            this.lblBanner.AutoSize = true;
            this.lblBanner.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblBanner.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblBanner.Location = new System.Drawing.Point(270, 115);
            this.lblBanner.Name = "lblBanner";
            this.lblBanner.Size = new System.Drawing.Size(86, 15);
            this.lblBanner.TabIndex = 11;
            this.lblBanner.Text = "Profile Banner:";
            // 
            // pbBannerPreview
            // 
            this.pbBannerPreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(19)))), ((int)(((byte)(19)))), ((int)(((byte)(22)))));
            this.pbBannerPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbBannerPreview.Location = new System.Drawing.Point(270, 135);
            this.pbBannerPreview.Name = "pbBannerPreview";
            this.pbBannerPreview.Size = new System.Drawing.Size(210, 60);
            this.pbBannerPreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbBannerPreview.TabIndex = 12;
            this.pbBannerPreview.TabStop = false;
            // 
            // btnBrowseBanner
            // 
            this.btnBrowseBanner.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnBrowseBanner.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowseBanner.FlatAppearance.BorderSize = 0;
            this.btnBrowseBanner.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseBanner.Font = new System.Drawing.Font("Segoe UI Semibold", 8F, System.Drawing.FontStyle.Bold);
            this.btnBrowseBanner.ForeColor = System.Drawing.Color.White;
            this.btnBrowseBanner.Location = new System.Drawing.Point(270, 200);
            this.btnBrowseBanner.Name = "btnBrowseBanner";
            this.btnBrowseBanner.Size = new System.Drawing.Size(210, 25);
            this.btnBrowseBanner.TabIndex = 13;
            this.btnBrowseBanner.Text = "Choose Banner...";
            this.btnBrowseBanner.UseVisualStyleBackColor = false;
            // 
            // lblFavoriteGames
            // 
            this.lblFavoriteGames.AutoSize = true;
            this.lblFavoriteGames.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblFavoriteGames.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblFavoriteGames.Location = new System.Drawing.Point(20, 295);
            this.lblFavoriteGames.Name = "lblFavoriteGames";
            this.lblFavoriteGames.Size = new System.Drawing.Size(161, 15);
            this.lblFavoriteGames.TabIndex = 14;
            this.lblFavoriteGames.Text = "Select Favorite Games (Max 5):";
            // 
            // clbFavoriteGames
            // 
            this.clbFavoriteGames.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.clbFavoriteGames.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.clbFavoriteGames.CheckOnClick = true;
            this.clbFavoriteGames.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.clbFavoriteGames.ForeColor = System.Drawing.Color.White;
            this.clbFavoriteGames.FormattingEnabled = true;
            this.clbFavoriteGames.Location = new System.Drawing.Point(20, 315);
            this.clbFavoriteGames.Name = "clbFavoriteGames";
            this.clbFavoriteGames.Size = new System.Drawing.Size(460, 110);
            this.clbFavoriteGames.TabIndex = 15;
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.btnSave.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSave.ForeColor = System.Drawing.Color.White;
            this.btnSave.Location = new System.Drawing.Point(20, 450);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(220, 35);
            this.btnSave.TabIndex = 16;
            this.btnSave.Text = "💾 Save Profile changes";
            this.btnSave.UseVisualStyleBackColor = false;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.FlatAppearance.BorderSize = 0;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancel.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCancel.ForeColor = System.Drawing.Color.White;
            this.btnCancel.Location = new System.Drawing.Point(260, 450);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(220, 35);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = false;
            // 
            // EditProfileForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.ClientSize = new System.Drawing.Size(500, 505);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.clbFavoriteGames);
            this.Controls.Add(this.lblFavoriteGames);
            this.Controls.Add(this.btnBrowseBanner);
            this.Controls.Add(this.pbBannerPreview);
            this.Controls.Add(this.lblBanner);
            this.Controls.Add(this.btnBrowseAvatar);
            this.Controls.Add(this.pbAvatarPreview);
            this.Controls.Add(this.lblAvatar);
            this.Controls.Add(this.cbThemeColor);
            this.Controls.Add(this.lblThemeColor);
            this.Controls.Add(this.cbFavoriteConsole);
            this.Controls.Add(this.lblFavoriteConsole);
            this.Controls.Add(this.tbBio);
            this.Controls.Add(this.lblBio);
            this.Controls.Add(this.tbUsername);
            this.Controls.Add(this.lblUsername);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EditProfileForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit User Profile - RetroLauncher";
            ((System.ComponentModel.ISupportInitialize)(this.pbAvatarPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pbBannerPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblUsername;
        private System.Windows.Forms.TextBox tbUsername;
        private System.Windows.Forms.Label lblBio;
        private System.Windows.Forms.TextBox tbBio;
        private System.Windows.Forms.Label lblFavoriteConsole;
        private System.Windows.Forms.ComboBox cbFavoriteConsole;
        private System.Windows.Forms.Label lblThemeColor;
        private System.Windows.Forms.ComboBox cbThemeColor;
        private System.Windows.Forms.Label lblAvatar;
        private System.Windows.Forms.PictureBox pbAvatarPreview;
        private System.Windows.Forms.Button btnBrowseAvatar;
        private System.Windows.Forms.Label lblBanner;
        private System.Windows.Forms.PictureBox pbBannerPreview;
        private System.Windows.Forms.Button btnBrowseBanner;
        private System.Windows.Forms.Label lblFavoriteGames;
        private System.Windows.Forms.CheckedListBox clbFavoriteGames;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnCancel;
    }
}
