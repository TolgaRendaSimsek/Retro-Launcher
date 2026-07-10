using System.Diagnostics;
using System.Threading.Tasks;

namespace RetroLauncher
{
    public interface IEmulatorAdapter
    {
        string EmulatorId { get; }
        bool IsInstalled();
        bool CanRun(Game game);
        string GetExecutablePath();
        ProcessStartInfo BuildLaunchCommand(Game game);
        Task<Process> LaunchGameAsync(Game game);
        bool ValidateGame(Game game);
        string GetSaveFolder(Game game);
        string GetScreenshotFolder(Game game);
    }
}
