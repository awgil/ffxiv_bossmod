namespace BossMod.Endwalker.Criterion.C01ASS.C010Kaluk
{
    class RightSweep : Components.SelfTargetedAOEs
    {
        public RightSweep(ActionID aid) : base(aid, new AOEShapeCone(30, 105.Degrees())) { }
    }

    class LeftSweep : Components.SelfTargetedAOEs
    {
        public LeftSweep(ActionID aid) : base(aid, new AOEShapeCone(30, 105.Degrees())) { }
    }

    class CreepingIvy : Components.SelfTargetedAOEs
    {
        public CreepingIvy(ActionID aid) : base(aid, new AOEShapeCone(10, 45.Degrees())) { }
    }

    namespace Normal
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

        class NRightSweep : RightSweep { public NRightSweep() : base(ActionID.MakeSpell(AID.RightSweep)) { } }
        class NLeftSweep : LeftSweep { public NLeftSweep() : base(ActionID.MakeSpell(AID.LeftSweep)) { } }
        class NCreepingIvy : CreepingIvy { public NCreepingIvy() : base(ActionID.MakeSpell(AID.CreepingIvy)) { } }

        class C010NKalukStates : StateMachineBuilder
        {
            public C010NKalukStates(BossModule module) : base(module)
            {
                TrivialPhase()
                    .ActivateOnEnter<NRightSweep>()
                    .ActivateOnEnter<NLeftSweep>()
                    .ActivateOnEnter<NCreepingIvy>();
            }
        }

        public class C010NKaluk : SimpleBossModule
        {
            public C010NKaluk(WorldState ws, Actor primary) : base(ws, primary) { }
        }
    }

    namespace Savage
    {
        public enum OID : uint
        {
            Boss = 0x3ADF, // R2.800, x1
        };

        public enum AID : uint
        {
            AutoAttack = 31320, // Boss->player, no cast, single-target
            RightSweep = 31099, // Boss->self, 4.0s cast, range 30 210-degree cone aoe
            LeftSweep = 31100, // Boss->self, 4.0s cast, range 30 210-degree cone aoe
            CreepingIvy = 31101, // Boss->self, 3.0s cast, range 10 90-degree cone aoe
        };

        class SRightSweep : RightSweep { public SRightSweep() : base(ActionID.MakeSpell(AID.RightSweep)) { } }
        class SLeftSweep : LeftSweep { public SLeftSweep() : base(ActionID.MakeSpell(AID.LeftSweep)) { } }
        class SCreepingIvy : CreepingIvy { public SCreepingIvy() : base(ActionID.MakeSpell(AID.CreepingIvy)) { } }

        class C010SKalukStates : StateMachineBuilder
        {
            public C010SKalukStates(BossModule module) : base(module)
            {
                TrivialPhase()
                    .ActivateOnEnter<SRightSweep>()
                    .ActivateOnEnter<SLeftSweep>()
                    .ActivateOnEnter<SCreepingIvy>();
            }
        }

        public class C010SKaluk : SimpleBossModule
        {
            public C010SKaluk(WorldState ws, Actor primary) : base(ws, primary) { }
        }
    }
}
