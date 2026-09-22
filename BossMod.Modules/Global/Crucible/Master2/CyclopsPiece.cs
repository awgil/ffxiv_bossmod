namespace BossMod.Global.Crucible.CyclopsPiece;

public enum OID : uint
{
    Boss = 0x4D04,
    Helper = 0x233C,
}

class CyclopsPieceStates : StateMachineBuilder
{
    public CyclopsPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1092, NameID = 14671)]
public class CyclopsPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

