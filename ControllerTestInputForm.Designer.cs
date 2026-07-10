namespace RetroLauncher
{
    partial class ControllerTestInputForm
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
            this.components = new System.ComponentModel.Container();
            this.lblChoose = new System.Windows.Forms.Label();
            this.cbDevices = new System.Windows.Forms.ComboBox();
            this.gbLiveState = new System.Windows.Forms.GroupBox();
            this.lblPOVState = new System.Windows.Forms.Label();
            this.lblAxesState = new System.Windows.Forms.Label();
            this.lblButtonsPressed = new System.Windows.Forms.Label();
            this.btnCloseTest = new System.Windows.Forms.Button();
            this.tmrPoll = new System.Windows.Forms.Timer(this.components);
            this.gbLiveState.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblChoose
            // 
            this.lblChoose.AutoSize = true;
            this.lblChoose.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblChoose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblChoose.Location = new System.Drawing.Point(15, 15);
            this.lblChoose.Name = "lblChoose";
            this.lblChoose.Size = new System.Drawing.Size(147, 15);
            this.lblChoose.TabIndex = 0;
            this.lblChoose.Text = "Select Controller to Test:";
            // 
            // cbDevices
            // 
            this.cbDevices.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.cbDevices.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbDevices.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbDevices.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbDevices.ForeColor = System.Drawing.Color.White;
            this.cbDevices.FormattingEnabled = true;
            this.cbDevices.Location = new System.Drawing.Point(15, 40);
            this.cbDevices.Name = "cbDevices";
            this.cbDevices.Size = new System.Drawing.Size(415, 23);
            this.cbDevices.TabIndex = 1;
            // 
            // gbLiveState
            // 
            this.gbLiveState.Controls.Add(this.lblPOVState);
            this.gbLiveState.Controls.Add(this.lblAxesState);
            this.gbLiveState.Controls.Add(this.lblButtonsPressed);
            this.gbLiveState.Font = new System.Drawing.Font("Segoe UI Black", 8.5F, System.Drawing.FontStyle.Bold);
            this.gbLiveState.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.gbLiveState.Location = new System.Drawing.Point(15, 80);
            this.gbLiveState.Name = "gbLiveState";
            this.gbLiveState.Size = new System.Drawing.Size(415, 180);
            this.gbLiveState.TabIndex = 2;
            this.gbLiveState.TabStop = false;
            this.gbLiveState.Text = "LIVE INPUT STATE";
            // 
            // lblPOVState
            // 
            this.lblPOVState.AutoSize = true;
            this.lblPOVState.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblPOVState.ForeColor = System.Drawing.Color.White;
            this.lblPOVState.Location = new System.Drawing.Point(15, 140);
            this.lblPOVState.Name = "lblPOVState";
            this.lblPOVState.Size = new System.Drawing.Size(140, 17);
            this.lblPOVState.TabIndex = 2;
            this.lblPOVState.Text = "D-Pad / POV: Centered";
            // 
            // lblAxesState
            // 
            this.lblAxesState.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblAxesState.ForeColor = System.Drawing.Color.White;
            this.lblAxesState.Location = new System.Drawing.Point(15, 75);
            this.lblAxesState.Name = "lblAxesState";
            this.lblAxesState.Size = new System.Drawing.Size(380, 50);
            this.lblAxesState.TabIndex = 1;
            this.lblAxesState.Text = "Axes Coordinates:\r\nX: 32768  Y: 32768\r\nZ: 32768  R: 32768";
            // 
            // lblButtonsPressed
            // 
            this.lblButtonsPressed.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblButtonsPressed.ForeColor = System.Drawing.Color.White;
            this.lblButtonsPressed.Location = new System.Drawing.Point(15, 30);
            this.lblButtonsPressed.Name = "lblButtonsPressed";
            this.lblButtonsPressed.Size = new System.Drawing.Size(380, 35);
            this.lblButtonsPressed.TabIndex = 0;
            this.lblButtonsPressed.Text = "Pressed Buttons: None";
            // 
            // btnCloseTest
            // 
            this.btnCloseTest.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnCloseTest.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCloseTest.FlatAppearance.BorderSize = 0;
            this.btnCloseTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCloseTest.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnCloseTest.ForeColor = System.Drawing.Color.White;
            this.btnCloseTest.Location = new System.Drawing.Point(310, 280);
            this.btnCloseTest.Name = "btnCloseTest";
            this.btnCloseTest.Size = new System.Drawing.Size(120, 35);
            this.btnCloseTest.TabIndex = 3;
            this.btnCloseTest.Text = "Close";
            this.btnCloseTest.UseVisualStyleBackColor = false;
            // 
            // tmrPoll
            // 
            this.tmrPoll.Interval = 50;
            // 
            // ControllerTestInputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.ClientSize = new System.Drawing.Size(445, 335);
            this.Controls.Add(this.btnCloseTest);
            this.Controls.Add(this.gbLiveState);
            this.Controls.Add(this.cbDevices);
            this.Controls.Add(this.lblChoose);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ControllerTestInputForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Controller Test Input utility";
            this.gbLiveState.ResumeLayout(false);
            this.gbLiveState.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblChoose;
        private System.Windows.Forms.ComboBox cbDevices;
        private System.Windows.Forms.GroupBox gbLiveState;
        private System.Windows.Forms.Label lblPOVState;
        private System.Windows.Forms.Label lblAxesState;
        private System.Windows.Forms.Label lblButtonsPressed;
        private System.Windows.Forms.Button btnCloseTest;
        private System.Windows.Forms.Timer tmrPoll;
    }
}
