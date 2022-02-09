using System;
using System.Numerics;

namespace BossMod
{
    class DebugEventLogger : IDisposable
    {
        private WorldState _ws;
        private GeneralConfig _config;
        private bool _subscribed = false;

        public DebugEventLogger(WorldState ws, GeneralConfig config)
        {
            _ws = ws;
            _config = config;
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
            if (!_subscribed)
            {
                _ws.CurrentZoneChanged += ZoneChange;
                _ws.PlayerInCombatChanged += EnterExitCombat;
                _ws.PlayerActorIDChanged += PlayerIDChanged;
                _ws.WaymarkChanged += WaymarkChanged;
                _ws.ActorCreated += ActorCreated;
                _ws.ActorDestroyed += ActorDestroyed;
                _ws.ActorRenamed += ActorRenamed;
                _ws.ActorClassRoleChanged += ActorClassRoleChanged;
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
                _ws.EventIcon += EventIcon;
                _ws.EventCast += EventCast;
                _ws.EventEnvControl += EventEnvControl;
                _subscribed = true;
            }
        }

        private void Unsubscribe()
        {
            if (_subscribed)
            {
                _ws.CurrentZoneChanged -= ZoneChange;
                _ws.PlayerInCombatChanged -= EnterExitCombat;
                _ws.PlayerActorIDChanged -= PlayerIDChanged;
                _ws.WaymarkChanged -= WaymarkChanged;
                _ws.ActorCreated -= ActorCreated;
                _ws.ActorDestroyed -= ActorDestroyed;
                _ws.ActorRenamed -= ActorRenamed;
                _ws.ActorClassRoleChanged -= ActorClassRoleChanged;
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
                _ws.EventIcon -= EventIcon;
                _ws.EventCast -= EventCast;
                _ws.EventEnvControl -= EventEnvControl;
                _subscribed = false;
            }
        }

        private void ZoneChange(object? sender, ushort zone)
        {
            Service.Log($"[World] ZoneChange|{zone}");
        }

        private void EnterExitCombat(object? sender, bool inCombat)
        {
            if (inCombat)
                Service.Log($"[World] Combat+++|{Utils.ObjectString(Service.ClientState.LocalPlayer?.TargetObjectId ?? 0)}");
            else
                Service.Log($"[World] Combat---");
        }

        private void PlayerIDChanged(object? sender, uint id)
        {
            Service.Log($"[World] PlayerID|{Utils.ObjectString(id)}");
        }

        private void WaymarkChanged(object? sender, (WorldState.Waymark i, Vector3? value) arg)
        {
            if (arg.value != null)
                Service.Log($"[World] Waymark|{arg.i}|{Utils.Vec3String(arg.value.Value)}");
            else
                Service.Log($"[World] Waymark|{arg.i}|inactive");
        }

        private void ActorCreated(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[World] Actor+++|{ActorString(actor)}|{Utils.CharacterClassString(actor.ClassID)}|{actor.Role}|{actor.IsTargetable}");
        }

        private void ActorDestroyed(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[World] Actor---|{ActorString(actor)}");
        }

        private void ActorRenamed(object? sender, (WorldState.Actor actor, string oldName) arg)
        {
            Service.Log($"[World] ActorRename|{ActorString(arg.actor)}|{arg.oldName}");
        }

        private void ActorClassRoleChanged(object? sender, (WorldState.Actor actor, uint prevClass, WorldState.ActorRole prevRole) arg)
        {
            Service.Log($"[World] ActorClassChange|{ActorString(arg.actor)}|{Utils.CharacterClassString(arg.prevClass)}|{arg.prevRole}|{Utils.CharacterClassString(arg.actor.ClassID)}|{arg.actor.Role}");
        }

        private void ActorMoved(object? sender, (WorldState.Actor actor, Vector3 prevPos, float prevRot) arg)
        {
            Service.Log($"[World] Move|{ActorString(arg.actor)}");
        }

        private void ActorIsTargetableChanged(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[World] ActorTargetable|{ActorString(actor)}|{actor.IsTargetable}");
        }

        private void ActorIsDeadChanged(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[World] ActorDead|{ActorString(actor)}|{actor.IsDead}");
        }

        private void ActorTargetChanged(object? sender, (WorldState.Actor actor, uint prev) arg)
        {
            Service.Log($"[World] ActorTarget|{ActorString(arg.actor)}|{ActorString(arg.actor.TargetID)}");
        }

        private void ActorCastStarted(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[World] Cast+++|{ActorString(actor)}|{Utils.ActionString(actor.CastInfo!.ActionID, actor.CastInfo!.ActionType)}|{ActorString(actor.CastInfo!.TargetID)}|{Utils.CastTimeString(actor.CastInfo!.CurrentTime, actor.CastInfo!.TotalTime)}");
        }

        private void ActorCastFinished(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[World] Cast---|{ActorString(actor)}|{Utils.ActionString(actor.CastInfo!.ActionID, actor.CastInfo!.ActionType)}|{ActorString(actor.CastInfo!.TargetID)}|{Utils.CastTimeString(actor.CastInfo!.CurrentTime, actor.CastInfo!.TotalTime)}");
        }

        private void ActorTethered(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[World] Tether+++|{ActorString(actor)}|{actor.Tether.ID}|{ActorString(actor.Tether.Target)}");
        }

        private void ActorUntethered(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[World] Tether---|{ActorString(actor)}|{actor.Tether.ID}|{ActorString(actor.Tether.Target)}");
        }

        private void ActorStatusGain(object? sender, (WorldState.Actor actor, int index) arg)
        {
            var s = arg.actor.Statuses[arg.index];
            Service.Log($"[World] Status+++|{ActorString(arg.actor)}|{arg.index}|{Utils.StatusString(s.ID)}|{s.Param}|{s.StackCount}|{s.RemainingTime:f2}|{ActorString(s.SourceID)}");
        }

        private void ActorStatusLose(object? sender, (WorldState.Actor actor, int index) arg)
        {
            var s = arg.actor.Statuses[arg.index];
            Service.Log($"[World] Status---|{ActorString(arg.actor)}|{arg.index}|{Utils.StatusString(s.ID)}|{s.Param}|{s.StackCount}|{s.RemainingTime:f2}|{ActorString(s.SourceID)}");
        }

        private void EventIcon(object? sender, (uint actorID, uint iconID) arg)
        {
            Service.Log($"[World] Icon|{ActorString(arg.actorID)}|{arg.iconID}");
        }

        private void EventCast(object? sender, WorldState.CastResult info)
        {
            Service.Log($"[World] Cast|{ActorString(info.CasterID)}|{Utils.ActionString(info.ActionID, info.ActionType)}|{ActorString(info.MainTargetID)}|{info.NumTargets}/{info.MaxTargets}|{info.AnimationLockTime:f2}");
        }

        private void EventEnvControl(object? sender, (uint featureID, byte index, uint state) arg)
        {
            Service.Log($"[World] EnvControl|{arg.featureID:X8}|{arg.index}|{arg.state:X8}");
        }

        private string ActorString(WorldState.Actor actor)
        {
            return $"{actor.OID:X} '{actor.Name}' <{actor.InstanceID:X}> ({actor.Type}) @ {Utils.Vec3String(actor.Position)}::{Utils.RadianString(actor.Rotation)}";
        }

        private string ActorString(uint instanceID)
        {
            if (instanceID == 0 || instanceID == Dalamud.Game.ClientState.Objects.Types.GameObject.InvalidGameObjectId)
                return "null";
            var actor = _ws.FindActor(instanceID);
            return actor != null ? ActorString(actor) : $"(not found) <{instanceID:X}>";
        }
    }
}
