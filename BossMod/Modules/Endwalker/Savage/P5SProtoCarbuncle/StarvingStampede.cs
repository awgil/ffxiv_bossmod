using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle
{
    class StarvingStampede : Components.GenericAOEs
    {
        private List<WPos> _positions = new();

        private static AOEShape _shape = new AOEShapeCircle(12);

        public StarvingStampede() : base(ActionID.MakeSpell(AID.StarvingStampede)) { }

        public override IEnumerable<(AOEShape shape, WPos origin, Angle rotation, DateTime time)> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            // TODO: timings...
            return _positions.Skip(NumCasts).Take(3).Select(p => (_shape, p, new Angle(), module.WorldState.CurrentTime));
        }

        public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
        {
            base.OnEventCast(module, caster, spell);
            switch ((AID)spell.Action.ID)
            {
                case AID.JawsTeleport:
                    if (_positions.Count == 0)
                        _positions.Add(caster.Position);
                    _positions.Add(new(spell.TargetPos.XZ()));
                    break;
            }
        }
    }
}
