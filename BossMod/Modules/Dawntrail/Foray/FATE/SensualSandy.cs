namespace BossMod.Dawntrail.Foray.FATE.SensualSandy;

public enum OID : uint
{
    Boss = 0x4D56,
    Helper = 0x233C,
    PoisonCloud = 0x4D57, // R1.700, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50535, // Boss->player, no cast, single-target
    PutridBreath = 48944, // Boss->self, 5.0s cast, range 25 130-degree cone
    PutridBreath1 = 48952, // Boss->self, 3.0s cast, range 25 130-degree cone
    WildWildBreath = 48945, // Boss->self, 5.0s cast, range 30 width 6 cross
    WildWildWildWildWildBreath = 48946, // Boss->self, 5.0s cast, range 30 width 6 cross
    ExtensibleTendrils = 48947, // Boss->self, 3.0s cast, range 30 width 6 cross
    PoisonPassel = 48951, // Boss->self, 3.0s cast, single-target
    Burst = 48950, // 4D57->self, 5.0s cast, range 10 circle
}

public enum SID : uint
{
    Poison = 2104, // 4D57->player, extra=0x0
}

class PutridBreath(BossModule module) : Components.GroupedAOEs(module, [AID.PutridBreath, AID.PutridBreath1], new AOEShapeCone(25.0f, 65.0f.Degrees()));
class WildWildBreath(BossModule module) : Components.GroupedAOEs(module, [AID.WildWildBreath, AID.WildWildWildWildWildBreath, AID.ExtensibleTendrils],
    new AOEShapeCross(30.0f, 3.0f));
class Burst(BossModule module) : Components.StandardAOEs(module, AID.Burst, 10.0f);

class SensualSandyStates : StateMachineBuilder
{
    public SensualSandyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PutridBreath>()
            .ActivateOnEnter<WildWildBreath>()
            .ActivateOnEnter<Burst>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14738)]
public class SensualSandy(WorldState ws, Actor primary) : BossModule(ws, primary, new(-402.000f, -253.000f), new ArenaBoundsCircle(30));
