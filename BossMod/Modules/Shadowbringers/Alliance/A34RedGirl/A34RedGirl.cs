namespace BossMod.Shadowbringers.Alliance.A34RedGirl;

public enum OID : uint
{
    Boss = 0x32BB,
    BossP2 = 0x32BD,
    RedSphere = 0x32EA,
    Helper = 0x233C,
}

class A34RedGirlStates : StateMachineBuilder
{
    public A34RedGirlStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed
                && module.Enemies(OID.RedSphere).All(e => e.IsDeadOrDestroyed) // hacking minigame
                && module.Enemies(OID.BossP2).All(e => e.IsDeadOrDestroyed); // p2
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9950)]
public class A34RedGirl(WorldState ws, Actor primary) : BossModule(ws, primary, new(850, -856), new ArenaBoundsSquare(24.5f));
