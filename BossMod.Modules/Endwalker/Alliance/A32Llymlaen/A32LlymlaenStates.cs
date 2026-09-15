namespace BossMod.Endwalker.Alliance.A32Llymlaen;

class A32LlymlaenStates : StateMachineBuilder
{
    public A32LlymlaenStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Tempest(id, 7.2f);
        SeafoamSpiral(id + 0x10000, 5.2f);
        WindRose(id + 0x20000, 3.2f);
        NavigatorsTridentNormal(id + 0x30000, 10.6f);
        WindRoseSeafoamSpiral(id + 0x40000, 6.2f);
        SurgingWaveSeaFoam(id + 0x50000, 2.1f);
        DeepDiveNormal(id + 0x60000, 3.2f);
        TorrentialTridents(id + 0x70000, 9.3f, false);
        StormySeas(id + 0x80000, 1.3f);
        DenizensOfTheDeep(id + 0x90000, 9.9f);
        Godsbane(id + 0xA0000, 3.6f);
        NavigatorsTridentAdds(id + 0xB0000, 9.5f);
        WindRoseSeafoamSpiral(id + 0xC0000, 5.1f);
        SurgingWaveToTheLast(id + 0xD0000, 2.1f);
        DeepDiveHardWater(id + 0xE0000, 5.3f);
        TorrentialTridents(id + 0xF0000, 6.2f, true);
        StormySeasMaelstrom(id + 0x100000, 0.3f);
        Godsbane(id + 0x110000, 4.3f);
        WindRoseSeafoamSpiral(id + 0x120000, 11.3f);
        StormySeas(id + 0x130000, 2.9f);
        LeftRightStrait(id + 0x140000, 0.3f);

        SimpleState(id + 0xFF0000, 10, "???");
    }

    private void Tempest(uint id, float delay)
    {
        Cast(id, AID.Tempest, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void SeafoamSpiral(uint id, float delay)
    {
        Cast(id, AID.SeafoamSpiral, delay, 6, "In")
            .ActivateOnEnter<SeafoamSpiral>()
            .DeactivateOnExit<SeafoamSpiral>();
    }

    private void WindRose(uint id, float delay)
    {
        Cast(id, AID.WindRose, delay, 6, "Out")
            .ActivateOnEnter<WindRose>()
            .DeactivateOnExit<WindRose>();
    }

    private State WindRoseSeafoamSpiral(uint id, float delay)
    {
        return CastMulti(id, [AID.WindRose, AID.SeafoamSpiral], delay, 6, "In/out")
            .ActivateOnEnter<WindRose>()
            .ActivateOnEnter<SeafoamSpiral>()
            .DeactivateOnExit<WindRose>()
            .DeactivateOnExit<SeafoamSpiral>();
    }

    private void LeftRightStrait(uint id, float delay)
    {
        CastMulti(id, [AID.LeftStrait, AID.RightStrait], delay, 6, "Side")
            .ActivateOnEnter<LeftStrait>()
            .ActivateOnEnter<RightStrait>()
            .DeactivateOnExit<LeftStrait>()
            .DeactivateOnExit<RightStrait>();
    }

    private void NavigatorsTridentNormal(uint id, float delay)
    {
        Cast(id, AID.NavigatorsTrident, delay, 6.5f)
            .ActivateOnEnter<DireStraits>();
        ComponentCondition<DireStraits>(id + 0x10, 1.0f, comp => comp.NumCasts > 0, "Side 1");
        ComponentCondition<DireStraits>(id + 0x11, 1.8f, comp => comp.NumCasts > 1, "Side 2")
            .DeactivateOnExit<DireStraits>();

        ComponentCondition<NavigatorsTridentAOE>(id + 0x20, 1.8f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<NavigatorsTridentAOE>();
        ComponentCondition<NavigatorsTridentAOE>(id + 0x21, 7, comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<SurgingWaveCorridor>() // envcontrol will happen right after knockback end
            .ActivateOnEnter<NavigatorsTridentKnockback>()
            .DeactivateOnExit<NavigatorsTridentKnockback>()
            .DeactivateOnExit<NavigatorsTridentAOE>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void SurgingWaveSeaFoam(uint id, float delay)
    {
        Cast(id, AID.SurgingWave, delay, 9)
            .ActivateOnEnter<SurgingWaveAOE>()
            .ActivateOnEnter<SurgingWaveShockwave>()
            .ActivateOnEnter<SurgingWaveSeaFoam>()
            .ExecOnEnter<SurgingWaveCorridor>(comp => Module.Arena.Center = A32Llymlaen.DefaultCenter + A32Llymlaen.CorridorHalfLength * comp.CorridorDir)
            .ExecOnEnter<SurgingWaveCorridor>(comp => Module.Arena.Bounds = comp.CorridorDir.X > 0 ? A32Llymlaen.EastCorridorBounds : A32Llymlaen.WestCorridorBounds);
        ComponentCondition<SurgingWaveAOE>(id + 0x10, 1, comp => comp.NumCasts > 0)
            .DeactivateOnExit<SurgingWaveAOE>();
        ComponentCondition<SurgingWaveShockwave>(id + 0x11, 0.2f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<SurgingWaveShockwave>();
        ComponentCondition<SurgingWaveFrothingSea>(id + 0x20, 6.2f, comp => comp.NumCasts > 0)
            .ActivateOnEnter<SurgingWaveFrothingSea>();
        CastStartMulti(id + 0x30, [AID.LeftStrait, AID.RightStrait], 7.8f);
        ComponentCondition<SurgingWaveFrothingSea>(id + 0x40, 3.9f, comp => comp.NumCasts > 12)
            .ActivateOnEnter<LeftStrait>()
            .ActivateOnEnter<RightStrait>()
            .DeactivateOnExit<SurgingWaveFrothingSea>();
        ComponentCondition<SurgingWaveCorridor>(id + 0x50, 0.5f, comp => comp.CorridorDir == default)
            .DeactivateOnExit<SurgingWaveSeaFoam>()
            .DeactivateOnExit<SurgingWaveCorridor>()
            .OnExit(() => (Module.Arena.Center, Module.Arena.Bounds) = (A32Llymlaen.DefaultCenter, A32Llymlaen.DefaultBounds));
        CastEnd(id + 0x60, 1.6f, "Side")
            .DeactivateOnExit<LeftStrait>()
            .DeactivateOnExit<RightStrait>();
    }

    private void DeepDiveNormal(uint id, float delay)
    {
        Cast(id, AID.DeepDiveNormal, delay, 5, "Stack")
            .ActivateOnEnter<DeepDiveNormal>()
            .DeactivateOnExit<DeepDiveNormal>();
    }

    private void TorrentialTridents(uint id, float delay, bool longDelay)
    {
        Cast(id, AID.TorrentialTridents, delay, 4);
        ComponentCondition<TorrentialTridentLanding>(id + 0x10, 2.8f, comp => comp.NumCasts > 0)
            .ActivateOnEnter<TorrentialTridentLanding>();
        ComponentCondition<TorrentialTridentLanding>(id + 0x20, 5, comp => comp.NumCasts > 5, "Raidwide x6")
            .DeactivateOnExit<TorrentialTridentLanding>();

        ComponentCondition<TorrentialTridentAOE>(id + 0x30, longDelay ? 4.1f : 2.1f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<TorrentialTridentAOE>();
        ComponentCondition<TorrentialTridentAOE>(id + 0x40, 6, comp => comp.NumCasts > 0, "Explosions start");
        ComponentCondition<TorrentialTridentAOE>(id + 0x50, 5, comp => comp.NumCasts > 5, "Explosions end")
            .DeactivateOnExit<TorrentialTridentAOE>();
    }

    private void StormySeas(uint id, float delay)
    {
        ComponentCondition<Stormwhorl>(id, delay, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Stormwhorl>();
        ComponentCondition<Stormwhorl>(id + 0x10, 4, comp => comp.NumCasts > 0, "Puddles")
            .ActivateOnEnter<Stormwinds>();
        ComponentCondition<Stormwhorl>(id + 0x11, 2, comp => comp.NumCasts > 3);
        ComponentCondition<Stormwhorl>(id + 0x12, 2, comp => comp.NumCasts > 6)
            .DeactivateOnExit<Stormwhorl>();
        ComponentCondition<Stormwinds>(id + 0x20, 1, comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<Stormwinds>();
    }

    private void DenizensOfTheDeep(uint id, float delay)
    {
        Cast(id, AID.DenizensOfTheDeep, delay, 4);
        ComponentCondition<SerpentsTide>(id + 0x10, 6.0f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<SerpentsTide>();
        WindRose(id + 0x20, 2);
        ComponentCondition<SerpentsTide>(id + 0x30, 0.2f, comp => comp.AOEs.Count == 0, "Lines 1");

        ComponentCondition<SerpentsTide>(id + 0x100, 3.1f, comp => comp.AOEs.Count > 0);
        CastStartMulti(id + 0x110, [AID.LeftStrait, AID.RightStrait], 7);
        ComponentCondition<SerpentsTide>(id + 0x120, 1.2f, comp => comp.AOEs.Count == 0, "Lines 2")
            .ActivateOnEnter<LeftStrait>()
            .ActivateOnEnter<RightStrait>();
        ComponentCondition<SerpentsTide>(id + 0x130, 2.8f, comp => comp.AOEs.Count > 0);
        CastEnd(id + 0x140, 2, "Side")
            .DeactivateOnExit<LeftStrait>()
            .DeactivateOnExit<RightStrait>();
        ComponentCondition<SerpentsTide>(id + 0x150, 6.2f, comp => comp.AOEs.Count == 0, "Lines 3")
            .DeactivateOnExit<SerpentsTide>();

        ComponentCondition<Maelstrom>(id + 0x200, 0.6f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Maelstrom>();
        ComponentCondition<Maelstrom>(id + 0x201, 4, comp => comp.NumCasts > 0, "Puddles")
            .DeactivateOnExit<Maelstrom>();
    }

    private void Godsbane(uint id, float delay)
    {
        Cast(id, AID.Godsbane, delay, 5);
        ComponentCondition<Godsbane>(id + 2, 2, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<Godsbane>()
            .DeactivateOnExit<Godsbane>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void NavigatorsTridentAdds(uint id, float delay)
    {
        Cast(id, AID.NavigatorsTrident, delay, 6.5f)
            .ActivateOnEnter<DireStraits>();
        ComponentCondition<DireStraits>(id + 0x10, 1.0f, comp => comp.NumCasts > 0, "Side 1");
        ComponentCondition<DireStraits>(id + 0x11, 1.8f, comp => comp.NumCasts > 1, "Side 2")
            .DeactivateOnExit<DireStraits>();

        ComponentCondition<NavigatorsTridentAOE>(id + 0x20, 1.8f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<NavigatorsTridentAOE>();
        ComponentCondition<NavigatorsTridentAOE>(id + 0x21, 7, comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<SurgingWaveCorridor>() // envcontrol will happen right after knockback end
            .ActivateOnEnter<SerpentsTide>()
            .ActivateOnEnter<NavigatorsTridentKnockback>()
            .DeactivateOnExit<NavigatorsTridentKnockback>()
            .DeactivateOnExit<NavigatorsTridentAOE>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<SerpentsTide>(id + 0x30, 1.2f, comp => comp.AOEs.Count == 0, "Lines")
            .DeactivateOnExit<SerpentsTide>();
    }

    private void SurgingWaveToTheLast(uint id, float delay)
    {
        Cast(id, AID.SurgingWave, delay, 9)
            .ActivateOnEnter<SurgingWaveAOE>()
            .ActivateOnEnter<SurgingWaveShockwave>()
            .ExecOnEnter<SurgingWaveCorridor>(comp => Module.Arena.Center = A32Llymlaen.DefaultCenter + A32Llymlaen.CorridorHalfLength * comp.CorridorDir)
            .ExecOnEnter<SurgingWaveCorridor>(comp => Module.Arena.Bounds = comp.CorridorDir.X > 0 ? A32Llymlaen.EastCorridorBounds : A32Llymlaen.WestCorridorBounds);
        ComponentCondition<SurgingWaveAOE>(id + 0x10, 1, comp => comp.NumCasts > 0)
            .DeactivateOnExit<SurgingWaveAOE>();
        ComponentCondition<SurgingWaveShockwave>(id + 0x11, 0.2f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<SurgingWaveShockwave>();

        CastStart(id + 0x20, AID.ToTheLast, 3.0f)
            .ActivateOnEnter<SurgingWaveFrothingSea>(); // note: first cast should happen ~3.2s later, but for whatever reason it is skipped sometimes...
        ComponentCondition<ToTheLast>(id + 0x30, 6.0f, comp => comp.NumCasts > 0, "Side 1")
            .ActivateOnEnter<ToTheLast>();
        ComponentCondition<ToTheLast>(id + 0x40, 2.0f, comp => comp.NumCasts > 1, "Side 2");
        ComponentCondition<ToTheLast>(id + 0x50, 2.0f, comp => comp.NumCasts > 2, "Side 3")
            .DeactivateOnExit<ToTheLast>();
        CastStartMulti(id + 0x60, [AID.LeftStrait, AID.RightStrait], 1.4f);

        ComponentCondition<SurgingWaveCorridor>(id + 0x70, 4.0f, comp => comp.CorridorDir == default)
            .ActivateOnEnter<LeftStrait>()
            .ActivateOnEnter<RightStrait>()
            .DeactivateOnExit<SurgingWaveFrothingSea>() // note: last cast happens ~0.5s later, but for whatever reason first cast is skipped sometimes, making counting hard
            .DeactivateOnExit<SurgingWaveCorridor>()
            .OnExit(() => (Module.Arena.Center, Module.Arena.Bounds) = (A32Llymlaen.DefaultCenter, A32Llymlaen.DefaultBounds));
        CastEnd(id + 0x80, 2.0f, "Side")
            .DeactivateOnExit<LeftStrait>()
            .DeactivateOnExit<RightStrait>();
    }

    private void DeepDiveHardWater(uint id, float delay)
    {
        Cast(id, AID.DeepDiveHardWater, delay, 9, "Stack x3")
            .ActivateOnEnter<DeepDiveHardWater>()
            .DeactivateOnExit<DeepDiveHardWater>();
    }

    private void StormySeasMaelstrom(uint id, float delay)
    {
        ComponentCondition<Stormwhorl>(id, delay, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Stormwhorl>();
        ComponentCondition<Stormwhorl>(id + 0x10, 4, comp => comp.NumCasts > 0, "Puddles")
            .ActivateOnEnter<Stormwinds>();
        ComponentCondition<Stormwhorl>(id + 0x11, 2, comp => comp.NumCasts > 3)
            .ActivateOnEnter<SerpentsTide>();
        ComponentCondition<Stormwhorl>(id + 0x12, 2, comp => comp.NumCasts > 6)
            .DeactivateOnExit<Stormwhorl>();
        ComponentCondition<Stormwinds>(id + 0x20, 1, comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<Stormwinds>();
        ComponentCondition<SerpentsTide>(id + 0x30, 4.4f, comp => comp.AOEs.Count == 0, "Lines")
            .DeactivateOnExit<SerpentsTide>();

        ComponentCondition<Maelstrom>(id + 0x100, 0.6f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<Maelstrom>();
        WindRoseSeafoamSpiral(id + 0x110, 2.3f)
            .DeactivateOnExit<Maelstrom>();
    }
}
