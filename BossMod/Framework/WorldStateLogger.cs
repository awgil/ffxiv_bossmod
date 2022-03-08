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
        private DirectoryInfo _logDir;
        private StreamWriter? _logger = null;

        public WorldStateLogger(WorldState ws, GeneralConfig config, DirectoryInfo logDir)
        {
            _ws = ws;
            _config = config;
            _logDir = logDir;

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
            if (_logger == null)
            {
                try
                {
                    _logDir.Create();
                    _logger = new StreamWriter($"{_logDir.FullName}/World_{DateTime.Now:yyyy_MM_dd_HH_mm_ss}.log");
                }
                catch (IOException e)
                {
                    Service.Log($"Failed to create directory for log: {e}");
                    _config.DumpWorldStateEvents = false;
                    return;
                }

                // log initial state
                Log("VER ", 2);
                ZoneChange(null, _ws.CurrentZone);
                foreach (var actor in _ws.Actors)
                {
                    ActorCreated(null, actor);
                    if (actor.IsDead)
                        ActorIsDeadChanged(null, actor);
                    if (actor.InCombat)
                        ActorInCombatChanged(null, actor);
                    if (actor.TargetID != 0)
                        ActorTargetChanged(null, (actor, 0));
                    if (actor.CastInfo != null)
                        ActorCastStarted(null, actor);
                    if (actor.Tether.ID != 0)
                        ActorTethered(null, actor);
                    for (int i = 0; i < actor.Statuses.Length; ++i)
                        if (actor.Statuses[i].ID != 0)
                            ActorStatusGain(null, (actor, i));
                }
                for (int i = 0; i < _ws.Party.ContentIDs.Length; ++i)
                    if (_ws.Party.ContentIDs[i] != 0)
                        PartyJoined(null, (i, _ws.Party.ContentIDs[i], _ws.Party.Members[i]));
                for (var i = Waymark.A; i < Waymark.Count; ++i)
                {
                    var w = _ws.Waymarks[i];
                    if (w != null)
                        WaymarkChanged(null, (i, w));
                }

                // log changes
                _ws.CurrentZoneChanged += ZoneChange;
                _ws.Waymarks.Changed += WaymarkChanged;
                _ws.Actors.Added += ActorCreated;
                _ws.Actors.Removed += ActorDestroyed;
                _ws.Actors.Renamed += ActorRenamed;
                _ws.Actors.ClassChanged += ActorClassChanged;
                _ws.Actors.Moved += ActorMoved;
                _ws.Actors.IsTargetableChanged += ActorIsTargetableChanged;
                _ws.Actors.IsDeadChanged += ActorIsDeadChanged;
                _ws.Actors.InCombatChanged += ActorInCombatChanged;
                _ws.Actors.TargetChanged += ActorTargetChanged;
                _ws.Actors.CastStarted += ActorCastStarted;
                _ws.Actors.CastFinished += ActorCastFinished;
                _ws.Actors.Tethered += ActorTethered;
                _ws.Actors.Untethered += ActorUntethered;
                _ws.Actors.StatusGain += ActorStatusGain;
                _ws.Actors.StatusLose += ActorStatusLose;
                _ws.Actors.StatusChange += ActorStatusChange;
                _ws.Party.Joined += PartyJoined;
                _ws.Party.Left += PartyLeft;
                _ws.Party.Reassigned += PartyReassigned;
                _ws.Events.Icon += EventIcon;
                _ws.Events.Cast += EventCast;
                _ws.Events.EnvControl += EventEnvControl;
            }
        }

        private void Unsubscribe()
        {
            if (_logger != null)
            {
                _ws.CurrentZoneChanged -= ZoneChange;
                _ws.Waymarks.Changed -= WaymarkChanged;
                _ws.Actors.Added -= ActorCreated;
                _ws.Actors.Removed -= ActorDestroyed;
                _ws.Actors.Renamed -= ActorRenamed;
                _ws.Actors.ClassChanged -= ActorClassChanged;
                _ws.Actors.Moved -= ActorMoved;
                _ws.Actors.IsTargetableChanged -= ActorIsTargetableChanged;
                _ws.Actors.IsDeadChanged -= ActorIsDeadChanged;
                _ws.Actors.InCombatChanged -= ActorInCombatChanged;
                _ws.Actors.TargetChanged -= ActorTargetChanged;
                _ws.Actors.CastStarted -= ActorCastStarted;
                _ws.Actors.CastFinished -= ActorCastFinished;
                _ws.Actors.Tethered -= ActorTethered;
                _ws.Actors.Untethered -= ActorUntethered;
                _ws.Actors.StatusGain -= ActorStatusGain;
                _ws.Actors.StatusLose -= ActorStatusLose;
                _ws.Actors.StatusChange -= ActorStatusChange;
                _ws.Party.Joined -= PartyJoined;
                _ws.Party.Left -= PartyLeft;
                _ws.Party.Reassigned -= PartyReassigned;
                _ws.Events.Icon -= EventIcon;
                _ws.Events.Cast -= EventCast;
                _ws.Events.EnvControl -= EventEnvControl;

                _logger.Dispose();
                _logger = null;
            }
        }

        private void Log(string type, object msg)
        {
            if (_logger != null)
                _logger.WriteLine($"{_ws.CurrentTime:O}|{type}|{msg}");
        }

        private string Vec3(Vector3 v)
        {
            return $"{v.X:f3}/{v.Y:f3}/{v.Z:f3}";
        }

        private string Actor(Actor actor)
        {
            return $"{actor.InstanceID:X8}/{actor.OID:X}/{actor.Name}/{actor.Type}/{Vec3(actor.Position)}/{Utils.RadianString(actor.Rotation)}";
        }

        private string Actor(uint instanceID)
        {
            var actor = (instanceID != 0 && instanceID != Dalamud.Game.ClientState.Objects.Types.GameObject.InvalidGameObjectId) ? _ws.Actors.Find(instanceID) : null;
            return actor != null ? Actor(actor) : $"{instanceID:X8}";
        }

        private void ZoneChange(object? sender, ushort zone)
        {
            Log("ZONE", zone);
        }

        private void WaymarkChanged(object? sender, (Waymark i, Vector3? value) arg)
        {
            if (arg.value != null)
                Log("WAY+", $"{arg.i}|{Vec3(arg.value.Value)}");
            else
                Log("WAY-", arg.i);
        }

        private void ActorCreated(object? sender, Actor actor)
        {
            Log("ACT+", $"{Actor(actor)}|{actor.Class}|{actor.IsTargetable}|{actor.HitboxRadius:f3}|{Actor(actor.OwnerID)}");
        }

        private void ActorDestroyed(object? sender, Actor actor)
        {
            Log("ACT-", Actor(actor));
        }

        private void ActorRenamed(object? sender, (Actor actor, string oldName) arg)
        {
            Log("NAME", $"{Actor(arg.actor)}|{arg.oldName}");
        }

        private void ActorClassChanged(object? sender, (Actor actor, Class prevClass) arg)
        {
            Log("CLSR", $"{Actor(arg.actor)}|{arg.prevClass}|{arg.actor.Class}");
        }

        private void ActorMoved(object? sender, (Actor actor, Vector4 prevPosRot) arg)
        {
            Log("MOVE", Actor(arg.actor));
        }

        private void ActorIsTargetableChanged(object? sender, Actor actor)
        {
            Log(actor.IsTargetable ? "ATG+" : "ATG-", Actor(actor));
        }

        private void ActorIsDeadChanged(object? sender, Actor actor)
        {
            Log(actor.IsDead ? "DIE+" : "DIE-", Actor(actor));
        }

        private void ActorInCombatChanged(object? sender, Actor actor)
        {
            Log(actor.InCombat ? "COM+" : "COM-", Actor(actor));
        }

        private void ActorTargetChanged(object? sender, (Actor actor, uint prev) arg)
        {
            Log("TARG", $"{Actor(arg.actor)}|{Actor(arg.actor.TargetID)}");
        }

        private void ActorCastStarted(object? sender, Actor actor)
        {
            Log("CST+", $"{Actor(actor)}|{actor.CastInfo!.Action}|{Actor(actor.CastInfo!.TargetID)}|{Vec3(actor.CastInfo!.Location)}|{Utils.CastTimeString(actor.CastInfo!, _ws.CurrentTime)}");
        }

        private void ActorCastFinished(object? sender, Actor actor)
        {
            Log("CST-", $"{Actor(actor)}|{actor.CastInfo!.Action}|{Actor(actor.CastInfo!.TargetID)}|{Vec3(actor.CastInfo!.Location)}|{Utils.CastTimeString(actor.CastInfo!, _ws.CurrentTime)}");
        }

        private void ActorTethered(object? sender, Actor actor)
        {
            Log("TET+", $"{Actor(actor)}|{actor.Tether.ID}|{Actor(actor.Tether.Target)}");
        }

        private void ActorUntethered(object? sender, Actor actor)
        {
            Log("TET-", $"{Actor(actor)}|{actor.Tether.ID}|{Actor(actor.Tether.Target)}");
        }

        private void ActorStatusGain(object? sender, (Actor actor, int index) arg)
        {
            var s = arg.actor.Statuses[arg.index];
            Log("STA+", $"{Actor(arg.actor)}|{arg.index}|{Utils.StatusString(s.ID)}|{s.Extra:X4}|{Utils.StatusTimeString(s.ExpireAt, _ws.CurrentTime)}|{Actor(s.SourceID)}");
        }

        private void ActorStatusLose(object? sender, (Actor actor, int index) arg)
        {
            var s = arg.actor.Statuses[arg.index];
            Log("STA-", $"{Actor(arg.actor)}|{arg.index}|{Utils.StatusString(s.ID)}|{s.Extra:X4}|{Utils.StatusTimeString(s.ExpireAt, _ws.CurrentTime)}|{Actor(s.SourceID)}");
        }

        private void ActorStatusChange(object? sender, (Actor actor, int index, ushort prevExtra, DateTime prevExpire) arg)
        {
            var s = arg.actor.Statuses[arg.index];
            Log("STA!", $"{Actor(arg.actor)}|{arg.index}|{Utils.StatusString(s.ID)}|{s.Extra:X4}|{Utils.StatusTimeString(s.ExpireAt, _ws.CurrentTime)}|{Actor(s.SourceID)}");
        }

        private void PartyJoined(object? sender, (int slot, ulong contentID, Actor? actor) arg)
        {
            Log("PAR+", $"{arg.slot}|{arg.contentID:X}|{arg.actor?.InstanceID ?? 0:X8}");
        }

        private void PartyLeft(object? sender, (int slot, ulong contentID, Actor? actor) arg)
        {
            Log("PAR-", $"{arg.slot}|{arg.contentID:X}|{arg.actor?.InstanceID ?? 0:X8}");
        }

        private void PartyReassigned(object? sender, (int slot, ulong contentID, Actor? actor) arg)
        {
            Log("PAR!", $"{arg.slot}|{arg.contentID:X}|{arg.actor?.InstanceID ?? 0:X8}");
        }

        private void EventIcon(object? sender, (uint actorID, uint iconID) arg)
        {
            Log("ICON", $"{Actor(arg.actorID)}|{arg.iconID}");
        }

        private void EventCast(object? sender, CastEvent info)
        {
            var sb = new StringBuilder($"{Actor(info.CasterID)}|{info.Action}|{Actor(info.MainTargetID)}|{info.AnimationLockTime:f2}|{info.MaxTargets}");
            foreach (var t in info.Targets)
            {
                sb.Append($"|{Actor(t.ID)}");
                for (int i = 0; i < 8; ++i)
                    if (t[i] != 0)
                        sb.Append($"!{t[i]:X16}");
            }
            Log("CST!", sb.ToString());
        }

        private void EventEnvControl(object? sender, (uint featureID, byte index, uint state) arg)
        {
            Log("ENVC", $"{arg.featureID:X8}|{arg.index:X2}|{arg.state:X8}");
        }
    }
}
