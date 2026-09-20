namespace BossMod.Global.Crucible.GargoylePiece;

public enum OID : uint
{
    Boss = 0x4CC2,
    Helper = 0x233C,
}

class GargoylePieceStates : StateMachineBuilder
{
    public GargoylePieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1091, NameID = 14608)]
public class GargoylePiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, 0), new ArenaBoundsSquare(20));

