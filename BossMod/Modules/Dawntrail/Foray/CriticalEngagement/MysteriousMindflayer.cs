namespace BossMod.Dawntrail.Foray.CriticalEngagement.MysteriousMindflayer;

public enum OID : uint
{
    Boss = 0x46B5,
    Helper = 0x233C,
}

class MysteriousMindflayerStates : StateMachineBuilder
{
    public MysteriousMindflayerStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13646, DevOnly = true)]
public class MysteriousMindflayer(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
