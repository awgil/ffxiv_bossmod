using System;
using System.IO;

namespace BossMod
{
    class WorldStateLogger : IDisposable
    {
        private WorldState _ws;
        private GeneralConfig _config;
        private DirectoryInfo _logDir;
        private StreamWriter? _logger = null;

        public bool Active => _logger != null;

        public const int Version = 10;

        public WorldStateLogger(WorldState ws, DirectoryInfo logDir)
        {
            _ws = ws;
            _config = Service.Config.Get<GeneralConfig>();
            _logDir = logDir;

            _config.Modified += ApplyConfig;
        }

        public void Dispose()
        {
            _config.Modified -= ApplyConfig;
            Deactivate();
        }

        private void ApplyConfig(object? sender, EventArgs args)
        {
            if (_config.DumpWorldStateEvents)
                Activate();
            else
                Deactivate();
        }

        private void Activate()
        {
            if (!Active)
            {
                try
                {
                    _logDir.Create();
                    _logger = new StreamWriter($"{_logDir.FullName}/World_{_ws.CurrentTime:yyyy_MM_dd_HH_mm_ss}.log");
                }
                catch (IOException e)
                {
                    Service.Log($"Failed to start logging: {e}");
                    _config.DumpWorldStateEvents = false;
                    return;
                }

                // log initial state
                Log($"VER |{Version}|{_ws.QPF}");
                foreach (var op in _ws.CompareToInitial())
                    Log(op.Str(_ws));

                // log changes
                _ws.Modified += LogModification;
            }
        }

        private void Deactivate()
        {
            if (Active)
            {
                _ws.Modified -= LogModification;
                _logger?.Dispose();
                _logger = null;
            }
        }

        private void LogModification(object? sender, WorldState.Operation op) => Log(op.Str(_ws));
        private void Log(string message) => _logger?.WriteLine($"{_ws.CurrentTime:O}|{message}");
    }
}
