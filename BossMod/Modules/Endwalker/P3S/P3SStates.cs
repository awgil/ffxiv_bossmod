using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.P3S
{
    class P3SStates : StateMachineBuilder
    {
        public P3SStates(BossModule module) : base(module)
        {
            ScorchedExaltation(0x00000000, 8.1f);
            HeatOfCondemnation(0x00010000, 3.2f);
            FireplumeCinderwing(0x00020000, 5.1f);
            DarkenedFire(0x00030000, 8.2f);
            HeatOfCondemnation(0x00040000, 6.6f);
            ScorchedExaltation(0x00050000, 2.1f);
            DevouringBrandFireplumeBreezeCinderwing(0x00060000, 7.2f);
            HeatOfCondemnation(0x00070000, 3.2f);

            FlyAwayBirds(0x00100000, 2.1f);

            DeadRebirth(0x00200000, 9.2f);
            HeatOfCondemnation(0x00210000, 9.2f);
            FledglingFlight(0x00220000, 7.1f);
            GloryplumeMulti(0x00230000, 8);
            FountainOfFire(0x00240000, 12.1f);

            ScorchedExaltation(0x00300000, 2.1f);
            ScorchedExaltation(0x00310000, 2.1f);
            HeatOfCondemnation(0x00320000, 5.2f);
            FirestormsOfAsphodelos(0x00330000, 8.6f);
            ConesAshplume(0x00340000, 3.2f);
            ConesStorms(0x00350000, 2.1f);
            DarkblazeTwister(0x00360000, 2.2f);
            ScorchedExaltation(0x00370000, 2.1f);
            DeathToll(0x00380000, 7.2f);

            GloryplumeSingle(0x00400000, 7.3f);
            FlyAwayNoBirds(0x00410000, 3);
            DevouringBrandFireplumeBreezeCinderwing(0x00420000, 5.1f);
            ScorchedExaltation(0x00430000, 6.2f);
            ScorchedExaltation(0x00440000, 2.2f);
            Cast(0x00450000, AID.FinalExaltation, 2.1f, 10, "Enrage");
        }

        private void ScorchedExaltation(uint id, float delay)
        {
            var s = Cast(id, AID.ScorchedExaltation, delay, 5, "AOE");
            s.EndHint |= StateMachine.StateHint.Raidwide;
        }

        private void DeadRebirth(uint id, float delay)
        {
            var s = Cast(id, AID.DeadRebirth, delay, 10, "DeadRebirth");
            s.EndHint |= StateMachine.StateHint.Raidwide;
        }

        private void FirestormsOfAsphodelos(uint id, float delay)
        {
            var s = Cast(id, AID.FirestormsOfAsphodelos, delay, 5, "Firestorm");
            s.EndHint |= StateMachine.StateHint.Raidwide;
        }

        private void HeatOfCondemnation(uint id, float delay)
        {
            var cast = Cast(id, AID.HeatOfCondemnation, delay, 6);
            cast.Enter.Add(Module.ActivateComponent<HeatOfCondemnation>);

            var resolve = ComponentCondition<HeatOfCondemnation>(id + 2, 1.1f, comp => comp.NumCasts > 0, "Tether");
            resolve.Exit.Add(Module.DeactivateComponent<HeatOfCondemnation>);
            resolve.EndHint |= StateMachine.StateHint.Tankbuster;
        }

        // note - activates component, which should be later deactivated manually
        // note - positioning state is set at the end, make sure to clear later - this is because this mechanic overlaps with other stuff
        private StateMachine.State Fireplume(uint id, float delay)
        {
            // mechanics:
            // 1. single-plume version: immediately after cast end, 1 helper teleports to position and starts casting 26303, which takes 6s
            // 2. multi-plume version: immediately after cast end, 9 helpers teleport to positions and start casting 26305
            //    first pair starts cast almost immediately, then pairs 2-4 and finally central start their cast with 1 sec between them; each cast lasts 2 sec
            // so center (last/only) plume hits around 6s after cast end
            // note that our helpers rely on 233C casts rather than states
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            var start = CastStartMulti(id, new AID[] { AID.ExperimentalFireplumeSingle, AID.ExperimentalFireplumeMulti }, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CastEnd(id + 1, 5, "Fireplume");
            end.Exit.Add(Module.ActivateComponent<Fireplume>);
            return end;
        }

        // note - no positioning flags, since this is part of mechanics that manage it themselves
        // note - since it resolves in a complex way, make sure to add a resolve state!
        private void AshplumeCast(uint id, float delay)
        {
            var start = CastStartMulti(id, new AID[] { AID.ExperimentalAshplumeStack, AID.ExperimentalAshplumeSpread }, delay);
            start.Exit.Add(Module.ActivateComponent<Ashplume>);

            CastEnd(id + 1, 5, "Ashplume");
        }

        private StateMachine.State AshplumeResolve(uint id, float delay)
        {
            var resolve = ComponentCondition<Ashplume>(id, delay, comp => comp.CurState == Ashplume.State.Done, "Ashplume resolve");
            resolve.Exit.Add(Module.DeactivateComponent<Ashplume>);
            resolve.EndHint |= StateMachine.StateHint.Raidwide;
            return resolve;
        }

        private void GloryplumeMulti(uint id, float delay)
        {
            // first part for this mechanic always seems to be "multi-plume", works just like fireplume
            // 9 helpers teleport to position, first pair almost immediately starts casting 26315s, 1 sec stagger between pairs, 7 sec for each cast
            // ~3 sec after cast ends, boss makes an instant cast that determines stack/spread (26316/26312), ~10 sec after that hits with real AOE (26317/26313)
            var cast = Cast(id, AID.ExperimentalGloryplumeMulti, delay, 5, "GloryplumeMulti");
            cast.Enter.Add(Module.ActivateComponent<Ashplume>);
            cast.Exit.Add(Module.ActivateComponent<Fireplume>);
            cast.EndHint |= StateMachine.StateHint.PositioningStart;

            var resolve = ComponentCondition<Ashplume>(id + 0x10, 13.2f, comp => comp.CurState == Ashplume.State.Done, "Gloryplume resolve");
            resolve.Exit.Add(Module.DeactivateComponent<Fireplume>);
            resolve.Exit.Add(Module.DeactivateComponent<Ashplume>);
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
        }

        private void GloryplumeSingle(uint id, float delay)
        {
            // first part for this mechanic always seems to be "single-plume", works just like fireplume
            // helper teleports to position, almost immediately starts casting 26311, 6 sec for cast
            // ~3 sec after cast ends, boss makes an instant cast that determines stack/spread (26316/26312), ~4 sec after that hits with real AOE (26317/26313)
            // note that our helpers rely on casts rather than states
            var cast = Cast(id, AID.ExperimentalGloryplumeSingle, delay, 5, "GloryplumeSingle");
            cast.Enter.Add(Module.ActivateComponent<Ashplume>);
            cast.Exit.Add(Module.ActivateComponent<Fireplume>);
            cast.EndHint |= StateMachine.StateHint.PositioningStart;

            var resolve = ComponentCondition<Ashplume>(id + 0x10, 7.2f, comp => comp.CurState == Ashplume.State.Done, "Gloryplume resolve");
            resolve.Exit.Add(Module.DeactivateComponent<Fireplume>);
            resolve.Exit.Add(Module.DeactivateComponent<Ashplume>);
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
        }

        private StateMachine.State Cinderwing(uint id, float delay)
        {
            var start = CastStartMulti(id, new AID[] { AID.RightCinderwing, AID.LeftCinderwing }, delay);
            start.Exit.Add(Module.ActivateComponent<Cinderwing>);

            var end = CastEnd(id + 1, 5, "Wing");
            end.Exit.Add(Module.DeactivateComponent<Cinderwing>);
            return end;
        }

        private void FireplumeCinderwing(uint id, float delay)
        {
            Fireplume(id, delay); // pos-start

            var cinderwing = Cinderwing(id + 0x1000, 5.7f);
            cinderwing.Exit.Add(Module.DeactivateComponent<Fireplume>);
            cinderwing.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private void DevouringBrandFireplumeBreezeCinderwing(uint id, float delay)
        {
            Cast(id, AID.DevouringBrand, delay, 3, "DevouringBrand");

            var fireplume = Fireplume(id + 0x1000, 2.1f); // pos-start
            fireplume.Exit.Add(Module.ActivateComponent<DevouringBrand>);

            var breeze = Cast(id + 0x2000, AID.SearingBreeze, 7.2f, 3, "SearingBreeze");
            breeze.Enter.Add(Module.DeactivateComponent<Fireplume>);

            var wing = Cinderwing(id + 0x3000, 3.2f);
            wing.Enter.Add(Module.DeactivateComponent<DevouringBrand>);
            wing.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private void DarkenedFire(uint id, float delay)
        {
            // 3s after cast ends, adds start casting 26299
            var addsStart = CastStart(id, AID.DarkenedFire, delay);
            addsStart.Exit.Add(Module.ActivateComponent<DarkenedFire>);
            addsStart.EndHint |= StateMachine.StateHint.PositioningStart;

            var addsEnd = CastEnd(id + 0x1000, 6, "DarkenedFire adds");
            addsEnd.Exit.Add(Module.DeactivateComponent<DarkenedFire>);
            addsEnd.Exit.Add(Module.ActivateComponent<BrightenedFire>); // icons appear just before next cast start

            Cast(id + 0x2000, AID.BrightenedFire, 5.2f, 5, "Numbers"); // numbers appear at the beginning of the cast, at the end he starts shooting 1-8

            var lastAOE = ComponentCondition<BrightenedFire>(id + 0x3000, 8.4f, comp => comp.NumCasts == 8);
            lastAOE.Exit.Add(Module.DeactivateComponent<BrightenedFire>);

            var resolve = Timeout(id + 0x4000, 6.6f, "DarkenedFire resolve");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private StateMachine.State TrailOfCondemnation(uint id, float delay)
        {
            // at this point boss teleports to one of the cardinals
            // parallel to this one of the helpers casts 26365 (actual aoe fire trails)
            var start = CastStartMulti(id, new AID[] { AID.TrailOfCondemnationCenter, AID.TrailOfCondemnationSides }, delay);
            start.Exit.Add(Module.ActivateComponent<TrailOfCondemnation>);

            CastEnd(id + 1, 6);

            var resolve = ComponentCondition<TrailOfCondemnation>(id + 2, 1.5f, comp => comp.Done, "Trail");
            resolve.Exit.Add(Module.DeactivateComponent<TrailOfCondemnation>);
            return resolve;
        }

        // note: expects downtime at enter, clears when birds spawn, reset when birds die
        private void SmallBirdsPhase(uint id, float delay)
        {
            var birds = Module.Enemies(OID.SunbirdSmall);

            var spawn = Condition(id, delay, () => birds.Count > 0, "Small birds", 10000);
            spawn.Exit.Add(Module.ActivateComponent<SmallBirdDistance>);
            spawn.EndHint |= StateMachine.StateHint.DowntimeEnd; // adds become targetable <1sec after spawn

            var enrage = Condition(id + 0x10, 25, () => birds.Find(x => !x.IsDead) == null, "Small birds enrage", 10000);
            enrage.Exit.Add(Module.DeactivateComponent<BirdDistance>);
            enrage.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart; // raidwide (26326) happens ~3sec after last bird death
        }

        // note: expects downtime at enter, clears when birds spawn, reset when birds die
        private void LargeBirdsPhase(uint id, float delay)
        {
            var birds = Module.Enemies(OID.SunbirdLarge);

            var spawn = Condition(id, delay, () => birds.Count > 0, "Large birds", 10000);
            spawn.Exit.Add(Module.ActivateComponent<BirdTether>); // note that first tethers appear ~5s after this
            spawn.EndHint |= StateMachine.StateHint.DowntimeEnd; // adds become targetable ~1sec after spawn

            var chargesDone = ComponentCondition<BirdTether>(id + 0x1000, 18.2f, comp => comp.NumFinishedChains == 4, "", 10000);
            chargesDone.Exit.Add(Module.DeactivateComponent<BirdTether>);
            chargesDone.Exit.Add(Module.ActivateComponent<LargeBirdDistance>);

            // note that enrage is ~55sec after spawn
            var enrage = Condition(id + 0x2000, 36.8f, () => birds.Find(x => !x.IsDead) == null, "Large birds enrage", 10000);
            enrage.Exit.Add(Module.DeactivateComponent<BirdDistance>);
            enrage.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart; // raidwide (26326) happens ~3sec after last bird death
        }

        private void FlyAwayBirds(uint id, float delay)
        {
            Fireplume(id, delay); // pos-start

            Targetable(id + 0x10000, false, 4.6f, "Boss disappears");

            var trail = TrailOfCondemnation(id + 0x20000, 3.8f);
            trail.Enter.Add(Module.DeactivateComponent<Fireplume>);

            SmallBirdsPhase(id + 0x30000, 6.5f);
            LargeBirdsPhase(id + 0x40000, 3.4f);

            var reappear = Targetable(id + 0x50000, true, 5.2f, "Boss reappears");
            reappear.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private void FlyAwayNoBirds(uint id, float delay)
        {
            Targetable(id, false, delay, "Boss disappears");
            TrailOfCondemnation(id + 0x1000, 3.8f);
            Targetable(id + 0x2000, true, 3.1f, "Boss reappears");
        }

        private void FledglingFlight(uint id, float delay)
        {
            // mechanic timeline:
            // 0s cast end
            // 2s icons appear
            // 8s 3540's teleport to players
            // 10s 3540's start casting 26342
            // 14s 3540's finish casting 26342
            // note that helper does relies on icons and cast events rather than states
            var cast = Cast(id, AID.FledglingFlight, delay, 3);
            cast.Exit.Add(Module.ActivateComponent<FledglingFlight>);

            ComponentCondition<FledglingFlight>(id + 0x10, 10.3f, comp => comp.PlacementDone, "Eyes place");

            var resolve = ComponentCondition<FledglingFlight>(id + 0x20, 4, comp => comp.CastsDone, "Eyes resolve");
            resolve.Exit.Add(Module.DeactivateComponent<FledglingFlight>);
        }

        private void DeathToll(uint id, float delay)
        {
            // notes on mechanics:
            // - on 26349 cast end, debuffs with 25sec appear
            // - 12-15sec after 26350 cast starts, eyes finish casting their cones - at this point, there's about 5sec left on debuffs
            Cast(id, AID.DeathToll, delay, 6, "DeathToll");

            var eyes = Cast(id + 0x1000, AID.FledglingFlight, 3.2f, 3, "Eyes");
            eyes.Exit.Add(Module.ActivateComponent<FledglingFlight>);

            var agonies = Cast(id + 0x2000, AID.LifesAgonies, 2.1f, 24, "LifeAgonies");
            agonies.Exit.Add(Module.DeactivateComponent<FledglingFlight>);
        }

        private void FountainOfFire(uint id, float delay)
        {
            // TODO: healer component - not even sure, mechanic looks so simple...
            var fountain = Cast(id, AID.FountainOfFire, delay, 6, "FountainOfFire");
            fountain.EndHint |= StateMachine.StateHint.PositioningStart;

            var pinion = Cast(id + 0x1000, AID.SunsPinion, 2.1f, 6, "First birds");
            pinion.Exit.Add(Module.ActivateComponent<SunshadowTether>);

            var charges = ComponentCondition<SunshadowTether>(id + 0x2000, 16.1f, comp => comp.NumCharges == 6, "Charges");
            charges.Exit.Add(Module.DeactivateComponent<SunshadowTether>);
            charges.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private void ConesAshplume(uint id, float delay)
        {
            var flames = Cast(id, AID.FlamesOfAsphodelos, delay, 3, "Cones");
            flames.Exit.Add(Module.ActivateComponent<FlamesOfAsphodelos>);
            flames.EndHint |= StateMachine.StateHint.PositioningStart;

            AshplumeCast(id + 0x1000, 2.1f);

            var resolve = AshplumeResolve(id + 0x2000, 6.1f);
            resolve.Exit.Add(Module.DeactivateComponent<FlamesOfAsphodelos>);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private void ConesStorms(uint id, float delay)
        {
            var flames = Cast(id, AID.FlamesOfAsphodelos, delay, 3, "Cones");
            flames.Exit.Add(Module.ActivateComponent<FlamesOfAsphodelos>);
            flames.EndHint |= StateMachine.StateHint.PositioningStart;

            var state = Cast(id + 0x1000, AID.StormsOfAsphodelos, 10.2f, 8, "Storms");
            state.Enter.Add(Module.DeactivateComponent<FlamesOfAsphodelos>);
            state.Enter.Add(Module.ActivateComponent<StormsOfAsphodelos>);
            state.Exit.Add(Module.DeactivateComponent<StormsOfAsphodelos>);
            state.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.Tankbuster | StateMachine.StateHint.PositioningEnd;
        }

        private void DarkblazeTwister(uint id, float delay)
        {
            var twister = Cast(id, AID.DarkblazeTwister, delay, 4, "Twister");
            twister.Exit.Add(Module.ActivateComponent<DarkblazeTwister>);
            twister.EndHint |= StateMachine.StateHint.PositioningStart;

            Cast(id + 0x1000, AID.SearingBreeze, 4.1f, 3, "SearingBreeze");

            AshplumeCast(id + 0x2000, 4.1f);

            var knockback = ComponentCondition<DarkblazeTwister>(id + 0x3000, 2.8f, comp => comp.DarkTwister(Module) == null, "Knockback");
            knockback.EndHint |= StateMachine.StateHint.Knockback;

            var aoe = ComponentCondition<DarkblazeTwister>(id + 0x4000, 2, comp => !comp.BurningTwisters(Module).Any(), "AOE");
            aoe.Exit.Add(Module.DeactivateComponent<DarkblazeTwister>);

            var resolve = AshplumeResolve(id + 0x5000, 2.3f);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
        }
    }
}
