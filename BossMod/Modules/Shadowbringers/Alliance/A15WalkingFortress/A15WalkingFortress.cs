namespace BossMod.Shadowbringers.Alliance.A15WalkingFortress;

public enum OID : uint
{
    Boss = 0x2C74,
    Helper = 0x233C,
    //_Gen_Marx = 0x2C0C, // R0.700, x1
    //_Gen_Marx1 = 0x2C0B, // R0.700, x1
    //_Gen_9SOperatedFlightUnit = 0x2C75, // R2.800, x1
    //_Gen_Marx2 = 0x2C78, // R12.000, x0 (spawn during fight)
    //_Gen_GoliathTank = 0x2C77, // R9.600, x0 (spawn during fight)
    //_Gen_ = 0x2C86, // R1.000, x0 (spawn during fight)
    //_Gen_SerialJointedServiceModel = 0x2C76, // R3.360, x0 (spawn during fight)
    //_Gen_1 = 0x2C87, // R1.000, x0 (spawn during fight)
    //_Gen_2 = 0x2C88, // R1.000, x0 (spawn during fight)
    //_Gen_3 = 0x2CC7, // R1.000, x0 (spawn during fight)
    //_Gen_4 = 0x2CC8, // R1.000, x0 (spawn during fight)
}

class A15WalkingFortressStates : StateMachineBuilder
{
    public A15WalkingFortressStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9153)]
public class A15WalkingFortress(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

