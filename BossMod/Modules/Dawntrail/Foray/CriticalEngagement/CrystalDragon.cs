namespace BossMod.Dawntrail.Foray.CriticalEngagement.CrystalDragon;

public enum OID : uint
{
    Boss = 0x4715,
    Helper = 0x233C,
}

class CrystalDragonStates : StateMachineBuilder
{
    public CrystalDragonStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13696, DevOnly = true)]
public class CrystalDragon(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
