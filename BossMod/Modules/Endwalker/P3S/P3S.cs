using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BossMod.P3S
{
    public class P3S : BossModule
    {
        public Actor? Boss() => PrimaryActor;

        public P3S(BossModuleManager manager, Actor primary)
            : base(manager, primary, true)
        {
            Arena.IsCircle = true;

            StateMachine.State? s;
            s = ScorchedExaltation(ref InitialState, 8);
            s = HeatOfCondemnation(ref s.Next, 3.2f);
            s = FireplumeCinderwing(ref s.Next, 5.1f);
            s = DarkenedFire(ref s.Next, 8.2f);
            s = HeatOfCondemnation(ref s.Next, 6.6f);
            s = ScorchedExaltation(ref s.Next, 2.1f);
            s = DevouringBrandFireplumeBreezeCinderwing(ref s.Next, 7.2f);
            s = HeatOfCondemnation(ref s.Next, 3.2f);
            s = FlyAwayBirds(ref s.Next, 2.1f);
            s = DeadRebirth(ref s.Next, 9.2f);
            s = HeatOfCondemnation(ref s.Next, 9.2f);
            s = FledglingFlight(ref s.Next, 7.1f);
            s = GloryplumeMulti(ref s.Next, 8);
            s = FountainOfFire(ref s.Next, 12.1f);
            s = ScorchedExaltation(ref s.Next, 2.1f);
            s = ScorchedExaltation(ref s.Next, 2.1f);
            s = HeatOfCondemnation(ref s.Next, 5.2f);
            s = FirestormsOfAsphodelos(ref s.Next, 8.6f);
            s = ConesAshplume(ref s.Next, 3.2f);
            s = ConesStorms(ref s.Next, 2);
            s = DarkblazeTwister(ref s.Next, 2.2f);
            s = ScorchedExaltation(ref s.Next, 2.1f);
            s = DeathToll(ref s.Next, 7.2f);
            s = GloryplumeSingle(ref s.Next, 7.3f);
            s = FlyAwayNoBirds(ref s.Next, 3);
            s = DevouringBrandFireplumeBreezeCinderwing(ref s.Next, 5.1f);
            s = ScorchedExaltation(ref s.Next, 6.2f);
            s = ScorchedExaltation(ref s.Next, 2.2f);
            s = CommonStates.Cast(ref s.Next, Boss, AID.FinalExaltation, 2.1f, 10, "Enrage");
        }

        protected override void DrawArenaForegroundPost(int pcSlot, Actor pc)
        {
            Arena.Actor(Boss(), Arena.ColorEnemy);
            Arena.Actor(pc, Arena.ColorPC);
        }

        private StateMachine.State ScorchedExaltation(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.ScorchedExaltation, delay, 5, "AOE");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State DeadRebirth(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.DeadRebirth, delay, 10, "DeadRebirth");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State FirestormsOfAsphodelos(ref StateMachine.State? link, float delay)
        {
            var s = CommonStates.Cast(ref link, Boss, AID.FirestormsOfAsphodelos, delay, 5, "Firestorm");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            return s;
        }

        private StateMachine.State HeatOfCondemnation(ref StateMachine.State? link, float delay)
        {
            var cast = CommonStates.Cast(ref link, Boss, AID.HeatOfCondemnation, delay, 6);
            cast.Enter.Add(() => ActivateComponent(new HeatOfCondemnation(this)));

            var resolve = CommonStates.ComponentCondition<HeatOfCondemnation>(ref cast.Next, 1.1f, this, comp => comp.NumCasts > 0, "Tether");
            resolve.Exit.Add(DeactivateComponent<HeatOfCondemnation>);
            resolve.EndHint |= StateMachine.StateHint.Tankbuster;
            return resolve;
        }

        // note - activates component, which should be later deactivated manually
        // note - positioning state is set at the end, make sure to clear later - this is because this mechanic overlaps with other stuff
        private StateMachine.State Fireplume(ref StateMachine.State? link, float delay)
        {
            // mechanics:
            // 1. single-plume version: immediately after cast end, 1 helper teleports to position and starts casting 26303, which takes 6s
            // 2. multi-plume version: immediately after cast end, 9 helpers teleport to positions and start casting 26305
            //    first pair starts cast almost immediately, then pairs 2-4 and finally central start their cast with 1 sec between them; each cast lasts 2 sec
            // so center (last/only) plume hits around 6s after cast end
            // note that our helpers rely on 233C casts rather than states
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.ExperimentalFireplumeSingle] = new(null, () => { });
            dispatch[AID.ExperimentalFireplumeMulti] = new(null, () => { });
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CommonStates.CastEnd(ref start.Next, Boss, 5, "Fireplume");
            end.Exit.Add(() => ActivateComponent(new Fireplume(this)));
            return end;
        }

        // note - no positioning flags, since this is part of mechanics that manage it themselves
        // note - since it resolves in a complex way, make sure to add a resolve state!
        private StateMachine.State AshplumeCast(ref StateMachine.State? link, float delay)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.ExperimentalAshplumeStack] = new(null, () => ActivateComponent(new Ashplume(this, Ashplume.State.Stack)));
            dispatch[AID.ExperimentalAshplumeSpread] = new(null, () => ActivateComponent(new Ashplume(this, Ashplume.State.Spread)));
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);
            var end = CommonStates.CastEnd(ref start.Next, Boss, 5, "Ashplume");
            end.EndHint |= StateMachine.StateHint.GroupWithNext;
            return end;
        }

        private StateMachine.State AshplumeResolve(ref StateMachine.State? link, float delay)
        {
            var resolve = CommonStates.ComponentCondition<Ashplume>(ref link, delay, this, comp => comp.CurState == Ashplume.State.Done, "Ashplume resolve");
            resolve.Exit.Add(DeactivateComponent<Ashplume>);
            resolve.EndHint |= StateMachine.StateHint.Raidwide;
            return resolve;
        }

        private StateMachine.State GloryplumeMulti(ref StateMachine.State? link, float delay)
        {
            // first part for this mechanic always seems to be "multi-plume", works just like fireplume
            // 9 helpers teleport to position, first pair almost immediately starts casting 26315s, 1 sec stagger between pairs, 7 sec for each cast
            // ~3 sec after cast ends, boss makes an instant cast that determines stack/spread (26316/26312), ~10 sec after that hits with real AOE (26317/26313)
            var cast = CommonStates.Cast(ref link, Boss, AID.ExperimentalGloryplumeMulti, delay, 5, "GloryplumeMulti");
            cast.Exit.Add(() => ActivateComponent(new Fireplume(this)));
            cast.Exit.Add(() => ActivateComponent(new Ashplume(this, Ashplume.State.UnknownGlory))); // instant cast turns this into correct state in ~3 sec
            cast.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var resolve = CommonStates.ComponentCondition<Ashplume>(ref cast.Next, 13.2f, this, comp => comp.CurState == Ashplume.State.Done, "Gloryplume resolve");
            resolve.Exit.Add(DeactivateComponent<Fireplume>);
            resolve.Exit.Add(DeactivateComponent<Ashplume>);
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State GloryplumeSingle(ref StateMachine.State? link, float delay)
        {
            // first part for this mechanic always seems to be "single-plume", works just like fireplume
            // helper teleports to position, almost immediately starts casting 26311, 6 sec for cast
            // ~3 sec after cast ends, boss makes an instant cast that determines stack/spread (26316/26312), ~4 sec after that hits with real AOE (26317/26313)
            // note that our helpers rely on casts rather than states
            var cast = CommonStates.Cast(ref link, Boss, AID.ExperimentalGloryplumeSingle, delay, 5, "GloryplumeSingle");
            cast.Exit.Add(() => ActivateComponent(new Fireplume(this)));
            cast.Exit.Add(() => ActivateComponent(new Ashplume(this, Ashplume.State.UnknownGlory))); // instant cast turns this into correct state in ~3 sec
            cast.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var resolve = CommonStates.ComponentCondition<Ashplume>(ref cast.Next, 7.2f, this, comp => comp.CurState == Ashplume.State.Done, "Gloryplume resolve");
            resolve.Exit.Add(DeactivateComponent<Fireplume>);
            resolve.Exit.Add(DeactivateComponent<Ashplume>);
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State Cinderwing(ref StateMachine.State? link, float delay)
        {
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.RightCinderwing] = new(null, () => ActivateComponent(new Cinderwing(this, false)));
            dispatch[AID.LeftCinderwing] = new(null, () => ActivateComponent(new Cinderwing(this, true)));
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);

            var end = CommonStates.CastEnd(ref start.Next, Boss, 5, "Wing");
            end.Exit.Add(DeactivateComponent<Cinderwing>);
            return end;
        }

        private StateMachine.State FireplumeCinderwing(ref StateMachine.State? link, float delay)
        {
            var fireplume = Fireplume(ref link, delay); // pos-start
            fireplume.EndHint |= StateMachine.StateHint.GroupWithNext;

            var cinderwing = Cinderwing(ref fireplume.Next, 5.7f);
            cinderwing.Exit.Add(DeactivateComponent<Fireplume>);
            cinderwing.EndHint |= StateMachine.StateHint.PositioningEnd;
            return cinderwing;
        }

        private StateMachine.State DevouringBrandFireplumeBreezeCinderwing(ref StateMachine.State? link, float delay)
        {
            var devouring = CommonStates.Cast(ref link, Boss, AID.DevouringBrand, delay, 3, "DevouringBrand");
            devouring.EndHint |= StateMachine.StateHint.GroupWithNext;

            var fireplume = Fireplume(ref devouring.Next, 2.1f); // pos-start
            fireplume.Exit.Add(() => ActivateComponent(new DevouringBrand(this)));
            fireplume.EndHint |= StateMachine.StateHint.GroupWithNext;

            var breeze = CommonStates.Cast(ref fireplume.Next, Boss, AID.SearingBreeze, 7.2f, 3, "SearingBreeze");
            breeze.Enter.Add(DeactivateComponent<Fireplume>);
            breeze.EndHint |= StateMachine.StateHint.GroupWithNext;

            var wing = Cinderwing(ref breeze.Next, 3.2f);
            wing.Enter.Add(DeactivateComponent<DevouringBrand>);
            wing.EndHint |= StateMachine.StateHint.PositioningEnd;
            return wing;
        }

        private StateMachine.State DarkenedFire(ref StateMachine.State? link, float delay)
        {
            // 3s after cast ends, adds start casting 26299
            var addsStart = CommonStates.CastStart(ref link, Boss, AID.DarkenedFire, delay);
            addsStart.Exit.Add(() => ActivateComponent(new DarkenedFire(this)));
            addsStart.EndHint |= StateMachine.StateHint.PositioningStart;

            var addsEnd = CommonStates.CastEnd(ref addsStart.Next, Boss, 6, "DarkenedFire adds");
            addsEnd.Exit.Add(DeactivateComponent<DarkenedFire>);
            addsEnd.Exit.Add(() => ActivateComponent(new BrightenedFire(this))); // icons appear just before next cast start
            addsEnd.EndHint |= StateMachine.StateHint.GroupWithNext;

            var numbers = CommonStates.Cast(ref addsEnd.Next, Boss, AID.BrightenedFire, 5.2f, 5, "Numbers"); // numbers appear at the beginning of the cast, at the end he starts shooting 1-8
            numbers.EndHint |= StateMachine.StateHint.GroupWithNext;

            var lastAOE = CommonStates.ComponentCondition<BrightenedFire>(ref numbers.Next, 8.4f, this, comp => comp.NumCasts == 8);
            lastAOE.Exit.Add(DeactivateComponent<BrightenedFire>);

            var resolve = CommonStates.Timeout(ref lastAOE.Next, 6.6f, "DarkenedFire resolve");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State TrailOfCondemnation(ref StateMachine.State? link, float delay)
        {
            // at this point boss teleports to one of the cardinals
            // parallel to this one of the helpers casts 26365 (actual aoe fire trails)
            Dictionary<AID, (StateMachine.State?, Action)> dispatch = new();
            dispatch[AID.TrailOfCondemnationCenter] = new(null, () => ActivateComponent(new TrailOfCondemnation(this, true)));
            dispatch[AID.TrailOfCondemnationSides] = new(null, () => ActivateComponent(new TrailOfCondemnation(this, false)));
            var start = CommonStates.CastStart(ref link, Boss, dispatch, delay);

            var end = CommonStates.CastEnd(ref start.Next, Boss, 6);

            var resolve = CommonStates.ComponentCondition<TrailOfCondemnation>(ref end.Next, 1.6f, this, comp => comp.Done, "Trail");
            resolve.Exit.Add(DeactivateComponent<TrailOfCondemnation>);
            return resolve;
        }

        // note: expects downtime at enter, clears when birds spawn, reset when birds die
        private StateMachine.State SmallBirdsPhase(ref StateMachine.State? link, float delay)
        {
            var birds = Enemies(OID.SunbirdSmall);

            var spawn = CommonStates.Condition(ref link, delay, () => birds.Count > 0, "Small birds", 10000);
            spawn.Exit.Add(() => ActivateComponent(new BirdDistance(this, birds)));
            spawn.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.DowntimeEnd; // adds become targetable <1sec after spawn

            var enrage = CommonStates.Condition(ref spawn.Next, 25, () => birds.Find(x => !x.IsDead) == null, "Small birds enrage", 10000);
            enrage.Exit.Add(DeactivateComponent<BirdDistance>);
            enrage.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart; // raidwide (26326) happens ~3sec after last bird death
            return enrage;
        }

        // note: expects downtime at enter, clears when birds spawn, reset when birds die
        private StateMachine.State LargeBirdsPhase(ref StateMachine.State? link, float delay)
        {
            var birds = Enemies(OID.SunbirdLarge);

            var spawn = CommonStates.Condition(ref link, delay, () => birds.Count > 0, "Large birds", 10000);
            spawn.Exit.Add(() => ActivateComponent(new BirdTether(this))); // note that first tethers appear ~5s after this
            spawn.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.DowntimeEnd; // adds become targetable ~1sec after spawn

            var chargesDone = CommonStates.ComponentCondition<BirdTether>(ref spawn.Next, 18.2f, this, comp => comp.NumFinishedChains == 4, "", 10000);
            chargesDone.Exit.Add(DeactivateComponent<BirdTether>);
            chargesDone.Exit.Add(() => ActivateComponent(new BirdDistance(this, birds)));

            // note that enrage is ~55sec after spawn
            var enrage = CommonStates.Condition(ref chargesDone.Next, 36.8f, () => birds.Find(x => !x.IsDead) == null, "Large birds enrage", 10000);
            enrage.Exit.Add(DeactivateComponent<BirdDistance>);
            enrage.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.DowntimeStart; // raidwide (26326) happens ~3sec after last bird death
            return enrage;
        }

        private StateMachine.State FlyAwayBirds(ref StateMachine.State? link, float delay)
        {
            var plume = Fireplume(ref link, delay); // pos-start

            var flyAway = CommonStates.Targetable(ref plume.Next, Boss, false, 4.6f);

            var trail = TrailOfCondemnation(ref flyAway.Next, 3.8f);
            trail.Enter.Add(DeactivateComponent<Fireplume>);

            var small = SmallBirdsPhase(ref trail.Next, 6.4f);

            var large = LargeBirdsPhase(ref small.Next, 3.6f);

            var reappear = CommonStates.Targetable(ref large.Next, Boss, true, 5.2f, "Boss reappears");
            reappear.EndHint |= StateMachine.StateHint.PositioningEnd;
            return reappear;
        }

        private StateMachine.State FlyAwayNoBirds(ref StateMachine.State? link, float delay)
        {
            var flyAway = CommonStates.Targetable(ref link, Boss, false, delay);

            var trail = TrailOfCondemnation(ref flyAway.Next, 3.8f);
            trail.EndHint |= StateMachine.StateHint.GroupWithNext;

            var reappear = CommonStates.Targetable(ref trail.Next, Boss, true, 3.1f, "Boss reappears");
            return reappear;
        }

        private StateMachine.State FledglingFlight(ref StateMachine.State? link, float delay)
        {
            // mechanic timeline:
            // 0s cast end
            // 2s icons appear
            // 8s 3540's teleport to players
            // 10s 3540's start casting 26342
            // 14s 3540's finish casting 26342
            // note that helper does relies on icons and cast events rather than states
            var cast = CommonStates.Cast(ref link, Boss, AID.FledglingFlight, delay, 3);
            cast.Exit.Add(() => ActivateComponent(new FledglingFlight(this)));

            var placement = CommonStates.ComponentCondition<FledglingFlight>(ref cast.Next, 10.3f, this, comp => comp.PlacementDone, "Eyes place");
            placement.EndHint |= StateMachine.StateHint.GroupWithNext;

            var resolve = CommonStates.ComponentCondition<FledglingFlight>(ref placement.Next, 4, this, comp => comp.CastsDone, "Eyes resolve");
            resolve.Exit.Add(DeactivateComponent<FledglingFlight>);
            return resolve;
        }

        private StateMachine.State DeathToll(ref StateMachine.State? link, float delay)
        {
            // notes on mechanics:
            // - on 26349 cast end, debuffs with 25sec appear
            // - 12-15sec after 26350 cast starts, eyes finish casting their cones - at this point, there's about 5sec left on debuffs
            var deathtoll = CommonStates.Cast(ref link, Boss, AID.DeathToll, delay, 6, "DeathToll");
            deathtoll.EndHint |= StateMachine.StateHint.GroupWithNext;

            var eyes = CommonStates.Cast(ref deathtoll.Next, Boss, AID.FledglingFlight, 3.2f, 3, "Eyes");
            eyes.Exit.Add(() => ActivateComponent(new FledglingFlight(this)));
            eyes.EndHint |= StateMachine.StateHint.GroupWithNext;

            var agonies = CommonStates.Cast(ref eyes.Next, Boss, AID.LifesAgonies, 2.1f, 24, "LifeAgonies");
            agonies.Exit.Add(DeactivateComponent<FledglingFlight>);
            return agonies;
        }

        private StateMachine.State FountainOfFire(ref StateMachine.State? link, float delay)
        {
            // TODO: healer component - not even sure, mechanic looks so simple...
            var fountain = CommonStates.Cast(ref link, Boss, AID.FountainOfFire, delay, 6, "FountainOfFire");
            fountain.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var pinion = CommonStates.Cast(ref fountain.Next, Boss, AID.SunsPinion, 2.1f, 6);
            pinion.Exit.Add(() => ActivateComponent(new SunshadowTether(this)));

            var charges = CommonStates.ComponentCondition<SunshadowTether>(ref pinion.Next, 16, this, comp => comp.NumCharges == 6, "Charges");
            charges.Exit.Add(DeactivateComponent<SunshadowTether>);
            charges.EndHint |= StateMachine.StateHint.PositioningEnd;
            return charges;
        }

        private StateMachine.State ConesAshplume(ref StateMachine.State? link, float delay)
        {
            var flames = CommonStates.Cast(ref link, Boss, AID.FlamesOfAsphodelos, delay, 3, "Cones");
            flames.Exit.Add(() => ActivateComponent(new FlamesOfAsphodelos(this)));
            flames.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var plume = AshplumeCast(ref flames.Next, 2.1f);

            var resolve = AshplumeResolve(ref plume.Next, 6.1f);
            resolve.Exit.Add(DeactivateComponent<FlamesOfAsphodelos>);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            return resolve;
        }

        private StateMachine.State ConesStorms(ref StateMachine.State? link, float delay)
        {
            var flames = CommonStates.Cast(ref link, Boss, AID.FlamesOfAsphodelos, delay, 3, "Cones");
            flames.Exit.Add(() => ActivateComponent(new FlamesOfAsphodelos(this)));
            flames.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var state = CommonStates.Cast(ref flames.Next, Boss, AID.StormsOfAsphodelos, 10.2f, 8, "Storms");
            state.Enter.Add(DeactivateComponent<FlamesOfAsphodelos>);
            state.Enter.Add(() => ActivateComponent(new StormsOfAsphodelos(this)));
            state.Exit.Add(DeactivateComponent<StormsOfAsphodelos>);
            state.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.Tankbuster | StateMachine.StateHint.PositioningEnd;
            return state;
        }

        private StateMachine.State DarkblazeTwister(ref StateMachine.State? link, float delay)
        {
            var twister = CommonStates.Cast(ref link, Boss, AID.DarkblazeTwister, delay, 4, "Twister");
            twister.Exit.Add(() => ActivateComponent(new DarkblazeTwister(this)));
            twister.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.PositioningStart;

            var breeze = CommonStates.Cast(ref twister.Next, Boss, AID.SearingBreeze, 4.1f, 3, "SearingBreeze");
            breeze.EndHint |= StateMachine.StateHint.GroupWithNext;

            var ashplume = AshplumeCast(ref breeze.Next, 4.1f);

            var knockback = CommonStates.ComponentCondition<DarkblazeTwister>(ref ashplume.Next, 2.8f, this, comp => comp.DarkTwister == null, "Knockback");
            knockback.EndHint |= StateMachine.StateHint.GroupWithNext | StateMachine.StateHint.Knockback;

            var aoe = CommonStates.ComponentCondition<DarkblazeTwister>(ref knockback.Next, 2, this, comp => !comp.BurningTwisters.Any(), "AOE");
            aoe.Exit.Add(DeactivateComponent<DarkblazeTwister>);
            aoe.EndHint |= StateMachine.StateHint.GroupWithNext;

            var resolve = AshplumeResolve(ref aoe.Next, 2.3f);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            return resolve;
        }
    }
}
