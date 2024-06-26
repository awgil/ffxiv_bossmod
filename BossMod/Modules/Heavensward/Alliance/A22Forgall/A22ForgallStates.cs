namespace BossMod.Heavensward.Alliance.A22Forgall;

class A22ForgallStates : StateMachineBuilder
{
    public A22ForgallStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<BrandOfTheFallen>()
            .ActivateOnEnter<MegiddoFlame2>()
            .ActivateOnEnter<DarkEruption2>()
            .ActivateOnEnter<MortalRay>()
            .ActivateOnEnter<Mow>()
            .ActivateOnEnter<TailDrive>();
    }
}
