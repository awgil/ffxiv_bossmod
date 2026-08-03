namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN3Necrophobia;

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(NecrophobiaStates),
    ConfigType = null, // replace null with typeof(NecrophobiaConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.Necrophobia,
    Contributors = "The Combat Reborn Team (LTS)",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14503u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Necrophobia(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 800f), new ArenaBoundsCircle(24f));
