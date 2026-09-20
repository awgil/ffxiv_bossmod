namespace BossMod.Global.Crucible.StrixPiece;

public enum OID : uint
{
    Boss = 0x4CB6,
    Helper = 0x233C,
}

class StrixPieceStates : StateMachineBuilder
{
    public StrixPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14596)]
public class StrixPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

