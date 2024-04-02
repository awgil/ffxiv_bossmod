namespace BossMod.Endwalker.Criterion.C01ASS.C012Gladiator;

class C012GladiatorStates : StateMachineBuilder
{
    private bool _savage;

    public C012GladiatorStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        FlashOfSteel(id, 6.2f);
        SpecterOfMight(id + 0x10000, 8.2f);
        SculptorsPassion(id + 0x20000, 2.6f);
        MightySmite(id + 0x30000, 10.2f);
        CurseOfTheFallen(id + 0x40000, 8.2f);
        FlashOfSteel(id + 0x50000, 2.8f);
        HatefulVisage(id + 0x60000, 12.5f);
        FlashOfSteel(id + 0x70000, 3.4f);
        AccursedVisage(id + 0x80000, 7.2f);
        FlashOfSteel(id + 0x90000, 3.4f);
        CurseOfTheMonument(id + 0xA0000, 12.4f);
        FlashOfSteel(id + 0xB0000, 2.2f);
        SpecterOfMight(id + 0xC0000, 10.2f);
        SculptorsPassion(id + 0xD0000, 2.6f);
        FlashOfSteel(id + 0xE0000, 10.6f);
        Cast(id + 0xF0000, AID.Enrage, 2.1f, 10, "Enrage");
    }

    private void FlashOfSteel(uint id, float delay)
    {
        Cast(id, _savage ? AID.SFlashOfSteel : AID.NFlashOfSteel, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MightySmite(uint id, float delay)
    {
        Cast(id, _savage ? AID.SMightySmite : AID.NMightySmite, delay, 5, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void SpecterOfMight(uint id, float delay)
    {
        Cast(id, _savage ? AID.SSpecterOfMight : AID.NSpecterOfMight, delay, 4);
        ComponentCondition<RushOfMightFront>(id + 0x10, 4.2f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<NRushOfMightFront>(!_savage)
            .ActivateOnEnter<SRushOfMightFront>(_savage);
        ComponentCondition<RushOfMightFront>(id + 0x11, 10.5f, comp => comp.NumCasts > 0, "Charge 1 front")
            .DeactivateOnExit<RushOfMightFront>();

        CastStart(id + 0x20, _savage ? AID.SSpecterOfMight : AID.NSpecterOfMight, 0.5f)
            .ActivateOnEnter<NRushOfMightBack>(!_savage)
            .ActivateOnEnter<SRushOfMightBack>(_savage);
        ComponentCondition<RushOfMightBack>(id + 0x21, 1.5f, comp => comp.NumCasts > 0, "Charge 1 back")
            .DeactivateOnExit<RushOfMightBack>();
        CastEnd(id + 0x22, 2.5f);
        ComponentCondition<RushOfMightFront>(id + 0x30, 4.2f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<NRushOfMightFront>(!_savage)
            .ActivateOnEnter<SRushOfMightFront>(_savage);
        ComponentCondition<RushOfMightFront>(id + 0x31, 10.5f, comp => comp.NumCasts > 0, "Charge 2 front")
            .DeactivateOnExit<RushOfMightFront>();
        ComponentCondition<RushOfMightBack>(id + 0x40, 2, comp => comp.NumCasts > 0, "Charge 2 back")
            .ActivateOnEnter<NRushOfMightBack>(!_savage)
            .ActivateOnEnter<SRushOfMightBack>(_savage)
            .DeactivateOnExit<RushOfMightBack>();
    }

    private void SculptorsPassion(uint id, float delay)
    {
        CastStart(id, _savage ? AID.SSculptorsPassion : AID.NSculptorsPassion, delay)
            .ActivateOnEnter<NSculptorsPassion>(!_savage)
            .ActivateOnEnter<SSculptorsPassion>(_savage);
        CastEnd(id + 1, 5);
        ComponentCondition<SculptorsPassion>(id + 2, 0.3f, comp => comp.NumCasts > 0, "Wild charge")
            .DeactivateOnExit<SculptorsPassion>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void CurseOfTheFallen(uint id, float delay)
    {
        Cast(id, _savage ? AID.SCurseOfTheFallen : AID.NCurseOfTheFallen, delay, 5);
        ComponentCondition<CurseOfTheFallen>(id + 2, 1.1f, comp => comp.Active)
            .ActivateOnEnter<CurseOfTheFallen>();

        CastMulti(id + 0x10, new AID[] { _savage ? AID.SRingOfMight1Out : AID.NRingOfMight1Out, _savage ? AID.SRingOfMight2Out : AID.NRingOfMight2Out, _savage ? AID.SRingOfMight3Out : AID.NRingOfMight3Out }, 3.7f, 10, "Out")
            .ActivateOnEnter<NRingOfMight1Out>(!_savage)
            .ActivateOnEnter<NRingOfMight2Out>(!_savage)
            .ActivateOnEnter<NRingOfMight3Out>(!_savage)
            .ActivateOnEnter<SRingOfMight1Out>(_savage)
            .ActivateOnEnter<SRingOfMight2Out>(_savage)
            .ActivateOnEnter<SRingOfMight3Out>(_savage)
            .DeactivateOnExit<RingOfMight1Out>()
            .DeactivateOnExit<RingOfMight2Out>()
            .DeactivateOnExit<RingOfMight3Out>()
            .SetHint(StateMachine.StateHint.Raidwide); // first debuff resolve ~0.2s later


        Condition(id + 0x20, 2, () => Module.FindComponent<RingOfMight1In>()!.NumCasts + Module.FindComponent<RingOfMight2In>()!.NumCasts + Module.FindComponent<RingOfMight3In>()!.NumCasts > 0, "In")
            .ActivateOnEnter<NRingOfMight1In>(!_savage)
            .ActivateOnEnter<NRingOfMight2In>(!_savage)
            .ActivateOnEnter<NRingOfMight3In>(!_savage)
            .ActivateOnEnter<SRingOfMight1In>(_savage)
            .ActivateOnEnter<SRingOfMight2In>(_savage)
            .ActivateOnEnter<SRingOfMight3In>(_savage)
            .DeactivateOnExit<RingOfMight1In>()
            .DeactivateOnExit<RingOfMight2In>()
            .DeactivateOnExit<RingOfMight3In>();

        ComponentCondition<CurseOfTheFallen>(id + 0x30, 1.4f, comp => !comp.Active, "Resolve")
            .DeactivateOnExit<CurseOfTheFallen>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void WrathOfRuin(uint id, float delay)
    {
        // -0.7s: HatefulVisage actors spawn
        Cast(id, _savage ? AID.SWrathOfRuin : AID.NWrathOfRuin, delay, 3)
            .ActivateOnEnter<GoldenSilverFlame>(); // casts start ~2.1s after cast-start
        // +1.4s: first set of Regrets spawn
        // +3.4s: second set of Regrets spawn
        // +5.4s: first set of RackAndRuin cast-starts
        CastStart(id + 0x10, _savage ? AID.SNothingBesideRemains : AID.NNothingBesideRemains, 5.7f)
            .ActivateOnEnter<NRackAndRuin>(!_savage)
            .ActivateOnEnter<SRackAndRuin>(_savage);
        // +1.7s: second set of RackAndRuin cast-starts
        ComponentCondition<GoldenSilverFlame>(id + 0x20, 3.4f, comp => !comp.Active, "Cells")
            .ActivateOnEnter<NNothingBesideRemains>(!_savage)
            .ActivateOnEnter<SNothingBesideRemains>(_savage)
            .DeactivateOnExit<GoldenSilverFlame>();
        ComponentCondition<RackAndRuin>(id + 0x21, 0.3f, comp => comp.NumCasts > 0, "Lines 1");
        CastEnd(id + 0x30, 1.3f, "Spread")
            .DeactivateOnExit<NothingBesideRemains>();
        ComponentCondition<RackAndRuin>(id + 0x31, 0.7f, comp => comp.Casters.Count == 0, "Lines 2")
            .DeactivateOnExit<RackAndRuin>();
    }

    private void HatefulVisage(uint id, float delay)
    {
        Cast(id, _savage ? AID.SHatefulVisage : AID.NHatefulVisage, delay, 3);
        WrathOfRuin(id + 0x100, 2.2f);
    }

    private void AccursedVisage(uint id, float delay)
    {
        Cast(id, _savage ? AID.SAccursedVisage : AID.NAccursedVisage, delay, 3);
        // +1.1s: gilded/silvered fate statuses
        WrathOfRuin(id + 0x100, 2.2f);
    }

    // TODO: component for tether?..
    private void CurseOfTheMonument(uint id, float delay)
    {
        Cast(id, _savage ? AID.SCurseOfTheMonument : AID.NCurseOfTheMonument, delay, 4);
        // +1.0s: debuffs/tethers appear

        // after central cast, other pairs are staggered by ~0.6s
        ComponentCondition<SunderedRemains>(id + 0x10, 1.2f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<NSunderedRemains>(!_savage)
            .ActivateOnEnter<SSunderedRemains>(_savage);
        ComponentCondition<SunderedRemains>(id + 0x11, 8.4f, comp => comp.Casters.Count == 0, "Last aoe")
            .DeactivateOnExit<SunderedRemains>();

        // note: spread explosions happen ~0.8s before tower explosions...
        Cast(id + 0x100, _savage ? AID.SColossalWreck : AID.NColossalWreck, 4.9f, 6)
            .ActivateOnEnter<ScreamOfTheFallen>();
        ComponentCondition<ScreamOfTheFallen>(id + 0x110, 0.5f, comp => comp.NumCasts > 0, "Towers 1");
        ComponentCondition<ScreamOfTheFallen>(id + 0x120, 4, comp => comp.NumCasts > 2, "Towers 2")
            .DeactivateOnExit<ScreamOfTheFallen>();
    }
}

class C012NGladiatorStates : C012GladiatorStates { public C012NGladiatorStates(BossModule module) : base(module, false) { } }
class C012SGladiatorStates : C012GladiatorStates { public C012SGladiatorStates(BossModule module) : base(module, true) { } }
