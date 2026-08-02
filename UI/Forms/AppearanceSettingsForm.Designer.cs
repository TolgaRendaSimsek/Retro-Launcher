namespace RetroLauncher.UI.Forms
{
    partial class AppearanceSettingsForm
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
            this.lblTheme = new System.Windows.Forms.Label();
            this.cbTheme = new System.Windows.Forms.ComboBox();
            this.lblAccent = new System.Windows.Forms.Label();
            this.pnlAccentPreview = new System.Windows.Forms.Panel();
            this.btnPickAccent = new System.Windows.Forms.Button();
            this.lblBackground = new System.Windows.Forms.Label();
            this.tbBackgroundPath = new System.Windows.Forms.TextBox();
            this.btnBrowseBg = new System.Windows.Forms.Button();
            this.btnClearBg = new System.Windows.Forms.Button();
            this.lblFontSize = new System.Windows.Forms.Label();
            this.cbFontSize = new System.Windows.Forms.ComboBox();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblTheme
            // 
            this.lblTheme.AutoSize = true;
            this.lblTheme.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblTheme.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblTheme.Location = new System.Drawing.Point(15, 15);
            this.lblTheme.Name = "lblTheme";
            this.lblTheme.Size = new System.Drawing.Size(163, 15);
            this.lblTheme.TabIndex = 0;
            this.lblTheme.Text = "Active Theme / Appearance Skin:";
            // 
            // cbTheme
            // 
            this.cbTheme.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.cbTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbTheme.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbTheme.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbTheme.ForeColor = System.Drawing.Color.White;
            this.cbTheme.FormattingEnabled = true;
            this.cbTheme.Location = new System.Drawing.Point(15, 40);
            this.cbTheme.Name = "cbTheme";
            this.cbTheme.Size = new System.Drawing.Size(220, 23);
            this.cbTheme.TabIndex = 1;
            // 
            // lblAccent
            // 
            this.lblAccent.AutoSize = true;
            this.lblAccent.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblAccent.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblAccent.Location = new System.Drawing.Point(15, 80);
            this.lblAccent.Name = "lblAccent";
            this.lblAccent.Size = new System.Drawing.Size(117, 15);
            this.lblAccent.TabIndex = 2;
            this.lblAccent.Text = "Custom Accent Color:";
            // 
            // pnlAccentPreview
            // 
            this.pnlAccentPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlAccentPreview.Location = new System.Drawing.Point(15, 105);
            this.pnlAccentPreview.Name = "pnlAccentPreview";
            this.pnlAccentPreview.Size = new System.Drawing.Size(40, 25);
            this.pnlAccentPreview.TabIndex = 3;
            // 
            // btnPickAccent
            // 
            this.btnPickAccent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnPickAccent.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnPickAccent.FlatAppearance.BorderSize = 0;
            this.btnPickAccent.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPickAccent.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnPickAccent.ForeColor = System.Drawing.Color.White;
            this.btnPickAccent.Location = new System.Drawing.Point(65, 105);
            this.btnPickAccent.Name = "btnPickAccent";
            this.btnPickAccent.Size = new System.Drawing.Size(170, 25);
            this.btnPickAccent.TabIndex = 4;
            this.btnPickAccent.Text = "Choose Color...";
            this.btnPickAccent.UseVisualStyleBackColor = false;
            // 
            // lblBackground
            // 
            this.lblBackground.AutoSize = true;
            this.lblBackground.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblBackground.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblBackground.Location = new System.Drawing.Point(15, 150);
            this.lblBackground.Name = "lblBackground";
            this.lblBackground.Size = new System.Drawing.Size(147, 15);
            this.lblBackground.TabIndex = 5;
            this.lblBackground.Text = "Custom Background Image:";
            // 
            // tbBackgroundPath
            // 
            this.tbBackgroundPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.tbBackgroundPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tbBackgroundPath.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.tbBackgroundPath.ForeColor = System.Drawing.Color.White;
            this.tbBackgroundPath.Location = new System.Drawing.Point(15, 175);
            this.tbBackgroundPath.Name = "tbBackgroundPath";
            this.tbBackgroundPath.ReadOnly = true;
            this.tbBackgroundPath.Size = new System.Drawing.Size(360, 23);
            this.tbBackgroundPath.TabIndex = 6;
            // 
            // btnBrowseBg
            // 
            this.btnBrowseBg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnBrowseBg.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnBrowseBg.FlatAppearance.BorderSize = 0;
            this.btnBrowseBg.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnBrowseBg.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnBrowseBg.ForeColor = System.Drawing.Color.White;
            this.btnBrowseBg.Location = new System.Drawing.Point(385, 174);
            this.btnBrowseBg.Name = "btnBrowseBg";
            this.btnBrowseBg.Size = new System.Drawing.Size(110, 25);
            this.btnBrowseBg.TabIndex = 7;
            this.btnBrowseBg.Text = "Browse...";
            this.btnBrowseBg.UseVisualStyleBackColor = false;
            // 
            // btnClearBg
            // 
            this.btnClearBg.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnClearBg.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClearBg.FlatAppearance.BorderSize = 0;
            this.btnClearBg.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClearBg.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.btnClearBg.ForeColor = System.Drawing.Color.White;
            this.btnClearBg.Location = new System.Drawing.Point(385, 205);
            this.btnClearBg.Name = "btnClearBg";
            this.btnClearBg.Size = new System.Drawing.Size(110, 25);
            this.btnClearBg.TabIndex = 8;
            this.btnClearBg.Text = "Clear";
            this.btnClearBg.UseVisualStyleBackColor = false;
            // 
            // lblFontSize
            // 
            this.lblFontSize.AutoSize = true;
            this.lblFontSize.Font = new System.Drawing.Font("Segoe UI Semibold", 9F, System.Drawing.FontStyle.Bold);
            this.lblFontSize.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblFontSize.Location = new System.Drawing.Point(15, 230);
            this.lblFontSize.Name = "lblFontSize";
            this.lblFontSize.Size = new System.Drawing.Size(123, 15);
            this.lblFontSize.TabIndex = 9;
            this.lblFontSize.Text = "UI Text Font Size Scale:";
            // 
            // cbFontSize
            // 
            this.cbFontSize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(36)))), ((int)(((byte)(36)))), ((int)(((byte)(42)))));
            this.cbFontSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbFontSize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cbFontSize.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.cbFontSize.ForeColor = System.Drawing.Color.White;
            this.cbFontSize.FormattingEnabled = true;
            this.cbFontSize.Location = new System.Drawing.Point(15, 255);
            this.cbFontSize.Name = "cbFontSize";
            this.cbFontSize.Size = new System.Drawing.Size(220, 23);
            this.cbFontSize.TabIndex = 10;
            // 
            // btnApply
            // 
            this.btnApply.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(99)))), ((int)(((byte)(102)))), ((int)(((byte)(241)))));
            this.btnApply.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnApply.FlatAppearance.BorderSize = 0;
            this.btnApply.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnApply.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnApply.ForeColor = System.Drawing.Color.White;
            this.btnApply.Location = new System.Drawing.Point(15, 310);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(220, 35);
            this.btnApply.TabIndex = 11;
            this.btnApply.Text = "💾  Save && Apply Theme";
            this.btnApply.UseVisualStyleBackColor = false;
            // 
            // btnReset
            // 
            this.btnReset.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnReset.FlatAppearance.BorderSize = 0;
            this.btnReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnReset.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnReset.ForeColor = System.Drawing.Color.White;
            this.btnReset.Location = new System.Drawing.Point(245, 310);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(130, 35);
            this.btnReset.TabIndex = 12;
            this.btnReset.Text = "Reset to Defaults";
            this.btnReset.UseVisualStyleBackColor = false;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(55)))), ((int)(((byte)(65)))), ((int)(((byte)(81)))));
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(385, 310);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(110, 35);
            this.btnClose.TabIndex = 13;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // AppearanceSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.ClientSize = new System.Drawing.Size(515, 365);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.cbFontSize);
            this.Controls.Add(this.lblFontSize);
            this.Controls.Add(this.btnClearBg);
            this.Controls.Add(this.btnBrowseBg);
            this.Controls.Add(this.tbBackgroundPath);
            this.Controls.Add(this.lblBackground);
            this.Controls.Add(this.btnPickAccent);
            this.Controls.Add(this.pnlAccentPreview);
            this.Controls.Add(this.lblAccent);
            this.Controls.Add(this.cbTheme);
            this.Controls.Add(this.lblTheme);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AppearanceSettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Appearance Skins & Theme Manager";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblTheme;
        private System.Windows.Forms.ComboBox cbTheme;
        private System.Windows.Forms.Label lblAccent;
        private System.Windows.Forms.Panel pnlAccentPreview;
        private System.Windows.Forms.Button btnPickAccent;
        private System.Windows.Forms.Label lblBackground;
        private System.Windows.Forms.TextBox tbBackgroundPath;
        private System.Windows.Forms.Button btnBrowseBg;
        private System.Windows.Forms.Button btnClearBg;
        private System.Windows.Forms.Label lblFontSize;
        private System.Windows.Forms.ComboBox cbFontSize;
        private System.Windows.Forms.Button btnApply;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.Button btnClose;
    }
}
