namespace BossMod.Endwalker.Criterion.C01ASS.C010Kaluk
{
    public enum OID : uint
    {
        Boss = 0x3AD6, // R2.800, x1
    };

    public enum AID : uint
    {
        AutoAttack = 31320, // Boss->player, no cast, single-target
        RightSweep = 31075, // Boss->self, 4.0s cast, range 30 210-degree cone aoe
        LeftSweep = 31076, // Boss->self, 4.0s cast, range 30 210-degree cone aoe
        CreepingIvy = 31077, // Boss->self, 3.0s cast, range 10 90-degree cone aoe
    };

    class RightSweep : Components.SelfTargetedAOEs
    {
        public RightSweep() : base(ActionID.MakeSpell(AID.RightSweep), new AOEShapeCone(30, 105.Degrees())) { }
    }

    class LeftSweep : Components.SelfTargetedAOEs
    {
        public LeftSweep() : base(ActionID.MakeSpell(AID.LeftSweep), new AOEShapeCone(30, 105.Degrees())) { }
    }

    class CreepingIvy : Components.SelfTargetedAOEs
    {
        public CreepingIvy() : base(ActionID.MakeSpell(AID.CreepingIvy), new AOEShapeCone(10, 45.Degrees())) { }
    }

    class C010KalukStates : StateMachineBuilder
    {
        public C010KalukStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<RightSweep>()
                .ActivateOnEnter<LeftSweep>()
                .ActivateOnEnter<CreepingIvy>();
        }
    }

    public class C010Kaluk : SimpleBossModule
    {
        public C010Kaluk(WorldState ws, Actor primary) : base(ws, primary) { }
    }
}
