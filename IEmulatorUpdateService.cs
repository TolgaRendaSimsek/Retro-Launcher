using System.Threading;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public interface IEmulatorUpdateService
    {
        Task<EmulatorUpdateInfo> CheckForUpdateAsync(string emulatorId, EmulatorReleaseChannel channel, CancellationToken cancellationToken);
    }
}
