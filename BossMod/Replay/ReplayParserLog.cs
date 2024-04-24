using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;

namespace BossMod;

public class ReplayParserLog : ReplayParser
{
    abstract class Input : IDisposable
    {
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) { }
        public abstract string NextEntry();
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

        public override string NextEntry()
        {
            while (_input.ReadLine() is var line && line != null)
            {
                if (line.Length == 0 || line[0] == '#')
                    continue; // empty line or comment

                _line = line.Split("|");
                if (_line.Length < 2)
                    continue; // invalid string

                Timestamp = DateTime.Parse(_line[0]);
                _nextPayload = 2;
                return _line[1];
            }
            return "";
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

        public override string NextEntry()
        {
            try
            {
                var headerRaw = _input.ReadUInt32();
                var header = new byte[] { (byte)headerRaw, (byte)(headerRaw >> 8), (byte)(headerRaw >> 16), (byte)(headerRaw >> 24) };
                return Encoding.UTF8.GetString(header);
            }
            catch (EndOfStreamException)
            {
                return "";
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
            var header = new byte[4];
            if (rawStream.Read(header, 0, header.Length) == header.Length)
            {
                if (header[0] == 'B' && header[1] == 'L' && header[2] == 'C' && header[3] == 'B')
                {
                    var decompressStream = new BrotliStream(rawStream, CompressionMode.Decompress, false);
                    input = new BinaryInput(decompressStream);
                }
                else if (header[0] == 'B' && header[1] == 'L' && header[2] == 'O' && header[3] == 'G')
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
            using ReplayParserLog parser = new(input);
            while (parser.ParseLine())
            {
                if ((++curOp & 0x3ff) == 0)
                {
                    progress = rawStream.Position * streamInvLength;
                    if (cancel.IsCancellationRequested)
                        break;
                }
            }
            return cancel.IsCancellationRequested ? new() : parser.Finish(path);
        }
        catch (Exception e)
        {
            Service.Log($"Failed to read {path}: {e}");
            return new();
        }
    }

    private readonly Input _input;
    private int _version;
    private DateTime _tsStart;
    private ulong _qpcStart;

    private ReplayParserLog(Input input) => _input = input;

    protected override void Dispose(bool disposing)
    {
        base.Dispose(true);
        _input.Dispose();
    }

    private bool ParseLine()
    {
        var tag = _input.NextEntry();
        if (tag.Length < 4)
            return false;

        if (_version is > 0 and < 5 && _input is TextInput ti && (_res.Ops.Count == 0 || _res.Ops[^1].Timestamp < ti.Timestamp))
            AddOp(new WorldState.OpFrameStart(new() { Timestamp = ti.Timestamp }, default, 0));

        switch (tag)
        {
            case "VER ": ParseVersion(); break;
            case "FRAM": ParseFrameStart(); break;
            case "UMRK": ParseUserMarker(); break;
            case "RSV ": ParseRSVData(); break;
            case "ZONE": ParseZoneChange(); break;
            case "DIRU": ParseDirectorUpdate(); break;
            case "ENVC": ParseEnvControl(); break;
            case "WAY+": ParseWaymarkChange(true); break;
            case "WAY-": ParseWaymarkChange(false); break;
            case "ACT+": ParseActorCreate(); break;
            case "ACT-": ParseActorDestroy(); break;
            case "NAME": ParseActorRename(); break;
            case "CLSR": ParseActorClassChange(); break;
            case "MOVE": ParseActorMove(); break;
            case "ACSZ": ParseActorSizeChange(); break;
            case "HP  ": ParseActorHPMP(); break;
            case "ATG+": ParseActorTargetable(true); break;
            case "ATG-": ParseActorTargetable(false); break;
            case "ALLY": ParseActorAlly(); break;
            case "DIE+": ParseActorDead(true); break;
            case "DIE-": ParseActorDead(false); break;
            case "COM+": ParseActorCombat(true); break;
            case "COM-": ParseActorCombat(false); break;
            case "MDLS": ParseActorModelState(); break;
            case "EVTS": ParseActorEventState(); break;
            case "TARG": ParseActorTarget(); break;
            case "TETH": ParseActorTether(true); break;
            case "TET+": ParseActorTether(true); break; // legacy (up to v4)
            case "TET-": ParseActorTether(false); break; // legacy (up to v4)
            case "CST+": ParseActorCastInfo(true); break;
            case "CST-": ParseActorCastInfo(false); break;
            case "CST!": ParseActorCastEvent(); break;
            case "ER  ": ParseActorEffectResult(); break;
            case "STA+": ParseActorStatus(true); break;
            case "STA-": ParseActorStatus(false); break;
            case "STA!": ParseActorStatus(true); break;
            case "ICON": ParseActorIcon(); break;
            case "ESTA": ParseActorEventObjectStateChange(); break;
            case "EANM": ParseActorEventObjectAnimation(); break;
            case "PATE": ParseActorPlayActionTimelineEvent(); break;
            case "NYEL": ParseActorEventNpcYell(); break;
            case "PAR ": ParsePartyModify(); break;
            case "PAR+": ParsePartyModify(); break; // legacy (up to v3)
            case "PAR-": ParsePartyLeave(); break; // legacy (up to v3)
            case "PAR!": ParsePartyModify(); break; // legacy (up to v3)
            case "LB  ": ParsePartyLimitBreak(); break;
            case "CLAR": ParseClientActionRequest(); break;
            case "CLRJ": ParseClientActionReject(); break;
            case "CDN+": ParseClientCountdown(true); break;
            case "CDN-": ParseClientCountdown(false); break;
            case "CLCD": ParseCooldown(); break;
            case "CLDA": ParseClientDutyActions(); break;
            case "CLBH": ParseClientBozjaHolster(); break;
        }

        return true;
    }

    private void ParseVersion()
    {
        _version = _input.ReadInt();
        if (_version < 2)
            throw new InvalidOperationException($"Version {_version} is too old and is no longer supported, sorry");
        var qpf = _version >= 10 ? _input.ReadULong(false) : TimeSpan.TicksPerSecond; // newer windows versions have 10mhz qpc frequency
        var gameVersion = _version >= 11 ? _input.ReadString() : "old";
        _tsStart = _input is TextInput ti ? ti.Timestamp : new(_input.ReadLong());
        Start(_tsStart, qpf, gameVersion);
    }

    private void ParseFrameStart()
    {
        var frame = new FrameState();
        var prevUpdateTime = TimeSpan.FromMilliseconds(_input.ReadDouble());
        _input.ReadVoid();
        var gauge = _input.CanRead() ? _input.ReadULong(true) : 0;
        if (_version >= 10)
        {
            frame.QPC = _input.ReadULong(false);
            frame.Index = _input.ReadUInt(false);
            frame.DurationRaw = _input.ReadFloat();
            frame.Duration = _input.ReadFloat();
            frame.TickSpeedMultiplier = _input.ReadFloat();
            if (_qpcStart != 0)
            {
                frame.Timestamp = _tsStart.AddSeconds((double)(frame.QPC - _qpcStart) / _res.QPF);
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
            frame.Index = _ws.Frame.Index + 1;
            frame.Duration = frame.DurationRaw = (float)(ti.Timestamp - _ws.CurrentTime).TotalSeconds;
            frame.TickSpeedMultiplier = 1;
        }
        AddOp(new WorldState.OpFrameStart(frame, prevUpdateTime, gauge));
    }

    private void ParseUserMarker() => AddOp(new WorldState.OpUserMarker(_input.ReadString()));
    private void ParseRSVData() => AddOp(new WorldState.OpRSVData(_input.ReadString(), _input.ReadString()));
    private void ParseZoneChange() => AddOp(new WorldState.OpZoneChange(_input.ReadUShort(false), _version >= 13 ? _input.ReadUShort(false) : (ushort)0));
    private void ParseDirectorUpdate() => AddOp(new WorldState.OpDirectorUpdate(
        _input.ReadUInt(true), _input.ReadUInt(true), _input.ReadUInt(true), _input.ReadUInt(true), _input.ReadUInt(true), _input.ReadUInt(true)));

    private void ParseEnvControl()
    {
        // director id field is removed in v11
        if (_version < 11)
            _input.ReadUInt(true);
        AddOp(new WorldState.OpEnvControl(_input.ReadByte(true), _input.ReadUInt(true)));
    }

    private void ParseWaymarkChange(bool set) => AddOp(new WaymarkState.OpWaymarkChange(
        _version < 10 ? Enum.Parse<Waymark>(_input.ReadString()) : (Waymark)_input.ReadByte(false),
        set ? _input.ReadVec3() : null));

    private void ParseActorCreate()
    {
        ActorState.OpCreate op;
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
            op = new
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
                owner
            );
        }
        else
        {
            op = new
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
                _input.ReadActorID()
            );
        }
        AddOp(op);
    }

    private void ParseActorDestroy() => AddOp(new ActorState.OpDestroy(_input.ReadActorID()));

    private void ParseActorRename()
    {
        ActorState.OpRename op;
        if (_version < 10)
        {
            var parts = _input.ReadString().Split('/');
            op = new(ulong.Parse(parts[0], NumberStyles.HexNumber), parts[2], 0);
        }
        else
        {
            op = new(_input.ReadULong(true), _input.ReadString(), _version >= 13 ? _input.ReadUInt(false) : 0);
        }
        AddOp(op);
    }

    private void ParseActorClassChange()
    {
        var instanceID = _input.ReadActorID();
        _input.ReadVoid();
        AddOp(new ActorState.OpClassChange(instanceID, _input.ReadClass(), _version < 12 ? 0 : _input.ReadInt()));
    }

    private void ParseActorMove()
    {
        ActorState.OpMove op;
        if (_version < 10)
        {
            var parts = _input.ReadString().Split('/');
            op = new(ulong.Parse(parts[0], NumberStyles.HexNumber), new(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]).Degrees().Rad));
        }
        else
        {
            op = new(_input.ReadULong(true), new(_input.ReadVec3(), _input.ReadAngle().Rad));
        }
        AddOp(op);
    }

    private void ParseActorSizeChange() => AddOp(new ActorState.OpSizeChange(_input.ReadActorID(), _input.ReadFloat()));
    private void ParseActorHPMP() => AddOp(new ActorState.OpHPMP(_input.ReadActorID(), ReadActorHPMP()));
    private void ParseActorTargetable(bool targetable) => AddOp(new ActorState.OpTargetable(_input.ReadActorID(), targetable));
    private void ParseActorAlly() => AddOp(new ActorState.OpAlly(_input.ReadActorID(), _input.ReadBool()));
    private void ParseActorDead(bool dead) => AddOp(new ActorState.OpDead(_input.ReadActorID(), dead));
    private void ParseActorCombat(bool value) => AddOp(new ActorState.OpCombat(_input.ReadActorID(), value));
    private void ParseActorModelState() => AddOp(new ActorState.OpModelState(_input.ReadActorID(),
        new(_input.ReadByte(false), _input.CanRead() ? _input.ReadByte(false) : (byte)0, _input.CanRead() ? _input.ReadByte(false) : (byte)0)));
    private void ParseActorEventState() => AddOp(new ActorState.OpEventState(_input.ReadActorID(), _input.ReadByte(false)));
    private void ParseActorTarget() => AddOp(new ActorState.OpTarget(_input.ReadActorID(), _input.ReadActorID()));
    private void ParseActorTether(bool tether) => AddOp(new ActorState.OpTether(_input.ReadActorID(), tether ? new(_input.ReadUInt(false), _input.ReadActorID()) : default));

    private void ParseActorCastInfo(bool start)
    {
        var instanceID = _input.ReadActorID();
        ActorCastInfo? cast = null;
        if (start)
        {
            var action = _input.ReadAction();
            var target = _input.ReadActorID();
            var loc = _input.ReadVec3();
            var (finishAt, totalTime) = _input.ReadTimePair();
            cast = new()
            {
                Action = action,
                TargetID = target,
                Location = loc,
                TotalTime = totalTime,
                FinishAt = finishAt,
                Interruptible = _input.CanRead() && _input.ReadBool(),
                Rotation = _input.CanRead() ? _input.ReadAngle() : default,
            };
        }
        AddOp(new ActorState.OpCastInfo(instanceID, cast));
    }

    private void ParseActorCastEvent() => AddOp(new ActorState.OpCastEvent(_input.ReadActorID(), new()
    {
        Action = _input.ReadAction(),
        MainTargetID = _input.ReadActorID(),
        AnimationLockTime = _input.ReadFloat(),
        MaxTargets = _input.ReadUInt(false),
        TargetPos = _version >= 6 ? _input.ReadVec3() : new(),
        GlobalSequence = _version >= 7 ? _input.ReadUInt(false) : 0,
        SourceSequence = _version >= 9 ? _input.ReadUInt(false) : 0,
        Targets = _input.ReadTargets()
    }));

    private void ParseActorEffectResult() => AddOp(new ActorState.OpEffectResult(_input.ReadActorID(), _input.ReadUInt(false), _input.ReadInt()));
    private void ParseActorStatus(bool gainOrUpdate) => AddOp(new ActorState.OpStatus(_input.ReadActorID(), _input.ReadInt(), gainOrUpdate ? _input.ReadStatus() : default));
    private void ParseActorIcon() => AddOp(new ActorState.OpIcon(_input.ReadActorID(), _input.ReadUInt(false)));
    private void ParseActorEventObjectStateChange() => AddOp(new ActorState.OpEventObjectStateChange(_input.ReadActorID(), _input.ReadUShort(true)));
    private void ParseActorEventObjectAnimation() => AddOp(new ActorState.OpEventObjectAnimation(_input.ReadActorID(), _input.ReadUShort(true), _input.ReadUShort(true)));
    private void ParseActorPlayActionTimelineEvent() => AddOp(new ActorState.OpPlayActionTimelineEvent(_input.ReadActorID(), _input.ReadUShort(true)));
    private void ParseActorEventNpcYell() => AddOp(new ActorState.OpEventNpcYell(_input.ReadActorID(), _input.ReadUShort(false)));
    private void ParsePartyModify() => AddOp(new PartyState.OpModify(_input.ReadInt(), _input.ReadULong(true), _input.ReadULong(true)));
    private void ParsePartyLeave() => AddOp(new PartyState.OpModify(_input.ReadInt(), 0, 0));
    private void ParsePartyLimitBreak() => AddOp(new PartyState.OpLimitBreakChange(_input.ReadInt(), _input.ReadInt()));

    private void ParseClientActionRequest()
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
        AddOp(new ClientState.OpActionRequest(req));
    }

    private void ParseClientActionReject()
    {
        var rej = new ClientActionReject()
        {
            Action = _input.ReadAction(),
            SourceSequence = _input.ReadUInt(false),
        };
        (rej.RecastElapsed, rej.RecastTotal) = _input.ReadFloatPair();
        rej.LogMessageID = _input.ReadUInt(false);
        AddOp(new ClientState.OpActionReject(rej));
    }

    private void ParseClientCountdown(bool start) => AddOp(new ClientState.OpCountdownChange(start ? _input.ReadFloat() : null));

    private void ParseCooldown()
    {
        var reset = _input.ReadBool();
        List<(int, Cooldown)> cooldowns = [];
        cooldowns.Capacity = _input.ReadByte(false);
        for (int i = 0; i < cooldowns.Capacity; ++i)
            cooldowns.Add((_input.ReadByte(false), new(_input.ReadFloat(), _input.ReadFloat())));
        AddOp(new ClientState.OpCooldown(reset, cooldowns));
    }

    private void ParseClientDutyActions() => AddOp(new ClientState.OpDutyActionsChange(_input.ReadAction(), _input.ReadAction()));

    private void ParseClientBozjaHolster()
    {
        List<(BozjaHolsterID, byte)> contents = [];
        contents.Capacity = _input.ReadByte(false);
        for (int i = 0; i < contents.Capacity; ++i)
            contents.Add(((BozjaHolsterID)_input.ReadByte(false), _input.ReadByte(false)));
        AddOp(new ClientState.OpBozjaHolsterChange(contents));
    }

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
