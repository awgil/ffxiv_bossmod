namespace BossMod.Global.Crucible.SphinxPiece;

public enum OID : uint
{
    Boss = 0x4CFE,
    Helper = 0x233C,
}

class SphinxPieceStates : StateMachineBuilder
{
    public SphinxPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1092, NameID = 14665)]
public class SphinxPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20));

