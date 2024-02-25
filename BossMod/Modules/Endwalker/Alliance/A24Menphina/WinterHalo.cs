using System.Collections.Generic;

namespace BossMod.Endwalker.Alliance.A24Menphina
{
    class WinterHalo : Components.GenericAOEs
    {
        private AOEInstance? _aoe;

        private static AOEShapeDonut _shape = new(10, 60);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_aoe != null)
                yield return _aoe.Value;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.WinterHaloShortAOE or AID.WinterHaloLongMountedAOE or AID.WinterHaloLongDismountedAOE)
                _aoe = new(_shape, caster.Position, spell.Rotation, spell.NPCFinishAt);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.WinterHaloShortAOE or AID.WinterHaloLongMountedAOE or AID.WinterHaloLongDismountedAOE)
            {
                ++NumCasts;
                _aoe = null;
            }
        }
    }
}
