namespace BossMod.Shadowbringers.Alliance.A32HanselAndGretel;

public enum OID : uint
{
    Boss = 0x31A4,
    Hansel = 0x31A5,
    Helper = 0x233C,
}

class A32HanselAndGretelStates : StateMachineBuilder
{
    public A32HanselAndGretelStates(A32HanselAndGretel module) : base(module)
    {
        TrivialPhase()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed && module.Enemies(OID.Hansel).All(e => e.IsDeadOrDestroyed);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9989)]
public class A32HanselAndGretel(WorldState ws, Actor primary) : BossModule(ws, primary, new(-800, -951), new ArenaBoundsCircle(24.5f));
