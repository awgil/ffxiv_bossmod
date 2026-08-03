namespace BossMod.Dawntrail.Foray.FATE.NH102SensualSandy;

public enum OID : uint
{
    SensualSandy = 0x4D56,
    Helper = 0x233C,
    PoisonCloud = 0x4D57, // R1.700, x0 (spawn during fight)
    LilithLavatera = 0x0, // R0.500, x0 (spawn during fight), None type
}

public enum AID : uint
{
    AutoAttack = 50535, // SensualSandy->player, no cast, single-target
    PutridBreath = 48944, // SensualSandy->self, 5.0s cast, range 25 130.000-degree cone
    PutridBreath1 = 48952, // SensualSandy->self, 3.0s cast, range 25 130.000-degree cone
    WildWildBreath = 48945, // SensualSandy->self, 5.0s cast, range 30 width 6 cross
    WildWildWildWildWildBreath = 48946, // SensualSandy->self, 5.0s cast, range 30 width 6 cross
    ExtensibleTendrils = 48947, // SensualSandy->self, 3.0s cast, range 30 width 6 cross
    PoisonPassel = 48951, // SensualSandy->self, 3.0s cast, single-target
    Burst = 48950, // 4D57->self, 5.0s cast, range 10 circle
}

sealed class PutridBreath(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.PutridBreath, (uint)AID.PutridBreath1],
    new AOEShapeCone(25.0f, 65.0f.Degrees()));
sealed class WildWildBreath(BossModule module) : Components.SimpleAOEGroups(module,
    [(uint)AID.WildWildBreath, (uint)AID.WildWildWildWildWildBreath, (uint)AID.ExtensibleTendrils], new AOEShapeCross(30.0f, 3.0f));
sealed class Burst(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Burst, 10f);

[SkipLocalsInit]
sealed class SensualSandyStates : StateMachineBuilder
{
    public SensualSandyStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PutridBreath>()
            .ActivateOnEnter<WildWildBreath>()
            .ActivateOnEnter<Burst>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified,
    StatesType = typeof(SensualSandyStates),
    ConfigType = null, // replace null with typeof(SensualSandyConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = null, // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.SensualSandy,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14738u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class SensualSandy(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
