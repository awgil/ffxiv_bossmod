namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

[SkipLocalsInit]
sealed class PariOfPlentyStates : StateMachineBuilder
{
    public PariOfPlentyStates(BossModule module) : base(module)
    {
        DeathPhase(default, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        FireFlight(id, 8.1f);
        WheelOfFableFlight(id + 0x100, 12.7f);
        FireFlightFourLongNight(id + 0x200, 7.0f);
        ParisCurse(id + 0x300, 9.3f);
        SpurningFlames(id + 0x400, 8.4f);
        Doubling(id + 0x500, 7.2f);
        SimpleState(id + 0xFF0000u, 10000f, "???");
    }

    private void FireFlight(uint id, float delay)
    {
        Cast(id, (uint)AID.HeatBurst, delay, 5, "Raidwide")
            .ActivateOnEnter<HeatBurst>()
            .DeactivateOnExit<HeatBurst>();

        CastMulti(id + 0x10,
            [
                (uint)AID.FireflightByPyrelightLeft, (uint)AID.FireflightByPyrelightRight,
                (uint)AID.FireflightByEmberlightLeft, (uint)AID.FireflightByEmberlightRight
            ], 10.1f, 10.0f, "Fireflight")
            .ActivateOnEnter<Fireflight>()
            .ActivateOnEnter<FireflightStackSpread>();
        ComponentCondition<Fireflight>(id + 0x20, 2.6f, o => o.NumCasts > 0, "Cleave 1");
        ComponentCondition<Fireflight>(id + 0x30, 2.1f, o => o.NumCasts > 1, "Cleave 2");
        ComponentCondition<Fireflight>(id + 0x40, 2.0f, o => o.NumCasts > 2, "Cleave 3")
            .ActivateOnEnter<SunCirclet>()
            .DeactivateOnExit<Fireflight>();
        Cast(id + 0x50, (uint)AID.SunCirclet, 1.7f, 2.0f, "SunCirclet")
            .DeactivateOnExit<SunCirclet>();
        ComponentCondition<FireflightStackSpread>(id + 0x60, 1.1f, o => !o.Active, "Spread/Stack")
            .DeactivateOnExit<FireflightStackSpread>()
            .ActivateOnEnter<WheelOfFableFlight>(); // Easier to just activate this here rather than adding a condition for when the four clones move in-game
    }

    private void WheelOfFableFlight(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.WheelOfFableflightLeft, (uint)AID.WheelOfFableflightRight], delay, 11.0f, "WheelOfFableFlight")
            .ActivateOnEnter<WheelofFableFlightStackSpread>();
        ComponentCondition<WheelOfFableFlight>(id + 0x10, 0.3f, o => o.NumCasts > 0, "Cleaves");
        ComponentCondition<WheelofFableFlightStackSpread>(id + 0x20, 0.5f, o => !o.Active, "Spread/Stack", checkDelay: 0.5f)
            .DeactivateOnExit<WheelOfFableFlight>()
            .DeactivateOnExit<WheelofFableFlightStackSpread>();

        Cast(id + 0x30, (uint)AID.FireOfVictory, 4.4f, 5.0f, "Tankbuster")
            .ActivateOnEnter<FireOfVictory>()
            .DeactivateOnExit<FireOfVictory>();
    }

    private void FireFlightFourLongNight(uint id, float delay)
    {
        CastMulti(id, [(uint)AID.FireflightFourLongNightsLeft, (uint)AID.FireflightFourLongNightsRight], delay, 17.0f,
                "FireFlightFourLongNight")
            .ActivateOnEnter<FireFlightFourLongNight>()
            .ActivateOnEnter<WitchHunt>()
            .ActivateOnEnter<WitchHuntStack>();
        ComponentCondition<FireFlightFourLongNight>(id + 0x10, 2.1f, o => o.NumCasts > 0, "Bait 1");
        ComponentCondition<FireFlightFourLongNight>(id + 0x20, 3.0f, o => o.NumCasts > 1, "Bait 2");
        ComponentCondition<FireFlightFourLongNight>(id + 0x30, 3.0f, o => o.NumCasts > 2, "Bait 3");
        ComponentCondition<FireFlightFourLongNight>(id + 0x40, 3.0f, o => o.NumCasts > 3, "Bait 4")
            .DeactivateOnExit<FireFlightFourLongNight>()
            .DeactivateOnExit<WitchHunt>()
            .DeactivateOnExit<WitchHuntStack>();
    }

    private void ParisCurse(uint id, float delay)
    {
        Cast(id, (uint)AID.ParisCurse, delay, 5, "Pari's Curse")
            .ActivateOnEnter<ParisCurse>()
            .ActivateOnEnter<Fableflight>();
        ComponentCondition<ParisCurse>(id + 0x10, 32.0f, o => o.NumCasts > 0, "Spread/Stack Resolve")
            .DeactivateOnExit<ParisCurse>();
    }

    private void SpurningFlames(uint id, float delay)
    {
        Cast(id, (uint)AID.SpurningFlames, delay, 7, "Spurning Flames")
            .ActivateOnEnter<SpurningFlames>()
            .DeactivateOnExit<SpurningFlames>()
            .ActivateOnExit<ImpassionedSparks>()
            .ActivateOnExit<BurningPillar>();
        ComponentCondition<BurningPillar>(id + 0x10, 20.0f, o => o.NumCasts > 0, "Baits 1");
        ComponentCondition<BurningPillar>(id + 0x20, 5.0f, o => o.NumCasts > 2, "Baits 2");
        ComponentCondition<BurningPillar>(id + 0x30, 5.0f, o => o.NumCasts > 4, "Baits 3");
        ComponentCondition<BurningPillar>(id + 0x40, 5.0f, o => o.NumCasts > 6, "Baits 4")
            .ActivateOnEnter<BurningPillarSpreads>()
            .ActivateOnEnter<FireChains>();
        ComponentCondition<FireChains>(id + 0x50, 3.7f, o => o.TethersAssigned, "Chains");
        Cast(id + 0x60, (uint)AID.ScouringScorn, 5.6f, 6.0f, "Raidwide")
            .ActivateOnEnter<ScouringScorn>()
            .DeactivateOnExit<ScouringScorn>()
            .DeactivateOnExit<ImpassionedSparks>()
            .DeactivateOnExit<BurningPillar>()
            .DeactivateOnExit<FireChains>();
    }

    private void Doubling(uint id, float delay)
    {
        Cast(id, (uint)AID.Doubling, delay, 3, "Doubling")
            .ActivateOnEnter<Doubling>();
        ComponentCondition<Doubling>(id + 0x10, 14.2f, o => o.NumCasts > 0, "1st Towers");
        ComponentCondition<Doubling>(id + 0x20, 7.0f, o => o.NumCasts > 4, "2nd Towers")
            .DeactivateOnExit<Doubling>()
            .ActivateOnEnter<RedCrystals>()
            .ActivateOnEnter<KindleFlameStackIcon>();
        ComponentCondition<RedCrystals>(id + 0x30, 11.7f, o => o.NumCasts > 0, "Stacks + Crystals resolve")
            .DeactivateOnExit<RedCrystals>()
            .DeactivateOnExit<KindleFlameStackIcon>();
        CastMulti(id + 0x40, [(uint)AID.CharmedFlightFourNightsLeft, (uint)AID.CharmedFlightFourNightsRight], 5.0f, 17.5f, "CharmedFlightFourNights")
            .ActivateOnEnter<CharmedFlightFourNights>()
            .ActivateOnEnter<RedCrystals2>();
    }
}