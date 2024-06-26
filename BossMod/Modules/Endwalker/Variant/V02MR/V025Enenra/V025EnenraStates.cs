namespace BossMod.Endwalker.Variant.V02MR.V025Enenra;

class V025EnenraStates : StateMachineBuilder
{
    public V025EnenraStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PipeCleaner>()
            .ActivateOnEnter<Uplift>()
            .ActivateOnEnter<Snuff>()
            .ActivateOnEnter<Smoldering>()
            .ActivateOnEnter<IntoTheFireAOE>()
            .ActivateOnEnter<FlagrantCombustion>()
            .ActivateOnEnter<SmokeRingsAOE>()
            .ActivateOnEnter<ClearingSmokeKB>()
            .ActivateOnEnter<KiseruClamor>()
            .ActivateOnEnter<StringRock>();
    }
}
