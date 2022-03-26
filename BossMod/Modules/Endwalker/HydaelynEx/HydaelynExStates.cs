using System;
using System.Collections.Generic;
using System.Linq;

namespace BossMod.Endwalker.HydaelynEx
{
    class HydaelynExStates : StateMachineBuilder
    {
        public HydaelynExStates(BossModule module) : base(module)
        {
            HerosRadiance(0x00000000, 10.2f);
            ShiningSaber(0x00010000, 5.2f);
            CrystallizeSwitchWeapon(0x00020000, 5.5f, false);
            ForkByWeapon(0x00030000, 8, ForkFirstStaff, ForkFirstChakram);
        }

        void ForkByWeapon(uint id, uint secondOffset, Action<uint> forkStaff, Action<uint> forkChakram)
        {
            Dictionary<WeaponTracker.Stance, Action> dispatch = new();
            dispatch[WeaponTracker.Stance.Staff] = () => forkStaff((id & 0xFF000000) + (1 << 24));
            dispatch[WeaponTracker.Stance.Chakram] = () => forkChakram((id & 0xFF000000) + (secondOffset << 24));
            ComponentConditionFork<WeaponTracker, WeaponTracker.Stance>(id, 0, _ => true, comp => comp.CurStance, dispatch);
        }

        private void ForkFirstStaff(uint id)
        {
            MagosRadiance(id, 2.7f);
            Aureole(id + 0x10000, 5.2f);
            CrystallizeSwitchWeapon(id + 0x20000, 4.6f, false);
            MousaScorn(id + 0x30000, 3.2f);
            Aureole(id + 0x40000, 5.2f);
            CrystallizeSwitchWeapon(id + 0x50000, 4.6f, true);
            ForkFirstMerge(id, 2.2f);
        }

        private void ForkFirstChakram(uint id)
        {
            MousaScorn(id, 3.2f);
            Aureole(id + 0x10000, 5.2f);
            CrystallizeSwitchWeapon(id + 0x20000, 4.6f, false);
            MagosRadiance(id + 0x30000, 2.7f);
            Aureole(id + 0x40000, 5.2f);
            CrystallizeSwitchWeapon(id + 0x50000, 4.6f, true);
            ForkFirstMerge(id, 2.2f);
        }

        private void ForkFirstMerge(uint id, float delay)
        {
            Intermission(id + 0x100000, delay);
            Halo(id + 0x200000, 10.2f);
            Lightwave1(id + 0x210000, 4.1f);
            Lightwave2(id + 0x220000, 2.1f);
            Halo(id + 0x230000, 3.8f);
            HerosSundering(id + 0x240000, 6.1f);
            ShiningSaber(id + 0x250000, 5.3f);
            SwitchWeapon(id + 0x260000, 5.9f, false);
            ForkByWeapon(id + 0x270000, 4, ForkSecondStaff, ForkSecondChakram);
        }

        private void ForkSecondStaff(uint id)
        {
            MagosRadiance(id, 5.2f);
            CrystallizeParhelicCircleAureole(id + 0x10000, 5.2f);
            SwitchWeapon(id + 0x20000, 5, false);
            MousaScorn(id + 0x30000, 5.2f);
            ParhelionCrystallizeAureole(id + 0x40000, 6.8f);
            SwitchWeapon(id + 0x50000, 5, true);
            ForkSecondMerge(id, 8);
        }

        private void ForkSecondChakram(uint id)
        {
            MousaScorn(id, 5.2f);
            ParhelionCrystallizeAureole(id + 0x10000, 6.8f);
            SwitchWeapon(id + 0x20000, 5, false);
            MagosRadiance(id + 0x30000, 5.2f);
            CrystallizeParhelicCircleAureole(id + 0x40000, 5.2f);
            SwitchWeapon(id + 0x50000, 5, true);
            ForkSecondMerge(id, 8);
        }

        private void ForkSecondMerge(uint id, float delay)
        {
            RadiantHalo(id + 0x100000, delay);
            Lightwave3(id + 0x110000, 5.2f);
            CrystallizeShiningSaber(id + 0x120000, 9.1f); // TODO: can there be aureole instead of saber here?..
            SwitchWeapon(id + 0x130000, 1.3f, false); // note: we don't create a fork here, since it's kind of irrelevant...
            Lightwave3(id + 0x140000, 7.3f);
            CrystallizeAureole(id + 0x150000, 9.1f, true);
            SwitchWeapon(id + 0x160000, 1.3f, false);
            CrystallizeAureole(id + 0x170000, 7.3f, false);
            SwitchWeapon(id + 0x180000, 1.3f, true);
            Cast(id + 0x190000, AID.Enrage, 9.5f, 10, "Enrage");
        }

        private void Intermission(uint id, float delay)
        {
            Targetable(id, false, delay, "Intermission start");

            var pureCrystal = ComponentCondition<PureCrystal>(id + 0x10000, 12.5f, comp => comp.NumCasts > 0, "Raidwide + adds appear");
            pureCrystal.Enter.Add(Module.ActivateComponent<PureCrystal>);
            pureCrystal.Exit.Add(Module.DeactivateComponent<PureCrystal>);
            pureCrystal.EndHint |= StateMachine.StateHint.DowntimeEnd; // crystals become targetable ~0.1s before, echoes ~0.1s after

            var echoes = Module.Enemies(OID.Echo);
            var echoesDead = Condition(id + 0x20000, 60, () => !echoes.Where(e => e.IsTargetable && !e.IsDead).Any(), "Adds down", 10000, 1); // note that time is arbitrary
            echoesDead.Enter.Add(Module.ActivateComponent<IntermissionAdds>);
            echoesDead.Exit.Add(Module.DeactivateComponent<IntermissionAdds>);
            echoesDead.EndHint |= StateMachine.StateHint.DowntimeStart;
            // +2.1s: boss casts 26043 'Exodus'

            var exodus = ComponentCondition<Exodus>(id + 0x30000, 16.9f, comp => comp.NumCasts > 0, "Raidwide");
            exodus.Enter.Add(Module.ActivateComponent<Exodus>);
            exodus.Exit.Add(Module.DeactivateComponent<Exodus>);

            Targetable(id + 0x40000, true, 5.2f, "Intermission end");
        }

        private void Lightwave1(uint id, float delay)
        {
            Cast(id, AID.LightwaveSword, delay, 4, "Lightwave1");

            var hit1 = ComponentCondition<Lightwave1>(id + 0x1000, 12.1f, comp => comp.NumCasts > 0, "Crystal1");
            hit1.Enter.Add(Module.ActivateComponent<Lightwave1>);
            hit1.EndHint |= StateMachine.StateHint.PositioningStart;

            ComponentCondition<Lightwave1>(id + 0x2000, 2.1f, comp => comp.NumCasts > 1, "Crystal2");

            var infraStart = CastStart(id + 0x3000, AID.InfralateralArc, 1.3f);
            infraStart.Enter.Add(Module.ActivateComponent<InfralateralArc>);

            CastEnd(id + 0x3001, 4.9f, "InfralateralArc");

            var hit3 = ComponentCondition<Lightwave1>(id + 0x4000, 1.3f, comp => comp.NumCasts > 2, "Crystal3");
            hit3.Exit.Add(Module.DeactivateComponent<Lightwave1>);

            var resolve = ComponentCondition<InfralateralArc>(id + 0x6000, 2.2f, comp => comp.NumCasts > 2, "Resolve", 1.5f); // very large variance here...
            resolve.Exit.Add(Module.DeactivateComponent<InfralateralArc>);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private void Lightwave2(uint id, float delay)
        {
            Cast(id, AID.LightwaveSword, delay, 4, "Lightwave2");

            // note that we don't show any hints until first glory starts casting, since it's a bit misleading...
            var glory1 = Cast(id + 0x1000, AID.HerosGlory, 4.7f, 5, "Glory1");
            glory1.Enter.Add(Module.ActivateComponent<Lightwave2>);
            glory1.EndHint |= StateMachine.StateHint.PositioningStart;

            ComponentCondition<Lightwave2>(id + 0x2000, 4.6f, comp => comp.NumCasts > 0, "Crystal1");
            ComponentCondition<Lightwave2>(id + 0x3000, 3.0f, comp => comp.NumCasts > 1);
            ComponentCondition<Lightwave2>(id + 0x4000, 2.9f, comp => comp.NumCasts > 2);
            ComponentCondition<Lightwave2>(id + 0x5000, 3.0f, comp => comp.NumCasts > 3);
            Cast(id + 0x6000, AID.HerosGlory, 0.5f, 5, "Glory2");

            var resolve = ComponentCondition<Lightwave2>(id + 0x7000, 1.3f, comp => comp.NumCasts > 4, "Resolve");
            resolve.Exit.Add(Module.DeactivateComponent<Lightwave2>);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        // note: keeps Lightwave3 component active, since it is relevant for next mechanic
        private void Lightwave3(uint id, float delay)
        {
            CastMulti(id, new AID[] { AID.LightwaveSword, AID.LightwaveStaff, AID.LightwaveChakram }, delay, 4, "Lightwave");

            var echoesStart = CastStartMulti(id + 0x1000, new AID[] { AID.EchoesSword, AID.EchoesStaff, AID.EchoesChakram }, 15.2f);
            echoesStart.Enter.Add(Module.ActivateComponent<Lightwave3>);
            echoesStart.Enter.Add(Module.ActivateComponent<Echoes>); // note that icon appears slightly before cast start...

            CastEnd(id + 0x1001, 5, "Stack");
            // + ~1.0s: new lightwaves

            var echoesResolve = ComponentCondition<Echoes>(id + 0x2000, 4.5f, comp => comp.NumCasts > 4, "Echoes resolve");
            echoesResolve.Exit.Add(Module.DeactivateComponent<Echoes>);
            echoesResolve.Exit.Add(Module.DeactivateComponent<Lightwave3>);

            var spectrums = ComponentCondition<Spectrum>(id + 0x3000, 3.5f, comp => comp.NumCasts > 0, "Stack/spread");
            spectrums.Enter.Add(Module.ActivateComponent<Spectrum>);
            spectrums.Enter.Add(Module.ActivateComponent<Lightwave3>);
            spectrums.Exit.Add(Module.DeactivateComponent<Spectrum>);
        }

        private void HerosRadiance(uint id, float delay)
        {
            var s = Cast(id, AID.HerosRadiance, delay, 5, "Raidwide");
            s.EndHint |= StateMachine.StateHint.Raidwide;
        }

        private void MagosRadiance(uint id, float delay)
        {
            var s = Cast(id, AID.MagosRadiance, delay, 5, "Raidwide");
            s.EndHint |= StateMachine.StateHint.Raidwide;
        }

        private void MousaScorn(uint id, float delay)
        {
            var s = Cast(id, AID.MousaScorn, delay, 5, "Shared Tankbuster");
            s.Enter.Add(Module.ActivateComponent<MousaScorn>);
            s.Exit.Add(Module.DeactivateComponent<MousaScorn>);
            s.EndHint |= StateMachine.StateHint.Tankbuster;
        }

        private void Halo(uint id, float delay)
        {
            var s = Cast(id, AID.Halo, delay, 5, "Raidwide");
            s.EndHint |= StateMachine.StateHint.Raidwide;
        }

        private void RadiantHalo(uint id, float delay)
        {
            var s = Cast(id, AID.RadiantHalo, delay, 5, "Raidwide");
            s.EndHint |= StateMachine.StateHint.Raidwide;
        }

        private void HerosSundering(uint id, float delay)
        {
            var s = Cast(id, AID.HerosSundering, delay, 5, "AOE Tankbuster");
            s.Enter.Add(Module.ActivateComponent<HerosSundering>);
            s.Exit.Add(Module.DeactivateComponent<HerosSundering>);
            s.EndHint |= StateMachine.StateHint.Tankbuster;
        }

        private void ShiningSaber(uint id, float delay)
        {
            // note: resolve happens slightly after cast, but variance is too large (0.2-0.5s), so just ignore it...
            var cast = Cast(id, AID.ShiningSaber, delay, 4.9f, "Stack");
            cast.Enter.Add(Module.ActivateComponent<ShiningSaber>);
            cast.Exit.Add(Module.DeactivateComponent<ShiningSaber>);
            cast.EndHint |= StateMachine.StateHint.Raidwide;
        }

        private StateMachine.State Aureole(uint id, float delay)
        {
            // note: what is the difference between aureole spells? seems to be determined by weapon?..
            var cast = CastMulti(id, new AID[] { AID.Aureole1, AID.Aureole2, AID.LateralAureole1, AID.LateralAureole2 }, delay, 5);
            cast.Enter.Add(Module.ActivateComponent<Aureole>);

            var resolve = ComponentCondition<Aureole>(id + 2, 0.5f, comp => comp.Done, "Aureole");
            resolve.Exit.Add(Module.DeactivateComponent<Aureole>);
            return resolve;
        }

        private void ParhelicCircle(uint id, float delay)
        {
            var cast = Cast(id, AID.ParhelicCircle, delay, 6, "Orbs");
            cast.Enter.Add(Module.ActivateComponent<ParhelicCircle>);

            var resolve = ComponentCondition<ParhelicCircle>(id + 0x10, 1.9f, comp => comp.NumCasts > 0, "Orbs resolve");
            resolve.Exit.Add(Module.DeactivateComponent<ParhelicCircle>);
        }

        private void SwitchWeapon(uint id, float delay, bool toSword)
        {
            ComponentCondition<WeaponTracker>(id, delay, comp => comp.AOEImminent, "Select weapon");
            ComponentCondition<WeaponTracker>(id + 0x10, toSword ? 4.5f : 3.7f, comp => !comp.AOEImminent, "Weapon AOE");
        }

        // note: activates Crystallize component and sets positioning flag
        private StateMachine.State CrystallizeCast(uint id, float delay, string name = "Crystallize")
        {
            // note: there are several crystallize spells, concrete is determined by element and current weapon; weapon to switch to doesn't seem to matter
            var s = CastMulti(id, new AID[] { AID.CrystallizeSwordStaffWater, AID.CrystallizeStaffEarth, AID.CrystallizeStaffIce, AID.CrystallizeChakramIce, AID.CrystallizeChakramEarth, AID.CrystallizeChakramWater }, delay, 4, name);
            s.Enter.Add(Module.ActivateComponent<Crystallize>);
            s.EndHint |= StateMachine.StateHint.PositioningStart;
            return s;
        }

        // note: deactivates Crystallize component and clears positioning flag
        private void CrystallizeResolve(uint id, float delay, string name = "Element resolve")
        {
            var resolve = ComponentCondition<Crystallize>(id, delay, comp => comp.CurElement == Crystallize.Element.None, name);
            resolve.Exit.Add(Module.DeactivateComponent<Crystallize>);
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
        }

        private void CrystallizeSwitchWeapon(uint id, float delay, bool toSword)
        {
            CrystallizeCast(id, delay);
            SwitchWeapon(id + 0x200, 5.5f, toSword);
            CrystallizeResolve(id + 0x300, toSword ? 3.2f : 4);
        }

        private void CrystallizeParhelicCircleAureole(uint id, float delay)
        {
            CrystallizeCast(id, delay, "Crystallize (ice)");
            ParhelicCircle(id + 0x1000, 4.8f);
            CrystallizeResolve(id + 0x3000, 3.5f, "Ice resolve");
            Aureole(id + 0x4000, 1);
        }

        private void ParhelionCrystallizeAureole(uint id, float delay)
        {
            var parhelion = Cast(id, AID.Parhelion, delay, 5, "Parhelion");
            parhelion.Enter.Add(Module.ActivateComponent<Parhelion>);

            CrystallizeCast(id + 0x1000, 4.8f, "Crystallize (water)");
            Cast(id + 0x2000, AID.Subparhelion, 3.2f, 5, "Subparhelion");
            CrystallizeResolve(id + 0x2800, 2, "Water resolve");

            var aureole = Aureole(id + 0x3000, 3.3f); // note that aureole cast starts slightly before last subparhelion resolves
            aureole.Exit.Add(Module.DeactivateComponent<Parhelion>); // note that last beacon happens slightly after cast start
        }

        // note: expects Lightwave3 component
        private void CrystallizeShiningSaber(uint id, float delay)
        {
            var cast = CrystallizeCast(id, delay);
            cast.Exit.Add(Module.DeactivateComponent<Lightwave3>);

            ShiningSaber(id + 0x1000, 3.2f);
            CrystallizeResolve(id + 0x3000, 4.3f);
        }

        private void CrystallizeAureole(uint id, float delay, bool afterLightwave)
        {
            var cast = CrystallizeCast(id, delay);
            if (afterLightwave)
                cast.Exit.Add(Module.DeactivateComponent<Lightwave3>);

            Aureole(id + 0x1000, 3.1f);
            CrystallizeResolve(id + 0x3000, 3.7f);
        }
    }
}
