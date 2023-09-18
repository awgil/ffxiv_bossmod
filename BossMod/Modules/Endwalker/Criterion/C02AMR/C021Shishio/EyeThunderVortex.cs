using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Criterion.C02AMR.C021Shishio
{
    class EyeThunderVortex : Components.GenericAOEs
    {
        private List<AOEInstance> _aoes = new();

        private static AOEShapeCircle _shapeCircle = new(15);
        private static AOEShapeDonut _shapeDonut = new(8, 30);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(1);

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            switch ((AID)spell.Action.ID)
            {
                case AID.NEyeOfTheThunderVortexFirst:
                case AID.SEyeOfTheThunderVortexFirst:
                    _aoes.Add(new(_shapeCircle, caster.Position, default, spell.FinishAt));
                    _aoes.Add(new(_shapeDonut, caster.Position, default, spell.FinishAt.AddSeconds(4)));
                    break;
                case AID.NVortexOfTheThunderEyeFirst:
                case AID.SVortexOfTheThunderEyeFirst:
                    _aoes.Add(new(_shapeDonut, caster.Position, default, spell.FinishAt));
                    _aoes.Add(new(_shapeCircle, caster.Position, default, spell.FinishAt.AddSeconds(4)));
                    break;
            }
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            if ((AID)spell.Action.ID is AID.NEyeOfTheThunderVortexFirst or AID.NEyeOfTheThunderVortexSecond or AID.NVortexOfTheThunderEyeFirst or AID.NVortexOfTheThunderEyeSecond
                or AID.SEyeOfTheThunderVortexFirst or AID.SEyeOfTheThunderVortexSecond or AID.SVortexOfTheThunderEyeFirst or AID.SVortexOfTheThunderEyeSecond)
            {
                if (_aoes.Count > 0)
                    _aoes.RemoveAt(0);
                ++NumCasts;
            }
        }
    }
}
