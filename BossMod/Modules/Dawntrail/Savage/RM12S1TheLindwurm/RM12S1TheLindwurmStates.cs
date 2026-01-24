namespace BossMod.Dawntrail.Savage.RM12S1TheLindwurm;

class RM12S1TheLindwurmStates : StateMachineBuilder
{
    public RM12S1TheLindwurmStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        MortalSlayer(id, 10.2f);
        GrotesquerieAct1(id + 0x10000, 17.3f);

        Timeout(id + 0xFF0000, 10000, "???")
            .ActivateOnEnter<CruelCoil>();
    }

    void MortalSlayer(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_TheFixer, delay, 5, "Raidwide")
            .ActivateOnEnter<TheFixer>()
            .DeactivateOnExit<TheFixer>();

        Cast(id + 0x100, AID._Weaponskill_MortalSlayer, 13.3f, 12)
            .ActivateOnEnter<MortalSlayer>();

        ComponentCondition<MortalSlayer>(id + 0x110, 0, m => m.NumCasts == 2, "Orbs 1");
        ComponentCondition<MortalSlayer>(id + 0x111, 3, m => m.NumCasts == 4, "Orbs 2");
        ComponentCondition<MortalSlayer>(id + 0x112, 3, m => m.NumCasts == 6, "Orbs 3");
        ComponentCondition<MortalSlayer>(id + 0x113, 3, m => m.NumCasts == 8, "Orbs 4")
            .DeactivateOnExit<MortalSlayer>();
    }

    void GrotesquerieAct1(uint id, float delay)
    {
        Cast(id, AID._Weaponskill_GrotesquerieAct1, delay, 3)
            .ActivateOnEnter<PhagocyteSpotlight>()
            .ActivateOnEnter<RavenousReach>()
            .ExecOnEnter<RavenousReach>(r => r.Risky = false);

        ComponentCondition<PhagocyteSpotlight>(id + 0x10, 6.3f, s => s.Casters.Count > 0, "Puddles start");
        ComponentCondition<PhagocyteSpotlight>(id + 0x11, 6.1f, p => p.Finished) // last puddle baits
            .ExecOnExit<RavenousReach>(r => r.Risky = true);

        ComponentCondition<RavenousReach>(id + 0x20, 5.7f, r => r.NumCasts > 0, "Head AOE")
            .ActivateOnEnter<Act1GrotesquerieStackSpread>()
            .ActivateOnEnter<DirectedGrotesquerie>()
            .DeactivateOnExit<RavenousReach>()
            .DeactivateOnExit<PhagocyteSpotlight>();

        ComponentCondition<Act1GrotesquerieStackSpread>(id + 0x21, 0.3f, g => g.NumCasts > 0, "Stack/spread/cones")
            .DeactivateOnExit<Act1GrotesquerieStackSpread>()
            .DeactivateOnExit<DirectedGrotesquerie>();

        ComponentCondition<Burst>(id + 0x30, 8.8f, b => b.NumCasts > 0, "Big puddles")
            .ActivateOnEnter<BurstStackSpread>()
            .ActivateOnEnter<BurstPre>()
            .ActivateOnEnter<Burst>()
            .DeactivateOnExit<BurstPre>()
            .DeactivateOnExit<Burst>();

        ComponentCondition<BurstStackSpread>(id + 0x31, 0.5f, p => p.NumCasts > 0, "Stack + tankbusters")
            .DeactivateOnExit<BurstStackSpread>();

        Cast(id + 0x100, AID._Weaponskill_TheFixer, 5.1f, 5, "Raidwide")
            .ActivateOnEnter<TheFixer>()
            .DeactivateOnExit<TheFixer>();
    }
}
