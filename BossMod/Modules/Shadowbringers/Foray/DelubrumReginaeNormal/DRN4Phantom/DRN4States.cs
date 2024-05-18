namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN4Phantom;

class DRN4PhantomStates : StateMachineBuilder
{
    public DRN4PhantomStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<UndyingHatred>()
            .ActivateOnEnter<VileWave>()
            .ActivateOnEnter<Miasma>();
    }
}
