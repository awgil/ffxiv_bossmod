using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A10Lions
{
    class RoaringBlaze : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeCone _shape = new(50, 90.Degrees());

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(1);

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.RoaringBlazeFirst or AID.RoaringBlazeSecond or AID.RoaringBlazeSolo)
            {
                _aoes.Add(new(_shape, caster.Position, spell.Rotation, spell.FinishAt));
                _aoes.SortBy(aoe => aoe.Activation);
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.RoaringBlazeFirst or AID.RoaringBlazeSecond or AID.RoaringBlazeSolo)
                _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}
