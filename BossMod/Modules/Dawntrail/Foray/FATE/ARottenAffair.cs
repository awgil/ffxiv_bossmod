namespace BossMod.Dawntrail.Foray.FATE.ARottenAffair;

public enum OID : uint
{
    PatientKuribu = 0x4D61,
    Helper = 0x233C,
    PatientKuribuHelper = 0x4DCC, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50537, // PatientKuribu->player, no cast, single-target
    Glory = 49915, // PatientKuribu->self, 5.0s cast, range 50 90.000-degree cone

    EnsorcelStoneIII = 49906, // PatientKuribu->self, 5.0s cast, single-target
    StoneIII = 49907, // 4DCC->self, 4.0s cast, range 40 45.000-degree cone
    StoneIII1 = 50071, // 4DCC->self, 4.5s cast, range 40 45-degree cone

    EnsorcelAeroIII = 49905, // PatientKuribu->self, 5.0s cast, single-target
    AeroIII = 49908, // 4DCC->location, 4.0s cast, range 40 width 8 rect - This is a real cast as well
    AeroIII1 = 50072, // PatientKuribu1->location, 4.5s cast, range 40 width 8 rect - This is a real cast

    HolyCast = 49911, // PatientKuribu->self, 3.0s cast, single-target
    HolyStart = 49912, // 4DCC->self, 5.0s cast, range 6 circle
    HolyNext = 49913, // 4DCC->location, 3.0s cast, range 6 circle

    ShortswordAndSorcery = 50118, // PatientKuribu->self, 5.0s cast, range 15 circle
    ShortswordAndSorcery1 = 50119, // PatientKuribu->self, 5.0s cast, range 15 circle
    LongswordAndSorcery = 50121, // PatientKuribu->self, 5.0s cast, range 10-25 donut
    LongswordAndSorcery1 = 50120, // PatientKuribu->self, 5.0s cast, range 10-25 donut
}

public enum SID : uint
{
    EnsorcelledStoneIII = 5375, // PatientKuribu->PatientKuribu, extra=0x0
    EnsorcelledAeroIII = 5374, // PatientKuribu->PatientKuribu, extra=0x0
}

sealed class Glory(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Glory, new AOEShapeCone(50.0f, 45.0f.Degrees()));
sealed class StoneIII(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.StoneIII, (uint)AID.StoneIII1],
    new AOEShapeCone(40.0f, 22.5f.Degrees()));
sealed class AeroIII(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.AeroIII, (uint)AID.AeroIII1], new AOEShapeRect(40.0f, 4.0f));
sealed class Holy(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HolyStart, (uint)AID.HolyNext], new AOEShapeCircle(6.0f));
sealed class ShortswordAndSorcery(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ShortswordAndSorcery, (uint)AID.ShortswordAndSorcery1], new AOEShapeCircle(15.0f));
sealed class LongswordAndSorcery(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LongswordAndSorcery, (uint)AID.LongswordAndSorcery1],
    new AOEShapeDonut(10.0f, 25.0f));

[SkipLocalsInit]
sealed class ARottenAffairStates : StateMachineBuilder
{
    public ARottenAffairStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Glory>()
            .ActivateOnEnter<StoneIII>()
            .ActivateOnEnter<AeroIII>()
            .ActivateOnEnter<Holy>()
            .ActivateOnEnter<ShortswordAndSorcery>()
            .ActivateOnEnter<LongswordAndSorcery>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(ARottenAffairStates),
    ConfigType = null, // replace null with typeof(PatientKuribuConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.PatientKuribu,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2081u,
    SortOrder = 10,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class ARottenAffair(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
