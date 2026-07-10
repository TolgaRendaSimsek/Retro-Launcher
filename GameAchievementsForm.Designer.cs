namespace RetroLauncher
{
    partial class GameAchievementsForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Label lblHeaderTitle;
        private System.Windows.Forms.Label lblHeaderProgress;
        private System.Windows.Forms.ProgressBar pbHeaderProgress;
        private System.Windows.Forms.FlowLayoutPanel flpAchievements;
        private System.Windows.Forms.Button btnClose;

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
            this.lblHeaderTitle = new System.Windows.Forms.Label();
            this.lblHeaderProgress = new System.Windows.Forms.Label();
            this.pbHeaderProgress = new System.Windows.Forms.ProgressBar();
            this.flpAchievements = new System.Windows.Forms.FlowLayoutPanel();
            this.btnClose = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblHeaderTitle
            // 
            this.lblHeaderTitle.AutoSize = true;
            this.lblHeaderTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblHeaderTitle.ForeColor = System.Drawing.Color.White;
            this.lblHeaderTitle.Location = new System.Drawing.Point(20, 20);
            this.lblHeaderTitle.Name = "lblHeaderTitle";
            this.lblHeaderTitle.Size = new System.Drawing.Size(268, 25);
            this.lblHeaderTitle.Text = "Game Title - Achievements";
            // 
            // lblHeaderProgress
            // 
            this.lblHeaderProgress.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblHeaderProgress.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblHeaderProgress.ForeColor = System.Drawing.Color.FromArgb(156, 163, 175);
            this.lblHeaderProgress.Location = new System.Drawing.Point(360, 25);
            this.lblHeaderProgress.Name = "lblHeaderProgress";
            this.lblHeaderProgress.Size = new System.Drawing.Size(220, 20);
            this.lblHeaderProgress.Text = "0 / 0 Unlocked (0%)";
            this.lblHeaderProgress.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // pbHeaderProgress
            // 
            this.pbHeaderProgress.Location = new System.Drawing.Point(20, 55);
            this.pbHeaderProgress.Name = "pbHeaderProgress";
            this.pbHeaderProgress.Size = new System.Drawing.Size(560, 12);
            this.pbHeaderProgress.TabIndex = 2;
            // 
            // flpAchievements
            // 
            this.flpAchievements.AutoScroll = true;
            this.flpAchievements.BackColor = System.Drawing.Color.FromArgb(18, 18, 22);
            this.flpAchievements.Location = new System.Drawing.Point(20, 80);
            this.flpAchievements.Name = "flpAchievements";
            this.flpAchievements.Size = new System.Drawing.Size(560, 360);
            this.flpAchievements.TabIndex = 3;
            // 
            // btnClose
            // 
            this.btnClose.BackColor = System.Drawing.Color.FromArgb(55, 65, 81);
            this.btnClose.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnClose.ForeColor = System.Drawing.Color.White;
            this.btnClose.Location = new System.Drawing.Point(460, 455);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(120, 30);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = false;
            // 
            // GameAchievementsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(24, 24, 28);
            this.ClientSize = new System.Drawing.Size(600, 500);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.flpAchievements);
            this.Controls.Add(this.pbHeaderProgress);
            this.Controls.Add(this.lblHeaderProgress);
            this.Controls.Add(this.lblHeaderTitle);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GameAchievementsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Game Achievements";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
    }
}
