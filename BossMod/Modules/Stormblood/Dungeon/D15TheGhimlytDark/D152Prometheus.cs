using Microsoft.Win32.SafeHandles;

namespace BossMod.Stormblood.Dungeon.D15TheGhimlytDark.D152Prometheus;

public enum OID : uint
{
    Helper = 0x233C, // R0.500, x11, Helper type
    Boss = 0x2515, // R7.800, x1
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x10 (spawn during fight), EventObj type
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 15046, // Boss->player, no cast, single-target
    FreezingMissile = 13403, // Boss->self, 3.0s cast, single-target
    FreezingMissileHelper = 13404, // Helper->location, 3.5s cast, range 40 circle
    Heat = 13400, // Helper->self, no cast, range 100+R width 16 rect // cast'd 5 times in a row, need a way to telegraph where this is coming from.
    NeedleGun = 13402, // Boss->self, 4.5s cast, range 40+R 90-degree cone
    Nitrospin = 13397, // Boss->self, 4.5s cast, range 50 circle
    OilShower = 13398, // Boss->self, 4.5s cast, range 40+R 270-degree cone
    Tunnel = 13399, // Boss->self, no cast, single-target
    UnbreakableCermetDrill = 13401, // Boss->player, 3.0s cast, single-target
}

class FreezingMissle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FreezingMissileHelper), new AOEShapeCircle(10));
class Heat(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Heat), new AOEShapeRect(100, 8));
class NeedleGun(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NeedleGun), new AOEShapeCone(47.8f, 45.Degrees()));
class Nitrospin(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Nitrospin));
class OilShower(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OilShower), new AOEShapeCone(47.8f, 135.Degrees()));
class UnbreakableCermetDrill(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.UnbreakableCermetDrill));

class D152PrometheusStates : StateMachineBuilder
{
    public D152PrometheusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FreezingMissle>()
            .ActivateOnEnter<Heat>()
            .ActivateOnEnter<NeedleGun>()
            .ActivateOnEnter<Nitrospin>()
            .ActivateOnEnter<OilShower>()
            .ActivateOnEnter<UnbreakableCermetDrill>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 611, NameID = 7856)]
public class D152Prometheus(WorldState ws, Actor primary) : BossModule(ws, primary, new(132.5f, -36f), new ArenaBoundsCircle(22));
