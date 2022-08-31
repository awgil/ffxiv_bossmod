using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Components
{
    public class ChargeAOEs : GenericAOEs
    {
        public int HalfWidth { get; private init; }
        private List<(Actor, AOEShape, Angle)> _casters = new();
        public IReadOnlyList<(Actor, AOEShape, Angle)> Casters => _casters;

        public ChargeAOEs(ActionID aid, int halfWidth) : base(aid)
        {
            HalfWidth = halfWidth;
        }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module)
        {
            return Casters.Select(csr => (csr.Item2, csr.Item1.Position, csr.Item3, csr.Item1.CastInfo!.FinishAt));
        }

        public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
            {
                var dir = spell.LocXZ - caster.Position;
                _casters.Add((caster, new AOEShapeRect(dir.Length(), HalfWidth), Angle.FromDirection(dir)));
            }
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if (spell.Action == WatchedAction)
                _casters.RemoveAll(e => e.Item1 == caster);
        }
    }
}
