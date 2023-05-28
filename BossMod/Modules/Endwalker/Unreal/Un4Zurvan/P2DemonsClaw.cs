using System.Collections.Generic;

namespace BossMod.Endwalker.Unreal.Un4Zurvan
{
    class P2DemonsClawKnockback : Components.Knockback
    {
        public P2DemonsClawKnockback() : base(ActionID.MakeSpell(AID.DemonsClaw), true) { }

        private Actor? _caster;

        public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
        {
            if (_caster?.CastInfo?.TargetID == actor.InstanceID)
                yield return new(_caster.Position, 17, _caster.CastInfo.FinishAt);
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _caster = caster;
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _caster = null;
        }
    }

    class P2DemonsClawWaveCannon : Components.GenericWildCharge
    {
        private ulong _targetID;

        public P2DemonsClawWaveCannon() : base(5, ActionID.MakeSpell(AID.WaveCannonShared)) { }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                Source = caster;
                foreach (var (slot, player) in module.Raid.WithSlot())
                {
                    PlayerRoles[slot] = player.InstanceID == _targetID ? PlayerRole.Target : PlayerRole.Share;
                }
            }
            else if ((AID)spell.Action.ID == AID.DemonsClaw)
            {
                _targetID = spell.TargetID;
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                Source = null;
        }
    }
}
