namespace BossMod.Global.Crucible.VoidmancerPiece;

public enum OID : uint
{
    Boss = 0x4C5C,
    Helper = 0x233C,
}

class VoidmancerPieceStates : StateMachineBuilder
{
    public VoidmancerPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1089, NameID = 14552)]
public class VoidmancerPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20));

