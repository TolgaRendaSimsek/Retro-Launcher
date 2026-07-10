using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RetroLauncher
{
    public partial class UpdateProgressForm : Form
    {
        private readonly UpdateInfo _updateInfo;
        private CancellationTokenSource? _cts;

        public UpdateProgressForm(UpdateInfo updateInfo)
        {
            InitializeComponent();
            _updateInfo = updateInfo;
            
            this.Load += UpdateProgressForm_Load;
            this.FormClosing += UpdateProgressForm_FormClosing;
            btnCancel.Click += (s, e) => CancelDownload();
        }

        private async void UpdateProgressForm_Load(object? sender, EventArgs e)
        {
            _cts = new CancellationTokenSource();
            await StartDownloadAsync();
        }

        private void UpdateProgressForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            CancelDownload();
        }

        private void CancelDownload()
        {
            if (_cts != null && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _cts.Dispose();
                _cts = null;
                lblStatus.Text = "Download cancelled.";
            }
        }

        private async Task StartDownloadAsync()
        {
            string tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            string zipPath = Path.Combine(tempDir, "update.zip");

            try
            {
                // In mock mode, if download URL is local or mock, we can copy the mock archive directly to simulate!
                // Let's check if the DownloadUrl is a file path in our workspace or if it matches local mock file.
                if (_updateInfo.DownloadUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.UserAgent.ParseAdd("RetroLauncher/1.0");
                        
                        using (var response = await client.GetAsync(_updateInfo.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, _cts!.Token))
                        {
                            response.EnsureSuccessStatusCode();
                            long? totalBytes = response.Content.Headers.ContentLength;

                            using (var contentStream = await response.Content.ReadAsStreamAsync(_cts.Token))
                            using (var fileStream = new FileStream(zipPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                            {
                                var buffer = new byte[8192];
                                long totalRead = 0;
                                int bytesRead;

                                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, _cts.Token)) > 0)
                                {
                                    await fileStream.WriteAsync(buffer, 0, bytesRead, _cts.Token);
                                    totalRead += bytesRead;

                                    if (totalBytes.HasValue)
                                    {
                                        int progress = (int)((double)totalRead / totalBytes.Value * 100);
                                        pbDownload.Value = Math.Min(100, Math.Max(0, progress));
                                        lblStatus.Text = $"Downloading update... ({progress}%)";
                                    }
                                }
                            }
                        }
                    }
                }
                else if (File.Exists(_updateInfo.DownloadUrl))
                {
                    // Local file mock download simulation
                    lblStatus.Text = "Simulating download from local package...";
                    File.Copy(_updateInfo.DownloadUrl, zipPath, true);
                    pbDownload.Value = 100;
                    await Task.Delay(1000); // Wait 1 second to show progress
                }
                else
                {
                    // Simulated empty mock update zip package for demo/test purposes if nothing else works
                    lblStatus.Text = "Creating mock update package...";
                    // We can generate a simple zip or copy a mock
                    await Task.Delay(1000);
                    pbDownload.Value = 100;
                }

                // Verify file exists
                if (!File.Exists(zipPath) || new FileInfo(zipPath).Length == 0)
                {
                    throw new FileNotFoundException("Downloaded update package is invalid or empty.");
                }

                // Close and start updater
                lblStatus.Text = "Launching updater utility...";
                await Task.Delay(500);

                string updaterFileName = "RetroLauncherUpdater.exe";
                string updaterPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, updaterFileName);

                if (!File.Exists(updaterPath))
                {
                    // Try looking in subfolders or parent
                    string altPath = Path.Combine(Directory.GetCurrentDirectory(), updaterFileName);
                    if (File.Exists(altPath))
                    {
                        updaterPath = altPath;
                    }
                }

                if (!File.Exists(updaterPath))
                {
                    MessageBox.Show($"Update utility '{updaterFileName}' was not found at:\n{updaterPath}\n\nUnable to apply the update.", 
                        "Updater Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.Close();
                    return;
                }

                int currentPid = Process.GetCurrentProcess().Id;
                string targetDir = AppDomain.CurrentDomain.BaseDirectory;

                // Spawn external updater process
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = updaterPath,
                    Arguments = $"--pid {currentPid} --zip \"{zipPath}\" --target \"{targetDir}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false // Let console show up to show progress/logs to user
                };

                Process.Start(psi);

                // Exit launcher safely
                Application.Exit();
            }
            catch (OperationCanceledException)
            {
                // Silently handle cancel
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to download system update.\n\nError: {ex.Message}", 
                    "Update Download Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close();
            }
        }
    }
}
