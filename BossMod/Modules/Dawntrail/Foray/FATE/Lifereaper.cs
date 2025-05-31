namespace BossMod.Modules.Dawntrail.Foray.FATE.Lifereaper;

public enum OID : uint
{
    Boss = 0x4772,
    Helper = 0x233C,
}

class LifereaperStates : StateMachineBuilder
{
    public LifereaperStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

// TODO: this is a fate
//[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13741)]
//public class Lifereaper(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
