namespace BossMod.Global.Crucible.ElderTablitaurPiece;

public enum OID : uint
{
    Boss = 0x4C5F,
    Helper = 0x233C,
}

class ElderTablitaurPieceStates : StateMachineBuilder
{
    public ElderTablitaurPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1089, NameID = 14555)]
public class ElderTablitaurPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20));

