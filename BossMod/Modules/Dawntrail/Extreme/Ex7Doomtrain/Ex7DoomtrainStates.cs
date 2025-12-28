namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

class Ex7DoomtrainStates : StateMachineBuilder
{
    public Ex7DoomtrainStates(BossModule module) : base(module)
    {
        SimplePhase(0, P1, "P1")
            .ActivateOnEnter<CarGeometry>()
            .Raw.Update = () => Module.Enemies(OID.AetherIntermission).FirstOrDefault()?.IsDead == true;

        DeathPhase(1, P2);
    }

    void P1(uint id)
    {
        Car1(id, 8.2f);
        Car2(id + 0x10000, 5.9f);
        Car3_1(id + 0x20000, 5.7f);
        Intermission(id + 0x30000, 7);
    }

    void P2(uint id)
    {
        ComponentCondition<RunawayTrain>(id, 4.3f, r => r.Activation != default)
            .ActivateOnEnter<RunawayTrain>()
            .SetHint(StateMachine.StateHint.DowntimeStart);

        ComponentCondition<RunawayTrain>(id + 1, 15.2f, r => r.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<RunawayTrain>()
            .SetHint(StateMachine.StateHint.Raidwide)
            .ExecOnExit<CarGeometry>(c => c.Car = 3); // restore arena bounds

        Car3_2(id + 0x10, 5.1f);
        Car4(id + 0x10000, 5.2f);
        Car5(id + 0x20000, 18.3f);
        Car6(id + 0x30000, 10.3f);
    }

    void Car1(uint id, float delay)
    {
        CastStartMulti(id, [AID.DeadMansOverdraughtStack, AID.DeadMansOverdraughtSpread], delay)
            .ActivateOnEnter<Plasma>()
            .ActivateOnEnter<LevinSignal>()
            .ActivateOnEnter<DeadMansExpress>()
            .ActivateOnEnter<DeadMansWindpipe>()
            .ActivateOnEnter<DeadMansBlastpipe>();
        CastEnd(id + 1, 4);

        CastStartMulti(id + 0x10, [AID.DeadMansExpress, AID.DeadMansWindpipeBoss], 2.1f);

        Plasma(id + 0x20, 11);

        CastMulti(id + 0x30, [AID.DeadMansOverdraughtStack, AID.DeadMansOverdraughtSpread], 1.9f, 4)
            .ActivateOnEnter<Plasma>();

        CastStartMulti(id + 0x40, [AID.DeadMansExpress, AID.DeadMansWindpipeBoss], 2.1f);

        Plasma(id + 0x50, 10.9f);

        CastStart(id + 0x100, AID.UnlimitedExpressBoss, 1.9f)
            .ActivateOnEnter<UnlimitedExpress1>();
        ComponentCondition<UnlimitedExpress1>(id + 0x101, 5.9f, x => x.NumCasts > 0)
            .DeactivateOnExit<UnlimitedExpress1>();
        Targetable(id + 0x110, false, 0.2f, "Car 2 start");
    }

    void Car2(uint id, float delay)
    {
        Timeout(id, 0)
            .ActivateOnEnter<Electray>()
            .ExecOnEnter<CarGeometry>(c => c.Car++);

        Targetable(id + 1, true, delay, "Boss reappears");

        CastMulti(id + 0x10, [AID.DeadMansOverdraughtStack, AID.DeadMansOverdraughtSpread], 2, 4)
            .ActivateOnEnter<Plasma>();
        Cast(id + 0x20, AID.TurretCrossing, 2.1f, 3);

        CastStartMulti(id + 0x30, [AID.DeadMansExpress, AID.DeadMansWindpipeBoss], 4.1f);

        ComponentCondition<Electray>(id + 0x31, 5.2f, e => e.NumCasts >= 5, "Lasers");

        Plasma(id + 0x40, 5.9f);

        CastStartMulti(id + 0x50, [AID.DeadMansOverdraughtStack, AID.DeadMansOverdraughtSpread], 2)
            .ActivateOnEnter<Plasma>()
            .ActivateOnEnter<LightningBurst>();

        ComponentCondition<Electray>(id + 0x51, 8.8f, e => e.NumCasts >= 10, "Lasers");
        ComponentCondition<LightningBurst>(id + 0x52, 2.8f, l => l.NumCasts > 0, "Tankbusters")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .ExecOnEnter<LightningBurst>(l => l.EnableHints = true)
            .DeactivateOnExit<LightningBurst>();

        ComponentCondition<Electray>(id + 0x53, 8.7f, e => e.NumCasts >= 15, "Lasers")
            .DeactivateOnExit<Electray>();

        Plasma(id + 0x60, 5.9f);

        CastStart(id + 0x100, AID.UnlimitedExpressBoss, 2)
            .ActivateOnEnter<UnlimitedExpress1>();
        ComponentCondition<UnlimitedExpress1>(id + 0x101, 5.9f, x => x.NumCasts > 0)
            .DeactivateOnExit<UnlimitedExpress1>();
        Targetable(id + 0x110, false, 0.2f, "Car 2 start");
    }

    void Car3_1(uint id, float delay)
    {
        Timeout(id, 0).ExecOnEnter<CarGeometry>(c => c.Car++);

        Targetable(id + 1, true, delay, "Boss reappears");

        CastStart(id + 0x10, AID.LightningBurstCast, 4.1f)
            .ActivateOnEnter<LightningBurst>()
            .ExecOnEnter<LightningBurst>(b => b.EnableHints = true);

        ComponentCondition<LightningBurst>(id + 0x52, 5.6f, l => l.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<LightningBurst>();

        Cast(id + 0x100, AID.RunawayTrainStart, 7.7f, 5);

        Targetable(id + 0x102, false, 3.7f, "Intermission start");
    }

    void Intermission(uint id, float delay)
    {
        ActorTargetable(id, () => Module.Enemies(OID.AetherIntermission).FirstOrDefault(), true, delay, "Orb appears")
            .OnEnter(() =>
            {
                Module.Arena.Bounds = new ArenaBoundsCircle(14.5f);
                Module.Arena.Center = new(-400, -400);
            })
            .ActivateOnEnter<AetherialRay>()
            .ActivateOnEnter<MultiToot>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        // short vs long rotation differ by 1 second, so writing states isn't super useful here

        Timeout(id + 0x10, 75, "Orb enrage");
    }

    void Car3_2(uint id, float delay)
    {
        Timeout(id, delay, "Boss reappears")
            .ActivateOnEnter<Shockwave>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ComponentCondition<Shockwave>(id + 0x10, 13.1f, s => s.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<Shockwave>()
            .ActivateOnEnter<LightningBurst>();
        ComponentCondition<LightningBurst>(id + 0x20, 10, l => l.NumCasts > 0, "Tankbusters")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .ExecOnEnter<LightningBurst>(l => l.EnableHints = true)
            .DeactivateOnExit<LightningBurst>();

        ComponentCondition<DerailmentSiege>(id + 0x30, 8.6f, d => d.NumCasts > 0, "Tower 1")
            .ActivateOnEnter<Launchpad>()
            .ActivateOnEnter<Derail>()
            .ActivateOnEnter<DerailmentSiege>();
        ComponentCondition<DerailmentSiege>(id + 0x31, 2.9f, d => d.NumCasts > 2, "Tower 3")
            .ExecOnExit<CarGeometry>(c => c.MultiCar3());

        ComponentCondition<Derail>(id + 0x40, 5.2f, d => d.NumCasts > 0, "Derail")
            .DeactivateOnExit<Launchpad>()
            .DeactivateOnExit<DerailmentSiege>()
            .DeactivateOnExit<Derail>()
            .ExecOnExit<CarGeometry>(c => c.Car++);
    }

    void Car4(uint id, float delay)
    {
        ComponentCondition<ZoomCounter>(id, delay, z => z.NumCasts > 0)
            .ActivateOnEnter<ZoomCounter>();

        ThirdRail(id + 0x10, 10.1f);

        HeadlightBreath(id + 0x20, 10.2f);

        Cast(id + 0x100, AID.ArcaneRevelation, 3.5f, 2, "AR start")
            .ActivateOnEnter<HailOfThunder>()
            .ActivateOnEnter<DesignatedConductor>();

        ComponentCondition<HailOfThunder>(id + 0x110, 39.8f, h => h.NumCasts >= 3, "AR end")
            .DeactivateOnExit<HailOfThunder>()
            .DeactivateOnExit<DesignatedConductor>();

        HeadlightBreath(id + 0x200, 12.8f);
        ThirdRail(id + 0x210, 6.3f);

        ComponentCondition<LightningBurst>(id + 0x220, 8.6f, l => l.NumCasts > 0, "Tankbusters")
            .ActivateOnEnter<LightningBurst>()
            .SetHint(StateMachine.StateHint.Tankbuster)
            .ExecOnEnter<LightningBurst>(l => l.EnableHints = true)
            .DeactivateOnExit<LightningBurst>();

        ComponentCondition<DerailmentSiege>(id + 0x230, 10.7f, d => d.NumCasts > 0, "Tower 1")
            .ActivateOnEnter<Launchpad>()
            .ActivateOnEnter<Derail>()
            .ActivateOnEnter<DerailmentSiege>();
        ComponentCondition<DerailmentSiege>(id + 0x231, 3.9f, d => d.NumCasts > 3, "Tower 4")
            .ExecOnExit<CarGeometry>(c => c.MultiCar4());

        ComponentCondition<Derail>(id + 0x240, 6.2f, d => d.NumCasts > 0, "Derail")
            .DeactivateOnExit<Launchpad>()
            .DeactivateOnExit<DerailmentSiege>()
            .DeactivateOnExit<Derail>()
            .ExecOnExit<CarGeometry>(c => c.Car++);
    }

    void Car5(uint id, float delay)
    {
        ComponentCondition<Plummet>(id, delay, p => p.NumFinishedSpreads == 2, "Spreads 1")
            .ActivateOnEnter<Plummet>();
        ThirdRail(id + 0x10, 5);

        ComponentCondition<Plummet>(id + 0x20, 10.1f, p => p.NumFinishedSpreads == 5, "Spreads 2");

        ComponentCondition<LightningBurst>(id + 0x30, 6.6f, l => l.NumCasts > 0, "Tankbusters")
            .ActivateOnEnter<LightningBurst>()
            .SetHint(StateMachine.StateHint.Tankbuster)
            .ExecOnEnter<LightningBurst>(l => l.EnableHints = true)
            .DeactivateOnExit<LightningBurst>();

        ComponentCondition<Plummet>(id + 0x40, 9.6f, p => p.NumFinishedSpreads == 10, "Spreads 3")
            .DeactivateOnExit<Plummet>();

        ThirdRail(id + 0x50, 5);

        ComponentCondition<DerailmentSiege>(id + 0x60, 12.3f, d => d.NumCasts > 0, "Tower 1")
            .ActivateOnEnter<Launchpad>()
            .ActivateOnEnter<Derail>()
            .ActivateOnEnter<DerailmentSiege>();
        ComponentCondition<DerailmentSiege>(id + 0x61, 4.9f, d => d.NumCasts > 4, "Tower 5")
            .ExecOnExit<CarGeometry>(c => c.MultiCar5());

        ComponentCondition<Derail>(id + 0x70, 5.6f, d => d.NumCasts > 0, "Derail")
            .DeactivateOnExit<Launchpad>()
            .DeactivateOnExit<DerailmentSiege>()
            .DeactivateOnExit<Derail>()
            .ExecOnExit<CarGeometry>(c => c.Car++);
    }

    void Car6(uint id, float delay)
    {
        CastEnd(id, 0.3f);
        CastStartMulti(id + 1, [AID.DeadMansOverdraughtStack, AID.DeadMansOverdraughtSpread], delay)
            .ActivateOnEnter<Plasma>()
            .ActivateOnEnter<LevinSignal>();
        CastEnd(id + 2, 4);

        Cast(id + 0x10, AID.TurretCrossing, 2.1f, 3)
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<Breathlight>()
            .ExecOnEnter<Breathlight>(b => b.Draw = false);

        ComponentCondition<Electray>(id + 0x20, 9.2f, e => e.NumCasts > 0, "Lasers")
            .ExecOnExit<Breathlight>(b => b.Draw = true)
            .DeactivateOnExit<Electray>();
        HeadlightBreath(id + 0x21, 4, false);

        ComponentCondition<Electray>(id + 0x30, 9.5f, e => e.NumCasts > 0, "Lasers + crates disappear")
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<Plummet>()
            .DeactivateOnExit<Electray>();

        ComponentCondition<Plummet>(id + 0x40, 3.8f, p => p.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<Plummet>();

        ThirdRail(id + 0x50, 5);

        ComponentCondition<Electray>(id + 0x60, 8.3f, e => e.NumCasts > 0, "Lasers")
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<DeadMansExpress>()
            .ActivateOnEnter<DeadMansWindpipe>()
            .ActivateOnEnter<DeadMansBlastpipe>()
            .DeactivateOnExit<Electray>();

        Plasma(id + 0x62, 6.9f);

        ComponentCondition<LightningBurst>(id + 0x70, 7.5f, l => l.NumCasts > 0, "Tankbusters")
            .ActivateOnEnter<LightningBurst>()
            .SetHint(StateMachine.StateHint.Tankbuster)
            .ExecOnEnter<LightningBurst>(l => l.EnableHints = true)
            .DeactivateOnExit<LightningBurst>();

        CastMulti(id + 0x80, [AID.DeadMansOverdraughtSpread, AID.DeadMansOverdraughtStack], 3.5f, 4)
            .ActivateOnEnter<Plasma>();

        Cast(id + 0x100, AID.ArcaneRevelation, 2.1f, 2, "Short AR start")
            .ActivateOnEnter<HailOfThunder>()
            .ActivateOnEnter<DesignatedConductor>();

        ComponentCondition<HailOfThunder>(id + 0x110, 25.7f, h => h.NumCasts >= 2, "Short AR end")
            .DeactivateOnExit<HailOfThunder>()
            .DeactivateOnExit<DesignatedConductor>();

        ThirdRail(id + 0x120, 6.6f);

        Plasma(id + 0x131, 14.9f)
            .DeactivateOnExit<DeadMansBlastpipe>()
            .DeactivateOnExit<DeadMansExpress>()
            .DeactivateOnExit<DeadMansWindpipe>();

        HeadlightBreath(id + 0x200, 9.1f);

        ComponentCondition<LightningBurst>(id + 0x210, 9, l => l.NumCasts > 0, "Tankbusters")
            .ActivateOnEnter<LightningBurst>()
            .SetHint(StateMachine.StateHint.Tankbuster)
            .ExecOnEnter<LightningBurst>(l => l.EnableHints = true)
            .DeactivateOnExit<LightningBurst>();

        ThirdRail(id + 0x220, 5.5f);

        ComponentCondition<DerailmentSiege>(id + 0x300, 11, d => d.NumCasts > 0, "Tower 1").ActivateOnEnter<DerailmentSiege>();
        ComponentCondition<DerailmentSiege>(id + 0x310, 5.9f, d => d.NumCasts > 5, "Tower 6")
            .DeactivateOnExit<DerailmentSiege>();

        Cast(id + 0x10000, AID.DerailBossEnrage, 0.2f, 15.1f, "Enrage");
    }

    State Plasma(uint id, float delay)
    {
        return ComponentCondition<Plasma>(id, delay, p => p.NumCasts > 0, "Stack/spread", maxOverdue: 100)
            .DeactivateOnExit<Plasma>();
    }

    State ThirdRail(uint id, float delay)
    {
        return ComponentCondition<ThirdRail>(id, delay, t => t.NumCasts > 0, "Puddles")
            .ActivateOnEnter<ThirdRailBait>()
            .ActivateOnEnter<ThirdRail>()
            .DeactivateOnExit<ThirdRailBait>()
            .DeactivateOnExit<ThirdRail>();
    }

    void HeadlightBreath(uint id, float delay, bool activate = true)
    {
        ComponentCondition<Breathlight>(id, delay, b => b.NumCasts > 0, "Headlight/ground")
            .ActivateOnEnter<Breathlight>(activate);
        ComponentCondition<Breathlight>(id + 1, 2.5f, b => b.NumCasts > 1, "Headlight/ground")
            .DeactivateOnExit<Breathlight>();
    }
}
