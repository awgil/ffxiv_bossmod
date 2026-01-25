namespace BossMod.Dawntrail.Savage.RM12S1TheLindwurm;

class RM12S1TheLindwurmStates : StateMachineBuilder
{
    public RM12S1TheLindwurmStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Cast(id, AID.TheFixer, 10.2f, 5, "Raidwide")
            .ActivateOnEnter<TheFixer>()
            .DeactivateOnExit<TheFixer>();

        MortalSlayer(id + 0x100, 13.3f);
        GrotesquerieAct1(id + 0x10000, 17.3f);
        GrotesquerieAct2(id + 0x20000, 9.2f);
        GrotesquerieAct3(id + 0x30000, 6.1f);
        GrotesquerieCurtainCall(id + 0x40000, 9.1f);
        MortalSlayer(id + 0x50000, 13.8f);
        Slaughtershed(id + 0x60000, 12.2f);
        Slaughtershed(id + 0x70000, 3.6f);
        Slaughtershed(id + 0x80000, 3.8f);

        Targetable(id + 0x90000, false, 8.1f, "Boss disappears");
        Cast(id + 0x90001, AID.TheFixerEnrage, 0, 5, "Enrage");
    }

    void MortalSlayer(uint id, float delay)
    {
        Cast(id, AID.MortalSlayerBoss, delay, 12)
            .ActivateOnEnter<MortalSlayer>();

        ComponentCondition<MortalSlayer>(id + 0x10, 0.1f, m => m.NumCasts == 2, "Orbs 1");
        ComponentCondition<MortalSlayer>(id + 0x11, 3, m => m.NumCasts == 4, "Orbs 2");
        ComponentCondition<MortalSlayer>(id + 0x12, 3, m => m.NumCasts == 6, "Orbs 3");
        ComponentCondition<MortalSlayer>(id + 0x13, 3, m => m.NumCasts == 8, "Orbs 4")
            .DeactivateOnExit<MortalSlayer>();
    }

    void GrotesquerieAct1(uint id, float delay)
    {
        Cast(id, AID.GrotesquerieAct1, delay, 3)
            .ActivateOnEnter<PhagocyteSpotlightPlayer>()
            .ActivateOnEnter<Act1GrotesquerieSpreadHint>()
            .ActivateOnEnter<RavenousReach>()
            .ExecOnEnter<RavenousReach>(r => r.Risky = false);

        ComponentCondition<PhagocyteSpotlightPlayer>(id + 0x10, 6.3f, s => s.Casters.Count > 0, "Puddles start");
        ComponentCondition<PhagocyteSpotlightPlayer>(id + 0x11, 6, p => p.Finished) // last puddle baits
            .DeactivateOnExit<Act1GrotesquerieSpreadHint>()
            .ExecOnExit<RavenousReach>(r => r.Risky = true);

        ComponentCondition<RavenousReach>(id + 0x20, 5.7f, r => r.NumCasts > 0, "Head AOE")
            .ActivateOnEnter<Act1GrotesquerieStackSpread>()
            .ActivateOnEnter<DirectedGrotesquerie>()
            .DeactivateOnExit<RavenousReach>()
            .DeactivateOnExit<PhagocyteSpotlightPlayer>();

        ComponentCondition<Act1GrotesquerieStackSpread>(id + 0x21, 0.3f, g => g.NumCasts > 0, "Stack/spread/cones")
            .DeactivateOnExit<Act1GrotesquerieStackSpread>()
            .DeactivateOnExit<DirectedGrotesquerie>();

        ComponentCondition<Burst>(id + 0x30, 8.7f, b => b.NumCasts > 0, "Big puddles")
            .ActivateOnEnter<BurstStackSpread>()
            .ActivateOnEnter<BurstPre>()
            .ActivateOnEnter<Burst>()
            .DeactivateOnExit<BurstPre>()
            .DeactivateOnExit<Burst>();

        ComponentCondition<BurstStackSpread>(id + 0x31, 0.5f, p => p.NumCasts > 0, "Stack + tankbusters")
            .DeactivateOnExit<BurstStackSpread>();

        Cast(id + 0x100, AID.TheFixer, 5.1f, 5, "Raidwide")
            .ActivateOnEnter<TheFixer>()
            .DeactivateOnExit<TheFixer>();
    }

    void GrotesquerieAct2(uint id, float delay)
    {
        Cast(id, AID.GrotesquerieAct2, delay, 3)
            .ActivateOnEnter<CruelCoil>()
            .ActivateOnEnter<Constrictor>()
            .ActivateOnEnter<Act2PhagocyteSpotlight>()
            .ActivateOnEnter<Act2Assignments>()
            .ActivateOnEnter<RoilingMass>()
            .ActivateOnEnter<Act2DramaticLysis>()
            .ActivateOnEnter<Act2CellChains>();

        CastMulti(id + 0x100, [AID.CruelCoilBoss4, AID.CruelCoilBoss2, AID.CruelCoilBoss3, AID.CruelCoilBoss1], 12.1f, 3);

        ComponentCondition<Act2CellChains>(id + 0x110, 11.9f, c => c.NumChains > 0, "Chains 1")
            .DeactivateOnExit<Act2PhagocyteSpotlight>();
        ComponentCondition<Act2CellChains>(id + 0x111, 5, c => c.NumChains > 1, "Chains 2");
        ComponentCondition<RoilingMass>(id + 0x112, 0.8f, r => r.BlobCasts > 0, "Blob tower 1");
        ComponentCondition<Act2CellChains>(id + 0x113, 4.2f, c => c.NumChains > 2, "Chains 3");
        ComponentCondition<RoilingMass>(id + 0x114, 0.8f, r => r.BlobCasts > 1, "Blob tower 2");
        ComponentCondition<Act2CellChains>(id + 0x115, 4.2f, c => c.NumChains > 3, "Chains 4");
        ComponentCondition<RoilingMass>(id + 0x116, 0.8f, r => r.BlobCasts > 2, "Blob tower 3");
        ComponentCondition<RoilingMass>(id + 0x117, 5, r => r.BlobCasts > 3, "Blob tower 4");

        ComponentCondition<Constrictor>(id + 0x200, 9, c => c.NumCasts > 0, "Center AOE")
            .DeactivateOnExit<CruelCoil>()
            .DeactivateOnExit<Constrictor>()
            .DeactivateOnExit<RoilingMass>()
            .DeactivateOnExit<Act2DramaticLysis>()
            .DeactivateOnExit<Act2Assignments>()
            .DeactivateOnExit<Act2CellChains>();
    }

    void GrotesquerieAct3(uint id, float delay)
    {
        Cast(id, AID.SplattershedBoss2, delay, 3)
            .ActivateOnEnter<Splattershed>();
        ComponentCondition<Splattershed>(id + 2, 2.5f, s => s.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<Splattershed>();

        Cast(id + 0x100, AID.GrotesquerieAct3, 14.8f, 3)
            .ActivateOnEnter<GrandEntrance>()
            .ActivateOnEnter<BringDownTheHouse>()
            .ActivateOnEnter<BringDownTheHouseSmall>()
            .ActivateOnEnter<BringDownTheHouseMedium>()
            .ActivateOnEnter<BringDownTheHouseLarge>()
            .ActivateOnEnter<MetamitosisProjected>()
            .ActivateOnEnter<MitoticPhaseDramaticLysis>()
            .ActivateOnEnter<MetamitosisTower>()
            .ExecOnEnter<MitoticPhaseDramaticLysis>(l => l.EnableHints = false);

        Targetable(id + 0x102, false, 3.1f, "Boss disappears");

        ComponentCondition<BringDownTheHouse>(id + 0x110, 7, b => b.Platforms, "Platforms")
            .DeactivateOnExit<BringDownTheHouseSmall>()
            .DeactivateOnExit<BringDownTheHouseMedium>()
            .DeactivateOnExit<BringDownTheHouseLarge>()
            .DeactivateOnExit<GrandEntrance>()
            .ExecOnExit<MitoticPhaseDramaticLysis>(l => l.EnableHints = true);

        ComponentCondition<MitoticPhaseDramaticLysis>(id + 0x120, 2.6f, d => d.Spreads.Count == 0, "Spreads")
            .DeactivateOnExit<MitoticPhaseDramaticLysis>();
        ComponentCondition<MetamitosisTower>(id + 0x130, 0.2f, m => m.NumCasts > 0, "Towers")
            .ActivateOnEnter<SplitScourge>()
            .DeactivateOnExit<MetamitosisTower>()
            .DeactivateOnExit<MetamitosisProjected>();

        Targetable(id + 0x140, true, 2.5f, "Boss reappears");

        ComponentCondition<SplitScourge>(id + 0x200, 7.4f, s => s.NumCasts > 0, "Line tankbusters")
            .DeactivateOnExit<SplitScourge>();
        ComponentCondition<VenomousScourge>(id + 0x210, 2.4f, v => v.NumCasts > 0, "East/west baits")
            .ActivateOnEnter<VenomousScourge>()
            .DeactivateOnExit<VenomousScourge>();

        Cast(id + 0x300, AID.TheFixer, 4.3f, 5, "Raidwide")
            .ActivateOnEnter<TheFixer>()
            .DeactivateOnExit<TheFixer>();
    }

    void GrotesquerieCurtainCall(uint id, float delay)
    {
        Cast(id, AID.GrotesquerieCurtainCall, delay, 3)
            .ActivateOnEnter<RavenousReachInverted>()
            .ActivateOnEnter<PhagocyteSpotlightPlayer>()
            .ExecOnEnter<PhagocyteSpotlightPlayer>(p => p.FinalCastDelay = 7)
            .ExecOnEnter<RavenousReachInverted>(r => r.Risky = false);

        ComponentCondition<PhagocyteSpotlightPlayer>(id + 0x10, 1, p => p.Casters.Count > 0, "Puddles start");
        ComponentCondition<PhagocyteSpotlightPlayer>(id + 0x11, 8, p => p.Finished)
            .ExecOnExit<RavenousReachInverted>(r => r.Risky = true);

        ComponentCondition<RavenousReachInverted>(id + 0x20, 5.8f, r => r.NumCasts > 0, "Head AOE")
            .ActivateOnEnter<CurtainCallStackSpread>()
            .DeactivateOnExit<RavenousReachInverted>()
            .DeactivateOnExit<PhagocyteSpotlightPlayer>();
        ComponentCondition<CurtainCallStackSpread>(id + 0x30, 0.4f, c => c.NumSpreads > 0, "Spreads")
            .ActivateOnEnter<BurstPre>()
            .ActivateOnEnter<Burst>()
            .DeactivateOnExit<CurtainCallStackSpread>()
            .ExecOnEnter<BurstPre>(b => b.Risky = false);

        ComponentCondition<CurtainCallChains>(id + 0x40, 4.8f, c => c.TethersAssigned, "Chains appear")
            .ActivateOnEnter<CurtainCallChainSpread>()
            .ActivateOnEnter<CurtainCallChains>();

        Timeout(id + 0x43, 3).ExecOnExit<BurstPre>(b => b.Risky = true);

        ComponentCondition<Burst>(id + 0x50, 4.9f, b => b.NumCasts > 0, "Safe corners")
            .DeactivateOnExit<BurstPre>()
            .DeactivateOnExit<Burst>()
            .DeactivateOnExit<CurtainCallChainSpread>()
            .DeactivateOnExit<CurtainCallChains>()
            .DeactivateOnExit<BringDownTheHouse>();

        Cast(id + 0x100, AID.SplattershedBoss1, 3.7f, 3)
            .ActivateOnEnter<Splattershed>();
        ComponentCondition<Splattershed>(id + 0x102, 2.5f, s => s.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<Splattershed>();
    }

    void Slaughtershed(uint id, float delay)
    {
        // i think this cast depends on whether the boss is in arms or heads mode
        CastMulti(id, [AID.SlaughtershedBoss1, AID.SlaughtershedBoss2], delay, 3)
            .ActivateOnEnter<Slaughtershed>()
            .ActivateOnEnter<BurstPre>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<SlaughtershedStackSpread>()
            .ActivateOnEnter<SerpentineScourge>()
            .ActivateOnEnter<RaptorKnuckles>()
            .ActivateOnEnter<ArmCounter>();

        ComponentCondition<Slaughtershed>(id + 2, 2.5f, s => s.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<Slaughtershed>();

        ComponentCondition<SlaughtershedStackSpread>(id + 3, 8.7f, s => s.NumFinishedSpreads > 0, "Stack + spread")
            .DeactivateOnExit<SlaughtershedStackSpread>();

        ComponentCondition<Burst>(id + 4, 0.5f, b => b.NumCasts > 0, "Safe corners")
            .DeactivateOnExit<BurstPre>()
            .DeactivateOnExit<Burst>()
            .ExecOnExit<SerpentineScourge>(s => s.Draw = true)
            .ExecOnExit<RaptorKnuckles>(k => k.Draw = true);

        ComponentCondition<ArmCounter>(id + 0x10, 6.1f, c => c.NumCasts > 0, "Left/right 1");
        ComponentCondition<ArmCounter>(id + 0x11, 4.6f, c => c.NumCasts > 1, "Left/right 2")
            .DeactivateOnExit<SerpentineScourge>()
            .DeactivateOnExit<RaptorKnuckles>()
            .DeactivateOnExit<ArmCounter>();
    }
}
