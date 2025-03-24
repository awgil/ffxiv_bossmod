namespace BossMod.Shadowbringers.Foray.Dalriada.D5DiabloArmament;

public enum OID : uint
{
    Boss = 0x31B3,
    Helper = 0x233C,
}

class TheDiabloArmamentStates : StateMachineBuilder
{
    public TheDiabloArmamentStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 778, NameID = 10007)]
public class TheDiabloArmament(WorldState ws, Actor primary) : BossModule(ws, primary, new(-720, -760), new ArenaBoundsCircle(29.5f))
{
    public override bool DrawAllPlayers => true;
}

