namespace BossMod.Global.Crucible.BanemitePiece;

public enum OID : uint
{
    Boss = 0x4B8B,
    Helper = 0x233C,
}

class BanemitePieceStates : StateMachineBuilder
{
    public BanemitePieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1088, NameID = 14536)]
public class BanemitePiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

