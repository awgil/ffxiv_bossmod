using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Logging;
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
            _ws.ActorCastStarted += ActorCastStarted;
            _ws.ActorCastFinished += ActorCastFinished;
            _ws.ActorStatusAdded += ActorStatusAdded;
            _ws.ActorStatusRemoved += ActorStatusRemoved;
        }

        public void Dispose()
        {
            _ws.CurrentZoneChanged -= ZoneChange;
            _ws.PlayerInCombatChanged -= EnterExitCombat;
            _ws.PlayerActorIDChanged -= PlayerIDChanged;
            _ws.ActorCreated -= ActorCreated;
            _ws.ActorDestroyed -= ActorDestroyed;
            _ws.ActorMoved -= ActorMoved;
            _ws.ActorCastStarted -= ActorCastStarted;
            _ws.ActorCastFinished -= ActorCastFinished;
            _ws.ActorStatusAdded -= ActorStatusAdded;
            _ws.ActorStatusRemoved -= ActorStatusRemoved;
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
            Service.Log($"New actor: {Utils.ObjectString(actor.InstanceID)}, kind={actor.Type}, position={Utils.Vec3String(actor.Position)}, rotation={Utils.RadianString(actor.Rotation)}, playerOrPet={IsPlayerOrPet(actor)}");
        }

        private void ActorDestroyed(object? sender, WorldState.Actor actor)
        {
            Service.Log($"Removed actor: id={actor.InstanceID:X}, playerOrPet={IsPlayerOrPet(actor)}");
        }

        private void ActorMoved(object? sender, (WorldState.Actor actor, Vector3 prevPos, float prevRot) arg)
        {
            if ((arg.actor.Position - arg.prevPos).LengthSquared() < 4)
                return;
            Service.Log($"Actor teleported: {Utils.ObjectString(arg.actor.InstanceID)}, position={Utils.Vec3String(arg.actor.Position)}, rotation={Utils.RadianString(arg.actor.Rotation)}, playerOrPet={IsPlayerOrPet(arg.actor)}");
        }

        private void ActorCastStarted(object? sender, WorldState.Actor actor)
        {
            Service.Log($"Cast started: caster={Utils.ObjectString(actor.InstanceID)}, target={Utils.ObjectString(actor.CastInfo!.TargetID)}, action={Utils.ActionString(actor.CastInfo!.ActionID)}, time={Utils.CastTimeString(actor.CastInfo!.CurrentTime, actor.CastInfo!.TotalTime)}, casterpos={Utils.Vec3String(actor.Position)}, targetpos={Utils.Vec3String(actor.CastInfo!.Location)}, casterrot={Utils.RadianString(actor.Rotation)}, playerOrPet={IsPlayerOrPet(actor)}");
        }

        private void ActorCastFinished(object? sender, WorldState.Actor actor)
        {
            Service.Log($"Cast finished: caster={Utils.ObjectString(actor.InstanceID)}, target={Utils.ObjectString(actor.CastInfo!.TargetID)}, action={Utils.ActionString(actor.CastInfo!.ActionID)}, time={Utils.CastTimeString(actor.CastInfo!.CurrentTime, actor.CastInfo!.TotalTime)}, casterpos={Utils.Vec3String(actor.Position)}, targetpos={Utils.Vec3String(actor.CastInfo!.Location)}, casterrot={Utils.RadianString(actor.Rotation)}, playerOrPet={IsPlayerOrPet(actor)}");
        }

        private void ActorStatusAdded(object? sender, (WorldState.Actor actor, int index) arg)
        {
            var s = arg.actor.Statuses[arg.index];
            Service.Log($"Status applied: {Utils.ObjectString(arg.actor.InstanceID)}, status={Utils.StatusString(s.ID)}, param={s.Param}, stacks={s.StackCount}, time={s.RemainingTime:f2}, source={Utils.ObjectString(s.SourceID)}, position={Utils.Vec3String(arg.actor.Position)}, rotation={Utils.RadianString(arg.actor.Rotation)}, playerOrPet={IsPlayerOrPet(arg.actor)}");
        }

        private void ActorStatusRemoved(object? sender, (WorldState.Actor actor, int index) arg)
        {
            var s = arg.actor.Statuses[arg.index];
            Service.Log($"Status faded: {Utils.ObjectString(arg.actor.InstanceID)}, status={Utils.StatusString(s.ID)}, param={s.Param}, stacks={s.StackCount}, time={s.RemainingTime:f2}, source={Utils.ObjectString(s.SourceID)}, position={Utils.Vec3String(arg.actor.Position)}, rotation={Utils.RadianString(arg.actor.Rotation)}, playerOrPet={IsPlayerOrPet(arg.actor)}");
        }

        private bool IsPlayerOrPet(WorldState.Actor actor)
        {
            return actor.Type == WorldState.ActorType.Player || actor.Type == WorldState.ActorType.Pet || actor.Type == WorldState.ActorType.Chocobo;
        }
    }
}
