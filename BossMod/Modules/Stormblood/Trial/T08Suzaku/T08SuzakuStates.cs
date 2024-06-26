namespace BossMod.Stormblood.Trial.T08Suzaku;

class T08SuzakuStates : StateMachineBuilder
{
    public T08SuzakuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ScreamsOfTheDamned>()
            .ActivateOnEnter<SouthronStar>()
            .ActivateOnEnter<AshesToAshes>()
            .ActivateOnEnter<ScarletFeverAOE>()
            .ActivateOnEnter<RuthlessRefrain>()
            .ActivateOnEnter<Cremate>()
            .ActivateOnEnter<PhantomFlurryTankbuster>()
            .ActivateOnEnter<PhantomFlurryAOE>()
            .ActivateOnEnter<FleetingSummer>()
            .ActivateOnEnter<Hotspot>()
            .ActivateOnEnter<Swoop>()
            .ActivateOnEnter<WellOfFlame>()
            .ActivateOnEnter<ScarletFever>();
    }
}
