using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace RetroLauncher
{
    public static class HttpPackageDownloaderTests
    {
        public static async Task RunTestsAsync()
        {
            RetroLogger.Log("Starting HttpPackageDownloader Unit Tests...");

            await TestValidArchiveDownloadAsync();
            await TestInterruptedStreamAsync();
            await TestWrongContentLengthAsync();
            await TestHtmlResponseRejectedAsync();
            await Test404ResponseAsync();
            await Test500FollowedBySuccessAsync();
            await TestCancellationAsync();

            RetroLogger.Log("All HttpPackageDownloader Unit Tests completed successfully!");
        }

        private static async Task TestValidArchiveDownloadAsync()
        {
            var validZipBytes = new byte[] { 0x50, 0x4B, 0x03, 0x04, 0, 0, 0, 0, 0, 0 };
            
            var handler = new MockHttpMessageHandler
            {
                SendAsyncFunc = (req, token) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(validZipBytes)
                    };
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
                    response.Content.Headers.ContentLength = validZipBytes.Length;
                    return Task.FromResult(response);
                }
            };

            using (var client = new HttpClient(handler))
            {
                var downloader = new HttpPackageDownloader(null, client, 1);
                string packageId = "test_valid_dl";
                string operationId = Guid.NewGuid().ToString("N");
                string assetName = "emulator.zip";

                string resultFile = await downloader.DownloadAsync("https://github.com/test/emu/releases/download/v1/emu.zip", null, CancellationToken.None, packageId, operationId, assetName);

                Debug.Assert(File.Exists(resultFile), "Download result file must exist.");
                Debug.Assert(new FileInfo(resultFile).Length == validZipBytes.Length, "Downloaded file length must match input.");
                
                // Cleanup
                try { Directory.Delete(Path.GetDirectoryName(resultFile)!, true); } catch { }
            }
            RetroLogger.Log("Test Case: Valid archive download passed.");
        }

        private static async Task TestInterruptedStreamAsync()
        {
            var handler = new MockHttpMessageHandler
            {
                SendAsyncFunc = (req, token) =>
                {
                    // Return a stream that throws IOException during read
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StreamContent(new InterruptedStream())
                    };
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
                    response.Content.Headers.ContentLength = 100;
                    return Task.FromResult(response);
                }
            };

            using (var client = new HttpClient(handler))
            {
                var downloader = new HttpPackageDownloader(null, client, 1);
                string packageId = "test_interrupted";
                string operationId = Guid.NewGuid().ToString("N");
                string assetName = "emu.zip";

                bool failed = false;
                try
                {
                    await downloader.DownloadAsync("https://github.com/test/emu.zip", null, CancellationToken.None, packageId, operationId, assetName);
                }
                catch (IOException)
                {
                    failed = true;
                }
                catch (Exception ex)
                {
                    failed = true;
                    RetroLogger.Log($"Note: Stream interrupted caught {ex.GetType().Name}: {ex.Message}");
                }

                Debug.Assert(failed, "Download should fail when the stream is interrupted.");
                
                // Verify part file cleanup
                string downloadsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", "downloads", packageId, operationId);
                string partFile = Path.Combine(downloadsDir, $"{assetName}.part");
                Debug.Assert(!File.Exists(partFile), "Part file must be cleaned up on failure.");
                
                try { Directory.Delete(downloadsDir, true); } catch { }
            }
            RetroLogger.Log("Test Case: Interrupted stream handled cleanly passed.");
        }

        private static async Task TestWrongContentLengthAsync()
        {
            var zipBytes = new byte[] { 0x50, 0x4B, 0x03, 0x04, 0, 0, 0, 0, 0, 0 };
            var handler = new MockHttpMessageHandler
            {
                SendAsyncFunc = (req, token) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(zipBytes)
                    };
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
                    response.Content.Headers.ContentLength = 999; // Set mismatched length
                    return Task.FromResult(response);
                }
            };

            using (var client = new HttpClient(handler))
            {
                var downloader = new HttpPackageDownloader(null, client, 1);
                string packageId = "test_len_mismatch";
                string operationId = Guid.NewGuid().ToString("N");
                string assetName = "emu.zip";

                bool failed = false;
                try
                {
                    await downloader.DownloadAsync("https://github.com/test/emu.zip", null, CancellationToken.None, packageId, operationId, assetName);
                }
                catch (InvalidDataException)
                {
                    failed = true;
                }

                Debug.Assert(failed, "Download should fail when Content-Length does not match downloaded byte count.");
                
                string downloadsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", "downloads", packageId, operationId);
                Debug.Assert(!File.Exists(Path.Combine(downloadsDir, $"{assetName}.part")), "Part file must be cleaned up.");
                try { Directory.Delete(downloadsDir, true); } catch { }
            }
            RetroLogger.Log("Test Case: Wrong Content-Length mismatch rejected passed.");
        }

        private static async Task TestHtmlResponseRejectedAsync()
        {
            string htmlContent = "<!DOCTYPE html><html><body><h1>Error page</h1></body></html>";
            var handler = new MockHttpMessageHandler
            {
                SendAsyncFunc = (req, token) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent(htmlContent, Encoding.UTF8, "text/html")
                    };
                    return Task.FromResult(response);
                }
            };

            using (var client = new HttpClient(handler))
            {
                var downloader = new HttpPackageDownloader(null, client, 1);
                string packageId = "test_html_reject";
                string operationId = Guid.NewGuid().ToString("N");
                string assetName = "emu.zip";

                bool failed = false;
                try
                {
                    await downloader.DownloadAsync("https://github.com/test/emu.zip", null, CancellationToken.None, packageId, operationId, assetName);
                }
                catch (InvalidDataException)
                {
                    failed = true;
                }

                Debug.Assert(failed, "Download should fail immediately if the response is an HTML page.");
                
                string downloadsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", "downloads", packageId, operationId);
                Debug.Assert(!File.Exists(Path.Combine(downloadsDir, $"{assetName}.part")), "Part file must be cleaned up.");
                try { Directory.Delete(downloadsDir, true); } catch { }
            }
            RetroLogger.Log("Test Case: HTML error page rejection passed.");
        }

        private static async Task Test404ResponseAsync()
        {
            int callCount = 0;
            var handler = new MockHttpMessageHandler
            {
                SendAsyncFunc = (req, token) =>
                {
                    callCount++;
                    var response = new HttpResponseMessage(HttpStatusCode.NotFound);
                    return Task.FromResult(response);
                }
            };

            using (var client = new HttpClient(handler))
            {
                var downloader = new HttpPackageDownloader(null, client, 1);
                string packageId = "test_404";
                string operationId = Guid.NewGuid().ToString("N");
                string assetName = "emu.zip";

                bool failed = false;
                try
                {
                    await downloader.DownloadAsync("https://github.com/test/emu.zip", null, CancellationToken.None, packageId, operationId, assetName);
                }
                catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    failed = true;
                }

                Debug.Assert(failed, "Should fail with HTTP 404 Exception.");
                Debug.Assert(callCount == 1, "Should not retry on fatal 404 response.");
            }
            RetroLogger.Log("Test Case: 404 failure without retrying passed.");
        }

        private static async Task Test500FollowedBySuccessAsync()
        {
            int callCount = 0;
            var validZipBytes = new byte[] { 0x50, 0x4B, 0x03, 0x04, 0, 0, 0, 0, 0, 0 };

            var handler = new MockHttpMessageHandler
            {
                SendAsyncFunc = (req, token) =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError));
                    }
                    var response = new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new ByteArrayContent(validZipBytes)
                    };
                    response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
                    response.Content.Headers.ContentLength = validZipBytes.Length;
                    return Task.FromResult(response);
                }
            };

            using (var client = new HttpClient(handler))
            {
                var downloader = new HttpPackageDownloader(null, client, 1);
                string packageId = "test_retry_success";
                string operationId = Guid.NewGuid().ToString("N");
                string assetName = "emu.zip";

                string resultFile = await downloader.DownloadAsync("https://github.com/test/emu.zip", null, CancellationToken.None, packageId, operationId, assetName);

                Debug.Assert(File.Exists(resultFile), "Download must succeed after transient retry.");
                Debug.Assert(callCount == 2, "Should attempt download exactly twice.");
                
                try { Directory.Delete(Path.GetDirectoryName(resultFile)!, true); } catch { }
            }
            RetroLogger.Log("Test Case: 500 error followed by success passed.");
        }

        private static async Task TestCancellationAsync()
        {
            var cts = new CancellationTokenSource();
            cts.Cancel(); // Pre-cancel

            var handler = new MockHttpMessageHandler
            {
                SendAsyncFunc = (req, token) =>
                {
                    var response = new HttpResponseMessage(HttpStatusCode.OK);
                    return Task.FromResult(response);
                }
            };

            using (var client = new HttpClient(handler))
            {
                var downloader = new HttpPackageDownloader(null, client, 1);
                string packageId = "test_cancel";
                string operationId = Guid.NewGuid().ToString("N");
                string assetName = "emu.zip";

                bool failed = false;
                try
                {
                    await downloader.DownloadAsync("https://github.com/test/emu.zip", null, cts.Token, packageId, operationId, assetName);
                }
                catch (OperationCanceledException)
                {
                    failed = true;
                }

                Debug.Assert(failed, "Download should throw OperationCanceledException.");
                
                string downloadsDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp", "downloads", packageId, operationId);
                Debug.Assert(!File.Exists(Path.Combine(downloadsDir, $"{assetName}.part")), "Part file must be cleaned up on cancellation.");
                try { Directory.Delete(downloadsDir, true); } catch { }
            }
            RetroLogger.Log("Test Case: Cancellation handled cleanly passed.");
        }

        // Mock HttpMessageHandler Helper
        private class MockHttpMessageHandler : HttpMessageHandler
        {
            public Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> SendAsyncFunc { get; set; } = null!;

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                return SendAsyncFunc(request, cancellationToken);
            }
        }

        // Interrupted Stream helper
        private class InterruptedStream : Stream
        {
            private int _reads = 0;

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => 100;
            public override long Position { get => 0; set { } }

            public override int Read(byte[] buffer, int offset, int count)
            {
                _reads++;
                if (_reads > 1)
                {
                    throw new IOException("Simulated network stream interruption!");
                }
                // Write a mock ZIP header first so it doesn't fail signature check before network failure
                buffer[offset] = 0x50;
                buffer[offset + 1] = 0x4B;
                buffer[offset + 2] = 0x03;
                buffer[offset + 3] = 0x04;
                return 4;
            }

            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }
    }
}
