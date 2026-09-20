namespace BossMod.Global.Crucible.CorpseFlowerPiece;

public enum OID : uint
{
    Boss = 0x4CBD,
    Helper = 0x233C,
}

class CorpseFlowerPieceStates : StateMachineBuilder
{
    public CorpseFlowerPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14603)]
public class CorpseFlowerPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

