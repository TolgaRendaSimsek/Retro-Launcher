namespace RetroLauncher
{
    partial class ControllerManagerForm
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
            this.lblDevicesHeader = new System.Windows.Forms.Label();
            this.lvDevices = new System.Windows.Forms.ListView();
            this.colPortId = new System.Windows.Forms.ColumnHeader();
            this.colControllerName = new System.Windows.Forms.ColumnHeader();
            this.colDeviceType = new System.Windows.Forms.ColumnHeader();
            this.colStatus = new System.Windows.Forms.ColumnHeader();
            this.btnScan = new System.Windows.Forms.Button();
            this.lblProfilesHeader = new System.Windows.Forms.Label();
            this.lbProfiles = new System.Windows.Forms.ListBox();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnDelete = new System.Windows.Forms.Button();
            this.btnTestInput = new System.Windows.Forms.Button();
            this.lblConfigHeader = new System.Windows.Forms.Label();
            this.pnlConfig = new System.Windows.Forms.Panel();
            this.btnSaveProfile = new System.Windows.Forms.Button();
            this.flpMappings = new System.Windows.Forms.FlowLayoutPanel();
            this.lblMappingLabel = new System.Windows.Forms.Label();
            this.cbTargetGame = new System.Windows.Forms.ComboBox();
            this.lblGameLabel = new System.Windows.Forms.Label();
            this.cbTargetEmulator = new System.Windows.Forms.ComboBox();
            this.lblEmulatorLabel = new System.Windows.Forms.Label();
            this.cbControllerType = new System.Windows.Forms.ComboBox();
            this.lblControllerTypeLabel = new System.Windows.Forms.Label();
            this.tbProfileName = new System.Windows.Forms.TextBox();
            this.lblProfileNameLabel = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.pnlConfig.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDevicesHeader
            // 
            this.lblDevicesHeader.AutoSize = true;
            this.lblDevicesHeader.Font = new System.Drawing.Font("Segoe UI Black", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblDevicesHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblDevicesHeader.Location = new System.Drawing.Point(15, 15);
            this.lblDevicesHeader.Name = "lblDevicesHeader";
            this.lblDevicesHeader.Size = new System.Drawing.Size(161, 15);
            this.lblDevicesHeader.TabIndex = 0;
            this.lblDevicesHeader.Text = "CONNECTED CONTROLLERS";
            // 
            // lvDevices
            // 
            this.lvDevices.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this.lvDevices.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lvDevices.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colPortId,
            this.colControllerName,
            this.colDeviceType,
            this.colStatus});
            this.lvDevices.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.lvDevices.ForeColor = System.Drawing.Color.White;
            this.lvDevices.FullRowSelect = true;
            this.lvDevices.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lvDevices.Location = new System.Drawing.Point(15, 40);
            this.lvDevices.MultiSelect = false;
            this.lvDevices.Name = "lvDevices";
            this.lvDevices.Size = new System.Drawing.Size(750, 110);
            this.lvDevices.TabIndex = 1;
            this.lvDevices.UseCompatibleStateImageBehavior = false;
            this.lvDevices.View = System.Windows.Forms.View.Details;
            // 
            // colPortId
            // 
            this.colPortId.Text = "Port/ID";
            // 
            // colControllerName
            // 
            this.colControllerName.Text = "Controller Name / Product Name";
            this.colControllerName.Width = 430;
            // 
            // colDeviceType
            // 
            this.colDeviceType.Text = "Controller Type";
            this.colDeviceType.Width = 140;
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status";
            this.colStatus.Width = 100;
            // 
            // btnScan
            // 
            this.btnScan.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.btnScan.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnScan.FlatAppearance.BorderSize = 0;
            this.btnScan.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnScan.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnScan.ForeColor = System.Drawing.Color.White;
            this.btnScan.Location = new System.Drawing.Point(780, 40);
            this.btnScan.Name = "btnScan";
            this.btnScan.Size = new System.Drawing.Size(120, 35);
            this.btnScan.TabIndex = 2;
            this.btnScan.Text = "🔄  Scan Devices";
            this.btnScan.UseVisualStyleBackColor = false;
            // 
            // lblProfilesHeader
            // 
            this.lblProfilesHeader.AutoSize = true;
            this.lblProfilesHeader.Font = new System.Drawing.Font("Segoe UI Black", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblProfilesHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblProfilesHeader.Location = new System.Drawing.Point(15, 165);
            this.lblProfilesHeader.Name = "lblProfilesHeader";
            this.lblProfilesHeader.Size = new System.Drawing.Size(63, 15);
            this.lblProfilesHeader.TabIndex = 3;
            this.lblProfilesHeader.Text = "PROFILES";
            // 
            // lbProfiles
            // 
            this.lbProfiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this.lbProfiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lbProfiles.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lbProfiles.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lbProfiles.ForeColor = System.Drawing.Color.White;
            this.lbProfiles.ItemHeight = 28;
            this.lbProfiles.Location = new System.Drawing.Point(15, 190);
            this.lbProfiles.Name = "lbProfiles";
            this.lbProfiles.Size = new System.Drawing.Size(260, 275);
            this.lbProfiles.TabIndex = 4;
            // 
            // btnCreate
            // 
            this.btnCreate.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(16)))), ((int)(((byte)(185)))), ((int)(((byte)(129)))));
            this.btnCreate.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCreate.FlatAppearance.BorderSize = 0;
            this.btnCreate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCreate.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnCreate.ForeColor = System.Drawing.Color.White;
            this.btnCreate.Location = new System.Drawing.Point(15, 475);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(120, 35);
            this.btnCreate.TabIndex = 5;
            this.btnCreate.Text = "➕  Create Profile";
            this.btnCreate.UseVisualStyleBackColor = false;
            // 
            // btnDelete
            // 
            this.btnDelete.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(68)))), ((int)(((byte)(68)))));
            this.btnDelete.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDelete.FlatAppearance.BorderSize = 0;
            this.btnDelete.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDelete.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnDelete.ForeColor = System.Drawing.Color.White;
            this.btnDelete.Location = new System.Drawing.Point(155, 475);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(120, 35);
            this.btnDelete.TabIndex = 6;
            this.btnDelete.Text = "🗑️  Delete";
            this.btnDelete.UseVisualStyleBackColor = false;
            // 
            // btnTestInput
            // 
            this.btnTestInput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnTestInput.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTestInput.FlatAppearance.BorderSize = 0;
            this.btnTestInput.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTestInput.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnTestInput.ForeColor = System.Drawing.Color.White;
            this.btnTestInput.Location = new System.Drawing.Point(15, 515);
            this.btnTestInput.Name = "btnTestInput";
            this.btnTestInput.Size = new System.Drawing.Size(260, 35);
            this.btnTestInput.TabIndex = 11;
            this.btnTestInput.Text = "🎮  Test Input";
            this.btnTestInput.UseVisualStyleBackColor = false;
            // 
            // lblConfigHeader
            // 
            this.lblConfigHeader.AutoSize = true;
            this.lblConfigHeader.Font = new System.Drawing.Font("Segoe UI Black", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblConfigHeader.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(107)))), ((int)(((byte)(114)))), ((int)(((byte)(128)))));
            this.lblConfigHeader.Location = new System.Drawing.Point(295, 165);
            this.lblConfigHeader.Name = "lblConfigHeader";
            this.lblConfigHeader.Size = new System.Drawing.Size(201, 15);
            this.lblConfigHeader.TabIndex = 7;
            this.lblConfigHeader.Text = "PROFILE MAPPING & ASSIGNMENT";
            // 
            // pnlConfig
            // 
            this.pnlConfig.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(31)))), ((int)(((byte)(35)))));
            this.pnlConfig.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlConfig.Controls.Add(this.btnSaveProfile);
            this.pnlConfig.Controls.Add(this.flpMappings);
            this.pnlConfig.Controls.Add(this.lblMappingLabel);
            this.pnlConfig.Controls.Add(this.cbTargetGame);
            this.pnlConfig.Controls.Add(this.lblGameLabel);
            this.pnlConfig.Controls.Add(this.cbTargetEmulator);
            this.pnlConfig.Controls.Add(this.lblEmulatorLabel);
            this.pnlConfig.Controls.Add(this.cbControllerType);
            this.pnlConfig.Controls.Add(this.lblControllerTypeLabel);
            this.pnlConfig.Controls.Add(this.tbProfileName);
            this.pnlConfig.Controls.Add(this.lblProfileNameLabel);
            this.pnlConfig.Location = new System.Drawing.Point(295, 190);
            this.pnlConfig.Name = "pnlConfig";
            this.pnlConfig.Size = new System.Drawing.Size(605, 320);
            this.pnlConfig.TabIndex = 8;
            // 
            // btnSaveProfile
            // 
            this.btnSaveProfile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.btnSaveProfile.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnSaveProfile.FlatAppearance.BorderSize = 0;
            this.btnSaveProfile.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSaveProfile.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnSaveProfile.ForeColor = System.Drawing.Color.White;
            this.btnSaveProfile.Location = new System.Drawing.Point(380, 278);
            this.btnSaveProfile.Name = "btnSaveProfile";
            this.btnSaveProfile.Size = new System.Drawing.Size(210, 32);
            this.btnSaveProfile.TabIndex = 10;
            this.btnSaveProfile.Text = "💾  Save Profile Changes";
            this.btnSaveProfile.UseVisualStyleBackColor = false;
            // 
            // flpMappings
            // 
            this.flpMappings.AutoScroll = true;
            this.flpMappings.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.flpMappings.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.flpMappings.Location = new System.Drawing.Point(15, 105);
            this.flpMappings.Name = "flpMappings";
            this.flpMappings.Padding = new System.Windows.Forms.Padding(5);
            this.flpMappings.Size = new System.Drawing.Size(575, 165);
            this.flpMappings.TabIndex = 9;
            // 
            // lblMappingLabel
            // 
            this.lblMappingLabel.AutoSize = true;
            this.lblMappingLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblMappingLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(223)))));
            this.lblMappingLabel.Location = new System.Drawing.Point(15, 85);
            this.lblMappingLabel.Name = "lblMappingLabel";
            this.lblMappingLabel.Size = new System.Drawing.Size(176, 15);
            this.lblMappingLabel.TabIndex = 8;
            this.lblMappingLabel.Text = "Button Mapping Configuration:";
            // 
            // cbTargetGame
            // 
            this.cbTargetGame.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.cbTargetGame.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTargetGame.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbTargetGame.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbTargetGame.ForeColor = System.Drawing.Color.White;
            this.cbTargetGame.FormattingEnabled = true;
            this.cbTargetGame.Location = new System.Drawing.Point(380, 47);
            this.cbTargetGame.Name = "cbTargetGame";
            this.cbTargetGame.Size = new System.Drawing.Size(210, 23);
            this.cbTargetGame.TabIndex = 7;
            // 
            // lblGameLabel
            // 
            this.lblGameLabel.AutoSize = true;
            this.lblGameLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblGameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblGameLabel.Location = new System.Drawing.Point(290, 50);
            this.lblGameLabel.Name = "lblGameLabel";
            this.lblGameLabel.Size = new System.Drawing.Size(78, 15);
            this.lblGameLabel.TabIndex = 6;
            this.lblGameLabel.Text = "Target Game:";
            // 
            // cbTargetEmulator
            // 
            this.cbTargetEmulator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.cbTargetEmulator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTargetEmulator.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbTargetEmulator.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbTargetEmulator.ForeColor = System.Drawing.Color.White;
            this.cbTargetEmulator.FormattingEnabled = true;
            this.cbTargetEmulator.Location = new System.Drawing.Point(110, 47);
            this.cbTargetEmulator.Name = "cbTargetEmulator";
            this.cbTargetEmulator.Size = new System.Drawing.Size(160, 23);
            this.cbTargetEmulator.TabIndex = 5;
            // 
            // lblEmulatorLabel
            // 
            this.lblEmulatorLabel.AutoSize = true;
            this.lblEmulatorLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblEmulatorLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblEmulatorLabel.Location = new System.Drawing.Point(15, 50);
            this.lblEmulatorLabel.Name = "lblEmulatorLabel";
            this.lblEmulatorLabel.Size = new System.Drawing.Size(95, 15);
            this.lblEmulatorLabel.TabIndex = 4;
            this.lblEmulatorLabel.Text = "Target Emulator:";
            // 
            // cbControllerType
            // 
            this.cbControllerType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.cbControllerType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbControllerType.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbControllerType.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbControllerType.ForeColor = System.Drawing.Color.White;
            this.cbControllerType.FormattingEnabled = true;
            this.cbControllerType.Location = new System.Drawing.Point(380, 12);
            this.cbControllerType.Name = "cbControllerType";
            this.cbControllerType.Size = new System.Drawing.Size(210, 23);
            this.cbControllerType.TabIndex = 3;
            // 
            // lblControllerTypeLabel
            // 
            this.lblControllerTypeLabel.AutoSize = true;
            this.lblControllerTypeLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblControllerTypeLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblControllerTypeLabel.Location = new System.Drawing.Point(290, 15);
            this.lblControllerTypeLabel.Name = "lblControllerTypeLabel";
            this.lblControllerTypeLabel.Size = new System.Drawing.Size(73, 15);
            this.lblControllerTypeLabel.TabIndex = 2;
            this.lblControllerTypeLabel.Text = "Device Type:";
            // 
            // tbProfileName
            // 
            this.tbProfileName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.tbProfileName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbProfileName.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tbProfileName.ForeColor = System.Drawing.Color.White;
            this.tbProfileName.Location = new System.Drawing.Point(110, 12);
            this.tbProfileName.Name = "tbProfileName";
            this.tbProfileName.Size = new System.Drawing.Size(160, 23);
            this.tbProfileName.TabIndex = 1;
            // 
            // lblProfileNameLabel
            // 
            this.lblProfileNameLabel.AutoSize = true;
            this.lblProfileNameLabel.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblProfileNameLabel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblProfileNameLabel.Location = new System.Drawing.Point(15, 15);
            this.lblProfileNameLabel.Name = "lblProfileNameLabel";
            this.lblProfileNameLabel.Size = new System.Drawing.Size(79, 15);
            this.lblProfileNameLabel.TabIndex = 0;
            this.lblProfileNameLabel.Text = "Profile Name:";
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(780, 520);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 30);
            this.btnClose.TabIndex = 9;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // ControllerManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.ClientSize = new System.Drawing.Size(915, 565);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.pnlConfig);
            this.Controls.Add(this.lblConfigHeader);
            this.Controls.Add(this.btnDelete);
            this.Controls.Add(this.btnTestInput);
            this.Controls.Add(this.btnCreate);
            this.Controls.Add(this.lbProfiles);
            this.Controls.Add(this.lblProfilesHeader);
            this.Controls.Add(this.btnScan);
            this.Controls.Add(this.lvDevices);
            this.Controls.Add(this.lblDevicesHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ControllerManagerForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Controller Manager - Emulator Settings";
            this.pnlConfig.ResumeLayout(false);
            this.pnlConfig.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblDevicesHeader;
        private System.Windows.Forms.ListView lvDevices;
        private System.Windows.Forms.ColumnHeader colPortId;
        private System.Windows.Forms.ColumnHeader colControllerName;
        private System.Windows.Forms.ColumnHeader colDeviceType;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.Button btnScan;
        private System.Windows.Forms.Label lblProfilesHeader;
        private System.Windows.Forms.ListBox lbProfiles;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnDelete;
        private System.Windows.Forms.Button btnTestInput;
        private System.Windows.Forms.Label lblConfigHeader;
        private System.Windows.Forms.Panel pnlConfig;
        private System.Windows.Forms.Label lblProfileNameLabel;
        private System.Windows.Forms.TextBox tbProfileName;
        private System.Windows.Forms.Label lblControllerTypeLabel;
        private System.Windows.Forms.ComboBox cbControllerType;
        private System.Windows.Forms.Label lblEmulatorLabel;
        private System.Windows.Forms.ComboBox cbTargetEmulator;
        private System.Windows.Forms.Label lblGameLabel;
        private System.Windows.Forms.ComboBox cbTargetGame;
        private System.Windows.Forms.Label lblMappingLabel;
        private System.Windows.Forms.FlowLayoutPanel flpMappings;
        private System.Windows.Forms.Button btnSaveProfile;
        private System.Windows.Forms.Button btnClose;
    }
}
