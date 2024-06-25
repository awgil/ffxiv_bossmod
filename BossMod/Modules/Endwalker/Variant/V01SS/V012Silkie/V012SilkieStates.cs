namespace BossMod.Endwalker.Variant.V01SS.V012Silkie;

class V012SilkieStates : StateMachineBuilder
{
    public V012SilkieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<DustBlusterKnockback>()
            .ActivateOnEnter<WashOutKnockback>()
            .ActivateOnEnter<BracingDuster1>()
            .ActivateOnEnter<BracingDuster2>()
            .ActivateOnEnter<ChillingDuster1>()
            .ActivateOnEnter<ChillingDuster2>()
            .ActivateOnEnter<ChillingDuster3>()
            .ActivateOnEnter<SlipperySoap>()
            .ActivateOnEnter<SpotRemover2>()
            .ActivateOnEnter<SqueakyCleanAOE1E>()
            .ActivateOnEnter<SqueakyCleanAOE2E>()
            .ActivateOnEnter<SqueakyCleanAOE3E>()
            .ActivateOnEnter<SqueakyCleanAOE1W>()
            .ActivateOnEnter<SqueakyCleanAOE2W>()
            .ActivateOnEnter<SqueakyCleanAOE3W>()
            .ActivateOnEnter<V012PuffTracker>()
            .ActivateOnEnter<PuffAndTumble1>()
            .ActivateOnEnter<PuffAndTumble2>()
            .ActivateOnEnter<CarpetBeater>()
            .ActivateOnEnter<EasternEwers>()
            .ActivateOnEnter<TotalWash>();
    }
}
