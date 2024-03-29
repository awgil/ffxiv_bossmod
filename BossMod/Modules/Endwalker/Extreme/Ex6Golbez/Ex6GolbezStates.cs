namespace BossMod.Endwalker.Extreme.Ex6Golbez;

class Ex6GolbezStates : StateMachineBuilder
{
    public Ex6GolbezStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        TerrastormLingeringSpark(id, 12.6f);
        BindingCold(id + 0x10000, 4.5f);
        GaleSphere1(id + 0x20000, 10.4f);
        BindingCold(id + 0x30000, 4.5f);
        VoidMeteor(id + 0x40000, 3.2f);
        BlackFang(id + 0x50000, 9.6f);

        AzdajasShadow(id + 0x60000, 8.6f);
        PhasesOfTheShadow(id + 0x70000, 4.4f);

        DoubleMeteor(id + 0x80000, 2.9f);

        AzdajasShadow(id + 0x90000, 13.7f);
        VoidStardust(id + 0xA0000, 4.5f);
        BindingCold(id + 0xB0000, 8.0f);
        VoidMeteor(id + 0xC0000, 3.1f);
        PhasesOfTheShadow(id + 0xD0000, 4.4f);

        TerrastormArcticAssault(id + 0xE0000, 9.9f);
        BindingCold(id + 0xF0000, 2.2f);
        GaleSphere2(id + 0x100000, 8.4f);
        BindingCold(id + 0x110000, 4.5f);

        AzdajasShadow(id + 0x120000, 8.2f);
        VoidStardustLingeringSpark(id + 0x130000, 4.4f);
        PhasesOfTheShadow(id + 0x140000, 6.1f);

        DoubleMeteor(id + 0x150000, 2.9f);
        VoidMeteor(id + 0x160000, 8.6f);
        GaleSphere2(id + 0x170000, 9.6f);
        BindingCold(id + 0x180000, 4.5f);

        AzdajasShadow(id + 0x190000, 8.2f);
        PhasesOfTheShadow(id + 0x1A0000, 5); // TODO: don't know exact timings below
        BindingCold(id + 0x1B0000, 5);
        BindingCold(id + 0x1C0000, 8);
        VoidMeteor(id + 0x1D0000, 3);
        SimpleState(id + 0x1E0000, 20, "Enrage");
    }

    private void BindingCold(uint id, float delay)
    {
        Cast(id, AID.BindingCold, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void LingeringSparkStart(uint id, float delay)
    {
        CastStart(id, AID.LingeringSpark, delay);
    }

    private void LingeringSparkEnd(uint id, float delay)
    {
        CastEnd(id, delay);
        ComponentCondition<LingeringSpark>(id + 1, 1.2f, comp => comp.Casters.Count > 0, "Puddles bait")
            .ActivateOnEnter<LingeringSpark>();
        ComponentCondition<LingeringSpark>(id + 2, 3, comp => comp.NumCasts > 0, "Puddles resolve")
            .DeactivateOnExit<LingeringSpark>();
    }

    private void PhasesOfTheBlade(uint id, float delay)
    {
        Cast(id, AID.PhasesOfTheBlade, delay, 5, "Front cleave")
            .ActivateOnEnter<PhasesOfTheBladeFront>()
            .DeactivateOnExit<PhasesOfTheBladeFront>();
        ComponentCondition<PhasesOfTheBladeBack>(id + 2, 3.4f, comp => comp.NumCasts > 0, "Back cleave")
            .ActivateOnEnter<PhasesOfTheBladeBack>()
            .DeactivateOnExit<PhasesOfTheBladeBack>();
    }

    private void TerrastormLingeringSpark(uint id, float delay)
    {
        Cast(id, AID.Terrastorm, delay, 3);
        ComponentCondition<Terrastorm>(id + 0x10, 1.2f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Terrastorm>();
        LingeringSparkStart(id + 0x20, 6.0f);
        ComponentCondition<Terrastorm>(id + 0x30, 2.0f, comp => comp.NumCasts > 0, "Diagonals")
            .DeactivateOnExit<Terrastorm>();
        LingeringSparkEnd(id + 0x40, 1.0f);

        PhasesOfTheBlade(id + 0x1000, 0.3f);
    }

    private void TerrastormArcticAssault(uint id, float delay)
    {
        Cast(id, AID.Terrastorm, delay, 3);
        ComponentCondition<Terrastorm>(id + 0x10, 1.2f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Terrastorm>();
        CastStart(id + 0x20, AID.ArcticAssault, 2.0f)
            .ActivateOnEnter<VoidBlizzard>(); // icons appear and spells start at the same time
        CastEnd(id + 0x21, 3)
            .ActivateOnEnter<ArcticAssault>();
        ComponentCondition<ArcticAssault>(id + 0x22, 1.0f, comp => comp.NumCasts > 0, "Half room")
            .DeactivateOnExit<ArcticAssault>();
        ComponentCondition<Terrastorm>(id + 0x30, 2.0f, comp => comp.NumCasts > 0, "Diagonals + Party stacks")
            .DeactivateOnExit<Terrastorm>()
            .DeactivateOnExit<VoidBlizzard>(); // ends at the same time
    }

    private void GaleSphere1(uint id, float delay)
    {
        Cast(id, AID.GaleSphere, delay, 3);
        CastStart(id + 0x10, AID.ArcticAssault, 12.7f)
            .ActivateOnEnter<GaleSphere>();
        ComponentCondition<GaleSphere>(id + 0x12, 0.5f, comp => comp.NumCasts >= 1, "Spheres 1");
        CastEnd(id + 0x13, 2.5f)
            .ActivateOnEnter<ArcticAssault>(); // start showing this only after first sphere aoes are done
        ComponentCondition<GaleSphere>(id + 0x14, 1.0f, comp => comp.NumCasts >= 2, "Spheres 2 + Half room")
            .DeactivateOnExit<ArcticAssault>(); // happens at the same time
        ComponentCondition<GaleSphere>(id + 0x20, 3.5f, comp => comp.NumCasts >= 3, "Spheres 3");
        ComponentCondition<GaleSphere>(id + 0x30, 3.5f, comp => comp.NumCasts >= 4, "Spheres 4")
            .DeactivateOnExit<GaleSphere>();

        PhasesOfTheBlade(id + 0x1000, 0.3f);
    }

    // very similar to 1, but with extra 2/4 man stacks
    private void GaleSphere2(uint id, float delay)
    {
        Cast(id, AID.GaleSphere, delay, 3);
        CastStart(id + 0x10, AID.ArcticAssault, 12.7f)
            .ActivateOnEnter<GaleSphere>()
            .ActivateOnEnter<VoidAero>() // these start 7s after previous cast end
            .ActivateOnEnter<VoidTornado>();
        Condition(id + 0x11, 0.3f, () => !(Module.FindComponent<VoidAero>()?.Active ?? false) && !(Module.FindComponent<VoidTornado>()?.Active ?? false), "Stacks")
            .DeactivateOnExit<VoidAero>()
            .DeactivateOnExit<VoidTornado>();
        ComponentCondition<GaleSphere>(id + 0x12, 0.2f, comp => comp.NumCasts >= 1, "Spheres 1");
        CastEnd(id + 0x13, 2.5f)
            .ActivateOnEnter<ArcticAssault>(); // start showing this only after first sphere aoes are done
        ComponentCondition<GaleSphere>(id + 0x14, 1.0f, comp => comp.NumCasts >= 2, "Spheres 2 + Half room")
            .DeactivateOnExit<ArcticAssault>(); // happens at the same time
        ComponentCondition<GaleSphere>(id + 0x20, 3.5f, comp => comp.NumCasts >= 3, "Spheres 3")
            .ActivateOnEnter<VoidAero>() // these start 1.2s before 3rd spheres
            .ActivateOnEnter<VoidTornado>();
        ComponentCondition<GaleSphere>(id + 0x30, 3.5f, comp => comp.NumCasts >= 4, "Spheres 4")
            .DeactivateOnExit<GaleSphere>();

        CastStart(id + 0x1000, AID.PhasesOfTheBlade, 0.3f);
        Condition(id + 0x1001, 0.9f, () => !(Module.FindComponent<VoidAero>()?.Active ?? false) && !(Module.FindComponent<VoidTornado>()?.Active ?? false), "Stacks")
            .ActivateOnEnter<PhasesOfTheBladeFront>()
            .DeactivateOnExit<VoidAero>()
            .DeactivateOnExit<VoidTornado>();
        CastEnd(id + 0x1002, 4.1f, "Front cleave")
            .DeactivateOnExit<PhasesOfTheBladeFront>();
        ComponentCondition<PhasesOfTheBladeBack>(id + 0x1003, 3.4f, comp => comp.NumCasts > 0, "Back cleave")
            .ActivateOnEnter<PhasesOfTheBladeBack>()
            .DeactivateOnExit<PhasesOfTheBladeBack>();
    }

    private void VoidMeteor(uint id, float delay)
    {
        CastStart(id, AID.VoidMeteor, delay)
            .ActivateOnEnter<VoidMeteor>();
        CastEnd(id + 1, 5, "Small tankbusters start");
        // comets at +0.1, +1.1, +2.1 & +3.1
        ComponentCondition<VoidMeteor>(id + 0x10, 4.1f, comp => comp.NumCasts > 0, "Tankbuster hit")
            .DeactivateOnExit<VoidMeteor>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void BlackFang(uint id, float delay)
    {
        Cast(id, AID.AzdajasShadowBlackFang, delay, 5);
        Cast(id + 0x10, AID.BlackFang, 6.2f, 4);
        ComponentCondition<BlackFang>(id + 0x20, 3.8f, comp => comp.NumCasts > 0, "Raidwide hit 1")
            .ActivateOnEnter<BlackFang>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BlackFang>(id + 0x30, 2.9f, comp => comp.NumCasts >= 6, "Raidwide hit 6")
            .DeactivateOnExit<BlackFang>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    // leaves component active
    private void AzdajasShadow(uint id, float delay)
    {
        CastMulti(id, new[] { AID.AzdajasShadowCircleStack, AID.AzdajasShadowDonutSpread }, delay, 8)
            .ActivateOnEnter<AzdajasShadow>();
        ComponentCondition<FlamesOfEventide>(id + 0x10, 5.2f, comp => comp.NumCasts >= 1, "Tankbuster 1")
            .ActivateOnEnter<FlamesOfEventide>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<FlamesOfEventide>(id + 0x11, 3.1f, comp => comp.NumCasts >= 2, "Tankbuster 2")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<FlamesOfEventide>(id + 0x12, 3.1f, comp => comp.NumCasts >= 3, "Tankbuster 3")
            .DeactivateOnExit<FlamesOfEventide>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    // resolve for azdaja's shadow
    private void PhasesOfTheShadow(uint id, float delay)
    {
        Cast(id, AID.PhasesOfTheShadow, delay, 5, "Front cleave")
            .ActivateOnEnter<PhasesOfTheShadowFront>()
            .DeactivateOnExit<PhasesOfTheShadowFront>();
        ComponentCondition<PhasesOfTheShadowBack>(id + 2, 3.4f, comp => comp.NumCasts > 0, "Back cleave")
            .ActivateOnEnter<PhasesOfTheShadowBack>()
            .ActivateOnEnter<RisingBeacon>() // note: these casts start right after front cleave, but we don't want that interfering with first cleave aoe
            .ActivateOnEnter<RisingRing>()
            .ActivateOnEnter<BurningShade>() // note: these casts start ~1s before second cleave
            .ActivateOnEnter<ImmolatingShade>()
            .DeactivateOnExit<PhasesOfTheShadowBack>();
        Condition(id + 0x10, 1.3f, () => (Module.FindComponent<RisingBeacon>()?.NumCasts ?? 0) + (Module.FindComponent<RisingRing>()?.NumCasts ?? 0) > 0, "In/out")
            .DeactivateOnExit<RisingBeacon>()
            .DeactivateOnExit<RisingRing>();
        Condition(id + 0x20, 2.7f, () => !(Module.FindComponent<BurningShade>()?.Active ?? false) && !(Module.FindComponent<ImmolatingShade>()?.Active ?? false), "Spread/stack")
            .DeactivateOnExit<BurningShade>()
            .DeactivateOnExit<ImmolatingShade>()
            .DeactivateOnExit<AzdajasShadow>();
    }

    private void DoubleMeteor(uint id, float delay)
    {
        CastStart(id, AID.DoubleMeteor, delay)
            .ActivateOnEnter<DragonsDescent>()
            .ActivateOnEnter<DoubleMeteor>()
            .ActivateOnEnter<Explosion>();
        ComponentCondition<DragonsDescent>(id + 0x10, 8.0f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<DragonsDescent>();
        ComponentCondition<Explosion>(id + 0x11, 2.4f, comp => comp.Done, "Towers")
            .ActivateOnEnter<Cauterize>() // icon & tether appear 0.5s after knockback
            .DeactivateOnExit<Explosion>();
        CastEnd(id + 0x12, 0.5f);
        ComponentCondition<DoubleMeteor>(id + 0x13, 0.2f, comp => !comp.Active, "Flares")
            .DeactivateOnExit<DoubleMeteor>();
        ComponentCondition<Cauterize>(id + 0x20, 1.5f, comp => comp.NumCasts > 0, "Line")
            .DeactivateOnExit<Cauterize>();
    }

    // leaves abyssal quasar active
    private void VoidStardustAbyssalQuasarStart(uint id, float delay)
    {
        Cast(id, AID.VoidStardust, delay, 3);
        ComponentCondition<VoidStardust>(id + 0x10, 6.3f, comp => comp.NumCasts > 0, "Exaflares start")
            .ActivateOnEnter<VoidStardust>()
            .ActivateOnEnter<AbyssalQuasar>(); // casts start ~4.3s after boss cast end
        ComponentCondition<VoidStardust>(id + 0x20, 2.9f, comp => comp.NumCasts >= 16, "Exaflares end")
            .DeactivateOnExit<VoidStardust>();
    }

    private State VoidStardustAbyssalQuasarEnd(uint id, float delay)
    {
        return ComponentCondition<AbyssalQuasar>(id, delay, comp => !comp.Active, "Stack in pairs")
            .DeactivateOnExit<AbyssalQuasar>();
    }

    private void VoidStardust(uint id, float delay)
    {
        VoidStardustAbyssalQuasarStart(id, delay);
        CastStartMulti(id + 0x100, new[] { AID.EventideTriad, AID.EventideFall }, 3.0f);
        VoidStardustAbyssalQuasarEnd(id + 0x110, 0.1f)
            .ActivateOnEnter<EventideFallTriad>();
        CastEnd(id + 0x120, 4.9f, "Triad/fall")
            .DeactivateOnExit<EventideFallTriad>();
        // fall: aoe at +1.6 & +4.6
        // triad: aoe at +1.4 & +3.2 & +5.0
    }

    private void VoidStardustLingeringSpark(uint id, float delay)
    {
        VoidStardustAbyssalQuasarStart(id, delay);
        LingeringSparkStart(id + 0x100, 1.0f);
        VoidStardustAbyssalQuasarEnd(id + 0x110, 2.1f);
        LingeringSparkEnd(id + 0x120, 0.9f);
        CastMulti(id + 0x200, new[] { AID.EventideTriad, AID.EventideFall }, 0.1f, 5, "Triad/fall")
            .ActivateOnEnter<EventideFallTriad>()
            .DeactivateOnExit<EventideFallTriad>();
    }
}
