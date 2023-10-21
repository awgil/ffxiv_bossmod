using System.Linq;

namespace BossMod.RealmReborn.Extreme.Ex3Titan
{
    class Ex3TitanStates : StateMachineBuilder
    {
        Ex3Titan _module;

        public Ex3TitanStates(Ex3Titan module) : base(module)
        {
            _module = module;
            SimplePhase(0, Phase1, "Phase 1")
                .ActivateOnEnter<LandslideBurst>()
                .ActivateOnEnter<WeightOfTheLand>() // note that these have to be activated before AI
                .ActivateOnEnter<Ex3TitanAI>()
                .Raw.Update = () => module.PrimaryActor.IsDestroyed || !module.PrimaryActor.IsTargetable; // lasts until first untargetable
            SimplePhase(1, Intermission1, "Intermission 1")
                .Raw.Update = () => module.PrimaryActor.IsDestroyed || module.PrimaryActor.IsTargetable;
            SimplePhase(2, Phase2, "Phase 2")
                .ActivateOnEnter<LandslideBurst>()
                .ActivateOnEnter<WeightOfTheLand>()
                .ActivateOnEnter<GraniteGaol>()
                .ActivateOnEnter<Upheaval>()
                .ActivateOnEnter<Ex3TitanAI>()
                .Raw.Update = () => module.PrimaryActor.IsDestroyed || !module.PrimaryActor.IsTargetable; // lasts until second untargetable
            SimplePhase(3, Intermission2, "Intermission 2")
                .Raw.Update = () => module.PrimaryActor.IsDestroyed || (module.Heart()?.IsTargetable ?? false);
            SimplePhase(4, Phase3, "Phase 3")
                .ActivateOnEnter<LandslideBurst>()
                .ActivateOnEnter<WeightOfTheLand>()
                .ActivateOnEnter<GraniteGaol>()
                .ActivateOnEnter<Upheaval>()
                .ActivateOnEnter<Ex3TitanAI>()
                .Raw.Update = () => module.PrimaryActor.IsDestroyed || !(module.Heart()?.IsTargetable ?? false);
            SimplePhase(5, Intermission3, "Intermission 3")
                .Raw.Update = () => module.PrimaryActor.IsDestroyed || module.PrimaryActor.IsTargetable;
            DeathPhase(6, Phase4)
                .ActivateOnEnter<LandslideBurst>()
                .ActivateOnEnter<WeightOfTheLand>()
                .ActivateOnEnter<GraniteGaol>()
                .ActivateOnEnter<Upheaval>()
                .ActivateOnEnter<Ex3TitanAI>()
                .ActivateOnEnter<GaolerVoidzone>(); // note: activated after AI, so that it doesn't fuck up positioning
        }

        private void Phase1(uint id)
        {
            Phase1Repeat(id, 7.1f);
            Phase1Repeat(id + 0x100000, 4.1f);
            Phase1Repeat(id + 0x200000, 4.1f);
            SimpleState(id + 0xFF0000, 1000, "???");
        }

        private void Phase1Repeat(uint id, float delay)
        {
            Landslide(id, delay);
            WeightOfTheLand(id + 0x10000, 6.1f);
            MountainBuster(id + 0x20000, 3.6f);
            Tumult(id + 0x30000, 5.1f);
            Landslide(id + 0x40000, 4.1f);
            MountainBuster(id + 0x50000, 4.1f);
            WeightOfTheLand(id + 0x60000, 4.1f);
            MountainBuster(id + 0x70000, 7.6f);
        }

        private void Intermission1(uint id)
        {
            Geocrush<Geocrush1>(id, 0, Geocrush1.Radius);
        }

        private void Phase2(uint id)
        {
            Landslide(id, 7.1f);
            Phase2Repeat(id + 0x100000, 4.1f, false);
            Phase2Repeat(id + 0x200000, 4.1f, true);
            Phase2Repeat(id + 0x300000, 4.1f, false);
            Phase2Repeat(id + 0x400000, 4.1f, true);
            SimpleState(id + 0xFF0000, 1000, "???");
        }

        private void Phase2Repeat(uint id, float delay, bool even)
        {
            GaolsNonHeart(id, delay);
            Tumult(id + 0x10000, 6.2f);
            WeightOfTheLand(id + 0x20000, 3.1f);
            MountainBuster(id + 0x30000, 3.6f);
            Landslide(id + 0x40000, 5.1f);
            WeightOfTheLand(id + 0x50000, 7.1f);
            BombsLandslidePhase2(id + 0x60000, 6.4f, even);
        }

        private void Intermission2(uint id)
        {
            Geocrush<Geocrush2>(id, 0, Geocrush2.Radius);
            Targetable(id + 0x100, false, 2.1f, "Disappear");
            ActorTargetable(id + 0x101, _module.Heart, true, 0.2f, "Heart appears")
                .SetHint(StateMachine.StateHint.DowntimeEnd);
        }

        private void Phase3(uint id)
        {
            WeightOfTheLand(id, 7.5f);
            GaolsHeart(id + 0x10000, 5.5f);
            Tumult(id + 0x20000, 6.1f); // TODO: timing?..
            WeightOfTheLand(id + 0x30000, 3.1f);
            RockBuster(id + 0x40000, 8.6f);
            BombsLandslidePhase3(id + 0x50000, 6.7f);
            Tumult(id + 0x60000, 4.2f);
            WeightOfTheLand(id + 0x70000, 3.3f);
            ActorTargetable(id + 0x80000, _module.Heart, false, 9.5f, "Heart enrage");
        }

        private void Intermission3(uint id)
        {
            Targetable(id, true, 5.3f, "Reappear + raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void Phase4(uint id)
        {
            Phase4Repeat(id, 12.9f);
            Phase4Repeat(id + 0x100000, 11.2f);
            Phase4Repeat(id + 0x200000, 11.2f);
            Phase4Repeat(id + 0x300000, 11.2f);
            Phase4Repeat(id + 0x400000, 11.2f);
            SimpleState(id + 0xFF0000, 1000, "???");
        }

        private void Phase4Repeat(uint id, float delay)
        {
            Condition(id, delay, () => _module.Gaolers.Any(a => a.IsTargetable), "Adds");
            MountainBuster(id + 0x10000, 3.6f);
            BombsLandslidePhase4Pattern1(id + 0x20000, 11.8f);
            MountainBuster(id + 0x30000, 3.0f);
            WeightOfTheLand(id + 0x40000, 6.1f);
            GaolsNonHeart(id + 0x50000, 5.7f);
            MountainBuster(id + 0x60000, 10.1f); // TODO: timings
            Tumult(id + 0x70000, 4.1f);
            WeightOfTheLand(id + 0x80000, 3.1f, true);
            MountainBuster(id + 0x90000, 5.3f);
            BombsLandslidePhase4Pattern2(id + 0xA0000, 8.8f);
            MountainBuster(id + 0xB0000, 6.0f);
            BombsTumultWeightOfTheLand(id + 0xC0000, 13.6f);
            Geocrush<Geocrush2>(id + 0xD0000, 4.6f);
            Landslide(id + 0xE0000, 3.1f);
        }

        private void Landslide(uint id, float delay)
        {
            Cast(id, AID.LandslideBoss, delay, 2.2f, "Landslide");
        }

        private void WeightOfTheLand(uint id, float delay, bool twice = false)
        {
            Cast(id, AID.WeightOfTheLand, delay, 2);
            ComponentCondition<WeightOfTheLand>(id + 2, twice ? 3 : 0.5f, comp => comp.Casters.Count == 0, "Puddles", 1, twice ? 1 : 0);
        }

        private State MountainBuster(uint id, float delay)
        {
            return ComponentCondition<MountainBuster>(id, delay, comp => comp.NumCasts > 0, "Tankbuster", 3)
                .ActivateOnEnter<MountainBuster>()
                .DeactivateOnExit<MountainBuster>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void RockBuster(uint id, float delay)
        {
            ComponentCondition<RockBuster>(id, delay, comp => comp.NumCasts > 0, "Tankbuster")
                .ActivateOnEnter<RockBuster>()
                .DeactivateOnExit<RockBuster>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void Tumult(uint id, float delay)
        {
            ComponentCondition<Tumult>(id, delay, comp => comp.NumCasts > 0, "Raidwide first", 10)
                .ActivateOnEnter<Tumult>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<Tumult>(id + 1, 1.2f, comp => comp.NumCasts > 1)
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<Tumult>(id + 2, 1.2f, comp => comp.NumCasts > 2)
                .SetHint(StateMachine.StateHint.Raidwide);
            ComponentCondition<Tumult>(id + 3, 1.2f, comp => comp.NumCasts > 3, "Raidwide last")
                .DeactivateOnExit<Tumult>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void Geocrush<Crush>(uint id, float delay, float newRadius = 0) where Crush : Geocrush, new()
        {
            Targetable(id, false, delay, "Disappear")
                .ActivateOnEnter<Crush>();
            Cast(id + 0x10, AID.Geocrush, 3.3f, 3, "Geocrush")
                .DeactivateOnExit<Crush>()
                .OnExit(() => Module.Arena.Bounds = new ArenaBoundsCircle(Module.Bounds.Center, newRadius), newRadius > 0);
            Targetable(id + 0x20, true, 0.4f, "Reappear");
        }

        private void GaolsNonHeart(uint id, float delay)
        {
            ComponentCondition<GraniteGaol>(id, delay, comp => comp.PendingFetters.Any(), "Gaol markers");
            // +2.9s: fetters
            // +4.2s: gaols create
            // +5.0s: gaols targetable
            // +6.5s: granite sepulchre cast start
            MountainBuster(id + 0x100, 4.1f);
            Cast(id + 0x200, AID.Upheaval, 4.1f, 5, "Knockback");
            Landslide(id + 0x300, 2.8f); // note: this gets skipped entirely if there is no one alive and unfettered, e.g. when doing solo
            // granite sepulchre cast ends during next mechanic, but realistically they should be killed earlier
        }

        private void GaolsHeart(uint id, float delay)
        {
            ComponentCondition<GraniteGaol>(id, delay, comp => comp.PendingFetters.Any(), "Gaol markers");
            // +2.9s: fetters
            // +4.2s: gaols create
            // +5.0s: gaols targetable
            // +6.5s: granite sepulchre cast start
            RockBuster(id + 0x100, 5.5f);
            Cast(id + 0x200, AID.Upheaval, 2.0f, 5, "Knockback");
            Landslide(id + 0x300, 2.8f); // note: this gets skipped entirely if there is no one alive and unfettered, e.g. when doing solo
            // granite sepulchre cast ends during next mechanic, but realistically they should be killed earlier
        }

        private void BombsLandslidePhase2(uint id, float delay, bool even)
        {
            // note: bombs are created and become targetable slightly before MB (odd are created ~2.2s earlier and become targetable ~0.9s later)
            // odd pattern: 8 bombs on one side, landslide begins ~3.8s cast into burst and resolves after => we show all bombs asap and dodge them + landslides normally
            // even pattern: 8 staggered bombs in clockwise order followed by bomb in center, ~0.4s between successive explosions, landslide begins right after second explosion and ends around 8th explosion => we show next 5 bombs
            MountainBuster(id, delay)
                .ExecOnEnter<LandslideBurst>(comp => comp.MaxBombs = 0) // don't show anything until TB resolves
                .ExecOnExit<LandslideBurst>(comp => comp.MaxBombs = even ? 5 : 8);
            Cast(id + 0x100, AID.LandslideBoss, 8.1f, 2.2f, "Bombs & Landslide")
                .ExecOnExit<LandslideBurst>(comp => comp.MaxBombs = 9); // reset to default, show any remaining bombs
        }

        private void BombsLandslidePhase3(uint id, float delay)
        {
            // note: first bomb is created ~5.5s before landslide start; first burst starts ~0.6s before landslide start
            // pattern: 6 staggered bombs in clockwise order followed by bomb in center, ~0.4s between successive explosions, landslide begins after second burst start and resolves ~2.2s before first burst => we show next 5 bombs
            CastStart(id + 0x100, AID.LandslideBoss, delay)
                .ExecOnEnter<LandslideBurst>(comp => comp.MaxBombs = 0) // don't show anything until landslide cast starts
                .ExecOnExit<LandslideBurst>(comp => comp.MaxBombs = 5); // show next 5 bombs
            CastEnd(id + 0x101, 2.2f, "Landslide + Bombs");
        }

        private void BombsLandslidePhase4Pattern1(uint id, float delay)
        {
            // note: first set of bombs are created ~5.3s before landslide start; first burst starts ~0.6s before landslide start
            // pattern: 4 bombs on cardinals, ~2.5s later 5 bombs on intercardinals and in center; we show first set when landslide starts, then second set after first is done
            CastStart(id + 0x100, AID.LandslideBoss, delay)
                .ExecOnEnter<LandslideBurst>(comp => comp.MaxBombs = 0) // don't show anything until landslide cast starts
                .ExecOnExit<LandslideBurst>(comp => comp.MaxBombs = 4); // show first set
            CastEnd(id + 0x101, 2.2f, "Landslide");
            ComponentCondition<LandslideBurst>(id + 0x200, 2.2f, comp => comp.NumActiveBursts <= 5, "First bombs")
                .ExecOnExit<LandslideBurst>(comp => comp.MaxBombs = 9); // reset to default, show any remaining bombs
        }

        private void BombsLandslidePhase4Pattern2(uint id, float delay)
        {
            // note: first set of bombs are created ~5.3s before landslide start; first burst starts ~0.6s before landslide start
            // pattern: 3 staggered lines of 3 bombs, ~1.5s between explosions; we show next two sets after landslide is done
            CastStart(id + 0x100, AID.LandslideBoss, delay)
                .ExecOnEnter<LandslideBurst>(comp => comp.MaxBombs = 0); // don't show anything until landslide cast ends
            CastEnd(id + 0x101, 2.2f, "Landslide")
                .ExecOnExit<LandslideBurst>(comp => comp.MaxBombs = 6); // show two sets
            ComponentCondition<LandslideBurst>(id + 0x200, 0.2f, comp => comp.NumActiveBursts > 6);
            ComponentCondition<LandslideBurst>(id + 0x201, 2.0f, comp => comp.NumActiveBursts <= 6, "First bombs")
                .ExecOnExit<LandslideBurst>(comp => comp.MaxBombs = 9); // reset to default, show any remaining bombs
        }

        private void BombsTumultWeightOfTheLand(uint id, float delay)
        {
            Condition(id, delay, () => _module.Bombs.Any(a => a.IsTargetable), "Bombs to kill")
                .ExecOnExit<Ex3TitanAI>(comp => comp.KillNextBomb = true);
            Tumult(id + 0x100, 4.2f);
            WeightOfTheLand(id + 0x200, 3.1f);
            // +4.3s: explosions start
        }
    }
}
