using System;
using System.Collections.Generic;

namespace RetroLauncher.Core.Abstractions
{
    public interface IApplicationSettingsService
    {
        NetworkSettings Network { get; }
        GitHubSettings GitHub { get; }
        CacheSettings Cache { get; }
        DownloadSettings Download { get; }
        InstallationSettings Installation { get; }
        
        void SaveSettings();
        List<string> ValidateSettings();
    }

    public class NetworkSettings
    {
        public string ProxyMode { get; set; } = "SystemDefault"; // "SystemDefault", "NoProxy", "ManualProxy"
        public string? ProxyUri { get; set; }
        public string? ProxyUsername { get; set; }
        public string? EncryptedProxyPassword { get; set; }
        public bool BypassLocalAddresses { get; set; } = true;
        public List<string> BypassList { get; set; } = new();
        public int RequestTimeoutSeconds { get; set; } = 30;
        public int MaxRetryCount { get; set; } = 3;

        private string? _decryptedProxyPassword;
        private bool _isProxyPasswordLoaded;

        public string? GetProxyPassword()
        {
            if (_isProxyPasswordLoaded) return _decryptedProxyPassword;
            if (!string.IsNullOrEmpty(EncryptedProxyPassword))
            {
                try
                {
                    byte[] encryptedBytes = Convert.FromBase64String(EncryptedProxyPassword);
                    byte[] decryptedBytes = System.Security.Cryptography.ProtectedData.Unprotect(
                        encryptedBytes,
                        null,
                        System.Security.Cryptography.DataProtectionScope.CurrentUser);
                    _decryptedProxyPassword = System.Text.Encoding.UTF8.GetString(decryptedBytes);
                }
                catch (Exception ex)
                {
                    RetroLogger.Log($"Failed to decrypt proxy password: {ex.Message}", "WARNING");
                }
            }
            _isProxyPasswordLoaded = true;
            return _decryptedProxyPassword;
        }

        public void SetProxyPassword(string? password)
        {
            _decryptedProxyPassword = password;
            _isProxyPasswordLoaded = true;

            if (string.IsNullOrEmpty(password))
            {
                EncryptedProxyPassword = null;
            }
            else
            {
                try
                {
                    byte[] plainBytes = System.Text.Encoding.UTF8.GetBytes(password);
                    byte[] encryptedBytes = System.Security.Cryptography.ProtectedData.Protect(
                        plainBytes,
                        null,
                        System.Security.Cryptography.DataProtectionScope.CurrentUser);
                    EncryptedProxyPassword = Convert.ToBase64String(encryptedBytes);
                }
                catch (Exception ex)
                {
                    RetroLogger.Log($"Failed to encrypt proxy password: {ex.Message}", "ERROR");
                    throw;
                }
            }
        }
    }

    public class GitHubSettings
    {
        public string BaseUrl { get; set; } = "https://api.github.com";
        public string? EncryptedToken { get; set; } // stored in JSON
        public int RequestTimeoutSeconds { get; set; } = 15;

        private string? _decryptedToken;
        private bool _isTokenLoaded;

        public string? GetToken()
        {
            if (_isTokenLoaded) return _decryptedToken;

            // 1. Environment variable override
            string? envToken = Environment.GetEnvironmentVariable("RETRO_LAUNCHER_GITHUB_TOKEN");
            if (!string.IsNullOrEmpty(envToken))
            {
                _decryptedToken = envToken;
                _isTokenLoaded = true;
                return _decryptedToken;
            }

            // 2. Decrypt stored token using DPAPI
            if (!string.IsNullOrEmpty(EncryptedToken))
            {
                try
                {
                    byte[] encryptedBytes = Convert.FromBase64String(EncryptedToken);
                    byte[] decryptedBytes = System.Security.Cryptography.ProtectedData.Unprotect(
                        encryptedBytes,
                        null,
                        System.Security.Cryptography.DataProtectionScope.CurrentUser);
                    _decryptedToken = System.Text.Encoding.UTF8.GetString(decryptedBytes);
                }
                catch (Exception ex)
                {
                    RetroLogger.Log($"Failed to decrypt GitHub token: {ex.Message}", "WARNING");
                }
            }

            _isTokenLoaded = true;
            return _decryptedToken;
        }

        public void SetToken(string? token)
        {
            _decryptedToken = token;
            _isTokenLoaded = true;

            if (string.IsNullOrEmpty(token))
            {
                EncryptedToken = null;
            }
            else
            {
                try
                {
                    byte[] plainBytes = System.Text.Encoding.UTF8.GetBytes(token);
                    byte[] encryptedBytes = System.Security.Cryptography.ProtectedData.Protect(
                        plainBytes,
                        null,
                        System.Security.Cryptography.DataProtectionScope.CurrentUser);
                    EncryptedToken = Convert.ToBase64String(encryptedBytes);
                }
                catch (Exception ex)
                {
                    RetroLogger.Log($"Failed to encrypt GitHub token: {ex.Message}", "ERROR");
                    throw;
                }
            }
        }
    }

    public class CacheSettings
    {
        public int CacheDurationMinutes { get; set; } = 10;
    }

    public class DownloadSettings
    {
        public int MaxParallelDownloads { get; set; } = 2;
        public string DownloadTempDir { get; set; } = "temp";
    }

    public class InstallationSettings
    {
        public string EmulatorInstallationRoot { get; set; } = "Emulators";
        public string DefaultReleaseChannel { get; set; } = "Stable";
    }
}
