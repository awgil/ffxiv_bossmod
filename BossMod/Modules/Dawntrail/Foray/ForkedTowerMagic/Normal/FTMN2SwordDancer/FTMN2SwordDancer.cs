namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN2SwordDancer;

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(SwordDancerStates),
    ConfigType = null, // replace null with typeof(SwordDancerConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.SwordDancer,
    Contributors = "The Combat Reborn Team (LTS)",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14820u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class SwordDancer(WorldState ws, Actor primary) : BossModule(ws, primary, new(600f, 704f), new ArenaBoundsCircle(25f));
