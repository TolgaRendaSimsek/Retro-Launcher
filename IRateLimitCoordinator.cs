using System;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public interface IAsyncDelay
    {
        Task DelayAsync(TimeSpan duration, CancellationToken cancellationToken);
    }

    public class DefaultAsyncDelay : IAsyncDelay
    {
        public Task DelayAsync(TimeSpan duration, CancellationToken cancellationToken)
        {
            return Task.Delay(duration, cancellationToken);
        }
    }

    public class RateLimitState
    {
        public int Limit { get; set; } = 60;
        public int Remaining { get; set; } = 60;
        public DateTime ResetTime { get; set; } = DateTime.MinValue;
        public bool IsRateLimited => Remaining == 0 && DateTime.UtcNow < ResetTime;
    }

    public interface IRateLimitCoordinator
    {
        RateLimitState GetState();
        void UpdateState(int limit, int remaining, DateTime resetTime);
        Task WaitIfNeededAsync(CancellationToken cancellationToken);
        Task<T> CoordinateRequestAsync<T>(string key, Func<Task<T>> requestFunc);
    }
}
