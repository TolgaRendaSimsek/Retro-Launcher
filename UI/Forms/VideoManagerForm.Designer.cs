namespace RetroLauncher.UI.Forms
{
    partial class VideoManagerForm
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
            this.lblGamesHeader = new System.Windows.Forms.Label();
            this.lbGames = new System.Windows.Forms.ListBox();
            this.lblVideosHeader = new System.Windows.Forms.Label();
            this.lvVideos = new System.Windows.Forms.ListView();
            this.colClipTitle = new System.Windows.Forms.ColumnHeader();
            this.colDuration = new System.Windows.Forms.ColumnHeader();
            this.colCaptureDate = new System.Windows.Forms.ColumnHeader();
            this.lblPreviewHeader = new System.Windows.Forms.Label();
            this.pnlPreviewPlaceholder = new System.Windows.Forms.Panel();
            this.lblPlaceholderText = new System.Windows.Forms.Label();
            this.btnPlayClip = new System.Windows.Forms.Button();
            this.lblTitleLabel = new System.Windows.Forms.Label();
            this.tbTitle = new System.Windows.Forms.TextBox();
            this.btnUpdateCaption = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlPreviewPlaceholder.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblGamesHeader
            // 
            this.lblGamesHeader.AutoSize = true;
            this.lblGamesHeader.Font = new System.Drawing.Font("Segoe UI Black", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblGamesHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblGamesHeader.Location = new System.Drawing.Point(15, 15);
            this.lblGamesHeader.Name = "lblGamesHeader";
            this.lblGamesHeader.Size = new System.Drawing.Size(48, 15);
            this.lblGamesHeader.TabIndex = 0;
            this.lblGamesHeader.Text = "GAMES";
            // 
            // lbGames
            // 
            this.lbGames.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this.lbGames.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbGames.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lbGames.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lbGames.ForeColor = System.Drawing.Color.White;
            this.lbGames.ItemHeight = 28;
            this.lbGames.Location = new System.Drawing.Point(15, 40);
            this.lbGames.Name = "lbGames";
            this.lbGames.Size = new System.Drawing.Size(200, 470);
            this.lbGames.TabIndex = 1;
            // 
            // lblVideosHeader
            // 
            this.lblVideosHeader.AutoSize = true;
            this.lblVideosHeader.Font = new System.Drawing.Font("Segoe UI Black", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblVideosHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblVideosHeader.Location = new System.Drawing.Point(230, 15);
            this.lblVideosHeader.Name = "lblVideosHeader";
            this.lblVideosHeader.Size = new System.Drawing.Size(107, 15);
            this.lblVideosHeader.TabIndex = 2;
            this.lblVideosHeader.Text = "RECORDED CLIPS";
            // 
            // lvVideos
            // 
            this.lvVideos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this.lvVideos.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lvVideos.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colClipTitle,
            this.colDuration,
            this.colCaptureDate});
            this.lvVideos.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lvVideos.ForeColor = System.Drawing.Color.White;
            this.lvVideos.FullRowSelect = true;
            this.lvVideos.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvVideos.Location = new System.Drawing.Point(230, 40);
            this.lvVideos.MultiSelect = false;
            this.lvVideos.Name = "lvVideos";
            this.lvVideos.Size = new System.Drawing.Size(420, 470);
            this.lvVideos.TabIndex = 3;
            this.lvVideos.UseCompatibleStateImageBehavior = false;
            this.lvVideos.View = System.Windows.Forms.View.Details;
            // 
            // colClipTitle
            // 
            this.colClipTitle.Text = "Clip Title / Name";
            this.colClipTitle.Width = 200;
            // 
            // colDuration
            // 
            this.colDuration.Text = "Duration";
            this.colDuration.Width = 75;
            // 
            // colCaptureDate
            // 
            this.colCaptureDate.Text = "Capture Date";
            this.colCaptureDate.Width = 140;
            // 
            // lblPreviewHeader
            // 
            this.lblPreviewHeader.AutoSize = true;
            this.lblPreviewHeader.Font = new System.Drawing.Font("Segoe UI Black", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblPreviewHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblPreviewHeader.Location = new System.Drawing.Point(665, 15);
            this.lblPreviewHeader.Name = "lblPreviewHeader";
            this.lblPreviewHeader.Size = new System.Drawing.Size(107, 15);
            this.lblPreviewHeader.TabIndex = 4;
            this.lblPreviewHeader.Text = "CLIP OPERATIONS";
            // 
            // pnlPreviewPlaceholder
            // 
            this.pnlPreviewPlaceholder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this.pnlPreviewPlaceholder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlPreviewPlaceholder.Controls.Add(this.lblPlaceholderText);
            this.pnlPreviewPlaceholder.Location = new System.Drawing.Point(665, 40);
            this.pnlPreviewPlaceholder.Name = "pnlPreviewPlaceholder";
            this.pnlPreviewPlaceholder.Size = new System.Drawing.Size(255, 170);
            this.pnlPreviewPlaceholder.TabIndex = 5;
            // 
            // lblPlaceholderText
            // 
            this.lblPlaceholderText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lblPlaceholderText.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Italic);
            this.lblPlaceholderText.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblPlaceholderText.Location = new System.Drawing.Point(0, 0);
            this.lblPlaceholderText.Name = "lblPlaceholderText";
            this.lblPlaceholderText.Size = new System.Drawing.Size(253, 168);
            this.lblPlaceholderText.TabIndex = 0;
            this.lblPlaceholderText.Text = "🎥\nSelect a clip from the list";
            this.lblPlaceholderText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnPlayClip
            // 
            this.btnPlayClip.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.btnPlayClip.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPlayClip.FlatAppearance.BorderSize = 0;
            this.btnPlayClip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPlayClip.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold);
            this.btnPlayClip.ForeColor = System.Drawing.Color.White;
            this.btnPlayClip.Location = new System.Drawing.Point(665, 220);
            this.btnPlayClip.Name = "btnPlayClip";
            this.btnPlayClip.Size = new System.Drawing.Size(255, 40);
            this.btnPlayClip.TabIndex = 6;
            this.btnPlayClip.Text = "▶️  Play Video Clip";
            this.btnPlayClip.UseVisualStyleBackColor = false;
            // 
            // lblTitleLabel
            // 
            this.lblTitleLabel.AutoSize = true;
            this.lblTitleLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblTitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblTitleLabel.Location = new System.Drawing.Point(665, 275);
            this.lblTitleLabel.Name = "lblTitleLabel";
            this.lblTitleLabel.Size = new System.Drawing.Size(107, 15);
            this.lblTitleLabel.TabIndex = 7;
            this.lblTitleLabel.Text = "Clip Title/Caption:";
            // 
            // tbTitle
            // 
            this.tbTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.tbTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tbTitle.ForeColor = System.Drawing.Color.White;
            this.tbTitle.Location = new System.Drawing.Point(665, 295);
            this.tbTitle.Name = "tbTitle";
            this.tbTitle.Size = new System.Drawing.Size(255, 23);
            this.tbTitle.TabIndex = 8;
            // 
            // btnUpdateCaption
            // 
            this.btnUpdateCaption.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.btnUpdateCaption.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUpdateCaption.FlatAppearance.BorderSize = 0;
            this.btnUpdateCaption.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpdateCaption.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnUpdateCaption.ForeColor = System.Drawing.Color.White;
            this.btnUpdateCaption.Location = new System.Drawing.Point(665, 330);
            this.btnUpdateCaption.Name = "btnUpdateCaption";
            this.btnUpdateCaption.Size = new System.Drawing.Size(255, 30);
            this.btnUpdateCaption.TabIndex = 9;
            this.btnUpdateCaption.Text = "✏️  Update Caption";
            this.btnUpdateCaption.UseVisualStyleBackColor = false;
            // 
            // btnExport
            // 
            this.btnExport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnExport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExport.FlatAppearance.BorderSize = 0;
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExport.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnExport.ForeColor = System.Drawing.Color.White;
            this.btnExport.Location = new System.Drawing.Point(665, 375);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(120, 35);
            this.btnExport.TabIndex = 10;
            this.btnExport.Text = "📤  Export File";
            this.btnExport.UseVisualStyleBackColor = false;
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(800, 375);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(120, 35);
            this.btnDelete.TabIndex = 11;
            this.btnDelete.Text = "🗑️  Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnOpenFolder.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOpenFolder.FlatAppearance.BorderSize = 0;
            this.btnOpenFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenFolder.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnOpenFolder.ForeColor = System.Drawing.Color.White;
            this.btnOpenFolder.Location = new System.Drawing.Point(665, 425);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(255, 35);
            this.btnOpenFolder.TabIndex = 12;
            this.btnOpenFolder.Text = "📂  Open Folder";
            this.btnOpenFolder.UseVisualStyleBackColor = false;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(800, 475);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 35);
            this.btnClose.TabIndex = 13;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // VideoManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.ClientSize = new System.Drawing.Size(935, 525);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnOpenFolder);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnUpdateCaption);
            this.Controls.Add(this.tbTitle);
            this.Controls.Add(this.lblTitleLabel);
            this.Controls.Add(this.btnPlayClip);
            this.Controls.Add(this.pnlPreviewPlaceholder);
            this.Controls.Add(this.lblPreviewHeader);
            this.Controls.Add(this.lvVideos);
            this.Controls.Add(this.lblVideosHeader);
            this.Controls.Add(this.lbGames);
            this.Controls.Add(this.lblGamesHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "VideoManagerForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Video manager - Gameplay Clip Catalog";
            this.pnlPreviewPlaceholder.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblGamesHeader;
        private System.Windows.Forms.ListBox lbGames;
        private System.Windows.Forms.Label lblVideosHeader;
        private System.Windows.Forms.ListView lvVideos;
        private System.Windows.Forms.ColumnHeader colClipTitle;
        private System.Windows.Forms.ColumnHeader colDuration;
        private System.Windows.Forms.ColumnHeader colCaptureDate;
        private System.Windows.Forms.Label lblPreviewHeader;
        private System.Windows.Forms.Panel pnlPreviewPlaceholder;
        private System.Windows.Forms.Label lblPlaceholderText;
        private System.Windows.Forms.Button btnPlayClip;
        private System.Windows.Forms.Label lblTitleLabel;
        private System.Windows.Forms.TextBox tbTitle;
        private System.Windows.Forms.Button btnUpdateCaption;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Button btnClose;
    }
}
