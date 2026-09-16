namespace BossMod.Global.Crucible.TaurusPiece;

public enum OID : uint
{
    Boss = 0x4C54,
    Helper = 0x233C,
}

class TaurusPieceStates : StateMachineBuilder
{
    public TaurusPieceStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

// platforms width 5 ("radius" 2.5)
// 520, -7.5
// 520, 7.5
// 530, 0
// 510, 0
[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1089, NameID = 14546)]
public class TaurusPiece(WorldState ws, Actor primary) : BossModule(ws, primary, new(520, 0), new ArenaBoundsRect(20, 15));

