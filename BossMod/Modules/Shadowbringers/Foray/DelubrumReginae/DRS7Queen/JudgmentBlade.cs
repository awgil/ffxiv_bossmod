using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7Queen
{
    class JudgmentBlade : Components.GenericAOEs
    {
        private AOEInstance? _aoe;

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_aoe != null)
                yield return _aoe.Value;
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            var offset = (AID)spell.Action.ID switch
            {
                AID.JudgmentBladeRAOE => -10,
                AID.JudgmentBladeLAOE => +10,
                _ => 0
            };
            if (offset != 0)
                _aoe = new(new AOEShapeRect(70, 15), caster.Position + offset * spell.Rotation.ToDirection().OrthoL(), spell.Rotation, spell.NPCFinishAt);
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.JudgmentBladeRAOE or AID.JudgmentBladeLAOE)
            {
                _aoe = null;
                ++NumCasts;
            }
        }
    }
}
