namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN3Necrophobia;

[SkipLocalsInit]
sealed class NecrophobiaStates : StateMachineBuilder
{
    public NecrophobiaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<HailOfHellflares>()
            .ActivateOnEnter<AncientFire>()
            .ActivateOnEnter<AncientBlizzard>()
            .ActivateOnEnter<CorpseMangler>()
            .ActivateOnEnter<AncientThunder>()
            //.ActivateOnEnter<DarkCurrent1>()
            //.ActivateOnEnter<DarkCurrent2>()
            .ActivateOnEnter<DarkCurrent>()
            .ActivateOnEnter<DeathlyRay>()
            .ActivateOnEnter<VacuumWave>();
    }
}
