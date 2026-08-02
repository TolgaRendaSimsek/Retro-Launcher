namespace RetroLauncher.UI.Controls
{
    partial class GameListRow
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.PictureBox pbThumb;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblPlatform;
        private System.Windows.Forms.Label lblPlaytime;

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
            this.pbThumb = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblPlatform = new System.Windows.Forms.Label();
            this.lblPlaytime = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbThumb)).BeginInit();
            this.SuspendLayout();
            // 
            // pbThumb
            // 
            this.pbThumb.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.pbThumb.Location = new System.Drawing.Point(12, 5);
            this.pbThumb.Name = "pbThumb";
            this.pbThumb.Size = new System.Drawing.Size(40, 50);
            this.pbThumb.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pbThumb.TabIndex = 0;
            this.pbThumb.TabStop = false;
            // 
            // lblTitle
            // 
            this.lblTitle.BackColor = System.Drawing.Color.Transparent;
            this.lblTitle.Font = new System.Drawing.Font("Segoe UI Semibold", 9.5F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblTitle.ForeColor = System.Drawing.Color.White;
            this.lblTitle.Location = new System.Drawing.Point(65, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(250, 20);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Game Title";
            // 
            // lblPlatform
            // 
            this.lblPlatform.BackColor = System.Drawing.Color.Transparent;
            this.lblPlatform.Font = new System.Drawing.Font("Segoe UI", 8F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point);
            this.lblPlatform.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(156)))), ((int)(((byte)(163)))), ((int)(((byte)(175)))));
            this.lblPlatform.Location = new System.Drawing.Point(65, 32);
            this.lblPlatform.Name = "lblPlatform";
            this.lblPlatform.Size = new System.Drawing.Size(200, 18);
            this.lblPlatform.TabIndex = 2;
            this.lblPlatform.Text = "Console Platform";
            // 
            // lblPlaytime
            // 
            this.lblPlaytime.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblPlaytime.BackColor = System.Drawing.Color.Transparent;
            this.lblPlaytime.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblPlaytime.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(209)))), ((int)(((byte)(213)))), ((int)(((byte)(223)))));
            this.lblPlaytime.Location = new System.Drawing.Point(340, 20);
            this.lblPlaytime.Name = "lblPlaytime";
            this.lblPlaytime.Size = new System.Drawing.Size(120, 20);
            this.lblPlaytime.TabIndex = 3;
            this.lblPlaytime.Text = "Playtime: 0 mins";
            this.lblPlaytime.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // GameListRow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(24)))), ((int)(((byte)(28)))));
            this.Controls.Add(this.lblPlaytime);
            this.Controls.Add(this.lblPlatform);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.pbThumb);
            this.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Name = "GameListRow";
            this.Size = new System.Drawing.Size(560, 60);
            ((System.ComponentModel.ISupportInitialize)(this.pbThumb)).EndInit();
            this.ResumeLayout(false);

        }
    }
}
