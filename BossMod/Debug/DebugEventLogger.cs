using System;
using System.Numerics;

namespace BossMod
{
    class DebugEventLogger : IDisposable
    {
        private WorldState _ws;

        public DebugEventLogger(WorldState ws)
        {
            _ws = ws;
            _ws.CurrentZoneChanged += ZoneChange;
            _ws.PlayerInCombatChanged += EnterExitCombat;
            _ws.PlayerActorIDChanged += PlayerIDChanged;
            _ws.WaymarkChanged += WaymarkChanged;
            _ws.ActorCreated += ActorCreated;
            _ws.ActorDestroyed += ActorDestroyed;
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
        }

        public void Dispose()
        {
            _ws.CurrentZoneChanged -= ZoneChange;
            _ws.PlayerInCombatChanged -= EnterExitCombat;
            _ws.PlayerActorIDChanged -= PlayerIDChanged;
            _ws.WaymarkChanged -= WaymarkChanged;
            _ws.ActorCreated -= ActorCreated;
            _ws.ActorDestroyed -= ActorDestroyed;
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
        }

        private void ZoneChange(object? sender, ushort zone)
        {
            Service.Log($"Zone changed to {zone}");
        }

        private void EnterExitCombat(object? sender, bool inCombat)
        {
            if (inCombat)
                Service.Log($"Entering combat, target = '{Utils.ObjectString(Service.ClientState.LocalPlayer?.TargetObjectId ?? 0)}'");
            else
                Service.Log($"Exiting combat");
        }

        private void PlayerIDChanged(object? sender, uint id)
        {
            Service.Log($"Player ID changed: {Utils.ObjectString(id)}");
        }

        private void WaymarkChanged(object? sender, (WorldState.Waymark i, Vector3? value) arg)
        {
            string str = arg.value != null ? $"at {Utils.Vec3String(arg.value.Value)}" : "inactive";
            Service.Log($"Waymark changed: {arg.i} is now {str}");
        }

        private void ActorCreated(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[Actor] New actor: {Utils.ObjectString(actor.InstanceID)}, kind={actor.Type}, position={Utils.Vec3String(actor.Position)}, rotation={Utils.RadianString(actor.Rotation)}, targetable={actor.IsTargetable}, playerOrPet={IsPlayerOrPet(actor)}");
        }

        private void ActorDestroyed(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[Actor] Removed actor: id={actor.InstanceID:X}, playerOrPet={IsPlayerOrPet(actor)}");
        }

        private void ActorMoved(object? sender, (WorldState.Actor actor, Vector3 prevPos, float prevRot) arg)
        {
            if ((arg.actor.Position - arg.prevPos).LengthSquared() < 4)
                return;
            Service.Log($"[Actor] Actor teleported: {Utils.ObjectString(arg.actor.InstanceID)}, position={Utils.Vec3String(arg.actor.Position)}, rotation={Utils.RadianString(arg.actor.Rotation)}, playerOrPet={IsPlayerOrPet(arg.actor)}");
        }

        private void ActorIsTargetableChanged(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[Actor] Actor is-targetable changed: {Utils.ObjectString(actor.InstanceID)} is now {(actor.IsTargetable ? "targetable" : "untargetable")}, position={Utils.Vec3String(actor.Position)}, rotation={Utils.RadianString(actor.Rotation)}, playerOrPet={IsPlayerOrPet(actor)}");
        }

        private void ActorIsDeadChanged(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[Actor] Actor is-dead changed: {Utils.ObjectString(actor.InstanceID)} is now {(actor.IsDead ? "dead" : "alive")}, position={Utils.Vec3String(actor.Position)}, rotation={Utils.RadianString(actor.Rotation)}, playerOrPet={IsPlayerOrPet(actor)}");
        }

        private void ActorTargetChanged(object? sender, (WorldState.Actor actor, uint prev) arg)
        {
            Service.Log($"[Actor] Actor target changed: {Utils.ObjectString(arg.actor.InstanceID)}, {Utils.ObjectString(arg.prev)} -> {Utils.ObjectString(arg.actor.TargetID)}, position={Utils.Vec3String(arg.actor.Position)}, rotation={Utils.RadianString(arg.actor.Rotation)}, playerOrPet={IsPlayerOrPet(arg.actor)}");
        }

        private void ActorCastStarted(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[Actor] Cast started: caster={Utils.ObjectString(actor.InstanceID)}, target={Utils.ObjectString(actor.CastInfo!.TargetID)}, action={Utils.ActionString(actor.CastInfo!.ActionID, actor.CastInfo!.ActionType)}, time={Utils.CastTimeString(actor.CastInfo!.CurrentTime, actor.CastInfo!.TotalTime)}, casterpos={Utils.Vec3String(actor.Position)}, targetpos={Utils.Vec3String(actor.CastInfo!.Location)}, casterrot={Utils.RadianString(actor.Rotation)}, playerOrPet={IsPlayerOrPet(actor)}");
        }

        private void ActorCastFinished(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[Actor] Cast finished: caster={Utils.ObjectString(actor.InstanceID)}, target={Utils.ObjectString(actor.CastInfo!.TargetID)}, action={Utils.ActionString(actor.CastInfo!.ActionID, actor.CastInfo!.ActionType)}, time={Utils.CastTimeString(actor.CastInfo!.CurrentTime, actor.CastInfo!.TotalTime)}, casterpos={Utils.Vec3String(actor.Position)}, targetpos={Utils.Vec3String(actor.CastInfo!.Location)}, casterrot={Utils.RadianString(actor.Rotation)}, playerOrPet={IsPlayerOrPet(actor)}");
        }

        private void ActorTethered(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[Actor] Tether +++ {actor.Tether.ID}: {Utils.ObjectString(actor.InstanceID)} -> {Utils.ObjectString(actor.Tether.Target)}");
        }

        private void ActorUntethered(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[Actor] Tether --- {actor.Tether.ID}: {Utils.ObjectString(actor.InstanceID)} -> {Utils.ObjectString(actor.Tether.Target)}");
        }

        private void ActorStatusGain(object? sender, (WorldState.Actor actor, int index) arg)
        {
            var s = arg.actor.Statuses[arg.index];
            var src = _ws.FindActor(s.SourceID);
            Service.Log($"[Actor] Status +++ {Utils.StatusString(s.ID)}: index={arg.index}, param={s.Param}, stacks={s.StackCount}, time={s.RemainingTime:f2}, {Utils.ObjectString(s.SourceID)} -> {Utils.ObjectString(arg.actor.InstanceID)}, playerOrPet={(src != null ? IsPlayerOrPet(src) : false)}");
        }

        private void ActorStatusLose(object? sender, (WorldState.Actor actor, int index) arg)
        {
            var s = arg.actor.Statuses[arg.index];
            var src = _ws.FindActor(s.SourceID);
            Service.Log($"[Actor] Status --- {Utils.StatusString(s.ID)}: index={arg.index}, param={s.Param}, stacks={s.StackCount}, time={s.RemainingTime:f2}, {Utils.ObjectString(s.SourceID)} -> {Utils.ObjectString(arg.actor.InstanceID)}, playerOrPet={(src != null ? IsPlayerOrPet(src) : false)}");
        }

        private void EventIcon(object? sender, (uint actorID, uint iconID) arg)
        {
            Service.Log($"[ActorEvent] Icon: {Utils.ObjectString(arg.actorID)}: {arg.iconID}");
        }

        private void EventCast(object? sender, WorldState.CastResult info)
        {
            var src = _ws.FindActor(info.CasterID);
            Service.Log($"[ActorEvent] Cast: {Utils.ActionString(info.ActionID, info.ActionType)}, {Utils.ObjectString(info.CasterID)} -> {Utils.ObjectString(info.MainTargetID)}, affected {info.NumTargets}/{info.MaxTargets} targets, lock={info.AnimationLockTime:f2}, playerOrPet={(src != null ? IsPlayerOrPet(src) : false)}");
        }

        private void EventEnvControl(object? sender, (uint featureID, byte index, uint state) arg)
        {
            Service.Log($"[ActorEvent] Env control: {arg.featureID:X8}.{arg.index}: {arg.state:X8}");
        }

        private bool IsPlayerOrPet(WorldState.Actor actor)
        {
            return actor.Type == WorldState.ActorType.Player || actor.Type == WorldState.ActorType.Pet || actor.Type == WorldState.ActorType.Chocobo;
        }
    }
}
