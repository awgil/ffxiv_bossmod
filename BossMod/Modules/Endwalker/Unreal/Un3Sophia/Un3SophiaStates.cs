namespace BossMod.Endwalker.Unreal.Un3Sophia;

class Un3SophiaStates : StateMachineBuilder
{
    public Un3SophiaStates(BossModule module) : base(module)
    {
        SimplePhase(0, Phase1, "Before adds (until 83%)")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || !Module.PrimaryActor.IsTargetable;
        SimplePhase(1, Phase2, "Adds + next few mechanics (until 75%)")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || (Module.PrimaryActor.CastInfo?.IsSpell(AID.ThunderCone) ?? false);
        SimplePhase(2, Phase3, "Skippable mechanics (until 68%)")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.Enemies(OID.AionTeleos).Any();
        DeathPhase(3, Phase4);
    }

    private void Phase1(uint id)
    {
        ThunderCone(id, 8.2f);
        Gnosis(id + 0x10000, 7.4f);
        Execute(id + 0x20000, 15.4f); // TODO: sometimes 22.4 instead?.. maybe there should be an extra phase here?
        ArmsOfWisdom(id + 0x30000, 6.2f);
        SimpleState(id + 0x40000, 15.4f, "Add phase (83% short circuit)");
    }

    private void Phase2(uint id)
    {
        CloudyHeavensScalesOfWisdom(id, 0.1f);
        Proximity(id + 0x10000, 5.2f);
        ThunderDonutAero(id + 0x20000, 1.5f);
        Quasar(id + 0x30000, 3.2f);
        Cintamani(id + 0x40000, 4.7f, false);
        ArmsOfWisdom(id + 0x50000, 2.2f);
        SimpleState(id + 0x60000, 4.5f, "Frontal cone (75% short circuit)");
    }

    private void Phase3(uint id)
    {
        ThunderCone(id, 0);
        QuasarOnrush(id + 0x10000, 3.2f);
        PairsCintamani(id + 0x20000, 10.1f);
        ArmsOfWisdom(id + 0x30000, 3.1f);
        SimpleState(id + 0x40000, 8.6f, "Raidwide x3 (68% short circuit)");
    }

    private void Phase4(uint id)
    {
        Cintamani(id, 3.9f, true);
        ExecuteArmsOfWisdomLightDew(id + 0x10000, 3.1f);
        Cintamani(id + 0x20000, 10.5f, true);
        ArmsOfWisdom(id + 0x30000, 3.1f);
        QuasarLightDew(id + 0x40000, 3.2f);

        // repeat 1
        ThunderDonut(id + 0x100000, 9.3f);
        ArmsOfWisdom(id + 0x110000, 4.2f);
        QuasarOnrushLightDew(id + 0x120000, 3.3f);
        Cintamani(id + 0x130000, 7.2f, true);
        ArmsOfWisdom(id + 0x140000, 3.1f);
        ThunderCone(id + 0x150000, 4.2f);
        PairsGnosis(id + 0x160000, 5.1f);
        ArmsOfWisdom(id + 0x170000, 8.3f);
        PairsQuasar(id + 0x180000, 11.2f);
        Cintamani(id + 0x190000, 5.7f, true);
        ArmsOfWisdom(id + 0x1A0000, 3.1f);
        ExecuteProximityArmsOfWisdomPairs(id + 0x1B0000, 4.2f);
        Cintamani(id + 0x1C0000, 12.2f, true);
        ArmsOfWisdom(id + 0x1D0000, 3.1f);
        QuasarLightDew(id + 0x1E0000, 3.2f);

        // repeat 2
        ThunderDonut(id + 0x200000, 9.3f);
        ArmsOfWisdom(id + 0x210000, 4.2f);
        QuasarOnrushLightDew(id + 0x220000, 3.3f);
        Cintamani(id + 0x230000, 7.2f, true);
        ArmsOfWisdom(id + 0x240000, 3.1f);
        ThunderCone(id + 0x250000, 4.2f);
        PairsGnosis(id + 0x260000, 5.1f);
        ArmsOfWisdom(id + 0x270000, 8.3f); // note: sometimes it is skipped and encounter goes straight to enrage

        SimpleState(id + 0x300000, 13.4f, "Enrage"); // 5 seconds before that, everyone gets icons of same color; enrage cast happens together with impossible 'resolve'
    }

    private void ArmsOfWisdom(uint id, float delay)
    {
        Cast(id, AID.ArmsOfWisdom, delay, 4, "Tankbuster")
            .ActivateOnEnter<ArmsOfWisdom>()
            .DeactivateOnExit<ArmsOfWisdom>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Gnosis(uint id, float delay)
    {
        ActorCast(id, Module.Enemies(OID.Barbelo).FirstOrDefault, AID.Gnosis, delay, 3, false, "Knockback")
            .ActivateOnEnter<Gnosis>()
            .DeactivateOnExit<Gnosis>();
    }

    private void Cintamani(uint id, float delay, bool triple)
    {
        ComponentCondition<Cintamani>(id, delay, comp => comp.NumCasts > 0, "Raidwide (1)")
            .ActivateOnEnter<Cintamani>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Cintamani>(id + 0x10, triple ? 4.1f : 2.1f, comp => comp.NumCasts >= (triple ? 3 : 2), triple ? "Raidwide (3)" : "Raidwide (2)")
            .DeactivateOnExit<Cintamani>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ThunderCone(uint id, float delay)
    {
        Cast(id, AID.ThunderCone, delay, 3, "Frontal cone")
            .ActivateOnEnter<ThunderCone>()
            .DeactivateOnExit<ThunderCone>();
    }

    private void ThunderDonut(uint id, float delay)
    {
        Cast(id, AID.ThunderDonut, delay, 3, "Donut")
            .ActivateOnEnter<ThunderDonut>()
            .DeactivateOnExit<ThunderDonut>();
    }

    private void Aero(uint id, float delay)
    {
        Cast(id, AID.Aero, delay, 3, "Circle")
            .ActivateOnEnter<Aero>()
            .DeactivateOnExit<Aero>();
    }

    private void RandomThunder(uint id, float delay)
    {
        CastMulti(id, new[] { AID.ThunderDonut, AID.ThunderCone }, delay, 3, "Donut/cone")
            .ActivateOnEnter<ThunderDonut>()
            .ActivateOnEnter<ThunderCone>()
            .DeactivateOnExit<ThunderDonut>()
            .DeactivateOnExit<ThunderCone>();
    }

    private void ThunderDonutAero(uint id, float delay)
    {
        CastMulti(id, new[] { AID.ThunderDonut, AID.Aero }, delay, 3, "Donut/circle")
            .ActivateOnEnter<ThunderDonut>()
            .ActivateOnEnter<Aero>()
            .DeactivateOnExit<ThunderDonut>()
            .DeactivateOnExit<Aero>();
    }

    // TODO: consider merging components and providing early execute hints...
    private void Execute(uint id, float delay)
    {
        RandomThunder(id, delay);
        Aero(id + 0x100, 3.2f);
        Cast(id + 0x200, AID.Execute, 5.2f, 5, "Repeat by copies")
            .ActivateOnEnter<ExecuteDonut>()
            .ActivateOnEnter<ExecuteAero>()
            .ActivateOnEnter<ExecuteCone>()
            .DeactivateOnExit<ExecuteDonut>()
            .DeactivateOnExit<ExecuteAero>()
            .DeactivateOnExit<ExecuteCone>();
    }

    private void ExecuteArmsOfWisdomLightDew(uint id, float delay)
    {
        ThunderDonut(id, delay);
        Aero(id + 0x100, 3.1f);
        ArmsOfWisdom(id + 0x200, 5.2f);

        CastStart(id + 0x300, AID.Execute, 7.3f);
        ActorCastStart(id + 0x301, Module.Enemies(OID.Barbelo).FirstOrDefault, AID.LightDewShort, 2.1f)
            .ActivateOnEnter<ExecuteDonut>()
            .ActivateOnEnter<ExecuteAero>()
            .ActivateOnEnter<ExecuteCone>();
        ActorCastEnd(id + 0x302, Module.Enemies(OID.Barbelo).FirstOrDefault, 2, false, "Line")
            .ActivateOnEnter<LightDewShort>()
            .DeactivateOnExit<LightDewShort>();
        CastEnd(id + 0x303, 0.9f, "Repeat by copies")
            .DeactivateOnExit<ExecuteDonut>()
            .DeactivateOnExit<ExecuteAero>()
            .DeactivateOnExit<ExecuteCone>();
    }

    private void CloudyHeavensScalesOfWisdom(uint id, float delay)
    {
        Targetable(id, true, delay);
        Cast(id + 0x10, AID.CloudyHeavens, 2.1f, 3, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<Demiurges>(id + 0x1000, 1.5f, comp => comp.AddsActive, "Adds appear")
            .ActivateOnEnter<Demiurges>();
        ComponentCondition<Demiurges>(id + 0x2000, 80, comp => !comp.AddsActive, "Adds enrage", 100) // note: it could've been a phase instead, but cba
            .ActivateOnEnter<DivineSpark>()
            .ActivateOnEnter<GnosticRant>()
            .ActivateOnEnter<GnosticSpear>()
            .ActivateOnEnter<RingOfPain>()
            .DeactivateOnExit<Demiurges>()
            .DeactivateOnExit<DivineSpark>()
            .DeactivateOnExit<GnosticRant>()
            .DeactivateOnExit<GnosticSpear>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        // +2.3s: remove doom statuses (not sure what happens if it ticks down naturally right after adds are killed)

        ComponentCondition<ScalesOfWisdom>(id + 0x3000, 2.4f, comp => comp.Distance > 0, "", 5) // note: can be slightly delayed if adds enrage
            .ActivateOnEnter<ScalesOfWisdom>();
        ComponentCondition<ScalesOfWisdom>(id + 0x3001, 8.0f, comp => comp.NumCasts > 0, "Tilt 1");
        ComponentCondition<ScalesOfWisdom>(id + 0x3002, 5.0f, comp => comp.NumCasts > 1, "Tilt 2");
        ComponentCondition<ScalesOfWisdom>(id + 0x3003, 4.5f, comp => comp.RaidwideDone , "Raidwide")
            .DeactivateOnExit<ScalesOfWisdom>()
            .SetHint(StateMachine.StateHint.Raidwide);

        Targetable(id + 0x4000, true, 5.5f, "Boss reappears")
            .DeactivateOnExit<RingOfPain>(); // voidzone from add phase should disappear by now
    }

    private void Proximity(uint id, float delay)
    {
        Cast(id, AID.Quasar1, delay, 2.7f);
        ComponentCondition<QuasarProximity1>(id + 0x10, 1, comp => comp.Casters.Count > 0, "Proximity baits")
            .ActivateOnEnter<QuasarProximity1>()
            .ActivateOnEnter<QuasarProximity2>();
        ComponentCondition<QuasarProximity1>(id + 0x11, 5, comp => comp.Casters.Count == 0, "Proximity resolve")
            .DeactivateOnExit<QuasarProximity1>()
            .DeactivateOnExit<QuasarProximity2>();
    }

    private void Quasar(uint id, float delay)
    {
        Cast(id, AID.Quasar1, delay, 2.7f);
        ComponentCondition<Quasar>(id + 0x10, 1, comp => comp.WeightLeft + comp.WeightRight > 0)
            .ActivateOnEnter<Quasar>();
        ComponentCondition<Quasar>(id + 0x20, 7.8f, comp => comp.NumCasts > 0, "Tilt")
            .DeactivateOnExit<Quasar>();
    }

    private void QuasarOnrush(uint id, float delay)
    {
        Cast(id, AID.Quasar2, delay, 3);
        Targetable(id + 0x10, false, 0.9f, "Boss disappear");
        ComponentCondition<Quasar>(id + 0x20, 0.1f, comp => comp.WeightLeft + comp.WeightRight > 0)
            .ActivateOnEnter<Quasar>();
        Cast(id + 0x30, AID.Onrush, 3.2f, 3, "Half room aoe")
            .ActivateOnEnter<Onrush>()
            .DeactivateOnExit<Onrush>();
        ComponentCondition<Quasar>(id + 0x40, 1.6f, comp => comp.NumCasts > 0, "Tilt")
            .DeactivateOnExit<Quasar>();
        Targetable(id + 0x50, true, 3.6f, "Boss reappear");
    }

    private void QuasarLightDew(uint id, float delay)
    {
        Cast(id, AID.Quasar1, delay, 2.7f);
        ComponentCondition<Quasar>(id + 0x10, 1, comp => comp.WeightLeft + comp.WeightRight > 0)
            .ActivateOnEnter<Quasar>();
        ComponentCondition<Quasar>(id + 0x20, 7.8f, comp => comp.NumCasts > 0, "Tilt")
            .DeactivateOnExit<Quasar>();
        ActorCast(id + 0x30, Module.Enemies(OID.Barbelo).FirstOrDefault, AID.LightDewShort, 0.5f, 2, false, "Line")
            .ActivateOnEnter<LightDewShort>()
            .DeactivateOnExit<LightDewShort>();
    }

    private void QuasarOnrushLightDew(uint id, float delay)
    {
        Cast(id, AID.Quasar2, delay, 3);
        Targetable(id + 0x10, false, 0.9f, "Boss disappear");
        ComponentCondition<Quasar>(id + 0x20, 0, comp => comp.WeightLeft + comp.WeightRight > 0)
            .ActivateOnEnter<Quasar>();
        ActorCastStart(id + 0x30, Module.Enemies(OID.Barbelo).FirstOrDefault, AID.LightDewLong, 3.3f);
        CastStart(id + 0x31, AID.Onrush, 3.0f)
            .ActivateOnEnter<LightDewLong>();
        ComponentCondition<Quasar>(id + 0x40, 1.5f, comp => comp.NumCasts > 0, "Tilt")
            .ActivateOnEnter<Onrush>()
            .DeactivateOnExit<Quasar>();
        CastEnd(id + 0x42, 1.5f, "Half room aoe")
            .DeactivateOnExit<Onrush>();
        ActorCastEnd(id + 0x43, Module.Enemies(OID.Barbelo).FirstOrDefault, 1, false, "Line")
            .DeactivateOnExit<LightDewLong>();
        Targetable(id + 0x50, true, 4.2f, "Boss reappear");
    }

    private void PairsCintamani(uint id, float delay)
    {
        ComponentCondition<Pairs>(id, delay, comp => comp.Active)
            .ActivateOnEnter<Pairs>();
        ComponentCondition<Pairs>(id + 1, 5, comp => !comp.Active, "Color pairs")
            .DeactivateOnExit<Pairs>();
        Cintamani(id + 0x100, 0.2f, false);
    }

    private void PairsGnosis(uint id, float delay)
    {
        ComponentCondition<Pairs>(id, delay, comp => comp.Active)
            .ActivateOnEnter<Pairs>();
        ComponentCondition<Pairs>(id + 1, 5, comp => !comp.Active, "Color pairs")
            .DeactivateOnExit<Pairs>();
        Gnosis(id + 0x100, 1.3f);
    }

    private void PairsQuasar(uint id, float delay)
    {
        ComponentCondition<Pairs>(id, delay, comp => comp.Active)
            .ActivateOnEnter<Pairs>();
        Cast(id + 0x10, AID.Quasar1, 1.1f, 2.7f);
        ComponentCondition<Quasar>(id + 0x20, 1, comp => comp.WeightLeft + comp.WeightRight > 0)
            .ActivateOnEnter<Quasar>(); // resolves slightly after casts start; TODO: find better condition
        ComponentCondition<Pairs>(id + 0x21, 0.2f, comp => !comp.Active, "Color pairs")
            .DeactivateOnExit<Pairs>();
        ComponentCondition<Quasar>(id + 0x30, 7.6f, comp => comp.NumCasts > 0, "Tilt")
            .DeactivateOnExit<Quasar>();
    }

    private void ExecuteProximityArmsOfWisdomPairs(uint id, float delay)
    {
        ThunderCone(id, delay);
        Aero(id + 0x1000, 3.1f);
        Proximity(id + 0x2000, 3.2f);
        ArmsOfWisdom(id + 0x3000, 2.5f);

        ComponentCondition<Pairs>(id + 0x4000, 7.1f, comp => comp.Active)
            .ActivateOnEnter<Pairs>();
        CastStart(id + 0x4001, AID.Execute, 2.1f);
        ComponentCondition<Pairs>(id + 0x4002, 2.9f, comp => !comp.Active, "Color pairs")
            .ActivateOnEnter<ExecuteDonut>()
            .ActivateOnEnter<ExecuteAero>()
            .ActivateOnEnter<ExecuteCone>()
            .DeactivateOnExit<Pairs>();
        CastEnd(id + 0x4003, 2.1f, "Repeat by copies")
            .DeactivateOnExit<ExecuteDonut>()
            .DeactivateOnExit<ExecuteAero>()
            .DeactivateOnExit<ExecuteCone>();
    }
}
