namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

public enum OID : uint
{
    Boss = 0x47C6,
    Helper = 0x233C,
}

class ZeleniaStates : StateMachineBuilder
{
    public ZeleniaStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1031, NameID = 13861)]
public class Zelenia(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));
