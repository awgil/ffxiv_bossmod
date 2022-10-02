namespace BossMod.RealmReborn.Dungeon.D13CastrumMeridianum.D132MagitekVanguardF1
{
    public enum OID : uint
    {
        Boss = 0x38CD, // x1
        Helper = 0x233C, // x7
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        ThermobaricStrike = 28778, // Boss->self, 4.0s cast, single-target, visual
        ThermobaricCharge = 28779, // Helper->self, 7.0s cast, range 60 circle aoe with ? falloff
        Hypercharge = 28780, // Boss->self, 4.1s cast, single-target, visual
        HyperchargeInner = 28781, // Helper->self, 5.0s cast, range 10 circle
        HyperchargeOuter = 28782, // Helper->self, 5.0s cast, range ?-30 donut
        TargetedSupport = 28783, // Boss->self, 4.0s cast, single-target, visual
        TargetedSupportAOE = 28784, // Helper->self, 3.0s cast, range 5 circle aoe
        CermetDrill = 28785, // Boss->player, 5.0s cast, single-target tankbuster
        Overcharge = 29146, // Boss->self, 3.0s cast, range 11 120-degree cone aoe
    };

    class ThermobaricCharge : Components.SelfTargetedAOEs
    {
        public ThermobaricCharge() : base(ActionID.MakeSpell(AID.ThermobaricCharge), new AOEShapeCircle(20)) { }
    }

    class HyperchargeInner : Components.SelfTargetedAOEs
    {
        public HyperchargeInner() : base(ActionID.MakeSpell(AID.HyperchargeInner), new AOEShapeCircle(10)) { }
    }

    class HyperchargeOuter : Components.SelfTargetedAOEs
    {
        public HyperchargeOuter() : base(ActionID.MakeSpell(AID.HyperchargeOuter), new AOEShapeDonut(12.5f, 30)) { }
    }

    class TargetedSupport : Components.SelfTargetedAOEs
    {
        public TargetedSupport() : base(ActionID.MakeSpell(AID.TargetedSupportAOE), new AOEShapeCircle(5)) { }
    }

    class CermetDrill : Components.SingleTargetCast
    {
        public CermetDrill() : base(ActionID.MakeSpell(AID.CermetDrill)) { }
    }

    class Overcharge : Components.SelfTargetedAOEs
    {
        public Overcharge() : base(ActionID.MakeSpell(AID.Overcharge), new AOEShapeCone(11, 60.Degrees())) { }
    }

    class D132MagitekVanguardF1States : StateMachineBuilder
    {
        public D132MagitekVanguardF1States(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<ThermobaricCharge>()
                .ActivateOnEnter<HyperchargeInner>()
                .ActivateOnEnter<HyperchargeOuter>()
                .ActivateOnEnter<TargetedSupport>()
                .ActivateOnEnter<CermetDrill>()
                .ActivateOnEnter<Overcharge>();
        }
    }

    public class D132MagitekVanguardF1 : BossModule
    {
        public D132MagitekVanguardF1(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsRect(new(-13, 31), 20, 20, 20.Degrees())) { }
    }
}
