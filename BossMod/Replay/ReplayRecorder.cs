using System.ComponentModel;
using System.IO;
using System.IO.Compression;

namespace BossMod;

public sealed class ReplayRecorder : IDisposable
{
    public abstract class Output : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected abstract void Dispose(bool disposing);
        public abstract void StartEntry(DateTime t);
        public abstract Output Emit();
        public abstract Output Emit(string v);
        public abstract Output Emit(FourCC v);
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
        public abstract Output Emit(byte[] v);
        public abstract Output Emit(ActionID v);
        public abstract Output Emit(Class v);
        public abstract Output Emit(ActorStatus v);
        public abstract Output Emit(List<ActorCastEvent.Target> v);
        public abstract Output EmitFloatPair(float t1, float t2);
        public abstract Output EmitActor(ulong instanceID);
        public abstract void EndEntry();
        public abstract void Flush();

        public Output EmitFourCC(ReadOnlySpan<byte> v) => Emit(new FourCC(v));
    }

    public sealed class TextOutput(Stream dest, ActorState? actorLookup) : Output
    {
        private readonly StreamWriter _dest = new(dest);
        private DateTime _curEntry;

        protected override void Dispose(bool disposing) => _dest.Dispose();
        public override void StartEntry(DateTime t)
        {
            _curEntry = t;
            _dest.Write(t.ToString("O"));
        }
        public override Output Emit() => WriteEntry("");
        public override Output Emit(string v) => WriteEntry(v);
        public override Output Emit(FourCC v) => WriteEntry(v.ToString());
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
        public override Output Emit(byte[] v)
        {
            _dest.Write('|');
            foreach (var b in v)
                _dest.Write($"{b:X2}");
            return this;
        }
        public override Output Emit(ActionID v) => WriteEntry(v.ToString());
        public override Output Emit(Class v) => WriteEntry(v.ToString());
        public override Output Emit(ActorStatus v) => WriteEntry(Utils.StatusString(v.ID)).WriteEntry(v.Extra.ToString("X4")).WriteEntry(Utils.StatusTimeString(v.ExpireAt, _curEntry)).EmitActor(v.SourceID);
        public override Output Emit(List<ActorCastEvent.Target> v)
        {
            foreach (var t in v)
            {
                EmitActor(t.ID);
                for (int i = 0; i < ActionEffects.MaxCount; ++i)
                    if (t.Effects[i] != 0)
                        _dest.Write($"!{t.Effects[i]:X16}");
            }
            return this;
        }
        public override Output EmitFloatPair(float t1, float t2) => WriteEntry($"{t1:f3}/{t2:f3}");
        public override Output EmitActor(ulong instanceID)
        {
            var actor = actorLookup?.Find(instanceID);
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

    public sealed class BinaryOutput(Stream dest) : Output
    {
        private readonly BinaryWriter _dest = new(dest);

        protected override void Dispose(bool disposing) => _dest.Dispose();
        public override void StartEntry(DateTime t) { }
        public override Output Emit() => this;
        public override Output Emit(string v) { _dest.Write(v); return this; }
        public override Output Emit(FourCC v) { _dest.Write(v.Value); return this; }
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
        public override Output Emit(byte[] v) { _dest.Write(v.Length); _dest.Write(v); return this; }
        public override Output Emit(ActionID v) { _dest.Write(v.Raw); return this; }
        public override Output Emit(Class v) { _dest.Write((byte)v); return this; }
        public override Output Emit(ActorStatus v) { _dest.Write(v.ID); _dest.Write(v.Extra); _dest.Write(v.ExpireAt.Ticks); _dest.Write(v.SourceID); return this; }
        public override Output Emit(List<ActorCastEvent.Target> v)
        {
            _dest.Write(v.Count);
            foreach (var t in v)
            {
                _dest.Write(t.ID);
                for (int i = 0; i < ActionEffects.MaxCount; ++i)
                    _dest.Write(t.Effects[i]);
            }
            return this;
        }
        public override Output EmitFloatPair(float t1, float t2) { _dest.Write(t1); _dest.Write(t2); return this; }
        public override Output EmitActor(ulong instanceID) { _dest.Write(instanceID); return this; }
        public override void EndEntry() { }
        public override void Flush() => _dest.Flush();
    }

    private readonly WorldState _ws;
    private readonly Output _logger;
    private readonly EventSubscription _subscription;

    public const int Version = 20;

    public ReplayRecorder(WorldState ws, ReplayLogFormat format, bool logInitialState, DirectoryInfo targetDirectory, string logPrefix)
    {
        _ws = ws;
        targetDirectory.Create();
        Stream stream = new FileStream($"{targetDirectory.FullName}/{logPrefix}_{_ws.CurrentTime:yyyy_MM_dd_HH_mm_ss}.log", FileMode.Create, FileAccess.Write, FileShare.Read);
        switch (format)
        {
            case ReplayLogFormat.BinaryCompressed:
                WriteHeader(stream, ReplayLogFormatMagic.CompressedBinary);
                stream = new BrotliStream(stream, CompressionLevel.Optimal, false);
                _logger = new BinaryOutput(stream);
                break;
            case ReplayLogFormat.BinaryUncompressed:
                WriteHeader(stream, ReplayLogFormatMagic.RawBinary);
                _logger = new BinaryOutput(stream);
                break;
            case ReplayLogFormat.TextCondensed:
                _logger = new TextOutput(stream, null);
                break;
            case ReplayLogFormat.TextVerbose:
                _logger = new TextOutput(stream, _ws.Actors);
                break;
            default:
                throw new InvalidEnumArgumentException("Bad format");
        }

        // log initial state
        _logger.StartEntry(_ws.CurrentTime);
        _logger.EmitFourCC("VER "u8).Emit(Version).Emit(_ws.QPF).Emit(_ws.GameVersion);
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
        _subscription = _ws.Modified.Subscribe(Log);
    }

    public void Dispose()
    {
        _subscription.Dispose();
        _logger.Dispose();
    }

    private unsafe void WriteHeader(Stream stream, FourCC magic)
    {
        Span<byte> buf = stackalloc byte[4];
        fixed (byte* raw = &buf[0])
        {
            *(uint*)raw = magic.Value;
            stream.Write(buf);
        }
    }

    private void Log(WorldState.Operation op)
    {
        _logger.StartEntry(op.Timestamp);
        op.Write(_logger);
        _logger.EndEntry();
    }
}
