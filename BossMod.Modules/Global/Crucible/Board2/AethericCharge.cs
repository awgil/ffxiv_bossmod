namespace BossMod.Global.Crucible.AethericCharge;

public enum OID : uint
{
    Boss = 0x4C64,
    Helper = 0x233C,
}

class AethericChargeStates : StateMachineBuilder
{
    public AethericChargeStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1089, NameID = 14560)]
public class AethericCharge(WorldState ws, Actor primary) : BossModule(ws, primary, new(120, -420), new ArenaBoundsCircle(20))
{
    protected override bool CheckPull() => PrimaryActor.InCombat;
}

