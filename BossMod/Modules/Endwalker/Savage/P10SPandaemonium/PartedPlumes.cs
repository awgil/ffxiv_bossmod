using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P10SPandaemonium
{
    class PartedPlumes : Components.SelfTargetedAOEs
    {
        public PartedPlumes() : base(ActionID.MakeSpell(AID.PartedPlumesAOE), new AOEShapeCone(50, 10.Degrees()), 16) { }

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            return ActiveCasters.Select((c, i) => new AOEInstance(Shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.FinishAt, i < 2 ? ArenaColor.Danger : ArenaColor.AOE));
        }
    }
    class PartedPlumesVoidzone : Components.GenericAOEs
    {
        public int castsleft;
        private bool active;
        private static readonly AOEShapeCircle circle = new(8);

        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
        {
            if (active && castsleft > 0)
                    yield return new(circle, new WPos(100,100), default, new());
        }

        public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
        {
            if ((AID)spell.Action.ID == AID.PartedPlumes)
                {
                    active = true;
                    castsleft = 18;
                }
            if ((AID)spell.Action.ID == AID.PartedPlumesAOE)
                --castsleft;
        }
        public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
        {
            if (active && castsleft > 0 && actor.Position.InCircle(new WPos(100,100), 8))
                {
                    hints.Add("GTFO from voidzone!");
                }
        }
    }
}