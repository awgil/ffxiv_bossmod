namespace BossMod.Heavensward.Dungeon.D03Aery.D032Gyascutus;

public enum OID : uint
{
    Boss = 0x3970, // R5.400, x?
    InflammableFumes = 0x3972, // R1.200, x?
    Helper = 0x233C, // x3
}

public enum AID : uint
{
    Attack = 872, // EA6/1098/1093/3970->player, no cast, single-target
    ProximityPyre = 30191, // 3970->self, 4.0s cast, range 12 circle
    InflammableFumes = 30181, // 3970->self, 4.0s cast, single-target
    Burst = 30184, // 233C->self, 10.0s cast, range 10 circle
    Burst2 = 30183, // 3972->self, no cast, single-target
    DeafeningBellow = 31233, // 3970->self, 4.0s cast, range 55 circle
    AshenOuroboros = 30190, // 3970->self, 8.0s cast, range 11-20 donut
    BodySlam = 31234, // 3970->self, 4.0s cast, range 30 circle
    CripplingBlow = 30193, // 3970->player, 5.0s cast, single-target
}

class ProximityPyre(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ProximityPyre), new AOEShapeCircle(12));
class InflammableFumes(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.InflammableFumes));
class Burst(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Burst), 10);
class Burst2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Burst2), 10);
class DeafeningBellow(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DeafeningBellow));
class AshenOuroboros(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AshenOuroboros), new AOEShapeDonut(11, 20));
class BodySlam(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.BodySlam), 20, stopAtWall: true);
class CripplingBlow(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.CripplingBlow));
class D032GyascutusStates : StateMachineBuilder
{
    public D032GyascutusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ProximityPyre>()
            .ActivateOnEnter<InflammableFumes>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<Burst2>()
            .ActivateOnEnter<DeafeningBellow>()
            .ActivateOnEnter<AshenOuroboros>()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<CripplingBlow>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala, xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 39, NameID = 3455)]
public class D032Gyascutus(WorldState ws, Actor primary) : BossModule(ws, primary, new(11.9f, 68f), new ArenaBoundsCircle(19.5f));

