namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN4Index;

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(IndexStates),
    ConfigType = null, // replace null with typeof(IndexConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.Index,
    Contributors = "The Combat Reborn Team (LTS)",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14503u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Index(WorldState ws, Actor primary) : BossModule(ws, primary, new(0f, -628f), new ArenaBoundsSquare(28f));
//temp arena for development, will need to be updated with actual arena bounds when known
