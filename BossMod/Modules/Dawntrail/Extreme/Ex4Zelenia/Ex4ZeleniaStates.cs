namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

class ZeleniaStates : StateMachineBuilder
{
    public ZeleniaStates(BossModule module) : base(module)
    {
        SimplePhase(0, Phase1, "P1")
            .ActivateOnEnter<ThornedCatharsis>()
            .Raw.Update = () => !Module.PrimaryActor.IsTargetable;

        SimplePhase(1, AddsPhase, "Adds")
            .SetHint(StateMachine.PhaseHint.StartWithDowntime)
            .Raw.Update = () =>
            {
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
        Raidwide(id, 6.1f);

        CastStart(id + 2, AID._Weaponskill_Shock, 7.6f)
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

        CastStart(id + 0x20, AID._Weaponskill_SpecterOfTheLost, 0.9f);

        ComponentCondition<SpecterOfTheLostAOE>(id + 0x22, 7.7f, e => e.NumCasts >= 2, "Tankbusters")
            .DeactivateOnExit<SpecterOfTheLostAOE>();

        EscelonsFall(id + 0x10000, 6.7f, "EF1");
        StockBreak(id + 0x20000, 2.2f);

        CastStart(id + 0x30000, AID._Weaponskill_BlessedBarricade, 3);

        Targetable(id + 0x30002, false, 3.8f, "Boss disappears");
    }

    private void AddsPhase(uint id)
    {
        ComponentCondition<RosebloodDrop>(id, 0.29f, d => d.ActiveActors.Any(), "Adds spawn")
            .ActivateOnEnter<RosebloodDrop>()
            .ActivateOnEnter<P2Explosion>()
            .ActivateOnEnter<SpearpointAOE>()
            .ActivateOnEnter<SpearpointBait>();

        ComponentCondition<P2Explosion>(id + 0x10, 10.8f, e => e.NumCasts >= 2, "Towers 1");
        ComponentCondition<P2Explosion>(id + 0x12, 12, e => e.NumCasts >= 4, "Towers 2");
        ComponentCondition<P2Explosion>(id + 0x14, 12, e => e.NumCasts >= 6, "Towers 3");
        ComponentCondition<SpearpointAOE>(id + 0x16, 11.9f, e => e.NumCasts >= 8, "Baits 4");

        Timeout(id + 0x100, 9999, "Enrage");
    }

    private void Phase2(uint id)
    {
        Targetable(id, true, 5, "Boss appears")
            .ActivateOnEnter<PerfumedQuietus>()
            .ActivateOnEnter<Voidzone>()
            .ActivateOnEnter<AlexandrianThunderIV>()
            .ActivateOnEnter<ThunderSlash>();
        CastStart(id + 0x10, AID._Weaponskill_PerfumedQuietus, 0.1f);
        ComponentCondition<PerfumedQuietus>(id + 0x12, 9.2f, p => p.NumCasts > 0, "Raidwide");

        Cast(id + 0x20, AID._Weaponskill_RosebloodBloom, 12.3f, 2.6f, "Bloom 1 start")
            .ActivateOnEnter<AlexandrianThunderII>()
            .ActivateOnEnter<AlexandrianThunderIII>()
            .ExecOnEnter<Tiles>(t => t.ShouldDraw = true);

        CastStart(id + 0x30, AID._Weaponskill_AlexandrianThunderII, 3.5f);
        ComponentCondition<AlexandrianThunderII>(id + 0x32, 5.7f, e => e.NumCasts > 0, "Rotating AOEs start");
        ComponentCondition<AlexandrianThunderII>(id + 0x34, 14.2f, e => e.NumCasts >= 90, "Rotating AOEs end");
        ComponentCondition<AlexandrianThunderIII>(id + 0x36, 5.7f, e => e.NumFinishedSpreads > 0, "Spreads");

        Raidwide(id + 0x100, 2.4f);

        id += 0x10000;

        Cast(id, AID._Weaponskill_Roseblood2NdBloom, 7.4f, 2.6f, "Bloom 2 start");

        ComponentCondition<AlexandrianThunderIV>(id + 0x10, 10.3f, t => t.NumCasts > 0, "In/out");
        ComponentCondition<AlexandrianThunderIV>(id + 0x11, 3, t => t.NumCasts > 1, "Out/in").DeactivateOnExit<AlexandrianThunderIV>();
        ComponentCondition<ThunderSlash>(id + 0x12, 2, t => t.NumCasts >= 6, "Slashes end")
            .DeactivateOnExit<ThunderSlash>();

        CastStart(id + 0x20, AID._Weaponskill_SpecterOfTheLost, 2.4f)
            .ActivateOnEnter<SpecterOfTheLost>()
            .ActivateOnEnter<SpecterOfTheLostAOE>();

        ComponentCondition<SpecterOfTheLostAOE>(id + 0x22, 7.7f, e => e.NumCasts >= 2, "Tankbusters")
            .DeactivateOnExit<SpecterOfTheLost>()
            .DeactivateOnExit<SpecterOfTheLostAOE>()
            .ExecOnExit<Tiles>(t => t.ShouldDraw = false);

        id += 0x10000;

        Cast(id, AID._Weaponskill_Roseblood3RdBloom, 6.8f, 2.6f, "Bloom 3 start")
            .ActivateOnEnter<Emblazon>()
            .ActivateOnEnter<Explosion2>();

        Timeout(id + 0x100000, 9999, "Enrage");
    }

    private void Raidwide(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_ThornedCatharsis, delay, 5, "Raidwide");
    }

    private void EscelonsFall(uint id, float delay, string name)
    {
        CastStart(id, AID._Weaponskill_EscelonsFall, delay, $"{name} start")
            .ActivateOnEnter<EscelonsFall>();

        ComponentCondition<EscelonsFall>(id + 2, 13.9f, e => e.NumCasts >= 4, "Baits 1");
        ComponentCondition<EscelonsFall>(id + 4, 3.1f, e => e.NumCasts >= 8, "Baits 2");
        ComponentCondition<EscelonsFall>(id + 6, 3.1f, e => e.NumCasts >= 12, "Baits 3");
        ComponentCondition<EscelonsFall>(id + 8, 3.1f, e => e.NumCasts >= 16, "Baits 4")
            .DeactivateOnExit<EscelonsFall>();
    }

    private void StockBreak(uint id, float delay)
    {
        CastStart(id, AID._Weaponskill_StockBreak, delay)
            .ActivateOnEnter<StockBreak>();

        ComponentCondition<StockBreak>(id + 0x10, 9.2f, s => s.NumCasts > 0, "Stack start");
        ComponentCondition<StockBreak>(id + 0x20, 2.2f, s => s.NumCasts >= 3, "Stack end")
            .DeactivateOnExit<StockBreak>();
    }
}
