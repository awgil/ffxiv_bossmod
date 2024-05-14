namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN3QueensGuard;
class DRN3QueensGuardStates : StateMachineBuilder
{
    public DRN3QueensGuardStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OptimalPlaySword>()
            .ActivateOnEnter<OptimalPlayShield>()
            //.ActivateOnEnter<OptimalPlayCone>()
            .ActivateOnEnter<CoatOfArms>()
            .ActivateOnEnter<AboveBoard>()
            .ActivateOnEnter<PawnOff>();
    }
}
