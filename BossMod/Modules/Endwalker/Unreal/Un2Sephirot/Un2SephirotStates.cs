using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BossMod.Endwalker.Unreal.Un2Sephirot
{
    class Un2SephirotStates : StateMachineBuilder
    {
        private Un2Sephirot _module;

        public Un2SephirotStates(Un2Sephirot module) : base(module)
        {
            _module = module;
            SimplePhase(0, Phase1, "P1")
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || !Module.PrimaryActor.IsTargetable;
            SimplePhase(1, Phase2, "P2: adds")
                .ActivateOnEnter<P2GenesisCochma>()
                .ActivateOnEnter<P2GenesisBinah>()
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.FindComponent<P2GenesisCochma>()!.NumCasts >= 2 && Module.FindComponent<P2GenesisBinah>()!.NumCasts >= 12;
            SimplePhase(2, Phase3, "P3")
                .ActivateOnEnter<P3Yesod>()
                .Raw.Update = () => Module.PrimaryActor.IsDestroyed && (_module.BossP3()?.IsDestroyed ?? true);
        }

        private void Phase1(uint id)
        {
            Phase1Repeat(id, 30.7f);
            Phase1Repeat(id + 0x100000, 10.2f);
            Phase1Repeat(id + 0x200000, 10.2f); // and so on...
            SimpleState(id + 0xFF0000, 10000, "???");
        }

        private void Phase1Repeat(uint id, float delay)
        {
            P1FiendishRage(id, delay);
            P1Chesed(id + 0x10000, 12.2f);
            P1EinRatzon(id + 0x20000, 12.2f);
            P1Chesed(id + 0x30000, 7);
        }

        private void P1FiendishRage(uint id, float delay)
        {
            ComponentCondition<P1EinSof>(id, delay, comp => comp.Active, "Orbs")
                .ActivateOnEnter<P1TripleTrial>()
                .ActivateOnEnter<P1EinSof>()
                .DeactivateOnExit<P1TripleTrial>();
            ComponentCondition<P1FiendishRage>(id + 0x10, 6.3f, comp => comp.NumCasts > 0, "Hit 1")
                .ActivateOnEnter<P1FiendishRage>();
            ComponentCondition<P1FiendishRage>(id + 0x11, 3.3f, comp => comp.NumCasts > 1, "Hit 2")
                .DeactivateOnExit<P1FiendishRage>();
            ComponentCondition<P1EinSof>(id + 0x20, 1.1f, comp => !comp.Active, "Orbs resolve")
                .DeactivateOnExit<P1EinSof>();
        }

        private void P1Chesed(uint id, float delay)
        {
            ActorCast(id, _module.BossP1, AID.Chesed, delay, 4, true, "Tankbuster")
                .ActivateOnEnter<P1TripleTrial>()
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<P1TripleTrial>(id + 0x10, 2.2f, comp => comp.NumCasts > 0, "Cleave")
                .DeactivateOnExit<P1TripleTrial>();
        }

        private void P1EinRatzon(uint id, float delay)
        {
            ComponentCondition<P1EinSof>(id, delay, comp => comp.Active, "Orb")
                .ActivateOnEnter<P1EinSof>();
            ActorCastStart(id + 0x10, _module.BossP1, AID.Ein, 2.3f, true, "Bait")
                .ActivateOnEnter<P1Ratzon>();
            ActorCastEnd(id + 0x11, _module.BossP1, 4, true)
                .ActivateOnEnter<P1Ein>()
                .DeactivateOnExit<P1Ein>();
            ComponentCondition<P1EinSof>(id + 0x20, 4.5f, comp => !comp.Active, "Orbs resolve")
                .DeactivateOnExit<P1Ratzon>()
                .DeactivateOnExit<P1EinSof>();
        }

        private void Phase2(uint id)
        {
            // TODO: adds spawn either when all from previous set are dead or by timeout...
            Timeout(id, 0)
                .SetHint(StateMachine.StateHint.DowntimeStart);
            Condition(id + 1, 2.6f, () => _module.Enemies(OID.Cochma).Any(c => c.IsTargetable), "Initial adds")
                .SetHint(StateMachine.StateHint.DowntimeEnd);
            SimpleState(id + 0xFF0000, 10000, "Adds enrage");
        }

        private void Phase3(uint id)
        {
            Phase3Start(id);
            Phase3Repeat(id + 0x100000, 1.2f);
            Phase3Ascension(id + 0x200000, 10.6f);
            Phase3Repeat(id + 0x300000, 8.2f);
            Phase3Enrage(id + 0x400000, 11.6f);
        }

        private void Phase3Start(uint id)
        {
            Timeout(id, 0)
                .SetHint(StateMachine.StateHint.DowntimeStart);
            ComponentCondition<P3EinSofOhr>(id + 0x10, 29.2f, comp => comp.NumCasts > 0, "Raidwide")
                .ActivateOnEnter<P3EinSofOhr>()
                .DeactivateOnExit<P3EinSofOhr>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ActorTargetable(id + 0x20, _module.BossP3, true, 9, "Reappear")
                .SetHint(StateMachine.StateHint.DowntimeEnd);
        }

        private void Phase3Repeat(uint id, float delay)
        {
            P3Yesod(id, delay);
            P3ForceField(id + 0x10000, 10.2f);
            P3EarthshakerYesod(id + 0x20000, 2.5f);
            P3Daat(id + 0x30000, 1.6f);
            P3FiendishWail(id + 0x40000, 1.1f);
            P3GevurahChesed(id + 0x50000, 2.1f); // consider merge with prev
            P3PillarsOfMercy(id + 0x60000, 3.6f);
            P3Earthshaker(id + 0x70000, 3);
            P3DaatYesad(id + 0x80000, 3.3f);
            P3FiendishWail(id + 0x90000, 1.1f);
            P3GevurahChesed(id + 0xA0000, 2.1f); // consider merge with prev
            P3Malkuth(id + 0xB0000, 1.6f);
            P3StormOfWords(id + 0xC0000, 4.2f);
        }

        private void P3Yesod(uint id, float delay)
        {
            ComponentCondition<P3Yesod>(id, delay, comp => comp.Casters.Count > 0, "Twisters bait");
            ComponentCondition<P3Yesod>(id + 1, 3, comp => comp.Casters.Count == 0, "Twisters resolve");
        }

        private void P3ForceField(uint id, float delay)
        {
            ActorCastMulti(id, _module.BossP3, new[] { AID.GevurahChesed, AID.ChesedGevurah }, delay, 5, true)
                .ActivateOnEnter<P3GevurahChesed>();
            ComponentCondition<P3GevurahChesed>(id + 2, 0.6f, comp => comp.NumCasts > 0, "Match color")
                .DeactivateOnExit<P3GevurahChesed>();
            ComponentCondition<P3FiendishWail>(id + 0x10, 1.6f, comp => comp.Active)
                .ActivateOnEnter<P3FiendishWail>();
            ComponentCondition<P3FiendishWail>(id + 0x11, 4, comp => !comp.Active, "Towers 1")
                .DeactivateOnExit<P3FiendishWail>();

            // TODO: tethers

            ComponentCondition<P3FiendishWail>(id + 0x40, 14.6f, comp => comp.Active)
                .ActivateOnEnter<P1EinSof>()
                .ActivateOnEnter<P3FiendishWail>()
                .DeactivateOnExit<P1EinSof>();
            ComponentCondition<P3FiendishWail>(id + 0x41, 4, comp => !comp.Active, "Towers 2")
                .DeactivateOnExit<P3FiendishWail>();

            ActorCastMulti(id + 0x50, _module.BossP3, new[] { AID.GevurahChesed, AID.ChesedGevurah }, 2.2f, 5, true)
                .ActivateOnEnter<P3GevurahChesed>();
            ComponentCondition<P3GevurahChesed>(id + 0x52, 0.6f, comp => comp.NumCasts > 0, "Match color")
                .DeactivateOnExit<P3GevurahChesed>();
        }

        private void P3EarthshakerYesod(uint id, float delay)
        {
            ComponentCondition<P3Earthshaker>(id, delay, comp => comp.Active)
                .ActivateOnEnter<P3Earthshaker>();
            ComponentCondition<P3Yesod>(id + 1, 3.6f, comp => comp.Casters.Count > 0, "Twisters bait");
            ComponentCondition<P3Earthshaker>(id + 2, 1.3f, comp => !comp.Active, "Earthshakers")
                .DeactivateOnExit<P3Earthshaker>();
            ComponentCondition<P3Yesod>(id + 3, 1.7f, comp => comp.Casters.Count == 0, "Twisters resolve");
        }

        private void P3Daat(uint id, float delay)
        {
            ActorCast(id, _module.BossP3, AID.DaatMT, delay, 5, true, "Spread hit 1")
                .ActivateOnEnter<P3Daat>();
            ComponentCondition<P3Daat>(id + 0x10, 3.2f, comp => comp.NumCasts >= 3, "Spread hit 4")
                .DeactivateOnExit<P3Daat>();
        }

        private void P3FiendishWail(uint id, float delay)
        {
            ComponentCondition<P3FiendishWail>(id, delay, comp => comp.Active)
                .ActivateOnEnter<P3FiendishWail>();
            ComponentCondition<P3FiendishWail>(id + 1, 4, comp => !comp.Active, "Towers (tanks)")
                .DeactivateOnExit<P3FiendishWail>();
        }

        private void P3GevurahChesed(uint id, float delay)
        {
            ActorCastMulti(id, _module.BossP3, new[] { AID.GevurahChesed, AID.ChesedGevurah }, delay, 5, true)
                .ActivateOnEnter<P3GevurahChesed>();
            ComponentCondition<P3GevurahChesed>(id + 2, 0.6f, comp => comp.NumCasts > 0, "Raidwide")
                .DeactivateOnExit<P3GevurahChesed>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void P3PillarsOfMercy(uint id, float delay)
        {
            ComponentCondition<P3PillarOfMercyKnockback>(id, delay, comp => comp.Casters.Count > 0)
                .ActivateOnEnter<P3PillarOfMercyAOE>()
                .ActivateOnEnter<P3PillarOfMercyKnockback>()
                .ActivateOnEnter<P1EinSof>();
            ComponentCondition<P3Yesod>(id + 1, 1.7f, comp => comp.Casters.Count > 0, "Twisters bait");
            ComponentCondition<P3PillarOfMercyKnockback>(id + 2, 2.7f, comp => comp.NumCasts >= 1, "Knockback 1");
            ComponentCondition<P3Yesod>(id + 3, 0.3f, comp => comp.Casters.Count == 0); // no name, since time difference is too small
            ComponentCondition<P3PillarOfMercyKnockback>(id + 0x10, 4.8f, comp => comp.NumCasts >= 2, "Knockback 2");
            ComponentCondition<P3PillarOfMercyKnockback>(id + 0x20, 4.0f, comp => comp.NumCasts >= 3, "Knockback 3")
                .DeactivateOnExit<P3PillarOfMercyAOE>()
                .DeactivateOnExit<P3PillarOfMercyKnockback>();
            ComponentCondition<P1EinSof>(id + 0x30, 1.4f, comp => !comp.Active, "Orbs resolve")
                .DeactivateOnExit<P1EinSof>();
        }

        private void P3Earthshaker(uint id, float delay)
        {
            ComponentCondition<P3Earthshaker>(id, delay, comp => comp.Active)
                .ActivateOnEnter<P3Earthshaker>();
            ComponentCondition<P3Earthshaker>(id + 1, 5, comp => !comp.Active, "Earthshakers")
                .DeactivateOnExit<P3Earthshaker>();
        }

        private void P3DaatYesad(uint id, float delay)
        {
            ActorCastStart(id, _module.BossP3, AID.DaatMT, delay, true);
            ComponentCondition<P3Yesod>(id + 1, 4.1f, comp => comp.Casters.Count > 0, "Twisters bait")
                .ActivateOnEnter<P3Daat>();
            ActorCastEnd(id + 2, _module.BossP3, 0.9f, true, "Spread hit 1");
            ComponentCondition<P3Yesod>(id + 0x10, 2.1f, comp => comp.Casters.Count == 0, "Twisters resolve");
            ComponentCondition<P3Daat>(id + 0x20, 1.1f, comp => comp.NumCasts >= 3, "Spread hit 4")
                .DeactivateOnExit<P3Daat>();
        }

        private void P3Malkuth(uint id, float delay)
        {
            ActorCast(id, _module.BossP3, AID.Malkuth, delay, 4, true, "Knockback")
                .ActivateOnEnter<P3Malkuth>()
                .DeactivateOnExit<P3Malkuth>();
        }

        private void P3StormOfWords(uint id, float delay)
        {
            P3GevurahChesed(id, delay);

            ComponentCondition<P3FiendishWail>(id + 0x1000, 1.5f, comp => comp.Active)
                .ActivateOnEnter<P3FiendishWail>();
            ComponentCondition<P3Yesod>(id + 0x1001, 2.7f, comp => comp.Casters.Count > 0, "Twisters bait");
            ComponentCondition<P3FiendishWail>(id + 0x1002, 1.3f, comp => !comp.Active, "Towers (tanks)")
                .DeactivateOnExit<P3FiendishWail>();
            ComponentCondition<P3Yesod>(id + 0x1003, 1.7f, comp => comp.Casters.Count == 0, "Twisters resolve");

            P3GevurahChesed(id + 0x2000, 0.5f);

            ComponentCondition<P3Yesod>(id + 0x3000, 9.5f, comp => comp.Casters.Count > 0, "Twisters bait");
            ActorCastStartMulti(id + 0x3001, _module.BossP3, new[] { AID.GevurahChesed, AID.ChesedGevurah }, 1.1f, true);
            ComponentCondition<P3Yesod>(id + 0x3002, 1.9f, comp => comp.Casters.Count == 0, "Twisters resolve")
                .ActivateOnEnter<P3GevurahChesed>();
            ActorCastEnd(id + 0x3003, _module.BossP3, 3, true);
            ComponentCondition<P3GevurahChesed>(id + 0x3004, 0.6f, comp => comp.NumCasts > 0, "Raidwide")
                .DeactivateOnExit<P3GevurahChesed>()
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void Phase3Ascension(uint id, float delay)
        {
            ComponentCondition<P3Ascension>(id, delay, comp => comp.NumCasts > 0, "Ascension")
                .ActivateOnEnter<P3Ascension>()
                .DeactivateOnExit<P3Ascension>();
        }

        private void Phase3Enrage(uint id, float delay)
        {
            ComponentCondition<P3PillarOfSeverity>(id, delay, comp => comp.NumCasts > 0, "Enrage")
                .ActivateOnEnter<P3PillarOfSeverity>()
                .DeactivateOnExit<P3PillarOfSeverity>();
        }
    }
}
