namespace BossMod.Shadowbringers.Foray.Dalriada.D3Cuchulainn;

public enum OID : uint
{
    Boss = 0x31AB,
    Helper = 0x233C,
}

class FourthMakeCuchulainnStates : StateMachineBuilder
{
    public FourthMakeCuchulainnStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 778, NameID = 10004)]
public class FourthMakeCuchulainn(WorldState ws, Actor primary) : BossModule(ws, primary, new(650, -187.4f), new ArenaBoundsCircle(25.5f))
{
    public override bool DrawAllPlayers => true;
}

