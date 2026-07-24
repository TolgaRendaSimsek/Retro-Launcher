using System;
using System.Net.Http;
using System.Net.Sockets;

namespace RetroLauncher
{
    public static class NetworkFailureMapper
    {
        public static OperationError MapException(Exception ex)
        {
            if (ex is OperationCanceledException)
            {
                return new OperationError
                {
                    Message = "The network operation was cancelled or timed out.",
                    Category = ErrorCategory.Network,
                    Exception = ex
                };
            }

            if (ex is HttpRequestException httpEx)
            {
                var inner = httpEx.InnerException;
                if (inner is SocketException socketEx)
                {
                    if (socketEx.SocketErrorCode == SocketError.HostNotFound || socketEx.SocketErrorCode == SocketError.AddressNotAvailable)
                    {
                        return new OperationError
                        {
                            Message = "DNS resolution failure: Host not found.",
                            Category = ErrorCategory.Network,
                            Exception = ex
                        };
                    }
                }
                
                if (ex.Message.Contains("SSL", StringComparison.OrdinalIgnoreCase) || 
                    ex.Message.Contains("TLS", StringComparison.OrdinalIgnoreCase) || 
                    ex.Message.Contains("certificate", StringComparison.OrdinalIgnoreCase))
                {
                    return new OperationError
                    {
                        Message = "TLS handshake or certificate verification failed.",
                        Category = ErrorCategory.Network,
                        Exception = ex
                    };
                }

                return new OperationError
                {
                    Message = $"Network request failed: {httpEx.Message}",
                    Category = ErrorCategory.Network,
                    Exception = ex
                };
            }

            return new OperationError
            {
                Message = $"An unexpected network error occurred: {ex.Message}",
                Category = ErrorCategory.Internal,
                Exception = ex
            };
        }
    }
}
