namespace RetroLauncher.UI.Forms
{
    partial class EmulatorManagerForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox lbEmulators;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.Label lblPath;
        private System.Windows.Forms.TextBox tbPath;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblVersionHeader;
        private System.Windows.Forms.Label lblVersion;
        private System.Windows.Forms.Label lblDefaultHeader;
        private System.Windows.Forms.ComboBox cbDefaultConsole;
        private System.Windows.Forms.Button btnTestLaunch;
        private System.Windows.Forms.Button btnSaveClose;

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
            this.lbEmulators = new System.Windows.Forms.ListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.lblName = new System.Windows.Forms.Label();
            this.tbName = new System.Windows.Forms.TextBox();
            this.lblPath = new System.Windows.Forms.Label();
            this.tbPath = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblVersionHeader = new System.Windows.Forms.Label();
            this.lblVersion = new System.Windows.Forms.Label();
            this.lblDefaultHeader = new System.Windows.Forms.Label();
            this.cbDefaultConsole = new System.Windows.Forms.ComboBox();
            this.btnTestLaunch = new System.Windows.Forms.Button();
            this.btnSaveClose = new System.Windows.Forms.Button();
            this.btnInstallDuckStationApi = new System.Windows.Forms.Button();
            this.pbProgress = new System.Windows.Forms.ProgressBar();
            this.lblStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbEmulators
            // 
            this.lbEmulators.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(28)))), ((int)(((byte)(34)))));
            this.lbEmulators.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lbEmulators.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lbEmulators.ForeColor = System.Drawing.Color.White;
            this.lbEmulators.FormattingEnabled = true;
            this.lbEmulators.ItemHeight = 17;
            this.lbEmulators.Location = new System.Drawing.Point(20, 20);
            this.lbEmulators.Name = "lbEmulators";
            this.lbEmulators.Size = new System.Drawing.Size(220, 323);
            this.lbEmulators.TabIndex = 0;
            // 
            // btnAdd
            // 
            this.btnAdd.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.btnAdd.FlatAppearance.BorderSize = 0;
            this.btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAdd.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnAdd.ForeColor = System.Drawing.Color.White;
            this.btnAdd.Location = new System.Drawing.Point(20, 365);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(105, 35);
            this.btnAdd.TabIndex = 1;
            this.btnAdd.Text = "➕  Add";
            this.btnAdd.UseVisualStyleBackColor = false;
            // 
            // btnRemove
            // 
            this.btnRemove.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.btnRemove.FlatAppearance.BorderSize = 0;
            this.btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRemove.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnRemove.ForeColor = System.Drawing.Color.White;
            this.btnRemove.Location = new System.Drawing.Point(135, 365);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(105, 35);
            this.btnRemove.TabIndex = 2;
            this.btnRemove.Text = "❌  Remove";
            this.btnRemove.UseVisualStyleBackColor = false;
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblName.Location = new System.Drawing.Point(260, 23);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(43, 15);
            this.lblName.TabIndex = 3;
            this.lblName.Text = "Name:";
            // 
            // tbName
            // 
            this.tbName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.tbName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbName.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tbName.ForeColor = System.Drawing.Color.White;
            this.tbName.Location = new System.Drawing.Point(350, 20);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(290, 24);
            this.tbName.TabIndex = 4;
            // 
            // lblPath
            // 
            this.lblPath.AutoSize = true;
            this.lblPath.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblPath.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblPath.Location = new System.Drawing.Point(260, 63);
            this.lblPath.Name = "lblPath";
            this.lblPath.Size = new System.Drawing.Size(35, 15);
            this.lblPath.TabIndex = 5;
            this.lblPath.Text = "Path:";
            // 
            // tbPath
            // 
            this.tbPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.tbPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbPath.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.tbPath.ForeColor = System.Drawing.Color.White;
            this.tbPath.Location = new System.Drawing.Point(350, 60);
            this.tbPath.Name = "tbPath";
            this.tbPath.Size = new System.Drawing.Size(240, 24);
            this.tbPath.TabIndex = 6;
            // 
            // btnBrowse
            // 
            this.btnBrowse.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.btnBrowse.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(75)))), ((int)(((byte)(85)))), ((int)(((byte)(99)))));
            this.btnBrowse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowse.ForeColor = System.Drawing.Color.White;
            this.btnBrowse.Location = new System.Drawing.Point(600, 60);
            this.btnBrowse.Name = "btnBrowse";
            this.btnBrowse.Size = new System.Drawing.Size(40, 24);
            this.btnBrowse.TabIndex = 7;
            this.btnBrowse.Text = "📁";
            this.btnBrowse.UseVisualStyleBackColor = false;
            // 
            // lblVersionHeader
            // 
            this.lblVersionHeader.AutoSize = true;
            this.lblVersionHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblVersionHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblVersionHeader.Location = new System.Drawing.Point(260, 103);
            this.lblVersionHeader.Name = "lblVersionHeader";
            this.lblVersionHeader.Size = new System.Drawing.Size(51, 15);
            this.lblVersionHeader.TabIndex = 8;
            this.lblVersionHeader.Text = "Version:";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(223)))));
            this.lblVersion.Location = new System.Drawing.Point(350, 103);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(91, 17);
            this.lblVersion.TabIndex = 9;
            this.lblVersion.Text = "Not Detected";
            // 
            // lblDefaultHeader
            // 
            this.lblDefaultHeader.AutoSize = true;
            this.lblDefaultHeader.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblDefaultHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblDefaultHeader.Location = new System.Drawing.Point(260, 143);
            this.lblDefaultHeader.Name = "lblDefaultHeader";
            this.lblDefaultHeader.Size = new System.Drawing.Size(167, 15);
            this.lblDefaultHeader.TabIndex = 10;
            this.lblDefaultHeader.Text = "Set as Default Emulator for:";
            // 
            // cbDefaultConsole
            // 
            this.cbDefaultConsole.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(44)))), ((int)(((byte)(44)))), ((int)(((byte)(52)))));
            this.cbDefaultConsole.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDefaultConsole.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbDefaultConsole.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.cbDefaultConsole.ForeColor = System.Drawing.Color.White;
            this.cbDefaultConsole.FormattingEnabled = true;
            this.cbDefaultConsole.Location = new System.Drawing.Point(260, 168);
            this.cbDefaultConsole.Name = "cbDefaultConsole";
            this.cbDefaultConsole.Size = new System.Drawing.Size(380, 25);
            this.cbDefaultConsole.TabIndex = 11;
            // 
            // btnTestLaunch
            // 
            this.btnTestLaunch.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.btnTestLaunch.FlatAppearance.BorderSize = 0;
            this.btnTestLaunch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTestLaunch.Font = new System.Drawing.Font("Segoe UI Black", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnTestLaunch.ForeColor = System.Drawing.Color.White;
            this.btnTestLaunch.Location = new System.Drawing.Point(260, 230);
            this.btnTestLaunch.Name = "btnTestLaunch";
            this.btnTestLaunch.Size = new System.Drawing.Size(380, 40);
            this.btnTestLaunch.TabIndex = 12;
            this.btnTestLaunch.Text = "⚡  TEST LAUNCH EMULATOR";
            this.btnTestLaunch.UseVisualStyleBackColor = false;
            // 
            // btnInstallDuckStationApi
            // 
            this.btnInstallDuckStationApi.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.btnInstallDuckStationApi.FlatAppearance.BorderSize = 0;
            this.btnInstallDuckStationApi.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnInstallDuckStationApi.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnInstallDuckStationApi.ForeColor = System.Drawing.Color.White;
            this.btnInstallDuckStationApi.Location = new System.Drawing.Point(260, 275);
            this.btnInstallDuckStationApi.Name = "btnInstallDuckStationApi";
            this.btnInstallDuckStationApi.Size = new System.Drawing.Size(380, 35);
            this.btnInstallDuckStationApi.TabIndex = 14;
            this.btnInstallDuckStationApi.Text = "⬇️  INSTALL DUCKSTATION (API)";
            this.btnInstallDuckStationApi.UseVisualStyleBackColor = false;
            // 
            // pbProgress
            // 
            this.pbProgress.Location = new System.Drawing.Point(260, 320);
            this.pbProgress.Name = "pbProgress";
            this.pbProgress.Size = new System.Drawing.Size(380, 15);
            this.pbProgress.TabIndex = 15;
            this.pbProgress.Visible = false;
            // 
            // lblStatus
            // 
            this.lblStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblStatus.Location = new System.Drawing.Point(260, 340);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(380, 20);
            this.lblStatus.TabIndex = 16;
            this.lblStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnSaveClose
            // 
            this.btnSaveClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.btnSaveClose.FlatAppearance.BorderSize = 0;
            this.btnSaveClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveClose.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.btnSaveClose.ForeColor = System.Drawing.Color.White;
            this.btnSaveClose.Location = new System.Drawing.Point(480, 365);
            this.btnSaveClose.Name = "btnSaveClose";
            this.btnSaveClose.Size = new System.Drawing.Size(160, 35);
            this.btnSaveClose.TabIndex = 13;
            this.btnSaveClose.Text = "Save & Close";
            this.btnSaveClose.UseVisualStyleBackColor = false;
            // 
            // EmulatorManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.ClientSize = new System.Drawing.Size(664, 421);
            this.Controls.Add(this.lbEmulators);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.lblName);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.lblPath);
            this.Controls.Add(this.tbPath);
            this.Controls.Add(this.btnBrowse);
            this.Controls.Add(this.lblVersionHeader);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.lblDefaultHeader);
            this.Controls.Add(this.cbDefaultConsole);
            this.Controls.Add(this.btnTestLaunch);
            this.Controls.Add(this.btnInstallDuckStationApi);
            this.Controls.Add(this.pbProgress);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.btnSaveClose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Emulator Manager";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Button btnInstallDuckStationApi;
        private System.Windows.Forms.ProgressBar pbProgress;
        private System.Windows.Forms.Label lblStatus;
    }
}
