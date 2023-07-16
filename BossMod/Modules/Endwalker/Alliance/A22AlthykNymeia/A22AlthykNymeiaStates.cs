using BossMod.Endwalker.Extreme.Ex2Hydaelyn;
using System.Collections.Generic;
using System;
using System.Net;

namespace BossMod.Endwalker.Alliance.A22AlthykNymeia
{
    class A22AlthykNymeiaStates : StateMachineBuilder
    {
        private A22AlthykNymeia _module;

        private bool IsDead(Actor? actor) => actor == null || actor.IsDestroyed || actor.IsDead;

        public A22AlthykNymeiaStates(A22AlthykNymeia module) : base(module)
        {
            _module = module;
            SimplePhase(0, SinglePhase, "Single phase")
                .ActivateOnEnter<Axioma>()
                .Raw.Update = () => IsDead(_module.Althyk()) && IsDead(_module.Nymeia());
        }

        private void SinglePhase(uint id)
        {
            ActorCast(id, _module.Nymeia, AID.SpinnersWheel, 10.3f, 4.5f);
            Dictionary<SpinnersWheelSelect.Branch, (uint seqID, Action<uint> buildState)> dispatch = new();
            dispatch[SpinnersWheelSelect.Branch.Gaze] = ((id >> 24) + 1, ForkGaze);
            dispatch[SpinnersWheelSelect.Branch.StayMove] = ((id >> 24) + 2, ForkStayMove);
            ComponentConditionFork<SpinnersWheelSelect, SpinnersWheelSelect.Branch>(id + 0x10, 0.9f, comp => comp.SelectedBranch != SpinnersWheelSelect.Branch.None, comp => comp.SelectedBranch, dispatch, "Gaze -or- stay/move")
                .ActivateOnEnter<SpinnersWheelSelect>()
                .DeactivateOnExit<SpinnersWheelSelect>();
        }

        private void ForkGaze(uint id)
        {
            SpinnersWheelGazeResolveMythrilGreataxe(id, 10.1f);
            SpinnersWheelGazeTimeAndTide(id + 0x10000, 8.9f);
            Axioma(id + 0x20000, 20.5f);
            Hydroptosis(id + 0x30000, 3.4f);
            InexorablePull(id + 0x40000, 5.8f);
            Hydrorythmos(id + 0x50000, 8.9f);
            HydrostasisPetrai(id + 0x60000, 14.8f);
            SpinnersWheelGazeMythrilGreataxe(id + 0x70000, 11.3f);
            Hydroptosis(id + 0x80000, 0.6f);
            Petrai(id + 0x90000, 10.2f);
            HydrostasisTimeAndTide(id + 0xA0000, 8.8f);
            Axioma(id + 0xB0000, 14.5f);
            SpinnersWheelGazeHydrorythmosTimeAndTide(id + 0xC0000, 9.4f);
            SimpleState(id + 0xFF0000, 100, "???");
        }

        private void ForkStayMove(uint id)
        {
            SpinnersWheelStayMoveResolveMythrilGreataxe(id, 4.8f);
            SpinnersWheelStayMoveTimeAndTide(id + 0x10000, 10.7f);
            Axioma(id + 0x20000, 16.5f);
            Hydroptosis(id + 0x30000, 3.4f);
            InexorablePull(id + 0x40000, 6.0f);
            Hydrorythmos(id + 0x50000, 8.9f);
            HydrostasisPetrai(id + 0x60000, 14.8f);
            SpinnersWheelStayMoveMythrilGreataxe(id + 0x70000, 13.4f);
            Hydroptosis(id + 0x80000, 0.7f);
            Petrai(id + 0x90000, 11.2f);
            HydrostasisTimeAndTide(id + 0xA0000, 8.9f);
            Axioma(id + 0xB0000, 14.5f);
            SimpleState(id + 0xFF0000, 100, "???");
        }

        private void MythrilGreataxe(uint id, float delay)
        {
            ActorCast(id, _module.Althyk, AID.MythrilGreataxe, delay, 7, false, "Cleave")
                .ActivateOnEnter<MythrilGreataxe>()
                .DeactivateOnExit<MythrilGreataxe>();
        }

        private void Hydrorythmos(uint id, float delay)
        {
            ActorCast(id, _module.Nymeia, AID.Hydrorythmos, delay, 5, false, "Line 1")
                .ActivateOnEnter<Hydrorythmos>();
            ComponentCondition<Hydrorythmos>(id + 0x10, 2.1f, comp => comp.NumCasts > 1);
            ComponentCondition<Hydrorythmos>(id + 0x11, 2.1f, comp => comp.NumCasts > 3);
            ActorCastStart(id + 0x20, _module.Althyk, AID.MythrilGreataxe, 0.7f);
            ComponentCondition<Hydrorythmos>(id + 0x30, 1.4f, comp => comp.NumCasts > 5)
                .ActivateOnEnter<MythrilGreataxe>();
            ComponentCondition<Hydrorythmos>(id + 0x31, 2.1f, comp => comp.NumCasts > 7)
                .DeactivateOnExit<Hydrorythmos>();
            ActorCastEnd(id + 0x40, _module.Althyk, 3.5f, false, "Cleave")
                .DeactivateOnExit<MythrilGreataxe>();
        }

        private State SpinnersWheelGazeResolve(uint id, float delay)
        {
            return Condition(id, delay, () => _module.FindComponent<SpinnersWheelArcaneAttraction>()!.NumCasts + _module.FindComponent<SpinnersWheelAttractionReversed>()!.NumCasts > 0, "Gaze resolve")
                .DeactivateOnExit<SpinnersWheelArcaneAttraction>()
                .DeactivateOnExit<SpinnersWheelAttractionReversed>();
        }

        private void SpinnersWheelStayMoveResolve(uint id, float delay)
        {
            ComponentCondition<SpinnersWheelStayMove>(id, delay, comp => comp.ActiveDebuffs > 0, "Stay/move");
            ComponentCondition<SpinnersWheelStayMove>(id + 1, 2, comp => comp.ActiveDebuffs == 0, "Stay/move resolve")
                .DeactivateOnExit<SpinnersWheelStayMove>();
        }

        private void SpinnersWheelGazeResolveMythrilGreataxe(uint id, float delay)
        {
            SpinnersWheelGazeResolve(id, delay)
                .ActivateOnEnter<SpinnersWheelArcaneAttraction>()
                .ActivateOnEnter<SpinnersWheelAttractionReversed>();
            MythrilGreataxe(id + 0x1000, 0.8f);
        }

        private void SpinnersWheelStayMoveResolveMythrilGreataxe(uint id, float delay)
        {
            ActorCastStart(id, _module.Althyk, AID.MythrilGreataxe, delay)
                .ActivateOnEnter<SpinnersWheelStayMove>();
            ComponentCondition<SpinnersWheelStayMove>(id + 1, 5.2f, comp => comp.ActiveDebuffs > 0, "Stay/move")
                .ActivateOnEnter<MythrilGreataxe>();
            ActorCastEnd(id + 2, _module.Althyk, 1.8f, false, "Cleave")
                .DeactivateOnExit<MythrilGreataxe>();
            ComponentCondition<SpinnersWheelStayMove>(id + 3, 0.2f, comp => comp.ActiveDebuffs == 0, "Stay/move resolve")
                .DeactivateOnExit<SpinnersWheelStayMove>();
        }

        private void SpinnersWheelGazeMythrilGreataxe(uint id, float delay)
        {
            ActorCast(id, _module.Nymeia, AID.SpinnersWheel, delay, 4.5f);
            SpinnersWheelGazeResolveMythrilGreataxe(id + 0x1000, 21);
        }

        private void SpinnersWheelStayMoveMythrilGreataxe(uint id, float delay)
        {
            ActorCast(id, _module.Nymeia, AID.SpinnersWheel, delay, 4.5f);
            ComponentCondition<SpinnersWheelStayMove>(id + 0x10, 20.9f, comp => comp.ActiveDebuffs > 0, "Stay/move")
                .ActivateOnEnter<SpinnersWheelStayMove>();
            ActorCastStart(id + 0x11, _module.Althyk, AID.MythrilGreataxe, 0.9f);
            ComponentCondition<SpinnersWheelStayMove>(id + 0x12, 1.1f, comp => comp.ActiveDebuffs == 0, "Stay/move resolve")
                .ActivateOnEnter<MythrilGreataxe>()
                .DeactivateOnExit<SpinnersWheelStayMove>();
            ActorCastEnd(id + 0x13, _module.Althyk, 5.9f, false, "Cleave")
                .DeactivateOnExit<MythrilGreataxe>();
        }

        private void SpinnersWheelGazeTimeAndTide(uint id, float delay)
        {
            ActorCast(id, _module.Nymeia, AID.SpinnersWheel, delay, 4.5f)
                .ActivateOnEnter<SpinnersWheelArcaneAttraction>()
                .ActivateOnEnter<SpinnersWheelAttractionReversed>();
            ActorCast(id + 0x10, _module.Althyk, AID.TimeAndTide, 1.7f, 10);
            SpinnersWheelGazeResolve(id + 0x20, 2.7f);
        }

        private void SpinnersWheelStayMoveTimeAndTide(uint id, float delay)
        {
            ActorCast(id, _module.Nymeia, AID.SpinnersWheel, delay, 4.5f)
                .ActivateOnEnter<SpinnersWheelStayMove>();
            ActorCast(id + 0x10, _module.Althyk, AID.TimeAndTide, 1.7f, 10);
            SpinnersWheelStayMoveResolve(id + 0x20, 2.7f);
        }

        private void SpinnersWheelGazeHydrorythmosTimeAndTide(uint id, float delay)
        {
            ActorCast(id, _module.Nymeia, AID.SpinnersWheel, delay, 4.5f);
            ActorCastStart(id + 0x10, _module.Althyk, AID.TimeAndTide, 1.5f)
                .ActivateOnEnter<SpinnersWheelArcaneAttraction>()
                .ActivateOnEnter<SpinnersWheelAttractionReversed>();
            ActorCast(id + 0x20, _module.Nymeia, AID.Hydrorythmos, 0.6f, 5, false, "Line 1")
                .ActivateOnEnter<Hydrorythmos>();
            ComponentCondition<Hydrorythmos>(id + 0x30, 2.1f, comp => comp.NumCasts > 1);
            ComponentCondition<Hydrorythmos>(id + 0x31, 2.1f, comp => comp.NumCasts > 3);
            ActorCastEnd(id + 0x40, _module.Althyk, 0.2f);
            // TODO: below should happen faster... didn't see any good logs however...
            ComponentCondition<Hydrorythmos>(id + 0x50, 1.9f, comp => comp.NumCasts > 5);
            ComponentCondition<Hydrorythmos>(id + 0x51, 2.1f, comp => comp.NumCasts > 7)
                .DeactivateOnExit<Hydrorythmos>();
            SpinnersWheelGazeResolve(id + 0x60, 5.6f);
        }

        private void Axioma(uint id, float delay)
        {
            ActorCast(id, _module.Althyk, AID.Axioma, delay, 5, false, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void Hydroptosis(uint id, float delay)
        {
            ActorCast(id, _module.Nymeia, AID.Hydroptosis, delay, 5, false, "Spread")
                .ActivateOnEnter<Hydroptosis>()
                .DeactivateOnExit<Hydroptosis>();
        }

        private void InexorablePull(uint id, float delay)
        {
            ActorCast(id, _module.Althyk, AID.InexorablePull, delay, 6);
            ComponentCondition<Axioma>(id + 0x10, 0.7f, comp => !comp.ShouldBeInZone, "Kick up");
        }

        private void Petrai(uint id, float delay)
        {
            ActorCast(id, _module.Althyk, AID.Petrai, delay, 5)
                .ActivateOnEnter<Petrai>();
            ComponentCondition<Petrai>(id + 0x10, 1, comp => comp.NumCasts > 0, "Shared tankbuster")
                .DeactivateOnExit<Petrai>()
                .SetHint(StateMachine.StateHint.Tankbuster);
        }

        private void HydrostasisPetrai(uint id, float delay)
        {
            ActorCast(id, _module.Nymeia, AID.Hydrostasis, delay, 4)
                .ActivateOnEnter<Petrai>();
            ActorCastEnd(id + 2, _module.Althyk, 1.1f); // this cast ends and tower knockback casts start at the same time
            ComponentCondition<Petrai>(id + 0x10, 1, comp => comp.NumCasts > 0, "Shared tankbuster")
                .ActivateOnEnter<Hydrostasis>()
                .DeactivateOnExit<Petrai>()
                .SetHint(StateMachine.StateHint.Tankbuster);
            ComponentCondition<Hydrostasis>(id + 0x20, 15, comp => comp.NumCasts >= 1, "Knockback 1");
            ComponentCondition<Hydrostasis>(id + 0x21, 3, comp => comp.NumCasts >= 2, "Knockback 2");
            ComponentCondition<Hydrostasis>(id + 0x22, 3, comp => comp.NumCasts >= 3, "Knockback 3")
                .DeactivateOnExit<Hydrostasis>();
        }

        private void HydrostasisTimeAndTide(uint id, float delay)
        {
            ActorCast(id, _module.Nymeia, AID.Hydrostasis, delay, 4);
            ComponentCondition<Hydrostasis>(id + 0x10, 2.0f, comp => comp.Active)
                .ActivateOnEnter<Hydrostasis>();
            ActorCast(id + 0x20, _module.Althyk, AID.TimeAndTide, 0.1f, 10); // TODO: boss often dies here...
            ComponentCondition<Hydrostasis>(id + 0x30, 2, comp => comp.NumCasts >= 1, "Knockback 1");
            ComponentCondition<Hydrostasis>(id + 0x31, 3, comp => comp.NumCasts >= 2, "Knockback 2");
            ComponentCondition<Hydrostasis>(id + 0x32, 3, comp => comp.NumCasts >= 3, "Knockback 3")
                .DeactivateOnExit<Hydrostasis>();
        }
    }
}
