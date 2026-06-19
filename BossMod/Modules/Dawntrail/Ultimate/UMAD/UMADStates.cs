namespace BossMod.Dawntrail.Ultimate.UMAD;

class UMADStates : StateMachineBuilder
{
    readonly UMAD _module;

    public UMADStates(BossModule module) : base(module)
    {
        _module = (UMAD)module;

        SimplePhase(0, P1, "P1")
            .Raw.Update = () => !Module.PrimaryActor.IsTargetable;
        SimplePhase(1, P2, "P2")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => _module.BossP2() is { IsTargetable: false, HPRatio: < 1 };
        SimplePhase(2, P3, "P3")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () => _module.ExdeathP3()?.IsDeadOrDestroyed == true && _module.ChaosP3()?.IsDeadOrDestroyed == true;
    }

    void P1(uint id)
    {
        P1RevoltingRuin(id, 10.2f);
        P1FireBlizzard1(id + 0x10000, 7.8f);
        P1WaveCannon(id + 0x20000, 3.4f);
        P1JudgmentBuster(id + 0x30000, 4.1f);
        P1Gravitas1(id + 0x40000, 7.2f);
        P1Gravitas2(id + 0x50000, 4.6f);
        P1JudgmentBuster(id + 0x60000, 3.6f);
        P1TeleTrouncing(id + 0x70000, 6.9f);
    }

    void P2(uint id)
    {
        ActorTargetable(id, _module.BossP2, true, 10.3f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        P2UltimateEmbrace(id + 0x10, 7.2f);
        P2Forsaken(id + 0x10000, 8.3f);
        P2Trine(id + 0x20000, 8.2f);

        // if the dps check is not met, he actually disappears after 3.3s
        ActorTargetable(id + 0x30000, _module.BossP2, false, 4.1f, "Boss disappears")
            .SetHint(StateMachine.StateHint.DowntimeStart);
    }

    void P3(uint id)
    {
        ActorCast(id, _module.KefkaP3, AID.AeroIIIAssault, 2.4f, 3, false, "Knockback")
            .ActivateOnEnter<P3KefkaIndicator>()
            .ActivateOnEnter<P3AeroIIIAssault>()
            .DeactivateOnExit<P3AeroIIIAssault>();

        ActorCast(id + 0x100, _module.KefkaP3, AID.DefinitionOfInsanity, 33.7f, 4, false);
        ActorTargetable(id + 0x200, _module.ExdeathP3, true, 3.1f, "Bosses appear")
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        P3Firewall(id + 0x300, 0.2f);
        P3BowelsOfAgony(id + 0x10000, 14.5f);
        P3UltimaBlaster(id + 0x20000, 9.2f);
        P3Earthquake(id + 0x30000, 0.9f);
        P3Blackhole(id + 0x40000, 10.4f);
    }

    void P1RevoltingRuin(uint id, float delay)
    {
        CastStart(id, AID.RevoltingRuinIIIFirst, delay)
            .ActivateOnEnter<P1RevoltingRuinIIIFirst>()
            .ActivateOnEnter<P1RevoltingRuinIIISecond>();

        ComponentCondition<P1RevoltingRuinIIIFirst>(id + 0x10, 5.1f, r => r.NumCasts > 0, "Tankbuster (1st aggro)");
        ComponentCondition<P1RevoltingRuinIIISecond>(id + 0x20, 3.1f, r => r.NumCasts > 0, "Tankbuster (2nd aggro)")
            .DeactivateOnExit<P1RevoltingRuinIIIFirst>()
            .DeactivateOnExit<P1RevoltingRuinIIISecond>();
    }

    void P1FireBlizzard1(uint id, float delay)
    {
        Cast(id, AID.GravenImage, delay, 3)
            .ActivateOnEnter<P1PulseWave>()
            .ActivateOnEnter<P1BlizzardIIIBlowout>()
            .ActivateOnEnter<P1FlagrantFireIII>()
            .ExecOnEnter<P1FlagrantFireIII>(f => f.EnableHints = false)
            .ExecOnEnter<P1BlizzardIIIBlowout>(b => b.Risky = false);

        CastStart(id + 0x10, AID.MysteryMagic, 3.2f);

        ComponentCondition<P1PulseWave>(id + 0x100, 2.6f, p => p.NumCasts > 0, "Knockback")
            .ExecOnExit<P1FlagrantFireIII>(f => f.EnableHints = true)
            .ExecOnExit<P1BlizzardIIIBlowout>(b => b.Risky = true)
            .DeactivateOnExit<P1PulseWave>();

        ComponentCondition<P1BlizzardIIIBlowout>(id + 0x110, 2.3f, p => p.NumCasts > 0, "Quadrants")
            .DeactivateOnExit<P1BlizzardIIIBlowout>();

        ComponentCondition<P1FlagrantFireIII>(id + 0x120, 0.8f, p => !p.Active, "Stack/spread")
            .DeactivateOnExit<P1FlagrantFireIII>();
    }

    void P1WaveCannon(uint id, float delay)
    {
        CastStart(id, AID.DoubleTroubleTrapCast, delay)
            .ActivateOnEnter<P1WaveCannon>();

        ComponentCondition<P1WaveCannon>(id + 0x10, 0.8f, p => p.NumCasts > 0, "Lasers")
            .ActivateOnEnter<P1Explosion>()
            .DeactivateOnExit<P1WaveCannon>();

        CastEnd(id + 0x20, 2.1f);

        ComponentCondition<P1Explosion>(id + 0x100, 1.5f, e => e.NumCasts > 0, "Towers")
            .ActivateOnEnter<P1DoubleTroubleTrap>()
            .DeactivateOnExit<P1Explosion>()
            .ExecOnExit<P1DoubleTroubleTrap>(t => t.EnableHints = true);

        CastStart(id + 0x110, AID.MysteryMagic, 2.7f)
            .ActivateOnEnter<P1DoubleTroubleTrapKB>()
            .ActivateOnEnter<P1BlizzardIIIBlowout>()
            .ActivateOnEnter<P1ThrummingThunderIII>()
            .ExecOnEnter<P1BlizzardIIIBlowout>(b => b.Risky = false)
            .ExecOnEnter<P1ThrummingThunderIII>(t => t.Risky = false);

        ComponentCondition<P1DoubleTroubleTrap>(id + 0x200, 1, e => e.NumCasts > 0, "Confetti 1")
            .DeactivateOnExit<P1DoubleTroubleTrap>()
            .DeactivateOnExit<P1DoubleTroubleTrapKB>()
            .ExecOnExit<P1BlizzardIIIBlowout>(b => b.Risky = true)
            .ExecOnExit<P1ThrummingThunderIII>(t => t.Risky = true);

        ComponentCondition<P1BlizzardIIIBlowout>(id + 0x210, 3.9f, b => b.NumCasts > 0, "AOEs")
            .DeactivateOnExit<P1BlizzardIIIBlowout>()
            .DeactivateOnExit<P1ThrummingThunderIII>();
    }

    void P1JudgmentBuster(uint id, float delay)
    {
        Cast(id, AID.LightOfJudgmentP1, delay, 5, "Raidwide")
            .ActivateOnEnter<P1LightOfJudgment>()
            .ActivateOnEnter<P1Hyperdrive>()
            .ExecOnEnter<P1Hyperdrive>(h => h.EnableHints = false)
            .DeactivateOnExit<P1LightOfJudgment>();

        ComponentCondition<P1Hyperdrive>(id + 0x10, 3.1f, h => h.NumCasts > 0, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .ExecOnEnter<P1Hyperdrive>(h => h.EnableHints = true);
        ComponentCondition<P1Hyperdrive>(id + 0x20, 4.2f, h => h.NumCasts > 2, "Tankbuster 3")
            .DeactivateOnExit<P1Hyperdrive>();
    }

    void P1Gravitas1(uint id, float delay)
    {
        Cast(id, AID.GravenImage, delay, 3);

        CastStart(id + 0x10, AID.BlizzardIIIBlowoutCast, 2.1f)
            .ActivateOnEnter<P1BlizzardIIIBlowout>()
            .ActivateOnEnter<P1GravitasVitrophyre>()
            .ActivateOnEnter<P1GravitasPuddle>();

        ComponentCondition<P1BlizzardIIIBlowout>(id + 0x20, 5, b => b.NumCasts > 0, "Quadrants")
            .DeactivateOnExit<P1BlizzardIIIBlowout>();
        ComponentCondition<P1GravitasVitrophyre>(id + 0x30, 0.1f, v => v.Stacks.Count == 0, "Stacks + puddles appear");
        ComponentCondition<P1GravitasVitrophyre>(id + 0x40, 4, g => g.Spreads.Count == 0, "Spreads")
            .ActivateOnEnter<P1GravitationalWaveIntemperateWill>()
            .DeactivateOnExit<P1GravitasVitrophyre>();

        P1RevoltingRuin(id + 0x100, 0.8f);

        ComponentCondition<P1GravitationalWaveIntemperateWill>(id + 0x200, 0.8f, p => p.NumCasts > 0, "Left/right")
            .DeactivateOnExit<P1GravitationalWaveIntemperateWill>();
    }

    void P1Gravitas2(uint id, float delay)
    {
        ComponentCondition<P1GravitasVitrophyre>(id, 0, g => g.Stacks.Count > 0)
            .ActivateOnEnter<P1GravitasVitrophyre>()
            .ExecOnEnter<P1GravitasVitrophyre>(p => p.SetNegativeOffset(1.9f));

        ComponentCondition<P1GravitasVitrophyre>(id + 0x10, delay, g => g.Stacks.Count == 0, "Stacks");

        ComponentCondition<P1GravitasVitrophyre>(id + 0x20, 4, g => g.Spreads.Count == 0, "Spreads")
            .ActivateOnEnter<P1GravitationalWaveIntemperateWill>()
            .ExecOnEnter<P1GravitationalWaveIntemperateWill>(w => w.Risky = false)
            .DeactivateOnExit<P1GravitasVitrophyre>();

        ComponentCondition<P1GravitationalWaveIntemperateWill>(id + 0x30, 4.5f, g => g.NumCasts > 0, "Left/right")
            .ActivateOnEnter<P1DoubleTroubleTrap>()
            .ActivateOnEnter<P1DoubleTroubleTrapKB>()
            .ActivateOnEnter<P1GravitasPuddleSoak>()
            .ExecOnEnter<P1GravitationalWaveIntemperateWill>(w => w.Risky = true)
            .DeactivateOnExit<P1GravitationalWaveIntemperateWill>();

        // might not have any confetti holders alive at this point depending on prog
        ComponentCondition<P1DoubleTroubleTrapCounter>(id + 0x100, 3.8f, t => t.Resolved, "Confetti 2")
            .ActivateOnEnter<P1DoubleTroubleTrapCounter>()
            .ExecOnEnter<P1DoubleTroubleTrapCounter>(c => c.Timeout = 3.8f)
            .DeactivateOnExit<P1DoubleTroubleTrap>()
            .DeactivateOnExit<P1DoubleTroubleTrapKB>()
            .DeactivateOnExit<P1DoubleTroubleTrapCounter>();

        // unsoaked puddles live 5.5s, but doing the mechanic correctly will despawn them earlier
        Timeout(id + 0x110, 5.5f, "Puddles disappear")
            .ExecOnEnter<P1GravitasPuddleSoak>(p => p.EnableHints = true)
            .DeactivateOnExit<P1GravitasPuddle>()
            .DeactivateOnExit<P1GravitasPuddleSoak>();
    }

    void P1TeleTrouncing(uint id, float delay)
    {
        Cast(id, AID.TeleTrouncingCast, delay, 5)
            .ActivateOnEnter<P1TelePortent>()
            .ActivateOnEnter<P1IdyllicWill>()
            .ActivateOnEnter<P1IndulgentWill>()
            .ActivateOnEnter<P1Arrow>();

        ComponentCondition<P1TelePortent>(id + 0x10, 7.7f, p => p.NumArrows > 0, "Arrows 1");

        CastStart(id + 0x100, AID.GravenImage, 1.3f);

        ComponentCondition<P1TelePortent>(id + 0x110, 1.7f, p => p.NumArrows > 8, "Arrows 2")
            .DeactivateOnExit<P1TelePortent>();

        ComponentCondition<P1DoubleTroubleTrapCounter>(id + 0x200, 5.5f, p => p.Resolved, "Confetti 3")
            .ActivateOnEnter<P1DoubleTroubleTrap>()
            .ActivateOnEnter<P1DoubleTroubleTrapKB>()
            .ActivateOnEnter<P1DoubleTroubleTrapCounter>()
            .ExecOnEnter<P1DoubleTroubleTrap>(p => p.EnableHints = true)
            .ExecOnEnter<P1DoubleTroubleTrapCounter>(c => c.Timeout = 5.5f)
            .DeactivateOnExit<P1DoubleTroubleTrap>()
            .DeactivateOnExit<P1DoubleTroubleTrapKB>()
            .DeactivateOnExit<P1DoubleTroubleTrapCounter>();

        ComponentCondition<P1IdyllicWillCounter>(id + 0x300, 5.6f, p => p.NumCasts > 0, "Stuns")
            .ActivateOnEnter<P1IdyllicWillCounter>()
            .ActivateOnEnter<P1StatueGaze>()
            .ExecOnEnter<P1IndulgentWill>(p => p.Draw = true)
            .ExecOnEnter<P1IdyllicWill>(p => p.Activate())
            .DeactivateOnExit<P1IdyllicWillCounter>()
            .DeactivateOnExit<P1IdyllicWill>()
            .DeactivateOnExit<P1IndulgentWill>()
            .SetHint(StateMachine.StateHint.DowntimeStart);

        Timeout(id + 0x310, 5.9f) // stuns end
            .SetHint(StateMachine.StateHint.DowntimeEnd)
            .DeactivateOnExit<P1Arrow>()
            .ExecOnExit<P1StatueGaze>(s => s.EnableHints = true);

        CastStart(id + 0x400, AID.MysteryMagic, 2)
            .ActivateOnEnter<P1ThrummingThunderIII>()
            .ActivateOnEnter<P1FlagrantFireIII>();

        Condition(id + 0x410, 5, () => Module.FindComponent<P1ThrummingThunderIII>()!.NumCasts > 0 && Module.FindComponent<P1StatueGaze>()!.NumCasts > 0, "Lightning + gaze")
            .DeactivateOnExit<P1ThrummingThunderIII>()
            .DeactivateOnExit<P1StatueGaze>();

        ComponentCondition<P1FlagrantFireIII>(id + 0x420, 0.7f, p => !p.Active, "Stack/spread")
            .DeactivateOnExit<P1FlagrantFireIII>();

        Targetable(id + 0x1000, false, 10.3f, "Boss disappears");
    }

    void P2UltimateEmbrace(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.UltimateEmbrace, delay, 5, true, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .ActivateOnEnter<P2UltimateEmbrace>()
            .DeactivateOnExit<P2UltimateEmbrace>();
    }

    void P2Forsaken(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.Forsaken, delay, 7, true, "Raidwide")
            .ActivateOnEnter<P2ForsakenRaidwide>()
            .ActivateOnEnter<P2PathOfLight>()
            .ActivateOnEnter<P2Shapes>()
            .ActivateOnEnter<P2StackSpread>()
            .ActivateOnEnter<P2Spellwave>()
            .ActivateOnEnter<P2AllThingsEnding>()
            .DeactivateOnExit<P2ForsakenRaidwide>();

        void TowerSet(uint id, int round, float delay = 5.1f)
        {
            var r1 = (round - 1) * 2 + 1;
            ComponentCondition<P2PathOfLight>(id, delay, p => p.NumCasts == 2 * r1, $"Towers {r1}");
            ComponentCondition<P2Shapes>(id + 1, 0.6f, s => s.NumCasts > 0, $"Shapes {r1}")
                .ExecOnEnter<P2Shapes>(s => s.Reset())
                .ExecOnEnter<P2PathOfLight>(p => p.EnableHints = false)
                .ExecOnExit<P2PathOfLight>(p => p.EnableHints = true);

            ComponentCondition<P2PastsEndFuturesEnd>(id + 0x10, 2.3f, p => p.Active)
                .ActivateOnEnter<P2AllThingsEndingBait>()
                .ActivateOnEnter<P2PastsEndFuturesEnd>();
            ComponentCondition<P2PastsEndFuturesEnd>(id + 0x11, 6.9f, p => !p.Active, "Clones")
                .DeactivateOnExit<P2PastsEndFuturesEnd>();

            ComponentCondition<P2PathOfLight>(id + 0x12, 0.2f, p => p.NumCasts == 2 * (r1 + 1), $"Towers {r1 + 1}");
            ComponentCondition<P2Shapes>(id + 0x13, 0.6f, s => s.NumCasts > 0, $"Shapes {r1 + 1}")
                .ExecOnEnter<P2Shapes>(s => s.Reset())
                .ExecOnExit<P2AllThingsEndingBait>(b => b.Draw = true)
                .ExecOnExit<P2PathOfLight>(p => p.EnableHints = false)
                .ExecOnExit<P2StackSpread>(p => p.EnableHints = false)
                .ExecOnExit<P2Spellwave>(p => p.EnableHints = false)
                .DeactivateOnExit<P2Shapes>(round == 4)
                .DeactivateOnExit<P2StackSpread>(round == 4)
                .DeactivateOnExit<P2Spellwave>(round == 4);

            ComponentCondition<P2AllThingsEndingBait>(id + 0x20, 5.4f, p => p.Casting, "Baits")
                .ExecOnExit<P2PathOfLight>(p => p.EnableHints = true)
                .ExecOnExit<P2StackSpread>(p => p.EnableHints = true)
                .ExecOnExit<P2Spellwave>(p => p.EnableHints = true)
                .DeactivateOnExit<P2AllThingsEndingBait>()
                .DeactivateOnExit<P2PathOfLight>(round == 4);
        }

        TowerSet(id + 0x100, 1, 13.2f);
        TowerSet(id + 0x200, 2);
        TowerSet(id + 0x300, 3);
        TowerSet(id + 0x400, 4);

        ComponentCondition<P2AllThingsEnding>(id + 0x430, 4.9f, p => p.Casters.Count == 0, "Baits resolve")
            .DeactivateOnExit<P2AllThingsEnding>();

        ActorCast(id + 0x1000, _module.BossP2, AID.LightOfJudgmentP2, 4.2f, 5, true, "Raidwide")
            .ActivateOnEnter<P2LightOfJudgment>()
            .DeactivateOnExit<P2LightOfJudgment>();
    }

    void P2Trine(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.TrineCast, delay, 3, true)
            .ActivateOnEnter<P2Trine>();

        ActorCastMulti(id + 0x10, _module.BossP2, [AID.WingsOfDestructionL, AID.WingsOfDestructionR], 3.1f, 4, true, "Left/right")
            .ActivateOnEnter<P2WingsOfDestructionLeftRight>()
            .DeactivateOnExit<P2WingsOfDestructionLeftRight>();

        ComponentCondition<P2Trine>(id + 0x100, 5.7f, p => p.NumCasts == 9, "Trines 1");
        ActorCastStart(id + 0x101, _module.BossP2, AID.WingsOfDestructionBusterCast, 0.5f, true)
            .ActivateOnEnter<P2WingsOfDestructionBuster>();
        ComponentCondition<P2Trine>(id + 0x102, 1.5f, p => p.NumCasts == 12, "Trines 2");
        ComponentCondition<P2Trine>(id + 0x103, 2.1f, p => p.NumCasts == 21, "Trines 3")
            .DeactivateOnExit<P2Trine>();
        ComponentCondition<P2WingsOfDestructionBuster>(id + 0x110, 0.4f, p => p.NumCasts > 0, "Tankbusters")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<P2WingsOfDestructionBuster>();

        P2UltimateEmbrace(id + 0x200, 2.1f);
    }

    State P3Firewall(uint id, float delay)
    {
        return ActorCast(id, _module.ChaosP3, AID.DecisiveBattleA, delay, 3, true, "Firewall")
            .ActivateOnEnter<P3Firewall>();
    }

    State P3ThunderIIIBuster(uint id, float delay)
    {
        var st = ActorCastStart(id, _module.ExdeathP3, AID.ThunderIIIBusterCast, delay, true)
            .ActivateOnEnter<P3ThunderIIIBuster>();
        ComponentCondition<P3ThunderIIIBuster>(id + 0x31, 5, p => p.NumCasts > 0, "Tankbuster hit 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P3ThunderIIIBuster>(id + 0x32, 3.1f, p => p.NumCasts > 1, "Tankbuster hit 2")
            .DeactivateOnExit<P3ThunderIIIBuster>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        return st;
    }

    void P3BowelsOfAgony(uint id, float delay)
    {
        ActorCast(id, _module.ChaosP3, AID.BowelsOfAgony, delay, 5, true, "Raidwide")
            .ActivateOnEnter<P3BowelsOfAgony>()
            .ActivateOnEnter<P3EntropyFluid>()
            .ActivateOnEnter<P3Cyclone>()
            .ActivateOnEnter<P3Crystals>(Service.IsDev)
            .ActivateOnEnter<P3HeadwindTailwind>()
            .DeactivateOnExit<P3BowelsOfAgony>();

        ActorCast(id + 0x10, _module.ExdeathP3, AID.ThunderIIICircle, 12.2f, 7, true, "Thunder + elements 1")
            .ActivateOnEnter<P3ThunderIII>()
            .ActivateOnEnter<P3InfernoTsunami>()
            .ExecOnEnter<P3EntropyFluid>(p => p.EnableHints = true)
            .ExecOnEnter<P3InfernoTsunami>(p => p.EnableHints = true)
            .DeactivateOnExit<P3ThunderIII>()
            .DeactivateOnExit<P3Firewall>(); // falls off when exdeath starts casting

        ComponentCondition<P3InfernoTsunami>(id + 0x20, 1, p => p.NumCasts > 0, "Crystals 1")
            .DeactivateOnExit<P3EntropyFluid>()
            .DeactivateOnExit<P3InfernoTsunami>();

        P3ThunderIIIBuster(id + 0x30, 3.2f);

        ActorCastStartMulti(id + 0x100, _module.ChaosP3, [AID.LongitudinalImplosion, AID.LatitudinalImplosion], 4.7f, true)
            .ActivateOnEnter<P3EntropyFluid>()
            .ActivateOnEnter<P3LatLongShockwave>();

        ComponentCondition<P3LatLongShockwave>(id + 0x110, 5.7f, p => p.NumCasts == 2, "Front/sides 1")
            .ActivateOnEnter<P3InfernoTsunami>();
        ComponentCondition<P3LatLongShockwave>(id + 0x111, 2, p => p.NumCasts == 4, "Front/sides 2")
            .ExecOnEnter<P3EntropyFluid>(p => p.EnableHints = true)
            .ExecOnEnter<P3InfernoTsunami>(p => p.EnableHints = true)
            .DeactivateOnExit<P3LatLongShockwave>();

        ComponentCondition<P3EntropyFluid>(id + 0x112, 2.2f, p => p.NumCasts == 2, "Elements 2");
        ComponentCondition<P3InfernoTsunami>(id + 0x113, 1, p => p.NumCasts > 0, "Crystals 2")
            .DeactivateOnExit<P3EntropyFluid>()
            .DeactivateOnExit<P3InfernoTsunami>();
    }

    void P3UltimaBlaster(uint id, float delay)
    {
        ActorCastStart(id, _module.ChaosP3, AID.UmbraSmash, delay, true, "Superjump bait")
            .ActivateOnEnter<P3UmbraSmashBait>()
            .ActivateOnEnter<P3UltimaBlasterRaidwide>()
            .ActivateOnEnter<P3UltimaBlasterCharge>()
            .ActivateOnEnter<P3UmbraSmash>()
            .DeactivateOnExit<P3UmbraSmashBait>()
            .ExecOnEnter<P3UltimaBlasterCharge>(c => c.Draw = false);
        ActorCastStart(id + 1, _module.ExdeathP3, AID.VacuumWave, 0.1f, true)
            .ActivateOnEnter<P3VacuumWave>();

        ComponentCondition<P3UltimaBlasterRaidwide>(id + 0x10, 0.8f, r => r.NumCasts > 0, "Raidwides start");

        ComponentCondition<P3UmbraSmash>(id + 0x20, 4.1f, p => p.NumCasts > 0, "Superjump")
            .DeactivateOnExit<P3UmbraSmash>();
        ComponentCondition<P3VacuumWave>(id + 0x30, 3.1f, p => p.NumCasts > 0, "Knockback")
            .DeactivateOnExit<P3VacuumWave>();

        ComponentCondition<P3Cyclone>(id + 0x100, 3.9f, c => c.NumCasts == 8, "Final cyclones")
            .DeactivateOnExit<P3Cyclone>()
            .DeactivateOnExit<P3HeadwindTailwind>()
            .DeactivateOnExit<P3Crystals>(Service.IsDev)
            .ExecOnExit<P3UltimaBlasterCharge>(b => b.Draw = true);

        ComponentCondition<P3UltimaBlasterCharge>(id + 0x110, 11, p => p.NumCasts > 0, "Charges start");
        ComponentCondition<P3UltimaBlasterCharge>(id + 0x111, 1.5f, p => p.NumCasts == 8, "Charges finish")
            .DeactivateOnExit<P3UltimaBlasterCharge>()
            .DeactivateOnExit<P3UltimaBlasterRaidwide>();
    }

    void P3Earthquake(uint id, float delay)
    {
        P3ThunderIIIBuster(id, delay);
        P3Firewall(id + 0x100, 1.8f);
        ActorCastEnd(id + 0x1FF, _module.ExdeathP3, 0.1f, true);
        P3ThunderIIIBuster(id + 0x200, 4.2f)
            .ActivateOnEnter<P3EarthquakeRaidwide>()
            .ActivateOnEnter<P3Nothingness>()
            .ActivateOnEnter<P3EarthHints>();
        ComponentCondition<P3EarthquakeRaidwide>(id + 0x210, 1.9f, p => p.NumCasts > 0, "Raidwide + debuffs")
            .DeactivateOnExit<P3EarthquakeRaidwide>();
    }

    void P3Blackhole(uint id, float delay)
    {
        ActorCastStartMulti(id, _module.KefkaP3, [AID.SlapHappyLeftHand, AID.SlapHappyRightHand], delay)
            .ActivateOnEnter<P3SlapHappy>()
            .ActivateOnEnter<P3SlapHappyShockwave>();

        ComponentCondition<P3SlapHappy>(id + 1, 5.8f, s => s.NumCasts > 0, "Slams start");
        ComponentCondition<P3SlapHappy>(id + 2, 2.6f, s => s.NumCasts >= 4, "Slams finish")
            .DeactivateOnExit<P3SlapHappy>();
        ComponentCondition<P3SlapHappyShockwave>(id + 0x10, 0.1f, s => s.Resolved, "Protean(s)")
            .DeactivateOnExit<P3SlapHappyShockwave>();

        ComponentCondition<P3Nothingness>(id + 0x100, 7.4f, n => n.NumCasts == 1, "Laser 1")
            .ActivateOnEnter<P3Blackhole>();

        ActorCastStart(id + 0x101, _module.ExdeathP3, AID.ThunderIIIBusterCast, 5.3f, true)
            .ActivateOnEnter<P3ThunderIIIBuster>();
        ComponentCondition<P3Nothingness>(id + 0x102, 1.6f, n => n.NumCasts == 3, "Lasers 2");
        ComponentCondition<P3ThunderIIIBuster>(id + 0x103, 3.4f, t => t.NumCasts > 0, "Tankbuster hit 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<P3ThunderIIIBuster>(id + 0x104, 3.1f, t => t.NumCasts > 1, "Tankbuster hit 2")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .DeactivateOnExit<P3ThunderIIIBuster>();

        ActorCastStart(id + 0x200, _module.ChaosP3, AID.DamningEdict, 0, true)
            .ActivateOnEnter<P3DamningEdict>();
        ActorCastStartMulti(id + 0x201, _module.KefkaP3, [AID.SlapHappyLeftHand, AID.SlapHappyRightHand], 1.3f)
            .ActivateOnEnter<P3SlapHappy>()
            .ActivateOnEnter<P3SlapHappyShockwave>()
            .ExecOnEnter<P3SlapHappyShockwave>(s => s.EnableHints = false);

        ComponentCondition<P3DamningEdict>(id + 0x202, 3.7f, d => d.NumCasts > 0, "Cleave")
            .DeactivateOnExit<P3DamningEdict>()
            .ExecOnExit<P3SlapHappyShockwave>(s => s.EnableHints = true);

        ComponentCondition<P3SlapHappy>(id + 0x210, 2.1f, s => s.NumCasts > 0, "Slams start");
        ComponentCondition<P3SlapHappy>(id + 0x211, 2.6f, s => s.NumCasts >= 4, "Slams finish")
            .DeactivateOnExit<P3SlapHappy>();
        ComponentCondition<P3SlapHappyShockwave>(id + 0x220, 0.1f, s => s.Resolved, "Protean(s)")
            .DeactivateOnExit<P3SlapHappyShockwave>();

        Timeout(id + 0xFF0000, 10000, "???")
            .ActivateOnEnter<P3DamningEdict>()
            .ActivateOnEnter<P3HotTail>();
    }
}
