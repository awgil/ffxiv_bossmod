namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to demigod double mechanic (shared tankbuster)
class DemigodDouble : Components.CastSharedTankbuster
{
    public DemigodDouble() : base(ActionID.MakeSpell(AID.DemigodDouble), 6) { }
}

// state related to heart stake mechanic (dual hit tankbuster with bleed)
// TODO: consider showing some tank swap / invul hint...
class HeartStake : Components.CastCounter
{
    public HeartStake() : base(ActionID.MakeSpell(AID.HeartStakeSecond)) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 801, NameID = 10744, SortOrder = 2)]
public class P4S2 : BossModule
{
    // common wreath of thorns constants
    public static readonly float WreathAOERadius = 20;
    public static readonly float WreathTowerRadius = 4;

    public P4S2(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsCircle(new(100, 100), 20)) { }
}
