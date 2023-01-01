using System;
using System.Collections.Generic;

namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS5TrinityAvowed
{
    class DRS5States : StateMachineBuilder
    {
        public DRS5States(BossModule module) : base(module)
        {
            DeathPhase(0, SinglePhase)
                .ActivateOnEnter<AllegiantArsenal>();
        }

        private void SinglePhase(uint id)
        {
            WrathOfBozja(id, 8.5f);
            GloryOfBozja(id + 0x10000, 3.2f);
            AllegiantArsenalAOE(id + 0x20000, 6.3f);
            AllegiantArsenalAOE(id + 0x30000, 5.2f);

            Dictionary<AllegiantArsenal.Order, (uint seqID, Action<uint> buildState)> dispatch = new();
            dispatch[AllegiantArsenal.Order.StaffSwordBow] = ((id >> 24) + 1, ForkStaffSwordBow);
            dispatch[AllegiantArsenal.Order.BowSwordStaff] = ((id >> 24) + 2, ForkBowSwordStaff);
            dispatch[AllegiantArsenal.Order.SwordBowStaff] = ((id >> 24) + 3, ForkSwordBowStaff);
            dispatch[AllegiantArsenal.Order.StaffBowSword] = ((id >> 24) + 4, ForkStaffBowSword);
            dispatch[AllegiantArsenal.Order.SwordStaffBow] = ((id >> 24) + 5, ForkSwordStaffBow);
            dispatch[AllegiantArsenal.Order.BowStaffSword] = ((id >> 24) + 6, ForkBowStaffSword);
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
            SimpleState(id + 0xFF0000, 10, "Enrage"); // TODO: action
        }

        private void ForkBowSwordStaff(uint id)
        {
            Bow1(id, 6.0f);
            Sword1(id + 0x100000, 7.4f);
            Staff1(id + 0x200000, 7.5f);
            Bow2(id + 0x300000, 9.6f);
            Sword2(id + 0x400000, 8);
            Staff2(id + 0x500000, 8);
            SimpleState(id + 0xFF0000, 10, "Enrage"); // TODO: action
        }

        private void ForkSwordBowStaff(uint id)
        {
            // TODO: no idea about timings here
            Sword1(id, 8);
            Bow1(id + 0x100000, 8);
            Staff1(id + 0x200000, 8);
            Sword2(id + 0x300000, 8);
            Bow2(id + 0x400000, 8);
            Staff2(id + 0x500000, 8);
            SimpleState(id + 0xFF0000, 10, "Enrage"); // TODO: action
        }

        private void ForkStaffBowSword(uint id)
        {
            // TODO: no idea about timings here
            Staff1(id, 8);
            Bow1(id + 0x100000, 8);
            Sword1(id + 0x200000, 8);
            Staff2(id + 0x300000, 8);
            Bow2(id + 0x400000, 8);
            Sword2(id + 0x500000, 8);
            SimpleState(id + 0xFF0000, 10, "Enrage"); // TODO: action
        }

        private void ForkSwordStaffBow(uint id)
        {
            // TODO: no idea about timings here
            Sword1(id, 8);
            Staff1(id + 0x100000, 8);
            Bow1(id + 0x200000, 8);
            Sword2(id + 0x300000, 8);
            Staff2(id + 0x400000, 8);
            Bow2(id + 0x500000, 8);
            SimpleState(id + 0xFF0000, 10, "Enrage"); // TODO: action
        }

        private void ForkBowStaffSword(uint id)
        {
            Bow1(id, 6.3f);
            Staff1(id + 0x100000, 7.9f);
            Sword1(id + 0x200000, 7.5f);
            Bow2(id + 0x300000, 8.4f);
            Staff2(id + 0x400000, 7.6f);
            Sword2(id + 0x500000, 7.5f); // TODO: timing
            SimpleState(id + 0xFF0000, 10, "Enrage"); // TODO: timing, action
        }

        private void Sword1(uint id, float delay)
        {
            AllegiantArsenalAOE(id, delay);

            Cast(id + 0x10000, AID.HotAndColdSword, 4.9f, 3);
            // +1.1s: temperature statuses
            Cast(id + 0x10010, AID.UnwaveringApparition, 4.1f, 3);
            Targetable(id + 0x10020, false, 5.7f, "Disappear");
            CastMulti(id + 0x10030, new[] { AID.BladeOfEntropyBC11, AID.BladeOfEntropyBC21, AID.BladeOfEntropyBH11, AID.BladeOfEntropyBH21 }, 0.1f, 10, "Sword 1")
                .ActivateOnEnter<BladeOfEntropy>()
                .DeactivateOnExit<BladeOfEntropy>();
            CastMulti(id + 0x10040, new[] { AID.BladeOfEntropyBC11, AID.BladeOfEntropyBC21, AID.BladeOfEntropyBH11, AID.BladeOfEntropyBH21 }, 3.7f, 10, "Sword 2")
                .ActivateOnEnter<BladeOfEntropy>()
                .DeactivateOnExit<BladeOfEntropy>();
            Targetable(id + 0x10050, true, 3.1f, "Reappear");

            GloryOfBozja(id + 0x20000, 6.5f);
        }

        private void Bow1(uint id, float delay)
        {
            AllegiantArsenalAOE(id, delay);

            Cast(id + 0x10000, AID.QuickMarchBow, 3.2f, 3)
                .ActivateOnEnter<FlamesOfBozja1>()
                .ActivateOnEnter<QuickMarchBow1>(); // debuffs are applied ~1s after cast end
            WrathOfBozja(id + 0x10010, 3.1f);
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

            GloryOfBozja(id + 0x30000, 5.3f);
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

            ComponentCondition<QuickMarch>(id + 0x20000, 6.7f, comp => comp.NumActiveForcedMarches > 0, "Forced march start")
                .ActivateOnEnter<FreedomOfBozja1>()
                .ActivateOnEnter<QuickMarchStaff1>();
            ComponentCondition<FreedomOfBozja>(id + 0x20010, 3.3f, comp => comp.NumCasts > 0, "Orbs hit")
                .DeactivateOnExit<QuickMarch>()
                .DeactivateOnExit<FreedomOfBozja>();

            GloryOfBozja(id + 0x30000, 7.9f);
        }

        private void Sword2(uint id, float delay)
        {
            AllegiantArsenalAOE(id, delay);

            // TODO: no idea about timings here
            Cast(id + 0x10000, AID.HotAndColdSword, 4.9f, 3);
            Cast(id + 0x10010, AID.ElementalBrandSword, 4.1f, 3);
            Cast(id + 0x10020, AID.UnwaveringApparition, 4.1f, 3);
            Targetable(id + 0x10030, false, 5.7f, "Disappear");
            CastMulti(id + 0x10040, new[] { AID.BladeOfEntropyBC11, AID.BladeOfEntropyBC21, AID.BladeOfEntropyBH11, AID.BladeOfEntropyBH21 }, 0.1f, 10, "Sword 1")
                .ActivateOnEnter<BladeOfEntropy>()
                .DeactivateOnExit<BladeOfEntropy>();
            CastMulti(id + 0x10050, new[] { AID.BladeOfEntropyBC11, AID.BladeOfEntropyBC21, AID.BladeOfEntropyBH11, AID.BladeOfEntropyBH21 }, 3.7f, 10, "Sword 2")
                .ActivateOnEnter<BladeOfEntropy>()
                .DeactivateOnExit<BladeOfEntropy>();
            Targetable(id + 0x10060, true, 3.1f, "Reappear");

            GloryOfBozja(id + 0x20000, 6.5f);
        }

        private void Bow2(uint id, float delay)
        {
            AllegiantArsenalAOE(id, delay);

            Cast(id + 0x10000, AID.UnseenEye, 3.1f, 3);
            Cast(id + 0x10010, AID.FlamesOfBozja, 3.1f, 3)
                .ActivateOnEnter<GleamingArrow>(); // PATE events happen together with cast-start, actual casts start ~2.1s later - if we want to rely on former, need to activate earlier
            ComponentCondition<GleamingArrow>(id + 0x10020, 5.1f, comp => comp.NumCasts > 0, "Criss-cross")
                .DeactivateOnExit<GleamingArrow>();
            ComponentCondition<FlamesOfBozja>(id + 0x10030, 5, comp => comp.NumCasts > 0, "Single safe row")
                .ActivateOnEnter<FlamesOfBozja2>(); // activate late, since criss-cross have to be resolved first

            Cast(id + 0x20000, AID.HotAndColdBow, 0, 3); // this can start slightly earlier than flames of bozja end, but whatever...
            Cast(id + 0x20010, AID.ElementalBrandBow, 4.2f, 3);
            Cast(id + 0x20020, AID.QuickMarchBow, 3.2f, 3);
            Cast(id + 0x20030, AID.ShimmeringShot, 3.9f, 3);
            ComponentCondition<QuickMarch>(id + 0x20040, 13.2f, comp => comp.NumActiveForcedMarches > 0, "Forced march start")
                .ActivateOnEnter<ShimmeringShot2>() // env controls happen ~0.9s after cast end, arrows spawn ~3.2s after cast end
                .ActivateOnEnter<QuickMarchBow2>();
            ComponentCondition<ShimmeringShot>(id + 0x20050, 4, comp => comp.NumCasts > 0, "Arrows hit")
                .DeactivateOnExit<QuickMarch>()
                .DeactivateOnExit<ShimmeringShot>();
            ComponentCondition<FlamesOfBozja>(id + 0x20060, 2.1f, comp => comp.AOE == null, "Bow 1 resolve")
                .DeactivateOnExit<FlamesOfBozja>();

            GloryOfBozja(id + 0x30000, 5.3f);
            WrathOfBozja(id + 0x40000, 3.2f);
        }

        private void Staff2(uint id, float delay)
        {
            AllegiantArsenalAOE(id, delay);

            Cast(id + 0x10000, AID.HotAndColdStaff, 3.1f, 3);
            Cast(id + 0x10010, AID.ElementalBrandStaff, 4.1f, 3);
            // TODO: timings below are guesses until i get a log
            Cast(id + 0x10020, AID.FreedomOfBozja, 3.2f, 3)
                .ActivateOnEnter<ElementalImpact1>()
                .ActivateOnEnter<ElementalImpact2>();
            Cast(id + 0x10030, AID.UnseenEye, 3.1f, 3);
            ComponentCondition<ElementalImpact1>(id + 0x10040, 1.2f, comp => comp.NumCasts > 0, "Proximity", 10)
                .DeactivateOnExit<ElementalImpact1>()
                .DeactivateOnExit<ElementalImpact2>();

            ComponentCondition<FreedomOfBozja>(id + 0x20000, 10, comp => comp.NumCasts > 0, "Orbs + criss-cross", 10)
                .ActivateOnEnter<FreedomOfBozja2>()
                .ActivateOnEnter<GleamingArrow>()
                .DeactivateOnExit<FreedomOfBozja>()
                .DeactivateOnExit<GleamingArrow>();

            GloryOfBozja(id + 0x30000, 7.9f);
        }

        // TODO: component
        private void WrathOfBozja(uint id, float delay)
        {
            CastMulti(id, new[] { AID.WrathOfBozja, AID.WrathOfBozjaBow }, delay, 5, "Tankbuster")
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
            CastMulti(id, new[] { AID.AllegiantArsenalSword, AID.AllegiantArsenalBow, AID.AllegiantArsenalStaff }, delay, 3);
            ComponentCondition<AllegiantArsenal>(id + 0x10, 5.2f, comp => !comp.Active, "Weapon aoe");
        }
    }
}
