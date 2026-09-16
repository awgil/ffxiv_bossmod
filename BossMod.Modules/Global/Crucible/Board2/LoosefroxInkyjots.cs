namespace BossMod.Global.Crucible.LoosefroxInkyjots;

public enum OID : uint
{
    Boss = 0x4C65,
    Helper = 0x233C,
}

class LoosefroxInkyjotsStates : StateMachineBuilder
{
    public LoosefroxInkyjotsStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1089, NameID = 14561, BitmapType = BossModuleInfo.BitmapType.Enabled)]
public class LoosefroxInkyjots(WorldState ws, Actor primary) : BossModule(ws, primary, new(520, -420), new ArenaBoundsSquare(27));

