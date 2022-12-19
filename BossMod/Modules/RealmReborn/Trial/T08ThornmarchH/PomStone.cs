using System;
using System.Collections.Generic;

namespace BossMod.RealmReborn.Trial.T08ThornmarchH
{
    class PomStone : Components.GenericAOEs
    {
        private List<(Actor caster, AOEShape shape)> _casters = new();
        private static AOEShapeCircle _shapeIn = new(10);
        private static AOEShapeDonut _shapeMid = new(10, 20);
        private static AOEShapeDonut _shapeOut = new(20, 30);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (_casters.Count > 0)
            {
                var deadline = _casters[0].caster.CastInfo!.FinishAt.AddSeconds(1);
                foreach (var c in _casters)
                    if (c.caster.CastInfo!.FinishAt < deadline)
                        yield return new(c.shape, c.caster.Position, c.caster.CastInfo.Rotation, c.caster.CastInfo.FinishAt);
            }
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            AOEShape? shape = (AID)spell.Action.ID switch
            {
                AID.PomStoneIn => _shapeIn,
                AID.PomStoneMid => _shapeMid,
                AID.PomStoneOut => _shapeOut,
                _ => null
            };
            if (shape != null)
                _casters.Add((caster, shape));
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID is AID.PomStoneIn or AID.PomStoneMid or AID.PomStoneOut)
                _casters.RemoveAll(c => c.caster == caster);
        }
    }
}
