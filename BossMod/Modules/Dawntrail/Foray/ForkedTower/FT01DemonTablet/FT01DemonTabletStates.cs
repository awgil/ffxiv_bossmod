namespace BossMod.Dawntrail.Foray.ForkedTower.FT01DemonTablet;

class FT01DemonTabletStates : StateMachineBuilder
{
    public FT01DemonTabletStates(BossModule module) : base(module)
    {
        DeathPhase(0, Phase1)
            .ActivateOnEnter<BossCollision>();
    }

    private void Phase1(uint id)
    {
        Cast(id, AID.DemonicDarkIICast, 4.2f, 5, "Raidwide")
            .ActivateOnEnter<DemonicDarkII>();

        RayOfIgnorance(id + 0x100, 6.2f, "Safe side 1");
        RayOfIgnorance(id + 0x200, 2.7f, "Safe side 2");

        Cast(id + 0x300, AID.OccultChiselCast, 5.7f, 8.5f, "Tankbusters")
            .ActivateOnEnter<OccultChisel>()
            .DeactivateOnExit<OccultChisel>();

        Towers1(id + 0x10000, 6.2f);

        Rotate(id + 0x20000, 5.1f);
        Meteors(id + 0x30000, 9.3f);

        Rotate(id + 0x40000, 5.7f);
        Adds(id + 0x50000, 9.3f);
        GravityTowers(id + 0x60000, 41.3f);

        Rotate(id + 0x70000, 6.1f);

        Timeout(id + 0xFF0000, 9999, "???");
    }

    private State CloseFar(uint id, float delay)
    {
        return CastStartMulti(id, [AID.RayOfDangersNear, AID.RayOfExpulsionAfar, AID.DemonographOfDangersNear, AID.DemonographOfExpulsionAfar], delay)
            .ActivateOnEnter<LandingBoss>()
            .ActivateOnEnter<LandingNear>()
            .ActivateOnEnter<LandingKnockback>();
    }

    private void RayOfIgnorance(uint id, float delay, string title)
    {
        CloseFar(id, delay)
            .ActivateOnEnter<RayOfIgnorance>();

        ComponentCondition<RayOfIgnorance>(id + 0x10, 10.5f, r => r.NumCasts > 0, title)
            .DeactivateOnExit<LandingBoss>()
            .DeactivateOnExit<LandingNear>()
            .DeactivateOnExit<LandingKnockback>()
            .DeactivateOnExit<RayOfIgnorance>();
    }

    private void Towers1(uint id, float delay)
    {
        CloseFar(id, delay).ActivateOnEnter<Explosion>();
        ComponentCondition<LandingBoss>(id + 0x10, 10.5f, l => l.NumCasts > 0, "In/out")
            .ExecOnExit<Explosion>(e => e.EnableHints = true)
            .DeactivateOnExit<LandingBoss>()
            .DeactivateOnExit<LandingNear>()
            .DeactivateOnExit<LandingKnockback>();
        ComponentCondition<Explosion>(id + 0x20, 4.5f, e => e.NumCasts > 0, "Towers")
            .DeactivateOnExit<Explosion>();
    }

    private void Rotate(uint id, float delay)
    {
        CastStartMulti(id, [AID.RotateLeft, AID.RotateRight], delay)
            .ActivateOnEnter<Rotation>()
            .ActivateOnEnter<Rotation1>();

        ComponentCondition<Rotation>(id + 0x10, 8.8f, r => r.NumCasts > 0, "Rotation 1")
            .DeactivateOnExit<Rotation>()
            .DeactivateOnExit<Rotation1>();

        CastStartMulti(id + 0x100, [AID.RotateLeft, AID.RotateRight], 3.3f)
            .ActivateOnEnter<Rotation>()
            .ActivateOnEnter<Rotation1>()
            .ActivateOnEnter<LacunateStream>();

        ComponentCondition<Rotation>(id + 0x110, 8.8f, r => r.NumCasts > 1, "Rotation 2")
            .DeactivateOnExit<Rotation>()
            .DeactivateOnExit<Rotation1>();

        ComponentCondition<LacunateStream>(id + 0x120, 4.2f, l => l.NumCasts > 0, "Safe side")
            .DeactivateOnExit<LacunateStream>();
    }

    private void Meteors(uint id, float delay)
    {
        ComponentCondition<PortentousComet>(id, delay, c => c.ActiveSpreads.Any(), "Meteors appear")
            .ActivateOnEnter<PortentousComet>()
            .ActivateOnEnter<PortentousCometeor>()
            .ActivateOnEnter<PortentousCometKB>()
            .ActivateOnEnter<LandingBoss>()
            .ActivateOnEnter<LandingNear>()
            .ActivateOnEnter<LandingKnockback>();

        ComponentCondition<LandingBoss>(id + 1, 10.4f, l => l.NumCasts > 0, "In/out")
            .DeactivateOnExit<LandingBoss>()
            .DeactivateOnExit<LandingNear>()
            .DeactivateOnExit<LandingKnockback>();

        ComponentCondition<PortentousComet>(id + 0x10, 1.6f, c => !c.ActiveSpreads.Any(), "Meteor target resolve")
            .ExecOnExit<PortentousComet>(e => e.EnableHints = true);

        ComponentCondition<PortentousCometKB>(id + 0x20, 5.1f, c => c.NumCasts > 0, "Stacks")
            .ExecOnExit<PortentousCometeor>(c => c.Risky = true)
            .DeactivateOnExit<PortentousComet>()
            .DeactivateOnExit<PortentousCometKB>();

        ComponentCondition<PortentousCometeor>(id + 0x30, 7, c => c.NumCasts > 0, "Meteor AOEs")
            .DeactivateOnExit<PortentousCometeor>();
    }

    private void Adds(uint id, float delay)
    {
        Cast(id, AID.SummonRect, delay, 10)
            .ActivateOnEnter<Summon>()
            .ActivateOnEnter<Summons>();

        ComponentCondition<Summons>(id + 0x10, 0.9f, s => s.ActiveActors.Any(), "Adds appear")
            .ActivateOnEnter<DarkDefenses>();
    }

    private void GravityTowers(uint id, float delay)
    {
        Cast(id, AID.Summon, delay, 4, "Statues appear");

        Cast(id + 0x10, AID.Demonography, 2.1f, 4)
            .ActivateOnEnter<GravityExplosion>()
            .ActivateOnEnter<LandingNear>()
            .ActivateOnEnter<LandingKnockback>()
            .ActivateOnEnter<LandingBoss>()
            .ActivateOnEnter<EraseGravity>();

        ComponentCondition<LandingBoss>(id + 0x20, 12.6f, b => b.NumCasts > 0, "In/out")
            .DeactivateOnExit<LandingNear>()
            .DeactivateOnExit<LandingKnockback>()
            .DeactivateOnExit<LandingBoss>()
            .ExecOnExit<EraseGravity>(e => e.Risky = true);

        ComponentCondition<EraseGravity>(id + 0x30, 3.5f, e => e.NumCasts > 0, "Statues activate")
            .DeactivateOnExit<EraseGravity>()
            .ExecOnExit<GravityExplosion>(g => g.EnableHints = true);

        ComponentCondition<GravityExplosion>(id + 0x40, 5.7f, g => g.NumCasts > 0, "Towers")
            .DeactivateOnExit<GravityExplosion>();

        ComponentCondition<LandingStatue>(id + 0x50, 6.3f, l => l.NumCasts > 0, "Statue AOEs")
            .ActivateOnEnter<LandingStatue>();
    }
}
