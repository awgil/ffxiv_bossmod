using System.Collections.Generic;

namespace BossMod.Endwalker.Alliance.A23Halone
{
    class ThousandfoldThrust : Components.GenericAOEs
    {
        private AOEInstance? _aoe;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_aoe != null)
                yield return _aoe.Value;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.ThousandfoldThrustAOEFirst)
                _aoe = new(new AOEShapeCone(30, 90.Degrees()), caster.Position, spell.Rotation, spell.NPCFinishAt);
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.ThousandfoldThrustAOEFirst or AID.ThousandfoldThrustAOERest)
                ++NumCasts;
        }
    }
}
