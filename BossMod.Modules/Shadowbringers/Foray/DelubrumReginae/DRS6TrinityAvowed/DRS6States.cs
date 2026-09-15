namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS6TrinityAvowed;

class DRS6States : StateMachineBuilder
{
    public DRS6States(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<AllegiantArsenal>();
    }

    private void SinglePhase(uint id)
    {
        WrathOfBozja(id, 7.4f, false);
        GloryOfBozja(id + 0x10000, 3.2f);
        AllegiantArsenalAOE(id + 0x20000, 6.3f);
        AllegiantArsenalAOE(id + 0x30000, 5.2f);

        Dictionary<AllegiantArsenal.Order, (uint seqID, Action<uint> buildState)> dispatch = new()
        {
            [AllegiantArsenal.Order.StaffSwordBow] = ((id >> 24) + 1, ForkStaffSwordBow),
            [AllegiantArsenal.Order.BowSwordStaff] = ((id >> 24) + 2, ForkBowSwordStaff),
            [AllegiantArsenal.Order.SwordBowStaff] = ((id >> 24) + 3, ForkSwordBowStaff),
            [AllegiantArsenal.Order.StaffBowSword] = ((id >> 24) + 4, ForkStaffBowSword),
            [AllegiantArsenal.Order.SwordStaffBow] = ((id >> 24) + 5, ForkSwordStaffBow),
            [AllegiantArsenal.Order.BowStaffSword] = ((id >> 24) + 6, ForkBowStaffSword)
        };
        ComponentConditionFork<AllegiantArsenal, AllegiantArsenal.Order>(id + 0x40000, 0, _ => true, comp => comp.Mechanics, dispatch);
    }

    private void ForkStaffSwordBow(uint id)
    {
        // TODO: no idea about timings here
        Staff1(id, 8);
        Sword1(id + 0x100000, 8);
        Bow1(id + 0x200000, 8);
        Staff2(id + 0x300000, 8);
        Sword2(id + 0x400000, 8);
        Bow2(id + 0x500000, 8);
        Enrage(id + 0x600000, 16);
    }

    private void ForkBowSwordStaff(uint id)
    {
        Bow1(id, 6.0f);
        Sword1(id + 0x100000, 7.4f);
        Staff1(id + 0x200000, 7.5f);
        Bow2(id + 0x300000, 9.6f);
        Sword2(id + 0x400000, 8);
        Staff2(id + 0x500000, 8);
        Enrage(id + 0x600000, 16); // TODO: timing
    }

    private void ForkSwordBowStaff(uint id)
    {
        Sword1(id, 5.3f);
        Bow1(id + 0x100000, 8.6f);
        Staff1(id + 0x200000, 8); // note: very high variance here...
        Sword2(id + 0x300000, 7.4f);
        Bow2(id + 0x400000, 8.6f);
        Staff2(id + 0x500000, 8.4f);
        Enrage(id + 0x600000, 16); // TODO: timing
    }

    private void ForkStaffBowSword(uint id)
    {
        Staff1(id, 5.3f);
        Bow1(id + 0x100000, 8.7f);
        Sword1(id + 0x200000, 10.4f);
        Staff2(id + 0x300000, 7.4f);
        Bow2(id + 0x400000, 9.1f);
        Sword2(id + 0x500000, 7.9f);
        Enrage(id + 0x600000, 16); // TODO: timing
    }

    private void ForkSwordStaffBow(uint id)
    {
        Sword1(id, 5.2f);
        Staff1(id + 0x100000, 7.6f);
        Bow1(id + 0x200000, 8.9f);
        Sword2(id + 0x300000, 7.7f);
        Staff2(id + 0x400000, 7.6f);
        Bow2(id + 0x500000, 9.7f);
        Enrage(id + 0x600000, 15.8f);
    }

    private void ForkBowStaffSword(uint id)
    {
        Bow1(id, 6); // note: very high variance here...
        Staff1(id + 0x100000, 7.9f); // note: very high variance here...
        Sword1(id + 0x200000, 7.5f);
        Bow2(id + 0x300000, 8.5f);
        Staff2(id + 0x400000, 8); // note: very high variance here...
        Sword2(id + 0x500000, 7.5f);
        Enrage(id + 0x600000, 20.5f);
    }

    private void Sword1(uint id, float delay)
    {
        AllegiantArsenalAOE(id, delay);

        Cast(id + 0x10000, AID.HotAndColdSword, 4.5f, 3); // note: large variance
        // +1.1s: temperature statuses
        Cast(id + 0x10010, AID.UnwaveringApparition, 6, 3);
        Targetable(id + 0x10020, false, 5.7f, "Disappear"); // note: large variance
        BladeOfEntropy(id + 0x10030, 0.1f, "Sword 1");
        BladeOfEntropy(id + 0x10040, 4.0f, "Sword 2"); // note: large variance
        Targetable(id + 0x10050, true, 3.1f, "Reappear");

        GloryOfBozja(id + 0x20000, 6.5f);
    }

    private void Bow1(uint id, float delay)
    {
        AllegiantArsenalAOE(id, delay);

        Cast(id + 0x10000, AID.QuickMarchBow, 3.1f, 3)
            .ActivateOnEnter<FlamesOfBozja1>()
            .ActivateOnEnter<QuickMarchBow1>(); // debuffs are applied ~1s after cast end
        WrathOfBozja(id + 0x10010, 3.1f, true);
        Cast(id + 0x10020, AID.FlamesOfBozja, 3.2f, 3);
        // +1.1s: flames of bozja aoe cast start
        ComponentCondition<QuickMarch>(id + 0x10030, 5.7f, comp => comp.NumActiveForcedMarches > 0, "Forced march start");
        ComponentCondition<FlamesOfBozja>(id + 0x10040, 4.5f, comp => comp.NumCasts > 0, "Single safe row")
            .DeactivateOnExit<QuickMarch>();

        Cast(id + 0x20000, AID.HotAndColdBow, 0.4f, 3);
        // +1.1s: temperature statuses
        Cast(id + 0x20010, AID.ShimmeringShot, 5, 3);
        ComponentCondition<ShimmeringShot>(id + 0x20020, 16.2f, comp => comp.NumCasts > 0, "Arrows hit")
            .ActivateOnEnter<ShimmeringShot1>() // env controls happen ~15.2s before resolve, arrows spawn ~12.8s before resolve
            .DeactivateOnExit<ShimmeringShot>();
        ComponentCondition<FlamesOfBozja>(id + 0x20030, 2.2f, comp => comp.AOE == null, "Bow 1 resolve")
            .DeactivateOnExit<FlamesOfBozja>();

        GloryOfBozja(id + 0x30000, 5.3f); // TODO: this seems to have slightly different timings depending on forks...
    }

    private void Staff1(uint id, float delay)
    {
        AllegiantArsenalAOE(id, delay);

        Cast(id + 0x10000, AID.HotAndColdStaff, 3.1f, 3);
        // +1.1s: temperature statuses
        Cast(id + 0x10010, AID.QuickMarchStaff, 4.1f, 3);
        // +1.0s: march debuffs (but we start showing hints only after proximity)
        Cast(id + 0x10020, AID.FreedomOfBozja, 3.2f, 3);

        // +1.3s: orb spawn
        // +2.2s: impact visual cast start
        ComponentCondition<ElementalImpact1>(id + 0x10030, 7.2f, comp => comp.NumCasts > 0, "Proximity")
            .ActivateOnEnter<ElementalImpact1>()
            .ActivateOnEnter<ElementalImpact2>()
            .DeactivateOnExit<ElementalImpact1>()
            .DeactivateOnExit<ElementalImpact2>();
        // +0.3s: actual aoes (who cares)
        // +2.0s: blast cast starts

        ComponentCondition<QuickMarch>(id + 0x20000, 6.8f, comp => comp.NumActiveForcedMarches > 0, "Forced march start")
            .ActivateOnEnter<FreedomOfBozja1>()
            .ActivateOnEnter<QuickMarchStaff1>();
        ComponentCondition<FreedomOfBozja>(id + 0x20010, 3.3f, comp => comp.NumCasts > 0, "Orbs hit")
            .DeactivateOnExit<QuickMarch>()
            .DeactivateOnExit<FreedomOfBozja>();

        GloryOfBozja(id + 0x30000, 7.9f); // TODO: this seems to have slightly different timings depending on forks...
    }

    private void Sword2(uint id, float delay)
    {
        AllegiantArsenalAOE(id, delay);

        Cast(id + 0x10000, AID.HotAndColdSword, 4.4f, 3); // note: large variance
        Cast(id + 0x10010, AID.ElementalBrandSword, 4.1f, 3);
        Cast(id + 0x10020, AID.UnwaveringApparition, 3.2f, 3);
        Targetable(id + 0x10030, false, 6.0f, "Disappear");
        BladeOfEntropy(id + 0x10040, 0.1f, "Sword 1");
        BladeOfEntropy(id + 0x10050, 4.0f, "Sword 2");
        Targetable(id + 0x10060, true, 3.1f, "Reappear");

        GloryOfBozja(id + 0x20000, 6.5f);
    }

    private void Bow2(uint id, float delay)
    {
        AllegiantArsenalAOE(id, delay);

        Cast(id + 0x10000, AID.UnseenEyeBow, 3.1f, 3);
        Cast(id + 0x10010, AID.FlamesOfBozja, 3.1f, 3)
            .ActivateOnEnter<GleamingArrow>(); // PATE events happen together with cast-start, actual casts start ~2.1s later - if we want to rely on former, need to activate earlier
        ComponentCondition<GleamingArrow>(id + 0x10020, 5.1f, comp => comp.NumCasts > 0, "Criss-cross")
            .DeactivateOnExit<GleamingArrow>();
        ComponentCondition<FlamesOfBozja>(id + 0x10030, 5, comp => comp.NumCasts > 0, "Single safe row")
            .ActivateOnEnter<FlamesOfBozja2>(); // activate late, since criss-cross have to be resolved first

        Cast(id + 0x20000, AID.HotAndColdBow, 0, 3); // note: very high variance, sometimes even starts slightly beofre flames of bozja end...
        Cast(id + 0x20010, AID.ElementalBrandBow, 4.2f, 3);
        Cast(id + 0x20020, AID.QuickMarchBow, 3.2f, 3);
        Cast(id + 0x20030, AID.ShimmeringShot, 3.9f, 3);
        ComponentCondition<QuickMarch>(id + 0x20040, 13.2f, comp => comp.NumActiveForcedMarches > 0, "Forced march start")
            .ActivateOnEnter<ShimmeringShot2>() // env controls happen ~0.9s after cast end, arrows spawn ~3.2s after cast end
            .ActivateOnEnter<QuickMarchBow2>();
        ComponentCondition<ShimmeringShot>(id + 0x20050, 4, comp => comp.NumCasts > 0, "Arrows hit")
            .DeactivateOnExit<QuickMarch>()
            .DeactivateOnExit<ShimmeringShot>();
        ComponentCondition<FlamesOfBozja>(id + 0x20060, 2.2f, comp => comp.AOE == null, "Bow 2 resolve")
            .DeactivateOnExit<FlamesOfBozja>();

        GloryOfBozja(id + 0x30000, 5.3f);
        WrathOfBozja(id + 0x40000, 3.2f, false);
    }

    private void Staff2(uint id, float delay)
    {
        AllegiantArsenalAOE(id, delay);

        Cast(id + 0x10000, AID.HotAndColdStaff, 3.1f, 3);
        Cast(id + 0x10010, AID.ElementalBrandStaff, 4.1f, 3);
        Cast(id + 0x10020, AID.FreedomOfBozja, 3.2f, 3)
            .ActivateOnEnter<ElementalImpact1>()
            .ActivateOnEnter<ElementalImpact2>();
        Cast(id + 0x10030, AID.UnseenEyeStaff, 3.1f, 3);
        ComponentCondition<ElementalImpact1>(id + 0x10040, 1.0f, comp => comp.NumCasts > 0, "Proximity", 10)
            .DeactivateOnExit<ElementalImpact1>()
            .DeactivateOnExit<ElementalImpact2>();

        ComponentCondition<FreedomOfBozja>(id + 0x20000, 10, comp => comp.NumCasts > 0, "Orbs + criss-cross", 10)
            .ActivateOnEnter<FreedomOfBozja2>()
            .ActivateOnEnter<GleamingArrow>()
            .DeactivateOnExit<FreedomOfBozja>()
            .DeactivateOnExit<GleamingArrow>();

        GloryOfBozja(id + 0x30000, 8);
    }

    private void WrathOfBozja(uint id, float delay, bool bow)
    {
        Cast(id, bow ? AID.WrathOfBozjaBow : AID.WrathOfBozja, delay, 5, "Tankbuster")
            .ActivateOnEnter<WrathOfBozja>(!bow)
            .ActivateOnEnter<WrathOfBozjaBow>(bow)
            .DeactivateOnExit<WrathOfBozja>(!bow)
            .DeactivateOnExit<WrathOfBozjaBow>(bow)
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void GloryOfBozja(uint id, float delay)
    {
        Cast(id, AID.GloryOfBozja, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        // note: second half of raid is hit 0.6s later, fuck that...
    }

    private void AllegiantArsenalAOE(uint id, float delay)
    {
        CastMulti(id, [AID.AllegiantArsenalSword, AID.AllegiantArsenalBow, AID.AllegiantArsenalStaff], delay, 3);
        ComponentCondition<AllegiantArsenal>(id + 0x10, 5.2f, comp => !comp.Active, "Weapon aoe");
    }

    private void BladeOfEntropy(uint id, float delay, string name)
    {
        CastMulti(id, [AID.BladeOfEntropyBC11, AID.BladeOfEntropyBC21, AID.BladeOfEntropyBH11, AID.BladeOfEntropyBH21, AID.BladeOfEntropyAC11, AID.BladeOfEntropyAC21, AID.BladeOfEntropyAH11, AID.BladeOfEntropyAH21], delay, 10, name)
            .ActivateOnEnter<BladeOfEntropy>()
            .DeactivateOnExit<BladeOfEntropy>();
    }

    private void Enrage(uint id, float delay)
    {
        Cast(id, AID.Enrage, delay, 12, "Enrage"); // boss becomes untargetable at the end of the cast
    }
}
