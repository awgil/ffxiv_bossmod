namespace BossMod.Shadowbringers.Foray.Dalriada.D4SaunionDawon;

public enum OID : uint
{
    Boss = 0x31DD,
    Helper = 0x233C,
}

class SaunionStates : StateMachineBuilder
{
    public SaunionStates(BossModule module) : base(module)
    {
        TrivialPhase().Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && module.Enemies(0x31DE).All(x => x.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 778, NameID = 10192)]
public class Saunion(WorldState ws, Actor primary) : BossModule(ws, primary, new(650, -659), new ArenaBoundsRect(26.5f, 26.5f))
{
    public override bool DrawAllPlayers => true;
}

