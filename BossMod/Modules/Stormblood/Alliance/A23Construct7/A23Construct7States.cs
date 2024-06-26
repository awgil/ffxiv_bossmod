namespace BossMod.Stormblood.Alliance.A23Construct7;

class A23Construct7States : StateMachineBuilder
{
    public A23Construct7States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Destroy1>()
            .ActivateOnEnter<Destroy2>()
            .ActivateOnEnter<Accelerate>()
            .ActivateOnEnter<Compress1>()
            .ActivateOnEnter<Compress2>()
            .ActivateOnEnter<Pulverize2>()
            .ActivateOnEnter<Dispose1>()
            .ActivateOnEnter<Dispose3>();
    }
}
