namespace BossMod.Dawntrail.Foray.CriticalEngagement.BlackChocobo;

public enum OID : uint
{
    Boss = 0x46A7,
    Helper = 0x233C,
}

class BlackChocoboStates : StateMachineBuilder
{
    public BlackChocoboStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

// TODO: fix state machine
//[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13637)]
//public class BlackChocobo(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
