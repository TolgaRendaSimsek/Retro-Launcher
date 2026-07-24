using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public enum VerificationStatus
    {
        SizeVerified,
        LocalHashRecorded,
        OfficialChecksumVerified,
        SignatureVerified,
        VerificationUnavailable,
        VerificationFailed
    }

    public enum ChecksumSource
    {
        None,
        GitHubAssetMetadata,
        ReleaseManifestFile,
        EmbeddedSignature
    }

    public class VerificationResult
    {
        public bool Success { get; set; }
        public VerificationStatus Status { get; set; } = VerificationStatus.VerificationUnavailable;
        public string? CalculatedHash { get; set; }
        public string? ExpectedHash { get; set; }
        public string Message { get; set; } = "";
    }

    public interface IEmuPackageVerifier
    {
        Task<VerificationResult> VerifyPackageAsync(
            string packagePath,
            long expectedSize,
            string? expectedHash,
            CancellationToken cancellationToken);
    }

    public static class LocalHashCalculator
    {
        public static string CalculateSHA256(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }
    }

    public class EmuPackageVerifier : IEmuPackageVerifier
    {
        public Task<VerificationResult> VerifyPackageAsync(
            string packagePath,
            long expectedSize,
            string? expectedHash,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!File.Exists(packagePath))
            {
                return Task.FromResult(new VerificationResult
                {
                    Success = false,
                    Status = VerificationStatus.VerificationFailed,
                    Message = "Package file not found."
                });
            }

            long actualSize = new FileInfo(packagePath).Length;
            if (expectedSize > 0 && actualSize != expectedSize)
            {
                return Task.FromResult(new VerificationResult
                {
                    Success = false,
                    Status = VerificationStatus.VerificationFailed,
                    Message = $"Size mismatch. Expected {expectedSize} bytes, but got {actualSize}."
                });
            }

            string localHash;
            try
            {
                localHash = LocalHashCalculator.CalculateSHA256(packagePath);
            }
            catch (Exception ex)
            {
                return Task.FromResult(new VerificationResult
                {
                    Success = false,
                    Status = VerificationStatus.VerificationFailed,
                    Message = $"Failed to calculate SHA256 hash: {ex.Message}"
                });
            }

            if (!string.IsNullOrEmpty(expectedHash))
            {
                bool isMatch = string.Equals(localHash, expectedHash, StringComparison.OrdinalIgnoreCase);
                if (isMatch)
                {
                    return Task.FromResult(new VerificationResult
                    {
                        Success = true,
                        Status = VerificationStatus.OfficialChecksumVerified,
                        CalculatedHash = localHash,
                        ExpectedHash = expectedHash,
                        Message = "Official checksum verified successfully."
                    });
                }
                else
                {
                    return Task.FromResult(new VerificationResult
                    {
                        Success = false,
                        Status = VerificationStatus.VerificationFailed,
                        CalculatedHash = localHash,
                        ExpectedHash = expectedHash,
                        Message = "Official checksum verification failed: hash mismatch."
                    });
                }
            }

            return Task.FromResult(new VerificationResult
            {
                Success = true,
                Status = VerificationStatus.LocalHashRecorded,
                CalculatedHash = localHash,
                Message = "Package size verified and local hash recorded."
            });
        }
    }
}
