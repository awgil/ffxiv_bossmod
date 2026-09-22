namespace BossMod.Global.Crucible.GigantisPiece;

public enum OID : uint
{
    Boss = 0x4D03,
    Helper = 0x233C,
}

class GigantisPieceStates : StateMachineBuilder
{
    public GigantisPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1092, NameID = 14670)]
public class GigantisPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20));

