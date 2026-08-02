namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN4Index;

[SkipLocalsInit]
sealed class IndexStates : StateMachineBuilder
{
    public IndexStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}
