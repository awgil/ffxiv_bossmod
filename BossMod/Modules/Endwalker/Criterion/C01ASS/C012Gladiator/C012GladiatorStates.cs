namespace BossMod.Endwalker.Criterion.C01ASS.C012Gladiator
{
    class C012GladiatorStates : StateMachineBuilder
    {
        public C012GladiatorStates(BossModule module) : base(module)
        {
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
            Cast(id, AID.FlashOfSteel, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void MightySmite(uint id, float delay)
        {
            Cast(id, AID.MightySmite, delay, 5, "Tankbuster")
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void SpecterOfMight(uint id, float delay)
        {
            Cast(id, AID.SpecterOfMight, delay, 4);
            ComponentCondition<RushOfMightFront>(id + 0x10, 4.2f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<RushOfMightFront>();
            ComponentCondition<RushOfMightFront>(id + 0x11, 10.5f, comp => comp.NumCasts > 0, "Charge 1 front")
                .DeactivateOnExit<RushOfMightFront>();

            CastStart(id + 0x20, AID.SpecterOfMight, 0.5f)
                .ActivateOnEnter<RushOfMightBack>();
            ComponentCondition<RushOfMightBack>(id + 0x21, 1.5f, comp => comp.NumCasts > 0, "Charge 1 back")
                .DeactivateOnExit<RushOfMightBack>();
            CastEnd(id + 0x22, 2.5f);
            ComponentCondition<RushOfMightFront>(id + 0x30, 4.2f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<RushOfMightFront>();
            ComponentCondition<RushOfMightFront>(id + 0x31, 10.5f, comp => comp.NumCasts > 0, "Charge 2 front")
                .DeactivateOnExit<RushOfMightFront>();
            ComponentCondition<RushOfMightBack>(id + 0x40, 2, comp => comp.NumCasts > 0, "Charge 2 back")
                .ActivateOnEnter<RushOfMightBack>()
                .DeactivateOnExit<RushOfMightBack>();
        }

        private void SculptorsPassion(uint id, float delay)
        {
            CastStart(id, AID.SculptorsPassion, delay)
                .ActivateOnEnter<SculptorsPassion>();
            CastEnd(id + 1, 5);
            ComponentCondition<SculptorsPassion>(id + 2, 0.3f, comp => comp.NumCasts > 0, "Wild charge")
                .DeactivateOnExit<SculptorsPassion>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void CurseOfTheFallen(uint id, float delay)
        {
            Cast(id, AID.CurseOfTheFallen, delay, 5);
            ComponentCondition<CurseOfTheFallen>(id + 2, 1.1f, comp => comp.Active)
                .ActivateOnEnter<CurseOfTheFallen>();

            CastMulti(id + 0x10, new AID[] { AID.RingOfMight1Out, AID.RingOfMight2Out, AID.RingOfMight3Out }, 3.7f, 10, "Out")
                .ActivateOnEnter<RingOfMight1Out>()
                .ActivateOnEnter<RingOfMight2Out>()
                .ActivateOnEnter<RingOfMight3Out>()
                .DeactivateOnExit<RingOfMight1Out>()
                .DeactivateOnExit<RingOfMight2Out>()
                .DeactivateOnExit<RingOfMight3Out>()
                .SetHint(StateMachine.StateHint.Raidwide); // first debuff resolve ~0.2s later


            Condition(id + 0x20, 2, () => Module.FindComponent<RingOfMight1In>()!.NumCasts + Module.FindComponent<RingOfMight2In>()!.NumCasts + Module.FindComponent<RingOfMight3In>()!.NumCasts > 0, "In")
                .ActivateOnEnter<RingOfMight1In>()
                .ActivateOnEnter<RingOfMight2In>()
                .ActivateOnEnter<RingOfMight3In>()
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
            Cast(id, AID.WrathOfRuin, delay, 3)
                .ActivateOnEnter<GoldenSilverFlame>(); // casts start ~2.1s after cast-start
            // +1.4s: first set of Regrets spawn
            // +3.4s: second set of Regrets spawn
            // +5.4s: first set of RackAndRuin cast-starts
            CastStart(id + 0x10, AID.NothingBesideRemains, 5.7f)
                .ActivateOnEnter<RackAndRuin>();
            // +1.7s: second set of RackAndRuin cast-starts
            ComponentCondition<GoldenSilverFlame>(id + 0x20, 3.4f, comp => !comp.Active, "Cells")
                .ActivateOnEnter<NothingBesideRemains>()
                .DeactivateOnExit<GoldenSilverFlame>();
            ComponentCondition<RackAndRuin>(id + 0x21, 0.3f, comp => comp.NumCasts > 0, "Lines 1");
            CastEnd(id + 0x30, 1.3f, "Spread")
                .DeactivateOnExit<NothingBesideRemains>();
            ComponentCondition<RackAndRuin>(id + 0x31, 0.7f, comp => comp.Casters.Count == 0, "Lines 2")
                .DeactivateOnExit<RackAndRuin>();
        }

        private void HatefulVisage(uint id, float delay)
        {
            Cast(id, AID.HatefulVisage, delay, 3);
            WrathOfRuin(id + 0x100, 2.2f);
        }

        private void AccursedVisage(uint id, float delay)
        {
            Cast(id, AID.AccursedVisage, delay, 3);
            // +1.1s: gilded/silvered fate statuses
            WrathOfRuin(id + 0x100, 2.2f);
        }

        // TODO: component for tether?..
        private void CurseOfTheMonument(uint id, float delay)
        {
            Cast(id, AID.CurseOfTheMonument, delay, 4);
            // +1.0s: debuffs/tethers appear

            // after central cast, other pairs are staggered by ~0.6s
            ComponentCondition<SunderedRemains>(id + 0x10, 1.2f, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<SunderedRemains>();
            ComponentCondition<SunderedRemains>(id + 0x11, 8.4f, comp => comp.Casters.Count == 0, "Last aoe")
                .DeactivateOnExit<SunderedRemains>();

            // note: spread explosions happen ~0.8s before tower explosions...
            Cast(id + 0x100, AID.ColossalWreck, 4.9f, 6)
                .ActivateOnEnter<ScreamOfTheFallen>();
            ComponentCondition<ScreamOfTheFallen>(id + 0x110, 0.5f, comp => comp.NumCasts > 0, "Towers 1");
            ComponentCondition<ScreamOfTheFallen>(id + 0x120, 4, comp => comp.NumCasts > 2, "Towers 2")
                .DeactivateOnExit<ScreamOfTheFallen>();
        }
    }
}
