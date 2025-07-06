namespace BossMod.Dawntrail.Foray.ForkedTower.FT03MarbleDragon;

class FT03MarbleDragonStates : StateMachineBuilder
{
    public FT03MarbleDragonStates(BossModule module) : base(module)
    {
        DeathPhase(0, Phase1);
    }

    private void Phase1(uint id)
    {
        ImitationStar(id, 6.1f);
        DraconiformMotion(id + 0x100, 8.6f);
        Rain1(id + 0x10000, 5);
        Rain2(id + 0x20000, 5.8f);
        Golems(id + 0x30000, 9.5f);
        Sprites(id + 0x40000, 6);
        Rain3(id + 0x50000, 8.5f);
        Rain4(id + 0x60000, 10.7f);
        Enrage(id + 0x70000, 7.7f);
    }

    private State ImitationStar(uint id, float delay)
    {
        CastStart(id, AID.ImitationStarCast, delay)
            .ActivateOnEnter<ImitationStar>();
        return ComponentCondition<ImitationStar>(id + 1, 6.8f, s => s.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<ImitationStar>();
    }

    private State DraconiformMotion(uint id, float delay)
    {
        CastStart(id, AID.DraconiformMotionCast, delay)
            .ActivateOnEnter<DraconiformHint>()
            .ActivateOnEnter<DraconiformMotion>()
            .DeactivateOnExit<DraconiformHint>();
        return ComponentCondition<DraconiformMotion>(id + 0x10, 4.8f, d => d.NumCasts > 0, "Baited AOE")
            .DeactivateOnExit<DraconiformMotion>();
    }

    private State DreadDeluge(uint id, float delay)
    {
        Cast(id, AID.DreadDelugeCast, delay, 3)
            .ActivateOnEnter<DreadDeluge>();

        return ComponentCondition<DreadDeluge>(id + 3, 2, d => d.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<DreadDeluge>();
    }

    private void Rain1(uint id, float delay)
    {
        ComponentCondition<ImitationRain>(id, delay, i => i.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<ImitationRain>()
            .ActivateOnEnter<ImitationBlizzard1>()
            .DeactivateOnExit<ImitationRain>();

        Cast(id + 0x10, AID.ImitationIcicle, 3.6f, 3);

        DraconiformMotion(id + 0x100, 4.4f)
            .ActivateOnEnter<ImitationIcicle>()
            .ExecOnEnter<ImitationBlizzard1>(b => b.Enabled = true)
            .DeactivateOnExit<ImitationIcicle>();

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

        Cast(id + 0x10, AID.FrigidTwisterCast, 1.4f, 4)
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
        CastStart(id, AID.WitheringEternity, delay);

        ActorTargetable(id + 0x10, () => Module.PrimaryActor, false, 7, "Boss disappears + adds appear")
            .ActivateOnEnter<ImitationRain>()
            .ActivateOnEnter<Golem>();

        ComponentCondition<ImitationRain>(id + 0x20, 0.7f, i => i.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<ImitationRain>();

        DiveTowers(id + 0x100, 12.2f);

        ComponentCondition<ImitationRain>(id + 0x200, 4.6f, i => i.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<ImitationRain>()
            .DeactivateOnExit<ImitationRain>();

        DiveTowers(id + 0x300, 11.4f)
            .DeactivateOnExit<Golem>();
    }

    private State DiveTowers(uint id, float delay)
    {
        ComponentCondition<FrigidDive>(id, delay, f => f.NumCasts > 0, "Dive")
            .ActivateOnEnter<FrigidDive>()
            .DeactivateOnExit<FrigidDive>()
            .ActivateOnEnter<AddsTowersPre>()
            .ActivateOnEnter<AddsTowers>()
            .ActivateOnEnter<AddsCross>();

        ComponentCondition<AddsTowers>(id + 0x10, 4.2f, t => t.NumCasts > 0, "Cross + towers 1")
            .DeactivateOnExit<AddsCross>();
        return ComponentCondition<AddsTowers>(id + 0x20, 4, t => t.NumCasts > 2, "Towers 2")
            .DeactivateOnExit<AddsTowersPre>()
            .DeactivateOnExit<AddsTowers>();
    }

    private void Sprites(uint id, float delay)
    {
        Targetable(id, true, delay, "Boss reappears");

        CastStart(id + 0x0A, AID.LifelessLegacyCast, 1)
            .ActivateOnEnter<IceSprite>()
            .ActivateOnEnter<SpriteInvincible>()
            .ActivateOnEnter<LifelessLegacy>();

        ComponentCondition<IceSprite>(id + 0x10, 0.1f, s => s.ActiveActors.Any(), "Sprites appear");

        ComponentCondition<LifelessLegacy>(id + 0x20, 36.7f, l => l.NumCasts > 0, "Raidwide + sprites enrage")
            .DeactivateOnExit<IceSprite>()
            .DeactivateOnExit<SpriteInvincible>()
            .DeactivateOnExit<LifelessLegacy>();
    }

    private void Rain3(uint id, float delay)
    {
        Cast(id, AID.WickedWater, delay, 4)
            .ActivateOnEnter<ImitationBlizzard3>();

        Cast(id + 0x10, AID.ImitationIcicle, 6.3f, 3);

        DraconiformMotion(id + 0x100, 4.4f)
            .ExecOnEnter<ImitationBlizzard3>(b => b.Enabled = true);

        ComponentCondition<ImitationBlizzard3>(id + 0x200, 4.2f, b => b.NumCasts > 0, "Rain 1 start");
        ComponentCondition<ImitationBlizzard3>(id + 0x210, 3, b => b.NumCasts >= 8, "Rain 1 end")
            .DeactivateOnExit<ImitationBlizzard3>()
            .ActivateOnEnter<GelidGaol>();

        ComponentCondition<GelidGaol>(id + 0x220, 0.4f, g => g.Actors.Count > 0, "Ice cubes appear");
    }

    private void Rain4(uint id, float delay)
    {
        ImitationStar(id, delay)
            .DeactivateOnExit<GelidGaol>();

        Cast(id + 0x100, AID.FrigidTwisterCast, 13.4f, 4)
            .ActivateOnEnter<ImitationBlizzard4>();

        DraconiformMotion(id + 0x200, 4.3f)
            .ActivateOnEnter<B4TowerFreeze>()
            .ExecOnEnter<ImitationBlizzard4>(b => b.Enabled = true);

        ComponentCondition<ImitationBlizzard4>(id + 0x220, 3.8f, b => b.NumCasts > 0, "AOEs 1")
            .ExecOnExit<ImitationBlizzard4>(b => b.Risky = false);

        ComponentCondition<B4TowerFreeze>(id + 0x230, 0.6f, p => p.NumCasts > 0, "Towers freeze")
            .DeactivateOnExit<B4TowerFreeze>();

        ComponentCondition<B4Tower>(id + 0x240, 3.5f, b => b.NumCasts > 0, "Towers activate")
            .ActivateOnEnter<B4Tower>()
            .DeactivateOnExit<B4Tower>()
            .ExecOnExit<ImitationBlizzard4>(b => b.Risky = true);

        ComponentCondition<ImitationBlizzard4>(id + 0x250, 6, b => b.NumCasts > 2, "AOEs 2")
            .DeactivateOnExit<ImitationBlizzard4>();

        DraconiformMotion(id + 0x300, 3.9f)
            .ActivateOnEnter<ImitationBlizzard4>()
            .ActivateOnEnter<B4TowerFreeze>()
            .ExecOnEnter<ImitationBlizzard4>(b => b.Enabled = true);

        ComponentCondition<ImitationBlizzard4>(id + 0x320, 1.6f, b => b.NumCasts > 0, "AOEs 3")
            .ExecOnExit<ImitationBlizzard4>(b => b.Risky = false);

        ComponentCondition<B4TowerFreeze>(id + 0x330, 0.6f, p => p.NumCasts > 0, "Towers freeze")
            .DeactivateOnExit<B4TowerFreeze>();

        ComponentCondition<B4Tower>(id + 0x340, 3.5f, b => b.NumCasts > 0, "Towers activate")
            .ActivateOnEnter<B4Tower>()
            .DeactivateOnExit<B4Tower>()
            .ExecOnExit<ImitationBlizzard4>(b => b.Risky = true);

        ComponentCondition<ImitationBlizzard4>(id + 0x350, 6, b => b.NumCasts > 2, "AOEs 4")
            .DeactivateOnExit<ImitationBlizzard4>();
    }

    private void Enrage(uint id, float delay)
    {
        ImitationStar(id, delay);
        DreadDeluge(id + 0x100, 6.4f);
        Cast(id + 0x200, AID.LifelessLegacyEnrage, 12.3f, 21.6f, "Enrage");
    }
}
