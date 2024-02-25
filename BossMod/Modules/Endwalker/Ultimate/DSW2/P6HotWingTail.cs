using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Ultimate.DSW2
{
    class P6HotWingTail : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeRect _shapeWing = new(50, 10.5f);
        private static AOEShapeRect _shapeTail = new(50, 8);

        public int NumAOEs => _aoes.Count; // 0 if not started, 1 if tail, 2 if wings

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Skip(NumCasts);

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            AOEShape? shape = (AID)spell.Action.ID switch
            {
                AID.HotWingAOE => _shapeWing,
                AID.HotTailAOE => _shapeTail,
                _ => null
            };
            if (shape != null)
                _aoes.Add(new(shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
        }

        // note: we don't remove aoe's, since that is used e.g. by spreads component
        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.HotWingAOE or AID.HotTailAOE)
                ++NumCasts;
        }
    }
}
