namespace BossMod.Dawntrail.Savage.RM03SBruteBomber;

class RM03SBruteBomberStates : StateMachineBuilder
{
    public RM03SBruteBomberStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        BrutalImpact(id, 6.2f, 4);
        KnuckleSandwich(id + 0x10000, 1.1f, 4);
        Lariat(id + 0x20000, 6.8f, true, false);
        Diveboom(id + 0x30000, 2.9f, true, false);
        BrutalImpact(id + 0x40000, 3.2f, 4);
        BarbarousBarrage1(id + 0x50000, 9.2f);

        DopingDraughtLariat(id + 0x100000, 9.8f);
        Diveboom(id + 0x110000, 2.9f, false, true);
        BrutalImpact(id + 0x120000, 3.2f, 6);
        KnuckleSandwich(id + 0x130000, 1.1f, 6);
        TagTeam(id + 0x140000, 8.8f);
        Lariat(id + 0x150000, 1.2f, true, true);
        BrutalImpact(id + 0x160000, 2.4f, 6);
        FinalFusedown(id + 0x170000, 10.3f);
        Diveboom(id + 0x180000, 0.1f, true, true);
        Fusefield(id + 0x190000, 11.8f);
        KnuckleSandwich(id + 0x1A0000, 1.5f, 6);

        BombarianSpecial(id + 0x200000, 7.8f);
        FusesOfFury(id + 0x210000, 7.7f);
        Diveboom(id + 0x220000, 1.3f, true, true);
        BrutalImpact(id + 0x230000, 4.2f, 8);
        KnuckleSandwich(id + 0x240000, 1.1f, 8);
        FuseOrFoe(id + 0x250000, 7.8f);
        BrutalImpact(id + 0x260000, 2.4f, 8);
        BarbarousBarrage2(id + 0x270000, 8.2f);
        KnuckleSandwich(id + 0x280000, 10.2f, 8);

        SpecialBombarianSpecial(id + 0x300000, 11.1f);
    }

    private void BrutalImpact(uint id, float delay, int count)
    {
        Cast(id, AID.BrutalImpact, delay, 5, "Raidwide hit 1")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BrutalImpact>(id + 0x10, 1.1f * (count - 1) - 0.1f, comp => comp.NumCasts >= count - 1, $"Raidwide hit {count}")
            .ActivateOnEnter<BrutalImpact>()
            .DeactivateOnExit<BrutalImpact>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void KnuckleSandwich(uint id, float delay, int count)
    {
        Cast(id, AID.KnuckleSandwich, delay, 5, "Shared tankbuster hit 1")
            .ActivateOnEnter<KnuckleSandwich>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<KnuckleSandwich>(id + 0x10, 1.1f * (count - 1) - 0.1f, comp => comp.NumCasts >= count, $"Shared tankbuster hit {count}")
            .DeactivateOnExit<KnuckleSandwich>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void LariatResolve(uint id, float delay, bool allowOut, bool allowIn, bool activateComponents)
    {
        CastEnd(id, delay)
            .ActivateOnEnter<OctupleLariatOut>(activateComponents && allowOut)
            .ActivateOnEnter<OctupleLariatIn>(activateComponents && allowIn)
            .ActivateOnEnter<QuadrupleLariatOut>(activateComponents && allowOut)
            .ActivateOnEnter<QuadrupleLariatIn>(activateComponents && allowIn)
            .ActivateOnEnter<BlazingLariat>(activateComponents);
        ComponentCondition<BlazingLariat>(id + 1, 0.8f, comp => comp.NumCasts > 0, $"{(!allowIn ? "Out" : !allowOut ? "In" : "In/Out")} + Spread/Pairs")
            .DeactivateOnExit<OctupleLariatOut>(allowOut)
            .DeactivateOnExit<OctupleLariatIn>(allowIn)
            .DeactivateOnExit<QuadrupleLariatOut>(allowOut)
            .DeactivateOnExit<QuadrupleLariatIn>(allowIn)
            .DeactivateOnExit<BlazingLariat>();
    }

    private void Lariat(uint id, float delay, bool allowOut, bool allowIn)
    {
        CastStartMulti(id, [AID.OctupleLariatOut, AID.OctupleLariatIn, AID.QuadrupleLariatOut, AID.QuadrupleLariatIn], delay);
        LariatResolve(id + 1, 6.2f, allowOut, allowIn, true);
    }

    private void Diveboom(uint id, float delay, bool allowProximity, bool allowKnockback)
    {
        CastMulti(id, [AID.OctoboomDiveProximity, AID.OctoboomDiveKnockback, AID.QuadroboomDiveProximity, AID.QuadroboomDiveKnockback], delay, 7.2f)
            .ActivateOnEnter<OctoboomDiveProximity>(allowProximity)
            .ActivateOnEnter<OctoboomDiveKnockback>(allowKnockback)
            .ActivateOnEnter<QuadroboomDiveProximity>(allowProximity)
            .ActivateOnEnter<QuadroboomDiveKnockback>(allowKnockback)
            .ActivateOnEnter<Diveboom>();

        bool cond()
            => Module.FindComponent<OctoboomDiveProximity>()?.NumCasts > 0
            || Module.FindComponent<OctoboomDiveKnockback>()?.NumCasts > 0
            || Module.FindComponent<QuadroboomDiveProximity>()?.NumCasts > 0
            || Module.FindComponent<QuadroboomDiveKnockback>()?.NumCasts > 0;
        Condition(id + 2, 0.8f, cond, !allowKnockback ? "Proximity" : !allowProximity ? "Knockback" : "Proximity/Knockback")
            .DeactivateOnExit<OctoboomDiveProximity>(allowProximity)
            .DeactivateOnExit<OctoboomDiveKnockback>(allowKnockback)
            .DeactivateOnExit<QuadroboomDiveProximity>(allowProximity)
            .DeactivateOnExit<QuadroboomDiveKnockback>(allowKnockback);

        ComponentCondition<Diveboom>(id + 3, 3.3f, comp => !comp.Active, "Spread/Pairs")
            .DeactivateOnExit<Diveboom>();
    }

    private void DopingDraughtLariat(uint id, float delay)
    {
        Cast(id, AID.DopingDraught1, delay, 4);
        Lariat(id + 0x1000, 27.2f, false, true);
    }

    private void BarbarousBarrage1(uint id, float delay)
    {
        Cast(id, AID.BarbarousBarrage, delay, 4);
        ComponentCondition<BarbarousBarrageTowers>(id + 0x10, 1, comp => comp.CurState != BarbarousBarrageTowers.State.None)
            .ActivateOnEnter<BarbarousBarrageTowers>();
        ComponentCondition<BarbarousBarrageTowers>(id + 0x20, 10.1f, comp => comp.CurState >= BarbarousBarrageTowers.State.NextCorners, "Cardinal towers")
            .ActivateOnEnter<BarbarousBarrageKnockback>();
        CastStart(id + 0x30, AID.BarbarousBarrageMurderousMist, 2.2f);
        ComponentCondition<BarbarousBarrageTowers>(id + 0x31, 0.8f, comp => comp.CurState >= BarbarousBarrageTowers.State.NextCenter, "Corner towers")
            .ActivateOnEnter<BarbarousBarrageMurderousMist>();
        ComponentCondition<BarbarousBarrageTowers>(id + 0x40, 3, comp => comp.CurState >= BarbarousBarrageTowers.State.Done, "Center tower")
            .DeactivateOnExit<BarbarousBarrageKnockback>()
            .DeactivateOnExit<BarbarousBarrageTowers>();
        CastEnd(id + 0x50, 2.2f, "Cleave")
            .DeactivateOnExit<BarbarousBarrageMurderousMist>();
    }

    private void TagTeam(uint id, float delay)
    {
        Cast(id, AID.TagTeam, delay, 4);
        Cast(id + 0x10, AID.ChainDeathmatch, 2.2f, 7)
            .ActivateOnEnter<TagTeamLariatCombo>(); // tethers appear right before cast start
        ComponentCondition<TagTeamLariatCombo>(id + 0x20, 3.2f, comp => comp.AOEs.Count > 0);
        ComponentCondition<TagTeamLariatCombo>(id + 0x30, 6.1f, comp => comp.NumCasts > 0, "Chain cleaves 1");
        ComponentCondition<TagTeamLariatCombo>(id + 0x40, 4.3f, comp => comp.AOEs.Count == 0, "Chain cleaves 2")
            .DeactivateOnExit<TagTeamLariatCombo>();
    }

    private void FinalFusedown(uint id, float delay)
    {
        Cast(id, AID.FinalFusedown, delay, 4)
            .ActivateOnEnter<FinalFusedownSelfDestruct>()
            .ActivateOnEnter<FinalFusedownExplosion>()
            .ExecOnEnter<FinalFusedownExplosion>(comp => comp.Show());
        ComponentCondition<FinalFusedownSelfDestruct>(id + 0x10, 12.2f, comp => comp.NumCasts > 0, "Bombs + spreads 1");
        ComponentCondition<FinalFusedownSelfDestruct>(id + 0x20, 5, comp => comp.NumCasts > 4, "Bombs + spreads 2")
            .DeactivateOnExit<FinalFusedownSelfDestruct>()
            .DeactivateOnExit<FinalFusedownExplosion>();
    }

    private void Fusefield(uint id, float delay)
    {
        Cast(id, AID.Fusefield, delay, 4)
            .ActivateOnEnter<Fusefield>();
        Cast(id + 0x10, AID.BombarianFlame, 3.2f, 3)
            .ActivateOnEnter<FusefieldVoidzone>();
        ComponentCondition<FusefieldVoidzone>(id + 0x20, 3.9f, comp => comp.Active, "Fuses start");
        ComponentCondition<FusefieldVoidzone>(id + 0x30, 40, comp => !comp.Active, "Fuses resolve")
            .DeactivateOnExit<FusefieldVoidzone>()
            .DeactivateOnExit<Fusefield>();
    }

    private void BombarianSpecial(uint id, float delay)
    {
        Cast(id, AID.DopingDraught2, delay, 4);
        CastMulti(id + 0x10, [AID.OctoboomBombarianSpecial, AID.QuadroboomBombarianSpecial], 13.4f, 6)
            .ActivateOnEnter<BombarianSpecial>();
        ComponentCondition<BombarianSpecialRaidwide>(id + 0x20, 1, comp => comp.NumCasts >= 1)
            .ActivateOnEnter<BombarianSpecialRaidwide>()
            .ActivateOnEnter<BombarianSpecialOut>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BombarianSpecialRaidwide>(id + 0x21, 1.5f, comp => comp.NumCasts >= 2)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BombarianSpecialRaidwide>(id + 0x22, 2.0f, comp => comp.NumCasts >= 3)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BombarianSpecialRaidwide>(id + 0x23, 2.2f, comp => comp.NumCasts >= 4)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BombarianSpecialRaidwide>(id + 0x24, 1.3f, comp => comp.NumCasts >= 5)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BombarianSpecialOut>(id + 0x25, 1.8f, comp => comp.NumCasts > 0, "Out")
            .DeactivateOnExit<BombarianSpecialOut>();
        ComponentCondition<BombarianSpecialIn>(id + 0x26, 2.5f, comp => comp.NumCasts > 0, "In")
            .ActivateOnEnter<BombarianSpecialIn>()
            .DeactivateOnExit<BombarianSpecialIn>();
        ComponentCondition<BombarianSpecialRaidwide>(id + 0x27, 1.7f, comp => comp.NumCasts >= 6)
            .ActivateOnEnter<BombarianSpecialAOE>()
            .ActivateOnEnter<BombarianSpecialKnockback>()
            .DeactivateOnExit<BombarianSpecialRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BombarianSpecialKnockback>(id + 0x28, 6.5f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<BombarianSpecialAOE>()
            .DeactivateOnExit<BombarianSpecialKnockback>();
        ComponentCondition<BombarianSpecial>(id + 0x30, 3.6f, comp => comp.CurMechanic == default, "Spread/Pairs")
            .ExecOnEnter<BombarianSpecial>(comp => comp.Show(3.6f))
            .DeactivateOnExit<BombarianSpecial>();
    }

    private void FusesOfFury(uint id, float delay)
    {
        Cast(id, AID.FusesOfFury, delay, 4)
            .ActivateOnEnter<FinalFusedownSelfDestruct>();
        Cast(id + 0x10, AID.TagTeam, 2.1f, 4);
        Cast(id + 0x20, AID.ChainDeathmatch, 3.2f, 7)
           .ActivateOnEnter<TagTeamLariatCombo>(); // tethers appear right before cast start
        ComponentCondition<FinalFusedownSelfDestruct>(id + 0x30, 2.8f, comp => comp.NumCasts > 0, "Bombs 1");
        CastStart(id + 0x40, AID.FusesOfFuryMurderousMist, 4);
        ComponentCondition<FinalFusedownSelfDestruct>(id + 0x50, 1, comp => comp.NumCasts > 4, "Bombs 2")
            .ActivateOnEnter<FusesOfFuryMurderousMist>()
            .DeactivateOnExit<FinalFusedownSelfDestruct>();
        CastEnd(id + 0x60, 7, "Cone + chain cleaves 1")
            .DeactivateOnExit<FusesOfFuryMurderousMist>();
        ComponentCondition<TagTeamLariatCombo>(id + 0x70, 4.4f, comp => comp.AOEs.Count == 0, "Chain cleaves 2")
            .DeactivateOnExit<TagTeamLariatCombo>();
    }

    private void FuseOrFoe(uint id, float delay)
    {
        Cast(id, AID.FuseOrFoe, delay, 4)
            .ActivateOnEnter<FinalFusedownExplosion>();
        CastMulti(id + 0x10, [AID.InfernalSpinFirstCW, AID.InfernalSpinFirstCCW], 3.6f, 5.5f)
            .ActivateOnEnter<InfernalSpin>()
            .ActivateOnEnter<ExplosiveRain>();
        ComponentCondition<InfernalSpin>(id + 0x20, 0.5f, comp => comp.NumCasts > 0, "Rotation + circles start");
        ComponentCondition<ExplosiveRain>(id + 0x30, 4, comp => comp.NumCasts >= 4);
        ComponentCondition<ExplosiveRain>(id + 0x40, 3.6f, comp => comp.NumCasts >= 6)
            .DeactivateOnExit<InfernalSpin>(); // last spin happens ~5s before
        ComponentCondition<ExplosiveRain>(id + 0x41, 0.4f, comp => comp.NumCasts >= 8);
        ComponentCondition<FinalFusedownExplosion>(id + 0x50, 3.2f, comp => comp.NumCasts > 0, "Bombs 1")
            .ExecOnEnter<FinalFusedownExplosion>(comp => comp.Show());
        ComponentCondition<ExplosiveRain>(id + 0x51, 0.4f, comp => comp.NumCasts >= 10);

        CastStartMulti(id + 0x100, [AID.OctupleLariatOut, AID.OctupleLariatIn, AID.QuadrupleLariatOut, AID.QuadrupleLariatIn], 3.2f);
        ComponentCondition<ExplosiveRain>(id + 0x110, 0.8f, comp => comp.NumCasts >= 12)
            .ActivateOnEnter<OctupleLariatOut>()
            .ActivateOnEnter<OctupleLariatIn>()
            .ActivateOnEnter<QuadrupleLariatOut>()
            .ActivateOnEnter<QuadrupleLariatIn>()
            .ActivateOnEnter<BlazingLariat>()
            .DeactivateOnExit<ExplosiveRain>();
        ComponentCondition<FinalFusedownExplosion>(id + 0x120, 0.6f, comp => comp.Spreads.Count == 0, "Bombs 2")
            .DeactivateOnExit<FinalFusedownExplosion>();
        LariatResolve(id + 0x130, 4.8f, true, true, false);
    }

    private void BarbarousBarrage2(uint id, float delay)
    {
        Cast(id, AID.BarbarousBarrage, delay, 4);
        ComponentCondition<BarbarousBarrageTowers>(id + 0x10, 1, comp => comp.CurState != BarbarousBarrageTowers.State.None)
            .ActivateOnEnter<BarbarousBarrageTowers>();
        ComponentCondition<BarbarousBarrageTowers>(id + 0x20, 10.1f, comp => comp.CurState >= BarbarousBarrageTowers.State.NextCorners, "Cardinal towers")
            .ActivateOnEnter<BarbarousBarrageKnockback>();
        ComponentCondition<BarbarousBarrageTowers>(id + 0x30, 3, comp => comp.CurState >= BarbarousBarrageTowers.State.NextCenter, "Corner towers");
        CastStartMulti(id + 0x40, [AID.BarbarousBarrageLariatComboFirstRR, AID.BarbarousBarrageLariatComboFirstRL, AID.BarbarousBarrageLariatComboFirstLL, AID.BarbarousBarrageLariatComboFirstLR], 0.2f);
        ComponentCondition<BarbarousBarrageTowers>(id + 0x41, 2.8f, comp => comp.CurState >= BarbarousBarrageTowers.State.Done, "Center tower")
            .ActivateOnEnter<BarbarousBarrageLariatCombo>()
            .DeactivateOnExit<BarbarousBarrageKnockback>()
            .DeactivateOnExit<BarbarousBarrageTowers>();
        CastEnd(id + 0x42, 2.1f);
        ComponentCondition<BarbarousBarrageLariatCombo>(id + 0x43, 1.2f, comp => comp.NumCasts >= 1, "Charge 1");
        CastMulti(id + 0x50, [AID.BarbarousBarrageLariatComboSecondRR, AID.BarbarousBarrageLariatComboSecondRL, AID.BarbarousBarrageLariatComboSecondLL, AID.BarbarousBarrageLariatComboSecondLR], 1.3f, 3);
        ComponentCondition<BarbarousBarrageLariatCombo>(id + 0x52, 0.1f, comp => comp.NumCasts >= 2, "Charge 2")
            .DeactivateOnExit<BarbarousBarrageLariatCombo>();
    }

    private void SpecialBombarianSpecial(uint id, float delay)
    {
        Cast(id, AID.DopingDraught3, delay, 4);
        Cast(id + 0x10, AID.SpecialBombarianSpecial, 13.4f, 6);
        ComponentCondition<BombarianSpecialRaidwide>(id + 0x20, 1, comp => comp.NumCasts >= 1)
            .ActivateOnEnter<BombarianSpecialRaidwide>()
            .ActivateOnEnter<SpecialBombarianSpecialOut>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BombarianSpecialRaidwide>(id + 0x21, 1.5f, comp => comp.NumCasts >= 2)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BombarianSpecialRaidwide>(id + 0x22, 2.0f, comp => comp.NumCasts >= 3)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BombarianSpecialRaidwide>(id + 0x23, 2.2f, comp => comp.NumCasts >= 4)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<BombarianSpecialRaidwide>(id + 0x24, 1.3f, comp => comp.NumCasts >= 5)
            .DeactivateOnExit<BombarianSpecialRaidwide>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<SpecialBombarianSpecialOut>(id + 0x25, 1.8f, comp => comp.NumCasts > 0, "Out")
            .DeactivateOnExit<SpecialBombarianSpecialOut>();
        ComponentCondition<SpecialBombarianSpecialIn>(id + 0x26, 2.5f, comp => comp.NumCasts > 0, "In")
            .ActivateOnEnter<SpecialBombarianSpecialIn>()
            .DeactivateOnExit<SpecialBombarianSpecialIn>();
        Targetable(id + 0x30, false, 2.7f, "Enrage");
    }
}
