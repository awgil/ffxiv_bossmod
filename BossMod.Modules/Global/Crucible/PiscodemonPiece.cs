namespace BossMod.Global.Crucible.PiscodemonPiece;

public enum OID : uint
{
    Boss = 0x4B8A,
    Helper = 0x233C,
}

class PiscodemonPieceStates : StateMachineBuilder
{
    public PiscodemonPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1088, NameID = 14535)]
public class PiscodemonPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20));

