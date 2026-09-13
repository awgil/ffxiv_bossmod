namespace BossMod.Global.Crucible.OgrePiece;

public enum OID : uint
{
    Boss = 0x4B8D,
    Helper = 0x233C,
}

class OgrePieceStates : StateMachineBuilder
{
    public OgrePieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1088, NameID = 14538)]
public class OgrePiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

