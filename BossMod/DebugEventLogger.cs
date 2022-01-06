using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Logging;
using System;

namespace BossMod
{
    class DebugEventLogger : IDisposable
    {
        private EventGenerator _gen;

        public DebugEventLogger(EventGenerator gen)
        {
            _gen = gen;
            _gen.CurrentZoneChanged += ZoneChange;
            _gen.PlayerCombatEnter += EnterCombat;
            _gen.PlayerCombatExit += ExitCombat;
            _gen.ActorCreated += ActorCreated;
            _gen.ActorDestroyed += ActorDestroyed;
            _gen.ActorCastStarted += ActorCastStarted;
            _gen.ActorCastFinished += ActorCastFinished;
            _gen.ActorTeleported += ActorTeleported;
        }

        public void Dispose()
        {
            _gen.CurrentZoneChanged -= ZoneChange;
            _gen.PlayerCombatEnter -= EnterCombat;
            _gen.PlayerCombatExit -= ExitCombat;
            _gen.ActorCreated -= ActorCreated;
            _gen.ActorDestroyed -= ActorDestroyed;
            _gen.ActorCastStarted -= ActorCastStarted;
            _gen.ActorCastFinished -= ActorCastFinished;
            _gen.ActorTeleported -= ActorTeleported;
        }

        private void ZoneChange(object? sender, ushort zone)
        {
            PluginLog.Log($"Zone changed to {zone}");
        }

        private void EnterCombat(object? sender, PlayerCharacter? pc)
        {
            PluginLog.Log($"Entering combat, target = '{_gen.ActorString(pc?.TargetObjectId ?? 0)}'");
        }

        private void ExitCombat(object? sender, PlayerCharacter? pc)
        {
            PluginLog.Log($"Exiting combat");
        }

        private void ActorCreated(object? sender, EventGenerator.Actor actor)
        {
            PluginLog.Log($"New actor: {Utils.ObjectString(actor.Chara)}, kind={Utils.ObjectKindString(actor.Chara)}, position={Utils.Vec3String(actor.Chara.Position)}, rotation={Utils.RadianString(actor.Chara.Rotation)}, playerOrPet={actor.IsPlayerOrPet}");
        }

        private void ActorDestroyed(object? sender, EventGenerator.Actor actor)
        {
            PluginLog.Log($"Removed actor: id={actor.ObjectID:X}, playerOrPet={actor.IsPlayerOrPet}");
        }

        private void ActorCastStarted(object? sender, EventGenerator.Actor actor)
        {
            PluginLog.Log($"Cast started: caster={Utils.ObjectString(actor.Chara)}, target={_gen.ActorString(actor.CastTargetID)}, action={Utils.ActionString(actor.CastActionID)}, time={Utils.CastTimeString(actor.CastCurrentTime, actor.CastTotalTime)}, casterpos={Utils.Vec3String(actor.Chara.Position)}, targetpos={Utils.Vec3String(actor.CastLocation)}, casterrot={Utils.RadianString(actor.Chara.Rotation)}, playerOrPet={actor.IsPlayerOrPet}");
        }

        private void ActorCastFinished(object? sender, EventGenerator.Actor actor)
        {
            PluginLog.Log($"Cast finished: caster={Utils.ObjectString(actor.Chara)}, target={_gen.ActorString(actor.CastTargetID)}, action={Utils.ActionString(actor.CastActionID)}, time={Utils.CastTimeString(actor.CastCurrentTime, actor.CastTotalTime)}, casterpos={Utils.Vec3String(actor.Chara.Position)}, targetpos={Utils.Vec3String(actor.CastLocation)}, casterrot={Utils.RadianString(actor.Chara.Rotation)}, playerOrPet={actor.IsPlayerOrPet}");
        }

        private void ActorTeleported(object? sender, EventGenerator.Actor actor)
        {
            PluginLog.Log($"Actor teleported: {Utils.ObjectString(actor.Chara)}, position={Utils.Vec3String(actor.Chara.Position)}, rotation={Utils.RadianString(actor.Chara.Rotation)}, playerOrPet={actor.IsPlayerOrPet}");
        }
    }
}
