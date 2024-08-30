using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace BossMod;

public sealed class ReplayParserLog : IDisposable
{
    abstract class Input : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }
        public abstract FourCC NextEntry();
        public abstract bool CanRead();
        public abstract void ReadVoid();
        public abstract string ReadString();
        public abstract float ReadFloat();
        public abstract double ReadDouble();
        public abstract Vector3 ReadVec3();
        public abstract Angle ReadAngle();
        public abstract bool ReadBool();
        public abstract sbyte ReadSByte();
        public abstract short ReadShort();
        public abstract int ReadInt();
        public abstract long ReadLong();
        public abstract byte ReadByte(bool hex);
        public abstract ushort ReadUShort(bool hex);
        public abstract uint ReadUInt(bool hex);
        public abstract ulong ReadULong(bool hex);
        public abstract byte[] ReadBytes();
        public abstract ActionID ReadAction();
        public abstract Class ReadClass();
        public abstract ActorStatus ReadStatus();
        public abstract List<ActorCastEvent.Target> ReadTargets();
        public abstract (float, float) ReadFloatPair();
        public abstract (DateTime, float) ReadTimePair();
        public abstract ulong ReadActorID();
    }

    sealed class TextInput(Stream stream) : Input
    {
        public DateTime Timestamp { get; private set; }
        private readonly StreamReader _input = new(stream);
        private string[] _line = [];
        private int _nextPayload;

        protected override void Dispose(bool disposing)
        {
            _input.Dispose();
            base.Dispose(disposing);
        }

        public override FourCC NextEntry()
        {
            while (_input.ReadLine() is var line && line != null)
            {
                if (line.Length == 0 || line[0] == '#')
                    continue; // empty line or comment

                _line = line.Split("|");
                if (_line.Length < 2)
                    continue; // invalid string

                var tag = _line[1];
                if (tag.Length != 4)
                    continue; // invalid tag

                Timestamp = DateTime.Parse(_line[0]);
                _nextPayload = 2;
                Span<byte> fourcc = [(byte)tag[0], (byte)tag[1], (byte)tag[2], (byte)tag[3]];
                return new(fourcc);
            }
            return default;
        }

        public override bool CanRead() => _nextPayload < _line.Length;
        public override void ReadVoid() => _nextPayload++;
        public override string ReadString() => _line[_nextPayload++];
        public override float ReadFloat() => float.Parse(ReadString());
        public override double ReadDouble() => double.Parse(ReadString());
        public override Vector3 ReadVec3()
        {
            var parts = ReadString().Split('/');
            return new(float.Parse(parts[0]), float.Parse(parts[1]), float.Parse(parts[2]));
        }
        public override Angle ReadAngle() => ReadFloat().Degrees();
        public override bool ReadBool() => bool.Parse(ReadString());
        public override sbyte ReadSByte() => sbyte.Parse(ReadString());
        public override short ReadShort() => short.Parse(ReadString());
        public override int ReadInt() => int.Parse(ReadString());
        public override long ReadLong() => long.Parse(ReadString());
        public override byte ReadByte(bool hex) => byte.Parse(ReadString(), hex ? NumberStyles.HexNumber : NumberStyles.Number);
        public override ushort ReadUShort(bool hex) => ushort.Parse(ReadString(), hex ? NumberStyles.HexNumber : NumberStyles.Number);
        public override uint ReadUInt(bool hex) => uint.Parse(ReadString(), hex ? NumberStyles.HexNumber : NumberStyles.Number);
        public override ulong ReadULong(bool hex) => ulong.Parse(ReadString(), hex ? NumberStyles.HexNumber : NumberStyles.Number);
        public override byte[] ReadBytes()
        {
            var str = ReadString();
            var res = new byte[str.Length >> 1];
            for (int i = 0; i < res.Length; ++i)
                res[i] = byte.Parse(str.AsSpan()[(2 * i)..(2 * i + 2)], NumberStyles.HexNumber);
            return res;
        }
        public override ActionID ReadAction()
        {
            var parts = ReadString().Split(' ');
            var type = parts.Length > 0 ? Enum.Parse<ActionType>(parts[0]) : ActionType.None;
            var id = parts.Length > 1 ? uint.Parse(parts[1]) : 0;
            return new(type, id);
        }
        public override Class ReadClass() => Enum.Parse<Class>(ReadString());
        public override ActorStatus ReadStatus()
        {
            var sid = ReadString();
            int sep = sid.IndexOf(' ', StringComparison.Ordinal);
            return new(uint.Parse(sep >= 0 ? sid.AsSpan(0, sep) : sid.AsSpan()), ushort.Parse(ReadString(), NumberStyles.HexNumber), Timestamp.AddSeconds(ReadFloat()), ReadActorID());
        }
        public override List<ActorCastEvent.Target> ReadTargets()
        {
            List<ActorCastEvent.Target> res = [];
            while (CanRead())
            {
                var parts = ReadString().Split('!');
                var effects = new ActionEffects();
                for (int j = 1; j < parts.Length; ++j)
                    effects[j - 1] = ulong.Parse(parts[j], NumberStyles.HexNumber);
                res.Add(new(ParseActorID(parts[0]), effects));
            }
            return res;
        }
        public override (float, float) ReadFloatPair()
        {
            var parts = ReadString().Split('/');
            return (float.Parse(parts[0]), float.Parse(parts[1]));
        }
        public override (DateTime, float) ReadTimePair()
        {
            var parts = ReadString().Split('/');
            return (Timestamp.AddSeconds(float.Parse(parts[0])), float.Parse(parts[1]));
        }
        public override ulong ReadActorID() => ParseActorID(ReadString());

        private ulong ParseActorID(string actor)
        {
            var sep = actor.IndexOf('/', StringComparison.Ordinal);
            return ulong.Parse(sep >= 0 ? actor.AsSpan(0, sep) : actor.AsSpan(), NumberStyles.HexNumber);
        }
    }

    sealed class BinaryInput(Stream stream) : Input
    {
        private readonly BinaryReader _input = new(stream);

        protected override void Dispose(bool disposing)
        {
            _input.Dispose();
            base.Dispose(disposing);
        }

        public override FourCC NextEntry()
        {
            try
            {
                return new(_input.ReadUInt32());
            }
            catch (EndOfStreamException)
            {
                return default;
            }
        }

        public override bool CanRead() => true;
        public override void ReadVoid() { }
        public override string ReadString() => _input.ReadString();
        public override float ReadFloat() => _input.ReadSingle();
        public override double ReadDouble() => _input.ReadDouble();
        public override Vector3 ReadVec3() => new(_input.ReadSingle(), _input.ReadSingle(), _input.ReadSingle());
        public override Angle ReadAngle() => _input.ReadSingle().Radians();
        public override bool ReadBool() => _input.ReadBoolean();
        public override sbyte ReadSByte() => _input.ReadSByte();
        public override short ReadShort() => _input.ReadInt16();
        public override int ReadInt() => _input.ReadInt32();
        public override long ReadLong() => _input.ReadInt64();
        public override byte ReadByte(bool hex) => _input.ReadByte();
        public override ushort ReadUShort(bool hex) => _input.ReadUInt16();
        public override uint ReadUInt(bool hex) => _input.ReadUInt32();
        public override ulong ReadULong(bool hex) => _input.ReadUInt64();
        public override byte[] ReadBytes() => _input.ReadBytes(_input.ReadInt32());
        public override ActionID ReadAction() => new(_input.ReadUInt32());
        public override Class ReadClass() => (Class)_input.ReadByte();
        public override ActorStatus ReadStatus() => new(_input.ReadUInt32(), _input.ReadUInt16(), new(_input.ReadInt64()), _input.ReadUInt64());
        public override List<ActorCastEvent.Target> ReadTargets()
        {
            var count = _input.ReadInt32();
            List<ActorCastEvent.Target> res = new(count);
            for (int i = 0; i < count; ++i)
            {
                var id = _input.ReadUInt64();
                var effects = new ActionEffects();
                for (int j = 0; j < ActionEffects.MaxCount; ++j)
                    effects[j] = _input.ReadUInt64();
                res.Add(new(id, effects));
            }
            return res;
        }
        public override (float, float) ReadFloatPair() => (_input.ReadSingle(), _input.ReadSingle());
        public override (DateTime, float) ReadTimePair() => (new(_input.ReadInt64()), _input.ReadSingle());
        public override ulong ReadActorID() => _input.ReadUInt64();
    }

    public static Replay Parse(string path, ref float progress, CancellationToken cancel)
    {
        try
        {
            Input? input = null;
            Stream rawStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            Span<byte> header = stackalloc byte[4];
            if (rawStream.Read(header) == header.Length)
            {
                var headerMagic = new FourCC(header);
                if (headerMagic == ReplayLogFormatMagic.CompressedBinary)
                {
                    var decompressStream = new BrotliStream(rawStream, CompressionMode.Decompress, false);
                    input = new BinaryInput(decompressStream);
                }
                else if (headerMagic == ReplayLogFormatMagic.RawBinary)
                {
                    input = new BinaryInput(rawStream);
                }
            }
            if (input == null)
            {
                rawStream.Seek(0, SeekOrigin.Begin);
                input = new TextInput(rawStream);
            }

            var streamInvLength = 1.0f / rawStream.Length;
            int curOp = 0;
            using ReplayBuilder builder = new(path);
            using ReplayParserLog parser = new(input, builder);
            while (parser.ParseLine())
            {
                if ((++curOp & 0x3ff) == 0)
                {
                    progress = rawStream.Position * streamInvLength;
                    if (cancel.IsCancellationRequested)
                        break;
                }
            }
            return cancel.IsCancellationRequested ? new() : builder.Finish();
        }
        catch (Exception e)
        {
            Service.Log($"Failed to read {path}: {e}");
            return new();
        }
    }

    private readonly Input _input;
    private readonly ReplayBuilder _builder;
    private readonly Dictionary<FourCC, Func<WorldState.Operation?>> _dispatch;
    private int _version;
    private DateTime _tsStart;
    private ulong _qpcStart;
    private uint _legacyFrameIndex;
    private DateTime _legacyPrevTS;
    private double _invQPF = 1.0 / TimeSpan.TicksPerSecond;

    private ReplayParserLog(Input input, ReplayBuilder builder)
    {
        _input = input;
        _builder = builder;
        _dispatch = new()
        {
            [new("VER "u8)] = ParseVersion,
            [new("FRAM"u8)] = ParseFrameStart,
            [new("UMRK"u8)] = ParseUserMarker,
            [new("RSV "u8)] = ParseRSVData,
            [new("ZONE"u8)] = ParseZoneChange,
            [new("DIRU"u8)] = ParseDirectorUpdate,
            [new("ENVC"u8)] = ParseEnvControl,
            [new("WAY+"u8)] = () => ParseWaymarkChange(true),
            [new("WAY-"u8)] = () => ParseWaymarkChange(false),
            [new("ACT+"u8)] = ParseActorCreate,
            [new("ACT-"u8)] = ParseActorDestroy,
            [new("NAME"u8)] = ParseActorRename,
            [new("CLSR"u8)] = ParseActorClassChange,
            [new("MOVE"u8)] = ParseActorMove,
            [new("ACSZ"u8)] = ParseActorSizeChange,
            [new("HP  "u8)] = ParseActorHPMP,
            [new("ATG+"u8)] = () => ParseActorTargetable(true),
            [new("ATG-"u8)] = () => ParseActorTargetable(false),
            [new("ALLY"u8)] = ParseActorAlly,
            [new("DIE+"u8)] = () => ParseActorDead(true),
            [new("DIE-"u8)] = () => ParseActorDead(false),
            [new("COM+"u8)] = () => ParseActorCombat(true),
            [new("COM-"u8)] = () => ParseActorCombat(false),
            [new("NENP"u8)] = ParseActorAggroPlayer,
            [new("MDLS"u8)] = ParseActorModelState,
            [new("EVTS"u8)] = ParseActorEventState,
            [new("TARG"u8)] = ParseActorTarget,
            [new("MNTD"u8)] = ParseActorMount,
            [new("TETH"u8)] = () => ParseActorTether(true),
            [new("TET+"u8)] = () => ParseActorTether(true), // legacy (up to v4)
            [new("TET-"u8)] = () => ParseActorTether(false), // legacy (up to v4)
            [new("CST+"u8)] = () => ParseActorCastInfo(true),
            [new("CST-"u8)] = () => ParseActorCastInfo(false),
            [new("CST!"u8)] = ParseActorCastEvent,
            [new("ER  "u8)] = ParseActorEffectResult,
            [new("STA+"u8)] = () => ParseActorStatus(true),
            [new("STA-"u8)] = () => ParseActorStatus(false),
            [new("STA!"u8)] = () => ParseActorStatus(true),
            [new("ICON"u8)] = ParseActorIcon,
            [new("ESTA"u8)] = ParseActorEventObjectStateChange,
            [new("EANM"u8)] = ParseActorEventObjectAnimation,
            [new("PATE"u8)] = ParseActorPlayActionTimelineEvent,
            [new("NYEL"u8)] = ParseActorEventNpcYell,
            [new("PAR "u8)] = ParsePartyModify,
            [new("PAR+"u8)] = ParsePartyModify, // legacy (up to v3)
            [new("PAR-"u8)] = ParsePartyLeave, // legacy (up to v3)
            [new("PAR!"u8)] = ParsePartyModify, // legacy (up to v3)
            [new("LB  "u8)] = ParsePartyLimitBreak,
            [new("CLAR"u8)] = ParseClientActionRequest,
            [new("CLRJ"u8)] = ParseClientActionReject,
            [new("CDN+"u8)] = () => ParseClientCountdown(true),
            [new("CDN-"u8)] = () => ParseClientCountdown(false),
            [new("CLAL"u8)] = ParseClientAnimationLock,
            [new("CLCB"u8)] = ParseClientCombo,
            [new("CLST"u8)] = ParseClientPlayerStats,
            [new("CLCD"u8)] = ParseClientCooldown,
            [new("CLDA"u8)] = ParseClientDutyActions,
            [new("CLBH"u8)] = ParseClientBozjaHolster,
            [new("CLAF"u8)] = ParseClientActiveFate,
            [new("CLVL"u8)] = ParseClientClassJobLevels,
            [new("IPCI"u8)] = ParseNetworkIDScramble,
            [new("IPCS"u8)] = ParseNetworkServerIPC,
        };
    }

    public void Dispose() => _input.Dispose();

    private bool ParseLine()
    {
        var tag = _input.NextEntry();
        if (tag == default)
            return false; // end of replay

        if (_version is > 0 and < 5 && _input is TextInput ti && _legacyPrevTS < ti.Timestamp)
        {
            _builder.AddOp(new WorldState.OpFrameStart(new() { Timestamp = ti.Timestamp }, default, default, default));
            _legacyPrevTS = ti.Timestamp;
        }

        if (!_dispatch.TryGetValue(tag, out var parse))
            throw new InvalidOperationException($"Replay contains unsupported tag {tag}");

        var op = parse();
        if (op != null)
            _builder.AddOp(op);

        return true;
    }

    private WorldState.Operation? ParseVersion()
    {
        _version = _input.ReadInt();
        if (_version < 2)
            throw new InvalidOperationException($"Version {_version} is too old and is no longer supported, sorry");
        var qpf = _version >= 10 ? _input.ReadULong(false) : TimeSpan.TicksPerSecond; // newer windows versions have 10mhz qpc frequency
        var gameVersion = _version >= 11 ? _input.ReadString() : "old";
        _tsStart = _input is TextInput ti ? ti.Timestamp : new(_input.ReadLong());
        _invQPF = 1.0 / qpf;
        _builder.Start(qpf, gameVersion);
        return null;
    }

    private WorldState.OpFrameStart ParseFrameStart()
    {
        var frame = new FrameState();
        var prevUpdateTime = TimeSpan.FromMilliseconds(_input.ReadDouble());
        _input.ReadVoid();
        var gauge = new ClientState.Gauge(_input.CanRead() ? _input.ReadULong(true) : 0, _version >= 18 ? _input.ReadULong(true) : 0);
        if (_version >= 10)
        {
            frame.QPC = _input.ReadULong(false);
            frame.Index = _input.ReadUInt(false);
            frame.DurationRaw = _input.ReadFloat();
            frame.Duration = _input.ReadFloat();
            frame.TickSpeedMultiplier = _input.ReadFloat();
            if (_qpcStart != 0)
            {
                frame.Timestamp = _tsStart.AddSeconds((frame.QPC - _qpcStart) * _invQPF);
            }
            else
            {
                _qpcStart = frame.QPC;
                frame.Timestamp = _tsStart;
            }
        }
        else if (_input is TextInput ti)
        {
            frame.Timestamp = ti.Timestamp;
            frame.QPC = (ulong)(ti.Timestamp - _tsStart).Ticks;
            frame.Index = ++_legacyFrameIndex;
            frame.Duration = frame.DurationRaw = (float)(ti.Timestamp - _legacyPrevTS).TotalSeconds;
            frame.TickSpeedMultiplier = 1;
        }
        _legacyPrevTS = frame.Timestamp;
        return new(frame, prevUpdateTime, gauge, _version >= 16 ? _input.ReadAngle() : default);
    }

    private WorldState.OpUserMarker ParseUserMarker() => new(_input.ReadString());
    private WorldState.OpRSVData ParseRSVData() => new(_input.ReadString(), _input.ReadString());
    private WorldState.OpZoneChange ParseZoneChange() => new(_input.ReadUShort(false), _version >= 13 ? _input.ReadUShort(false) : (ushort)0);
    private WorldState.OpDirectorUpdate ParseDirectorUpdate()
        => new(_input.ReadUInt(true), _input.ReadUInt(true), _input.ReadUInt(true), _input.ReadUInt(true), _input.ReadUInt(true), _input.ReadUInt(true));

    private WorldState.OpEnvControl ParseEnvControl()
    {
        // director id field is removed in v11
        if (_version < 11)
            _input.ReadUInt(true);
        return new(_input.ReadByte(true), _input.ReadUInt(true));
    }

    private WaymarkState.OpWaymarkChange ParseWaymarkChange(bool set)
        => new(_version < 10 ? Enum.Parse<Waymark>(_input.ReadString()) : (Waymark)_input.ReadByte(false), set ? _input.ReadVec3() : null);

    private ActorState.OpCreate ParseActorCreate()
    {
        if (_version < 10)
        {
            var parts = _input.ReadString().Split('/');
            var cls = _input.ReadClass();
            var targetable = _input.ReadBool();
            var radius = _input.ReadFloat();
            var owner = _input.CanRead() ? _input.ReadActorID() : 0;
            var hpmp = _input.CanRead() ? ReadActorHPMP() : default;
            var ally = _input.CanRead() && _input.ReadBool();
            var spawnIndex = _input.CanRead() ? _input.ReadInt() : -1;
            return new
            (
                ulong.Parse(parts[0], NumberStyles.HexNumber),
                uint.Parse(parts[1], NumberStyles.HexNumber),
                spawnIndex,
                parts[2],
                0,
                parts[3] == "Unknown" ? ActorType.Part : Enum.Parse<ActorType>(parts[3]),
                cls,
                0,
                new(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]).Degrees().Rad),
                radius,
                hpmp,
                targetable,
                ally,
                owner,
                0
            );
        }
        else
        {
            return new
            (
                _input.ReadULong(true),
                _input.ReadUInt(true),
                _input.ReadInt(),
                _input.ReadString(),
                _version >= 13 ? _input.ReadUInt(false) : 0,
                (ActorType)_input.ReadUShort(true),
                _input.ReadClass(),
                _version < 12 ? 0 : _input.ReadInt(),
                new(_input.ReadVec3(), _input.ReadAngle().Rad),
                _input.ReadFloat(),
                new(_input.ReadUInt(false), _input.ReadUInt(false), _input.ReadUInt(false), _input.ReadUInt(false)),
                _input.ReadBool(),
                _input.ReadBool(),
                _input.ReadActorID(),
                _version >= 14 ? _input.ReadUInt(false) : 0
            );
        }
    }

    private ActorState.OpDestroy ParseActorDestroy() => new(_input.ReadActorID());

    private ActorState.OpRename ParseActorRename()
    {
        if (_version < 10)
        {
            var parts = _input.ReadString().Split('/');
            return new(ulong.Parse(parts[0], NumberStyles.HexNumber), parts[2], 0);
        }
        else
        {
            return new(_input.ReadULong(true), _input.ReadString(), _version >= 13 ? _input.ReadUInt(false) : 0);
        }
    }

    private ActorState.OpClassChange ParseActorClassChange()
    {
        var instanceID = _input.ReadActorID();
        _input.ReadVoid();
        return new(instanceID, _input.ReadClass(), _version < 12 ? 0 : _input.ReadInt());
    }

    private ActorState.OpMove ParseActorMove()
    {
        if (_version < 10)
        {
            var parts = _input.ReadString().Split('/');
            return new(ulong.Parse(parts[0], NumberStyles.HexNumber), new(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]).Degrees().Rad));
        }
        else
        {
            return new(_input.ReadULong(true), new(_input.ReadVec3(), _input.ReadAngle().Rad));
        }
    }

    private ActorState.OpSizeChange ParseActorSizeChange() => new(_input.ReadActorID(), _input.ReadFloat());
    private ActorState.OpHPMP ParseActorHPMP() => new(_input.ReadActorID(), ReadActorHPMP());
    private ActorState.OpTargetable ParseActorTargetable(bool targetable) => new(_input.ReadActorID(), targetable);
    private ActorState.OpAlly ParseActorAlly() => new(_input.ReadActorID(), _input.ReadBool());
    private ActorState.OpDead ParseActorDead(bool dead) => new(_input.ReadActorID(), dead);
    private ActorState.OpCombat ParseActorCombat(bool value) => new(_input.ReadActorID(), value);
    private ActorState.OpAggroPlayer ParseActorAggroPlayer() => new(_input.ReadActorID(), _input.ReadBool());
    private ActorState.OpModelState ParseActorModelState()
        => new(_input.ReadActorID(), new(_input.ReadByte(false), _input.CanRead() ? _input.ReadByte(false) : (byte)0, _input.CanRead() ? _input.ReadByte(false) : (byte)0));
    private ActorState.OpEventState ParseActorEventState() => new(_input.ReadActorID(), _input.ReadByte(false));
    private ActorState.OpTarget ParseActorTarget() => new(_input.ReadActorID(), _input.ReadActorID());
    private ActorState.OpMount ParseActorMount() => new(_input.ReadActorID(), _input.ReadUInt(false));
    private ActorState.OpTether ParseActorTether(bool tether) => new(_input.ReadActorID(), tether ? new(_input.ReadUInt(false), _input.ReadActorID()) : default);

    private ActorState.OpCastInfo ParseActorCastInfo(bool start)
    {
        var instanceID = _input.ReadActorID();
        ActorCastInfo? cast = null;
        if (start)
        {
            var action = _input.ReadAction();
            var target = _input.ReadActorID();
            var loc = _input.ReadVec3();
            float elapsedTime, totalTime;
            if (_version >= 17)
            {
                (elapsedTime, totalTime) = _input.ReadFloatPair();
            }
            else
            {
                (var finishAt, totalTime) = _input.ReadTimePair();
                elapsedTime = totalTime + action.CastTimeExtra() - (float)(finishAt - _legacyPrevTS).TotalSeconds;
            }
            cast = new()
            {
                Action = action,
                TargetID = target,
                Location = loc,
                ElapsedTime = elapsedTime,
                TotalTime = totalTime,
                Interruptible = _input.CanRead() && _input.ReadBool(),
                Rotation = _input.CanRead() ? _input.ReadAngle() : default,
            };
        }
        return new(instanceID, cast);
    }

    private ActorState.OpCastEvent ParseActorCastEvent() => new(_input.ReadActorID(), new()
    {
        Action = _input.ReadAction(),
        MainTargetID = _input.ReadActorID(),
        AnimationLockTime = _input.ReadFloat(),
        MaxTargets = _input.ReadUInt(false),
        TargetPos = _version >= 6 ? _input.ReadVec3() : new(),
        GlobalSequence = _version >= 7 ? _input.ReadUInt(false) : 0,
        SourceSequence = _version >= 9 ? _input.ReadUInt(false) : 0,
        Targets = _input.ReadTargets()
    });

    private ActorState.OpEffectResult ParseActorEffectResult() => new(_input.ReadActorID(), _input.ReadUInt(false), _input.ReadInt());
    private ActorState.OpStatus ParseActorStatus(bool gainOrUpdate) => new(_input.ReadActorID(), _input.ReadInt(), gainOrUpdate ? _input.ReadStatus() : default);
    private ActorState.OpIcon ParseActorIcon() => new(_input.ReadActorID(), _input.ReadUInt(false));
    private ActorState.OpEventObjectStateChange ParseActorEventObjectStateChange() => new(_input.ReadActorID(), _input.ReadUShort(true));
    private ActorState.OpEventObjectAnimation ParseActorEventObjectAnimation() => new(_input.ReadActorID(), _input.ReadUShort(true), _input.ReadUShort(true));
    private ActorState.OpPlayActionTimelineEvent ParseActorPlayActionTimelineEvent() => new(_input.ReadActorID(), _input.ReadUShort(true));
    private ActorState.OpEventNpcYell ParseActorEventNpcYell() => new(_input.ReadActorID(), _input.ReadUShort(false));
    private PartyState.OpModify ParsePartyModify() => new(_input.ReadInt(), new(_input.ReadULong(true), _input.ReadULong(true), _version >= 15 && _input.ReadBool(), _version < 15 ? "" : _input.ReadString()));
    private PartyState.OpModify ParsePartyLeave() => new(_input.ReadInt(), new(0, 0, false, ""));
    private PartyState.OpLimitBreakChange ParsePartyLimitBreak() => new(_input.ReadInt(), _input.ReadInt());

    private ClientState.OpActionRequest ParseClientActionRequest()
    {
        var req = new ClientActionRequest()
        {
            Action = _input.ReadAction(),
            TargetID = _input.ReadActorID(),
            TargetPos = _input.ReadVec3(),
            SourceSequence = _input.ReadUInt(false),
            InitialAnimationLock = _input.ReadFloat(),
        };
        (req.InitialCastTimeElapsed, req.InitialCastTimeTotal) = _input.ReadFloatPair();
        (req.InitialRecastElapsed, req.InitialRecastTotal) = _input.ReadFloatPair();
        return new(req);
    }

    private ClientState.OpActionReject ParseClientActionReject()
    {
        var rej = new ClientActionReject()
        {
            Action = _input.ReadAction(),
            SourceSequence = _input.ReadUInt(false),
        };
        (rej.RecastElapsed, rej.RecastTotal) = _input.ReadFloatPair();
        rej.LogMessageID = _input.ReadUInt(false);
        return new(rej);
    }

    private ClientState.OpCountdownChange ParseClientCountdown(bool start) => new(start ? _input.ReadFloat() : null);
    private ClientState.OpAnimationLockChange ParseClientAnimationLock() => new(_input.ReadFloat());
    private ClientState.OpComboChange ParseClientCombo() => new(new(_input.ReadUInt(false), _input.ReadFloat()));
    private ClientState.OpPlayerStatsChange ParseClientPlayerStats() => new(new(_input.ReadInt(), _input.ReadInt(), _input.ReadInt()));

    private ClientState.OpCooldown ParseClientCooldown()
    {
        var reset = _input.ReadBool();
        List<(int, Cooldown)> cooldowns = [];
        cooldowns.Capacity = _input.ReadByte(false);
        for (int i = 0; i < cooldowns.Capacity; ++i)
            cooldowns.Add((_input.ReadByte(false), new(_input.ReadFloat(), _input.ReadFloat())));
        if (_version < 19)
            foreach (ref var cd in cooldowns.AsSpan())
                cd.Item2.Elapsed = cd.Item2.Total - cd.Item2.Elapsed; // there was a mistake before v19 where remaining was saved instead of elapsed
        return new(reset, cooldowns);
    }

    private ClientState.OpDutyActionsChange ParseClientDutyActions() => new(_input.ReadAction(), _input.ReadAction());

    private ClientState.OpBozjaHolsterChange ParseClientBozjaHolster()
    {
        List<(BozjaHolsterID, byte)> contents = [];
        contents.Capacity = _input.ReadByte(false);
        for (int i = 0; i < contents.Capacity; ++i)
            contents.Add(((BozjaHolsterID)_input.ReadByte(false), _input.ReadByte(false)));
        return new(contents);
    }

    private ClientState.OpActiveFateChange ParseClientActiveFate() => new(new(_input.ReadUInt(false), _input.ReadVec3(), _input.ReadFloat()));

    private ClientState.OpClassJobLevelsChange ParseClientClassJobLevels()
    {
        var contents = new short[_input.ReadByte(false)];
        for (int i = 0; i < contents.Length; i++)
            contents[i] = _input.ReadShort();
        return new(contents);
    }

    private NetworkState.OpIDScramble ParseNetworkIDScramble() => new(_input.ReadUInt(false));
    private NetworkState.OpServerIPC ParseNetworkServerIPC() => new(new((Network.ServerIPC.PacketID)_input.ReadInt(), _input.ReadUShort(false), _input.ReadUInt(false), _input.ReadUInt(true), new(_input.ReadLong()), _input.ReadBytes()));

    private ActorHPMP ReadActorHPMP()
    {
        if (_version < 10)
        {
            var parts = _input.ReadString().Split('/');
            return new(uint.Parse(parts[0]), uint.Parse(parts[1]), parts.Length > 2 ? uint.Parse(parts[2]) : 0, parts.Length > 3 ? uint.Parse(parts[3]) : 0);
        }
        else
        {
            return new(_input.ReadUInt(false), _input.ReadUInt(false), _input.ReadUInt(false), _input.ReadUInt(false));
        }
    }
}
