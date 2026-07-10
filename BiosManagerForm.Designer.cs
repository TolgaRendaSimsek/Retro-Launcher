namespace RetroLauncher
{
    partial class BiosManagerForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox lbConsoles;
        private System.Windows.Forms.Label lblStatusHeader;
        private System.Windows.Forms.Label lblStatusVal;
        private System.Windows.Forms.Label lblPathHeader;
        private System.Windows.Forms.TextBox tbPath;
        private System.Windows.Forms.Button btnLocate;
        private System.Windows.Forms.Button btnImport;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.ProgressBar pbProgress;
        private System.Windows.Forms.Label lblDownloadStatus;

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
            this.lbConsoles = new System.Windows.Forms.ListBox();
            this.lblStatusHeader = new System.Windows.Forms.Label();
            this.lblStatusVal = new System.Windows.Forms.Label();
            this.lblPathHeader = new System.Windows.Forms.Label();
            this.tbPath = new System.Windows.Forms.TextBox();
            this.btnLocate = new System.Windows.Forms.Button();
            this.btnImport = new System.Windows.Forms.Button();
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.pbProgress = new System.Windows.Forms.ProgressBar();
            this.lblDownloadStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbConsoles
            // 
            this.lbConsoles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(34)))));
            this.lbConsoles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lbConsoles.Font = new System.Drawing.Font("Segoe UI Semibold", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lbConsoles.ForeColor = System.Drawing.Color.White;
            this.lbConsoles.FormattingEnabled = true;
            this.lbConsoles.ItemHeight = 17;
            this.lbConsoles.Location = new System.Drawing.Point(20, 20);
            this.lbConsoles.Name = "lbConsoles";
            this.lbConsoles.Size = new System.Drawing.Size(220, 323);
            this.lbConsoles.TabIndex = 0;
            // 
            // lblStatusHeader
            // 
            this.lblStatusHeader.AutoSize = true;
            this.lblStatusHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblStatusHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblStatusHeader.Location = new System.Drawing.Point(260, 23);
            this.lblStatusHeader.Name = "lblStatusHeader";
            this.lblStatusHeader.Size = new System.Drawing.Size(75, 15);
            this.lblStatusHeader.TabIndex = 1;
            this.lblStatusHeader.Text = "BIOS Status:";
            // 
            // lblStatusVal
            // 
            this.lblStatusVal.AutoSize = true;
            this.lblStatusVal.Font = new System.Drawing.Font("Segoe UI Black", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblStatusVal.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.lblStatusVal.Location = new System.Drawing.Point(360, 20);
            this.lblStatusVal.Name = "lblStatusVal";
            this.lblStatusVal.Size = new System.Drawing.Size(68, 19);
            this.lblStatusVal.TabIndex = 2;
            this.lblStatusVal.Text = "MISSING";
            // 
            // lblPathHeader
            // 
            this.lblPathHeader.AutoSize = true;
            this.lblPathHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblPathHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblPathHeader.Location = new System.Drawing.Point(260, 63);
            this.lblPathHeader.Name = "lblPathHeader";
            this.lblPathHeader.Size = new System.Drawing.Size(65, 15);
            this.lblPathHeader.TabIndex = 3;
            this.lblPathHeader.Text = "BIOS Path:";
            // 
            // tbPath
            // 
            this.tbPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.tbPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbPath.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tbPath.ForeColor = System.Drawing.Color.White;
            this.tbPath.Location = new System.Drawing.Point(260, 85);
            this.tbPath.Name = "tbPath";
            this.tbPath.ReadOnly = true;
            this.tbPath.Size = new System.Drawing.Size(460, 24);
            this.tbPath.TabIndex = 4;
            // 
            // btnLocate
            // 
            this.btnLocate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.btnLocate.FlatAppearance.BorderSize = 0;
            this.btnLocate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLocate.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnLocate.ForeColor = System.Drawing.Color.White;
            this.btnLocate.Location = new System.Drawing.Point(260, 130);
            this.btnLocate.Name = "btnLocate";
            this.btnLocate.Size = new System.Drawing.Size(220, 35);
            this.btnLocate.TabIndex = 5;
            this.btnLocate.Text = "🔍  Locate BIOS Manually";
            this.btnLocate.UseVisualStyleBackColor = false;
            // 
            // btnImport
            // 
            this.btnImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.btnImport.FlatAppearance.BorderSize = 0;
            this.btnImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImport.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnImport.ForeColor = System.Drawing.Color.White;
            this.btnImport.Location = new System.Drawing.Point(500, 130);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(220, 35);
            this.btnImport.TabIndex = 6;
            this.btnImport.Text = "📥  Import BIOS File";
            this.btnImport.UseVisualStyleBackColor = false;
            // 
            // btnDownload
            // 
            this.btnDownload.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.btnDownload.FlatAppearance.BorderSize = 0;
            this.btnDownload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDownload.Font = new System.Drawing.Font("Segoe UI Black", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnDownload.ForeColor = System.Drawing.Color.White;
            this.btnDownload.Location = new System.Drawing.Point(260, 180);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(460, 40);
            this.btnDownload.TabIndex = 7;
            this.btnDownload.Text = "🌐  DOWNLOAD FROM MY API";
            this.btnDownload.UseVisualStyleBackColor = false;
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.btnOpenFolder.FlatAppearance.BorderSize = 0;
            this.btnOpenFolder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOpenFolder.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnOpenFolder.ForeColor = System.Drawing.Color.White;
            this.btnOpenFolder.Location = new System.Drawing.Point(260, 235);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(460, 35);
            this.btnOpenFolder.TabIndex = 8;
            this.btnOpenFolder.Text = "📂  Open BIOS Folder in Explorer";
            this.btnOpenFolder.UseVisualStyleBackColor = false;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(580, 365);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(140, 35);
            this.btnClose.TabIndex = 9;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // pbProgress
            // 
            this.pbProgress.Location = new System.Drawing.Point(260, 290);
            this.pbProgress.Name = "pbProgress";
            this.pbProgress.Size = new System.Drawing.Size(460, 15);
            this.pbProgress.TabIndex = 10;
            this.pbProgress.Visible = false;
            // 
            // lblDownloadStatus
            // 
            this.lblDownloadStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblDownloadStatus.Location = new System.Drawing.Point(260, 312);
            this.lblDownloadStatus.Name = "lblDownloadStatus";
            this.lblDownloadStatus.Size = new System.Drawing.Size(460, 20);
            this.lblDownloadStatus.TabIndex = 11;
            this.lblDownloadStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BiosManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.ClientSize = new System.Drawing.Size(744, 421);
            this.Controls.Add(this.lbConsoles);
            this.Controls.Add(this.lblStatusHeader);
            this.Controls.Add(this.lblStatusVal);
            this.Controls.Add(this.lblPathHeader);
            this.Controls.Add(this.tbPath);
            this.Controls.Add(this.btnLocate);
            this.Controls.Add(this.btnImport);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.btnOpenFolder);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.pbProgress);
            this.Controls.Add(this.lblDownloadStatus);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BiosManagerForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "BIOS / Firmware Manager";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
