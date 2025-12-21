namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

class Ex7DoomtrainStates : StateMachineBuilder
{
    public Ex7DoomtrainStates(BossModule module) : base(module)
    {
        DeathPhase(0, P1)
            .ActivateOnEnter<CarCounter>();
    }

    void P1(uint id)
    {
        Car1(id, 8.2f);
        Car2(id + 0x10000, 5.9f);
        Car3(id + 0x20000, 5.7f);
        Intermission(id + 0x30000, 7);

        Timeout(id + 0xFF0000, 10000, "???");
    }

    void Plasma(uint id, float delay)
    {
        ComponentCondition<Plasma>(id, delay, p => p.NumCasts > 0, "Stack/spread")
            .ExecOnExit<Plasma>(p => p.Reset());
    }

    void Car1(uint id, float delay)
    {
        CastStartMulti(id, [AID.DeadMansOverdraughtStack, AID.DeadMansOverdraughtSpread], delay)
            .ActivateOnEnter<Plasma>()
            .ActivateOnEnter<LevinSignal>()
            .ActivateOnEnter<DeadMansExpress>()
            .ActivateOnEnter<DeadMansWindpipe>()
            .ActivateOnEnter<DeadMansBlastpipe>()
            .ActivateOnEnter<PushPullCounter>();
        CastEnd(id + 1, 4);

        CastStartMulti(id + 0x10, [AID.DeadMansExpress, AID.DeadMansWindpipeBoss], 2.1f);
        ComponentCondition<PushPullCounter>(id + 0x11, 6, p => p.NumCasts > 0, "Push/pull");

        Plasma(id + 0x20, 5.1f);

        CastMulti(id + 0x30, [AID.DeadMansOverdraughtStack, AID.DeadMansOverdraughtSpread], 1.9f, 4);

        CastStartMulti(id + 0x40, [AID.DeadMansExpress, AID.DeadMansWindpipeBoss], 2.1f);
        ComponentCondition<PushPullCounter>(id + 0x41, 6.9f, p => p.NumCasts > 1, "Push/pull");

        Plasma(id + 0x50, 4.2f);

        CastStart(id + 0x100, AID._Ability_UnlimitedExpress, 1.9f)
            .ActivateOnEnter<UnlimitedExpress1>();
        ComponentCondition<UnlimitedExpress1>(id + 0x101, 5.9f, x => x.NumCasts > 0)
            .DeactivateOnExit<UnlimitedExpress1>();
        Targetable(id + 0x110, false, 0.2f, "Car 2 start");
    }

    void Car2(uint id, float delay)
    {
        Timeout(id, 0)
            .ActivateOnEnter<Electray>()
            .ExecOnEnter<CarCounter>(c => c.Car++);

        Targetable(id + 1, true, delay, "Boss reappears");

        CastMulti(id + 0x10, [AID.DeadMansOverdraughtStack, AID.DeadMansOverdraughtSpread], 2, 4);
        Cast(id + 0x20, AID._Ability_TurretCrossing, 2.1f, 3);

        CastStartMulti(id + 0x30, [AID.DeadMansExpress, AID.DeadMansWindpipeBoss], 5);

        ComponentCondition<Electray>(id + 0x31, 5.2f, e => e.NumCasts >= 5, "Lasers");

        ComponentCondition<PushPullCounter>(id + 0x32, 1.8f, p => p.NumCasts > 2, "Push/pull");

        Plasma(id + 0x40, 4.2f);

        CastStartMulti(id + 0x50, [AID.DeadMansOverdraughtStack, AID.DeadMansOverdraughtSpread], 1.8f)
            .ActivateOnEnter<LightningBurst>();

        ComponentCondition<Electray>(id + 0x51, 8.8f, e => e.NumCasts >= 10, "Lasers");
        ComponentCondition<LightningBurst>(id + 0x52, 2.8f, l => l.NumCasts > 0, "Tankbusters")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .ExecOnEnter<LightningBurst>(l => l.EnableHints = true)
            .DeactivateOnExit<LightningBurst>();

        ComponentCondition<Electray>(id + 0x53, 8.7f, e => e.NumCasts >= 15, "Lasers")
            .DeactivateOnExit<Electray>();
        ComponentCondition<PushPullCounter>(id + 0x54, 0.8f, p => p.NumCasts > 3, "Push/pull");

        Plasma(id + 0x60, 5.1f);

        CastStart(id + 0x100, AID._Ability_UnlimitedExpress, 2)
            .ActivateOnEnter<UnlimitedExpress1>();
        ComponentCondition<UnlimitedExpress1>(id + 0x101, 5.9f, x => x.NumCasts > 0)
            .DeactivateOnExit<UnlimitedExpress1>();
        Targetable(id + 0x110, false, 0.2f, "Car 2 start");
    }

    void Car3(uint id, float delay)
    {
        Timeout(id, 0).ExecOnEnter<CarCounter>(c => c.Car++);

        Targetable(id + 1, true, delay, "Boss reappears");

        CastStart(id + 0x10, AID._Ability_LightningBurst, 4.1f)
            .ActivateOnEnter<LightningBurst>()
            .ExecOnEnter<LightningBurst>(b => b.EnableHints = true);

        ComponentCondition<LightningBurst>(id + 0x52, 5.6f, l => l.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<LightningBurst>();

        Cast(id + 0x100, AID._Ability_RunawayTrain, 7.7f, 5);

        Targetable(id + 0x102, false, 3.7f, "Intermission start");
    }

    void Intermission(uint id, float delay)
    {
        ActorTargetable(id + 0x10, () => Module.Enemies(OID._Gen_Aether).FirstOrDefault(), true, delay, "Orb appears")
            .OnEnter(() =>
            {
                Module.Arena.Bounds = new ArenaBoundsCircle(14.5f);
                Module.Arena.Center = new(-400, -400);
            })
            .ActivateOnEnter<AetherialRay>()
            .ActivateOnEnter<MultiToot>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }
}
