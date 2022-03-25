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
            CrystallizeSwitchWeapon(0x00020000, 5.1f);
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
            MagosRadiance(id, 2.8f);
            Aureole(id + 0x10000, 5.2f);
            CrystallizeSwitchWeapon(id + 0x20000, 4.6f);
            MousaScorn(id + 0x30000, 3.3f);
            Aureole(id + 0x40000, 5.2f);
            CrystallizeSwitchWeapon(id + 0x50000, 4.6f);
            ForkFirstMerge(id, 2.2f);
        }

        private void ForkFirstChakram(uint id)
        {
            MousaScorn(id, 3.3f);
            Aureole(id + 0x10000, 5.2f);
            CrystallizeSwitchWeapon(id + 0x20000, 4.7f);
            MagosRadiance(id + 0x30000, 2.8f);
            Aureole(id + 0x40000, 5.2f);
            CrystallizeSwitchWeapon(id + 0x50000, 4.6f);
            ForkFirstMerge(id, 2.2f);
        }

        private void ForkFirstMerge(uint id, float delay)
        {
            Intermission(id + 0x100000, delay);
            Halo(id + 0x200000, 10.2f);
            Lightwave1(id + 0x210000, 4.1f);
            Lightwave2(id + 0x220000, 2.1f);
            Halo(id + 0x230000, 4);
            HerosSundering(id + 0x240000, 6.1f);
            ShiningSaber(id + 0x250000, 5.3f);
            SwitchWeapon(id + 0x260000, 5.4f);
            ForkByWeapon(id + 0x270000, 4, ForkSecondStaff, ForkSecondChakram);
        }

        private void ForkSecondStaff(uint id)
        {
            MagosRadiance(id, 5.2f);
            CrystallizeParhelicCircleAureole(id + 0x10000, 5.2f);
            SwitchWeapon(id + 0x20000, 4.9f);
            MousaScorn(id + 0x30000, 5.2f);
            ParhelionCrystallizeAureole(id + 0x40000, 6.8f);
            SwitchWeapon(id + 0x50000, 5);
            ForkSecondMerge(id, 7.9f);
        }

        private void ForkSecondChakram(uint id)
        {
            MousaScorn(id, 5.2f);
            ParhelionCrystallizeAureole(id + 0x10000, 6.8f);
            SwitchWeapon(id + 0x20000, 5);
            MagosRadiance(id + 0x30000, 5.2f);
            CrystallizeParhelicCircleAureole(id + 0x40000, 5.2f);
            SwitchWeapon(id + 0x50000, 5);
            ForkSecondMerge(id, 7.9f);
        }

        private void ForkSecondMerge(uint id, float delay)
        {
            RadiantHalo(id + 0x100000, delay);
            Lightwave3(id + 0x110000, 5.1f);
            CrystallizeShiningSaber(id + 0x120000, 9.2f); // TODO: can there be aureole instead of saber here?..
            SwitchWeapon(id + 0x130000, 1.3f);
            // TODO: should there be a fork here?..
            Lightwave3(id + 0x140000, 7.3f);
            // TODO: what next?..
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
            Cast(id, AID.Lightwave, delay, 4, "Lightwave1");

            var hit1 = ComponentCondition<Lightwave1>(id + 0x1000, 12.1f, comp => comp.NumCasts > 0, "Crystal1");
            hit1.Enter.Add(Module.ActivateComponent<Lightwave1>);
            hit1.EndHint |= StateMachine.StateHint.PositioningStart;

            ComponentCondition<Lightwave1>(id + 0x2000, 2.1f, comp => comp.NumCasts > 1, "Crystal2");

            var infraStart = CastStart(id + 0x3000, AID.InfralateralArc, 1.3f);
            infraStart.Enter.Add(Module.ActivateComponent<InfralateralArc>);

            CastEnd(id + 0x3001, 4.9f, "InfralateralArc");

            var hit3 = ComponentCondition<Lightwave1>(id + 0x4000, 1.3f, comp => comp.NumCasts > 2, "Crystal3");
            hit3.Exit.Add(Module.DeactivateComponent<Lightwave1>);

            var resolve = ComponentCondition<InfralateralArc>(id + 0x6000, 2.7f, comp => comp.NumCasts > 2, "Resolve");
            resolve.Exit.Add(Module.DeactivateComponent<InfralateralArc>);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private void Lightwave2(uint id, float delay)
        {
            Cast(id, AID.Lightwave, delay, 4, "Lightwave2");

            // note that we don't show any hints until first glory starts casting, since it's a bit misleading...
            var glory1 = Cast(id + 0x1000, AID.HerosGlory, 4.9f, 5, "Glory1");
            glory1.Enter.Add(Module.ActivateComponent<Lightwave2>);
            glory1.EndHint |= StateMachine.StateHint.PositioningStart;

            ComponentCondition<Lightwave2>(id + 0x2000, 4.4f, comp => comp.NumCasts > 0, "Crystal1");
            ComponentCondition<Lightwave2>(id + 0x3000, 3.0f, comp => comp.NumCasts > 1);
            ComponentCondition<Lightwave2>(id + 0x4000, 2.9f, comp => comp.NumCasts > 2);
            ComponentCondition<Lightwave2>(id + 0x5000, 3.0f, comp => comp.NumCasts > 3);
            Cast(id + 0x6000, AID.HerosGlory, 0.7f, 5, "Glory2");

            var resolve = ComponentCondition<Lightwave2>(id + 0x7000, 1.2f, comp => comp.NumCasts > 4, "Resolve");
            resolve.Exit.Add(Module.DeactivateComponent<Lightwave2>);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        // note: keeps Lightwave3 component active, since it is relevant for next mechanic
        private void Lightwave3(uint id, float delay)
        {
            Cast(id, AID.Lightwave, delay, 4, "Lightwave");

            var echoesStart = CastStart(id + 0x1000, AID.Echoes, 15.2f);
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
            var cast = Cast(id, AID.ShiningSaber, delay, 4.9f);
            cast.Enter.Add(Module.ActivateComponent<ShiningSaber>);

            var resolve = ComponentCondition<ShiningSaber>(id + 2, 0.4f, comp => comp.NumCasts > 0, "Stack");
            resolve.Exit.Add(Module.DeactivateComponent<ShiningSaber>);
            resolve.EndHint |= StateMachine.StateHint.Raidwide;
        }

        private StateMachine.State Aureole(uint id, float delay)
        {
            var cast = CastMulti(id, new AID[] { AID.Aureole1, AID.Aureole2, AID.LateralAureole1, AID.LateralAureole2 }, delay, 5);
            cast.Enter.Add(Module.ActivateComponent<Aureole>);

            var resolve = ComponentCondition<Aureole>(id + 2, 0.6f, comp => comp.Done, "Aureole");
            resolve.Exit.Add(Module.DeactivateComponent<Aureole>);
            return resolve;
        }

        private void SwitchWeapon(uint id, float delay)
        {
            ComponentCondition<WeaponTracker>(id, delay, comp => comp.AOEImminent, "Select weapon");
            ComponentCondition<WeaponTracker>(id + 0x10, 3.6f, comp => !comp.AOEImminent, "Weapon AOE");
        }

        private void CrystallizeSwitchWeapon(uint id, float delay)
        {
            // note: there are several crystallize spells, concrete is determined by element and current weapon; weapon to switch to doesn't seem to matter
            var cast = CastMulti(id, new AID[] { AID.Crystallize1, AID.Crystallize2, AID.Crystallize3, AID.Crystallize4, AID.Crystallize5, AID.Crystallize6 }, delay, 4);
            cast.Enter.Add(Module.ActivateComponent<Crystallize>);
            cast.EndHint |= StateMachine.StateHint.PositioningStart;

            ComponentCondition<Crystallize>(id + 0x100, 0.8f, comp => comp.CurElement != Crystallize.Element.None, "Select element");
            SwitchWeapon(id + 0x200, 4.7f);

            var resolve = ComponentCondition<Crystallize>(id + 0x300, 4, comp => comp.CurElement == Crystallize.Element.None, "Element resolve");
            resolve.Exit.Add(Module.DeactivateComponent<Crystallize>);
            resolve.EndHint |= StateMachine.StateHint.Raidwide | StateMachine.StateHint.PositioningEnd;
        }

        private void CrystallizeParhelicCircleAureole(uint id, float delay)
        {
            var ice = Cast(id, AID.Crystallize3, delay, 4, "Crystallize (ice)");
            ice.Enter.Add(Module.ActivateComponent<Crystallize>);

            var orbs = Cast(id + 0x1000, AID.ParhelicCircle, 4.7f, 6, "Orbs");
            orbs.Enter.Add(Module.ActivateComponent<ParhelicCircle>);

            var orbsResolve = ComponentCondition<ParhelicCircle>(id + 0x2000, 1.9f, comp => comp.NumCasts > 0, "Orbs resolve");
            orbsResolve.Exit.Add(Module.DeactivateComponent<ParhelicCircle>);

            var crystallizeResolve = ComponentCondition<Crystallize>(id + 0x3000, 2, comp => comp.CurElement == Crystallize.Element.None, "Ice resolve");
            crystallizeResolve.Exit.Add(Module.DeactivateComponent<Crystallize>);
            crystallizeResolve.EndHint |= StateMachine.StateHint.Raidwide;

            Aureole(id + 0x4000, 2.4f);
        }

        private void ParhelionCrystallizeAureole(uint id, float delay)
        {
            var parhelion = Cast(id, AID.Parhelion, delay, 5, "Parhelion");
            parhelion.Enter.Add(Module.ActivateComponent<Parhelion>);

            var water = Cast(id + 0x1000, AID.Crystallize6, 4.8f, 4, "Crystallize (water)");
            water.Enter.Add(Module.ActivateComponent<Crystallize>);

            // note: subparhelion cast end happens exactly together with crystallize resolve
            var subparhelion = Cast(id + 0x2000, AID.Subparhelion, 3.2f, 5, "Subparhelion");
            subparhelion.Exit.Add(Module.DeactivateComponent<Crystallize>);
            subparhelion.EndHint |= StateMachine.StateHint.Raidwide;

            var aureole = Aureole(id + 0x3000, 5.2f); // note that aureole cast starts slightly before last subparhelion resolves
            aureole.Exit.Add(Module.DeactivateComponent<Parhelion>); // note that last beacon happens slightly after cast start
        }

        private void CrystallizeShiningSaber(uint id, float delay)
        {
            var cast = CastMulti(id, new AID[] { AID.Crystallize1, AID.Crystallize2, AID.Crystallize3, AID.Crystallize4, AID.Crystallize5, AID.Crystallize6 }, delay, 4);
            cast.Enter.Add(Module.ActivateComponent<Crystallize>);

            ShiningSaber(id + 0x1000, 3.2f);

            var resolve = ComponentCondition<Crystallize>(id + 0x300, 4.1f, comp => comp.CurElement == Crystallize.Element.None, "Element resolve");
            resolve.Exit.Add(Module.DeactivateComponent<Crystallize>);
            resolve.EndHint |= StateMachine.StateHint.Raidwide;
        }
    }
}
