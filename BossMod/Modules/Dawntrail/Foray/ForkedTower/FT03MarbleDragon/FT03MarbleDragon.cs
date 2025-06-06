namespace BossMod.Dawntrail.Foray.ForkedTower.FT03MarbleDragon;

class FT03MarbleDragonStates : StateMachineBuilder
{
    public FT03MarbleDragonStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13838)]
public class FT03MarbleDragon(WorldState ws, Actor primary) : BossModule(ws, primary, new(-337, 157), new ArenaBoundsCircle(35));

