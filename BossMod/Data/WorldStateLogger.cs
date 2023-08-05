using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace BossMod
{
    public class WorldStateLogger : IDisposable
    {
        public abstract class Output : IDisposable
        {
            public abstract void Dispose();
            public abstract void StartEntry(DateTime t);
            public abstract void EndEntry();
            public abstract void Flush();
            public abstract Output Emit();
            public abstract Output Emit(string v);
            public abstract Output Emit(float v, string format = "g9");
            public abstract Output Emit(double v, string format = "g17");
            public abstract Output Emit(Vector3 v);
            public abstract Output Emit(Angle v);
            public abstract Output Emit(bool v);
            public abstract Output Emit(sbyte v);
            public abstract Output Emit(short v);
            public abstract Output Emit(int v);
            public abstract Output Emit(long v);
            public abstract Output Emit(byte v, string format = "d");
            public abstract Output Emit(ushort v, string format = "d");
            public abstract Output Emit(uint v, string format = "d");
            public abstract Output Emit(ulong v, string format = "d");
            public abstract Output Emit(ActionID v);
            public abstract Output Emit(Class v);
            public abstract Output Emit(ActorStatus v);
            public abstract Output Emit(List<ActorCastEvent.Target> v);
            public abstract Output EmitTimePair(float t1, float t2);
            public abstract Output EmitTimePair(DateTime t1, float t2);
            public abstract Output EmitActor(ulong instanceID);
        }

        public class TextOutput : Output
        {
            private StreamWriter _dest;
            private DateTime _curEntry;
            private ActorState? _actorLookup;

            public TextOutput(Stream dest, ActorState? actorLookup)
            {
                _dest = new(dest);
                _actorLookup = actorLookup;
            }

            public override void Dispose()
            {
                _dest.Dispose();
            }

            public override void StartEntry(DateTime t)
            {
                _curEntry = t;
                _dest.Write(t.ToString("O"));
            }

            public override void EndEntry()
            {
                _dest.WriteLine();
                _curEntry = default;
            }

            public override void Flush() => _dest.Flush();

            public override Output Emit() => WriteEntry("");
            public override Output Emit(string v) => WriteEntry(v);
            public override Output Emit(float v, string format) => WriteEntry(v.ToString(format));
            public override Output Emit(double v, string format) => WriteEntry(v.ToString(format));
            public override Output Emit(Vector3 v) => WriteEntry($"{v.X:f3}/{v.Y:f3}/{v.Z:f3}");
            public override Output Emit(Angle v) => WriteEntry(v.Deg.ToString("f3"));
            public override Output Emit(bool v) => WriteEntry(v.ToString());
            public override Output Emit(sbyte v) => WriteEntry(v.ToString());
            public override Output Emit(short v) => WriteEntry(v.ToString());
            public override Output Emit(int v) => WriteEntry(v.ToString());
            public override Output Emit(long v) => WriteEntry(v.ToString());
            public override Output Emit(byte v, string format = "d") => WriteEntry(v.ToString(format));
            public override Output Emit(ushort v, string format = "d") => WriteEntry(v.ToString(format));
            public override Output Emit(uint v, string format = "d") => WriteEntry(v.ToString(format));
            public override Output Emit(ulong v, string format = "d") => WriteEntry(v.ToString(format));
            public override Output Emit(ActionID v) => WriteEntry(v.ToString());
            public override Output Emit(Class v) => WriteEntry(v.ToString());
            public override Output Emit(ActorStatus v) => WriteEntry(Utils.StatusString(v.ID)).WriteEntry(v.Extra.ToString("X4")).WriteEntry(Utils.StatusTimeString(v.ExpireAt, _curEntry)).EmitActor(v.SourceID);
            public override Output Emit(List<ActorCastEvent.Target> v)
            {
                foreach (var t in v)
                {
                    EmitActor(t.ID);
                    for (int i = 0; i < 8; ++i)
                        if (t.Effects[i] != 0)
                            _dest.Write($"!{t.Effects[i]:X16}");
                }
                return this;
            }
            public override Output EmitTimePair(float t1, float t2) => WriteEntry($"{t1:f3}/{t2:f3}");
            public override Output EmitTimePair(DateTime t1, float t2) => WriteEntry($"{(t1 - _curEntry).TotalSeconds:f3}/{t2:f3}");
            public override Output EmitActor(ulong instanceID)
            {
                var actor = _actorLookup?.Find(instanceID);
                return WriteEntry(actor != null ? $"{actor.InstanceID:X8}/{actor.OID:X}/{actor.Name}/{actor.Type}/{actor.PosRot.X:f3}/{actor.PosRot.Y:f3}/{actor.PosRot.Z:f3}/{actor.Rotation}" : $"{instanceID:X8}");
            }

            private TextOutput WriteEntry(string v)
            {
                _dest.Write('|');
                _dest.Write(v);
                return this;
            }
        }

        //public class BinaryOutput : Output
        //{

        //}

        private WorldState _ws;
        private LoggingConfig _config;
        private Output? _logger = null;

        public bool Active => _logger != null;

        public const int Version = 10;

        public WorldStateLogger(WorldState ws, LoggingConfig config)
        {
            _ws = ws;
            _config = config;
            _config.Modified += ApplyConfig;
            if (_config.DumpWorldStateEvents)
                Activate();
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
            if (!Active && _config.TargetDirectory != null)
            {
                try
                {
                    _config.TargetDirectory.Create();
                    var logfile = new FileStream($"{_config.TargetDirectory.FullName}/{_config.LogPrefix}_{_ws.CurrentTime:yyyy_MM_dd_HH_mm_ss}.log", FileMode.Create, FileAccess.Write, FileShare.Read);
                    _logger = new TextOutput(logfile, _config.WorldLogFormat == LoggingConfig.LogFormat.TextVerbose ? _ws.Actors : null);
                }
                catch (IOException e)
                {
                    Service.Log($"Failed to start logging: {e}");
                    _config.DumpWorldStateEvents = false;
                    return;
                }

                // log initial state
                _logger.StartEntry(_ws.CurrentTime);
                _logger.Emit("VER ").Emit(Version).Emit(_ws.QPF);
                _logger.EndEntry();
                foreach (var op in _ws.CompareToInitial())
                    Log(null, op);

                // log changes
                _ws.Modified += Log;
            }
        }

        private void Deactivate()
        {
            if (Active)
            {
                _ws.Modified -= Log;
                _logger?.Dispose();
                _logger = null;
            }
        }

        private void Log(object? sender, WorldState.Operation op)
        {
            if (_logger != null)
            {
                _logger.StartEntry(_ws.CurrentTime);
                op.Write(_logger);
                _logger.EndEntry();
            }
        }
    }
}
