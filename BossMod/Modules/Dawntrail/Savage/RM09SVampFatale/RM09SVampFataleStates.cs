namespace BossMod.Dawntrail.Savage.RM09SVampFatale;

class RM09SVampFataleStates : StateMachineBuilder
{
    public RM09SVampFataleStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        KillerVoice(id, 5.2f)
            .ActivateOnEnter<BrutalRain>();

        Hardcore(id + 0x10, 5.1f);
        Bombpyre(id + 0x20, 5.1f);
        BrutalRain(id + 0x100, 5)
            .DeactivateOnExit<BatRing>()
            .DeactivateOnExit<CurseOfTheBombpyre>();

        Saws1(id + 0x10000, 8.3f);
        Aetherletting(id + 0x20000, 8.5f);
        Saws2(id + 0x30000, 7.2f);
        HellInCell(id + 0x40000, 8.5f);

        Cast(id + 0x50000, AID.FinaleFataleEnrageCast, 30, 10)
            .ActivateOnEnter<FFEnrage>();
        ComponentCondition<FFEnrage>(id + 0x50002, 1.1f, f => f.NumCasts > 0, "Enrage");
    }

    class FFEnrage(BossModule module) : Components.CastCounter(module, AID.FinaleFataleEnrage);

    private void Saws1(uint id, float delay)
    {
        CastStart(id, AID.SadisticScreechCast, delay)
            .ActivateOnEnter<SadisticScreech>()
            .ActivateOnEnter<BuzzsawArena>()
            .DeactivateOnExit<BrutalRain>();

        ComponentCondition<SadisticScreech>(id + 1, 5.9f, s => s.NumCasts > 0, "Raidwide + arena change")
            .DeactivateOnExit<SadisticScreech>();

        Targetable(id + 0x10, false, 6.7f, "Boss disappears")
            .ClearHint(StateMachine.StateHint.DowntimeStart)
            .ActivateOnEnter<Coffinmaker>()
            .ActivateOnEnter<DeadWake>()
            .ActivateOnEnter<DeadWakeArena>()
            .ActivateOnEnter<CoffinFiller>()
            .ActivateOnEnter<HalfMoon>();

        ActorTargetable(id + 0x11, () => Module.Enemies(OID.Coffinmaker).FirstOrDefault(), true, 0.2f, "Buzzsaw appears");

        Timeout(id + 0x100, 62.7f, "Buzzsaw enrage")
            .DeactivateOnExit<Coffinmaker>()
            .DeactivateOnExit<DeadWake>()
            .DeactivateOnExit<DeadWakeArena>()
            .DeactivateOnExit<CoffinFiller>()
            .DeactivateOnExit<HalfMoon>();

        CastStart(id + 0x110, AID.SadisticScreechCast, 2.1f)
            .ActivateOnEnter<SadisticScreech>();

        ComponentCondition<SadisticScreech>(id + 0x111, 5.9f, s => s.NumCasts > 0, "Raidwide + arena change")
            .DeactivateOnExit<SadisticScreech>()
            .DeactivateOnExit<BuzzsawArena>();
    }

    private void Aetherletting(uint id, float delay)
    {
        CrowdKill(id, delay).ActivateOnEnter<RingBounds>();
        FinaleFatale(id + 0x10, 12.8f)
            .ActivateOnEnter<PulpingPulse>()
            .ActivateOnEnter<AetherlettingCone>()
            .ActivateOnEnter<AetherlettingCross>()
            .ActivateOnEnter<AetherlettingSpread>();

        ComponentCondition<AetherlettingCone>(id + 0x100, 18.9f, c => c.NumCasts > 0, "Cones start");
        ComponentCondition<AetherlettingCone>(id + 0x101, 6, c => c.NumCasts >= 8, "Cones finish")
            .DeactivateOnExit<AetherlettingCone>();

        ComponentCondition<AetherlettingSpread>(id + 0x110, 2, s => s.NumFinishedSpreads >= 8, "Spreads finish")
            .DeactivateOnExit<AetherlettingSpread>()
            .ExecOnExit<AetherlettingCross>(c => c.Draw = true);

        ComponentCondition<AetherlettingCross>(id + 0x120, 8.4f, c => c.NumCasts > 0, "Crosses start");
        ComponentCondition<AetherlettingCross>(id + 0x130, 6.1f, c => c.NumCasts >= 8, "Crosses finish")
            .DeactivateOnExit<AetherlettingCross>();

        Hardcore(id + 0x200, 1.9f);
        Bombpyre(id + 0x300, 5.2f)
            .DeactivateOnExit<BatRing>()
            .ActivateOnEnter<HalfMoon>();

        ComponentCondition<HalfMoon>(id + 0x400, 5, h => h.NumCasts > 0, "Half-room cleave 1");
        ComponentCondition<HalfMoon>(id + 0x401, 2.9f, h => h.NumCasts > 1, "Half-room cleave 2")
            .ActivateOnEnter<BrutalRain>()
            .DeactivateOnExit<HalfMoon>()
            .DeactivateOnExit<CurseOfTheBombpyre>()
            .DeactivateOnExit<PulpingPulse>();

        BrutalRain(id + 0x500, 7.2f);

        InsatiableThirst(id + 0x600, 8.1f);
    }

    private void Saws2(uint id, float delay)
    {
        CastStart(id, AID.SadisticScreechCast, delay)
            .ActivateOnEnter<SadisticScreech>()
            .ActivateOnEnter<BuzzsawArena>()
            .DeactivateOnExit<BrutalRain>();

        ComponentCondition<SadisticScreech>(id + 1, 5.9f, s => s.NumCasts > 0, "Raidwide + arena change")
            .DeactivateOnExit<SadisticScreech>();

        ComponentCondition<Plummet>(id + 0x10, 12.2f, p => p.NumCasts > 1, "Towers 1")
            .ActivateOnEnter<GravegrazerHorizontal>()
            .ActivateOnEnter<GravegrazerVertical>()
            .ActivateOnEnter<Plummet>()
            .ActivateOnEnter<Electrocution>()
            .ActivateOnEnter<ElectroPuddle>()
            .ActivateOnEnter<DeadlyDoornail>()
            .ActivateOnEnter<FatalFlail>();

        KillerVoice(id + 0x20, 4);
        ComponentCondition<Plummet>(id + 0x30, 9.1f, p => p.NumCasts > 3, "Towers 2");
        KillerVoice(id + 0x40, 4);
        ComponentCondition<Plummet>(id + 0x50, 9.1f, p => p.NumCasts > 5, "Towers 3")
            .DeactivateOnExit<Plummet>();

        CastStart(id + 0x110, AID.SadisticScreechCast, 18)
            .ActivateOnEnter<SadisticScreech>();

        ComponentCondition<SadisticScreech>(id + 0x111, 5.9f, s => s.NumCasts > 0, "Raidwide + arena change")
            .DeactivateOnExit<SadisticScreech>()
            .DeactivateOnExit<BuzzsawArena>()
            .DeactivateOnExit<GravegrazerHorizontal>()
            .DeactivateOnExit<GravegrazerVertical>()
            .DeactivateOnExit<Electrocution>()
            .DeactivateOnExit<ElectroPuddle>()
            .DeactivateOnExit<DeadlyDoornail>()
            .DeactivateOnExit<FatalFlail>();
    }

    private void HellInCell(uint id, float delay)
    {
        CrowdKill(id, delay);
        FinaleFatale(id + 0x10, 12.8f)
            .ActivateOnEnter<PulpingPulse>();

        ComponentCondition<BloodyBondage>(id + 0x100, 11.1f, b => b.NumCasts > 0, "Towers 1")
            .ActivateOnEnter<BloodyBondage>()
            .ActivateOnEnter<CharnelCell>()
            .ActivateOnEnter<UltrasonicSpreadTank>()
            .ActivateOnEnter<UltrasonicSpreadOther>()
            .ActivateOnEnter<UltrasonicAmp>()
            .ActivateOnEnter<UltrasonicCounter>();

        ComponentCondition<UltrasonicCounter>(id + 0x101, 7.9f, c => c.NumCasts > 0, "Stack/spread");
        ComponentCondition<UltrasonicCounter>(id + 0x102, 7.1f, c => c.NumCasts > 1, "Stack/spread");

        ComponentCondition<BloodyBondage>(id + 0x200, 7.3f, b => b.NumCasts > 4, "Towers 2");
        ComponentCondition<UltrasonicCounter>(id + 0x201, 7.8f, c => c.NumCasts > 2, "Stack/spread");
        ComponentCondition<UltrasonicCounter>(id + 0x202, 7.2f, c => c.NumCasts > 3, "Stack/spread")
            .DeactivateOnExit<UltrasonicCounter>()
            .DeactivateOnExit<UltrasonicAmp>()
            .DeactivateOnExit<UltrasonicSpreadTank>()
            .DeactivateOnExit<UltrasonicSpreadOther>()
            .DeactivateOnExit<BloodyBondage>();

        ComponentCondition<BloodyBondageBig>(id + 0x300, 15.5f, b => b.NumCasts > 0, "Shared towers")
            .ActivateOnEnter<BloodyBondageBig>()
            .ActivateOnEnter<BatTether>()
            .DeactivateOnExit<BloodyBondageBig>()
            .DeactivateOnExit<CharnelCell>();

        Cast(id + 0x400, AID.SanguineScratchCast, 4, 2.3f)
            .ActivateOnEnter<SanguineScratch>()
            .ActivateOnEnter<SanguineScratchRepeat>()
            .ActivateOnEnter<BatShapePredict>()
            .ActivateOnEnter<BreakdownDrop>()
            .ActivateOnEnter<BreakwingBeat>();

        ComponentCondition<SanguineScratch>(id + 0x402, 0.7f, s => s.NumCasts > 0, "Cones start")
            .ActivateOnEnter<BreakCounter>();

        ComponentCondition<BreakCounter>(id + 0x410, 12.8f, c => c.NumCasts > 1, "Shapes 1");

        Cast(id + 0x420, AID.SanguineScratchCast, 2.6f, 2.3f);
        ComponentCondition<SanguineScratch>(id + 0x422, 0.7f, s => s.NumCasts > 8, "Cones start");

        ComponentCondition<BreakCounter>(id + 0x430, 12.8f, c => c.NumCasts > 3, "Shapes 2")
            .ActivateOnEnter<BrutalRain>()
            .DeactivateOnExit<SanguineScratch>()
            .DeactivateOnExit<SanguineScratchRepeat>()
            .DeactivateOnExit<BatShapePredict>()
            .DeactivateOnExit<BreakdownDrop>()
            .DeactivateOnExit<BreakwingBeat>()
            .DeactivateOnExit<BreakCounter>()
            .DeactivateOnExit<BatTether>();

        BrutalRain(id + 0x440, 6.8f);

        Bombpyre(id + 0x500, 8.3f)
            .DeactivateOnExit<BatRing>()
            .DeactivateOnExit<BrutalRain>()
            .ActivateOnEnter<HalfMoon>();

        ComponentCondition<HalfMoon>(id + 0x600, 4.9f, h => h.NumCasts > 0, "Half-room cleave 1");
        ComponentCondition<HalfMoon>(id + 0x601, 3, h => h.NumCasts > 1, "Half-room cleave 2")
            .DeactivateOnExit<HalfMoon>()
            .DeactivateOnExit<CurseOfTheBombpyre>()
            .DeactivateOnExit<PulpingPulse>();

        Hardcore(id + 0x700, 2.1f);

        Cast(id + 0x800, AID.SanguineScratchCast, 9.3f, 2.3f)
            .ActivateOnEnter<SanguineScratch>()
            .ActivateOnEnter<SanguineScratchRepeat>();
        ComponentCondition<SanguineScratch>(id + 0x802, 0.7f, s => s.NumCasts > 0, "Cones start");
        ComponentCondition<SanguineScratchRepeat>(id + 0x810, 9.8f, s => s.NumCasts >= 40, "Cones finish");

        InsatiableThirst(id + 0x900, 4.7f);
        CrowdKill(id + 0xA00, 8.5f);
    }

    private State KillerVoice(uint id, float delay)
    {
        return Cast(id, AID.KillerVoice, delay, 5, "Raidwide")
            .ActivateOnEnter<KillerVoice>()
            .DeactivateOnExit<KillerVoice>();
    }

    private State CrowdKill(uint id, float delay)
    {
        Cast(id, AID.CrowdKillCast, delay, 0.5f)
            .ActivateOnEnter<CrowdKill>();
        return ComponentCondition<CrowdKill>(id + 2, 5.5f, c => c.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<CrowdKill>();
    }

    private State FinaleFatale(uint id, float delay)
    {
        CastMulti(id, [AID.FinaleFataleCast, AID.FinaleFataleCast2], delay, 5)
            .ActivateOnEnter<FinaleFatale>();
        return ComponentCondition<FinaleFatale>(id + 2, 1.2f, f => f.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<FinaleFatale>();
    }

    private State InsatiableThirst(uint id, float delay)
    {
        Cast(id, AID.InsatiableThirstCast, delay, 2.8f)
            .ActivateOnEnter<InsatiableThirst>();
        return ComponentCondition<InsatiableThirst>(id + 2, 3.3f, f => f.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<InsatiableThirst>();
    }

    private void Hardcore(uint id, float delay)
    {
        Cast(id, AID.HardcoreCast, delay, 3)
            .ActivateOnEnter<HardcoreSmall>()
            .ActivateOnEnter<HardcoreBig>()
            .ActivateOnEnter<HardcoreCounter>();

        ComponentCondition<HardcoreCounter>(id + 2, 2, h => h.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<HardcoreSmall>()
            .DeactivateOnExit<HardcoreBig>()
            .DeactivateOnExit<HardcoreCounter>();
    }

    private State Bombpyre(uint id, float delay)
    {
        CastStart(id, AID.VampStompCast, delay)
            .ActivateOnEnter<VampStomp>()
            .ActivateOnEnter<BatRing>()
            .ActivateOnEnter<BlastBeat>();

        ComponentCondition<VampStomp>(id + 2, 4.1f, v => v.NumCasts > 0, "Center AOE")
            .DeactivateOnExit<VampStomp>();

        ComponentCondition<BlastBeat>(id + 0x10, 5.1f, b => b.NumCasts > 1, "Bats 1")
            .ActivateOnEnter<CurseOfTheBombpyre>();
        ComponentCondition<BlastBeat>(id + 0x11, 3.5f, b => b.NumCasts > 4, "Bats 2");
        return ComponentCondition<BlastBeat>(id + 0x12, 3.5f, b => b.NumCasts > 9, "Bats 3")
            .DeactivateOnExit<BlastBeat>();
    }

    private State BrutalRain(uint id, float delay)
    {
        return ComponentCondition<BrutalRain>(id, delay, b => b.NumCasts > 0, "Stack start");
    }

    //private void XXX(uint id, float delay)
}
