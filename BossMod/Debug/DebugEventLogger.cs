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
            _ws.ActorCreated += ActorCreated;
            _ws.ActorDestroyed += ActorDestroyed;
            _ws.ActorMoved += ActorMoved;
            _ws.ActorIsDeadChanged += ActorIsDeadChanged;
            _ws.ActorTargetChanged += ActorTargetChanged;
            _ws.ActorCastStarted += ActorCastStarted;
            _ws.ActorCastFinished += ActorCastFinished;
            _ws.ActorStatusGain += ActorStatusGain;
            _ws.ActorStatusLose += ActorStatusLose;
        }

        public void Dispose()
        {
            _ws.CurrentZoneChanged -= ZoneChange;
            _ws.PlayerInCombatChanged -= EnterExitCombat;
            _ws.PlayerActorIDChanged -= PlayerIDChanged;
            _ws.ActorCreated -= ActorCreated;
            _ws.ActorDestroyed -= ActorDestroyed;
            _ws.ActorMoved -= ActorMoved;
            _ws.ActorIsDeadChanged -= ActorIsDeadChanged;
            _ws.ActorTargetChanged -= ActorTargetChanged;
            _ws.ActorCastStarted -= ActorCastStarted;
            _ws.ActorCastFinished -= ActorCastFinished;
            _ws.ActorStatusGain -= ActorStatusGain;
            _ws.ActorStatusLose -= ActorStatusLose;
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

        private void ActorCreated(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[Actor] New actor: {Utils.ObjectString(actor.InstanceID)}, kind={actor.Type}, position={Utils.Vec3String(actor.Position)}, rotation={Utils.RadianString(actor.Rotation)}, playerOrPet={IsPlayerOrPet(actor)}");
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
            Service.Log($"[Actor] Cast started: caster={Utils.ObjectString(actor.InstanceID)}, target={Utils.ObjectString(actor.CastInfo!.TargetID)}, action={Utils.ActionString(actor.CastInfo!.ActionID)}, time={Utils.CastTimeString(actor.CastInfo!.CurrentTime, actor.CastInfo!.TotalTime)}, casterpos={Utils.Vec3String(actor.Position)}, targetpos={Utils.Vec3String(actor.CastInfo!.Location)}, casterrot={Utils.RadianString(actor.Rotation)}, playerOrPet={IsPlayerOrPet(actor)}");
        }

        private void ActorCastFinished(object? sender, WorldState.Actor actor)
        {
            Service.Log($"[Actor] Cast finished: caster={Utils.ObjectString(actor.InstanceID)}, target={Utils.ObjectString(actor.CastInfo!.TargetID)}, action={Utils.ActionString(actor.CastInfo!.ActionID)}, time={Utils.CastTimeString(actor.CastInfo!.CurrentTime, actor.CastInfo!.TotalTime)}, casterpos={Utils.Vec3String(actor.Position)}, targetpos={Utils.Vec3String(actor.CastInfo!.Location)}, casterrot={Utils.RadianString(actor.Rotation)}, playerOrPet={IsPlayerOrPet(actor)}");
        }

        private void ActorStatusGain(object? sender, (WorldState.Actor actor, int index) arg)
        {
            var s = arg.actor.Statuses[arg.index];
            var src = _ws.FindActor(s.SourceID);
            Service.Log($"[Actor] Status +++ {Utils.StatusString(s.ID)}: param={s.Param}, stacks={s.StackCount}, time={s.RemainingTime:f2}, {Utils.ObjectString(s.SourceID)} -> {Utils.ObjectString(arg.actor.InstanceID)}, playerOrPet={(src != null ? IsPlayerOrPet(src) : false)}");
        }

        private void ActorStatusLose(object? sender, (WorldState.Actor actor, int index) arg)
        {
            var s = arg.actor.Statuses[arg.index];
            var src = _ws.FindActor(s.SourceID);
            Service.Log($"[Actor] Status --- {Utils.StatusString(s.ID)}: param={s.Param}, stacks={s.StackCount}, time={s.RemainingTime:f2}, {Utils.ObjectString(s.SourceID)} -> {Utils.ObjectString(arg.actor.InstanceID)}, playerOrPet={(src != null ? IsPlayerOrPet(src) : false)}");
        }

        private bool IsPlayerOrPet(WorldState.Actor actor)
        {
            return actor.Type == WorldState.ActorType.Player || actor.Type == WorldState.ActorType.Pet || actor.Type == WorldState.ActorType.Chocobo;
        }
    }
}
