using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RetroLauncher.UI.Forms
{
    public class HealthCheckForm : Form
    {
        private readonly ISystemHealthService _healthService = new SystemHealthService();
        private CancellationTokenSource? _cts;
        private FlowLayoutPanel pnlItems = null!;
        private Label lblSummary = null!;
        private ProgressBar pbProgress = null!;
        private Button btnStart = null!;
        private Button btnCancel = null!;

        public HealthCheckForm()
        {
            InitializeForm();
        }

        private void InitializeForm()
        {
            this.Text = "Retro Launcher System Health Check";
            this.Size = new Size(700, 500);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(20, 20, 25);
            this.ForeColor = Color.White;

            // Top Control Bar
            btnStart = new Button
            {
                Text = "▶  Run Diagnostics",
                Location = new Point(20, 20),
                Size = new Size(150, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(16, 185, 129),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White
            };
            btnStart.FlatAppearance.BorderSize = 0;
            btnStart.Click += btnStart_Click;
            this.Controls.Add(btnStart);

            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(180, 20),
                Size = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(107, 114, 128),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White,
                Enabled = false
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            btnCancel.Click += btnCancel_Click;
            this.Controls.Add(btnCancel);

            pbProgress = new ProgressBar
            {
                Location = new Point(300, 25),
                Size = new Size(365, 20),
                Style = ProgressBarStyle.Continuous,
                Visible = false
            };
            this.Controls.Add(pbProgress);

            // Center scrollable list
            pnlItems = new FlowLayoutPanel
            {
                Location = new Point(20, 70),
                Size = new Size(645, 330),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.FromArgb(28, 28, 34)
            };
            this.Controls.Add(pnlItems);

            // Bottom summary panel
            lblSummary = new Label
            {
                Text = "Diagnostics ready. Click 'Run Diagnostics' to begin.",
                Location = new Point(20, 420),
                Size = new Size(500, 30),
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(156, 163, 175)
            };
            this.Controls.Add(lblSummary);

            Button btnClose = new Button
            {
                Text = "Close",
                Location = new Point(565, 415),
                Size = new Size(100, 35),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(75, 85, 99),
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.White
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => this.Close();
            this.Controls.Add(btnClose);
        }

        private async void btnStart_Click(object? sender, EventArgs e)
        {
            btnStart.Enabled = false;
            btnCancel.Enabled = true;
            pbProgress.Value = 0;
            pbProgress.Visible = true;
            pnlItems.Controls.Clear();
            lblSummary.Text = "Running system checks...";

            _cts = new CancellationTokenSource();
            var progress = new Progress<int>(v =>
            {
                if (this.IsHandleCreated)
                {
                    this.BeginInvoke(new Action(() => pbProgress.Value = v));
                }
            });

            try
            {
                var result = await _healthService.RunHealthCheckAsync(progress, _cts.Token);
                PopulateResults(result);
            }
            catch (OperationCanceledException)
            {
                lblSummary.Text = "Diagnostics cancelled by user.";
            }
            catch (Exception ex)
            {
                lblSummary.Text = "An error occurred during diagnostics.";
                MessageBox.Show($"Health Check failed:\n{ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnStart.Enabled = true;
                btnCancel.Enabled = false;
                pbProgress.Visible = false;
                _cts?.Dispose();
                _cts = null;
            }
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            _cts?.Cancel();
        }

        private void PopulateResults(HealthCheckResult result)
        {
            pnlItems.Controls.Clear();

            foreach (var item in result.Items)
            {
                Panel card = new Panel
                {
                    Size = new Size(610, 75),
                    Margin = new Padding(5),
                    BackColor = Color.FromArgb(44, 44, 52)
                };

                // Status indicator
                Color statusColor = Color.FromArgb(16, 185, 129); // Green
                if (item.Status == HealthStatus.Warning) statusColor = Color.FromArgb(245, 158, 11); // Orange
                if (item.Status == HealthStatus.Error) statusColor = Color.FromArgb(239, 68, 68); // Red
                if (item.Status == HealthStatus.Unknown) statusColor = Color.FromArgb(156, 163, 175); // Gray

                Panel indicator = new Panel
                {
                    Location = new Point(10, 15),
                    Size = new Size(10, 45),
                    BackColor = statusColor
                };
                card.Controls.Add(indicator);

                Label lblTitle = new Label
                {
                    Text = $"{item.Title} - {item.Status}",
                    Location = new Point(30, 8),
                    Size = new Size(400, 18),
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.White
                };
                card.Controls.Add(lblTitle);

                Label lblDesc = new Label
                {
                    Text = item.Description,
                    Location = new Point(30, 28),
                    Size = new Size(420, 15),
                    Font = new Font("Segoe UI", 8.5F),
                    ForeColor = Color.FromArgb(209, 213, 219)
                };
                card.Controls.Add(lblDesc);

                Label lblFix = new Label
                {
                    Text = string.IsNullOrEmpty(item.SuggestedFix) ? "" : $"Suggestion: {item.SuggestedFix}",
                    Location = new Point(30, 46),
                    Size = new Size(420, 15),
                    Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                    ForeColor = Color.FromArgb(245, 158, 11)
                };
                card.Controls.Add(lblFix);

                if (item.FixAction != HealthFixAction.None)
                {
                    Button btnFix = new Button
                    {
                        Text = "Fix",
                        Location = new Point(510, 20),
                        Size = new Size(80, 30),
                        FlatStyle = FlatStyle.Flat,
                        BackColor = Color.FromArgb(79, 70, 229),
                        Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                        ForeColor = Color.White
                    };
                    btnFix.FlatAppearance.BorderSize = 0;
                    btnFix.Click += async (s, e) =>
                    {
                        // Destructive warning
                        if (item.FixAction == HealthFixAction.ClearStaleStaging)
                        {
                            var conf = MessageBox.Show(
                                "Are you sure you want to clean up stale staging and backup directories?\n\nThis action cannot be undone.",
                                "Confirm Clean",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Warning
                            );
                            if (conf == DialogResult.No) return;
                        }

                        btnFix.Enabled = false;
                        bool success = await _healthService.ExecuteFixAsync(item, CancellationToken.None);
                        if (success)
                        {
                            MessageBox.Show("Fix executed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            btnStart.PerformClick();
                        }
                        else
                        {
                            MessageBox.Show("Failed to apply suggested fix action automatically. Please resolve the issue manually.", "Fix Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            btnFix.Enabled = true;
                        }
                    };
                    card.Controls.Add(btnFix);
                }

                pnlItems.Controls.Add(card);
            }

            lblSummary.Text = $"Healthy: {result.HealthyCount} | Warnings: {result.WarningCount} | Errors: {result.ErrorCount} | Unknown: {result.UnknownCount}";
        }
    }
}
