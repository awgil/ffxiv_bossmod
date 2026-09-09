namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class RM08SHowlingBladeStates : StateMachineBuilder
{
    private readonly RM08SHowlingBlade _module;

    public RM08SHowlingBladeStates(RM08SHowlingBlade module) : base(module)
    {
        _module = module;
        SimplePhase(0, Phase1, "P1")
            .Raw.Update = () => module.PrimaryActor.IsDeadOrDestroyed || module.PrimaryActor.HPMP.CurHP == 1;

        SimplePhase(1, Phase2, "P2")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && (_module.BossP2()?.IsDeadOrDestroyed ?? true);
    }

    private void Phase1(uint id)
    {
        ExtraplanarPursuit(id, 10.2f);
        WindStonefang(id + 0x10000, 8.9f);
        RevolutionaryReign(id + 0x20000, 5.1f);
        ExtraplanarPursuit(id + 0x30000, 2.2f);
        MillennialDecay(id + 0x40000, 8.5f);
        TrackingTremors(id + 0x50000, 0.8f);
        ExtraplanarPursuit(id + 0x60000, 1.8f);
        TerrestrialTitans(id + 0x70000, 3.8f);
        RevolutionaryReign(id + 0x80000, 0.5f);
        TacticalPack(id + 0x90000, 9.2f);
        TerrestrialRage(id + 0xA0000, 14.5f);
        RevolutionaryReign(id + 0xB0000, 0, castRemaining: 4.7f)
            .ActivateOnEnter<WealOfStone>()
            .ExecOnExit<WealOfStone>(w => w.Risky = true);
        ComponentCondition<WealOfStone>(id + 0xB0100, 2.9f, w => w.NumCasts > 0, "Line AOEs")
            .DeactivateOnExit<WealOfStone>();
        GreatDivide(id + 0xB0110, 5.4f);
        BeckonMoonlight(id + 0xC0000, 11.4f);
        WindStonefang(id + 0xD0000, 0, castRemaining: 4.9f);
        TrackingTremors(id + 0xE0000, 4.1f);
        ExtraplanarPursuit(id + 0xF0000, 1.8f);
        ExtraplanarPursuit(id + 0xF0100, 10.9f);
        Targetable(id + 0xF0200, false, 0.8f, "Boss disappears");
        Cast(id + 0xF0300, AID.ExtraplanarFeast, 0.1f, 4, "Enrage");
    }

    private void Phase2(uint id)
    {
        ComponentCondition<P2Arena>(id, 22, p => p.Active)
            .ActivateOnEnter<P2Arena>()
            .ActivateOnEnter<DestructiblePlatforms>()
            .DeactivateOnExit<P2Arena>()
            .OnExit(() => Module.Arena.Bounds = RM08SHowlingBlade.BoundsP2);

        ActorTargetable(id + 1, _module.BossP2, true, 24.4f, "Boss reappears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        QuakeIII(id + 0x100, 7.3f);
        UltraviolentRay(id + 0x200, 6.1f);
        Twinbite(id + 0x300, 4.1f);

        HerosBlow(id + 0x10000, 5.1f);
        UltraviolentRay(id + 0x10100, 4.1f);
        QuakeIII(id + 0x10200, 6.1f);

        Mooncleaver(id + 0x20000, 8.2f);
        TwofoldTempest(id + 0x30000, 14);
        ChampionsCircuit(id + 0x40000, 2.1f);
        LoneWolfsLament(id + 0x50000, 8.1f);

        HowlingEight(id + 0x60000, 7);

        ActorCast(id + 0x61000, _module.BossP2, AID.StarcleaverVisual, 0.4f, 10);
        Timeout(id + 0x61010, 1, "Enrage");
    }

    private void ExtraplanarPursuit(uint id, float delay)
    {
        CastStart(id, AID.ExtraplanarPursuitVisual, delay)
            .ActivateOnEnter<ExtraplanarPursuit>();
        ComponentCondition<ExtraplanarPursuit>(id + 1, 4, e => e.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<ExtraplanarPursuit>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void WindStonefang(uint id, float delay, float castRemaining = 6)
    {
        CastMulti(id, [AID.WindfangIntercards, AID.WindfangCards, AID.StonefangCards, AID.StonefangIntercards], delay, castRemaining, "In/out")
            .ActivateOnEnter<WindfangStonefangCross>()
            .ActivateOnEnter<WindfangDonut>()
            .ActivateOnEnter<StonefangCircle>()
            .ActivateOnEnter<WindfangStonefang>()
            .ActivateOnEnter<WindfangStonefangAI>();

        ComponentCondition<WindfangStonefang>(id + 2, 0.1f, w => w.NumCasts > 0, "Stack/spread")
            .DeactivateOnExit<WindfangStonefangCross>()
            .DeactivateOnExit<WindfangDonut>()
            .DeactivateOnExit<StonefangCircle>()
            .DeactivateOnExit<WindfangStonefang>()
            .DeactivateOnExit<WindfangStonefangAI>();
    }

    private State RevolutionaryReign(uint id, float delay, float castRemaining = 5.1f)
    {
        CastMulti(id, [AID.EminentReignVisual1, AID.RevolutionaryReignVisual1, AID.EminentReignVisual2, AID.RevolutionaryReignVisual2], delay, castRemaining)
            .ActivateOnEnter<ReignJumpCounter>()
            .ActivateOnEnter<ReignHints>()
            .ActivateOnEnter<WolvesReign>()
            .ActivateOnEnter<ReignInout>()
            .ActivateOnEnter<WolvesReignRect>();

        ComponentCondition<ReignJumpCounter>(id + 2, 1.9f, e => e.NumCasts > 0, "Boss jump")
            .DeactivateOnExit<ReignJumpCounter>()
            .DeactivateOnExit<WolvesReign>();
        ComponentCondition<WolvesReignRect>(id + 3, 2.5f, w => w.NumCasts > 0, "Line AOE")
            .DeactivateOnExit<WolvesReignRect>();
        return ComponentCondition<ReignInout>(id + 4, 3.1f, r => r.NumCasts > 0, "In/out")
            .ActivateOnEnter<ReignsEnd>()
            .ActivateOnEnter<SovereignScar>()
            .ExecOnEnter<ReignInout>(r => r.Risky = true)
            .DeactivateOnExit<ReignInout>()
            .DeactivateOnExit<SovereignScar>()
            .DeactivateOnExit<ReignsEnd>()
            .DeactivateOnExit<ReignHints>();
    }

    private void MillennialDecay(uint id, float delay)
    {
        Cast(id, AID.MillennialDecay, delay, 5, "Raidwide")
            .ActivateOnEnter<MillennialDecay>()
            .ActivateOnEnter<BreathOfDecay>()
            .ActivateOnEnter<Gust>()
            .ActivateOnEnter<AeroIII>()
            .ActivateOnEnter<ProwlingGale>()
            .DeactivateOnExit<MillennialDecay>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ComponentCondition<AeroIII>(id + 0x10, 10.7f, e => e.NumCasts > 0, "Knockback");

        ComponentCondition<BreathOfDecay>(id + 0x11, 1.5f, b => b.NumCasts > 0, "Line AOE 1");

        ComponentCondition<Gust>(id + 0x12, 0.5f, g => g.NumFinishedSpreads > 0, "Spreads 1");
        Timeout(id + 0x13, 5.1f, "Spreads 2")
            .DeactivateOnExit<Gust>();
        ComponentCondition<BreathOfDecay>(id + 0x14, 2.5f, b => b.NumCasts > 4, "Line AOE 5")
            .ActivateOnEnter<WindsOfDecay>()
            .ActivateOnEnter<WindsOfDecayTether>()
            .DeactivateOnExit<BreathOfDecay>();

        ComponentCondition<AeroIII>(id + 0x20, 6.2f, a => a.NumCasts > 1, "Knockback")
            .DeactivateOnExit<AeroIII>()
            .ExecOnExit<WindsOfDecay>(w => w.EnableHints = true)
            .ExecOnExit<WindsOfDecayTether>(w => w.EnableHints = true);

        ComponentCondition<ProwlingGale>(id + 0x22, 2.2f, p => p.NumCasts > 0, "Towers")
            .DeactivateOnExit<ProwlingGale>();
        ComponentCondition<WindsOfDecay>(id + 0x23, 0.2f, w => w.NumCasts > 0, "Baits")
            .DeactivateOnExit<WindsOfDecay>()
            .DeactivateOnExit<WindsOfDecayTether>();
    }

    private void TrackingTremors(uint id, float delay)
    {
        CastStart(id, AID.TrackingTremorsVisual, delay)
            .ActivateOnEnter<TrackingTremors>()
            .ActivateOnEnter<TrackingTremorsStack>();

        CastEnd(id + 1, 5);

        ComponentCondition<TrackingTremors>(id + 2, 0.9f, t => t.NumCasts > 0, "Stack 1");

        ComponentCondition<TrackingTremors>(id + 5, 7.5f, t => t.NumCasts == 8, "Stack 8")
            .DeactivateOnExit<TrackingTremors>()
            .DeactivateOnExit<TrackingTremorsStack>();
    }

    private void GreatDivide(uint id, float delay)
    {
        Cast(id, AID.GreatDivide, delay, 5, "Tankbuster")
            .ActivateOnEnter<GreatDivide>()
            .DeactivateOnExit<GreatDivide>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void TerrestrialTitans(uint id, float delay)
    {
        GreatDivide(id, delay);

        Cast(id + 0x10, AID.TerrestrialTitansVisual, 11, 4, "Pillars appear")
            .ActivateOnEnter<TerrestrialTitans>()
            .DeactivateOnExit<TerrestrialTitans>();

        CastStart(id + 0x20, AID.TitanicPursuitVisual, 3.2f)
            .ActivateOnEnter<TitanicPursuit>()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<FangedCrossing>();

        ComponentCondition<TitanicPursuit>(id + 0x21, 4, t => t.NumCasts > 0, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<TitanicPursuit>();

        ComponentCondition<FangedCrossing>(id + 0x30, 7.9f, f => f.NumCasts > 0, "Safe spot")
            .DeactivateOnExit<FangedCrossing>()
            .DeactivateOnExit<Towerfall>();
    }

    private void TacticalPack(uint id, float delay)
    {
        Cast(id, AID.TacticalPack, delay, 3)
            .ActivateOnEnter<HowlingHavoc>()
            .ActivateOnEnter<AddsVoidzone>()
            .ActivateOnEnter<WolfOfWindStone>()
            .ActivateOnEnter<StalkingWindStone>()
            .ActivateOnEnter<AlphaWindStone>()
            .ActivateOnEnter<ForlornWindStone>();

        Targetable(id + 0x10, false, 2, "Boss disappears");

        id += 0x20;

        ComponentCondition<HowlingHavoc>(id, 7.2f, h => h.NumCasts > 0, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<HowlingHavoc>();

        ComponentCondition<WolfOfWindStone>(id + 1, 2, w => w.WolfOfStone != null, "Adds appear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ComponentCondition<StalkingWindStone>(id + 0x10, 3, s => s.Baits.Count > 0)
            .ActivateOnEnter<EarthyWindborneEnd>();

        ComponentCondition<StalkingWindStone>(id + 0x11, 5.1f, s => s.NumCasts > 0, "Baits 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<StalkingWindStone>(id + 0x12, 14.2f, s => s.NumCasts > 2, "Baits 2")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<StalkingWindStone>(id + 0x13, 14.2f, s => s.NumCasts > 4, "Baits 3")
            .ActivateOnEnter<RavenousSaber>()
            .SetHint(StateMachine.StateHint.Tankbuster);

        // boss reappears early if both adds die, 22.5 is a rough estimate for the fixed timer
        Targetable(id + 0x20, true, 22.5f, "Boss reappears");

        Timeout(id + 0x30, 5, "Adds enrage")
            .DeactivateOnExit<AddsVoidzone>()
            .DeactivateOnExit<WolfOfWindStone>()
            .DeactivateOnExit<StalkingWindStone>()
            .DeactivateOnExit<AlphaWindStone>()
            .DeactivateOnExit<ForlornWindStone>()
            .DeactivateOnExit<EarthyWindborneEnd>();

        ComponentCondition<RavenousSaber>(id + 0x40, 8, r => r.NumCasts > 0, "Raidwide 1")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<RavenousSaber>(id + 0x42, 3.9f, r => r.NumCasts > 4, "Raidwide 5")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<RavenousSaber>();
    }

    private void TerrestrialRage(uint id, float delay)
    {
        Cast(id, AID.TerrestrialRage, delay, 3)
            .ActivateOnEnter<FangedCharge>()
            .ActivateOnEnter<HeavensearthSuspendedStone>()
            .ActivateOnEnter<Shadowchase>()
            .ActivateOnEnter<RoaringWind>();

        ComponentCondition<FangedCharge>(id + 0x10, 7, f => f.NumCasts > 0, "Lines 1");
        ComponentCondition<HeavensearthSuspendedStone>(id + 0x20, 1.4f, s => s.NumFinishedStacks > 0, "Stack/spread 1");
        ComponentCondition<FangedCharge>(id + 0x30, 1.2f, f => f.NumCasts > 2, "Lines 2")
            .DeactivateOnExit<FangedCharge>();
        Targetable(id + 0x32, false, 0.9f, "Boss disappears");
        ComponentCondition<HeavensearthSuspendedStone>(id + 0x40, 4.7f, h => h.NumFinishedStacks > 1, "Stack/spread 2")
            .ActivateOnEnter<TRHints>()
            .DeactivateOnExit<HeavensearthSuspendedStone>()
            .DeactivateOnExit<TRHints>();
        ComponentCondition<Shadowchase>(id + 0x50, 0.3f, s => s.NumCasts > 0, "Lines 3")
            .DeactivateOnExit<Shadowchase>()
            .ExecOnExit<RoaringWind>(r => r.Enabled = true);
        Targetable(id + 0x52, true, 2, "Boss reappears");
        ComponentCondition<RoaringWind>(id + 0x60, 2.6f, r => r.NumCasts > 0, "Lines 4");
    }

    private void BeckonMoonlight(uint id, float delay)
    {
        Cast(id, AID.BeckonMoonlight, delay, 3)
            .ActivateOnEnter<MoonbeamsBite>()
            .ActivateOnEnter<QuadHints>()
            .ActivateOnEnter<WealOfStone2>()
            .ActivateOnEnter<HeavensearthSuspendedStone>();

        ComponentCondition<HeavensearthSuspendedStone>(id + 0x10, 12.7f, s => s.NumFinishedStacks > 0, "Stack/spread 1");
        ComponentCondition<MoonbeamsBite>(id + 0x11, 1.4f, m => m.NumCasts > 0, "Cleave 1")
            .ExecOnExit<QuadHints>(q => q.Advance());
        ComponentCondition<MoonbeamsBite>(id + 0x12, 2, m => m.NumCasts > 1, "Cleave 2");
        ComponentCondition<MoonbeamsBite>(id + 0x13, 2, m => m.NumCasts > 2, "Cleave 3")
            .DeactivateOnExit<QuadHints>();
        ComponentCondition<MoonbeamsBite>(id + 0x14, 2, m => m.NumCasts > 3, "Cleave 4");
        ComponentCondition<HeavensearthSuspendedStone>(id + 0x15, 1.1f, s => s.NumFinishedStacks > 1, "Stack/spread 2")
            .DeactivateOnExit<MoonbeamsBite>()
            .DeactivateOnExit<HeavensearthSuspendedStone>();

        ComponentCondition<WealOfStone2>(id + 0x20, 4.3f, w => w.NumCasts > 0, "Safe corner")
            .ExecOnEnter<WealOfStone2>(w => w.Risky = true)
            .DeactivateOnExit<WealOfStone2>();
    }

    private void QuakeIII(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2, AID.QuakeIIICast, delay, true)
            .ActivateOnEnter<QuakeIII>();

        ComponentCondition<QuakeIII>(id + 1, 5.1f, q => q.NumCasts > 0, "Platform stacks")
            .DeactivateOnExit<QuakeIII>();
    }

    private void UltraviolentRay(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2, AID.UltraviolentRayCast, delay, true)
            .ActivateOnEnter<UVRBait>()
            .ActivateOnEnter<GleamingBeam>();

        ComponentCondition<UVRBait>(id + 2, 0, b => b.Baits.Count > 0, "Baits appear");

        ComponentCondition<UVRBait>(id + 0x10, 6, b => b.NumCasts > 0, "Baits/cleaves")
            .DeactivateOnExit<UVRBait>()
            .DeactivateOnExit<GleamingBeam>();
    }

    private void Twinbite(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2, AID.TwinbiteCast, delay, true)
            .ActivateOnEnter<Twinbite>();
        ComponentCondition<Twinbite>(id + 0x10, 7.1f, t => t.NumCasts > 0, "Tankbusters")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<Twinbite>();
    }

    private void HerosBlow(uint id, float delay)
    {
        ActorCastMulti(id, _module.BossP2, [AID.HerosBlow2Cast, AID.HerosBlow1Cast], delay, 6, true)
            .ActivateOnEnter<HerosBlow>()
            .ActivateOnEnter<HerosBlowInOut>()
            .ActivateOnEnter<FangedMaw>()
            .ActivateOnEnter<FangedPerimeter>();

        ComponentCondition<HerosBlowInOut>(id + 0x10, 1, h => h.NumCasts > 0, "Cleave + in/out")
            .DeactivateOnExit<HerosBlow>()
            .DeactivateOnExit<HerosBlowInOut>()
            .DeactivateOnExit<FangedMaw>()
            .DeactivateOnExit<FangedPerimeter>();
    }

    private void Mooncleaver(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2, AID.MooncleaverCast, delay, true)
            .ActivateOnEnter<Mooncleaver>();

        ComponentCondition<Mooncleaver>(id + 0x10, 5, m => m.NumCasts > 0, "Destroy platform")
            .ActivateOnEnter<TemporalBlast>()
            .ActivateOnEnter<HuntersHarvest>()
            .ActivateOnEnter<ElementalPurgeBind>()
            .DeactivateOnExit<Mooncleaver>();

        ComponentCondition<ElementalPurgeBind>(id + 0x20, 7.1f, e => e.Bound, "Bind tank")
            .DeactivateOnExit<ElementalPurgeBind>();

        ComponentCondition<TemporalBlast>(id + 0x30, 5.1f, t => t.NumFinishedSpreads > 0 && t.NumFinishedStacks > 0, "Stack + spread + bait")
            .DeactivateOnExit<TemporalBlast>()
            .DeactivateOnExit<HuntersHarvest>();
    }

    private void TwofoldTempest(uint id, float delay)
    {
        ComponentCondition<ProwlingGale2>(id, delay, p => p.NumCasts > 0, "Towers")
            .ActivateOnEnter<ProwlingGale2>()
            .DeactivateOnExit<ProwlingGale2>();

        ComponentCondition<TwofoldStack>(id + 0x10, 11.6f, t => t.NumFinishedStacks > 0, "Stack + line 1")
            .ActivateOnEnter<TwofoldStack>()
            .ActivateOnEnter<TwofoldTether>()
            .ActivateOnEnter<TwofoldLineBait>()
            .ActivateOnEnter<TwofoldVoidzone>();

        ComponentCondition<TwofoldStack>(id + 0x20, 7.1f, t => t.NumFinishedStacks > 1, "Stack + line 2");
        ComponentCondition<TwofoldStack>(id + 0x30, 7.1f, t => t.NumFinishedStacks > 2, "Stack + line 3");
        ComponentCondition<TwofoldStack>(id + 0x40, 7.1f, t => t.NumFinishedStacks > 3, "Stack + line 4")
            .DeactivateOnExit<TwofoldTether>()
            .DeactivateOnExit<TwofoldStack>()
            .DeactivateOnExit<TwofoldLineBait>();

        ComponentCondition<DestructiblePlatforms>(id + 0x50, 5.9f, d => d.ReappearCounter > 0, "Platform reappear")
            .DeactivateOnExit<TwofoldVoidzone>();
    }

    private void ChampionsCircuit(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.BareFangs3, delay, 4)
            .ActivateOnEnter<CCCone>()
            .ActivateOnEnter<CCDonutLarge1>()
            .ActivateOnEnter<CCDonutLarge2>()
            .ActivateOnEnter<CCDonutSmall>()
            .ActivateOnEnter<CCRect>()
            .ActivateOnEnter<GleamingBarrage>();

        ActorCastStartMulti(id + 0x10, _module.BossP2, [AID.ChampionsCircuitCCW, AID.ChampionsCircuitCW], 4.2f, true, "CC start");

        ComponentCondition<GleamingBarrage>(id + 0x20, 7.3f, g => g.NumCasts > 0, "Cleaves 1");
        ComponentCondition<CCCone>(id + 0x21, 0.6f, c => c.NumCasts > 0, "AOEs 1");
        ComponentCondition<GleamingBarrage>(id + 0x22, 3.7f, g => g.NumCasts > 5, "Cleaves 2");
        ComponentCondition<CCCone>(id + 0x23, 0.7f, c => c.NumCasts > 1, "AOEs 2");
        ComponentCondition<GleamingBarrage>(id + 0x24, 3.7f, g => g.NumCasts > 10, "Cleaves 3");
        ComponentCondition<CCCone>(id + 0x25, 0.7f, c => c.NumCasts > 2, "AOEs 3");
        ComponentCondition<GleamingBarrage>(id + 0x26, 3.7f, g => g.NumCasts > 15, "Cleaves 4");
        ComponentCondition<CCCone>(id + 0x27, 0.7f, c => c.NumCasts > 3, "AOEs 4");
        ComponentCondition<GleamingBarrage>(id + 0x28, 3.7f, g => g.NumCasts > 20, "Cleaves 5");
        ComponentCondition<CCCone>(id + 0x29, 0.7f, c => c.NumCasts > 4, "AOEs 5")
            .DeactivateOnExit<CCCone>()
            .DeactivateOnExit<CCDonutLarge1>()
            .DeactivateOnExit<CCDonutLarge2>()
            .DeactivateOnExit<CCDonutSmall>()
            .DeactivateOnExit<CCRect>()
            .DeactivateOnExit<GleamingBarrage>();

        QuakeIII(id + 0x100, 4.7f);
        UltraviolentRay(id + 0x200, 8);
        Twinbite(id + 0x300, 4.1f);
    }

    private void LoneWolfsLament(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.RiseOfTheHuntersBlade, delay, 7, true);

        ActorCastStart(id + 0x10, _module.BossP2, AID.LoneWolfsLament, 2.2f, true)
            .ActivateOnEnter<LoneWolfTethers>()
            .ActivateOnEnter<LoneWolfTowers>();

        ComponentCondition<LoneWolfTethers>(id + 0x20, 3.7f, l => l.Assignments.Any(r => r != default), "Tethers appear");

        ComponentCondition<LoneWolfTowers>(id + 0x30, 18.3f, w => w.NumCasts > 0, "Towers")
            .DeactivateOnExit<LoneWolfTethers>()
            .DeactivateOnExit<LoneWolfTowers>();

        HerosBlow(id + 0x100, 2.4f);
        UltraviolentRay(id + 0x200, 6.3f);
    }

    private void HowlingEight(uint id, float delay)
    {
        ActorCastStart(id, _module.BossP2, AID.HowlingEightFirstCast, delay, true)
            .ActivateOnEnter<HowlingEight>()
            .ActivateOnEnter<MooncleaverEnrage>();
        HowlingEightTower(id + 0x10, 9.1f);
        ActorCastStart(id + 0x20, _module.BossP2, AID.HowlingEightRestCast, 3.2f, true)
            .ActivateOnEnter<HowlingEight>()
            .ActivateOnEnter<MooncleaverEnrage>();
        HowlingEightTower(id + 0x30, 6.2f);
        ActorCastStart(id + 0x40, _module.BossP2, AID.HowlingEightRestCast, 3.2f, true)
            .ActivateOnEnter<HowlingEight>()
            .ActivateOnEnter<MooncleaverEnrage>();
        HowlingEightTower(id + 0x50, 6.2f);
        ActorCastStart(id + 0x60, _module.BossP2, AID.HowlingEightRestCast, 3.2f, true)
            .ActivateOnEnter<HowlingEight>()
            .ActivateOnEnter<MooncleaverEnrage>();
        HowlingEightTower(id + 0x70, 6.2f);
        ActorCastStart(id + 0x80, _module.BossP2, AID.HowlingEightRestCast, 3.2f, true)
            .ActivateOnEnter<HowlingEight>();
        HowlingEightTower(id + 0x90, 6.2f, destroy: false);
    }

    private void HowlingEightTower(uint id, float delay, bool destroy = true)
    {
        ComponentCondition<HowlingEight>(id, delay, e => e.NumCasts == 1, "Stack 1");
        ComponentCondition<HowlingEight>(id + 1, 6, e => e.NumCasts == 8, "Stack 8")
            .DeactivateOnExit<HowlingEight>();
        if (destroy)
            ComponentCondition<MooncleaverEnrage>(id + 2, 4.3f, m => m.NumCasts > 0, "Destroy platform")
                .DeactivateOnExit<MooncleaverEnrage>();
    }
}
