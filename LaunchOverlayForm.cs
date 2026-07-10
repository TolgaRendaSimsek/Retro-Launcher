using System;
using System.Drawing;
using System.Windows.Forms;

namespace RetroLauncher
{
    public class LaunchOverlayForm : Form
    {
        public LaunchOverlayForm(string gameTitle, Form owner)
        {
            this.Owner = owner;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ShowInTaskbar = false;
            this.StartPosition = FormStartPosition.Manual;
            this.BackColor = Color.FromArgb(17, 24, 39); // Premium dark navy
            this.Opacity = 0.90;
            this.Size = owner.Size;
            this.Location = owner.Location;

            // Center panel container for content
            Panel pnlCenter = new Panel
            {
                Size = new Size(500, 200),
                Location = new Point((this.Width - 500) / 2, (this.Height - 200) / 2),
                BackColor = Color.Transparent
            };

            Label lblSpinner = new Label
            {
                Text = "🎮",
                Font = new Font("Segoe UI", 36F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(99, 102, 241), // Indigo
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 80
            };

            Label lblTitle = new Label
            {
                Text = $"Launching {gameTitle}...",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40
            };

            Label lblSubtitle = new Label
            {
                Text = "Preparing emulator environment. Please wait...",
                Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(156, 163, 175), // Gray
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 30
            };

            pnlCenter.Controls.Add(lblSubtitle);
            pnlCenter.Controls.Add(lblTitle);
            pnlCenter.Controls.Add(lblSpinner);
            this.Controls.Add(pnlCenter);

            // Close overlay if owner form is moved or resized
            owner.LocationChanged += (s, e) => this.Location = owner.Location;
            owner.SizeChanged += (s, e) => {
                this.Size = owner.Size;
                pnlCenter.Location = new Point((this.Width - 500) / 2, (this.Height - 200) / 2);
            };
        }
    }
}
