using System.IO;
using System.IO.Compression;

namespace BossMod;

public class ReplayRecorder : IDisposable
{
    public abstract class Output : IDisposable
    {
        public abstract void Dispose();
        public abstract Output Entry(string tag, DateTime t);
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
        public abstract Output EmitFloatPair(float t1, float t2);
        public abstract Output EmitTimePair(DateTime t1, float t2);
        public abstract Output EmitActor(ulong instanceID);
        public abstract void EndEntry();
        public abstract void Flush();
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

        public override Output Entry(string tag, DateTime t)
        {
            _curEntry = t;
            _dest.Write(t.ToString("O"));
            return WriteEntry(tag);
        }

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
        public override Output EmitFloatPair(float t1, float t2) => WriteEntry($"{t1:f3}/{t2:f3}");
        public override Output EmitTimePair(DateTime t1, float t2) => WriteEntry($"{(t1 - _curEntry).TotalSeconds:f3}/{t2:f3}");
        public override Output EmitActor(ulong instanceID)
        {
            var actor = _actorLookup?.Find(instanceID);
            return WriteEntry(actor != null ? $"{actor.InstanceID:X8}/{actor.OID:X}/{actor.Name}/{actor.Type}/{actor.PosRot.X:f3}/{actor.PosRot.Y:f3}/{actor.PosRot.Z:f3}/{actor.Rotation}" : $"{instanceID:X8}");
        }

        public override void EndEntry()
        {
            _dest.WriteLine();
            _curEntry = default;
        }

        public override void Flush() => _dest.Flush();

        private TextOutput WriteEntry(string v)
        {
            _dest.Write('|');
            _dest.Write(v);
            return this;
        }
    }

    public class BinaryOutput : Output
    {
        private BinaryWriter _dest;

        public BinaryOutput(Stream dest)
        {
            _dest = new(dest);
        }

        public override void Dispose()
        {
            _dest.Dispose();
        }

        public override Output Entry(string tag, DateTime t)
        {
            foreach (var c in tag)
                _dest.Write((byte)c);
            return this;
        }

        public override Output Emit() => this;
        public override Output Emit(string v) { _dest.Write(v); return this; }
        public override Output Emit(float v, string format) { _dest.Write(v); return this; }
        public override Output Emit(double v, string format) { _dest.Write(v); return this; }
        public override Output Emit(Vector3 v) { _dest.Write(v.X); _dest.Write(v.Y); _dest.Write(v.Z); return this; }
        public override Output Emit(Angle v) { _dest.Write(v.Rad); return this; }
        public override Output Emit(bool v) { _dest.Write(v); return this; }
        public override Output Emit(sbyte v) { _dest.Write(v); return this; }
        public override Output Emit(short v) { _dest.Write(v); return this; }
        public override Output Emit(int v) { _dest.Write(v); return this; }
        public override Output Emit(long v) { _dest.Write(v); return this; }
        public override Output Emit(byte v, string format = "d") { _dest.Write(v); return this; }
        public override Output Emit(ushort v, string format = "d") { _dest.Write(v); return this; }
        public override Output Emit(uint v, string format = "d") { _dest.Write(v); return this; }
        public override Output Emit(ulong v, string format = "d") { _dest.Write(v); return this; }
        public override Output Emit(ActionID v) { _dest.Write(v.Raw); return this; }
        public override Output Emit(Class v) { _dest.Write((byte)v); return this; }
        public override Output Emit(ActorStatus v) { _dest.Write(v.ID); _dest.Write(v.Extra); _dest.Write(v.ExpireAt.Ticks); _dest.Write(v.SourceID); return this; }
        public override Output Emit(List<ActorCastEvent.Target> v)
        {
            _dest.Write(v.Count);
            foreach (var t in v)
            {
                _dest.Write(t.ID);
                for (int i = 0; i < 8; ++i)
                    _dest.Write(t.Effects[i]);
            }
            return this;
        }
        public override Output EmitFloatPair(float t1, float t2) { _dest.Write(t1); _dest.Write(t2); return this; }
        public override Output EmitTimePair(DateTime t1, float t2) { _dest.Write(t1.Ticks); _dest.Write(t2); return this; }
        public override Output EmitActor(ulong instanceID) { _dest.Write(instanceID); return this; }
        public override void EndEntry() { }
        public override void Flush() => _dest.Flush();
    }

    private WorldState _ws;
    private Output _logger;

    public const int Version = 13;

    public ReplayRecorder(WorldState ws, ReplayLogFormat format, bool logInitialState, DirectoryInfo targetDirectory, string logPrefix)
    {
        _ws = ws;
        targetDirectory.Create();
        Stream stream = new FileStream($"{targetDirectory.FullName}/{logPrefix}_{_ws.CurrentTime:yyyy_MM_dd_HH_mm_ss}.log", FileMode.Create, FileAccess.Write, FileShare.Read);
        switch (format)
        {
            case ReplayLogFormat.BinaryCompressed:
                WriteHeader(stream, "BLCB");// bossmod log compressed brotli
                stream = new BrotliStream(stream, CompressionLevel.Optimal, false);
                _logger = new BinaryOutput(stream);
                break;
            case ReplayLogFormat.BinaryUncompressed:
                WriteHeader(stream, "BLOG");// bossmod log
                _logger = new BinaryOutput(stream);
                break;
            case ReplayLogFormat.TextCondensed:
                _logger = new TextOutput(stream, null);
                break;
            case ReplayLogFormat.TextVerbose:
                _logger = new TextOutput(stream, _ws.Actors);
                break;
            default:
                throw new Exception("Bad format");
        }

        // log initial state
        _logger.Entry("VER ", _ws.CurrentTime).Emit(Version).Emit(_ws.QPF).Emit(_ws.GameVersion);
        if (_logger is BinaryOutput)
            _logger.Emit(_ws.CurrentTime.Ticks);
        _logger.EndEntry();
        if (logInitialState)
        {
            foreach (var op in _ws.CompareToInitial())
            {
                op.Timestamp = _ws.CurrentTime;
                Log(op);
            }
        }

        // log changes
        _ws.Modified += Log;
    }

    public void Dispose()
    {
        _ws.Modified -= Log;
        _logger.Dispose();
    }

    private void WriteHeader(Stream stream, string header)
    {
        foreach (var c in header)
            stream.WriteByte((byte)c);
    }

    private void Log(WorldState.Operation op)
    {
        op.Write(_logger);
        _logger.EndEntry();
    }
}
