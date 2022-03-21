using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.P1S
{
    class P1SStates : StateMachineBuilder
    {
        public P1SStates(BossModule module) : base(module)
        {
            HeavyHand(0x00000000, 8);
            AetherialShackles(0x00010000, 6, false);
            GaolerFlail(0x00020000, 4.5f);
            Knockback(0x00030000, 5.7f);
            GaolerFlail(0x00040000, 3.3f);
            WarderWrath(0x00050000, 5.6f);
            IntemperancePhase(0x00060000, 11.2f, true);
            Knockback(0x00070000, 5.3f);

            ShiningCells(0x00100000, 9.1f);
            Aetherflail(0x00110000, 8.1f);
            Knockback(0x00120000, 7.6f);
            Aetherflail(0x00130000, 3);
            ShacklesOfTime(0x00140000, 4.6f, false);
            SlamShut(0x00150000, 1.6f);

            FourfoldShackles(0x00200000, 13);
            WarderWrath(0x00210000, 5.4f);
            IntemperancePhase(0x00220000, 11.2f, false);
            WarderWrath(0x00230000, 3.7f);

            ShiningCells(0x00300000, 11.2f);

            Dictionary<AID, Action> fork = new();
            fork[AID.AetherialShackles] = Fork1;
            fork[AID.ShacklesOfTime] = Fork2;
            CastStartFork(0x00310000, fork, 6, "Shackles+Aetherchains -or- ShacklesOfTime+Knockback"); // first branch delay = 7.8
        }

        // if delay is >0, build cast-start + cast-end states, otherwise build only cast-end state (used for first cast after fork)
        private StateMachine.State CastMaybeOmitStart(uint id, AID aid, float delay, float castTime, string name)
        {
            if (delay > 0)
                return Cast(id, aid, delay, castTime, name);
            else
                return CastEnd(id, castTime, name);
        }

        private void HeavyHand(uint id, float delay)
        {
            var s = Cast(id, AID.HeavyHand, delay, 5, "HeavyHand");
            s.EndHint |= StateMachine.StateHint.Tankbuster;
        }

        private void WarderWrath(uint id, float delay)
        {
            var s = Cast(id, AID.WarderWrath, delay, 5, "Wrath");
            s.EndHint |= StateMachine.StateHint.Raidwide;
        }

        private void Aetherchain(uint id, float delay)
        {
            var s = Cast(id, AID.Aetherchain, delay, 5, "Aetherchain");
            s.Enter.Add(Module.ActivateComponent<AetherExplosion>);
            s.Exit.Add(Module.DeactivateComponent<AetherExplosion>);
        }

        // aetherial shackles is paired either with wrath (first time) or two aetherchains (second time)
        private void AetherialShackles(uint id, float delay, bool withAetherchains)
        {
            var cast = CastMaybeOmitStart(id, AID.AetherialShackles, delay, 3, "Shackles");
            cast.Exit.Add(Module.ActivateComponent<Shackles>);
            cast.EndHint |= StateMachine.StateHint.PositioningStart;

            if (withAetherchains)
            {
                Aetherchain(id + 0x1000, 6.1f);
                Aetherchain(id + 0x1100, 3.2f);
            }
            else
            {
                WarderWrath(id + 0x1000, 4.1f);
            }

            // ~19sec after cast end
            // technically, resolve happens ~0.4sec before second aetherchain cast end, but that's irrelevant
            var resolve = ComponentCondition<Shackles>(id + 0x2000, withAetherchains ? 0 : 9.8f, comp => comp.NumExpiredDebuffs >= 2, "Shackles resolve");
            resolve.Exit.Add(Module.DeactivateComponent<Shackles>);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private void FourfoldShackles(uint id, float delay)
        {
            var cast = Cast(id, AID.FourShackles, delay, 3, "FourShackles");
            cast.Exit.Add(Module.ActivateComponent<Shackles>);
            cast.EndHint |= StateMachine.StateHint.PositioningStart;

            // note that it takes almost a second for debuffs to be applied
            var hit1 = ComponentCondition<Shackles>(id + 0x10, 9, comp => comp.NumExpiredDebuffs >= 2, "Hit1", 1, 3);
            var hit2 = ComponentCondition<Shackles>(id + 0x20, 5, comp => comp.NumExpiredDebuffs >= 4, "Hit2");
            var hit3 = ComponentCondition<Shackles>(id + 0x30, 5, comp => comp.NumExpiredDebuffs >= 6, "Hit3");
            var hit4 = ComponentCondition<Shackles>(id + 0x40, 5, comp => comp.NumExpiredDebuffs >= 8, "Hit4");
            hit4.Exit.Add(Module.DeactivateComponent<Shackles>);
            hit4.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        // shackles of time is paired either with heavy hand or knockback mechanics; also cast-start sometimes is omitted if delay is 0, since it is used to determine fork path
        private void ShacklesOfTime(uint id, float delay, bool withKnockback)
        {
            var cast = CastMaybeOmitStart(id, AID.ShacklesOfTime, delay, 4, "ShacklesOfTime");
            cast.Exit.Add(Module.ActivateComponent<AetherExplosion>);
            cast.EndHint |= StateMachine.StateHint.PositioningStart;

            if (withKnockback)
            {
                Knockback(id + 0x1000, 2.2f, false);
            }
            else
            {
                HeavyHand(id + 0x1000, 5.2f);
            }

            // ~15s from cast end
            var resolve = ComponentCondition<AetherExplosion>(id + 0x2000, withKnockback ? 3.4f : 4.8f, comp => !comp.SOTActive, "Shackles resolve");
            resolve.Exit.Add(Module.DeactivateComponent<AetherExplosion>);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private StateMachine.State GaolerFlailStart(uint id, float delay)
        {
            var s = CastStartMulti(id, new AID[] { AID.GaolerFlailRL, AID.GaolerFlailLR, AID.GaolerFlailIO1, AID.GaolerFlailIO2, AID.GaolerFlailOI1, AID.GaolerFlailOI2 }, delay);
            s.Exit.Add(Module.ActivateComponent<Flails>);
            return s;
        }

        private StateMachine.State GaolerFlailEnd(uint id, float castTimeLeft, string name)
        {
            var end = CastEnd(id, castTimeLeft);
            var resolve = ComponentCondition<Flails>(id + 1, 3.6f, comp => comp.NumCasts == 2, name);
            resolve.Exit.Add(Module.DeactivateComponent<Flails>);
            return resolve;
        }

        private void GaolerFlail(uint id, float delay)
        {
            var start = GaolerFlailStart(id, delay);
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var resolve = GaolerFlailEnd(id + 1, 11.5f, "Flails");
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private void Aetherflail(uint id, float delay)
        {
            var start = CastStartMulti(id, new AID[] { AID.AetherflailRX, AID.AetherflailLX, AID.AetherflailIL, AID.AetherflailIR, AID.AetherflailOL, AID.AetherflailOR }, delay);
            start.Exit.Add(Module.ActivateComponent<Flails>);
            start.Exit.Add(Module.ActivateComponent<AetherExplosion>);
            start.EndHint |= StateMachine.StateHint.PositioningStart;

            var resolve = GaolerFlailEnd(id + 1, 11.5f, "Aetherflail");
            resolve.Exit.Add(Module.DeactivateComponent<AetherExplosion>);
            resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        private void Knockback(uint id, float delay, bool positioningHints = true)
        {
            var start = CastStartMulti(id, new AID[] { AID.KnockbackGrace, AID.KnockbackPurge }, delay);
            start.Exit.Add(Module.ActivateComponent<Knockback>);
            if (positioningHints)
                start.EndHint |= StateMachine.StateHint.PositioningStart;

            var end = CastEnd(id + 1, 5, "Knockback");
            end.EndHint |= StateMachine.StateHint.Tankbuster;

            var resolve = ComponentCondition<Knockback>(id + 2, 4.4f, comp => comp.AOEDone, "Explode");
            resolve.Exit.Add(Module.DeactivateComponent<Knockback>);
            if (positioningHints)
                resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
        }

        // full intemperance phases (overlap either with 2 wraths or with flails)
        private void IntemperancePhase(uint id, float delay, bool withWraths)
        {
            var intemp = Cast(id, AID.Intemperance, delay, 2, "Intemperance");
            intemp.Exit.Add(Module.ActivateComponent<Intemperance>);

            CastStartMulti(id + 0x1000, new AID[] { AID.IntemperateTormentUp, AID.IntemperateTormentDown }, 5.8f);
            CastEnd(id + 0x1001, 10);

            ComponentCondition<Intemperance>(id + 0x2000, 1.2f, comp => comp.NumExplosions > 0, "Cube1", 0.2f);
            if (withWraths)
            {
                WarderWrath(id + 0x3000, 1);
                ComponentCondition<Intemperance>(id + 0x4000, 5, comp => comp.NumExplosions > 1, "Cube2", 0.2f);
                WarderWrath(id + 0x5000, 0.2f);

                var resolve = ComponentCondition<Intemperance>(id + 0x6000, 5.8f, comp => comp.NumExplosions > 2, "Cube3");
                resolve.Exit.Add(Module.DeactivateComponent<Intemperance>);
            }
            else
            {
                var flailStart = GaolerFlailStart(id + 0x3000, 3);
                flailStart.EndHint |= StateMachine.StateHint.PositioningStart;

                ComponentCondition<Intemperance>(id + 0x4000, 8, comp => comp.NumExplosions > 1, "Cube2");
                GaolerFlailEnd(id + 0x5000, 3.5f, "Flails");

                var resolve = ComponentCondition<Intemperance>(id + 0x6000, 3.9f, comp => comp.NumExplosions > 2, "Cube3");
                resolve.Exit.Add(Module.DeactivateComponent<Intemperance>);
                resolve.EndHint |= StateMachine.StateHint.PositioningEnd;
            }
        }

        private void ShiningCells(uint id, float delay)
        {
            var s = Cast(id, AID.ShiningCells, delay, 7, "Cells");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            s.Exit.Add(() => Module.Arena.IsCircle = true);
        }

        private void SlamShut(uint id, float delay)
        {
            var s = Cast(id, AID.SlamShut, delay, 7, "SlamShut");
            s.EndHint |= StateMachine.StateHint.Raidwide;
            s.Exit.Add(() => Module.Arena.IsCircle = false);
        }

        // there are two possible orderings for last mechanics of the fight
        private void ForkMerge(uint id)
        {
            Aetherflail(id, 9);
            Aetherflail(id + 0x10000, 2.7f);
            Aetherflail(id + 0x20000, 2.7f);
            WarderWrath(id + 0x30000, 13); // not sure about timings below...
            Simple(id + 0x40000, 2, "?????");
        }

        private void Fork1()
        {
            AetherialShackles(0x01000000, 0, true);
            WarderWrath(0x01010000, 5.2f);
            ShacklesOfTime(0x01020000, 7.2f, true);
            WarderWrath(0x01030000, 5.9f);
            ForkMerge(0x01040000);
        }

        private void Fork2()
        {
            ShacklesOfTime(0x02000000, 0, true);
            WarderWrath(0x02010000, 3);
            AetherialShackles(0x02020000, 9, true);
            WarderWrath(0x02030000, 7);
            ForkMerge(0x02040000);
        }
    }
}
