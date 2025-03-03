namespace BossMod.Stormblood.Foray.BaldesionArsenal.Art;

public enum OID : uint
{
    Boss = 0x265A,
    Helper = 0x233C,
}

class ArtStates : StateMachineBuilder
{
    public ArtStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 639, NameID = 7968)]
public class Art(WorldState ws, Actor primary) : BossModule(ws, primary, new(-129, 748), new ArenaBoundsCircle(29));

