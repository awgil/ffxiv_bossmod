using System.Collections.Generic;

namespace BossMod.Endwalker.Alliance.A24Menphina
{
    class MidnightFrostWaxingClaw : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeCone _shape = new(60, 90.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.MidnightFrostShortNormalFrontAOE or AID.MidnightFrostShortNormalBackAOE or
                AID.MidnightFrostShortMountedFrontAOE or AID.MidnightFrostShortMountedBackAOE or
                AID.MidnightFrostLongMountedFrontAOE or AID.MidnightFrostLongMountedBackAOE or
                AID.MidnightFrostLongDismountedFrontAOE or AID.MidnightFrostLongDismountedBackAOE or
                AID.WaxingClawRight or AID.WaxingClawLeft)
            {
                _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.MidnightFrostShortNormalFrontAOE or AID.MidnightFrostShortNormalBackAOE or
                AID.MidnightFrostShortMountedFrontAOE or AID.MidnightFrostShortMountedBackAOE or
                AID.MidnightFrostLongMountedFrontAOE or AID.MidnightFrostLongMountedBackAOE or
                AID.MidnightFrostLongDismountedFrontAOE or AID.MidnightFrostLongDismountedBackAOE or
                AID.WaxingClawRight or AID.WaxingClawLeft)
            {
                ++NumCasts;
                _aoes.Clear();
            }
        }
    }
}
