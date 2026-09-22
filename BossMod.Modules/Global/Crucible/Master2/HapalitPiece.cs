namespace BossMod.Global.Crucible.HapalitPiece;

public enum OID : uint
{
    Boss = 0x4D1C,
    Helper = 0x233C,
}

class HapalitPieceStates : StateMachineBuilder
{
    public HapalitPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1092, NameID = 14691)]
public class HapalitPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

