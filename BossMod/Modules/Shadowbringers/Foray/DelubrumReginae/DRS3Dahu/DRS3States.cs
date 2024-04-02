namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS3Dahu;

class DRS3States : StateMachineBuilder
{
    public DRS3States(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        ReverberatingRoarHotChargeFirebreathe(id, 10.3f);
        HeadDownSpitFlameShockwaveFeralHowl(id + 0x10000, 14.5f);
        FirebreatheRotating(id + 0x20000, 1.6f);
        CrownedMarchosias(id + 0x30000, 7.4f);
        ReverberatingRoarHeadDownShockwaveSpitFlameHystericAssault(id + 0x40000, 37);
        FirebreatheRotating(id + 0x50000, 6.1f, true);
        // TODO: shockwave? -> hot charge + firebreathe -> ...
        SimpleState(id + 0xFF0000, 100, "???");
    }

    private void Shockwave(uint id, float delay)
    {
        CastMulti(id, new[] { AID.LeftSidedShockwaveFirst, AID.RightSidedShockwaveFirst }, delay, 3, "Shockwave 1")
            .ActivateOnEnter<Shockwave>();
        CastMulti(id + 0x10, new[] { AID.LeftSidedShockwaveSecond, AID.RightSidedShockwaveSecond }, 1.6f, 1, "Shockwave 2")
            .DeactivateOnExit<Shockwave>();
    }

    private State SpitFlame(uint id, float delay)
    {
        CastStart(id, AID.SpitFlame, delay)
            .ActivateOnEnter<SpitFlame>(); // first icon appears right before cast start
        CastEnd(id + 1, 8);
        return ComponentCondition<SpitFlame>(id + 0x10, 3.7f, comp => !comp.Active, "Spits resolve")
            .DeactivateOnExit<SpitFlame>();
    }

    private void ReverberatingRoarHotChargeFirebreathe(uint id, float delay)
    {
        ComponentCondition<FallingRock>(id, delay, comp => comp.Casters.Count > 0, "Rocks 1 bait")
            .ActivateOnEnter<FallingRock>();
        Cast(id + 0x10, AID.HotCharge, 9.5f, 3, "Charge 1") // note: large variance
            .ActivateOnEnter<HotCharge>()
            .DeactivateOnExit<HotCharge>();
        Cast(id + 0x20, AID.HotCharge, 1.8f, 3, "Charge 2")
            .ActivateOnEnter<HotCharge>()
            .DeactivateOnExit<HotCharge>()
            .DeactivateOnExit<FallingRock>(); // last rock ends ~1.1s before cast start
        Cast(id + 0x30, AID.Firebreathe, 1.8f, 5, "Cone")
            .ActivateOnEnter<Firebreathe>()
            .DeactivateOnExit<Firebreathe>();
    }

    private void HeadDownSpitFlameShockwaveFeralHowl(uint id, float delay)
    {
        ComponentCondition<HeadDown>(id, delay, comp => comp.Casters.Count > 0, "Add charges begin")
            .ActivateOnEnter<HeadDown>();
        SpitFlame(id + 0x100, 4.8f);
        Shockwave(id + 0x200, 3.2f);
        ComponentCondition<HeadDown>(id + 0x300, 1, comp => comp.Casters.Count == 0, "Add charges resolve", 5) // if one of the spit flame target dies, shockwave happens a bit earlier
            .DeactivateOnExit<HeadDown>();

        Cast(id + 0x400, AID.FeralHowl, 1.6f, 5)
            .ActivateOnEnter<HuntersClaw>()
            .ActivateOnEnter<FeralHowl>();
        ComponentCondition<FeralHowl>(id + 0x410, 2.1f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<FeralHowl>();
        ComponentCondition<HuntersClaw>(id + 0x420, 1.4f, comp => comp.NumCasts > 0, "Add aoes")
            .DeactivateOnExit<HuntersClaw>();
    }

    private void FirebreatheRotating(uint id, float delay, bool withHeadDown = false)
    {
        CastStart(id, AID.FirebreatheRotating, delay)
            .ActivateOnEnter<FirebreatheRotating>(); // icon appears just before cast start
        CastEnd(id + 1, 5)
            .ActivateOnEnter<HeadDown>(withHeadDown);
        ComponentCondition<FirebreatheRotating>(id + 0x10, 0.7f, comp => comp.NumCasts > 0, "Cone 1");
        ComponentCondition<FirebreatheRotating>(id + 0x11, 2, comp => comp.NumCasts > 1);
        ComponentCondition<FirebreatheRotating>(id + 0x12, 2, comp => comp.NumCasts > 2);
        ComponentCondition<FirebreatheRotating>(id + 0x13, 2, comp => comp.NumCasts > 3);
        ComponentCondition<FirebreatheRotating>(id + 0x14, 2, comp => comp.NumCasts > 4, "Cone 5")
            .DeactivateOnExit<FirebreatheRotating>()
            .DeactivateOnExit<HeadDown>(withHeadDown);
    }

    private void CrownedMarchosias(uint id, float delay)
    {
        Condition(id, delay, () => Module.Enemies(OID.CrownedMarchosias).Any(add => add.IsTargetable), "Adds appear");
        // +5.2s: second set
        // +10.0s: third set, first set gets damage up
        // +14.1s: first set gets second stack of damage up
        // and so on...
    }

    private void ReverberatingRoarHeadDownShockwaveSpitFlameHystericAssault(uint id, float delay)
    {
        ComponentCondition<FallingRock>(id, delay, comp => comp.Casters.Count > 0, "Rocks 1 bait")
            .ActivateOnEnter<FallingRock>();
        ComponentCondition<HeadDown>(id + 1, 0.2f, comp => comp.Casters.Count > 0, "Add charges begin")
            .ActivateOnEnter<HeadDown>();
        Shockwave(id + 0x100, 3.9f);
        SpitFlame(id + 0x200, 2.5f)
            .DeactivateOnExit<FallingRock>() // last rocks end ~1.1s before cast start
            .DeactivateOnExit<HeadDown>(); // last charges end ~2.5s after cast start

        ComponentCondition<Burn>(id + 0x300, 1.0f, comp => comp.CurrentBaits.Count > 0)
            .ActivateOnEnter<Burn>();

        Cast(id + 0x400, AID.HystericAssault, 0.1f, 5)
            .ActivateOnEnter<HuntersClaw>()
            .ActivateOnEnter<HystericAssault>();
        ComponentCondition<HystericAssault>(id + 0x310, 0.9f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<HystericAssault>();
        ComponentCondition<Burn>(id + 0x320, 2.2f, comp => comp.NumCasts > 0, "Flares")
            .DeactivateOnExit<Burn>();
        ComponentCondition<HuntersClaw>(id + 0x330, 0.4f, comp => comp.NumCasts > 0, "Add aoes")
            .DeactivateOnExit<HuntersClaw>();
    }
}
