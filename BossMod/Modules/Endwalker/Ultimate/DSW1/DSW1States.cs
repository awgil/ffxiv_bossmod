namespace BossMod.Endwalker.Ultimate.DSW1;

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
        return actor == null || actor.IsDestroyed || actor.HP.Cur < actor.HP.Max && !actor.IsTargetable;
    }

    private void MainPhase(uint id)
    {
        HoliestOfHoly(id, 5.2f);
        Heavensblaze(id + 0x10000, 8.2f);
        HyperdimensionalSlash(id + 0x20000, 10.4f);
        ShiningBlade(id + 0x30000, 3.9f);
        HoliestHallowing(id + 0x40000, 2.7f, true);
        Heavensflame(id + 0x50000, 4.9f);
        HoliestHallowing(id + 0x60000, 1.6f, true);
        EmptyFullDimension(id + 0x70000, 4.1f);
        HoliestHallowing(id + 0x80000, 5.1f, false);
        HoliestOfHoly(id + 0x90000, 5);
        AdelphelGrinnauxEnrage(id + 0xA0000, 2.2f);
    }

    private void PureHeartPhase(uint id)
    {
        // TODO: do we care about shockwaves?..
        ActorTargetable(id, _module.SerCharibert, false, 0);
        ActorTargetable(id + 1, _module.SerCharibert, true, 4, "Appear");
        ActorCastStart(id + 2, _module.SerCharibert, AID.PureOfHeart, 0.1f, true)
            .ActivateOnEnter<PureOfHeartBrightwing>()
            .ActivateOnEnter<PureOfHeartSkyblindBait>()
            .ActivateOnEnter<PureOfHeartSkyblind>();
        ComponentCondition<PureOfHeartBrightwing>(id + 0x10, 15.4f, comp => comp.NumCasts > 0, "Cone 1");
        ComponentCondition<PureOfHeartBrightwing>(id + 0x20, 5, comp => comp.NumCasts > 2, "Cone 2");
        ComponentCondition<PureOfHeartBrightwing>(id + 0x30, 5, comp => comp.NumCasts > 4, "Cone 3");
        ComponentCondition<PureOfHeartBrightwing>(id + 0x40, 5, comp => comp.NumCasts > 6, "Cone 4")
            .DeactivateOnExit<PureOfHeartBrightwing>();
        ActorCastEnd(id + 0x50, _module.SerCharibert, 5, true, "Raidwide");
        ActorTargetable(id + 0x60, _module.SerCharibert, false, 2.1f, "Disappear");
    }

    private State HoliestOfHoly(uint id, float delay)
    {
        return ActorCast(id, _module.SerAdelphel, AID.HoliestOfHoly, delay, 4, false, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Heavensblaze(uint id, float delay)
    {
        ActorCast(id, _module.SerGrinnaux, AID.EmptyDimension, delay, 5, false, "Donut + Tankbuster")
            .ActivateOnEnter<EmptyDimension>()
            .ActivateOnEnter<HolyShieldBash>()
            .ActivateOnEnter<HolyBladedance>()
            .ActivateOnEnter<Heavensblaze>()
            .DeactivateOnExit<EmptyDimension>()
            .DeactivateOnExit<HolyShieldBash>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ActorCast(id + 0x10, _module.SerCharibert, AID.Heavensblaze, 0.1f, 5, false, "Stack")
            .DeactivateOnExit<HolyBladedance>()
            .DeactivateOnExit<Heavensblaze>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void HyperdimensionalSlash(uint id, float delay)
    {
        ActorCastStart(id, _module.SerGrinnaux, AID.HyperdimensionalSlash, delay, false, "Adelphel disappear")
            .ActivateOnEnter<HyperdimensionalSlash>() // icons appear just before cast start
            .SetHint(StateMachine.StateHint.PositioningStart);
        ActorCastEnd(id + 1, _module.SerGrinnaux, 5);
        ComponentCondition<HyperdimensionalSlash>(id + 0x10, 1.1f, comp => comp.NumCasts >= 1, "Slash 1");
        ComponentCondition<HyperdimensionalSlash>(id + 0x11, 7.1f, comp => comp.NumCasts >= 2, "Slash 2")
            .DeactivateOnExit<HyperdimensionalSlash>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    // leaves positioning hint at the end, since tanks need to move bosses after this stage
    private void ShiningBlade(uint id, float delay)
    {
        ActorCastStart(id, _module.SerGrinnaux, AID.FaithUnmoving, delay)
            .ActivateOnEnter<ShiningBladeKnockback>()
            .ActivateOnEnter<ShiningBladeFlares>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ActorCastEnd(id + 1, _module.SerGrinnaux, 4, false, "Knockback");
        ActorCastEnd(id + 2, _module.SerAdelphel, 1.1f, false, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide); // holiest-of-holy overlap
        ComponentCondition<ShiningBladeExecution>(id + 8, 7.9f, comp => comp.NumCasts > 0, "Execution")
            .ActivateOnEnter<ShiningBladeExecution>()
            .DeactivateOnExit<ShiningBladeExecution>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<ShiningBladeFlares>(id + 0x10, 0.6f, comp => comp.Done, "Charges")
            .DeactivateOnExit<ShiningBladeKnockback>() // note: we keep this component alive, because it shows tears
            .DeactivateOnExit<ShiningBladeFlares>();
    }

    // this often happens when pos flag is set
    private void HoliestHallowing(uint id, float delay, bool clearPosFlag)
    {
        ActorCastStart(id, _module.SerAdelphel, AID.HoliestHallowing, delay);

        var castEnd = SimpleState(id + 1, 4, "Heal") // note: we use custom state instead of cast-end, since cast-end happens whenever anyone presses interrupt - and if not interrupted, spell finish can be slightly delayed
            .ActivateOnEnter<HoliestHallowing>()
            .DeactivateOnExit<HoliestHallowing>()
            .SetHint(StateMachine.StateHint.PositioningEnd, clearPosFlag);
        castEnd.Raw.Comment = "Interruptible cast end";
        castEnd.Raw.Update = timeSinceTransition => _module.SerAdelphel()?.CastInfo == null && timeSinceTransition >= castEnd.Raw.Duration ? 0 : -1;
    }

    // leaves positioning hint at the end, since tanks need to move bosses after this stage
    private void Heavensflame(uint id, float delay)
    {
        ActorCastStart(id, _module.SerCharibert, AID.Heavensflame, delay)
            .ActivateOnEnter<HeavensflameKnockback>() // icons appear just before cast start
            .SetHint(StateMachine.StateHint.PositioningStart);
        ActorCast(id + 0x10, _module.SerGrinnaux, AID.FaithUnmoving, 1.1f, 4, false, "Knockback");
        ActorCastEnd(id + 0x20, _module.SerCharibert, 1.9f);
        ComponentCondition<HeavensflameAOE>(id + 0x30, 0.6f, comp => comp.NumCasts > 0, "Heavensflame")
            .ActivateOnEnter<HeavensflameAOE>()
            .DeactivateOnExit<HeavensflameAOE>()
            .DeactivateOnExit<HeavensflameKnockback>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void EmptyFullDimension(uint id, float delay)
    {
        HoliestOfHoly(id, delay)
            .ActivateOnEnter<EmptyDimension>()
            .ActivateOnEnter<FullDimension>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ActorCastEnd(id + 0x10, _module.SerGrinnaux, 2, false, "Donut/circle") // holiest-of-holy overlap
            .DeactivateOnExit<EmptyDimension>()
            .DeactivateOnExit<FullDimension>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
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
            return adelphelCast || grinnauxCast ? 0 : -1;
        };

        var castEnd = SimpleState(id + 1, 3, "Adelphel/Grinnaux Enrage");
        castEnd.Raw.Comment = "Adelphel/Grinnaux enrage end";
        castEnd.Raw.Update = _ =>
        {
            bool adelphelDone = _module.SerAdelphel()?.CastInfo == null;
            bool grinnauxDone = _module.SerGrinnaux()?.CastInfo == null;
            return adelphelDone && grinnauxDone ? 0 : -1;
        };
    }
}
