namespace RetroLauncher.UI.Forms
{
    partial class SaveManagerForm
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
            this.lblActiveSavesHeader = new System.Windows.Forms.Label();
            this.lvActiveSaves = new System.Windows.Forms.ListView();
            this.colFilename = new System.Windows.Forms.ColumnHeader();
            this.colActiveSize = new System.Windows.Forms.ColumnHeader();
            this.colActiveModified = new System.Windows.Forms.ColumnHeader();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.btnBackupNow = new System.Windows.Forms.Button();
            this.lblBackupsHeader = new System.Windows.Forms.Label();
            this.lvBackups = new System.Windows.Forms.ListView();
            this.colBackupName = new System.Windows.Forms.ColumnHeader();
            this.colBackupSize = new System.Windows.Forms.ColumnHeader();
            this.colBackupCreated = new System.Windows.Forms.ColumnHeader();
            this.btnRestore = new System.Windows.Forms.Button();
            this.btnRename = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnExport = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
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
            this.lbGames.Size = new System.Drawing.Size(200, 420);
            this.lbGames.TabIndex = 1;
            // 
            // lblActiveSavesHeader
            // 
            this.lblActiveSavesHeader.AutoSize = true;
            this.lblActiveSavesHeader.Font = new System.Drawing.Font("Segoe UI Black", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblActiveSavesHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblActiveSavesHeader.Location = new System.Drawing.Point(235, 15);
            this.lblActiveSavesHeader.Name = "lblActiveSavesHeader";
            this.lblActiveSavesHeader.Size = new System.Drawing.Size(123, 15);
            this.lblActiveSavesHeader.TabIndex = 2;
            this.lblActiveSavesHeader.Text = "ACTIVE SAVE FILES";
            // 
            // lvActiveSaves
            // 
            this.lvActiveSaves.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this.lvActiveSaves.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lvActiveSaves.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colFilename,
            this.colActiveSize,
            this.colActiveModified});
            this.lvActiveSaves.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lvActiveSaves.ForeColor = System.Drawing.Color.White;
            this.lvActiveSaves.FullRowSelect = true;
            this.lvActiveSaves.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvActiveSaves.Location = new System.Drawing.Point(235, 40);
            this.lvActiveSaves.Name = "lvActiveSaves";
            this.lvActiveSaves.Size = new System.Drawing.Size(300, 390);
            this.lvActiveSaves.TabIndex = 3;
            this.lvActiveSaves.UseCompatibleStateImageBehavior = false;
            this.lvActiveSaves.View = System.Windows.Forms.View.Details;
            // 
            // colFilename
            // 
            this.colFilename.Text = "Filename";
            this.colFilename.Width = 125;
            // 
            // colActiveSize
            // 
            this.colActiveSize.Text = "Size";
            this.colActiveSize.Width = 65;
            // 
            // colActiveModified
            // 
            this.colActiveModified.Text = "Modified";
            this.colActiveModified.Width = 105;
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnOpenFolder.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnOpenFolder.FlatAppearance.BorderSize = 0;
            this.btnOpenFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenFolder.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnOpenFolder.ForeColor = System.Drawing.Color.White;
            this.btnOpenFolder.Location = new System.Drawing.Point(235, 445);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(140, 35);
            this.btnOpenFolder.TabIndex = 4;
            this.btnOpenFolder.Text = "📂  Open Folder";
            this.btnOpenFolder.UseVisualStyleBackColor = false;
            // 
            // btnBackupNow
            // 
            this.btnBackupNow.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.btnBackupNow.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBackupNow.FlatAppearance.BorderSize = 0;
            this.btnBackupNow.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBackupNow.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnBackupNow.ForeColor = System.Drawing.Color.White;
            this.btnBackupNow.Location = new System.Drawing.Point(395, 445);
            this.btnBackupNow.Name = "btnBackupNow";
            this.btnBackupNow.Size = new System.Drawing.Size(140, 35);
            this.btnBackupNow.TabIndex = 5;
            this.btnBackupNow.Text = "💾  Backup Now";
            this.btnBackupNow.UseVisualStyleBackColor = false;
            // 
            // lblBackupsHeader
            // 
            this.lblBackupsHeader.AutoSize = true;
            this.lblBackupsHeader.Font = new System.Drawing.Font("Segoe UI Black", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblBackupsHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblBackupsHeader.Location = new System.Drawing.Point(555, 15);
            this.lblBackupsHeader.Name = "lblBackupsHeader";
            this.lblBackupsHeader.Size = new System.Drawing.Size(111, 15);
            this.lblBackupsHeader.TabIndex = 6;
            this.lblBackupsHeader.Text = "BACKUP HISTORY";
            // 
            // lvBackups
            // 
            this.lvBackups.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this.lvBackups.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lvBackups.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colBackupName,
            this.colBackupSize,
            this.colBackupCreated});
            this.lvBackups.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lvBackups.ForeColor = System.Drawing.Color.White;
            this.lvBackups.FullRowSelect = true;
            this.lvBackups.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvBackups.Location = new System.Drawing.Point(555, 40);
            this.lvBackups.MultiSelect = false;
            this.lvBackups.Name = "lvBackups";
            this.lvBackups.Size = new System.Drawing.Size(335, 305);
            this.lvBackups.TabIndex = 7;
            this.lvBackups.UseCompatibleStateImageBehavior = false;
            this.lvBackups.View = System.Windows.Forms.View.Details;
            // 
            // colBackupName
            // 
            this.colBackupName.Text = "Backup Name";
            this.colBackupName.Width = 145;
            // 
            // colBackupSize
            // 
            this.colBackupSize.Text = "Size";
            this.colBackupSize.Width = 70;
            // 
            // colBackupCreated
            // 
            this.colBackupCreated.Text = "Created Date";
            this.colBackupCreated.Width = 115;
            // 
            // btnRestore
            // 
            this.btnRestore.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.btnRestore.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRestore.FlatAppearance.BorderSize = 0;
            this.btnRestore.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRestore.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnRestore.ForeColor = System.Drawing.Color.White;
            this.btnRestore.Location = new System.Drawing.Point(555, 360);
            this.btnRestore.Name = "btnRestore";
            this.btnRestore.Size = new System.Drawing.Size(100, 35);
            this.btnRestore.TabIndex = 8;
            this.btnRestore.Text = "↩️  Restore";
            this.btnRestore.UseVisualStyleBackColor = false;
            // 
            // btnRename
            // 
            this.btnRename.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnRename.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRename.FlatAppearance.BorderSize = 0;
            this.btnRename.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRename.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnRename.ForeColor = System.Drawing.Color.White;
            this.btnRename.Location = new System.Drawing.Point(670, 360);
            this.btnRename.Name = "btnRename";
            this.btnRename.Size = new System.Drawing.Size(100, 35);
            this.btnRename.TabIndex = 9;
            this.btnRename.Text = "✏️  Rename";
            this.btnRename.UseVisualStyleBackColor = false;
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(785, 360);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(100, 35);
            this.btnDelete.TabIndex = 10;
            this.btnDelete.Text = "🗑️  Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            // 
            // btnImport
            // 
            this.btnImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnImport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnImport.FlatAppearance.BorderSize = 0;
            this.btnImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImport.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnImport.ForeColor = System.Drawing.Color.White;
            this.btnImport.Location = new System.Drawing.Point(555, 410);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(160, 35);
            this.btnImport.TabIndex = 11;
            this.btnImport.Text = "📥  Import ZIP";
            this.btnImport.UseVisualStyleBackColor = false;
            // 
            // btnExport
            // 
            this.btnExport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnExport.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExport.FlatAppearance.BorderSize = 0;
            this.btnExport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExport.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnExport.ForeColor = System.Drawing.Color.White;
            this.btnExport.Location = new System.Drawing.Point(730, 410);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(160, 35);
            this.btnExport.TabIndex = 12;
            this.btnExport.Text = "📤  Export ZIP";
            this.btnExport.UseVisualStyleBackColor = false;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(785, 465);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.TabIndex = 13;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // SaveManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.ClientSize = new System.Drawing.Size(905, 510);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnRename);
            this.Controls.Add(this.btnRestore);
            this.Controls.Add(this.lvBackups);
            this.Controls.Add(this.lblBackupsHeader);
            this.Controls.Add(this.btnBackupNow);
            this.Controls.Add(this.btnOpenFolder);
            this.Controls.Add(this.lvActiveSaves);
            this.Controls.Add(this.lblActiveSavesHeader);
            this.Controls.Add(this.lbGames);
            this.Controls.Add(this.lblGamesHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SaveManagerForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Save SaveManager - Backup & Restore";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblGamesHeader;
        private System.Windows.Forms.ListBox lbGames;
        private System.Windows.Forms.Label lblActiveSavesHeader;
        private System.Windows.Forms.ListView lvActiveSaves;
        private System.Windows.Forms.ColumnHeader colFilename;
        private System.Windows.Forms.ColumnHeader colActiveSize;
        private System.Windows.Forms.ColumnHeader colActiveModified;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Button btnBackupNow;
        private System.Windows.Forms.Label lblBackupsHeader;
        private System.Windows.Forms.ListView lvBackups;
        private System.Windows.Forms.ColumnHeader colBackupName;
        private System.Windows.Forms.ColumnHeader colBackupSize;
        private System.Windows.Forms.ColumnHeader colBackupCreated;
        private System.Windows.Forms.Button btnRestore;
        private System.Windows.Forms.Button btnRename;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnExport;
        private System.Windows.Forms.Button btnClose;
    }
}
