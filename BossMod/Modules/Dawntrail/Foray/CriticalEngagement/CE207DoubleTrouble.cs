namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE207DoubleTrouble;

public enum OID : uint
{
    ConjuredCalofisteri = 0x4BB8,
    Helper = 0x233C,
    LitheLock = 0x4BBA, // R1.000, x0 (spawn during fight)
    Entanglement = 0x4BB9, // R4.440, x0 (spawn during fight)
    BlueIcon = 0x4BBB, // R1.000, x0 (spawn during fight)
    RedIcon = 0x4BBC, // R1.000, x0 (spawn during fight)
}

public enum SID : uint
{
    Fetters = 5349, // Entanglement->player, extra=0xEC4
}

public enum AID : uint
{
    AutoAttack = 50122, // ConjuredCalofisteri->player, no cast, single-target
    AuraBurst = 47079, // ConjuredCalofisteri->self, 5.0s cast, single-target
    AuraBurstVisual = 47080, // Helper->self, no cast, ???
    AsymmetricCoifChangeRightLeft = 47054, // ConjuredCalofisteri->self, 3.0s cast, single-target - right to left
    AsymmetricCoifChangeLeftRight = 47055, // ConjuredCalofisteri->self, 3.0s cast, single-target - left to right
    DualCutCast = 47058, // ConjuredCalofisteri->self, 2.0s cast, single-target
    DualCutCast1 = 47059, // ConjuredCalofisteri->self, 2.0s cast, single-target
    DualCutVisual = 47061, // ConjuredCalofisteri->self, no cast, single-target
    DualCutVisual2 = 47060, // ConjuredCalofisteri->self, no cast, single-target
    DualCut = 50691, // Helper->self, 2.8s cast, range 60 180-degree cone
    DualCut1 = 50692, // Helper->self, 4.8s cast, range 60 180-degree cone
    DashingCutLongTeleport = 47067, // ConjuredCalofisteri->location, 6.0s cast, single-target
    DashingCutTeleport = 47068, // ConjuredCalofisteri->location, 0.5s cast, single-target
    DashingCut = 49052, // Helper->location, 6.5s cast, width 10 rect charge
    DashingCut1 = 49053, // Helper->location, 1.0s cast, width 10 rect charge

    Extension = 47069, // ConjuredCalofisteri->self, 3.0s cast, single-target

    HairShearsCast = 47075, // ConjuredCalofisteri->self, 5.0s cast, single-target
    HairShearsVisual = 47599, // Helper->self, no cast, range 60 width 4 cross
    HairShearsCross = 47077, // Helper->self, 5.0s cast, range 60 width 4 cross
    HairShearsCircle = 47076, // Helper->self, 5.0s cast, range 10 circle

    Graft = 47070, // 4BBA->self, 3.0s cast, range 6 circle
    BalefulBlowout = 47071, // ConjuredCalofisteri->self, 5.0s cast, single-target
    MaliciousWeave = 47072, // 4BB9->self, 5.5s cast, range 6 circle
    MaliciousWeave1 = 47078, // 4BB9->self, 1.0s cast, range 6 circle
    GarroteConsume = 47073, // 4BB9->self, 10.0s cast, range 6 circle
    Garrote = 47074, // 4BB9->self, no cast, single-target

    CoifChange = 47057, // ConjuredCalofisteri->self, no cast, single-target
    CoifChange1 = 47056, // ConjuredCalofisteri->self, no cast, single-target
    ResettingSpray = 47062, // ConjuredCalofisteri->self, no cast, single-target
    ResettingSpray1 = 47065, // ConjuredCalofisteri->self, no cast, single-target
    ResettingSpray2 = 47063, // ConjuredCalofisteri->self, no cast, single-target
    ResettingSpray3 = 47064, // ConjuredCalofisteri->self, no cast, single-target
    RedIconTeleport = 47066, // 4BBC->location, no cast, single-target
}

sealed class AuraBurst(BossModule module) : Components.RaidwideCast(module, (uint)AID.AuraBurst);
sealed class Graft(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Graft, (uint)AID.MaliciousWeave, (uint)AID.MaliciousWeave1],
    new AOEShapeCircle(6.0f));
sealed class DashingCut(BossModule module) : Components.SimpleChargeAOEGroups(module, [(uint)AID.DashingCut, (uint)AID.DashingCut1], 5.0f);
sealed class HairShearsCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HairShearsCross, new AOEShapeCross(60.0f, 2.0f));
sealed class HairShearsCircle(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HairShearsCircle, new AOEShapeCircle(10.0f));

sealed class DualCut : Components.SimpleAOEGroups
{
    public DualCut(BossModule module) : base(module, [(uint)AID.DualCut, (uint)AID.DualCut1], new AOEShapeCone(60.0f, 90.0f.Degrees()),
        expectedNumCasters: 2)
    {
        MaxDangerColor = 1;
        MaxRisky = 1;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Casters.Count == 0)
        {
            return;
        }

        var nextAOE = Casters[0];
        var distance = nextAOE.Shape.Distance(nextAOE.Origin, nextAOE.Rotation);
        hints.GoalZones.Add(p => distance.Distance(p) is > 0.0f and <= 1.0f ? 100.0f : 0.0f);
    }
}

[SkipLocalsInit]
sealed class CE207DoubleTroubleStates : StateMachineBuilder
{
    public CE207DoubleTroubleStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AuraBurst>()
            .ActivateOnEnter<DualCut>()
            .ActivateOnEnter<Graft>()
            .ActivateOnEnter<DashingCut>()
            .ActivateOnEnter<HairShearsCross>()
            .ActivateOnEnter<HairShearsCircle>();
    }
}

//TODO: Add AI Hint to move closer to the middle of the cleaves to make dodging easier- can be marked as verified after implemented
[ModuleInfo(BossModuleInfo.Maturity.Verified,
    StatesType = typeof(CE207DoubleTroubleStates),
    ConfigType = null, // replace null with typeof(ConjuredCalofisteriConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.ConjuredCalofisteri,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CriticalEngagement,
    GroupID = 1093u,
    NameID = 50u,
    SortOrder = 2,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CE207DoubleTrouble(WorldState ws, Actor primary) : BossModule(ws, primary, new(-215.200f, -65.000f), new ArenaBoundsCircle(22f))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Entanglement));
    }

    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 22f);
}
