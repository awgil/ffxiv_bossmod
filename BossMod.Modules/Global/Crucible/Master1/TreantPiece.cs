namespace BossMod.Global.Crucible.TreantPiece;

public enum OID : uint
{
    Boss = 0x4CCD,
    Helper = 0x233C,
}

class TreantPieceStates : StateMachineBuilder
{
    public TreantPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14618)]
public class TreantPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

