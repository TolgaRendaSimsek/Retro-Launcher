using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher.Infrastructure.Http
{
    public class RateLimitCoordinator : IRateLimitCoordinator
    {
        private static RateLimitCoordinator? _instance;
        public static RateLimitCoordinator Instance => _instance ??= new RateLimitCoordinator();

        private readonly RateLimitState _state = new();
        private readonly object _stateLock = new();
        private readonly ConcurrentDictionary<string, Task> _inFlightRequests = new();
        
        public IAsyncDelay DelayProvider { get; set; } = new DefaultAsyncDelay();

        public RateLimitState GetState()
        {
            lock (_stateLock)
            {
                return new RateLimitState
                {
                    Limit = _state.Limit,
                    Remaining = _state.Remaining,
                    ResetTime = _state.ResetTime
                };
            }
        }

        public void UpdateState(int limit, int remaining, DateTime resetTime)
        {
            lock (_stateLock)
            {
                _state.Limit = limit;
                _state.Remaining = remaining;
                _state.ResetTime = resetTime;
            }
        }

        public async Task WaitIfNeededAsync(CancellationToken cancellationToken)
        {
            DateTime resetTime;
            lock (_stateLock)
            {
                if (!_state.IsRateLimited) return;
                resetTime = _state.ResetTime;
            }

            TimeSpan waitDelay = resetTime - DateTime.UtcNow;
            if (waitDelay > TimeSpan.FromSeconds(30))
            {
                throw new InvalidOperationException($"GitHub API primary rate limit exceeded. Retry after {resetTime.ToLocalTime()}.");
            }

            if (waitDelay > TimeSpan.Zero)
            {
                RetroLogger.Log($"Rate limit reached. Waiting for {waitDelay.TotalSeconds:F1} seconds until reset...", "WARNING");
                await DelayProvider.DelayAsync(waitDelay, cancellationToken);
            }
        }

        public async Task<T> CoordinateRequestAsync<T>(string key, Func<Task<T>> requestFunc)
        {
            var task = _inFlightRequests.GetOrAdd(key, _ => Task.Run(requestFunc));
            try
            {
                var result = await (Task<T>)task;
                return result;
            }
            finally
            {
                _inFlightRequests.TryRemove(key, out _);
            }
        }
    }
}
