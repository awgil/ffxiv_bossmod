using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;

namespace BossMod
{
    public class ReplayParserLog : ReplayParser
    {
        abstract class Input : IDisposable
        {
            public abstract void Dispose();
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

        class TextInput : Input
        {
            private StreamReader _input;
            private DateTime _timestamp;
            private string[] _line = new string[0];
            private int _nextPayload;

            public DateTime Timestamp => _timestamp;

            public TextInput(Stream stream)
            {
                _input = new(stream);
            }

            public override void Dispose()
            {
                _input.Dispose();
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

                    _timestamp = DateTime.Parse(_line[0]);
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
                int sep = sid.IndexOf(' ');
                return new()
                {
                    ID = uint.Parse(sep >= 0 ? sid.AsSpan(0, sep) : sid.AsSpan()),
                    Extra = ushort.Parse(ReadString(), NumberStyles.HexNumber),
                    ExpireAt = Timestamp.AddSeconds(ReadFloat()),
                    SourceID = ReadActorID()
                };
            }
            public override List<ActorCastEvent.Target> ReadTargets()
            {
                List<ActorCastEvent.Target> res = new();
                while (CanRead())
                {
                    var parts = ReadString().Split('!');
                    ActorCastEvent.Target target = new();
                    target.ID = ParseActorID(parts[0]);
                    for (int j = 1; j < parts.Length; ++j)
                        target.Effects[j - 1] = ulong.Parse(parts[j], NumberStyles.HexNumber);
                    res.Add(target);
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
                return (_timestamp.AddSeconds(float.Parse(parts[0])), float.Parse(parts[1]));
            }
            public override ulong ReadActorID() => ParseActorID(ReadString());

            private ulong ParseActorID(string actor)
            {
                var sep = actor.IndexOf('/');
                return ulong.Parse(sep >= 0 ? actor.AsSpan(0, sep) : actor.AsSpan(), NumberStyles.HexNumber);
            }
        }

        class BinaryInput : Input
        {
            private BinaryReader _input;

            public BinaryInput(Stream stream)
            {
                _input = new(stream);
            }

            public override void Dispose()
            {
                _input.Dispose();
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
            public override ActorStatus ReadStatus() => new() { ID = _input.ReadUInt32(), Extra = _input.ReadUInt16(), ExpireAt = new(_input.ReadInt64()), SourceID = _input.ReadUInt64() };
            public override List<ActorCastEvent.Target> ReadTargets()
            {
                var count = _input.ReadInt32();
                List<ActorCastEvent.Target> res = new(count);
                for (int i = 0; i < count; ++i)
                {
                    var t = new ActorCastEvent.Target() { ID = _input.ReadUInt64() };
                    for (int j = 0; j < 8; ++j)
                        t.Effects[j] = _input.ReadUInt64();
                    res.Add(t);
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

        private Input _input;
        private int _version = 0;
        private DateTime _tsStart;
        private ulong _qpcStart;

        private ReplayParserLog(Input input)
        {
            _input = input;
        }

        public override void Dispose()
        {
            base.Dispose();
            _input.Dispose();
        }

        private bool ParseLine()
        {
            var tag = _input.NextEntry();
            if (tag.Length < 4)
                return false;

            if (_version is > 0 and < 5 && _input is TextInput ti && (_res.Ops.Count == 0 || _res.Ops.Last().Timestamp < ti.Timestamp))
                AddOp(new WorldState.OpFrameStart() { Frame = new() { Timestamp = ti.Timestamp } });

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
                case "PAR+": ParsePartyJoin(); break; // legacy (up to v3)
                case "PAR-": ParsePartyLeave(); break; // legacy (up to v3)
                case "PAR!": ParsePartyAssign(); break; // legacy (up to v3)
                case "CLAR": ParseClientActionRequest(); break;
                case "CLRJ": ParseClientActionReject(); break;
                case "CDN+": ParseClientCountdown(true); break;
                case "CDN-": ParseClientCountdown(false); break;
            }

            return true;
        }

        private void ParseVersion()
        {
            _version = _input.ReadInt();
            if (_version < 2)
                throw new Exception($"Version {_version} is too old and is no longer supported, sorry");
            var qpf = _version >= 10 ? _input.ReadULong(false) : TimeSpan.TicksPerSecond; // newer windows versions have 10mhz qpc frequency
            var gameVersion = _version >= 11 ? _input.ReadString() : "old";
            _tsStart = _input is TextInput ti ? ti.Timestamp : new(_input.ReadLong());
            Start(_tsStart, qpf, gameVersion);
        }

        private void ParseFrameStart()
        {
            var prevUpdateTime = TimeSpan.FromMilliseconds(_input.ReadDouble());
            _input.ReadVoid();
            var gauge = _input.CanRead() ? _input.ReadULong(true) : 0;
            var op = new WorldState.OpFrameStart { PrevUpdateTime = prevUpdateTime, GaugePayload = gauge };
            if (_version >= 10)
            {
                op.Frame.QPC = _input.ReadULong(false);
                op.Frame.Index = _input.ReadUInt(false);
                op.Frame.DurationRaw = _input.ReadFloat();
                op.Frame.Duration = _input.ReadFloat();
                op.Frame.TickSpeedMultiplier = _input.ReadFloat();
                if (_qpcStart != 0)
                {
                    op.Frame.Timestamp = _tsStart.AddSeconds((double)(op.Frame.QPC - _qpcStart) / _res.QPF);
                }
                else
                {
                    _qpcStart = op.Frame.QPC;
                    op.Frame.Timestamp = _tsStart;
                }
            }
            else if (_input is TextInput ti)
            {
                op.Frame.Timestamp = ti.Timestamp;
                op.Frame.QPC = (ulong)(ti.Timestamp - _tsStart).Ticks;
                op.Frame.Index = _ws.Frame.Index + 1;
                op.Frame.Duration = op.Frame.DurationRaw = (float)(ti.Timestamp - _ws.CurrentTime).TotalSeconds;
                op.Frame.TickSpeedMultiplier = 1;
            }
            AddOp(op);
        }

        private void ParseUserMarker()
        {
            AddOp(new WorldState.OpUserMarker() { Text = _input.ReadString() });
        }

        private void ParseRSVData()
        {
            AddOp(new WorldState.OpRSVData() { Key = _input.ReadString(), Value = _input.ReadString() });
        }

        private void ParseZoneChange()
        {
            AddOp(new WorldState.OpZoneChange() { Zone = _input.ReadUShort(false) });
        }

        private void ParseDirectorUpdate()
        {
            AddOp(new WorldState.OpDirectorUpdate() {
                DirectorID = _input.ReadUInt(true),
                UpdateID = _input.ReadUInt(true),
                Param1 = _input.ReadUInt(true),
                Param2 = _input.ReadUInt(true),
                Param3 = _input.ReadUInt(true),
                Param4 = _input.ReadUInt(true),
            });
        }

        private void ParseEnvControl()
        {
            // director id field is removed in v11
            if (_version < 11)
                _input.ReadUInt(true);

            AddOp(new WorldState.OpEnvControl()
            {
                Index = _input.ReadByte(true),
                State = _input.ReadUInt(true),
            });
        }

        private void ParseWaymarkChange(bool set)
        {
            AddOp(new WaymarkState.OpWaymarkChange()
            {
                ID = _version < 10 ? Enum.Parse<Waymark>(_input.ReadString()) : (Waymark)_input.ReadByte(false),
                Pos = set ? _input.ReadVec3() : null,
            });
        }

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
                var hpmp = _input.CanRead() ? ActorHPMP(_input.ReadString()) : new();
                var ally = _input.CanRead() && _input.ReadBool();
                var spawnIndex = _input.CanRead() ? _input.ReadInt() : -1;
                op = new()
                {
                    InstanceID = ulong.Parse(parts[0], NumberStyles.HexNumber),
                    OID = uint.Parse(parts[1], NumberStyles.HexNumber),
                    SpawnIndex = spawnIndex,
                    Name = parts[2],
                    Type = Enum.Parse<ActorType>(parts[3]),
                    Class = cls,
                    PosRot = new(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]).Degrees().Rad),
                    HitboxRadius = radius,
                    HP = hpmp.hp,
                    CurMP = hpmp.curMP,
                    IsTargetable = targetable,
                    IsAlly = ally,
                    OwnerID = owner,
                };
            }
            else
            {
                op = new()
                {
                    InstanceID = _input.ReadULong(true),
                    OID = _input.ReadUInt(true),
                    SpawnIndex = _input.ReadInt(),
                    Name = _input.ReadString(),
                    Type = (ActorType)_input.ReadUShort(true),
                    Class = _input.ReadClass(),
                    PosRot = new(_input.ReadVec3(), _input.ReadAngle().Rad),
                    HitboxRadius = _input.ReadFloat(),
                    HP = new() { Cur = _input.ReadUInt(false), Max = _input.ReadUInt(false), Shield = _input.ReadUInt(false) },
                    CurMP = _input.ReadUInt(false),
                    IsTargetable = _input.ReadBool(),
                    IsAlly = _input.ReadBool(),
                    OwnerID = _input.ReadActorID(),
                };
            }
            AddOp(op);
        }

        private void ParseActorDestroy()
        {
            AddOp(new ActorState.OpDestroy() { InstanceID = _input.ReadActorID() });
        }

        private void ParseActorRename()
        {
            ActorState.OpRename op;
            if (_version < 10)
            {
                var parts = _input.ReadString().Split('/');
                op = new() { InstanceID = ulong.Parse(parts[0], NumberStyles.HexNumber), Name = parts[2] };
            }
            else
            {
                op = new() { InstanceID = _input.ReadULong(true), Name = _input.ReadString() };
            }
            AddOp(op);
        }

        private void ParseActorClassChange()
        {
            var instanceID = _input.ReadActorID();
            _input.ReadVoid();
            AddOp(new ActorState.OpClassChange() { InstanceID = instanceID, Class = _input.ReadClass() });
        }

        private void ParseActorMove()
        {
            ActorState.OpMove op;
            if (_version < 10)
            {
                var parts = _input.ReadString().Split('/');
                op = new() { InstanceID = ulong.Parse(parts[0], NumberStyles.HexNumber), PosRot = new(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]).Degrees().Rad) };
            }
            else
            {
                op = new() { InstanceID = _input.ReadULong(true), PosRot = new(_input.ReadVec3(), _input.ReadAngle().Rad) };
            }
            AddOp(op);
        }

        private void ParseActorSizeChange()
        {
            AddOp(new ActorState.OpSizeChange() { InstanceID = _input.ReadActorID(), HitboxRadius = _input.ReadFloat() });
        }

        private void ParseActorHPMP()
        {
            ActorState.OpHPMP op = new() { InstanceID = _input.ReadActorID() };
            if (_version < 10)
            {
                (op.HP, op.CurMP) = ActorHPMP(_input.ReadString());
            }
            else
            {
                op.HP = new() { Cur = _input.ReadUInt(false), Max = _input.ReadUInt(false), Shield = _input.ReadUInt(false) };
                op.CurMP = _input.ReadUInt(false);
            }
            AddOp(op);
        }

        private void ParseActorTargetable(bool targetable)
        {
            AddOp(new ActorState.OpTargetable() { InstanceID = _input.ReadActorID(), Value = targetable });
        }

        private void ParseActorAlly()
        {
            AddOp(new ActorState.OpDead() { InstanceID = _input.ReadActorID(), Value = _input.ReadBool() });
        }

        private void ParseActorDead(bool dead)
        {
            AddOp(new ActorState.OpDead() { InstanceID = _input.ReadActorID(), Value = dead });
        }

        private void ParseActorCombat(bool value)
        {
            AddOp(new ActorState.OpCombat() { InstanceID = _input.ReadActorID(), Value = value });
        }

        private void ParseActorModelState()
        {
            AddOp(new ActorState.OpModelState() { InstanceID = _input.ReadActorID(), Value = new() { ModelState = _input.ReadByte(false), AnimState1 = _input.CanRead() ? _input.ReadByte(false) : (byte)0, AnimState2 = _input.CanRead() ? _input.ReadByte(false) : (byte)0 } });
        }

        private void ParseActorEventState()
        {
            AddOp(new ActorState.OpEventState() { InstanceID = _input.ReadActorID(), Value = _input.ReadByte(false) });
        }

        private void ParseActorTarget()
        {
            AddOp(new ActorState.OpTarget() { InstanceID = _input.ReadActorID(), Value = _input.ReadActorID() });
        }

        private void ParseActorTether(bool tether)
        {
            AddOp(new ActorState.OpTether() { InstanceID = _input.ReadActorID(), Value = tether ? new() { ID = _input.ReadUInt(false), Target = _input.ReadActorID() } : new() });
        }

        private void ParseActorCastInfo(bool start)
        {
            var op = new ActorState.OpCastInfo() { InstanceID = _input.ReadActorID() };
            if (start)
            {
                var action = _input.ReadAction();
                var target = _input.ReadActorID();
                var loc = _input.ReadVec3();
                var (finishAt, totalTime) = _input.ReadTimePair();
                op.Value = new()
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
            AddOp(op);
        }

        private void ParseActorCastEvent()
        {
            AddOp(new ActorState.OpCastEvent()
            {
                InstanceID = _input.ReadActorID(),
                Value = new()
                {
                    Action = _input.ReadAction(),
                    MainTargetID = _input.ReadActorID(),
                    AnimationLockTime = _input.ReadFloat(),
                    MaxTargets = _input.ReadUInt(false),
                    TargetPos = _version >= 6 ? _input.ReadVec3() : new(),
                    GlobalSequence = _version >= 7 ? _input.ReadUInt(false) : 0,
                    SourceSequence = _version >= 9 ? _input.ReadUInt(false) : 0,
                    Targets = _input.ReadTargets()
                }
            });
        }

        private void ParseActorEffectResult()
        {
            AddOp(new ActorState.OpEffectResult() { InstanceID = _input.ReadActorID(), Seq = _input.ReadUInt(false), TargetIndex = _input.ReadInt() });
        }

        private void ParseActorStatus(bool gainOrUpdate)
        {
            AddOp(new ActorState.OpStatus()
            {
                InstanceID = _input.ReadActorID(),
                Index = _input.ReadInt(),
                Value = gainOrUpdate ? _input.ReadStatus() : new()
            });
        }

        private void ParseActorIcon()
        {
            AddOp(new ActorState.OpIcon() { InstanceID = _input.ReadActorID(), IconID = _input.ReadUInt(false) });
        }

        private void ParseActorEventObjectStateChange()
        {
            AddOp(new ActorState.OpEventObjectStateChange() { InstanceID = _input.ReadActorID(), State = _input.ReadUShort(true) });
        }

        private void ParseActorEventObjectAnimation()
        {
            AddOp(new ActorState.OpEventObjectAnimation() { InstanceID = _input.ReadActorID(), Param1 = _input.ReadUShort(true), Param2 = _input.ReadUShort(true) });
        }

        private void ParseActorPlayActionTimelineEvent()
        {
            AddOp(new ActorState.OpPlayActionTimelineEvent() { InstanceID = _input.ReadActorID(), ActionTimelineID = _input.ReadUShort(true) });
        }

        private void ParseActorEventNpcYell()
        {
            AddOp(new ActorState.OpEventNpcYell() { InstanceID = _input.ReadActorID(), Message = _input.ReadUShort(false) });
        }

        private void ParsePartyModify()
        {
            AddOp(new PartyState.OpModify()
            {
                Slot = _input.ReadInt(),
                ContentID = _input.ReadULong(true),
                InstanceID = _input.ReadULong(true),
            });
        }

        private void ParsePartyJoin()
        {
            AddOp(new PartyState.OpModify()
            {
                Slot = _input.ReadInt(),
                ContentID = _input.ReadULong(true),
                InstanceID = _input.ReadULong(true),
            });
        }

        private void ParsePartyLeave()
        {
            AddOp(new PartyState.OpModify() { Slot = _input.ReadInt() });
        }

        private void ParsePartyAssign()
        {
            AddOp(new PartyState.OpModify()
            {
                Slot = _input.ReadInt(),
                ContentID = _input.ReadULong(true),
                InstanceID = _input.ReadULong(true),
            });
        }

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
            AddOp(new ClientState.OpActionRequest() { Request = req });
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
            AddOp(new ClientState.OpActionReject() { Value = rej });
        }

        private void ParseClientCountdown(bool start)
        {
            AddOp(new ClientState.OpCountdownChange() { Value = start ? _input.ReadFloat() : null });
        }

        private static (ActorHP hp, uint curMP) ActorHPMP(string repr)
        {
            var parts = repr.Split('/');
            var hp = new ActorHP() { Cur = uint.Parse(parts[0]), Max = uint.Parse(parts[1]), Shield = parts.Length > 2 ? uint.Parse(parts[2]) : 0 };
            uint curMP = parts.Length > 3 ? uint.Parse(parts[3]) : 0;
            return (hp, curMP);
        }
    }
}
