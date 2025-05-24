namespace BossMod.Modules.Shadowbringers.Alliance.A33FlightUnits;

public enum OID : uint
{
    Boss = 0x3193,
    Helper = 0x233C,
}

class A33FlightUnitsStates : StateMachineBuilder
{
    public A33FlightUnitsStates(BossModule module) : base(module)
    {
        TrivialPhase().Raw.Update = () => module.Enemies(OID.Boss).All(f => f.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9918)]
public class A33FlightUnits(WorldState ws, Actor primary) : BossModule(ws, primary, new(755, -749.4f), new ArenaBoundsCircle(24.5f));
