namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN3Necrophobia;

[SkipLocalsInit]
sealed class NecrophobiaStates : StateMachineBuilder
{
    public NecrophobiaStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}
