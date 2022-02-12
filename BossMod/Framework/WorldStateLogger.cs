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
        }

        public void Dispose()
        {
            Unsubscribe();
        }

        public void Update()
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
                ZoneChange(null, _ws.CurrentZone);
                foreach (var actor in _ws.Actors.Values)
                {
                    ActorCreated(null, actor);
                    if (actor.IsDead)
                        ActorIsDeadChanged(null, actor);
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
                PlayerIDChanged(null, _ws.PlayerActorID);
                for (var i = WorldState.Waymark.A; i < WorldState.Waymark.Count; ++i)
                {
                    var w = _ws.GetWaymark(i);
                    if (w != null)
                        WaymarkChanged(null, (i, w));
                }
                if (_ws.PlayerInCombat)
                    EnterExitCombat(null, true);

                // log changes
                _ws.CurrentZoneChanged += ZoneChange;
                _ws.PlayerInCombatChanged += EnterExitCombat;
                _ws.PlayerActorIDChanged += PlayerIDChanged;
                _ws.WaymarkChanged += WaymarkChanged;
                _ws.ActorCreated += ActorCreated;
                _ws.ActorDestroyed += ActorDestroyed;
                _ws.ActorRenamed += ActorRenamed;
                _ws.ActorClassChanged += ActorClassChanged;
                _ws.ActorMoved += ActorMoved;
                _ws.ActorIsTargetableChanged += ActorIsTargetableChanged;
                _ws.ActorIsDeadChanged += ActorIsDeadChanged;
                _ws.ActorTargetChanged += ActorTargetChanged;
                _ws.ActorCastStarted += ActorCastStarted;
                _ws.ActorCastFinished += ActorCastFinished;
                _ws.ActorTethered += ActorTethered;
                _ws.ActorUntethered += ActorUntethered;
                _ws.ActorStatusGain += ActorStatusGain;
                _ws.ActorStatusLose += ActorStatusLose;
                _ws.ActorStatusChange += ActorStatusChange;
                _ws.EventIcon += EventIcon;
                _ws.EventCast += EventCast;
                _ws.EventEnvControl += EventEnvControl;
            }
        }

        private void Unsubscribe()
        {
            if (_logger != null)
            {
                _ws.CurrentZoneChanged -= ZoneChange;
                _ws.PlayerInCombatChanged -= EnterExitCombat;
                _ws.PlayerActorIDChanged -= PlayerIDChanged;
                _ws.WaymarkChanged -= WaymarkChanged;
                _ws.ActorCreated -= ActorCreated;
                _ws.ActorDestroyed -= ActorDestroyed;
                _ws.ActorRenamed -= ActorRenamed;
                _ws.ActorClassChanged -= ActorClassChanged;
                _ws.ActorMoved -= ActorMoved;
                _ws.ActorIsTargetableChanged -= ActorIsTargetableChanged;
                _ws.ActorIsDeadChanged -= ActorIsDeadChanged;
                _ws.ActorTargetChanged -= ActorTargetChanged;
                _ws.ActorCastStarted -= ActorCastStarted;
                _ws.ActorCastFinished -= ActorCastFinished;
                _ws.ActorTethered -= ActorTethered;
                _ws.ActorUntethered -= ActorUntethered;
                _ws.ActorStatusGain -= ActorStatusGain;
                _ws.ActorStatusLose -= ActorStatusLose;
                _ws.ActorStatusChange -= ActorStatusChange;
                _ws.EventIcon -= EventIcon;
                _ws.EventCast -= EventCast;
                _ws.EventEnvControl -= EventEnvControl;

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

        private string StatusTime(DateTime expireAt)
        {
            return $"{(expireAt != DateTime.MaxValue ? (expireAt - _ws.CurrentTime).TotalSeconds : 0):f3}";
        }

        private string Actor(WorldState.Actor actor)
        {
            return $"{actor.InstanceID:X8}/{actor.OID:X}/{actor.Name}/{actor.Type}/{Vec3(actor.Position)}/{Utils.RadianString(actor.Rotation)}";
        }

        private string Actor(uint instanceID)
        {
            var actor = (instanceID != 0 && instanceID != Dalamud.Game.ClientState.Objects.Types.GameObject.InvalidGameObjectId) ? _ws.FindActor(instanceID) : null;
            return actor != null ? Actor(actor) : $"{instanceID:X8}";
        }

        private void ZoneChange(object? sender, ushort zone)
        {
            Log("ZONE", zone);
        }

        private void EnterExitCombat(object? sender, bool inCombat)
        {
            Log("PCOM", inCombat);
        }

        private void PlayerIDChanged(object? sender, uint id)
        {
            Log("PID ", Actor(id));
        }

        private void WaymarkChanged(object? sender, (WorldState.Waymark i, Vector3? value) arg)
        {
            if (arg.value != null)
                Log("WAY+", $"{arg.i}|{Vec3(arg.value.Value)}");
            else
                Log("WAY-", arg.i);
        }

        private void ActorCreated(object? sender, WorldState.Actor actor)
        {
            Log("ACT+", $"{Actor(actor)}|{actor.Class}|{actor.IsTargetable}|{actor.HitboxRadius:f3}");
        }

        private void ActorDestroyed(object? sender, WorldState.Actor actor)
        {
            Log("ACT-", Actor(actor));
        }

        private void ActorRenamed(object? sender, (WorldState.Actor actor, string oldName) arg)
        {
            Log("NAME", $"{Actor(arg.actor)}|{arg.oldName}");
        }

        private void ActorClassChanged(object? sender, (WorldState.Actor actor, Class prevClass) arg)
        {
            Log("CLSR", $"{Actor(arg.actor)}|{arg.prevClass}|{arg.actor.Class}");
        }

        private void ActorMoved(object? sender, (WorldState.Actor actor, Vector4 prevPosRot) arg)
        {
            Log("MOVE", Actor(arg.actor));
        }

        private void ActorIsTargetableChanged(object? sender, WorldState.Actor actor)
        {
            Log(actor.IsTargetable ? "ATG+" : "ATG-", Actor(actor));
        }

        private void ActorIsDeadChanged(object? sender, WorldState.Actor actor)
        {
            Log(actor.IsDead ? "DIE+" : "DIE-", Actor(actor));
        }

        private void ActorTargetChanged(object? sender, (WorldState.Actor actor, uint prev) arg)
        {
            Log("TARG", $"{Actor(arg.actor)}|{Actor(arg.actor.TargetID)}");
        }

        private void ActorCastStarted(object? sender, WorldState.Actor actor)
        {
            Log("CST+", $"{Actor(actor)}|{actor.CastInfo!.Action}|{Actor(actor.CastInfo!.TargetID)}|{Vec3(actor.CastInfo!.Location)}|{Utils.CastTimeString((float)(actor.CastInfo!.FinishAt - _ws.CurrentTime).TotalSeconds, actor.CastInfo!.TotalTime)}");
        }

        private void ActorCastFinished(object? sender, WorldState.Actor actor)
        {
            Log("CST-", $"{Actor(actor)}|{actor.CastInfo!.Action}|{Actor(actor.CastInfo!.TargetID)}|{Vec3(actor.CastInfo!.Location)}|{Utils.CastTimeString((float)(actor.CastInfo!.FinishAt - _ws.CurrentTime).TotalSeconds, actor.CastInfo!.TotalTime)}");
        }

        private void ActorTethered(object? sender, WorldState.Actor actor)
        {
            Log("TET+", $"{Actor(actor)}|{actor.Tether.ID}|{Actor(actor.Tether.Target)}");
        }

        private void ActorUntethered(object? sender, WorldState.Actor actor)
        {
            Log("TET-", $"{Actor(actor)}|{actor.Tether.ID}|{Actor(actor.Tether.Target)}");
        }

        private void ActorStatusGain(object? sender, (WorldState.Actor actor, int index) arg)
        {
            var s = arg.actor.Statuses[arg.index];
            Log("STA+", $"{Actor(arg.actor)}|{arg.index}|{Utils.StatusString(s.ID)}|{s.Extra:X4}|{StatusTime(s.ExpireAt)}|{Actor(s.SourceID)}");
        }

        private void ActorStatusLose(object? sender, (WorldState.Actor actor, int index) arg)
        {
            var s = arg.actor.Statuses[arg.index];
            Log("STA-", $"{Actor(arg.actor)}|{arg.index}|{Utils.StatusString(s.ID)}|{s.Extra:X4}|{StatusTime(s.ExpireAt)}|{Actor(s.SourceID)}");
        }

        private void ActorStatusChange(object? sender, (WorldState.Actor actor, int index, ushort prevExtra, DateTime prevExpire) arg)
        {
            var s = arg.actor.Statuses[arg.index];
            Log("STA!", $"{Actor(arg.actor)}|{arg.index}|{Utils.StatusString(s.ID)}|{s.Extra:X4}|{StatusTime(s.ExpireAt)}|{Actor(s.SourceID)}");
        }

        private void EventIcon(object? sender, (uint actorID, uint iconID) arg)
        {
            Log("ICON", $"{Actor(arg.actorID)}|{arg.iconID}");
        }

        private void EventCast(object? sender, WorldState.CastResult info)
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
