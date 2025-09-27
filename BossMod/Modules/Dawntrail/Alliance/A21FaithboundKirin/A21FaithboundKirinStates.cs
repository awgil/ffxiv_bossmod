namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

class StonegaIV(BossModule module) : Components.RaidwideCast(module, AID.StonegaIV);

class SynchronizedStrike(BossModule module) : Components.StandardAOEs(module, AID.SynchronizedStrikeSlow, new AOEShapeRect(60, 5));
class SynchronizedSmite(BossModule module) : Components.GroupedAOEs(module, [AID.SynchronizedSmiteRightSlow, AID.SynchronizedSmiteLeftSlow], new AOEShapeRect(60, 16));
class CrimsonRiddle(BossModule module) : Components.GroupedAOEs(module, [AID.CrimsonRiddleFront, AID.CrimsonRiddleBack], new AOEShapeCone(30, 90.Degrees()));

class ArenaBounds(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        // suzaku bounds
        if (index == 0x4B && state == 0x00020001)
            Arena.Bounds = new ArenaBoundsSquare(20);

        // byakko bounds
        if (index == 0x4C && state == 0x00020001)
            Arena.Bounds = new ArenaBoundsCircle(27);

        if (index is 0x4B or 0x4C && state == 0x00080004)
            Arena.Bounds = new ArenaBoundsCircle(29.5f);
    }
}

class A21FaithboundKirinStates : StateMachineBuilder
{
    public A21FaithboundKirinStates(BossModule module) : base(module)
    {
        DeathPhase(0, P1)
            .ActivateOnEnter<SummonShijin>()
            .ActivateOnEnter<StonegaIV>()
            .ActivateOnEnter<CrimsonRiddle>()
            .ActivateOnEnter<ArenaBounds>();
    }

    private void P1(uint id)
    {
        StonegaIV(id, 7.1f);

        Cast(id + 0x10000, AID.WroughtArms, 7.9f, 3.4f);
        ArmsSingle(id + 0x10100, 6.5f);
        ArmsSingle(id + 0x10200, 2.5f);

        CastMulti(id + 0x10300, [AID.CrimsonRiddleFront, AID.CrimsonRiddleBack], 6.5f, 5, "Half-room cleave 1");
        CastMulti(id + 0x10400, [AID.CrimsonRiddleFront, AID.CrimsonRiddleBack], 2.1f, 5, "Half-room cleave 2");

        Cast(id + 0x20000, AID.SummonShijin, 7.2f, 7)
            .ActivateOnEnter<ByakkoWalls>();

        ComponentConditionFork<SummonShijin, Summon>(id + 0x20010, 2.1f, s => s.NextSummon != default, s => s.NextSummon, new()
        {
            { Summon.Seiryu, (1, SummonSeiryu) },
            { Summon.Genbu, (2, SummonGenbu) },
            { Summon.Suzaku, (3, SummonSuzaku) },
            { Summon.Byakko, (4, SummonByakko) },
        }, "Primal appears")
            .DeactivateOnExit<ByakkoWalls>();
    }

    private void Subphase1(uint id)
    {
        ArmsDoubleSlow(id, 3.5f);
        StonegaIV(id + 0x10000, 0.3f);
        ArmsDoubleFast(id + 0x20000, 4.8f);
        MightyGrip(id + 0x30000, 5.9f);
        Cast(id + 0x40000, AID.WroughtArms, 11.9f, 3.4f);
        ArmsDoubleFast(id + 0x40100, 3.5f);
        ArmsDoubleFast(id + 0x50000, 3.8f);

        Timeout(id + 0xF00000, 9999, "???");
    }

    private void StonegaIV(uint id, float delay)
    {
        Cast(id, AID.StonegaIV, delay, 5, "Raidwide");
    }

    private void ArmsSingle(uint id, float delay)
    {
        CastStart(id, AID.SynchronizedStrikeCast, delay)
            .ActivateOnEnter<SynchronizedStrike>()
            .ActivateOnEnter<SynchronizedSmite>();

        ComponentCondition<SynchronizedStrike>(id + 0x10, 4.9f, s => s.NumCasts > 0, "Boss AOE");
        ComponentCondition<SynchronizedSmite>(id + 0x20, 4.5f, s => s.NumCasts > 0, "Arm AOE")
            .DeactivateOnExit<SynchronizedStrike>()
            .DeactivateOnExit<SynchronizedSmite>();
    }

    private void ArmsDoubleSlow(uint id, float delay)
    {
        Cast(id, AID.WroughtArms, delay, 3.4f);

        Cast(id + 0x10, AID.WringerSlow, 3.5f, 4.9f, "Out")
            .ActivateOnEnter<Wringer>()
            .ActivateOnEnter<DeadWringer>();

        ComponentCondition<DeadWringer>(id + 0x20, 5.1f, d => d.NumCasts > 0, "In")
            .DeactivateOnExit<Wringer>()
            .DeactivateOnExit<DeadWringer>();

        CastMulti(id + 0x100, [AID.StrikingRightBoss, AID.StrikingLeftBoss], 4.6f, 4.9f, "Boss AOE")
            .ActivateOnEnter<Striking>()
            .ActivateOnEnter<Smiting>();

        ComponentCondition<Smiting>(id + 0x110, 5.1f, s => s.NumCasts > 0, "Arm AOE")
            .DeactivateOnExit<Striking>()
            .DeactivateOnExit<Smiting>();
    }

    private void ArmsDoubleFast(uint id, float delay)
    {
        CastStartMulti(id, [AID.DoubleWringer, AID.SynchronizedSequenceCast, AID.SmitingRightSequence, AID.SmitingLeftSequence], delay)
            .ActivateOnEnter<DoubleWringer>()
            .ActivateOnEnter<SmitingSequence>()
            .ActivateOnEnter<SynchronizedSequence>()
            .ActivateOnEnter<Smiting>()
            .ActivateOnEnter<SynchronizedSmite>()
            .ActivateOnEnter<DeadWringer>()
            .ActivateOnEnter<ArmsMulti>();

        CastEnd(id + 1, 10, "AOE 1");
        ComponentCondition<ArmsMulti>(id + 0x10, 5, m => m.NumCasts > 0, "AOEs 2");
        ComponentCondition<ArmsMulti>(id + 0x20, 5.1f, m => m.NumCasts > 1, "AOE 3")
            .DeactivateOnExit<DoubleWringer>()
            .DeactivateOnExit<SmitingSequence>()
            .DeactivateOnExit<SynchronizedSequence>()
            .DeactivateOnExit<Smiting>()
            .DeactivateOnExit<SynchronizedSmite>()
            .DeactivateOnExit<DeadWringer>()
            .DeactivateOnExit<ArmsMulti>();
    }

    private void MightyGrip(uint id, float delay)
    {
        Cast(id, AID.MightyGrip, delay, 7)
            .ActivateOnEnter<MightyGrip>();

        Targetable(id + 0x10, false, 2.1f, "Boss disappears")
            .ActivateOnEnter<Shockwave>()
            .ExecOnEnter<Shockwave>(s => s.Predict(10.6f));

        ComponentCondition<MightyGrip>(id + 0x20, 2, m => m.Transformed, "Arena change")
            .ActivateOnEnter<ChiseledArm>()
            .ActivateOnEnter<StandingFirm>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ComponentCondition<Shockwave>(id + 0x30, 6.6f, s => s.NumCasts > 1, "Raidwide");
        ComponentCondition<Shockwave>(id + 0x40, 9, s => s.NumCasts > 3, "Raidwide")
            .ExecOnEnter<Shockwave>(s => s.Predict(9))
            .DeactivateOnExit<Shockwave>();

        ComponentCondition<StandingFirm>(id + 0x100, 9.1f, s => s.NumCasts > 0, "Tank towers")
            .DeactivateOnExit<StandingFirm>();

        ComponentCondition<Shockwave>(id + 0x110, 5.4f, s => s.NumCasts > 1, "Raidwide")
            .ActivateOnEnter<Shockwave>()
            .ExecOnEnter<Shockwave>(s => s.Predict(5.4f));

        ComponentCondition<Shockwave>(id + 0x120, 9, s => s.NumCasts > 3, "Raidwide")
            .ExecOnEnter<Shockwave>(s => s.Predict(9))
            .DeactivateOnExit<Shockwave>();

        ComponentCondition<MightyGrip>(id + 0x200, 19, m => !m.Transformed, "Arms enrage")
            .DeactivateOnExit<MightyGrip>();
    }

    private void SummonSeiryu(uint id)
    {
        ComponentCondition<EastwindWheel1>(id, 18.6f, e => e.NumCasts >= 4, "Line 1")
            .DeactivateOnExit<CrimsonRiddle>()
            .ActivateOnEnter<EastwindWheel1>()
            .ActivateOnEnter<EastwindWheel2>()
            .ActivateOnEnter<StonegaIII>()
            .ActivateOnEnter<Quake2>();

        ComponentCondition<EastwindWheel2>(id + 0x10, 0.5f, e => e.NumCasts > 0, "Staff cleave 1");

        CastStart(id + 0x20, AID.CrimsonRiddleFront, 0.9f).ActivateOnEnter<CrimsonRiddle>();

        ComponentCondition<CrimsonRiddle>(id + 0x22, 5.1f, c => c.NumCasts > 0, "Boss cleave");

        ComponentCondition<EastwindWheel1>(id + 0x100, 12.7f, e => e.NumCasts >= 8, "Line 2")
            .DeactivateOnExit<EastwindWheel1>();
        ComponentCondition<EastwindWheel2>(id + 0x110, 0.5f, e => e.NumCasts > 1, "Staff cleave 2")
            .DeactivateOnExit<EastwindWheel2>();

        ComponentCondition<StonegaIII>(id + 0x120, 5.9f, s => s.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<StonegaIII>();

        Subphase1(id + 0x10000);
    }

    private void SummonGenbu(uint id)
    {
        ComponentCondition<MoontideFont>(id, 18.5f, m => m.NumCasts >= 10, "Puddles 1")
            .ActivateOnEnter<MoontideFont>()
            .ActivateOnEnter<ShatteringStomp>();
        ComponentCondition<MoontideFont>(id + 0x10, 5.1f, m => m.NumCasts >= 20, "Puddles 2")
            .DeactivateOnExit<MoontideFont>();

        ComponentCondition<MidwinterMarch>(id + 0x100, 5.7f, m => m.NumCasts > 0, "Jump")
            .ActivateOnEnter<MidwinterMarch>()
            .ActivateOnEnter<NorthernCurrent>();

        ComponentCondition<NorthernCurrent>(id + 0x110, 5.1f, n => n.NumCasts > 0, "Donut")
            .DeactivateOnExit<MidwinterMarch>()
            .DeactivateOnExit<NorthernCurrent>();

        CastMulti(id + 0x200, [AID.CrimsonRiddleFront, AID.CrimsonRiddleBack], 0, 5, "Half-room cleave");

        Subphase1(id + 0x10000);
    }

    private void SummonSuzaku(uint id)
    {
        ComponentCondition<VermilionFlight>(id, 18.7f, v => v.NumCasts > 0, "Line 1")
            .ActivateOnEnter<VermilionFlight>()
            .ActivateOnEnter<ArmOfPurgatory>()
            .ActivateOnEnter<StonegaIII2>();

        ComponentCondition<VermilionFlight>(id + 0x10, 15.3f, v => v.NumCasts > 1, "Line 2")
            .DeactivateOnExit<VermilionFlight>()
            .DeactivateOnExit<ArmOfPurgatory>();

        ComponentCondition<StonegaIII2>(id + 0x100, 4.3f, s => s.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<StonegaIII2>();

        // boss sits around doing nothing for a few seconds
        Timeout(id + 0x10000, 2.7f);

        Subphase1(id + 0x10001);
    }

    private void SummonByakko(uint id)
    {
        ComponentCondition<GloamingGleam>(id, 9, g => g.NumCasts > 0, "Charge")
            .ActivateOnEnter<GloamingGleam>()
            .ActivateOnEnter<RazorFang>()
            .ActivateOnEnter<Quake>();

        ComponentCondition<RazorFang>(id + 0x10, 1.5f, r => r.NumCasts > 0, "AOE");
        ComponentCondition<GloamingGleam>(id + 0x20, 8.6f, g => g.NumCasts > 1, "Charge");
        ComponentCondition<RazorFang>(id + 0x30, 1.5f, r => r.NumCasts > 1, "AOE");

        Cast(id + 0x100, AID.QuakeCast, 0.5f, 3, "Puddles start");

        ComponentCondition<GloamingGleam>(id + 0x110, 4.9f, g => g.NumCasts > 2, "Charge");
        ComponentCondition<RazorFang>(id + 0x120, 1.5f, r => r.NumCasts > 2, "AOE");

        ComponentCondition<GloamingGleam>(id + 0x130, 8.5f, g => g.NumCasts > 3, "Charge");
        ComponentCondition<RazorFang>(id + 0x140, 1.5f, r => r.NumCasts > 3, "AOE");

        Subphase1(id + 0x10000);
    }
}
