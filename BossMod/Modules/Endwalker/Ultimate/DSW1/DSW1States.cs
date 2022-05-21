namespace BossMod.Endwalker.Ultimate.DSW1
{
    class DSW1States : StateMachineBuilder
    {
        private DSW1 _module;

        public DSW1States(DSW1 module) : base(module)
        {
            _module = module;
            SimplePhase(0, MainPhase, "Main")
                .Raw.Update = () => ActorKilled(_module.SerAdelphel()) && ActorKilled(_module.SerGrinnaux());
            SimplePhase(1, PureHeartPhase, "Pure Heart")
                .Raw.Update = () => ActorKilled(_module.SerCharibert());
        }

        private bool ActorKilled(Actor? actor)
        {
            return actor == null || actor.IsDestroyed || actor.HPCur < actor.HPMax && !actor.IsTargetable;
        }

        private void MainPhase(uint id)
        {
            HoliestOfHoly(id, 5.2f);
            Heavensblaze(id + 0x10000, 8.2f);
            HyperdimensionalSlash(id + 0x20000, 10.4f);
            ShiningBlade(id + 0x30000, 3.9f);
            HoliestHallowing(id + 0x40000, 2.7f);
            Heavensflame(id + 0x50000, 4.9f);
            HoliestHallowing(id + 0x60000, 1.6f);
            EmptyFullDimension(id + 0x70000, 4.1f);
            HoliestHallowing(id + 0x80000, 5.1f);
            HoliestOfHoly(id + 0x90000, 5);
            AdelphelGrinnauxEnrage(id + 0xA0000, 2.2f);
        }

        private void PureHeartPhase(uint id)
        {
            // TODO: do we care about shockwaves?..
            ActorTargetable(id, _module.SerCharibert, false, 0);
            ActorTargetable(id + 1, _module.SerCharibert, true, 4, "Appear");
            ActorCastStart(id + 2, _module.SerCharibert, AID.PureOfHeart, 0.1f)
                .ActivateOnEnter<PureOfHeart>();
            ComponentCondition<PureOfHeart>(id + 0x10, 15.4f, comp => comp.NumCasts > 0, "Cone 1");
            ComponentCondition<PureOfHeart>(id + 0x20, 5, comp => comp.NumCasts > 2, "Cone 2");
            ComponentCondition<PureOfHeart>(id + 0x30, 5, comp => comp.NumCasts > 4, "Cone 3");
            ComponentCondition<PureOfHeart>(id + 0x40, 5, comp => comp.NumCasts > 6, "Cone 4");
            ActorCastEnd(id + 0x50, _module.SerCharibert, 5, "Raidwide");
            ActorTargetable(id + 0x60, _module.SerCharibert, false, 2.1f, "Disappear");
        }

        private State HoliestOfHoly(uint id, float delay)
        {
            return ActorCast(id, _module.SerAdelphel, AID.HoliestOfHoly, delay, 4, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide);
        }

        private void Heavensblaze(uint id, float delay)
        {
            ActorCast(id, _module.SerGrinnaux, AID.EmptyDimension, delay, 5, "Donut")
                .ActivateOnEnter<EmptyDimension>()
                .ActivateOnEnter<Heavensblaze>()
                .DeactivateOnExit<EmptyDimension>();
            ActorCast(id + 0x10, _module.SerCharibert, AID.Heavensblaze, 0.1f, 5, "Tankbuster + Stack")
                .DeactivateOnExit<Heavensblaze>();
        }

        private void HyperdimensionalSlash(uint id, float delay)
        {
            ActorCastStart(id, _module.SerGrinnaux, AID.HyperdimensionalSlash, delay)
                .ActivateOnEnter<HyperdimensionalSlash>(); // icons appear just before cast start
            ActorCastEnd(id + 1, _module.SerGrinnaux, 5);
            ComponentCondition<HyperdimensionalSlash>(id + 2, 8.2f, comp => comp.NumCasts >= 2, "Slash")
                .DeactivateOnExit<HyperdimensionalSlash>();
        }

        private void ShiningBlade(uint id, float delay)
        {
            ActorCast(id, _module.SerGrinnaux, AID.FaithUnmoving, delay, 4, "Knockback")
                .ActivateOnEnter<ShiningBlade>();
            ActorCastEnd(id + 2, _module.SerAdelphel, 1.1f, "Raidwide")
                .SetHint(StateMachine.StateHint.Raidwide); // holiest-of-holy overlap
            ComponentCondition<ShiningBlade>(id + 0x10, 8.5f, comp => comp.Done, "Resolve")
                .DeactivateOnExit<ShiningBlade>();
        }

        private void HoliestHallowing(uint id, float delay)
        {
            ActorCastStart(id, _module.SerAdelphel, AID.HoliestHallowing, delay);

            var castEnd = SimpleState(id + 1, 4, "Heal") // note: we use custom state instead of cast-end, since cast-end happens whenever anyone presses interrupt - and if not interrupted, spell finish can be slightly delayed
                .ActivateOnEnter<HoliestHallowing>()
                .DeactivateOnExit<HoliestHallowing>();
            castEnd.Raw.Comment = "Interruptible cast end";
            castEnd.Raw.Update = timeSinceTransition => _module.SerAdelphel()?.CastInfo == null && timeSinceTransition >= castEnd.Raw.Duration ? castEnd.Raw.Next : null;
        }

        private void Heavensflame(uint id, float delay)
        {
            ActorCastStart(id, _module.SerCharibert, AID.Heavensflame, delay)
                .ActivateOnEnter<Heavensflame>(); // icons appear just before cast start
            ActorCastEnd(id + 1, _module.SerCharibert, 7);
            ComponentCondition<Heavensflame>(id + 2, 0.6f, comp => comp.NumCasts > 0, "Heavensflame")
                .DeactivateOnExit<Heavensflame>();
        }

        private void EmptyFullDimension(uint id, float delay)
        {
            HoliestOfHoly(id, delay)
                .ActivateOnEnter<EmptyDimension>()
                .ActivateOnEnter<FullDimension>();
            ActorCastEnd(id + 0x10, _module.SerGrinnaux, 2, "Donut/circle") // holiest-of-holy overlap
                .DeactivateOnExit<EmptyDimension>()
                .DeactivateOnExit<FullDimension>();
        }

        private void AdelphelGrinnauxEnrage(uint id, float delay)
        {
            // if timeout is reached, both start their cast, otherwise as soon as any dies, remaining starts the cast
            var castStart = SimpleState(id, delay, "");
            castStart.Raw.Comment = $"Adelphel/Grinnaux enrage start";
            castStart.Raw.Update = _ =>
            {
                bool adelphelCast = _module.SerAdelphel()?.CastInfo?.IsSpell(AID.BrightbladesSteel) ?? false;
                bool grinnauxCast = _module.SerGrinnaux()?.CastInfo?.IsSpell(AID.TheBullsSteel) ?? false;
                return adelphelCast || grinnauxCast ? castStart.Raw.Next : null;
            };

            var castEnd = SimpleState(id + 1, 3, "Adelphel/Grinnaux Enrage");
            castEnd.Raw.Comment = "Adelphel/Grinnaux enrage end";
            castEnd.Raw.Update = _ =>
            {
                bool adelphelDone = _module.SerAdelphel()?.CastInfo == null;
                bool grinnauxDone = _module.SerGrinnaux()?.CastInfo == null;
                return adelphelDone && grinnauxDone ? castEnd.Raw.Next : null;
            };
        }
    }
}
