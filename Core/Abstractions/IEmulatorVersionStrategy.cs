using System;

namespace RetroLauncher.Core.Abstractions
{
    public interface IEmulatorVersionStrategy
    {
        bool IsNewer(string installed, string available, DateTime? installedTime, DateTime? availableTime);
    }
}
