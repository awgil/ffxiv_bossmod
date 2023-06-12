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
}
