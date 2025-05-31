namespace BossMod.Dawntrail.Foray.CriticalEngagement.RepairedLion;

public enum OID : uint
{
    Boss = 0x46CC,
    Helper = 0x233C,
}

class RepairedLionStates : StateMachineBuilder
{
    public RepairedLionStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13649, DevOnly = true)]
public class RepairedLion(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    public override bool DrawAllPlayers => true;
}

