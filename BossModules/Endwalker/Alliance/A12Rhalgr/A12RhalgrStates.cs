namespace BossMod.Endwalker.Alliance.A12Rhalgr;

class A12RhalgrStates : StateMachineBuilder
{
    public A12RhalgrStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        LightningReign(id, 7.2f);
        HandOfTheDestroyer(id + 0x10000, 7.2f);
        BrokenWorld(id + 0x20000, 12.5f);
        RhalgrBeacon(id + 0x30000, 1.6f);
        HandOfTheDestroyer(id + 0x40000, 11.2f);
        DestructiveBolt(id + 0x50000, 11.5f);
        HandOfTheDestroyerBrokenShards(id + 0x60000, 6.2f, true);
        HandOfTheDestroyerBrokenShards(id + 0x70000, 12.4f);

        BronzeWork(id + 0x100000, 9.8f);
        HandOfTheDestroyerBrokenWorldLightningStorm(id + 0x110000, 1.1f); // any?
        HellOfLightningRhalgrBeacon(id + 0x120000, 5.9f);
        HandOfTheDestroyerBrokenShards(id + 0x130000, 8.5f); // any?
        LightningReign(id + 0x140000, 4.4f);

        BronzeWork(id + 0x200000, 11.5f);
        HandOfTheDestroyerBrokenWorldLightningStorm(id + 0x210000, 1.1f); // any?
        HellOfLightningRhalgrBeacon(id + 0x220000, 5.9f);
        HandOfTheDestroyerBrokenShards(id + 0x230000, 8.5f); // any? (note: didn't see this and beyond)
        LightningReign(id + 0x240000, 4.4f);

        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void LightningReign(uint id, float delay)
    {
        Cast(id, AID.LightningReign, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State DestructiveBolt(uint id, float delay)
    {
        Cast(id, AID.DestructiveBolt, delay, 4)
            .ActivateOnEnter<DestructiveBolt>();
        return ComponentCondition<DestructiveBolt>(id + 0x10, 1, comp => comp.NumFinishedSpreads > 0, "Tankbusters")
            .DeactivateOnExit<DestructiveBolt>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void HandOfTheDestroyer(uint id, float delay)
    {
        Cast(id, AID.AdventOfTheEighth, delay, 4);
        CastMulti(id + 0x10, [AID.HandOfTheDestroyerWrath, AID.HandOfTheDestroyerJudgment], 6.1f, 9)
            .ActivateOnEnter<HandOfTheDestroyer>();
        ComponentCondition<HandOfTheDestroyer>(id + 0x20, 0.4f, comp => comp.NumCasts > 0, "Side cleave")
            .DeactivateOnExit<HandOfTheDestroyer>();
    }

    private void BrokenWorld(uint id, float delay)
    {
        Cast(id, AID.BrokenWorld, delay, 3);
        ComponentCondition<BrokenWorld>(id + 0x10, 2.1f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<BrokenWorld>();
        ComponentCondition<BrokenWorld>(id + 0x20, 10.6f, comp => comp.NumCasts > 0, "Proximity")
            .DeactivateOnExit<BrokenWorld>();
    }

    private void HandOfTheDestroyerBrokenShards(uint id, float delay, bool first = false)
    {
        Cast(id, AID.AdventOfTheEighth, delay, 4);
        Cast(id + 0x10, AID.BrokenWorld, first ? 7.5f : 6.1f, 3);
        CastMulti(id + 0x20, [AID.HandOfTheDestroyerWrathBroken/*, AID.HandOfTheDestroyerJudgmentBroken*/], 2.1f, 9)
            .ActivateOnEnter<BrokenShards>();
        ComponentCondition<BrokenShards>(id + 0x30, 5.7f, comp => comp.NumCasts >= 9, "AOEs")
            .DeactivateOnExit<BrokenShards>();
    }

    private void HandOfTheDestroyerBrokenWorldLightningStorm(uint id, float delay)
    {
        Cast(id, AID.AdventOfTheEighth, delay, 4);
        Cast(id + 0x10, AID.BrokenWorld, 6.2f, 3);
        CastMulti(id + 0x20, [AID.HandOfTheDestroyerWrath, AID.HandOfTheDestroyerJudgment], 2.1f, 9)
            .ActivateOnEnter<HandOfTheDestroyer>()
            .ActivateOnEnter<BrokenWorld>();
        ComponentCondition<HandOfTheDestroyer>(id + 0x30, 0.4f, comp => comp.NumCasts > 0, "Side cleave")
            .DeactivateOnExit<HandOfTheDestroyer>();
        ComponentCondition<BrokenWorld>(id + 0x40, 1.2f, comp => comp.NumCasts > 0, "Proximity")
            .ActivateOnEnter<LightningStorm>() // spreads start ~0.5s before proximity
            .DeactivateOnExit<BrokenWorld>();
        ComponentCondition<LightningStorm>(id + 0x50, 7.5f, comp => comp.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<LightningStorm>();
    }

    private void RhalgrBeacon(uint id, float delay)
    {
        Cast(id, AID.RhalgrsBeacon, delay, 9.3f)
            .ActivateOnEnter<RhalgrBeaconAOE>()
            .ActivateOnEnter<RhalgrBeaconKnockback>();
        ComponentCondition<RhalgrBeaconKnockback>(id + 0x10, 0.7f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<RhalgrBeaconKnockback>();
        ComponentCondition<RhalgrBeaconAOE>(id + 0x11, 0.3f, comp => comp.NumCasts > 0, "AOE")
            .DeactivateOnExit<RhalgrBeaconAOE>();
    }

    private void HellOfLightningRhalgrBeacon(uint id, float delay)
    {
        Cast(id, AID.HellOfLightning, delay, 3);
        Cast(id + 0x10, AID.RhalgrsBeacon, 2.1f, 9.3f)
            .ActivateOnEnter<RhalgrBeaconAOE>()
            .ActivateOnEnter<RhalgrBeaconShock>() // actors are created ~0.1s into cast, start their casts 7s later
            .ActivateOnEnter<RhalgrBeaconKnockback>();
        ComponentCondition<RhalgrBeaconKnockback>(id + 0x20, 0.7f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<RhalgrBeaconKnockback>();
        ComponentCondition<RhalgrBeaconAOE>(id + 0x21, 0.3f, comp => comp.NumCasts > 0, "AOE")
            .DeactivateOnExit<RhalgrBeaconAOE>();
        ComponentCondition<RhalgrBeaconShock>(id + 0x30, 2.7f, comp => comp.NumCasts > 0, "Lightning orbs")
            .DeactivateOnExit<RhalgrBeaconShock>();
    }

    private void BronzeWork(uint id, float delay)
    {
        Cast(id, AID.BronzeWork, delay, 6.5f)
            .ActivateOnEnter<BronzeLightning>()
            .ActivateOnEnter<StrikingMeteor>(); // puddles start ~0.5s before cast end
        ComponentCondition<BronzeLightning>(id + 0x10, 0.5f, comp => comp.NumCasts > 0, "Cones 1");
        ComponentCondition<BronzeLightning>(id + 0x20, 2.0f, comp => comp.NumCasts > 4, "Cones 2")
            .DeactivateOnExit<BronzeLightning>();
        DestructiveBolt(id + 0x100, 1.6f)
            .DeactivateOnExit<StrikingMeteor>(); // second set of puddles finish ~0.4s into cast
    }
}
