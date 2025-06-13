namespace BossMod.Dawntrail.Foray.ForkedTower.FT03MarbleDragon;

class FT03MarbleDragonStates : StateMachineBuilder
{
    public FT03MarbleDragonStates(BossModule module) : base(module)
    {
        DeathPhase(0, Phase1);
    }

    private void Phase1(uint id)
    {
        CastStart(id, AID._Weaponskill_ImitationStar, 6.1f)
            .ActivateOnEnter<ImitationStar>();
        ComponentCondition<ImitationStar>(id + 1, 6.9f, s => s.NumCasts > 0, "Raidwide");

        DraconiformMotion(id + 0x100, 8.6f);
        Rain1(id + 0x10000, 5);
        Rain2(id + 0x20000, 5.8f);
        Golems(id + 0x30000, 9.7f);
        Sprites(id + 0x40000, 6.1f);

        Timeout(id + 0xFF0000, 9999, "???");
    }

    private State DraconiformMotion(uint id, float delay)
    {
        CastStart(id, AID._Weaponskill_DraconiformMotion, delay)
            .ActivateOnEnter<DraconiformHint>()
            .ActivateOnEnter<DraconiformMotion>()
            .DeactivateOnExit<DraconiformHint>();
        return ComponentCondition<DraconiformMotion>(id + 0x10, 4.8f, d => d.NumCasts > 0, "Baited AOE")
            .DeactivateOnExit<DraconiformMotion>();
    }

    private State DreadDeluge(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_DreadDeluge, delay, 3)
            .ActivateOnEnter<DreadDeluge>();

        return ComponentCondition<DreadDeluge>(id + 3, 2.1f, d => d.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<DreadDeluge>();
    }

    private void Rain1(uint id, float delay)
    {
        ComponentCondition<ImitationRain>(id, delay, i => i.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<ImitationRain>()
            .ActivateOnEnter<ImitationBlizzard1>()
            .DeactivateOnExit<ImitationRain>();

        Cast(id + 0x10, AID._Weaponskill_ImitationIcicle, 3.6f, 3);

        DraconiformMotion(id + 0x100, 4.4f)
            .ExecOnEnter<ImitationBlizzard1>(b => b.Enabled = true);

        ComponentCondition<ImitationBlizzard1>(id + 0x200, 4.3f, b => b.NumCasts > 0, "Rain 1 start");
        ComponentCondition<ImitationBlizzard1>(id + 0x210, 3.1f, b => b.NumCasts >= 8, "Rain 1 end")
            .DeactivateOnExit<ImitationBlizzard1>();

        DreadDeluge(id + 0x300, 8.1f);
    }

    private void Rain2(uint id, float delay)
    {
        ComponentCondition<ImitationRain>(id, delay, i => i.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<ImitationRain>()
            .ActivateOnEnter<ImitationBlizzard2>()
            .DeactivateOnExit<ImitationRain>();

        Cast(id + 0x10, AID._Weaponskill_FrigidTwister, 1.4f, 4)
            .ActivateOnEnter<IceTwister>();

        DraconiformMotion(id + 0x100, 4.3f)
            .ExecOnEnter<ImitationBlizzard2>(i => i.Enabled = true);

        ComponentCondition<ImitationBlizzard2>(id + 0x200, 3.7f, b => b.NumCasts > 0, "AOEs 1");
        ComponentCondition<ImitationBlizzard2>(id + 0x201, 4, b => b.NumCasts > 2, "AOEs 2");
        ComponentCondition<ImitationBlizzard2>(id + 0x202, 4, b => b.NumCasts >= 7, "AOEs 3")
            .DeactivateOnExit<ImitationBlizzard2>();

        DreadDeluge(id + 0x300, 3.5f);
    }

    private void Golems(uint id, float delay)
    {
        CastStart(id, AID._Ability_WitheringEternity, delay);

        ActorTargetable(id + 0x10, () => Module.PrimaryActor, false, 7f, "Boss disappears + adds appear")
            .ActivateOnEnter<ImitationRain>()
            .ActivateOnEnter<Golem>();

        ComponentCondition<ImitationRain>(id + 0x20, 0.7f, i => i.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<ImitationRain>();

        DiveTowers(id + 0x100, 12.2f);

        ComponentCondition<ImitationRain>(id + 0x200, 4.6f, i => i.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<ImitationRain>()
            .DeactivateOnExit<ImitationRain>();

        DiveTowers(id + 0x300, 11.4f);
    }

    private void DiveTowers(uint id, float delay)
    {
        ComponentCondition<FrigidDive>(id, delay, f => f.NumCasts > 0, "Dive")
            .ActivateOnEnter<FrigidDive>()
            .DeactivateOnExit<FrigidDive>()
            .ActivateOnEnter<Rain3>()
            .ActivateOnEnter<Rain3Towers>()
            .ActivateOnEnter<Rain3Cross>();

        ComponentCondition<Rain3Towers>(id + 0x10, 4.2f, t => t.NumCasts > 0, "Cross + towers 1")
            .DeactivateOnExit<Rain3Cross>();
        ComponentCondition<Rain3Towers>(id + 0x20, 4, t => t.NumCasts > 2, "Towers 2")
            .DeactivateOnExit<Rain3>()
            .DeactivateOnExit<Rain3Towers>();
    }

    private void Sprites(uint id, float delay)
    {
        Targetable(id, true, delay, "Boss reappears");

        ComponentCondition<IceSprite>(id + 0x10, 1, s => s.ActiveActors.Any(), "Sprites appear")
            .ActivateOnEnter<IceSprite>()
            .ActivateOnEnter<LifelessLegacy>();

        ComponentCondition<LifelessLegacy>(id + 0x20, 36.7f, l => l.NumCasts > 0, "Raidwide + sprites enrage")
            .DeactivateOnExit<IceSprite>()
            .DeactivateOnExit<LifelessLegacy>();

        Cast(id + 0x100, AID._Weaponskill_WickedWater, 8.5f, 4, "wicked water");
    }
}
