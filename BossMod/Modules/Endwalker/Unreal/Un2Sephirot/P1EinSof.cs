using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Unreal.Un2Sephirot
{
    class P1EinSof : Components.GenericAOEs
    {
        private List<(Actor, int)> _active = new();

        private static AOEShape _shape = new AOEShapeCircle(10); // TODO: verify radius

        public bool Active => _active.Count > 0;

        public P1EinSof() : base(ActionID.MakeSpell(AID.EinSofAOE)) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module)
        {
            return _active.Select(ai => (_shape, ai.Item1.Position, ai.Item1.Rotation, module.WorldState.CurrentTime));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            if (spell.Action == WatchedAction)
            {
                var index = _active.FindIndex(ai => ai.Item1 == caster);
                if (index < 0)
                {
                    _active.Add((caster, 1));
                }
                else
                {
                    _active[index] = (caster, _active[index].Item2 + 1);
                    if (_active[index].Item2 > 10)
                        _active.RemoveAt(index);
                }
            }
        }
    }
}
