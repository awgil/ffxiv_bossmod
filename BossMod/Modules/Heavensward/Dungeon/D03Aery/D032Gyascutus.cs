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

class ProximityPyre(BossModule module) : Components.StandardAOEs(module, AID.ProximityPyre, new AOEShapeCircle(12));
class Burst(BossModule module) : Components.StandardAOEs(module, AID.Burst, 10);
class DeafeningBellow(BossModule module) : Components.RaidwideCast(module, AID.DeafeningBellow);
class AshenOuroboros(BossModule module) : Components.StandardAOEs(module, AID.AshenOuroboros, new AOEShapeDonut(11, 20));
class BodySlam(BossModule module) : Components.KnockbackFromCastTarget(module, AID.BodySlam, 10, stopAtWall: false);
class CripplingBlow(BossModule module) : Components.SingleTargetCast(module, AID.CripplingBlow);
class D032GyascutusStates : StateMachineBuilder
{
    public D032GyascutusStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ProximityPyre>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<DeafeningBellow>()
            .ActivateOnEnter<AshenOuroboros>()
            .ActivateOnEnter<BodySlam>()
            .ActivateOnEnter<CripplingBlow>();

    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "VeraNala, xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 39, NameID = 3455)]
public class D032Gyascutus(WorldState ws, Actor primary) : BossModule(ws, primary, new(11.9f, 68f), new ArenaBoundsCircle(19.5f));
