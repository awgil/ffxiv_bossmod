namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN4Index;

[SkipLocalsInit]
sealed class IndexStates : StateMachineBuilder
{
    public IndexStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<OmniElementPanels>()
            .ActivateOnEnter<Flare>()
            .ActivateOnEnter<Aim>()
            .ActivateOnEnter<RomeosBallad>()
            .ActivateOnEnter<ElementaryEvocation>()
            .ActivateOnEnter<ElementaryExpansion>()
            .ActivateOnEnter<ElementaryChemistry>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<Bombs>()
            .ActivateOnEnter<DuologyOfImplements>()
            .ActivateOnEnter<AllConsumingFlames>()
            .ActivateOnEnter<Predict>();
    }
}
