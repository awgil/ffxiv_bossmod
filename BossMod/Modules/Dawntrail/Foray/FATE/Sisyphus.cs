namespace BossMod.Modules.Dawntrail.Foray.FATE.Sisyphus;

public enum OID : uint
{
    Boss = 0x4735,
    Helper = 0x233C,
}

class SisyphusStates : StateMachineBuilder
{
    public SisyphusStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

// TODO: this is a fate, not a CE
//[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13703)]
//public class Sisyphus(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
