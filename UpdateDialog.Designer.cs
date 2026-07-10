namespace RetroLauncher
{
    partial class UpdateDialog
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblSubtitle;
        private System.Windows.Forms.Label lblVersionDetails;
        private System.Windows.Forms.RichTextBox rtbChangelog;
        private System.Windows.Forms.Label lblChangelogHeader;
        private System.Windows.Forms.Button btnUpdateNow;
        private System.Windows.Forms.Button btnSkipVersion;
        private System.Windows.Forms.Button btnRemindLater;

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
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblSubtitle = new System.Windows.Forms.Label();
            this.lblVersionDetails = new System.Windows.Forms.Label();
            this.rtbChangelog = new System.Windows.Forms.RichTextBox();
            this.lblChangelogHeader = new System.Windows.Forms.Label();
            this.btnUpdateNow = new System.Windows.Forms.Button();
            this.btnSkipVersion = new System.Windows.Forms.Button();
            this.btnRemindLater = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(20, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(227, 25);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "🚀   Update Available!";
            // 
            // lblSubtitle
            // 
            this.lblSubtitle.AutoSize = true;
            this.lblSubtitle.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblSubtitle.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblSubtitle.Location = new System.Drawing.Point(20, 55);
            this.lblSubtitle.Name = "lblSubtitle";
            this.lblSubtitle.Size = new System.Drawing.Size(262, 17);
            this.lblSubtitle.TabIndex = 1;
            this.lblSubtitle.Text = "A new version of RetroLauncher is available.";
            // 
            // lblVersionDetails
            // 
            this.lblVersionDetails.AutoSize = true;
            this.lblVersionDetails.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.lblVersionDetails.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.lblVersionDetails.Location = new System.Drawing.Point(20, 85);
            this.lblVersionDetails.Name = "lblVersionDetails";
            this.lblVersionDetails.Size = new System.Drawing.Size(193, 19);
            this.lblVersionDetails.TabIndex = 2;
            this.lblVersionDetails.Text = "Current: 1.0.0  ➔  Latest: 1.1.0";
            // 
            // rtbChangelog
            // 
            this.rtbChangelog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(18)))), ((int)(((byte)(22)))));
            this.rtbChangelog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbChangelog.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.rtbChangelog.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(223)))));
            this.rtbChangelog.Location = new System.Drawing.Point(20, 150);
            this.rtbChangelog.Name = "rtbChangelog";
            this.rtbChangelog.ReadOnly = true;
            this.rtbChangelog.Size = new System.Drawing.Size(460, 160);
            this.rtbChangelog.TabIndex = 4;
            this.rtbChangelog.Text = "";
            // 
            // lblChangelogHeader
            // 
            this.lblChangelogHeader.AutoSize = true;
            this.lblChangelogHeader.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblChangelogHeader.ForeColor = System.Drawing.Color.White;
            this.lblChangelogHeader.Location = new System.Drawing.Point(20, 125);
            this.lblChangelogHeader.Name = "lblChangelogHeader";
            this.lblChangelogHeader.Size = new System.Drawing.Size(95, 17);
            this.lblChangelogHeader.TabIndex = 3;
            this.lblChangelogHeader.Text = "Release Notes:";
            // 
            // btnUpdateNow
            // 
            this.btnUpdateNow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.btnUpdateNow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUpdateNow.FlatAppearance.BorderSize = 0;
            this.btnUpdateNow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpdateNow.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnUpdateNow.ForeColor = System.Drawing.Color.White;
            this.btnUpdateNow.Location = new System.Drawing.Point(20, 335);
            this.btnUpdateNow.Name = "btnUpdateNow";
            this.btnUpdateNow.Size = new System.Drawing.Size(140, 35);
            this.btnUpdateNow.TabIndex = 5;
            this.btnUpdateNow.Text = "Update Now";
            this.btnUpdateNow.UseVisualStyleBackColor = false;
            // 
            // btnSkipVersion
            // 
            this.btnSkipVersion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnSkipVersion.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSkipVersion.FlatAppearance.BorderSize = 0;
            this.btnSkipVersion.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSkipVersion.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnSkipVersion.ForeColor = System.Drawing.Color.White;
            this.btnSkipVersion.Location = new System.Drawing.Point(180, 335);
            this.btnSkipVersion.Name = "btnSkipVersion";
            this.btnSkipVersion.Size = new System.Drawing.Size(140, 35);
            this.btnSkipVersion.TabIndex = 6;
            this.btnSkipVersion.Text = "Skip Version";
            this.btnSkipVersion.UseVisualStyleBackColor = false;
            // 
            // btnRemindLater
            // 
            this.btnRemindLater.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnRemindLater.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRemindLater.FlatAppearance.BorderSize = 0;
            this.btnRemindLater.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemindLater.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnRemindLater.ForeColor = System.Drawing.Color.White;
            this.btnRemindLater.Location = new System.Drawing.Point(340, 335);
            this.btnRemindLater.Name = "btnRemindLater";
            this.btnRemindLater.Size = new System.Drawing.Size(140, 35);
            this.btnRemindLater.TabIndex = 7;
            this.btnRemindLater.Text = "Remind Later";
            this.btnRemindLater.UseVisualStyleBackColor = false;
            // 
            // UpdateDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.ClientSize = new System.Drawing.Size(500, 395);
            this.Controls.Add(this.btnRemindLater);
            this.Controls.Add(this.btnSkipVersion);
            this.Controls.Add(this.btnUpdateNow);
            this.Controls.Add(this.lblChangelogHeader);
            this.Controls.Add(this.rtbChangelog);
            this.Controls.Add(this.lblVersionDetails);
            this.Controls.Add(this.lblSubtitle);
            this.Controls.Add(this.lblTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Software Update Available";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
