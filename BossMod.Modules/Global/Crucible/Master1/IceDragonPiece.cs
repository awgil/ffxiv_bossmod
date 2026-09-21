namespace BossMod.Global.Crucible.IceDragonPiece;

public enum OID : uint
{
    Boss = 0x4CC0,
    Helper = 0x233C,
}

class IceDragonPieceStates : StateMachineBuilder
{
    public IceDragonPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14606)]
public class IceDragonPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20));

