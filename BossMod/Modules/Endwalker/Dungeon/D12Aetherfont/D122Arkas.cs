// CONTRIB: made by dhoggpt, not checked
namespace BossMod.Endwalker.Dungeon.D12Aetherfont.D122Arkas
{
    public enum OID : uint
    {
        Boss = 0x3EEA, // ???
        Helper = 0x233C, // ???
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->none, no cast, single-target
        BattleCry1 = 33364, // Boss->self, 5.0s cast, range 40 circle
        BattleCry2 = 34605, // Boss->self, 5.0s cast, range 40 circle
        ElectricEruption = 33615, // Boss->self, 5.0s cast, range 40 circle
        Electrify = 33367, // Helper->location, 5.5s cast, range 10 circle
        LightningClaw1 = 33366, // Boss->location, no cast, range 6 circle
        LightningClaw2 = 34712, // Boss->none, 5.0s cast, single-target 
        LightningLeap1 = 33358, // Boss->location, 4.0s cast, single-target
        LightningLeap2 = 33359, // Boss->location, 5.0s cast, single-target
        LightningLeap3 = 33360, // Helper->location, 6.0s cast, range 10 circle
        LightningLeap4 = 34713, // Helper->location, 5.0s cast, range 10 circle
        LightningRampage1 = 34318, // Boss->location, 4.0s cast, single-target
        LightningRampage2 = 34319, // Boss->location, 2.0s cast, single-target
        LightningRampage3 = 34320, // Boss->location, 2.0s cast, single-target
        LightningRampage4 = 34321, // Helper->location, 5.0s cast, range 10 circle
        LightningRampage5 = 34714, // Helper->location, 5.0s cast, range 10 circle
        RipperClaw = 33368, // Boss->none, 5.0s cast, single-target
        Shock = 33365, // Helper->location, 3.5s cast, range 6 circle
        SpinningClaw = 33362, // Boss->self, 3.5s cast, range 10 circle
        ForkedFissures = 33361, // Helper->location, 1.0s cast, width 4 rect charge
        SpunLightning = 33363, // Helper->self, 3.5s cast, range 30 width 8 rect
    };

    class LightningRampage1 : Components.LocationTargetedAOEs
    {
        public LightningRampage1() : base(ActionID.MakeSpell(AID.LightningRampage1), 5) { }
    }

    class LightningRampage2 : Components.LocationTargetedAOEs
    {
        public LightningRampage2() : base(ActionID.MakeSpell(AID.LightningRampage2), 5) { }
    }

    class LightningRampage3 : Components.LocationTargetedAOEs
    {
        public LightningRampage3() : base(ActionID.MakeSpell(AID.LightningRampage3), 5) { }
    }

    class LightningLeap1 : Components.LocationTargetedAOEs
    {
        public LightningLeap1() : base(ActionID.MakeSpell(AID.LightningLeap1), 5) { }
    }

    class LightningLeap2 : Components.LocationTargetedAOEs
    {
        public LightningLeap2() : base(ActionID.MakeSpell(AID.LightningLeap2), 5) { }
    }

    class LightningClaw2 : Components.LocationTargetedAOEs
    {
        public LightningClaw2() : base(ActionID.MakeSpell(AID.LightningClaw2), 5) { }
    }

    class SpunLightning : Components.SelfTargetedAOEs
    {
        public SpunLightning() : base(ActionID.MakeSpell(AID.SpunLightning), new AOEShapeRect(8, 4, -50)) { }
    }

    class LightningClaw1 : Components.LocationTargetedAOEs
    {
        public LightningClaw1() : base(ActionID.MakeSpell(AID.LightningClaw1), 6) { }
    }

    class ForkedFissures : Components.ChargeAOEs
    {
        public ForkedFissures() : base(ActionID.MakeSpell(AID.ForkedFissures), 2) { }
    }

    class ElectricEruption : Components.RaidwideCast
    {
        public ElectricEruption() : base(ActionID.MakeSpell(AID.ElectricEruption)) { }
    }

    class Electrify : Components.LocationTargetedAOEs
    {
        public Electrify() : base(ActionID.MakeSpell(AID.Electrify), 10) { }
    }

    class LightningLeap3 : Components.LocationTargetedAOEs
    {
        public LightningLeap3() : base(ActionID.MakeSpell(AID.LightningLeap3), 10) { }
    }

    class LightningLeap4 : Components.LocationTargetedAOEs
    {
        public LightningLeap4() : base(ActionID.MakeSpell(AID.LightningLeap4), 10) { }
    }

    class LightningRampage5 : Components.LocationTargetedAOEs
    {
        public LightningRampage5() : base(ActionID.MakeSpell(AID.LightningRampage5), 10) { }
    }

    class LightningRampage4 : Components.LocationTargetedAOEs
    {
        public LightningRampage4() : base(ActionID.MakeSpell(AID.LightningRampage4), 10) { }
    }

    class RipperClaw : Components.SingleTargetCast
    {
        public RipperClaw() : base(ActionID.MakeSpell(AID.RipperClaw)) { }
    }

    class Shock : Components.LocationTargetedAOEs
    {
        public Shock() : base(ActionID.MakeSpell(AID.Shock), 6) { }
    }

    class SpinningClaw : Components.SelfTargetedAOEs
    {
        public SpinningClaw() : base(ActionID.MakeSpell(AID.SpinningClaw), new AOEShapeCircle(10)) { }
    }

    class BattleCry1 : Components.RaidwideCast
    {
        public BattleCry1() : base(ActionID.MakeSpell(AID.BattleCry1)) { }
    }

    class BattleCry2 : Components.RaidwideCast
    {
        public BattleCry2() : base(ActionID.MakeSpell(AID.BattleCry1)) { }
    }

    class D122ArkasStates : StateMachineBuilder
    {
        public D122ArkasStates(BossModule module) : base(module)
        {
            TrivialPhase()
                .ActivateOnEnter<LightningClaw1>()
                .ActivateOnEnter<LightningClaw2>()
                .ActivateOnEnter<SpunLightning>()
                .ActivateOnEnter<ForkedFissures>()
                .ActivateOnEnter<ElectricEruption>()
                .ActivateOnEnter<Electrify>()
                .ActivateOnEnter<LightningLeap1>()
                .ActivateOnEnter<LightningLeap2>()
                .ActivateOnEnter<LightningLeap3>()
                .ActivateOnEnter<LightningLeap4>()
                .ActivateOnEnter<LightningRampage1>()
                .ActivateOnEnter<LightningRampage2>()
                .ActivateOnEnter<LightningRampage3>()
                .ActivateOnEnter<LightningRampage4>()
                .ActivateOnEnter<LightningRampage5>()
                .ActivateOnEnter<RipperClaw>()
                .ActivateOnEnter<Shock>()
                .ActivateOnEnter<SpinningClaw>()
                .ActivateOnEnter<BattleCry1>()
                .ActivateOnEnter<BattleCry2>();
        }
    }

    [ModuleInfo(CFCID = 822, NameID = 12337)]
    public class D122Arkas : BossModule
    {
        public D122Arkas(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(425, -440), 12)) { }
    }
}
