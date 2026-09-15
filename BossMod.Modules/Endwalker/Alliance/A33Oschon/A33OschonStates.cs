namespace BossMod.Endwalker.Alliance.A33Oschon;

class A33OschonStates : StateMachineBuilder
{
    private readonly A33Oschon _module;

    public A33OschonStates(A33Oschon module) : base(module)
    {
        _module = module;
        SimplePhase(0, Phase1, "P1")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || (Module.PrimaryActor.CastInfo?.IsSpell(AID.LoftyPeaks) ?? false);
        SimplePhase(1, Phase2, "P2")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed && (_module.BossP2()?.HPMP.CurHP ?? 0) <= 1;
    }

    private void Phase1(uint id)
    {
        P1SuddenDownpour(id, 10.2f);
        P1TrekShot(id + 0x10000, 4.4f);
        P1Reproduce(id + 0x20000, 5.8f);
        P1Reproduce(id + 0x30000, 2.4f);
        P1FlintedFoehn(id + 0x40000, 2.3f);
        P1SoaringMinuet(id + 0x50000, 1.4f);
        P1Arrow(id + 0x60000, 3.2f);
        P1Reproduce(id + 0x70000, 4.2f);
        P1SuddenDownpour(id + 0x80000, 0.1f);
        P1DownhillClimbingShot(id + 0x90000, 4.4f);
        P1FlintedFoehn(id + 0xA0000, 3.2f);
        P1TrekShot(id + 0xB0000, 1.3f);
        P1SuddenDownpour(id + 0xC0000, 2.6f);
        P1SuddenDownpour(id + 0xD0000, 1.1f);

        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void Phase2(uint id)
    {
        P2LoftyPeaks(id, 0);
        P2PitonPull(id + 0x10000, 5.2f);
        P2PitonPull(id + 0x20000, 3.7f);
        P2Altitude(id + 0x30000, 5.7f);
        P2FlintedFoehn(id + 0x40000, 5.2f);
        P2WanderingShot(id + 0x50000, 3.3f);
        P2WanderingShot(id + 0x60000, 3.5f);
        P2Arrow(id + 0x70000, 3.5f);
        P2ArrowTrail(id + 0x80000, 5.2f);
        P2WanderingVolley(id + 0x90000, 6.7f);
        P2FlintedFoehn(id + 0xA0000, 5.7f);
        P2Arrow(id + 0xB0000, 1.1f);
        P2Altitude(id + 0xC0000, 2.2f);

        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void P1SuddenDownpour(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, AID.SuddenDownpour, delay, 4, true);
        ComponentCondition<P1SuddenDownpour>(id + 2, 1, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<P1SuddenDownpour>()
            .DeactivateOnExit<P1SuddenDownpour>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P1TrekShot(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP1, [AID.TrekShotN, AID.TrekShotS], delay, 6, true)
            .ActivateOnEnter<P1TrekShotN>()
            .ActivateOnEnter<P1TrekShotS>();
        Condition(id + 2, 3.5f, () => _module.FindComponent<P1TrekShotN>()?.NumCasts > 0 || _module.FindComponent<P1TrekShotS>()?.NumCasts > 0, "Cone")
            .DeactivateOnExit<P1TrekShotN>()
            .DeactivateOnExit<P1TrekShotS>();
    }

    private void P1Reproduce(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, AID.Reproduce, delay, 3, true);
        ComponentCondition<P1SwingingDraw>(id + 0x10, 2.9f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<P1SwingingDraw>();
        ComponentCondition<P1SwingingDraw>(id + 0x20, 13.2f, comp => comp.NumCasts > 0, "Cone")
            .DeactivateOnExit<P1SwingingDraw>();
    }

    private void P1FlintedFoehn(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP1, AID.FlintedFoehnP1, delay, true)
            .ActivateOnEnter<P1FlintedFoehn>();
        ActorCastEnd(id + 1, _module.BossP1, 4.5f, true);
        ComponentCondition<P1FlintedFoehn>(id + 0x10, 0.6f, comp => comp.NumCasts > 0, "Stack 1");
        ComponentCondition<P1FlintedFoehn>(id + 0x20, 5.3f, comp => comp.NumCasts > 5, "Stack 6")
            .DeactivateOnExit<P1FlintedFoehn>();
    }

    private void P1SoaringMinuet(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, AID.SoaringMinuet1, delay, 5, true, "Wide cone")
            .ActivateOnEnter<P1SoaringMinuet1>()
            .DeactivateOnExit<P1SoaringMinuet1>();
    }

    private void P1Arrow(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, AID.ArrowP1, delay, 4, true)
            .ActivateOnEnter<P1Arrow>();
        ComponentCondition<P1Arrow>(id + 2, 1, comp => comp.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<P1Arrow>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P1DownhillClimbingShot(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, AID.DownhillP1, delay, 3, true);
        ComponentCondition<P1Downhill>(id + 0x10, 0.4f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<P1Downhill>();
        ActorCastMulti(id + 0x20, _module.BossP1, [AID.ClimbingShot1, AID.ClimbingShot2, AID.ClimbingShot3, AID.ClimbingShot4], 1.7f, 5, true, "Knockback")
            .ActivateOnEnter<P1ClimbingShot>()
            .DeactivateOnExit<P1ClimbingShot>();
        ComponentCondition<P1Downhill>(id + 0x30, 1.8f, comp => comp.NumCasts > 0, "Puddles")
            .DeactivateOnExit<P1Downhill>();
        ActorCast(id + 0x100, _module.BossP1, AID.SoaringMinuet2, 1.3f, 5, true, "Wide cone")
            .ActivateOnEnter<P1SoaringMinuet2>()
            .DeactivateOnExit<P1SoaringMinuet2>();
    }

    private void P2LoftyPeaks(uint id, float delay)
    {
        ActorCast(id, _module.BossP1, AID.LoftyPeaks, delay, 5, true, "Boss disappears")
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ComponentCondition<P2MovingMountains>(id + 0x10, 1.1f, comp => comp.NumCasts > 0, "Raidwide 1")
            .ActivateOnEnter<P2MovingMountains>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P2MovingMountains>(id + 0x11, 1.5f, comp => comp.NumCasts > 1, "Raidwide 2")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P2MovingMountains>(id + 0x12, 1.5f, comp => comp.NumCasts > 2, "Raidwide 3")
            .DeactivateOnExit<P2MovingMountains>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P2PeakPeril>(id + 0x13, 3.9f, comp => comp.NumCasts > 0, "Raidwide 4")
            .ActivateOnEnter<P2PeakPeril>()
            .DeactivateOnExit<P2PeakPeril>()
            .OnExit(() => Module.Arena.Bounds = new ArenaBoundsSquare(20))
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<P2Shockwave>(id + 0x20, 15.7f, comp => comp.NumCasts > 0, "Raidwide 5")
            .ActivateOnEnter<P2Shockwave>()
            .DeactivateOnExit<P2Shockwave>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorTargetable(id + 0x30, _module.BossP2, true, 2, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void P2PitonPull(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.PitonPullNW, AID.PitonPullNE], delay, 8, true)
            .ActivateOnEnter<P2PitonPull>();
        ComponentCondition<P2PitonPull>(id + 2, 0.5f, comp => comp.NumCasts > 0, "Diagonal circles")
            .DeactivateOnExit<P2PitonPull>();
    }

    private void P2Altitude(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.Altitude, delay, 6, true)
            .ActivateOnEnter<P2Altitude>();
        ComponentCondition<P2Altitude>(id + 2, 1, comp => comp.NumCasts > 0, "Puddles")
            .DeactivateOnExit<P2Altitude>();
    }

    private void P2FlintedFoehn(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2, AID.FlintedFoehnP2, delay, true)
            .ActivateOnEnter<P2FlintedFoehn>();
        ActorCastEnd(id + 1, _module.BossP2, 4.5f, true);
        ComponentCondition<P2FlintedFoehn>(id + 0x10, 0.6f, comp => comp.NumCasts > 0, "Stack 1");
        ComponentCondition<P2FlintedFoehn>(id + 0x20, 5.3f, comp => comp.NumCasts > 5, "Stack 6")
            .DeactivateOnExit<P2FlintedFoehn>();
    }

    private void P2WanderingShot(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.WanderingShotN, AID.WanderingShotS], delay, 7, true)
            .ActivateOnEnter<P2WanderingShot>();
        ComponentCondition<P2WanderingShot>(id + 0x10, 3.6f, comp => comp.NumCasts > 0, "N/S circle")
            .DeactivateOnExit<P2WanderingShot>();
    }

    private void P2Arrow(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.ArrowP2, delay, 6, true)
            .ActivateOnEnter<P2Arrow>();
        ComponentCondition<P2Arrow>(id + 2, 1, comp => comp.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<P2Arrow>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P2ArrowTrail(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.ArrowTrail, delay, 3, true);
        ComponentCondition<P2ArrowTrail>(id + 0x10, 2.4f, comp => comp.NumCasts > 0, "Exaflares start")
            .ActivateOnEnter<P2ArrowTrail>();

        ComponentCondition<P2DownhillArrowTrailDownhill>(id + 0x20, 5.7f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<P2DownhillArrowTrailDownhill>();
        ComponentCondition<P2DownhillArrowTrailDownhill>(id + 0x21, 3, comp => comp.NumCasts > 0, "Puddles 1");

        ComponentCondition<P2DownhillArrowTrailDownhill>(id + 0x30, 1, comp => comp.Casters.Count > 0);
        ComponentCondition<P2DownhillArrowTrailDownhill>(id + 0x31, 3, comp => comp.NumCasts > 3, "Puddles 2");

        ComponentCondition<P2DownhillArrowTrailDownhill>(id + 0x40, 1, comp => comp.Casters.Count > 0);
        ActorCastStartMulti(id + 0x41, _module.BossP2, [AID.PitonPullNW, AID.PitonPullNE], 2.1f, true);
        ComponentCondition<P2DownhillArrowTrailDownhill>(id + 0x42, 0.9f, comp => comp.NumCasts > 6, "Puddles 3")
            .ActivateOnEnter<P2PitonPull>()
            .DeactivateOnExit<P2DownhillArrowTrailDownhill>();
        ComponentCondition<P2ArrowTrail>(id + 0x43, 0.8f, comp => comp.NumCasts >= 64, "Exaflares resolve")
            .DeactivateOnExit<P2ArrowTrail>();
        ActorCastEnd(id + 0x44, _module.BossP2, 6.2f, true);
        ComponentCondition<P2PitonPull>(id + 0x45, 0.5f, comp => comp.NumCasts > 0, "Diagonal circles")
            .DeactivateOnExit<P2PitonPull>();
    }

    private void P2WanderingVolley(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.WanderingVolleyDownhill, delay, 3, true);
        ComponentCondition<P2WanderingVolleyDownhill>(id + 0x10, 0.5f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<P2WanderingVolleyDownhill>();
        ActorCastMulti(id + 0x20, _module.BossP2, [AID.WanderingVolleyN, AID.WanderingVolleyS], 2.7f, 10, true, "Knockback sides")
            .ActivateOnEnter<P2WanderingVolleyKnockback>()
            .ActivateOnEnter<P2WanderingVolleyAOE>()
            .ActivateOnEnter<P2WanderingShot>()
            .DeactivateOnExit<P2WanderingVolleyKnockback>()
            .DeactivateOnExit<P2WanderingVolleyAOE>();
        ComponentCondition<P2WanderingVolleyDownhill>(id + 0x30, 1.3f, comp => comp.NumCasts > 0, "Puddles")
            .DeactivateOnExit<P2WanderingVolleyDownhill>();
        ActorCastStartMulti(id + 0x40, _module.BossP2, [AID.PitonPullNW, AID.PitonPullNE], 1.9f, true);
        ComponentCondition<P2WanderingShot>(id + 0x41, 0.5f, comp => comp.NumCasts > 0, "N/S circle")
            .ActivateOnEnter<P2PitonPull>()
            .DeactivateOnExit<P2WanderingShot>();
        ActorCastEnd(id + 0x42, _module.BossP2, 7.5f, true);
        ComponentCondition<P2PitonPull>(id + 0x43, 0.5f, comp => comp.NumCasts > 0, "Diagonal circles")
            .DeactivateOnExit<P2PitonPull>();
    }
}
