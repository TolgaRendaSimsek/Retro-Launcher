namespace RetroLauncher
{
    partial class ScreenshotManagerForm
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
            this.lblThumbnailsHeader = new System.Windows.Forms.Label();
            this.flpThumbnails = new System.Windows.Forms.FlowLayoutPanel();
            this.lblPreviewHeader = new System.Windows.Forms.Label();
            this.pbLargePreview = new System.Windows.Forms.PictureBox();
            this.lblTitleLabel = new System.Windows.Forms.Label();
            this.tbTitle = new System.Windows.Forms.TextBox();
            this.btnUpdateCaption = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.pbLargePreview)).BeginInit();
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
            // lblThumbnailsHeader
            // 
            this.lblThumbnailsHeader.AutoSize = true;
            this.lblThumbnailsHeader.Font = new System.Drawing.Font("Segoe UI Black", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblThumbnailsHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblThumbnailsHeader.Location = new System.Drawing.Point(230, 15);
            this.lblThumbnailsHeader.Name = "lblThumbnailsHeader";
            this.lblThumbnailsHeader.Size = new System.Drawing.Size(142, 15);
            this.lblThumbnailsHeader.TabIndex = 2;
            this.lblThumbnailsHeader.Text = "SCREENSHOT GALLERY";
            // 
            // flpThumbnails
            // 
            this.flpThumbnails.AutoScroll = true;
            this.flpThumbnails.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this.flpThumbnails.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flpThumbnails.Location = new System.Drawing.Point(230, 40);
            this.flpThumbnails.Name = "flpThumbnails";
            this.flpThumbnails.Padding = new System.Windows.Forms.Padding(10);
            this.flpThumbnails.Size = new System.Drawing.Size(420, 470);
            this.flpThumbnails.TabIndex = 3;
            // 
            // lblPreviewHeader
            // 
            this.lblPreviewHeader.AutoSize = true;
            this.lblPreviewHeader.Font = new System.Drawing.Font("Segoe UI Black", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblPreviewHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblPreviewHeader.Location = new System.Drawing.Point(665, 15);
            this.lblPreviewHeader.Name = "lblPreviewHeader";
            this.lblPreviewHeader.Size = new System.Drawing.Size(142, 15);
            this.lblPreviewHeader.TabIndex = 4;
            this.lblPreviewHeader.Text = "SCREENSHOT PREVIEW";
            // 
            // pbLargePreview
            // 
            this.pbLargePreview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this.pbLargePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pbLargePreview.Location = new System.Drawing.Point(665, 40);
            this.pbLargePreview.Name = "pbLargePreview";
            this.pbLargePreview.Size = new System.Drawing.Size(255, 170);
            this.pbLargePreview.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbLargePreview.TabIndex = 5;
            this.pbLargePreview.TabStop = false;
            // 
            // lblTitleLabel
            // 
            this.lblTitleLabel.AutoSize = true;
            this.lblTitleLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblTitleLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblTitleLabel.Location = new System.Drawing.Point(665, 220);
            this.lblTitleLabel.Name = "lblTitleLabel";
            this.lblTitleLabel.Size = new System.Drawing.Size(83, 15);
            this.lblTitleLabel.TabIndex = 6;
            this.lblTitleLabel.Text = "Caption/Title:";
            // 
            // tbTitle
            // 
            this.tbTitle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.tbTitle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbTitle.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tbTitle.ForeColor = System.Drawing.Color.White;
            this.tbTitle.Location = new System.Drawing.Point(665, 240);
            this.tbTitle.Name = "tbTitle";
            this.tbTitle.Size = new System.Drawing.Size(255, 23);
            this.tbTitle.TabIndex = 7;
            // 
            // btnUpdateCaption
            // 
            this.btnUpdateCaption.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.btnUpdateCaption.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnUpdateCaption.FlatAppearance.BorderSize = 0;
            this.btnUpdateCaption.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUpdateCaption.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnUpdateCaption.ForeColor = System.Drawing.Color.White;
            this.btnUpdateCaption.Location = new System.Drawing.Point(665, 275);
            this.btnUpdateCaption.Name = "btnUpdateCaption";
            this.btnUpdateCaption.Size = new System.Drawing.Size(255, 30);
            this.btnUpdateCaption.TabIndex = 8;
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
            this.btnExport.Location = new System.Drawing.Point(665, 320);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(120, 35);
            this.btnExport.TabIndex = 9;
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
            this.btnDelete.Location = new System.Drawing.Point(800, 320);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(120, 35);
            this.btnDelete.TabIndex = 10;
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
            this.btnOpenFolder.Location = new System.Drawing.Point(665, 370);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(255, 35);
            this.btnOpenFolder.TabIndex = 11;
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
            this.btnClose.TabIndex = 12;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // ScreenshotManagerForm
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
            this.Controls.Add(this.pbLargePreview);
            this.Controls.Add(this.lblPreviewHeader);
            this.Controls.Add(this.flpThumbnails);
            this.Controls.Add(this.lblThumbnailsHeader);
            this.Controls.Add(this.lbGames);
            this.Controls.Add(this.lblGamesHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ScreenshotManagerForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Screenshot Manager - Gallery & Captures";
            ((System.ComponentModel.ISupportInitialize)(this.pbLargePreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblGamesHeader;
        private System.Windows.Forms.ListBox lbGames;
        private System.Windows.Forms.Label lblThumbnailsHeader;
        private System.Windows.Forms.FlowLayoutPanel flpThumbnails;
        private System.Windows.Forms.Label lblPreviewHeader;
        private System.Windows.Forms.PictureBox pbLargePreview;
        private System.Windows.Forms.Label lblTitleLabel;
        private System.Windows.Forms.TextBox tbTitle;
        private System.Windows.Forms.Button btnUpdateCaption;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Button btnClose;
    }
}
