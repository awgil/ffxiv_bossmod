using System;
using System.IO;

namespace BossMod
{
    class Logger : IDisposable
    {
        private string _prefix;
        private DirectoryInfo _logDir;
        private StreamWriter? _logger = null;

        public bool Active => _logger != null;

        public Logger(string prefix, DirectoryInfo logDir)
        {
            _prefix = prefix;
            _logDir = logDir;
        }

        public void Dispose()
        {
            Deactivate();
        }

        public bool Activate(int version, DateTime startTime, string payload)
        {
            try
            {
                _logDir.Create();
                _logger = new StreamWriter($"{_logDir.FullName}/{_prefix}_{startTime:yyyy_MM_dd_HH_mm_ss}.log");
                Log(startTime, $"VER |{version}|{payload}");
                return true;
            }
            catch (IOException e)
            {
                Service.Log($"Failed to start {_prefix} logging: {e}");
                return false;
            }
        }

        public void Deactivate()
        {
            _logger?.Dispose();
            _logger = null;
        }

        public void Log(DateTime timestamp, string message)
        {
            if (_logger != null)
                _logger.WriteLine($"{timestamp:O}|{message}");
        }
    }
}
