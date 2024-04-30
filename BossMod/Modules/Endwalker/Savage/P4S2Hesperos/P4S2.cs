namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to demigod double mechanic (shared tankbuster)
class DemigodDouble(BossModule module) : Components.CastSharedTankbuster(module, ActionID.MakeSpell(AID.DemigodDouble), 6);

// state related to heart stake mechanic (dual hit tankbuster with bleed)
// TODO: consider showing some tank swap / invul hint...
class HeartStake(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.HeartStakeSecond));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 801, NameID = 10744, SortOrder = 2)]
public class P4S2(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    // common wreath of thorns constants
    public const float WreathAOERadius = 20;
    public const float WreathTowerRadius = 4;
}
