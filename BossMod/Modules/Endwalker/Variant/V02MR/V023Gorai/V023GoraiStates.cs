namespace BossMod.Endwalker.Variant.V02MR.V023Gorai;

class V023GoraiStates : StateMachineBuilder
{
    public V023GoraiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            //Route 5
            .ActivateOnEnter<PureShock>()
            //Route 6
            .ActivateOnEnter<ShockAOE>()
            .ActivateOnEnter<HumbleHammerAOE>()
            //.ActivateOnEnter<Thundercall>()
            //Route 7
            .ActivateOnEnter<WorldlyPursuit>()
            .ActivateOnEnter<FightingSpiritsRaidwide>()
            .ActivateOnEnter<BiwaBreakerFirst>()
            .ActivateOnEnter<BiwaBreakerRest>()
            //Standard
            .ActivateOnEnter<RatTower>()
            .ActivateOnEnter<DramaticBurst>()
            .ActivateOnEnter<ImpurePurgation>()
            .ActivateOnEnter<StringSnap>()
            .ActivateOnEnter<SpikeOfFlameAOE>()
            .ActivateOnEnter<FlameAndSulphur>()
            .ActivateOnEnter<TorchingTorment>()
            .ActivateOnEnter<UnenlightenmentAOE>();
    }
}
