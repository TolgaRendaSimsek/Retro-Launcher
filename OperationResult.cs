using System;

namespace RetroLauncher
{
    public enum ErrorCategory
    {
        None,
        Network,
        RateLimit,
        NotFound,
        Unauthorized,
        Parser,
        Cache,
        Validation,
        Internal
    }

    public class OperationError
    {
        public string Message { get; set; } = "";
        public ErrorCategory Category { get; set; } = ErrorCategory.None;
        public Exception? Exception { get; set; }
    }

    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public OperationError? Error { get; set; }
        public bool IsValidatedFromCache { get; set; }

        public static OperationResult<T> Ok(T data, bool isFromCache = false)
        {
            return new OperationResult<T> { Success = true, Data = data, IsValidatedFromCache = isFromCache };
        }

        public static OperationResult<T> Fail(string message, ErrorCategory category, Exception? ex = null)
        {
            return new OperationResult<T>
            {
                Success = false,
                Error = new OperationError { Message = message, Category = category, Exception = ex }
            };
        }
    }
}
