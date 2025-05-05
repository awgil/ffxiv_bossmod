namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class ZeleniaStates : StateMachineBuilder
{
    public ZeleniaStates(BossModule module) : base(module)
    {
        SimplePhase(0, Phase1, "P1")
            .ActivateOnEnter<ThornedCatharsis>()
            .Raw.Update = () => !Module.PrimaryActor.IsTargetable || Module.PrimaryActor.IsDeadOrDestroyed;

        SimplePhase(1, AddsPhase, "Adds")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () =>
            {
                var enrage = Module.FindComponent<AddsEnrage>()!;
                if (enrage.Active)
                    return false;

                var adds = Module.FindComponent<RosebloodDrop>()!;
                if (adds.Spawned && !adds.ActiveActors.Any())
                {
                    // she likes to stick around until the adds are done
                    return Module.FindComponent<SpearpointBait>()?.ActiveBaits.Count() == 0 && Module.FindComponent<SpearpointAOE>()?.Casters.Count == 0;
                }
                return false;
            };

        DeathPhase(2, Phase2)
            .ActivateOnEnter<Tiles>()
            .SetHint(StateMachine.PhaseHint.StartWithDowntime);
    }

    private void Phase1(uint id)
    {
        ThornedCatharsis(id, 6.1f);

        CastStart(id + 2, AID.ShockVisual, 7.6f)
            .ActivateOnEnter<P1Explosion>()
            .ActivateOnEnter<ShockDonutBait>()
            .ActivateOnEnter<ShockCircleBait>()
            .ActivateOnEnter<ShockAOEs>()
            .ActivateOnEnter<SpecterOfTheLost>()
            .ActivateOnEnter<SpecterOfTheLostAOE>();

        ComponentCondition<ShockAOEs>(id + 0x10, 11.8f, e => e.NumCasts > 0, "AOEs 1");
        ComponentCondition<P1Explosion>(id + 0x12, 1.3f, e => e.NumCasts > 0, "Towers");
        ComponentCondition<ShockAOEs>(id + 0x14, 4.3f, e => e.NumCasts >= 96, "AOEs 11")
            .DeactivateOnExit<P1Explosion>()
            .DeactivateOnExit<ShockDonutBait>()
            .DeactivateOnExit<ShockCircleBait>()
            .DeactivateOnExit<ShockAOEs>();

        CastStart(id + 0x20, AID.SpecterOfTheLostVisual, 0.9f);

        ComponentCondition<SpecterOfTheLostAOE>(id + 0x22, 7.7f, e => e.NumCasts >= 2, "Tankbusters")
            .DeactivateOnExit<SpecterOfTheLostAOE>();

        EscalonsFall1(id + 0x10000, 6.7f);
        StockBreak(id + 0x20000, 2.2f);

        CastStart(id + 0x20100, AID.BlessedBarricade, 3);

        Targetable(id + 0x20102, false, 3.8f, "Boss disappears");
    }

    private void AddsPhase(uint id)
    {
        ComponentCondition<RosebloodDrop>(id, 0.29f, d => d.ActiveActors.Any(), "Adds spawn")
            .ActivateOnEnter<RosebloodDrop>()
            .ActivateOnEnter<P2Explosion>()
            .ActivateOnEnter<SpearpointAOE>()
            .ActivateOnEnter<SpearpointBait>()
            .ActivateOnEnter<AddsEnrage>();

        ComponentCondition<P2Explosion>(id + 0x10, 10.8f, e => e.NumCasts >= 2, "Towers 1");
        ComponentCondition<P2Explosion>(id + 0x12, 12, e => e.NumCasts >= 4, "Towers 2");
        ComponentCondition<P2Explosion>(id + 0x14, 12, e => e.NumCasts >= 6, "Towers 3");
        ComponentCondition<SpearpointAOE>(id + 0x16, 11.9f, e => e.NumCasts >= 8, "Baits 4");

        ComponentCondition<AddsEnrage>(id + 0x20, 8.4f, e => e.Active, "Adds disappear")
            .SetHint(StateMachine.StateHint.DowntimeStart);

        CastStart(id + 0xFF0000, AID.PerfumedQuietusEnrageVisual, 2.2f);
        Timeout(id + 0xFF0001, 9.2f, "Enrage");
    }

    private void Phase2(uint id)
    {
        Targetable(id, true, 5, "Boss appears")
            .ActivateOnEnter<PerfumedQuietus>()
            .ActivateOnEnter<Voidzone>()
            .ActivateOnEnter<AlexandrianThunderIV>()
            .ActivateOnEnter<ThunderSlash>();
        CastStart(id + 0x10, AID.PerfumedQuietusVisual, 0.1f);
        ComponentCondition<PerfumedQuietus>(id + 0x12, 9.2f, p => p.NumCasts > 0, "Raidwide");

        Bloom1(id + 0x10000, 12.3f);
        Bloom2(id + 0x20000, 7.4f);
        Bloom3(id + 0x30000, 6.8f);
        EscalonsFall2(id + 0x40000, 8.4f);
        Bloom4(id + 0x50000, 5);

        Timeout(id + 0x100000, 9999, "Enrage");
    }

    private void ThornedCatharsis(uint id, float delay)
    {
        Cast(id, AID.ThornedCatharsis, delay, 5, "Raidwide");
    }

    private void EscalonsFall1(uint id, float delay)
    {
        CastStart(id, AID.EscelonsFallVisual, delay, "EF1 start")
            .ActivateOnEnter<EscelonsFall>();

        ComponentCondition<EscelonsFall>(id + 2, 13.9f, e => e.NumCasts >= 4, "Baits 1");
        ComponentCondition<EscelonsFall>(id + 4, 3.1f, e => e.NumCasts >= 8, "Baits 2");
        ComponentCondition<EscelonsFall>(id + 6, 3.1f, e => e.NumCasts >= 12, "Baits 3");
        ComponentCondition<EscelonsFall>(id + 8, 3.1f, e => e.NumCasts >= 16, "Baits 4")
            .DeactivateOnExit<EscelonsFall>();
    }

    private void StockBreak(uint id, float delay)
    {
        CastStart(id, AID.StockBreak, delay)
            .ActivateOnEnter<StockBreak>();

        ComponentCondition<StockBreak>(id + 0x10, 9.2f, s => s.NumCasts > 0, "Stack start");
        ComponentCondition<StockBreak>(id + 0x20, 2.2f, s => s.NumCasts >= 3, "Stack end")
            .DeactivateOnExit<StockBreak>();
    }

    private void Bloom1(uint id, float delay)
    {
        Cast(id, AID.RosebloodBloom, delay, 2.6f, "Bloom 1 start")
            .ActivateOnEnter<AlexandrianThunderII>()
            .ActivateOnEnter<Bloom1AlexandrianThunderIII>()
            .ExecOnEnter<Tiles>(t => t.ShouldDraw = true);

        CastStart(id + 0x10, AID.AlexandrianThunderIIVisual, 3.5f);
        ComponentCondition<AlexandrianThunderII>(id + 0x11, 5.7f, e => e.NumCasts > 0, "Rotating AOEs start");
        ComponentCondition<AlexandrianThunderII>(id + 0x12, 14.2f, e => e.NumCasts >= 90, "Rotating AOEs end")
            .DeactivateOnExit<AlexandrianThunderII>();
        ComponentCondition<Bloom1AlexandrianThunderIII>(id + 0x13, 5.7f, e => e.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<Bloom1AlexandrianThunderIII>();

        ThornedCatharsis(id + 0x20, 2.4f);
    }

    private void Bloom2(uint id, float delay)
    {
        Cast(id, AID.Roseblood2ndBloom, delay, 2.6f, "Bloom 2 start")
            .ActivateOnEnter<ThunderHints>();

        ComponentCondition<AlexandrianThunderIV>(id + 0x10, 10.3f, t => t.NumCasts > 0, "In/out");
        ComponentCondition<AlexandrianThunderIV>(id + 0x11, 3, t => t.NumCasts > 1, "Out/in").DeactivateOnExit<AlexandrianThunderIV>();
        ComponentCondition<ThunderSlash>(id + 0x12, 2, t => t.NumCasts >= 6, "Slashes end")
            .DeactivateOnExit<ThunderSlash>()
            .DeactivateOnExit<ThunderHints>()
            .ExecOnExit<Tiles>(t => t.ShouldDraw = false);

        CastStart(id + 0x20, AID.SpecterOfTheLostVisual, 2.4f)
            .ActivateOnEnter<SpecterOfTheLost>()
            .ActivateOnEnter<SpecterOfTheLostAOE>();

        ComponentCondition<SpecterOfTheLostAOE>(id + 0x22, 7.7f, e => e.NumCasts >= 2, "Tankbusters")
            .DeactivateOnExit<SpecterOfTheLost>()
            .DeactivateOnExit<SpecterOfTheLostAOE>();
    }

    private void Bloom3(uint id, float delay)
    {
        Cast(id, AID.Roseblood3rdBloom, delay, 2.6f, "Bloom 3 start")
            .ActivateOnEnter<Bloom3Emblazon>()
            .ActivateOnEnter<Bloom3Explosion>();

        Cast(id + 0x10, AID.BudOfValor, 3.6f, 3);

        Cast(id + 0x20, AID.EmblazonVisual, 5.2f, 3)
            .ActivateOnEnter<EmblazonCounter>();

        ComponentCondition<Bloom3Emblazon>(id + 0x22, 0.8f, b => b.Baiters.Any(), "Roses appear");
        ComponentCondition<EmblazonCounter>(id + 0x30, 6.8f, b => b.NumCasts > 0, "Baits")
            .DeactivateOnExit<EmblazonCounter>();
        ComponentCondition<Bloom3Explosion>(id + 0x40, 4.4f, b => b.NumCasts > 0, "Towers")
            .DeactivateOnExit<Bloom3Emblazon>()
            .DeactivateOnExit<Bloom3Explosion>();

        ThornedCatharsis(id + 0x50, 3.3f);
    }

    private void EscalonsFall2(uint id, float delay)
    {
        Cast(id, AID.BudOfValor, delay, 3)
            .ActivateOnEnter<ShockDonutBait>()
            .ActivateOnEnter<ShockAOEs>()
            .ActivateOnEnter<AlexandrianBanishII>();

        CastStart(id + 0x10, AID.EscelonsFallVisual, 9.2f, "EF2 start")
            .ActivateOnEnter<EscelonsFall>()
            // wait for donuts to finish
            .ExecOnEnter<EscelonsFall>(e => e.EnableHints = false);

        ComponentCondition<ShockDonutBait>(id + 0x12, 6.7f, s => s.NumCasts > 0, "Donuts start");

        ComponentCondition<AlexandrianBanishII>(id + 0x14, 1.1f, b => b.NumFinishedStacks > 0, "Stacks");

        ComponentCondition<ShockAOEs>(id + 0x20, 4.5f, s => s.NumCasts >= 36, "Donuts end");

        ComponentCondition<EscelonsFall>(id + 0x22, 1.6f, e => e.NumCasts >= 4, "Baits 1");
        ComponentCondition<EscelonsFall>(id + 0x24, 3.1f, e => e.NumCasts >= 8, "Baits 2");
        ComponentCondition<EscelonsFall>(id + 0x26, 3.1f, e => e.NumCasts >= 12, "Baits 3");
        ComponentCondition<EscelonsFall>(id + 0x28, 3.1f, e => e.NumCasts >= 16, "Baits 4")
            .DeactivateOnExit<EscelonsFall>();

        StockBreak(id + 0x30, 2.2f);
    }

    private void Bloom4(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_Roseblood4ThBloom, delay, 2.6f, "Bloom 4 start")
            .ActivateOnEnter<Bloom4Emblazon>();
    }
}
