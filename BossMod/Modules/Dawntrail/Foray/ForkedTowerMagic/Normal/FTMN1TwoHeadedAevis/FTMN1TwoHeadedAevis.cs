namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(TwoHeadedAevisStates),
    ConfigType = null, // replace null with typeof(TwoHeadedAevisConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.GreenHead1,
    Contributors = "The Combat Reborn Team (LTS)",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14489u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class TwoHeadedAevis(WorldState ws, Actor primary) : BossModule(ws, primary, new(-900f, 700f), new ArenaBoundsSquare(20f));

