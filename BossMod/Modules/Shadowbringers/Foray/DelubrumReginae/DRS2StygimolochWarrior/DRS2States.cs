namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS2StygimolochWarrior;

class DRS2States : StateMachineBuilder
{
    public DRS2States(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<Devour>(); // TODO: reconsider...
    }

    private void SinglePhase(uint id)
    {
        SurgeOfVigor(id, 8.2f);
        UnrelentingCharge(id + 0x10000, 9.2f);
        Entrapment(id + 0x20000, 9.0f);
        ViciousSwipeCrazedRampage(id + 0x30000, 1.1f);
        FocusedTremorForcefulStrike(id + 0x40000, 8.2f);
        InescapableEntrapment1(id + 0x50000, 11.5f);
        SurgeOfVigor(id + 0x60000, 9.4f); // 12.6 if previous was withering curse
        FocusedTremorFlailingStrike(id + 0x70000, 11.2f);
        InescapableEntrapment2(id + 0x80000, 9.8f);
        FocusedTremorCoerceForcefulStrike(id + 0x90000, 9.7f);
        SurgeOfVigor(id + 0xA0000, 10.2f);
        UnrelentingCharge(id + 0xB0000, 11.2f);
        ViciousSwipeCrazedRampage(id + 0xC0000, 1.6f);
        SunsIre(id + 0xD0000, 9.5f);
    }

    private void SurgeOfVigor(uint id, float delay)
    {
        // note: it could be preceeded by optional devour cast, ignore it
        Condition(id, delay, () => Module.PrimaryActor.CastInfo?.IsSpell(AID.SurgeOfVigor) ?? false, maxOverdue: 100)
            .SetHint(StateMachine.StateHint.BossCastStart);
        CastEnd(id + 1, 3, "Damage up");
    }

    private void UnrelentingCharge(uint id, float delay)
    {
        Cast(id, AID.UnrelentingCharge, delay, 3)
            .ActivateOnEnter<UnrelentingCharge>();
        ComponentCondition<UnrelentingCharge>(id + 0x10, 0.3f, comp => comp.NumCasts >= 1, "Knockback 1");
        ComponentCondition<UnrelentingCharge>(id + 0x11, 1.6f, comp => comp.NumCasts >= 2, "Knockback 2");
        ComponentCondition<UnrelentingCharge>(id + 0x12, 1.6f, comp => comp.NumCasts >= 3, "Knockback 3")
            .DeactivateOnExit<UnrelentingCharge>();
    }

    private void Entrapment(uint id, float delay)
    {
        Cast(id, AID.Entrapment, delay, 3)
            .ActivateOnEnter<EntrapmentAttract>();
        ComponentCondition<EntrapmentAttract>(id + 0x10, 0.8f, comp => comp.NumCasts > 0, "Traps")
            .DeactivateOnExit<EntrapmentAttract>();

        Cast(id + 0x20, AID.LethalBlow, 1.3f, 20, "Traps deadline")
            .ActivateOnEnter<LethalBlow>()
            .ActivateOnEnter<EntrapmentNormal>()
            .DeactivateOnExit<LethalBlow>();
        ComponentCondition<Entrapment>(id + 0x30, 1.0f, comp => comp.NumCasts >= 16, "Traps resolve")
            .DeactivateOnExit<Entrapment>();
    }

    private void InescapableEntrapment1(uint id, float delay)
    {
        Cast(id, AID.InescapableEntrapment, delay, 3, "Traps")
            .ActivateOnEnter<EntrapmentInescapable>();
        CastMulti(id + 0x10, [AID.SurgingFlames, AID.WitheringCurse], 8.2f, 13, "Traps resolve (ice/mini)") // note: different start delay depending on cast
            .DeactivateOnExit<EntrapmentInescapable>();
        // note: withering curse is followed by devour here
    }

    private void InescapableEntrapment2(uint id, float delay)
    {
        Cast(id, AID.InescapableEntrapment, delay, 3, "Traps")
            .ActivateOnEnter<EntrapmentInescapable>();
        CastMulti(id + 0x10, [AID.SurgingFlames, AID.WitheringCurse], 3.2f, 13, "Traps resolve (ice/mini)"); // note: different start delay depending on cast
        // note: withering curse is followed by devour here

        Condition(id + 0x100, 6.6f, () => Module.PrimaryActor.CastInfo?.IsSpell(AID.SurgingFlood) ?? false, maxOverdue: 100) // 4.2 if previous was surging flames
            .SetHint(StateMachine.StateHint.BossCastStart);
        CastEnd(id + 0x101, 10, "Traps resolve (toad)");
        Cast(id + 0x110, AID.LeapingSpark, 0.3f, 8, "Remove toad")
            .DeactivateOnExit<EntrapmentInescapable>();
        ComponentCondition<LeapingSpark>(id + 0x120, 0.5f, comp => comp.NumCasts >= 1)
            .ActivateOnEnter<LeapingSpark>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<LeapingSpark>(id + 0x121, 1.1f, comp => comp.NumCasts >= 2)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<LeapingSpark>(id + 0x122, 1.1f, comp => comp.NumCasts >= 3, "Raidwide x3")
            .DeactivateOnExit<LeapingSpark>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void ViciousSwipeCrazedRampage(uint id, float delay)
    {
        Cast(id, AID.ViciousSwipe, delay, 4, "Out")
            .ActivateOnEnter<ViciousSwipe>()
            .ActivateOnEnter<CrazedRampage>() // starts at the same time
            .DeactivateOnExit<ViciousSwipe>();
        ComponentCondition<CrazedRampage>(id + 0x10, 2, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<CrazedRampage>();
    }

    private State FocusedTremorForcefulStrike(uint id, float delay)
    {
        Cast(id, AID.FocusedTremor, delay, 3);
        ComponentCondition<FocusedTremorLarge>(id + 0x10, 0.8f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<FocusedTremorLarge>();

        CastStart(id + 0x20, AID.ForcefulStrike, 2.7f);
        ComponentCondition<FocusedTremorLarge>(id + 0x30, 5.2f, comp => comp.NumCasts >= 1, "Tile 1");
        ComponentCondition<FocusedTremorLarge>(id + 0x31, 2.0f, comp => comp.NumCasts >= 2, "Tile 2");
        ComponentCondition<FocusedTremorLarge>(id + 0x32, 2.0f, comp => comp.NumCasts >= 3, "Tile 3");
        ComponentCondition<FocusedTremorLarge>(id + 0x33, 2.0f, comp => comp.NumCasts >= 4, "Tile 4")
            .ActivateOnEnter<ForcefulStrike>()
            .DeactivateOnExit<FocusedTremorLarge>();
        return CastEnd(id + 0x40, 3.8f, "Cleave")
            .DeactivateOnExit<ForcefulStrike>();
    }

    private void FocusedTremorFlailingStrike(uint id, float delay)
    {
        Cast(id, AID.FocusedTremor, delay, 3);
        ComponentCondition<FocusedTremorSmall>(id + 0x10, 0.8f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<FocusedTremorSmall>();

        ComponentCondition<FlailingStrikeBait>(id + 0x20, 4.7f, comp => comp.CurrentBaits.Count > 0)
            .ActivateOnEnter<FlailingStrikeBait>();
        CastStart(id + 0x30, AID.FlailingStrikeFirst, 6.1f, "Cone bait")
            .DeactivateOnExit<FlailingStrikeBait>();
        CastEnd(id + 0x31, 3, "Cone 1")
            .ActivateOnEnter<FlailingStrike>()
            .ExecOnEnter<FocusedTremorSmall>(comp => comp.Activate());
        ComponentCondition<FocusedTremorSmall>(id + 0x40, 1.2f, comp => comp.NumCasts >= 1, "Tile 1");

        ComponentCondition<FlailingStrike>(id + 0x100, 7.5f, comp => comp.NumCasts >= 6, "Cone 6")
            .DeactivateOnExit<FlailingStrike>();
        ViciousSwipeCrazedRampage(id + 0x200, 0.8f);
        ComponentCondition<FocusedTremorSmall>(id + 0x300, 0.7f, comp => comp.NumCasts >= 16, "Tiles resolve")
            .DeactivateOnExit<FocusedTremorSmall>();
    }

    private void FocusedTremorCoerceForcefulStrike(uint id, float delay)
    {
        Cast(id, AID.Coerce, delay, 3)
            .ActivateOnEnter<Coerce>(); // face debuff appears ~0.7s after cast end
        SurgeOfVigor(id + 0x10, 3.2f);

        Cast(id + 0x100, AID.FocusedTremor, 9.2f, 3);
        ComponentCondition<FocusedTremorLarge>(id + 0x110, 0.8f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<FocusedTremorLarge>();

        CastStart(id + 0x120, AID.ForcefulStrike, 2.7f);
        ComponentCondition<FocusedTremorLarge>(id + 0x130, 5.2f, comp => comp.NumCasts >= 1, "Tile 1");
        ComponentCondition<Coerce>(id + 0x131, 1.6f, comp => comp.NumActiveForcedMarches > 0, "Forced march");
        ComponentCondition<FocusedTremorLarge>(id + 0x132, 0.4f, comp => comp.NumCasts >= 2, "Tile 2");
        ComponentCondition<FocusedTremorLarge>(id + 0x133, 2.0f, comp => comp.NumCasts >= 3, "Tile 3");
        ComponentCondition<FocusedTremorLarge>(id + 0x134, 2.0f, comp => comp.NumCasts >= 4, "Tile 4")
            .ActivateOnEnter<ForcefulStrike>()
            .DeactivateOnExit<FocusedTremorLarge>()
            .DeactivateOnExit<Coerce>();
        CastEnd(id + 0x140, 3.8f, "Cleave")
            .DeactivateOnExit<ForcefulStrike>();
    }

    private void SunsIre(uint id, float delay)
    {
        Cast(id, AID.SunsIre, delay, 12);
        SimpleState(id + 0x10, 0.7f, "Enrage");
    }
}
