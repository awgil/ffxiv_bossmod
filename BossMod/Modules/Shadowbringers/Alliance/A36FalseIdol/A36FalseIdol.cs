namespace BossMod.Shadowbringers.Alliance.A36FalseIdol;

public enum OID : uint
{
    Boss = 0x318D,
    HerInflorescence = 0x3190,
    Helper = 0x233C,
}

class A36FalseIdolStates : StateMachineBuilder
{
    public A36FalseIdolStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && module.Enemies(OID.HerInflorescence).All(i => i.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9948)]
public class A36FalseIdol(WorldState ws, Actor primary) : BossModule(ws, primary, new(-700, -700), new ArenaBoundsSquare(24.5f));
