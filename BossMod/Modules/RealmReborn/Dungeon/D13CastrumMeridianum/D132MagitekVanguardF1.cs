namespace BossMod.RealmReborn.Dungeon.D13CastrumMeridianum.D132MagitekVanguardF1;

public enum OID : uint
{
    Boss = 0x38CD, // x1
    Helper = 0x233C, // x7
}

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
}

class ThermobaricCharge(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThermobaricCharge), new AOEShapeCircle(20));
class HyperchargeInner(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HyperchargeInner), new AOEShapeCircle(10));
class HyperchargeOuter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HyperchargeOuter), new AOEShapeDonut(12.5f, 30));
class TargetedSupport(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TargetedSupportAOE), new AOEShapeCircle(5));
class CermetDrill(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.CermetDrill));
class Overcharge(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Overcharge), new AOEShapeCone(11, 60.Degrees()));

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

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 15, NameID = 2116)]
public class D132MagitekVanguardF1(WorldState ws, Actor primary) : BossModule(ws, primary, new(-13, 31), new ArenaBoundsRect(20, 20, 20.Degrees()));
