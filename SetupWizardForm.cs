using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RetroLauncher
{
    public class SetupWizardForm : Form
    {
        private int _currentStep = 1;

        // UI Panels
        private Panel _pnlHeader = null!;
        private Panel _pnlContent = null!;
        private Panel _pnlFooter = null!;
        
        // Header Controls
        private Label _lblHeaderTitle = null!;
        private Label _lblHeaderSubtitle = null!;

        // Navigation Buttons
        private Button _btnBack = null!;
        private Button _btnNext = null!;
        private Button _btnCancel = null!;

        // Step 1 Controls (Selections)
        private Panel _pnlStep1 = null!;
        private List<EmulatorSelectionRow> _selectionRows = null!;

        // Step 2 Controls (Progress)
        private Panel _pnlStep2 = null!;
        private ProgressBar _pbOverall = null!;
        private Label _lblOverallStatus = null!;
        private Panel _pnlProgressContainer = null!;
        private List<EmulatorProgressRow> _progressRows = null!;

        // Step 3 Controls (Summary)
        private Panel _pnlStep3 = null!;
        private Label _lblSummaryText = null!;
        private ListBox _lstSummary = null!;

        public SetupWizardForm()
        {
            this.Text = "First-Time Setup Wizard";
            this.Size = new Size(640, 480);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeUI();
            ThemeManager.Instance.ApplyTheme(this);
            LoadStep(1);
        }

        private void InitializeUI()
        {
            // 1. Header Panel
            _pnlHeader = new Panel
            {
                Dock = DockStyle.Top,
                Height = 80,
                BackColor = Color.FromArgb(31, 41, 55) // Dark slate
            };
            
            _lblHeaderTitle = new Label
            {
                Text = "Emulator Setup Wizard",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.White,
                Location = new Point(20, 15),
                AutoSize = true
            };

            _lblHeaderSubtitle = new Label
            {
                Text = "Configure emulators for retro platforms.",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(156, 163, 175),
                Location = new Point(20, 42),
                Width = 580,
                Height = 30
            };

            _pnlHeader.Controls.Add(_lblHeaderTitle);
            _pnlHeader.Controls.Add(_lblHeaderSubtitle);
            this.Controls.Add(_pnlHeader);

            // 2. Footer Panel
            _pnlFooter = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 60,
                BackColor = Color.FromArgb(24, 24, 28)
            };

            _btnCancel = new Button
            {
                Text = "Cancel",
                Size = new Size(90, 32),
                Location = new Point(20, 14),
                FlatStyle = FlatStyle.Flat
            };
            _btnCancel.Click += (s, e) => this.Close();

            _btnBack = new Button
            {
                Text = "< Back",
                Size = new Size(90, 32),
                Location = new Point(410, 14),
                FlatStyle = FlatStyle.Flat
            };
            _btnBack.Click += (s, e) => LoadStep(_currentStep - 1);

            _btnNext = new Button
            {
                Text = "Next >",
                Size = new Size(90, 32),
                Location = new Point(510, 14),
                FlatStyle = FlatStyle.Flat
            };
            _btnNext.Click += btnNext_Click;

            _pnlFooter.Controls.Add(_btnCancel);
            _pnlFooter.Controls.Add(_btnBack);
            _pnlFooter.Controls.Add(_btnNext);
            this.Controls.Add(_pnlFooter);

            // 3. Main Content Area Panel
            _pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };
            this.Controls.Add(_pnlContent);

            // Setup step panels
            InitializeStep1Panel();
            InitializeStep2Panel();
            InitializeStep3Panel();
        }

        private void InitializeStep1Panel()
        {
            _pnlStep1 = new Panel { Dock = DockStyle.Fill, Visible = false };
            
            Label lblWelcome = new Label
            {
                Text = "Select the recommended emulators you wish to install. RetroLauncher will automatically fetch the latest release, extract, and configure the launch paths.",
                Font = new Font("Segoe UI", 10F),
                Location = new Point(0, 0),
                Size = new Size(580, 50),
                ForeColor = Color.White
            };
            _pnlStep1.Controls.Add(lblWelcome);

            Panel pnlGridHeader = new Panel
            {
                Location = new Point(0, 60),
                Size = new Size(580, 30),
                BackColor = Color.FromArgb(31, 41, 55)
            };
            Label lblHCol1 = new Label { Text = "Emulator", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(10, 7), AutoSize = true };
            Label lblHCol2 = new Label { Text = "Platforms", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(180, 7), AutoSize = true };
            Label lblHCol3 = new Label { Text = "Status", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(380, 7), AutoSize = true };
            Label lblHCol4 = new Label { Text = "Download Size", Font = new Font("Segoe UI", 9F, FontStyle.Bold), ForeColor = Color.White, Location = new Point(480, 7), AutoSize = true };
            pnlGridHeader.Controls.AddRange(new Control[] { lblHCol1, lblHCol2, lblHCol3, lblHCol4 });
            _pnlStep1.Controls.Add(pnlGridHeader);

            _selectionRows = new List<EmulatorSelectionRow>();
            int yOffset = 95;

            var emulators = EmulatorManager.Instance.Config.Emulators;
            foreach (var emu in emulators)
            {
                var row = new EmulatorSelectionRow(emu, yOffset);
                _selectionRows.Add(row);
                _pnlStep1.Controls.Add(row.ContainerPanel);
                yOffset += 45;
            }

            _pnlContent.Controls.Add(_pnlStep1);
        }

        private void InitializeStep2Panel()
        {
            _pnlStep2 = new Panel { Dock = DockStyle.Fill, Visible = false };

            _lblOverallStatus = new Label
            {
                Text = "Downloading and configuring emulators...",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(0, 0),
                Size = new Size(580, 25),
                ForeColor = Color.White
            };
            _pnlStep2.Controls.Add(_lblOverallStatus);

            _pbOverall = new ProgressBar
            {
                Location = new Point(0, 30),
                Size = new Size(580, 20),
                Style = ProgressBarStyle.Continuous
            };
            _pnlStep2.Controls.Add(_pbOverall);

            _pnlProgressContainer = new Panel
            {
                Location = new Point(0, 70),
                Size = new Size(580, 200),
                AutoScroll = true
            };
            _pnlStep2.Controls.Add(_pnlProgressContainer);

            _pnlContent.Controls.Add(_pnlStep2);
        }

        private void InitializeStep3Panel()
        {
            _pnlStep3 = new Panel { Dock = DockStyle.Fill, Visible = false };

            _lblSummaryText = new Label
            {
                Text = "First-time installation completed!",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(0, 0),
                Size = new Size(580, 30),
                ForeColor = Color.FromArgb(16, 185, 129) // Green
            };
            _pnlStep3.Controls.Add(_lblSummaryText);

            Label lblDetails = new Label
            {
                Text = "Below is a summary of the emulator setup status. You can now download BIOS files or launch your library directly.",
                Font = new Font("Segoe UI", 9.5F),
                Location = new Point(0, 35),
                Size = new Size(580, 40),
                ForeColor = Color.White
            };
            _pnlStep3.Controls.Add(lblDetails);

            _lstSummary = new ListBox
            {
                Location = new Point(0, 85),
                Size = new Size(580, 180),
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.FromArgb(31, 41, 55),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            _pnlStep3.Controls.Add(_lstSummary);

            _pnlContent.Controls.Add(_pnlStep3);
        }

        private void LoadStep(int step)
        {
            _currentStep = step;
            _pnlStep1.Visible = (_currentStep == 1);
            _pnlStep2.Visible = (_currentStep == 2);
            _pnlStep3.Visible = (_currentStep == 3);

            if (_currentStep == 1)
            {
                _lblHeaderTitle.Text = "Select Emulators";
                _lblHeaderSubtitle.Text = "Select recommended emulators to download and install.";
                _btnBack.Enabled = false;
                _btnNext.Enabled = true;
                _btnNext.Text = "Install >";
                _btnCancel.Text = "Cancel";

                // Fetch sizes in background
                foreach (var row in _selectionRows)
                {
                    row.LoadSizeAsync();
                }
            }
            else if (_currentStep == 2)
            {
                _lblHeaderTitle.Text = "Installing Emulators";
                _lblHeaderSubtitle.Text = "Downloading packages and extracting emulators. Do not close the window.";
                _btnBack.Enabled = false;
                _btnNext.Enabled = false;
                _btnCancel.Enabled = false;
            }
            else if (_currentStep == 3)
            {
                _lblHeaderTitle.Text = "Setup Summary";
                _lblHeaderSubtitle.Text = "Configuration is complete. Review installation results below.";
                _btnBack.Enabled = false;
                _btnNext.Enabled = true;
                _btnNext.Text = "Finish";
                _btnCancel.Enabled = false;
                _btnCancel.Visible = false;

                // Save first run configuration completion
                var settings = SettingsManager.LoadSettings();
                settings.IsFirstRun = false;
                SettingsManager.SaveSettings(settings);
            }
        }

        private async void btnNext_Click(object? sender, EventArgs e)
        {
            if (_currentStep == 1)
            {
                var selectedItems = _selectionRows.Where(r => r.IsChecked).Select(r => r.Emulator).ToList();
                if (selectedItems.Count == 0)
                {
                    var res = MessageBox.Show(
                        "You haven't selected any emulators to install.\n\nDo you want to skip installation and proceed to the library?",
                        "Skip Setup?",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question
                    );
                    if (res == DialogResult.Yes)
                    {
                        LoadStep(3);
                        BuildSummary(new Dictionary<string, string> { { "General", "No emulators were selected for installation." } });
                    }
                    return;
                }

                LoadStep(2);
                await StartInstallationAsync(selectedItems);
            }
            else if (_currentStep == 3)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private async Task StartInstallationAsync(List<EmulatorItem> emulators)
        {
            _pnlProgressContainer.Controls.Clear();
            _progressRows = new List<EmulatorProgressRow>();

            int y = 0;
            foreach (var emu in emulators)
            {
                var row = new EmulatorProgressRow(emu, y);
                _progressRows.Add(row);
                _pnlProgressContainer.Controls.Add(row.ContainerPanel);
                y += 50;
            }

            _pbOverall.Value = 0;
            var results = new Dictionary<string, string>();

            for (int i = 0; i < _progressRows.Count; i++)
            {
                var row = _progressRows[i];
                PackageInstallResult? installResult = null;
                int retries = 0;

                while ((installResult == null || !installResult.Success) && retries < 2)
                {
                    installResult = await RunSingleInstallation(row);
                    if (!installResult.Success)
                    {
                        retries++;
                        if (retries < 2)
                        {
                            row.SetStatus("Retrying setup... (attempt 2)");
                            await Task.Delay(2000);
                        }
                    }
                }

                if (installResult != null && installResult.Success)
                {
                    results[row.Emulator.Name] = "Installed successfully.";
                }
                else
                {
                    string errMsg = installResult?.ErrorMessage ?? "Unknown error";
                    results[row.Emulator.Name] = $"Installation failed: {errMsg}";
                    row.ShowRetryButton(async () =>
                    {
                        row.HideRetryButton();
                        row.SetStatus("Restarting installation...");
                        var retriedResult = await RunSingleInstallation(row);
                        if (retriedResult.Success)
                        {
                            results[row.Emulator.Name] = "Installed successfully.";
                            CheckIfAllDoneAndTransition(results);
                        }
                        else
                        {
                            string retriedErrMsg = retriedResult.ErrorMessage ?? "Unknown error";
                            results[row.Emulator.Name] = $"Installation failed: {retriedErrMsg}";
                            row.ShowRetryButton(null);
                        }
                    });
                }

                _pbOverall.Value = (int)(((double)(i + 1) / _progressRows.Count) * 100);
            }

            CheckIfAllDoneAndTransition(results);
        }

        private void CheckIfAllDoneAndTransition(Dictionary<string, string> results)
        {
            // If any rows still have visible retry buttons, wait for manual retries
            bool hasActiveRetries = _progressRows.Any(r => r.IsRetryButtonVisible);
            if (!hasActiveRetries)
            {
                BuildSummary(results);
                LoadStep(3);
            }
        }

        private async Task<PackageInstallResult> RunSingleInstallation(EmulatorProgressRow row)
        {
            row.HideRetryButton();
            var progress = new Progress<int>(percent =>
            {
                row.SetProgress(percent);
                if (percent < 90)
                    row.SetStatus($"Downloading... ({percent}%)");
                else if (percent < 100)
                    row.SetStatus("Extracting files...");
                else
                    row.SetStatus("Completed");
            });

            try
            {
                var result = await EmulatorManager.Instance.InstallEmulator(row.Emulator.Id, progress);
                if (result.Success)
                {
                    row.SetStatus("Completed");
                    row.SetProgress(100);
                }
                else
                {
                    row.SetProgress(0);
                    row.SetStatus($"Failed at {result.FailedStage}: {result.ErrorMessage}");
                }
                return result;
            }
            catch (Exception ex)
            {
                row.SetProgress(0);
                row.SetStatus($"Failed: {ex.Message}");
                return new PackageInstallResult
                {
                    Success = false,
                    PackageId = row.Emulator.Id,
                    FailedStage = PackageInstallStage.Downloading,
                    ErrorMessage = ex.Message,
                    Exception = ex
                };
            }
        }

        private void BuildSummary(Dictionary<string, string> results)
        {
            _lstSummary.Items.Clear();
            bool allOk = true;

            foreach (var kvp in results)
            {
                _lstSummary.Items.Add($"• {kvp.Key}: {kvp.Value}");
                if (kvp.Value.Contains("failed")) allOk = false;
            }

            if (allOk)
            {
                _lblSummaryText.Text = "First-time setup completed successfully!";
                _lblSummaryText.ForeColor = Color.FromArgb(16, 185, 129); // Green
            }
            else
            {
                _lblSummaryText.Text = "Setup completed with errors.";
                _lblSummaryText.ForeColor = Color.FromArgb(239, 68, 68); // Red
            }
        }

        // Inner Classes for Layout Helper Rows
        private class EmulatorSelectionRow
        {
            public EmulatorItem Emulator { get; }
            public Panel ContainerPanel { get; }
            public CheckBox ChkSelect { get; }
            private Label _lblPlatforms;
            private Label _lblStatus;
            private Label _lblSize;

            public bool IsChecked => ChkSelect.Checked;

            public EmulatorSelectionRow(EmulatorItem emu, int yPosition)
            {
                Emulator = emu;

                ContainerPanel = new Panel
                {
                    Location = new Point(0, yPosition),
                    Size = new Size(580, 40),
                    BackColor = Color.FromArgb(24, 24, 28)
                };

                bool isInstalled = EmulatorManager.Instance.VerifyExecutable(emu.Id);

                ChkSelect = new CheckBox
                {
                    Text = emu.Name,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(10, 8),
                    Size = new Size(160, 24),
                    Checked = !isInstalled,
                    Enabled = !isInstalled
                };

                _lblPlatforms = new Label
                {
                    Text = string.Join(", ", emu.SupportedPlatforms),
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(156, 163, 175),
                    Location = new Point(180, 11),
                    Size = new Size(190, 20)
                };

                _lblStatus = new Label
                {
                    Text = isInstalled ? "Installed" : "Not Installed",
                    Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                    ForeColor = isInstalled ? Color.FromArgb(16, 185, 129) : Color.FromArgb(245, 158, 11), // Orange
                    Location = new Point(380, 11),
                    Size = new Size(95, 20)
                };

                _lblSize = new Label
                {
                    Text = "Checking size...",
                    Font = new Font("Segoe UI", 9F),
                    ForeColor = Color.FromArgb(156, 163, 175),
                    Location = new Point(480, 11),
                    Size = new Size(95, 20)
                };

                ContainerPanel.Controls.AddRange(new Control[] { ChkSelect, _lblPlatforms, _lblStatus, _lblSize });
            }

            public async void LoadSizeAsync()
            {
                try
                {
                    var info = await EmulatorManager.GetLatestReleaseInfoAsync(Emulator.GithubRepository);
                    if (info.HasValue)
                    {
                        long bytes = info.Value.Size;
                        double mb = (double)bytes / (1024 * 1024);
                        _lblSize.Text = mb > 0 ? $"{mb:F1} MB" : "Unknown";
                    }
                    else
                    {
                        _lblSize.Text = "Unknown";
                    }
                }
                catch
                {
                    _lblSize.Text = "Offline";
                }
            }
        }

        private class EmulatorProgressRow
        {
            public EmulatorItem Emulator { get; }
            public Panel ContainerPanel { get; }
            private Label _lblName;
            private Label _lblStatus;
            private ProgressBar _pbItem;
            private Button _btnRetry;
            private Action? _retryAction;

            public bool IsRetryButtonVisible => _btnRetry.Visible;

            public EmulatorProgressRow(EmulatorItem emu, int yPosition)
            {
                Emulator = emu;

                ContainerPanel = new Panel
                {
                    Location = new Point(0, yPosition),
                    Size = new Size(560, 45),
                    BackColor = Color.FromArgb(24, 24, 28)
                };

                _lblName = new Label
                {
                    Text = emu.Name,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(10, 12),
                    Size = new Size(120, 20)
                };

                _lblStatus = new Label
                {
                    Text = "Queued",
                    Font = new Font("Segoe UI", 8.5F),
                    ForeColor = Color.FromArgb(156, 163, 175),
                    Location = new Point(140, 14),
                    Size = new Size(160, 20)
                };

                _pbItem = new ProgressBar
                {
                    Location = new Point(310, 12),
                    Size = new Size(160, 18),
                    Style = ProgressBarStyle.Continuous
                };

                _btnRetry = new Button
                {
                    Text = "Retry",
                    Size = new Size(60, 22),
                    Location = new Point(485, 10),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(239, 68, 68),
                    ForeColor = Color.White,
                    Visible = false
                };
                _btnRetry.Click += (s, e) => _retryAction?.Invoke();

                ContainerPanel.Controls.AddRange(new Control[] { _lblName, _lblStatus, _pbItem, _btnRetry });
            }

            public void SetProgress(int percent)
            {
                _pbItem.Value = Math.Min(100, Math.Max(0, percent));
            }

            public void SetStatus(string msg)
            {
                _lblStatus.Text = msg;
            }

            public void ShowRetryButton(Action? retryAction)
            {
                _retryAction = retryAction;
                _btnRetry.Visible = true;
                _lblStatus.ForeColor = Color.FromArgb(239, 68, 68); // Red
            }

            public void HideRetryButton()
            {
                _btnRetry.Visible = false;
                _lblStatus.ForeColor = Color.FromArgb(156, 163, 175);
            }
        }
    }
}
