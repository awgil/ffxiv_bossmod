using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Alliance.A23Halone
{
    class Tetrapagos : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(1);

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            AOEShape? shape = (AID)spell.Action.ID switch
            {
                AID.TetrapagosHailstormPrepare => new AOEShapeCircle(10),
                AID.TetrapagosSwirlPrepare => new AOEShapeDonut(10, 30),
                AID.TetrapagosRightrimePrepare or AID.TetrapagosLeftrimePrepare => new AOEShapeCone(30, 90.Degrees()),
                _ => null
            };
            if (shape != null)
                _aoes.Add(new(shape, caster.Position, caster.Rotation, spell.NPCFinishAt.AddSeconds(7.3f)));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.TetrapagosHailstormAOE or AID.TetrapagosSwirlAOE or AID.TetrapagosRightrimeAOE or AID.TetrapagosLeftrimeAOE)
            {
                ++NumCasts;
                if (_aoes.Count > 0)
                    _aoes.RemoveAt(0);
            }
        }
    }
}
