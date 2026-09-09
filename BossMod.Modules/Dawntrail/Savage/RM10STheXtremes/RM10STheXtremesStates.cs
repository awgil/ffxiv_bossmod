namespace BossMod.Dawntrail.Savage.RM10STheXtremes;

class RM10STheXtremesStates : StateMachineBuilder
{
    private readonly RM10STheXtremes _module;

    public RM10STheXtremesStates(BossModule module) : base(module)
    {
        _module = (RM10STheXtremes)module;
        DeathPhase(0, P1);
    }

    void P1(uint id)
    {
        RedP1(id, 9.2f);
        BlueP1(id + 0x10000, 1.9f);
        XtremeSpectacular(id + 0x20000, 8.2f);
        Air1(id + 0x21000, 11.3f);
        Snaking(id + 0x30000, 10.7f);
        BubblePhase(id + 0x40000, 14.4f);
        SplitPhase(id + 0x50000, 10.7f);
        Air2(id + 0x60000, 10.8f);
        PreEnrage(id + 0x70000, 10.8f);

        Cast(id + 0x80000, AID.OverTheFallsRed, 12.5f, 9, "Enrage");
    }

    void RedP1(uint id, float delay)
    {
        Cast(id, AID.HotImpact1, delay, 5, "Tankbuster")
            .ActivateOnEnter<HotImpact>()
            .DeactivateOnExit<HotImpact>();

        CastStart(id + 0x10, AID.FlameFloaterCast, 8.4f)
            .ActivateOnEnter<FlameFloater>()
            .ActivateOnEnter<FlameFloaterPuddle>();

        ComponentCondition<FlameFloater>(id + 0x20, 7.3f, f => f.NumCasts > 0, "Bait 1");
        ComponentCondition<FlameFloater>(id + 0x21, 9.8f, f => f.NumCasts > 3, "Bait 4")
            .DeactivateOnExit<FlameFloater>();

        Cast(id + 0x100, AID.AlleyOopInfernoCast, 8.1f, 4.3f)
            .ActivateOnEnter<AlleyOopInfernoSpread>()
            .ActivateOnEnter<AlleyOopInfernoPuddle>();

        ComponentCondition<AlleyOopInfernoSpread>(id + 0x110, 0.7f, p => p.NumFinishedSpreads > 0, "Spread (puddles)")
            .DeactivateOnExit<AlleyOopInfernoSpread>();

        Cast(id + 0x200, AID.CutbackBlazeCast, 4.5f, 4.3f)
            .ActivateOnEnter<CutbackBlazeBait>()
            .ActivateOnEnter<CutbackBlazePuddle>();

        ComponentCondition<CutbackBlazeBait>(id + 0x210, 0.9f, c => c.NumCasts > 0, "Safe cone bait")
            .DeactivateOnExit<CutbackBlazeBait>();

        CastStart(id + 0x300, AID.PyrotationCast, 1.3f)
            .ActivateOnEnter<Pyrotation>()
            .ActivateOnEnter<PyrotationPuddle>();

        ComponentCondition<Pyrotation>(id + 0x301, 5.2f, p => p.NumCasts > 0, "Stack 1");
        ComponentCondition<Pyrotation>(id + 0x302, 4, p => p.NumCasts > 2, "Stack 3")
            .DeactivateOnExit<Pyrotation>();

        Cast(id + 0x400, AID.DiversDareRed, 4.3f, 5, "Raidwide")
            .ActivateOnEnter<DiversDare1>()
            .DeactivateOnExit<DiversDare1>();

        Targetable(id + 0x500, false, 6.4f, "Red disappears")
            .DeactivateOnExit<FlameFloaterPuddle>()
            .DeactivateOnExit<AlleyOopInfernoPuddle>()
            .DeactivateOnExit<CutbackBlazePuddle>()
            .DeactivateOnExit<PyrotationPuddle>();
    }

    void BlueP1(uint id, float delay)
    {
        ActorTargetable(id, _module.B2, true, delay, "Blue appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ActorCast(id + 0x10, _module.B2, AID.SickSwellCast, 3.3f, 3)
            .ActivateOnEnter<AwesomeSplab>()
            .ActivateOnEnter<SickestTakeOff>()
            .ActivateOnEnter<SickSwell>();

        ActorCast(id + 0x20, _module.B2, AID.SickestTakeOffCast, 5.2f, 4);
        ActorTargetable(id + 0x22, _module.B2, false, 0, "Blue disappears")
            .SetHint(StateMachine.StateHint.DowntimeStart);

        ComponentCondition<SickestTakeOff>(id + 0x24, 9.1f, s => s.NumCasts > 0, "Line + knockback")
            .ExecOnExit<AwesomeSplab>(s => s.EnableHints = true);

        ActorTargetable(id + 0x25, _module.B2, true, 2.2f, "Blue reappears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<AwesomeSplab>(id + 0x26, 0.5f, s => s.NumCasts > 0, "Stack/spread")
            .DeactivateOnExit<AwesomeSplab>()
            .DeactivateOnExit<SickestTakeOff>()
            .DeactivateOnExit<SickSwell>();

        AlleyOop(id + 0x30, 2.6f);

        ActorCast(id + 0x100, _module.B2, AID.DeepImpactCast, 2.9f, 4.9f)
            .ActivateOnEnter<DeepImpactBait>()
            .ActivateOnEnter<DeepImpactKB>();

        ComponentCondition<DeepImpactBait>(id + 0x102, 0.3f, d => d.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<DeepImpactBait>()
            .DeactivateOnExit<DeepImpactKB>();

        ActorCast(id + 0x110, _module.B2, AID.DiversDareBlue, 2.1f, 5, true, "Raidwide")
            .ActivateOnEnter<DiversDare2>()
            .DeactivateOnExit<DiversDare2>();
    }

    void XtremeSpectacular(uint id, float delay)
    {
        Targetable(id, true, delay, "Red appears");

        Cast(id + 0x10, AID.XtremeSpectacularCastRed, 5.2f, 4)
            .ActivateOnEnter<XtremeSpectacularProximity>()
            .ActivateOnEnter<XtremeSpectacularRaidwideFirst>()
            .ActivateOnEnter<XtremeSpectacularRaidwideLast>();

        Targetable(id + 0x20, false, 0.7f, "Bosses disappear");

        ComponentCondition<XtremeSpectacularRaidwideFirst>(id + 0x30, 6.9f, x => x.NumCasts > 0, "Raidwide 1")
            .DeactivateOnExit<XtremeSpectacularProximity>()
            .DeactivateOnExit<XtremeSpectacularRaidwideFirst>();

        ComponentCondition<XtremeSpectacularRaidwideLast>(id + 0x31, 4.8f, x => x.NumCasts > 0, "Raidwide 8")
            .DeactivateOnExit<XtremeSpectacularRaidwideLast>();

        Targetable(id + 0x100, true, 3.9f, "Bosses reappear");
    }

    void Air1(uint id, float delay)
    {
        CastStart(id, AID.InsaneAir1RedFirst, delay)
            .ActivateOnEnter<AirBaits>()
            .ActivateOnEnter<AirPuddleCone>()
            .ActivateOnEnter<AirPuddleCircle>();

        ComponentCondition<AirBaits>(id + 0x10, 8.9f, r => r.NumCasts == 2, "Baits 1");
        ComponentCondition<AirBaits>(id + 0x11, 7.4f, r => r.NumCasts == 4, "Baits 2");
        ComponentCondition<AirBaits>(id + 0x12, 7.4f, r => r.NumCasts == 6, "Baits 3");
        ComponentCondition<AirBaits>(id + 0x13, 7.4f, r => r.NumCasts == 8, "Baits 4")
            .DeactivateOnExit<AirBaits>();

        Cast(id + 0x100, AID.DiversDareRed, 4.7f, 5, "Raidwide")
            .ActivateOnEnter<DiversDare1>()
            .DeactivateOnExit<DiversDare1>()
            .DeactivateOnExit<AirPuddleCone>()
            .DeactivateOnExit<AirPuddleCircle>();
    }

    void Snaking(uint id, float delay)
    {
        Cast(id, AID.Firesnaking, delay, 5, "Raidwide + assign debuffs")
            .ActivateOnEnter<FiresnakingRaidwide>()
            .ActivateOnEnter<AlleyOopProteanRepeat>()
            .ActivateOnEnter<AlleyOopProteans>()
            .ActivateOnEnter<AlleyOopInfernoSpread>()
            .ActivateOnEnter<AlleyOopInfernoPuddle>()
            .ActivateOnEnter<SteamBurstPredict>()
            .ActivateOnEnter<SteamBurst>()
            .ActivateOnEnter<DeepVarial>()
            .ActivateOnEnter<DeepVarialPredict>()
            .ActivateOnEnter<DeepVarialSpreadStack>()
            .DeactivateOnExit<FiresnakingRaidwide>();

        Cast(id + 0x10, AID.AlleyOopInfernoCast, 7.6f, 4.3f);

        ComponentCondition<AlleyOopInfernoSpread>(id + 0x20, 0.7f, s => s.NumFinishedSpreads > 0, "Puddles")
            .DeactivateOnExit<AlleyOopInfernoSpread>();
        ComponentCondition<AlleyOopProteans>(id + 0x21, 0.5f, s => s.NumCasts > 0, "Proteans")
            .DeactivateOnExit<AlleyOopProteans>();
        ComponentCondition<AlleyOopProteanRepeat>(id + 0x22, 2.6f, p => p.NumCasts > 0, "Proteans repeat")
            .DeactivateOnExit<AlleyOopProteanRepeat>();

        Cast(id + 0x30, AID.HotImpact2, 3.3f, 5, "Tankbuster (red)")
            .ActivateOnEnter<SnakingHotImpact>()
            .DeactivateOnExit<SnakingHotImpact>();

        CastStart(id + 0x40, AID.AlleyOopInfernoCast, 6.1f)
            .ActivateOnEnter<AlleyOopInfernoSpread>();

        ComponentCondition<DeepVarial>(id + 0x100, 1.7f, d => d.NumCasts > 0, "Big cone")
            .DeactivateOnExit<DeepVarial>()
            .DeactivateOnExit<DeepVarialPredict>();

        ComponentCondition<DeepVarialSpreadStack>(id + 0x110, 3.1f, s => s.NumCasts > 0, "Stack/spread (blue)")
            .DeactivateOnExit<DeepVarialSpreadStack>();
        ComponentCondition<AlleyOopInfernoSpread>(id + 0x120, 0.2f, s => s.NumFinishedSpreads > 0, "Spread (red)")
            .DeactivateOnExit<AlleyOopInfernoSpread>();

        Cast(id + 0x200, AID.HotAerialCast, 3.3f, 5)
            .ActivateOnEnter<AwesomeSplab>()
            .ActivateOnEnter<HotAerial>()
            .ActivateOnEnter<HotAerialPuddle>();

        ComponentCondition<HotAerial>(id + 0x210, 0.4f, h => h.NumCasts > 0, "Jumps start (red)");

        ActorTargetable(id + 0x220, _module.B2, false, 3.1f, "Blue disappears")
            .ActivateOnEnter<SickSwell>()
            .ActivateOnEnter<SickestTakeOff>()
            .ExecOnEnter<SickSwell>(s => s.EnableHints = false)
            .ExecOnEnter<SickestTakeOff>(s => s.Risky = false);

        ComponentCondition<HotAerial>(id + 0x230, 3.1f, h => h.NumCasts == 4, "Jumps end (red)")
            .DeactivateOnExit<HotAerial>()
            .ExecOnExit<SickSwell>(s => s.EnableHints = true)
            .ExecOnExit<SickestTakeOff>(s => s.Risky = true);

        ComponentCondition<SickSwell>(id + 0x240, 6, s => s.NumCasts > 0, "Knockback + line AOE")
            .DeactivateOnExit<SickSwell>()
            .DeactivateOnExit<SickestTakeOff>()
            .ExecOnExit<SteamBurst>(s => s.Reset());

        CastStart(id + 0x250, AID.CutbackBlazeCast, 1.2f)
            .ActivateOnEnter<CutbackBlazeBait>()
            .ActivateOnEnter<CutbackBlazePuddle>()
            .ActivateOnEnter<DeepImpactBait>()
            .ActivateOnEnter<DeepImpactKB>();

        ComponentCondition<AwesomeSplab>(id + 0x251, 1.4f, s => s.NumCasts > 0, "Stack/spread (blue)")
            .DeactivateOnExit<AwesomeSplab>();

        ComponentCondition<SteamBurst>(id + 0x252, 1.7f, s => s.NumCasts > 0, "Delayed explosions");

        ComponentCondition<CutbackBlazeBait>(id + 0x253, 2.2f, c => c.NumCasts > 0, "Safe cone bait")
            .DeactivateOnExit<CutbackBlazeBait>();

        ComponentCondition<DeepImpactBait>(id + 0x260, 5.9f, b => b.NumCasts > 0, "Tankbuster (blue)")
            .DeactivateOnExit<DeepImpactBait>()
            .DeactivateOnExit<DeepImpactKB>();

        Cast(id + 0x300, AID.DiversDareRed, 2.2f, 5, "Raidwide")
            .ActivateOnEnter<DiversDare1>()
            .DeactivateOnExit<DiversDare1>()
            .DeactivateOnExit<SteamBurst>()
            .DeactivateOnExit<SteamBurstPredict>()
            .DeactivateOnExit<AlleyOopInfernoPuddle>()
            .DeactivateOnExit<HotAerialPuddle>()
            .DeactivateOnExit<CutbackBlazePuddle>();
    }

    void BubblePhase(uint id, float delay)
    {
        ComponentCondition<DeepAerial>(id, delay, d => d.NumCasts > 0, "Tower")
            .ActivateOnEnter<DeepAerial>()
            .ActivateOnEnter<WateryGrave>()
            .ActivateOnEnter<BubbleBounds>()
            .DeactivateOnExit<DeepAerial>();

        ComponentCondition<WateryGrave>(id + 0x10, 3.2f, w => w.ActiveActors.Any(), "Bubble appears")
            .ActivateOnEnter<BubbleTether>()
            .ActivateOnEnter<ScathingSteam>();

        ComponentCondition<BubbleTether>(id + 0x100, 8.1f, b => b.NumCasts > 1, "Tethers 1");
        ComponentCondition<BubbleTether>(id + 0x101, 8.6f, b => b.NumCasts > 3, "Tethers 2");
        ComponentCondition<BubbleTether>(id + 0x102, 8.6f, b => b.NumCasts > 5, "Tethers 3");
        ComponentCondition<BubbleTether>(id + 0x103, 8.6f, b => b.NumCasts > 7, "Tethers 4");
        ComponentCondition<BubbleTether>(id + 0x104, 8.6f, b => b.NumCasts > 9, "Tethers 5");
        ComponentCondition<BubbleTether>(id + 0x105, 8.6f, b => b.NumCasts > 11, "Tethers 6")
            .DeactivateOnExit<BubbleTether>();

        Targetable(id + 0x200, true, 2, "Bosses reappear");

        // if not killed, bubble explodes at the same time cast starts
        Cast(id + 0x300, AID.DiversDareRed, 3.2f, 5, "Raidwide")
            .ActivateOnEnter<DiversDare1>()
            .DeactivateOnExit<DiversDare1>()
            .DeactivateOnExit<WateryGrave>()
            .DeactivateOnExit<ScathingSteam>();
    }

    void SplitPhase(uint id, float delay)
    {
        CastStart(id, AID.FlameFloaterSplitCast, delay)
            .ActivateOnEnter<FlameFloaterSplit>()
            .ActivateOnEnter<SplitPuddle>()
            .ActivateOnEnter<SteamBurst>();

        ComponentCondition<SplitPuddle>(id + 0x10, 6.3f, s => s.NumActors > 0, "Voidzone appears")
            .DeactivateOnExit<FlameFloaterSplit>();

        Cast(id + 0x20, AID.AlleyOopInfernoCast, 3, 4.3f)
            .ActivateOnEnter<AlleyOopInfernoPuddle>()
            .ActivateOnEnter<AlleyOopInfernoSpread>()
            .ActivateOnEnter<AlleyOopProteans>()
            .ActivateOnEnter<AlleyOopProteanRepeat>()
            .ExecOnEnter<AlleyOopProteans>(p => p.EnableHints = false);

        ComponentCondition<AlleyOopInfernoSpread>(id + 0x30, 0.7f, s => s.NumFinishedSpreads > 0, "Puddles")
            .DeactivateOnExit<AlleyOopInfernoSpread>()
            .ExecOnExit<AlleyOopProteans>(p => p.EnableHints = true);

        ComponentCondition<AlleyOopProteans>(id + 0x31, 5.4f, p => p.NumCasts > 0, "Proteans");
        ComponentCondition<AlleyOopProteanRepeat>(id + 0x32, 2.6f, p => p.NumCasts > 0, "Proteans (repeat)")
            .ActivateOnEnter<FreakyPyrotation>()
            .ActivateOnEnter<FreakyPyrotationPuddle>()
            .ActivateOnEnter<SickestTakeOff>()
            .ActivateOnEnter<SickSwell>()
            .DeactivateOnExit<AlleyOopProteans>()
            .DeactivateOnExit<AlleyOopProteanRepeat>();

        ActorTargetable(id + 0x40, _module.B2, false, 4, "Blue disappears");

        ComponentCondition<FreakyPyrotation>(id + 0x50, 1, p => p.NumFinishedStacks > 0, "Stacks")
            .DeactivateOnExit<FreakyPyrotation>();

        ComponentCondition<SickestTakeOff>(id + 0x60, 8, s => s.NumCasts > 0, "Knockback + line AOE")
            .ActivateOnEnter<AwesomeSplab>()
            .DeactivateOnExit<SickestTakeOff>()
            .DeactivateOnExit<SickSwell>()
            .ExecOnExit<AwesomeSplab>(s => s.EnableHints = true);

        ComponentCondition<AwesomeSplab>(id + 0x61, 2.5f, s => s.NumCasts > 0, "Party stacks")
            .DeactivateOnExit<AwesomeSplab>();

        // cast is 5s total, stacks go off about 0.2s after it starts
        Cast(id + 0x100, AID.DiversDareRed, 0, 4.7f, "Raidwide")
            .ActivateOnEnter<DiversDare1>()
            .DeactivateOnExit<DiversDare1>()
            .DeactivateOnExit<BubbleBounds>()
            .DeactivateOnExit<SplitPuddle>()
            .DeactivateOnExit<SteamBurst>()
            .DeactivateOnExit<AlleyOopInfernoPuddle>()
            .DeactivateOnExit<FreakyPyrotationPuddle>();
    }

    void Air2(uint id, float delay)
    {
        Cast(id, AID.XtremeFiresnaking, delay, 5, "Raidwide + assign debuffs")
            .ActivateOnEnter<XtremeFiresnakingRaidwide>()
            .ActivateOnEnter<Air2Assignments>()
            .ActivateOnEnter<Air2Baits>()
            .ActivateOnEnter<AirPuddleCircle>()
            .ActivateOnEnter<AirPuddleCone>()
            .ActivateOnEnter<Bailout>()
            .DeactivateOnExit<XtremeFiresnakingRaidwide>();

        ComponentCondition<Air2Baits>(id + 0x10, 17.9f, r => r.NumCasts == 2, "Baits 1");
        ComponentCondition<Bailout>(id + 0x11, 1.6f, b => b.NumCasts == 2, "Stacks 1");
        ComponentCondition<Air2Baits>(id + 0x12, 8.6f, r => r.NumCasts == 4, "Baits 2");
        ComponentCondition<Bailout>(id + 0x13, 1.6f, b => b.NumCasts == 4, "Stacks 2");
        ComponentCondition<Air2Baits>(id + 0x14, 8.6f, r => r.NumCasts == 6, "Baits 3");
        ComponentCondition<Bailout>(id + 0x15, 1.6f, b => b.NumCasts == 6, "Stacks 3");
        ComponentCondition<Air2Baits>(id + 0x16, 8.6f, r => r.NumCasts == 8, "Baits 4");
        ComponentCondition<Bailout>(id + 0x17, 1.6f, b => b.NumCasts == 8, "Stacks 4")
            .DeactivateOnExit<Air2Assignments>()
            .DeactivateOnExit<Air2Baits>()
            .DeactivateOnExit<Bailout>();

        Cast(id + 0x100, AID.DiversDareRed, 2.9f, 5, "Raidwide")
            .ActivateOnEnter<DiversDare1>()
            .DeactivateOnExit<DiversDare1>()
            .DeactivateOnExit<AirPuddleCircle>()
            .DeactivateOnExit<AirPuddleCone>();
    }

    void PreEnrage(uint id, float delay)
    {
        ActorCastStartMulti(id, _module.B2, [AID.AlleyOopDoubleDipCast, AID.ReverseAlleyOopCast], delay)
            .ActivateOnEnter<AlleyOopProteans>()
            .ActivateOnEnter<AlleyOopProteanRepeat>();

        CastStart(id + 1, AID.AlleyOopInfernoCast, 2.8f)
            .ActivateOnEnter<AlleyOopInfernoSpread>()
            .ActivateOnEnter<AlleyOopInfernoPuddle>();

        ComponentCondition<AlleyOopProteans>(id + 2, 2.7f, p => p.NumCasts > 0, "Proteans")
            .DeactivateOnExit<AlleyOopProteans>();
        ComponentCondition<AlleyOopInfernoSpread>(id + 3, 2.2f, p => p.NumFinishedSpreads > 0, "Puddles")
            .DeactivateOnExit<AlleyOopInfernoSpread>();
        ComponentCondition<AlleyOopProteanRepeat>(id + 4, 0.4f, p => p.NumCasts > 0, "Proteans (repeat)")
            .DeactivateOnExit<AlleyOopProteanRepeat>()
            .ActivateOnEnter<Pyrotation>()
            .ActivateOnEnter<PyrotationPuddle>()
            .ActivateOnEnter<DeepImpactBait>()
            .ActivateOnEnter<DeepImpactKB>();

        ComponentCondition<Pyrotation>(id + 0x10, 8.6f, p => p.NumCasts > 0, "Stack start");
        ComponentCondition<DeepImpactBait>(id + 0x11, 0.3f, d => d.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<DeepImpactBait>()
            .DeactivateOnExit<DeepImpactKB>();
        ComponentCondition<Pyrotation>(id + 0x12, 3.8f, p => p.NumCasts == 3, "Stack finish")
            .DeactivateOnExit<Pyrotation>();

        Cast(id + 0x100, AID.DiversDareRed, 4.3f, 5, "Raidwide")
            .ActivateOnEnter<DiversDare1>()
            .DeactivateOnExit<AlleyOopInfernoPuddle>()
            .DeactivateOnExit<PyrotationPuddle>();

        Cast(id + 0x110, AID.DiversDareRed, 4.2f, 5, "Raidwide");
    }

    void AlleyOop(uint id, float delay)
    {
        ActorCastMulti(id, _module.B2, [AID.AlleyOopDoubleDipCast, AID.ReverseAlleyOopCast], delay, 5)
            .ActivateOnEnter<AlleyOopProteans>()
            .ActivateOnEnter<AlleyOopProteanRepeat>();

        ComponentCondition<AlleyOopProteans>(id + 0x10, 0.6f, p => p.NumCasts > 0, "Proteans")
            .DeactivateOnExit<AlleyOopProteans>();
        ComponentCondition<AlleyOopProteanRepeat>(id + 0x11, 2.6f, p => p.NumCasts > 0, "Proteans (repeat)")
            .DeactivateOnExit<AlleyOopProteanRepeat>();
    }
}
