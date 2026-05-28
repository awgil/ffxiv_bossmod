namespace BossMod.Dawntrail.Extreme.Ex8Enuo;

class Ex8EnuoStates : StateMachineBuilder
{
    readonly Ex8Enuo _module;

    public Ex8EnuoStates(Ex8Enuo module) : base(module)
    {
        _module = module;

        SimplePhase(0, id =>
        {
            P1(id);
            Intermission(id + 0x20000);
        }, "P1 + intermission")
            .Raw.Update = () => module.FindComponent<ArenaSwitcher>()?.IntermissionOver == true || module.PrimaryActor.IsDeadOrDestroyed;
        DeathPhase(1, P2).SetHint(StateMachine.PhaseHint.StartWithDowntime);
    }

    void P1(uint id)
    {
        Meteorain(id, 9.2f);
        NaughtGrows(id + 0x100, 7.5f);
        Cast(id + 0x150, AID.NaughtWakes, 5.2f, 2);
        Meltdown(id + 0x200, 2.2f);
        Emptiness(id + 0x300, 1.8f);
        NaughtGrows(id + 0x400, 2.5f);
        GazeOfTheVoid(id + 0x500, 8.7f);

        Vacuum(id + 0x10000, 1.5f);
        Emptiness(id + 0x10100, 0.2f);
        DeepFreeze(id + 0x10200, 5.2f);
        Meteorain(id + 0x10300, 3.1f);
        Cast(id + 0x10400, AID.AllForNaught, 8.6f, 5);
        Targetable(id + 0x10410, false, 0.2f, "Boss disappears (intermission)")
            .DeactivateOnExit<DeepFreezeFreeze>();
    }

    void Meteorain(uint id, float delay)
    {
        Cast(id, AID.Meteorain, delay, 5, "Raidwide")
            .ActivateOnEnter<Meteorain>()
            .DeactivateOnExit<Meteorain>();
    }

    void Almagest(uint id, float delay)
    {
        Cast(id, AID.Almagest, delay, 5, "Raidwide")
            .ActivateOnEnter<Almagest>()
            .DeactivateOnExit<Almagest>();
    }

    void NaughtGrows(uint id, float delay)
    {
        CastStartMulti(id, [AID.NaughtGrowsCast1, AID.NaughtGrowsCast2], delay)
            .ActivateOnEnter<NaughtGrowsCounter>()
            .ActivateOnEnter<NaughtGrowsDonut>()
            .ActivateOnEnter<NaughtGrowsCircle>()
            .ActivateOnEnter<NaughtGrowsCircleSmall>()
            .ActivateOnEnter<NaughtGrowsDonutSmall>()
            .ActivateOnEnter<ReturnToNothing>();

        ComponentCondition<NaughtGrowsCounter>(id + 1, 8, n => n.NumCasts > 0, "In/out")
            .DeactivateOnExit<NaughtGrowsCounter>()
            .DeactivateOnExit<NaughtGrowsDonut>()
            .DeactivateOnExit<NaughtGrowsCircle>()
            .DeactivateOnExit<NaughtGrowsCircleSmall>()
            .DeactivateOnExit<NaughtGrowsDonutSmall>();

        ComponentCondition<ReturnToNothing>(id + 2, 0.8f, r => r.NumCasts > 0, "Wild charge")
            .DeactivateOnExit<ReturnToNothing>();
    }

    void Meltdown(uint id, float delay)
    {
        Cast(id, AID.MeltdownCast, delay, 4)
            .ActivateOnEnter<ChainsOfCondemnation>()
            .ActivateOnEnter<MeltdownBaited>()
            .ActivateOnEnter<MeltdownSpread>();

        ComponentCondition<ChainsOfCondemnation>(id + 0x10, 1, c => c.Active, "Stop moving");
        ComponentCondition<MeltdownBaited>(id + 0x11, 4.5f, m => m.NumCasts > 0, "Puddles")
            .DeactivateOnExit<MeltdownBaited>();
        ComponentCondition<MeltdownSpread>(id + 0x12, 1, m => m.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<MeltdownSpread>()
            .DeactivateOnExit<ChainsOfCondemnation>();
    }

    void Emptiness(uint id, float delay)
    {
        CastMulti(id, [AID.AiryEmptinessCast, AID.DenseEmptiness], delay, 4)
            .ActivateOnEnter<Emptiness>();

        ComponentCondition<Emptiness>(id + 2, 1, e => e.NumCasts > 0, "Stacks")
            .DeactivateOnExit<Emptiness>();
    }

    // slow orbs move ~1.3 units per second and start from arena edge (20 units away), so ~15.4s from spawn to absorb by boss
    // not sure how delayed the explosion is and i dont feel like checking
    void GazeOfTheVoid(uint id, float delay)
    {
        CastStart(id, AID.GazeOfTheVoidCast, delay)
            .ActivateOnEnter<GazeOfTheVoid>()
            .ActivateOnEnter<Burst>();
        ComponentCondition<GazeOfTheVoid>(id + 1, 7, v => v.NumCasts > 0, "Cones start");
        ComponentCondition<GazeOfTheVoid>(id + 2, 3.6f, v => v.NumCasts == 10, "Cones finish")
            .DeactivateOnExit<GazeOfTheVoid>();

        Timeout(id + 0x10, 18, "Orb deadline")
            .DeactivateOnExit<Burst>();
    }

    void Vacuum(uint id, float delay)
    {
        Cast(id, AID.VacuumCast, delay, 2)
            .ActivateOnEnter<SilentTorrentSmall>()
            .ActivateOnEnter<SilentTorrentMedium>()
            .ActivateOnEnter<SilentTorrentLarge>()
            .ActivateOnEnter<Vacuum>();

        ComponentCondition<SilentTorrentMedium>(id + 0x10, 6.1f, s => s.NumCasts > 0, "Lines")
            .DeactivateOnExit<SilentTorrentSmall>()
            .DeactivateOnExit<SilentTorrentMedium>()
            .DeactivateOnExit<SilentTorrentLarge>();
        ComponentCondition<Vacuum>(id + 0x20, 2.1f, v => v.NumCasts > 0, "Puddles")
            .DeactivateOnExit<Vacuum>();
    }

    void Vacuum2(uint id, float delay)
    {
        Cast(id, AID.VacuumCast, delay, 2)
            .ActivateOnEnter<SilentTorrentSmall>()
            .ActivateOnEnter<SilentTorrentMedium>()
            .ActivateOnEnter<SilentTorrentLarge>()
            .ActivateOnEnter<Vacuum>();

        CastStart(id + 0x10, AID.MeltdownCast, 2.1f)
            .ActivateOnEnter<MeltdownBaited>()
            .ActivateOnEnter<MeltdownSpread>()
            .ActivateOnEnter<ChainsOfCondemnation>();

        ComponentCondition<SilentTorrentMedium>(id + 0x100, 4, s => s.NumCasts > 0, "Lines")
            .DeactivateOnExit<SilentTorrentSmall>()
            .DeactivateOnExit<SilentTorrentMedium>()
            .DeactivateOnExit<SilentTorrentLarge>();

        ComponentCondition<ChainsOfCondemnation>(id + 0x110, 1, c => c.Active, "Stop moving");

        ComponentCondition<Vacuum>(id + 0x200, 1.1f, v => v.NumCasts > 0, "Void puddles")
            .DeactivateOnExit<Vacuum>();

        ComponentCondition<MeltdownBaited>(id + 0x210, 3.4f, m => m.NumCasts > 0, "Baited puddles")
            .DeactivateOnExit<MeltdownBaited>();
        ComponentCondition<MeltdownSpread>(id + 0x220, 1, m => m.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<MeltdownSpread>()
            .DeactivateOnExit<ChainsOfCondemnation>();
    }

    void DeepFreeze(uint id, float delay)
    {
        Cast(id, AID.DeepFreezeRaidwide, delay, 5, "Raidwide + move")
            .ActivateOnEnter<DeepFreezeRaidwide>()
            .ActivateOnEnter<DeepFreezeFreeze>()
            .ActivateOnEnter<DeepFreezeFlare>()
            .DeactivateOnExit<DeepFreezeRaidwide>()
            .DeactivateOnExit<DeepFreezeFlare>();
    }

    // apeiron cast happens at +278.9s or so, if gauge reaches 100 (110.4s after first director update)
    void Intermission(uint id)
    {
        ActorCastStart(id, _module.LoomingShadow, AID.LoomingEmptinessVisual, 14.2f)
            .ActivateOnEnter<LoomingEmptiness>()
            .ActivateOnEnter<LoomingEmptinessKB>()
            .ActivateOnEnter<LoomingShadow>()
            .ActivateOnEnter<ArenaSwitcher>()
            .ActivateOnEnter<DemonEye>()
            .ActivateOnEnter<WeightOfNothing>()
            .ActivateOnEnter<Beacon>()
            .ActivateOnEnter<Nothingness>();

        ActorTargetable(id + 0x10, _module.LoomingShadow, true, 6.1f, "Shadow appears")
            .DeactivateOnExit<LoomingEmptiness>()
            .DeactivateOnExit<LoomingEmptinessKB>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);

        ActorCastStart(id + 0x100, _module.LoomingShadow, AID.VoidalTurbulenceCast, 2.2f)
            .ActivateOnEnter<EmptyShadow>()
            .ActivateOnEnter<VoidalTurbulence>()
            .ActivateOnEnter<Shadows>()
            .ActivateOnEnter<Gauntlet>();

        ComponentCondition<VoidalTurbulence>(id + 0x110, 7.3f, v => v.NumCasts > 0, "Baits/towers 1")
            .DeactivateOnExit<EmptyShadow>()
            .DeactivateOnExit<VoidalTurbulence>();

        ComponentCondition<Shadows>(id + 0x120, 1.2f, s => s.ActiveActors.Any(), "Adds 1 appear");

        ActorCastStart(id + 0x200, _module.LoomingShadow, AID.VoidalTurbulenceCast, 16.1f)
            .ActivateOnEnter<EmptyShadow>()
            .ActivateOnEnter<VoidalTurbulence>();

        ComponentCondition<VoidalTurbulence>(id + 0x210, 7.3f, v => v.NumCasts > 0, "Baits/towers 2")
            .DeactivateOnExit<EmptyShadow>()
            .DeactivateOnExit<VoidalTurbulence>();

        ComponentCondition<Shadows>(id + 0x220, 1.2f, s => s.ActiveActors.Any(), "Adds 2 appear");

        Timeout(id + 0x300, 69, "Intermission enrage");
    }

    void P2(uint id)
    {
        Targetable(id, true, 8.3f, "Boss reappears");
        LightlessWorld(id + 0x10, 0.1f);
        Almagest(id + 0x100, 6.6f);
        NaughtGrows(id + 0x200, 5.8f);

        Cast(id + 0x10000, AID.NaughtWakes, 5.2f, 2)
            .ActivateOnEnter<PassageOfNaught>();
        ComponentCondition<PassageOfNaught>(id + 0x10010, 8, p => p.NumCasts > 0, "Lasers 1");
        ComponentCondition<PassageOfNaught>(id + 0x10020, 11.3f, p => p.NumCasts > 4, "Lasers 2")
            .DeactivateOnExit<PassageOfNaught>();

        Cast(id + 0x10100, AID.ShroudedHolyCast, 0.1f, 5)
            .ActivateOnEnter<ShroudedHoly>();
        ComponentCondition<ShroudedHoly>(id + 0x10110, 1, s => s.NumFinishedStacks > 0, "Stacks")
            .DeactivateOnExit<ShroudedHoly>();

        NaughtGrows(id + 0x20000, 7.6f);
        DimensionZero(id + 0x21000, 5.4f, 3);
        Vacuum2(id + 0x22000, 7.7f);

        GazeOfTheVoid(id + 0x30000, 6.8f);
        Almagest(id + 0x31000, 5.6f);

        Cast(id + 0x40000, AID.NaughtWakes, 8.4f, 2);
        NaughtHunts(id + 0x40100, 2.2f);
        Emptiness(id + 0x41000, 1);

        NaughtHunts(id + 0x50000, 5.3f);
        Emptiness(id + 0x51000, 1);
        DimensionZero(id + 0x52000, 4.1f, 4);
        Almagest(id + 0x53000, 5.5f);

        NaughtGrows(id + 0x60000, 5.8f);
        Cast(id + 0x61000, AID.NaughtWakes, 5.5f, 2)
            .ActivateOnEnter<PassageOfNaught>();
        ComponentCondition<PassageOfNaught>(id + 0x61010, 8, p => p.NumCasts > 0, "Lasers 1");
        ComponentCondition<PassageOfNaught>(id + 0x61020, 11.3f, p => p.NumCasts > 4, "Lasers 2");

        DeepFreeze(id + 0x70000, 0);
        NaughtGrows(id + 0x71000, 7.5f);
        DimensionZero(id + 0x72000, 5.3f, 4);
        Meteorain(id + 0x73000, 5.6f);
        Meteorain(id + 0x74000, 5.1f);

        Cast(id + 0x80000, AID.AlmagestEnrage, 8.6f, 9, "Enrage");
    }

    void LightlessWorld(uint id, float delay)
    {
        Cast(id, AID.LightlessWorldCast, delay, 10)
            .ActivateOnEnter<LightlessWorld>();
        ComponentCondition<LightlessWorld>(id + 0x2, 1, l => l.NumCasts > 0, "Raidwide 1");
        ComponentCondition<LightlessWorld>(id + 0x3, 2.5f, l => l.NumCasts == 6, "Raidwide 6")
            .DeactivateOnExit<LightlessWorld>();
    }

    void DimensionZero(uint id, float delay, int count)
    {
        CastStart(id, AID.DimensionZeroCast, delay)
            .ActivateOnEnter<DimensionZero>()
            .ExecOnEnter<DimensionZero>(z => z.NumExpected = count);
        ComponentCondition<DimensionZero>(id + 1, 5.4f, z => z.NumCasts > 0, "Line stack 1");
        ComponentCondition<DimensionZero>(id + 2, count == 3 ? 2.7f : 4.1f, z => z.NumCasts == count, $"Line stack {count}")
            .DeactivateOnExit<DimensionZero>();
    }

    void NaughtHunts(uint id, float delay)
    {
        CastStart(id, AID.NaughtHunts, delay)
            .ActivateOnEnter<PassageOfNaught>()
            .ActivateOnEnter<EndlessChase>();

        ComponentCondition<PassageOfNaught>(id + 0x10, 7, p => p.NumCasts > 0, "Safe diamond + chasers start")
            .DeactivateOnExit<PassageOfNaught>();
        ComponentCondition<EndlessChase>(id + 0x20, 8.4f, c => c.NumCasts >= 26, "Change targets");
        ComponentCondition<EndlessChase>(id + 0x30, 8.3f, c => c.NumCasts >= 50, "Chasers end")
            .ActivateOnEnter<PassageOfNaught>()
            .ExecOnEnter<PassageOfNaught>(p => p.Risky = false)
            .DeactivateOnExit<EndlessChase>();
        ComponentCondition<PassageOfNaught>(id + 0x40, 4.6f, p => p.NumCasts > 0, "Safe corners")
            .ExecOnEnter<PassageOfNaught>(p => p.Risky = true)
            .DeactivateOnExit<PassageOfNaught>();
    }
}
