namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN1TrinitySeeker;

class DRN1TrinitySeekerStates : StateMachineBuilder
{
    public DRN1TrinitySeekerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MercifulBreeze>()
            .ActivateOnEnter<MercifulBlooms>()
            .ActivateOnEnter<MercifulArc>()
            .ActivateOnEnter<BurningChains>()
            .ActivateOnEnter<IronImpact>()
            .ActivateOnEnter<IronRose>()
            //.ActivateOnEnter<ActOfMercy>() //cross aoes always on
            //.ActivateOnEnter<BalefulBlade>() //hide behind barrier always on
            //.ActivateOnEnter<BalefulSwathe>() //side aoes always on
            .ActivateOnEnter<IronSplitter>()
            .ActivateOnEnter<MercyFourfold>()
            //.ActivateOnEnter<MercifulMoon>() //gaze mechanic does not disable
            .ActivateOnEnter<DeadIron>();
    }
}