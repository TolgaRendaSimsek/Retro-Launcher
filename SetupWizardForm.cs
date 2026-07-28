using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RetroLauncher
{
    public class SetupWizardForm : Form
    {
        private int _currentStep = 1;
        private CancellationTokenSource? _cts;
        private bool _isInstalling = false;
        private readonly IEmulatorPackageDefinitionProvider _definitionProvider = new JsonEmulatorPackageDefinitionProvider();

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
            _btnCancel.Click += btnCancel_Click;

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
                _btnCancel.Enabled = true;
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

        private async void btnCancel_Click(object? sender, EventArgs e)
        {
            if (_isInstalling)
            {
                _btnCancel.Enabled = false;
                _btnCancel.Text = "Cancelling...";
                _lblOverallStatus.Text = "Cancelling installation and cleaning up...";
                _cts?.Cancel();

                while (_isInstalling)
                {
                    await Task.Delay(100);
                }

                _btnCancel.Enabled = true;
                _btnCancel.Text = "Cancel";
                _lblOverallStatus.Text = "Installation cancelled.";
                _btnBack.Enabled = true;
                _btnNext.Enabled = false;
            }
            else
            {
                this.Close();
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
            _isInstalling = true;
            _btnBack.Enabled = false;
            _btnNext.Enabled = false;
            _btnCancel.Enabled = true;
            _btnCancel.Text = "Cancel";

            _pnlProgressContainer.Controls.Clear();
            _progressRows = new List<EmulatorProgressRow>();

            int y = 0;
            foreach (var emu in emulators)
            {
                var row = new EmulatorProgressRow(emu, y);
                _progressRows.Add(row);
                _pnlProgressContainer.Controls.Add(row.ContainerPanel);
                y += 55;
            }

            _pbOverall.Value = 0;
            _lblOverallStatus.Text = "Downloading and configuring emulators...";

            _cts = new CancellationTokenSource();

            var tasks = _progressRows.Select(row => InstallSingleRowAsync(row, _cts.Token)).ToList();
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception)
            {
                // Handled in individual task logic
            }

            CheckOverallStatusAndTransition();
        }

        private async Task<PackageInstallResult> InstallSingleRowAsync(EmulatorProgressRow row, CancellationToken token)
        {
            var service = new EmulatorInstallationService();
            var progress = new Progress<EmulatorInstallationProgress>(p =>
            {
                if (this.IsDisposed) return;
                this.BeginInvoke(new Action(() =>
                {
                    row.SetProgress(p.Percentage);
                    row.SetStatus(p.CurrentStep);
                    UpdateOverallProgress();
                }));
            });

            var req = new EmulatorInstallationRequest
            {
                EmulatorId = row.Emulator.Id,
                OperationId = row.OperationId,
                Progress = progress,
                CancellationToken = token
            };

            try
            {
                var result = await service.InstallAsync(req);
                if (this.IsDisposed) return result;

                this.Invoke(new Action(() =>
                {
                    row.SetLastResult(result);
                    if (result.Success)
                    {
                        row.SetStatus("Installed successfully.");
                        row.SetProgress(100);
                        row.HideRetryButton();
                        row.HideDetailsButton();
                    }
                    else
                    {
                        row.SetProgress(0);
                        row.SetStatus("Failed");
                        row.SetLastFailedStage(result.FailedStage);
                        row.ShowRetryAndDetailsButtons(
                            retryAction: () => RetryRow(row),
                            detailsAction: () => ShowDetails(row)
                        );
                    }
                    UpdateOverallProgress();
                }));
                return result;
            }
            catch (OperationCanceledException)
            {
                if (this.IsDisposed) throw;
                var cancelResult = new PackageInstallResult
                {
                    Success = false,
                    PackageId = row.Emulator.Id,
                    FailedStage = PackageInstallStage.Downloading,
                    ErrorMessage = "Installation cancelled by user.",
                    Exception = new OperationCanceledException()
                };
                this.Invoke(new Action(() =>
                {
                    row.SetLastResult(cancelResult);
                    row.SetProgress(0);
                    row.SetStatus("Cancelled");
                    row.HideRetryButton();
                    row.HideDetailsButton();
                    UpdateOverallProgress();
                }));
                throw;
            }
            catch (Exception ex)
            {
                if (this.IsDisposed) throw;
                var failResult = new PackageInstallResult
                {
                    Success = false,
                    PackageId = row.Emulator.Id,
                    FailedStage = PackageInstallStage.Downloading,
                    ErrorMessage = ex.Message,
                    Exception = ex
                };
                this.Invoke(new Action(() =>
                {
                    row.SetLastResult(failResult);
                    row.SetProgress(0);
                    row.SetStatus("Failed");
                    row.ShowRetryAndDetailsButtons(
                        retryAction: () => RetryRow(row),
                        detailsAction: () => ShowDetails(row)
                    );
                    UpdateOverallProgress();
                }));
                return failResult;
            }
        }

        private async void RetryRow(EmulatorProgressRow row)
        {
            _btnBack.Enabled = false;
            _btnNext.Enabled = false;
            _btnCancel.Enabled = true;

            row.HideRetryButton();
            row.HideDetailsButton();
            row.SetStatus("Queued");

            if (_cts == null || _cts.IsCancellationRequested)
            {
                _cts = new CancellationTokenSource();
            }

            _isInstalling = true;

            try
            {
                await InstallSingleRowAsync(row, _cts.Token);
            }
            catch (Exception)
            {
                // Handled in InstallSingleRowAsync
            }

            CheckOverallStatusAndTransition();
        }

        private void ShowDetails(EmulatorProgressRow row)
        {
            var res = row.LastResult;
            string emulatorName = row.Emulator.Name;
            string repository = "";
            string releaseVersion = "Unknown";
            string assetName = "None";
            string httpStatus = "N/A";
            string fileSize = "Unknown";
            string archivePath = "N/A";
            string destPath = "N/A";
            string failedStage = "N/A";
            string exceptionMsg = "";
            string logFilePath = EmulatorInstallDiagnosticsLogger.GetLogFilePath(row.OperationId);

            var definition = _definitionProvider.GetById(row.Emulator.Id);
            if (definition != null)
            {
                repository = $"{definition.GitHubOwner}/{definition.GitHubRepository}";
                destPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, definition.InstallDirectoryName));
            }

            if (res != null)
            {
                releaseVersion = res.Version ?? "Unknown";
                assetName = res.SelectedAssetName ?? "None";
                if (res.HttpStatusCode.HasValue)
                {
                    httpStatus = $"{(int)res.HttpStatusCode.Value} ({res.HttpStatusCode.Value})";
                }
                if (res.DownloadedFileSize.HasValue)
                {
                    double mb = (double)res.DownloadedFileSize.Value / (1024 * 1024);
                    fileSize = $"{mb:F2} MB ({res.DownloadedFileSize.Value} bytes)";
                }
                archivePath = res.ArchivePath ?? "N/A";
                failedStage = res.FailedStage.ToString();
                exceptionMsg = res.ErrorMessage ?? "";
                if (res.Exception != null)
                {
                    exceptionMsg += Environment.NewLine + res.Exception.ToString();
                }
            }
            else
            {
                failedStage = row.LastFailedStage.ToString();
                exceptionMsg = "Installation failed before result was recorded.";
            }

            using (var detailsForm = new InstallationDetailsForm(
                emulatorName, repository, releaseVersion, assetName, httpStatus, 
                fileSize, archivePath, destPath, failedStage, exceptionMsg, logFilePath))
            {
                detailsForm.ShowDialog(this);
            }
        }

        private void UpdateOverallProgress()
        {
            if (this.IsDisposed) return;
            if (_progressRows == null || !_progressRows.Any()) return;

            int sum = _progressRows.Sum(r => r.Percentage);
            _pbOverall.Value = Math.Min(100, Math.Max(0, sum / _progressRows.Count));
        }

        private void CheckOverallStatusAndTransition()
        {
            bool hasActive = _progressRows.Any(r => r.Percentage > 0 && r.Percentage < 100 && !r.IsRetryButtonVisible && r.CurrentStatus != "Cancelled");
            bool hasFailed = _progressRows.Any(r => r.IsRetryButtonVisible);
            bool allSuccess = _progressRows.All(r => r.Percentage == 100);

            if (allSuccess)
            {
                _isInstalling = false;
                var results = _progressRows.ToDictionary(r => r.Emulator.Name, r => "Installed successfully.");
                BuildSummary(results);
                LoadStep(3);
            }
            else if (!hasActive)
            {
                _isInstalling = false;
                _btnBack.Enabled = true;
                _btnNext.Enabled = false;
                _btnCancel.Enabled = true;
                _btnCancel.Text = "Cancel";

                if (hasFailed)
                {
                    _lblOverallStatus.Text = "Some installations failed. Please retry or go back.";
                }
                else
                {
                    _lblOverallStatus.Text = "Installation stopped.";
                }
            }
            else
            {
                _btnBack.Enabled = false;
                _btnNext.Enabled = false;
                _btnCancel.Enabled = true;
            }
        }

        private void BuildSummary(Dictionary<string, string> results)
        {
            _lstSummary.Items.Clear();
            bool allOk = true;

            foreach (var kvp in results)
            {
                _lstSummary.Items.Add($"• {kvp.Key}: {kvp.Value}");
                if (kvp.Value.Contains("failed") || kvp.Value.Contains("Failed")) allOk = false;
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
            public string OperationId { get; } = Guid.NewGuid().ToString("N");
            public Panel ContainerPanel { get; }
            private Label _lblName;
            private Label _lblStatus;
            private ProgressBar _pbItem;
            private Button _btnRetry;
            private Button _btnDetails;
            private Action? _retryAction;
            private Action? _detailsAction;

            public bool IsRetryButtonVisible => _btnRetry.Visible;
            public bool IsDetailsButtonVisible => _btnDetails.Visible;
            public string CurrentStatus => _lblStatus.Text;
            public PackageInstallResult? LastResult { get; private set; }
            public PackageInstallStage LastFailedStage { get; private set; }
            public int Percentage => _pbItem.Value;

            public EmulatorProgressRow(EmulatorItem emu, int yPosition)
            {
                Emulator = emu;

                ContainerPanel = new Panel
                {
                    Location = new Point(0, yPosition),
                    Size = new Size(580, 50),
                    BackColor = Color.FromArgb(24, 24, 28)
                };

                _lblName = new Label
                {
                    Text = emu.Name,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                    ForeColor = Color.White,
                    Location = new Point(10, 15),
                    Size = new Size(110, 20)
                };

                _lblStatus = new Label
                {
                    Text = "Queued",
                    Font = new Font("Segoe UI", 8.5F),
                    ForeColor = Color.FromArgb(156, 163, 175),
                    Location = new Point(125, 17),
                    Size = new Size(160, 20)
                };

                _pbItem = new ProgressBar
                {
                    Location = new Point(290, 15),
                    Size = new Size(130, 18),
                    Style = ProgressBarStyle.Continuous
                };

                _btnRetry = new Button
                {
                    Text = "Retry",
                    Size = new Size(65, 24),
                    Location = new Point(430, 13),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(16, 185, 129),
                    ForeColor = Color.White,
                    Visible = false
                };
                _btnRetry.Click += (s, e) => _retryAction?.Invoke();

                _btnDetails = new Button
                {
                    Text = "Details",
                    Size = new Size(65, 24),
                    Location = new Point(505, 13),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(31, 41, 55),
                    ForeColor = Color.White,
                    Visible = false
                };
                _btnDetails.Click += (s, e) => _detailsAction?.Invoke();

                ContainerPanel.Controls.AddRange(new Control[] { _lblName, _lblStatus, _pbItem, _btnRetry, _btnDetails });
            }

            public void SetProgress(int percent)
            {
                _pbItem.Value = Math.Min(100, Math.Max(0, percent));
            }

            public void SetStatus(string msg)
            {
                _lblStatus.Text = msg;
            }

            public void SetLastResult(PackageInstallResult result)
            {
                LastResult = result;
            }

            public void SetLastFailedStage(PackageInstallStage stage)
            {
                LastFailedStage = stage;
            }

            public void ShowRetryAndDetailsButtons(Action retryAction, Action detailsAction)
            {
                _retryAction = retryAction;
                _detailsAction = detailsAction;
                _btnRetry.Visible = true;
                _btnDetails.Visible = true;
                _lblStatus.ForeColor = Color.FromArgb(239, 68, 68); // Red
            }

            public void HideRetryButton()
            {
                _btnRetry.Visible = false;
                _lblStatus.ForeColor = Color.FromArgb(156, 163, 175);
            }

            public void HideDetailsButton()
            {
                _btnDetails.Visible = false;
            }
        }
    }

    public class InstallationDetailsForm : Form
    {
        public InstallationDetailsForm(string emulatorName, string repository, string releaseVersion, 
            string assetName, string httpStatus, string fileSize, string archivePath, 
            string destPath, string failedStage, string exceptionMsg, string logFilePath)
        {
            this.Text = $"{emulatorName} - Installation Failure Details";
            this.Size = new Size(550, 480);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;
            this.BackColor = Color.FromArgb(24, 24, 28);
            this.ForeColor = Color.White;

            Label lblTitle = new Label
            {
                Text = "Diagnostic Details",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Location = new Point(15, 15),
                Size = new Size(300, 25),
                ForeColor = Color.FromArgb(239, 68, 68)
            };
            this.Controls.Add(lblTitle);

            TextBox txtDetails = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9F),
                BackColor = Color.FromArgb(31, 41, 55),
                ForeColor = Color.White,
                Location = new Point(15, 50),
                Size = new Size(500, 320),
                BorderStyle = BorderStyle.FixedSingle
            };

            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Emulator Name:    {emulatorName}");
            sb.AppendLine($"Repository:       {repository}");
            sb.AppendLine($"Release Version:  {releaseVersion}");
            sb.AppendLine($"Asset Name:       {assetName}");
            sb.AppendLine($"HTTP Status:      {httpStatus}");
            sb.AppendLine($"Downloaded Size:  {fileSize}");
            sb.AppendLine($"Archive Path:     {archivePath}");
            sb.AppendLine($"Destination Path: {destPath}");
            sb.AppendLine($"Failed Stage:     {failedStage}");
            sb.AppendLine($"Log File Path:    {logFilePath}");
            sb.AppendLine();
            sb.AppendLine("Exception Message:");
            sb.AppendLine("------------------");
            sb.AppendLine(exceptionMsg);

            txtDetails.Text = sb.ToString();
            txtDetails.SelectionLength = 0;
            this.Controls.Add(txtDetails);

            Button btnOpenLog = new Button
            {
                Text = "Open log",
                Size = new Size(95, 30),
                Location = new Point(315, 390),
                FlatStyle = FlatStyle.Flat
            };
            btnOpenLog.Click += (s, e) =>
            {
                try
                {
                    if (File.Exists(logFilePath))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = logFilePath,
                            UseShellExecute = true
                        });
                    }
                    else
                    {
                        MessageBox.Show($"Log file does not exist yet at:\n\n{logFilePath}", "Log Not Found", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to open log file automatically.\n\nPath:\n{logFilePath}\n\nError:\n{ex.Message}", "Failed to Open Log", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            this.Controls.Add(btnOpenLog);

            Button btnClose = new Button
            {
                Text = "Close",
                DialogResult = DialogResult.OK,
                Size = new Size(95, 30),
                Location = new Point(420, 390),
                FlatStyle = FlatStyle.Flat
            };
            this.Controls.Add(btnClose);
            this.AcceptButton = btnClose;

            ThemeManager.Instance.ApplyTheme(this);
        }
    }
}
