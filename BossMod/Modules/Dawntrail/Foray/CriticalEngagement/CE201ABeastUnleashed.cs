namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE201ABeastUnleashed;

public enum OID : uint
{
    AtlasCarbuncleHelper = 0x233C, // R0.500, x20, Helper type
    Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    AtlasCarbuncle1 = 0x4D88, // R1.000, x1
    TopazStone = 0x4C50, // R1.000, x12
    AtlasCarbuncle = 0x4C4F, // R9.067, x1
    Actor1ec031 = 0x1EC031, // R0.500, x1, EventObj type
    Actor1ec045 = 0x1EC045, // R0.500, x1, EventObj type
    Actor1ec046 = 0x1EC046, // R0.500, x2, EventObj type
}

public enum AID : uint
{
    UnknownAbility = 49104, // AtlasCarbuncle1->self, no cast, ???
    ClawToTail = 48296, // AtlasCarbuncle->self, no cast, range 40 ?-degree cone
    SonicHowl = 48298, // AtlasCarbuncle->self, 5.0s cast, ???
    SonicHowl1 = 49505, // AtlasCarbuncleHelper->self, no cast, ???
    AutoAttack = 50852, // AtlasCarbuncle->player, no cast, single-target
    TopazStones = 48280, // AtlasCarbuncle->self, 3.0s cast, single-target
    UnknownWeaponskill1 = 48299, // AtlasCarbuncle->location, no cast, single-target
    UnknownWeaponskill2 = 48289, // AtlasCarbuncleHelper->self, 2.5s cast, range 40 width 60 rect
    UnknownWeaponskill3 = 48288, // AtlasCarbuncleHelper->self, 2.5s cast, range 60 circle
    SpinebreakingStampede = 48291, // AtlasCarbuncle->location, 8.0s cast, ???
    SpinebreakingStampede1 = 49507, // AtlasCarbuncleHelper->self, no cast, ???
    SpinebreakingStampede2 = 48292, // AtlasCarbuncle->location, no cast, ???
    SpinebreakingStampede3 = 49506, // AtlasCarbuncleHelper->self, no cast, ???
    TopazRay1 = 48281, // TopazStone->self, 3.0s cast, range 4 circle
    TopazRay2 = 48282, // TopazStone->self, 3.0s cast, range 4 circle
    TailToClaw = 48295, // AtlasCarbuncle->self, 6.0s cast, range 40 180.000-degree cone
    TailToClaw1 = 48297, // AtlasCarbuncle->self, no cast, range 45 ?-degree cone
    UnknownAbility1 = 50461, // AtlasCarbuncle->self, no cast, single-target
    WeaponskillRubyGlow = 48284, // AtlasCarbuncle->self, 3.0s cast, ???
    AbilityRubyGlow = 50637, // AtlasCarbuncleHelper->self, no cast, ???
    ReflectiveCoat = 50418, // AtlasCarbuncle->self, 3.0s cast, single-target
    RubyReflection = 48287, // AtlasCarbuncleHelper->self, no cast, range 40 width 40 rect
    RubyReflection1 = 48286, // AtlasCarbuncleHelper->self, no cast, range 40 width 40 rect
}

public enum SID : uint
{
    VulnerabilityUp = 2347, // AtlasCarbuncle->player, extra=0x4/0x1
    DirectionalDisregard = 3808, // none->AtlasCarbuncle, extra=0x0
}

[SkipLocalsInit]
sealed class ABeastUnleashedStates : StateMachineBuilder
{
    public ABeastUnleashedStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(ABeastUnleashedStates),
    ConfigType = null, // replace null with typeof(ABeastUnleashedConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.AtlasCarbuncle,
    Contributors = "The Combat Reborn Team (LTS)",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14791u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class ABeastUnleashed(WorldState ws, Actor primary) : BossModule(ws, primary, new(238f, 352f), new ArenaBoundsSquare(20f));
