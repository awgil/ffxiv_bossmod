namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE211LostontheWind;

public enum OID : uint
{
    Abductor = 0x4BE1, // R5.004, x?
}

public enum AID : uint
{

}

public enum SID : uint
{

}

public enum IconID : uint
{

}

[SkipLocalsInit]
sealed class LostontheWindStates : StateMachineBuilder
{
    public LostontheWindStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(LostontheWindStates),
    ConfigType = null, // replace null with typeof(LostontheWindConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Abductor,
    Contributors = "The Combat Reborn Team (LTS)",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14505u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class LostontheWind(WorldState ws, Actor primary) : BossModule(ws, primary, new(-150f, -860f), new ArenaBoundsCircle(23.9f));
