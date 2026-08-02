using System;
using System.Collections.Generic;
using System.Linq;

namespace RetroLauncher.Emulators.Registry
{
    public static class EmulatorAdapterRegistry
    {
        private static readonly List<IEmulatorAdapter> _adapters = new()
        {
            new DuckStationAdapter(),
            new PCSX2Adapter(),
            new RPCS3Adapter(),
            new DolphinAdapter(),
            new PPSSPPAdapter()
        };

        public static IReadOnlyList<IEmulatorAdapter> GetAllAdapters() => _adapters.AsReadOnly();

        public static IEmulatorAdapter GetAdapter(Game game)
        {
            // 1. Resolve by direct CanRun (Platform matching)
            var adapter = _adapters.FirstOrDefault(a => a.CanRun(game));
            if (adapter != null) return adapter;

            // 2. Resolve by EmulatorId configuration matching
            if (!string.IsNullOrEmpty(game.EmulatorId))
            {
                var cleanId = System.IO.Path.GetFileNameWithoutExtension(game.EmulatorId).ToLower();
                adapter = _adapters.FirstOrDefault(a => 
                    string.Equals(a.EmulatorId, game.EmulatorId, StringComparison.OrdinalIgnoreCase) ||
                    cleanId.Contains(a.EmulatorId.ToLower())
                );
                if (adapter != null) return adapter;
            }

            // 3. Fallback to generic adapter
            return new GenericEmulatorAdapter(game.EmulatorId);
        }

        public static IEmulatorAdapter? GetAdapterByEmulatorId(string emulatorId)
        {
            if (string.IsNullOrEmpty(emulatorId)) return null;
            var cleanId = System.IO.Path.GetFileNameWithoutExtension(emulatorId).ToLower();
            
            var adapter = _adapters.FirstOrDefault(a => 
                string.Equals(a.EmulatorId, emulatorId, StringComparison.OrdinalIgnoreCase) ||
                cleanId.Contains(a.EmulatorId.ToLower())
            );
            
            return adapter ?? new GenericEmulatorAdapter(emulatorId);
        }
    }
}
