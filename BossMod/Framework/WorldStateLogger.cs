using System;
using System.IO;
using System.Numerics;
using System.Text;

namespace BossMod
{
    class WorldStateLogger : IDisposable
    {
        private WorldState _ws;
        private GeneralConfig _config;
        private Logger _logger;

        public WorldStateLogger(WorldState ws, DirectoryInfo logDir)
        {
            _ws = ws;
            _config = Service.Config.Get<GeneralConfig>();
            _logger = new("World", logDir);

            _config.Modified += ApplyConfig;
        }

        public void Dispose()
        {
            _config.Modified -= ApplyConfig;
            Unsubscribe();
        }

        private void ApplyConfig(object? sender, EventArgs args)
        {
            if (_config.DumpWorldStateEvents)
                Subscribe();
            else
                Unsubscribe();
        }

        private void Subscribe()
        {
            if (!_logger.Active)
            {
                if (!_logger.Activate(9))
                {
                    _config.DumpWorldStateEvents = false;
                    return;
                }

                // log initial state
                foreach (var op in _ws.CompareToInitial())
                    LogModification(null, op);

                // log changes
                _ws.Modified += LogModification;
            }
        }

        private void Unsubscribe()
        {
            if (_logger.Active)
            {
                _ws.Modified -= LogModification;
                _logger.Deactivate();
            }
        }

        private void LogModification(object? sender, WorldState.Operation op)
        {
            _logger.Log(_ws.CurrentTime, op.Str(_ws));
        }
    }
}
