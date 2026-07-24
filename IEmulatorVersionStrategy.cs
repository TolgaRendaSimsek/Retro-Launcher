using System;

namespace RetroLauncher
{
    public interface IEmulatorVersionStrategy
    {
        bool IsNewer(string installed, string available, DateTime? installedTime, DateTime? availableTime);
    }
}
