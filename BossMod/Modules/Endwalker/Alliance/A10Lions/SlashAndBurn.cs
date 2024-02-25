using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A10Lions
{
    class SlashAndBurn : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeCircle _shapeOut = new(14);
        private static AOEShapeDonut _shapeIn = new(6, 30);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(1);

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            AOEShape? shape = (AID)spell.Action.ID switch
            {
                AID.SlashAndBurnOutFirst or AID.SlashAndBurnOutSecond or AID.TrialByFire => _shapeOut,
                AID.SlashAndBurnInFirst or AID.SlashAndBurnInSecond or AID.SpinningSlash => _shapeIn,
                _ => null
            };
            if (shape != null)
            {
                _aoes.Add(new(shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
                _aoes.SortBy(aoe => aoe.Activation);
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.SlashAndBurnOutFirst or AID.SlashAndBurnOutSecond or AID.SlashAndBurnInFirst or AID.SlashAndBurnInSecond or AID.TrialByFire or AID.SpinningSlash)
                _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}
