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
            Phase1Repeat(id + 0x100000, 10.1f);
            //Phase1Repeat(id + 0x200000, 10.1f); // and so on...
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
            ActorCastStart(id + 0x10, _module.BossP1, AID.Ein, 2.2f, true, "Bait")
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
            SimpleState(id + 0xFF0000, 10000, "???");
        }

        private void Phase3Start(uint id)
        {
            Timeout(id, 0)
                .SetHint(StateMachine.StateHint.DowntimeStart);
            ComponentCondition<P3EinSofOhr>(id + 0x10, 18.2f, comp => comp.NumCasts > 0, "Raidwide")
                .ActivateOnEnter<P3EinSofOhr>()
                .DeactivateOnExit<P3EinSofOhr>()
                .SetHint(StateMachine.StateHint.Raidwide);
            ActorTargetable(id + 0x20, _module.BossP3, true, 20, "Reappear")
                .SetHint(StateMachine.StateHint.DowntimeEnd);
        }

        private void Phase3Repeat(uint id, float delay)
        {
            P3Yesod(id, delay);
            P3ForceField(id + 0x10000, 10.2f);
            P3EarthshakerTwister(id + 0x20000, 2.5f);

        }

        private void P3Yesod(uint id, float delay)
        {
            ComponentCondition<P3Yesod>(id, delay, comp => comp.Casters.Count > 0);
            ComponentCondition<P3Yesod>(id + 1, 3, comp => comp.Casters.Count == 0, "Twisters");
        }

        private void P3ForceField(uint id, float delay)
        {
            ActorCastMulti(id, _module.BossP3, new[] { AID.GevurahChesed, AID.ChesedGevurah }, delay, 5, true)
                .ActivateOnEnter<P3GevurahChesed>();
            ComponentCondition<P3GevurahChesed>(id + 2, 0.6f, comp => comp.NumCasts > 0, "Match color")
                .DeactivateOnExit<P3GevurahChesed>();
            ComponentCondition<P3FiendishWail>(id + 0x10, 1.5f, comp => comp.Active)
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

        private void P3EarthshakerTwister(uint id, float delay)
        {
            ComponentCondition<P3Earthshaker>(id, delay, comp => comp.Active)
                .ActivateOnEnter<P3Earthshaker>();
            ComponentCondition<P3Yesod>(id + 1, 3.6f, comp => comp.Casters.Count > 0);
            ComponentCondition<P3Earthshaker>(id + 2, 1.3f, comp => !comp.Active, "Earthshakers")
                .DeactivateOnExit<P3Earthshaker>();
            ComponentCondition<P3Yesod>(id + 3, 1.7f, comp => comp.Casters.Count == 0, "Twisters");
        }

        //private void P3GevurahChesed(uint id, float delay)
        //{
        //    // TODO: component
        //    ActorCastMulti(id, _module.BossP3, new[] { AID.GevurahChesed, AID.ChesedGevurah }, delay, 5, true, "Raidwide");
        //    // TODO: +0.6 resolve...
        //}
    }
}
