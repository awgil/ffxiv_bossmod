namespace BossMod.Shadowbringers.Alliance.A35XunZi;

public enum OID : uint
{
    Boss = 0x3195,
    MengZi = 0x3196,
    Helper = 0x233C,
}

class A35XunZiStates : StateMachineBuilder
{
    public A35XunZiStates(BossModule module) : base(module)
    {
        TrivialPhase().Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && module.Enemies(OID.MengZi).All(m => m.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9921)]
public class A35XunZi(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, 800), new ArenaBoundsSquare(24.5f));

