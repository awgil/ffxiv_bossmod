using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.Savage.P10SPandaemonium
{
    class DividingWings : Components.BaitAwayTethers
    {
        public DividingWings() : base(new AOEShapeCone(60, 60.Degrees()), (uint)TetherID.DividingWings, ActionID.MakeSpell(AID.DividingWingsAOE)) { }
    }

    class PandaemonsHoly : Components.SelfTargetedAOEs
    {
        public PandaemonsHoly() : base(ActionID.MakeSpell(AID.PandaemonsHoly), new AOEShapeCircle(36)) { }
    }

    // note: origin seems to be weird?
    class CirclesOfPandaemonium : Components.SelfTargetedAOEs
    {
        public CirclesOfPandaemonium() : base(ActionID.MakeSpell(AID.CirclesOfPandaemonium), new AOEShapeDonut(12, 40)) { }
        public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => ActiveCasters.Select(c => new AOEInstance(Shape, new(module.Bounds.Center.X, Border.MainPlatformCenterZ - Border.MainPlatformHalfSize.Z), c.CastInfo!.Rotation, c.CastInfo.FinishAt, Color, Risky));
    }

    class Imprisonment : Components.SelfTargetedAOEs
    {
        public Imprisonment() : base(ActionID.MakeSpell(AID.ImprisonmentAOE), new AOEShapeCircle(4)) { }
    }

    class Cannonspawn : Components.SelfTargetedAOEs
    {
        public Cannonspawn() : base(ActionID.MakeSpell(AID.CannonspawnAOE), new AOEShapeDonut(3, 8)) { }
    }

    class PealOfDamnation : Components.SelfTargetedAOEs
    {
        public PealOfDamnation() : base(ActionID.MakeSpell(AID.PealOfDamnation), new AOEShapeRect(50, 3.5f)) { }
    }

    class PandaemoniacPillars : Components.CastTowers
    {
        public PandaemoniacPillars() : base(ActionID.MakeSpell(AID.Bury), 2) { }
    }

    class Touchdown : Components.SelfTargetedAOEs
    {
        public Touchdown() : base(ActionID.MakeSpell(AID.TouchdownAOE), new AOEShapeCircle(20)) { }
    }

    [ConfigDisplay(Order = 0x1A0, Parent = typeof(EndwalkerConfig))]
    public class P10SPandaemoniumConfig : CooldownPlanningConfigNode
    {
        public P10SPandaemoniumConfig() : base(90) { }
    }

    public class P10SPandaemonium : BossModule
    {
        public P10SPandaemonium(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(100, 92.5f), 30, 22.5f)) { }
    }
}
