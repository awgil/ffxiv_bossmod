namespace BossMod.Global.Crucible.ArchDemonPiece;

public enum OID : uint
{
    Boss = 0x4B88,
    Helper = 0x233C,
}

class ArchDemonPieceStates : StateMachineBuilder
{
    public ArchDemonPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1088, NameID = 14533)]
public class ArchDemonPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(520, 0), new ArenaBoundsRect(20, 15));

