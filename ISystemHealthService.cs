using System;
using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public interface ISystemHealthService
    {
        Task<HealthCheckResult> RunHealthCheckAsync(IProgress<int>? progress, CancellationToken cancellationToken);
        Task<bool> ExecuteFixAsync(HealthCheckItem item, CancellationToken cancellationToken);
    }
}
