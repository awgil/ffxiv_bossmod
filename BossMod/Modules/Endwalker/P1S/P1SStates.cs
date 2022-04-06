using System;
using System.Collections.Generic;

namespace BossMod.Endwalker.P1S
{
    class P1SStates : StateMachineBuilder
    {
        public P1SStates(BossModule module) : base(module)
        {
            HeavyHand(0x00000000, 8.2f);
            AetherialShackles(0x00010000, 6, false);
            GaolerFlail(0x00020000, 4.6f);
            Knockback(0x00030000, 5.7f);
            GaolerFlail(0x00040000, 3.3f);
            WarderWrath(0x00050000, 5.6f);
            IntemperancePhase(0x00060000, 11.2f, true);
            Knockback(0x00070000, 5.4f);

            ShiningCells(0x00100000, 9.3f);
            Aetherflail(0x00110000, 8);
            Knockback(0x00120000, 7.7f);
            Aetherflail(0x00130000, 3);
            ShacklesOfTime(0x00140000, 4.6f, false);
            SlamShut(0x00150000, 1.6f);

            FourfoldShackles(0x00200000, 12.8f);
            WarderWrath(0x00210000, 5.4f);
            IntemperancePhase(0x00220000, 11.2f, false);
            WarderWrath(0x00230000, 3.7f);

            ShiningCells(0x00300000, 11.2f);

            Dictionary<AID, Action> fork = new();
            fork[AID.AetherialShackles] = Fork1;
            fork[AID.ShacklesOfTime] = Fork2;
            CastStartFork(0x00310000, fork, 6.2f, "Shackles+Aetherchains -or- ShacklesOfTime+Knockback"); // first branch delay = 7.8
        }

        // if delay is >0, build cast-start + cast-end states, otherwise build only cast-end state (used for first cast after fork)
        private State CastMaybeOmitStart(uint id, AID aid, float delay, float castTime, string name)
        {
            if (delay > 0)
                return Cast(id, aid, delay, castTime, name);
            else
                return CastEnd(id, castTime, name);
        }

        private void HeavyHand(uint id, float delay)
        {
            Cast(id, AID.HeavyHand, delay, 5, "Tankbuster")
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void WarderWrath(uint id, float delay)
        {
            Cast(id, AID.WarderWrath, delay, 5, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void Aetherchain(uint id, float delay)
        {
            Cast(id, AID.Aetherchain, delay, 5, "Aetherchain")
                .ActivateOnEnter<AetherExplosion>()
                .DeactivateOnExit<AetherExplosion>();
        }

        // aetherial shackles is paired either with wrath (first time) or two aetherchains (second time)
        private void AetherialShackles(uint id, float delay, bool withAetherchains)
        {
            CastMaybeOmitStart(id, AID.AetherialShackles, delay, 3, "Shackles")
                .ActivateOnEnter<Shackles>()
                .SetHint(StateMachine.StateHint.PositioningStart);

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
            ComponentCondition<Shackles>(id + 0x2000, withAetherchains ? 0 : 9.7f, comp => comp.NumExpiredDebuffs >= 2, "Shackles resolve")
                .DeactivateOnExit<Shackles>()
                .SetHint(StateMachine.StateHint.PositioningEnd);
        }

        private void FourfoldShackles(uint id, float delay)
        {
            Cast(id, AID.FourShackles, delay, 3, "FourShackles")
                .ActivateOnEnter<Shackles>()
                .SetHint(StateMachine.StateHint.PositioningStart);
            // note that it takes almost a second for debuffs to be applied
            ComponentCondition<Shackles>(id + 0x10, 8.9f, comp => comp.NumExpiredDebuffs >= 2, "Hit1", 1, 3);
            ComponentCondition<Shackles>(id + 0x20, 5, comp => comp.NumExpiredDebuffs >= 4, "Hit2");
            ComponentCondition<Shackles>(id + 0x30, 5, comp => comp.NumExpiredDebuffs >= 6, "Hit3");
            ComponentCondition<Shackles>(id + 0x40, 5, comp => comp.NumExpiredDebuffs >= 8, "Hit4")
                .DeactivateOnExit<Shackles>()
                .SetHint(StateMachine.StateHint.PositioningEnd);
        }

        // shackles of time is paired either with heavy hand or knockback mechanics; also cast-start sometimes is omitted if delay is 0, since it is used to determine fork path
        private void ShacklesOfTime(uint id, float delay, bool withKnockback)
        {
            var cast = CastMaybeOmitStart(id, AID.ShacklesOfTime, delay, 4, "ShacklesOfTime")
                .ActivateOnEnter<AetherExplosion>()
                .SetHint(StateMachine.StateHint.PositioningStart);

            if (withKnockback)
            {
                Knockback(id + 0x1000, 2.2f, false);
            }
            else
            {
                HeavyHand(id + 0x1000, 5.2f);
            }

            // ~15s from cast end
            ComponentCondition<AetherExplosion>(id + 0x2000, withKnockback ? 3.4f : 4.7f, comp => !comp.SOTActive, "Shackles resolve")
                .DeactivateOnExit<AetherExplosion>()
                .SetHint(StateMachine.StateHint.PositioningEnd);
        }

        private void GaolerFlail(uint id, float delay)
        {
            CastStartMulti(id, new AID[] { AID.GaolerFlailRL, AID.GaolerFlailLR, AID.GaolerFlailIO1, AID.GaolerFlailIO2, AID.GaolerFlailOI1, AID.GaolerFlailOI2 }, delay)
                .SetHint(StateMachine.StateHint.PositioningStart);
            CastEnd(id + 1, 11.5f)
                .ActivateOnEnter<Flails>();
            ComponentCondition<Flails>(id + 2, 3.6f, comp => comp.NumCasts == 2, "Flails")
                .DeactivateOnExit<Flails>()
                .SetHint(StateMachine.StateHint.PositioningEnd);
        }

        private void Aetherflail(uint id, float delay)
        {
            CastStartMulti(id, new AID[] { AID.AetherflailRX, AID.AetherflailLX, AID.AetherflailIL, AID.AetherflailIR, AID.AetherflailOL, AID.AetherflailOR }, delay)
                .SetHint(StateMachine.StateHint.PositioningStart);
            CastEnd(id + 1, 11.5f)
                .ActivateOnEnter<Flails>()
                .ActivateOnEnter<AetherExplosion>();
            ComponentCondition<Flails>(id + 2, 3.6f, comp => comp.NumCasts == 2, "Aetherflail")
                .DeactivateOnExit<Flails>()
                .DeactivateOnExit<AetherExplosion>()
                .SetHint(StateMachine.StateHint.PositioningEnd);
        }

        private void Knockback(uint id, float delay, bool positioningHints = true)
        {
            CastStartMulti(id, new AID[] { AID.KnockbackGrace, AID.KnockbackPurge }, delay)
                .SetHint(StateMachine.StateHint.PositioningStart, positioningHints);
            CastEnd(id + 1, 5, "Knockback")
                .ActivateOnEnter<Knockback>()
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<Knockback>(id + 2, 4.3f, comp => comp.AOEDone, "Explode")
                .DeactivateOnExit<Knockback>()
                .SetHint(StateMachine.StateHint.PositioningEnd, positioningHints);
        }

        // full intemperance phases (overlap either with 2 wraths or with flails)
        private void IntemperancePhase(uint id, float delay, bool withWraths)
        {
            Cast(id, AID.Intemperance, delay, 2, "Intemperance");
            CastStartMulti(id + 0x1000, new AID[] { AID.IntemperateTormentUp, AID.IntemperateTormentDown }, 5.9f)
                .ActivateOnEnter<Intemperance>();
            CastEnd(id + 0x1001, 10);
            ComponentCondition<Intemperance>(id + 0x2000, 1.2f, comp => comp.NumExplosions > 0, "Cube1", 0.2f)
                .SetHint(StateMachine.StateHint.PositioningStart);

            if (withWraths)
            {
                WarderWrath(id + 0x3000, 1);
                ComponentCondition<Intemperance>(id + 0x4000, 5, comp => comp.NumExplosions > 1, "Cube2", 0.2f);
                WarderWrath(id + 0x5000, 0.2f);
            }
            else
            {
                CastStartMulti(id + 0x3000, new AID[] { AID.GaolerFlailRL, AID.GaolerFlailLR, AID.GaolerFlailIO1, AID.GaolerFlailIO2, AID.GaolerFlailOI1, AID.GaolerFlailOI2 }, 3);
                ComponentCondition<Intemperance>(id + 0x4000, 8, comp => comp.NumExplosions > 1, "Cube2")
                    .ActivateOnEnter<Flails>();
                CastEnd(id + 0x5000, 3.5f);
                ComponentCondition<Flails>(id + 0x5001, 3.6f, comp => comp.NumCasts == 2, "Flails")
                    .DeactivateOnExit<Flails>();
            }

            ComponentCondition<Intemperance>(id + 0x6000, withWraths ? 5.8f : 3.9f, comp => comp.NumExplosions > 2, "Cube3")
                .DeactivateOnExit<Intemperance>()
                .SetHint(StateMachine.StateHint.PositioningEnd);
        }

        private void ShiningCells(uint id, float delay)
        {
            var s = Cast(id, AID.ShiningCells, delay, 7, "Cells")
                .SetHint(StateMachine.StateHint.Raidwide);
            s.Raw.Exit.Add(() => Module.Arena.IsCircle = true);
        }

        private void SlamShut(uint id, float delay)
        {
            var s = Cast(id, AID.SlamShut, delay, 7, "SlamShut")
                .SetHint(StateMachine.StateHint.Raidwide);
            s.Raw.Exit.Add(() => Module.Arena.IsCircle = false);
        }

        private void Fork1()
        {
            AetherialShackles(0x01000000, 0, true);
            WarderWrath(0x01010000, 5.2f);
            ShacklesOfTime(0x01020000, 7.2f, true);
            WarderWrath(0x01030000, 5.9f);
            ForkMerge(0x01040000, 9);
        }

        private void Fork2()
        {
            ShacklesOfTime(0x02000000, 0, true);
            WarderWrath(0x02010000, 3.9f);
            AetherialShackles(0x02020000, 9, true);
            WarderWrath(0x02030000, 7.2f);
            ForkMerge(0x02040000, 8.8f);
        }

        // there are two possible orderings for last mechanics of the fight
        private void ForkMerge(uint id, float delay)
        {
            Aetherflail(id, delay);
            Aetherflail(id + 0x10000, 2.7f);
            Aetherflail(id + 0x20000, 2.7f);
            WarderWrath(id + 0x30000, 10.7f);
            WarderWrath(id + 0x40000, 4.2f);
            WarderWrath(id + 0x50000, 4.2f);
            Cast(id + 0x60000, AID.Enrage, 8.2f, 12, "Enrage");
        }
    }
}
