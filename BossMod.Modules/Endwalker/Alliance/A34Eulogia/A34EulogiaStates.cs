namespace BossMod.Endwalker.Alliance.A34Eulogia;

class A34EulogiaStates : StateMachineBuilder
{
    public A34EulogiaStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    public void SinglePhase(uint id)
    {
        DawnOfTime(id, 9.5f);
        Quintessence(id + 0x10000, 10);

        Quintessence(id + 0x20000, 18.3f);
        Sunbeam(id + 0x30000, 7.2f);
        Whorl(id + 0x40000, 8.6f);
        LovesLight(id + 0x50000, 3.2f);
        SolarFans(id + 0x60000, 0.1f);
        Hydrostasis(id + 0x70000, 2.2f);
        DestructiveBolt(id + 0x80000, 2.3f);
        HieroglyphikaHandOfTheDestroyer(id + 0x90000, 5.5f);
        MatronsBreath(id + 0xA0000, 5.8f);
        TorrentialTridents(id + 0xB0000, 1.5f);
        DestructiveBolt(id + 0xC0000, 0.6f);
        ByregotsStrike(id + 0xD0000, 3.4f);
        ThousandfoldThrust(id + 0xE0000, 1.2f);
        AsAboveSoBelow(id + 0xF0000, 5.8f);
        EudaimonEorzea(id + 0x100000, 6.9f);

        Quintessence(id + 0x110000, 17.7f);
        Sunbeam(id + 0x120000, 7.2f);
        Whorl(id + 0x130000, 10.7f);
        // following mechanics order is either fully random, or has multiple possible forks...
        //Hydrostasis(id + 0x140000, 3.2f);
        //SolarFans(id + 0x150000, 2.5f);
        //ByregotsStrike(id + 0x160000, 2.6f);
        //ThousandfoldThrust(id + 0x170000, 1.2f);
        //DestructiveBolt(id + 0x180000, 2.5f);
        //LovesLight(id + 0x190000, 5.4f);
        SimpleState(id + 0xFF0000, 100, "Mechanics in random order")
            .ActivateOnEnter<LovesLight>()
            .ActivateOnEnter<SolarFans>()
            .ActivateOnEnter<RadiantRhythm>()
            .ActivateOnEnter<RadiantFlourish>()
            .ActivateOnEnter<Hydrostasis>()
            .ActivateOnEnter<DestructiveBolt>()
            .ActivateOnEnter<Hieroglyphika>()
            .ActivateOnEnter<HandOfTheDestroyerWrath>()
            .ActivateOnEnter<HandOfTheDestroyerJudgment>()
            .ActivateOnEnter<MatronsBreath>()
            .ActivateOnEnter<TorrentialTrident>()
            .ActivateOnEnter<ByregotStrikeJump>()
            .ActivateOnEnter<ByregotStrikeKnockback>()
            .ActivateOnEnter<ByregotStrikeCone>()
            .ActivateOnEnter<ThousandfoldThrust>()
            .ActivateOnEnter<AsAboveSoBelow>()
            .ActivateOnEnter<ClimbingShot>()
            .ActivateOnEnter<SoaringMinuet>();
    }

    private void DawnOfTime(uint id, float delay)
    {
        Cast(id, AID.DawnOfTime, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Quintessence(uint id, float delay)
    {
        CastStartMulti(id, [AID.FirstFormRight, AID.FirstFormLeft, AID.FirstFormDonut], delay)
            .ActivateOnEnter<Quintessence>();
        CastEnd(id + 1, 7);
        CastMulti(id + 0x10, [AID.SecondFormRight, AID.SecondFormLeft, AID.SecondFormDonut], 0.2f, 7);
        CastMulti(id + 0x20, [AID.ThirdFormRight, AID.ThirdFormLeft, AID.ThirdFormDonut], 0.2f, 7);
        Cast(id + 0x30, AID.Quintessence, 0.2f, 4);
        ComponentCondition<Quintessence>(id + 0x40, 0.8f, comp => comp.NumCasts > 0, "Form 1");
        ComponentCondition<Quintessence>(id + 0x50, 3.5f, comp => comp.NumCasts > 1, "Form 2");
        ComponentCondition<Quintessence>(id + 0x60, 3.6f, comp => comp.NumCasts > 2, "Form 3")
            .DeactivateOnExit<Quintessence>();
    }

    private void Sunbeam(uint id, float delay)
    {
        Cast(id, AID.Sunbeam, delay, 5, "Tankbusters")
            .ActivateOnEnter<Sunbeam>()
            .DeactivateOnExit<Sunbeam>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Whorl(uint id, float delay)
    {
        Cast(id, AID.Whorl, delay, 7, "Raidwide") // note: deathwall appears at the end of the cast
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void LovesLight(uint id, float delay)
    {
        Cast(id, AID.LovesLight, delay, 4);
        Cast(id + 0x10, AID.FullBright, 5.1f, 3);
        ComponentCondition<LovesLight>(id + 0x20, 0.9f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<LovesLight>();
        ComponentCondition<LovesLight>(id + 0x30, 10.3f, comp => comp.NumCasts >= 1, "Line 1");
        ComponentCondition<LovesLight>(id + 0x31, 2, comp => comp.NumCasts >= 2, "Line 2");
        ComponentCondition<LovesLight>(id + 0x32, 2, comp => comp.NumCasts >= 3, "Line 3");
        ComponentCondition<LovesLight>(id + 0x33, 2, comp => comp.NumCasts >= 4, "Line 4")
            .DeactivateOnExit<LovesLight>();
    }

    private void SolarFans(uint id, float delay)
    {
        Cast(id, AID.SolarFans, delay, 3)
            .ActivateOnEnter<SolarFans>()
            .ActivateOnEnter<RadiantRhythm>()
            .ActivateOnEnter<RadiantFlourish>();
        ComponentCondition<SolarFans>(id + 2, 1, comp => comp.NumCasts > 0, "Charge")
            .DeactivateOnExit<SolarFans>();
        ComponentCondition<RadiantRhythm>(id + 0x10, 2.8f, comp => comp.NumCasts > 0);
        ComponentCondition<RadiantRhythm>(id + 0x20, 2.1f, comp => comp.NumCasts > 2);
        ComponentCondition<RadiantRhythm>(id + 0x30, 2.1f, comp => comp.NumCasts > 4);
        ComponentCondition<RadiantRhythm>(id + 0x40, 2.1f, comp => comp.NumCasts > 6)
            .DeactivateOnExit<RadiantRhythm>();
        Cast(id + 0x50, AID.RadiantFinish, 1.5f, 3, "Solar fans resolve")
            .DeactivateOnExit<RadiantFlourish>();
    }

    private void Hydrostasis(uint id, float delay)
    {
        Cast(id, AID.Hydrostasis, delay, 4);
        Cast(id + 0x10, AID.TimeAndTide, 2.1f, 6)
            .ActivateOnEnter<Hydrostasis>();
        ComponentCondition<Hydrostasis>(id + 0x20, 2.9f, comp => comp.NumCasts > 0, "Knockback 1");
        ComponentCondition<Hydrostasis>(id + 0x21, 3, comp => comp.NumCasts > 1, "Knockback 2");
        ComponentCondition<Hydrostasis>(id + 0x22, 3, comp => comp.NumCasts > 2, "Knockback 3")
            .DeactivateOnExit<Hydrostasis>();
    }

    private void DestructiveBolt(uint id, float delay)
    {
        Cast(id, AID.DestructiveBolt, delay, 6)
            .ActivateOnEnter<DestructiveBolt>();
        ComponentCondition<DestructiveBolt>(id + 2, 1.1f, comp => comp.NumFinishedStacks > 0, "Stack")
            .DeactivateOnExit<DestructiveBolt>();
    }

    private void HieroglyphikaHandOfTheDestroyer(uint id, float delay)
    {
        Cast(id, AID.Hieroglyphika, delay, 5)
            .OnEnter(() => Module.Arena.Bounds = A34Eulogia.HieroglyphikaBounds);
        ComponentCondition<Hieroglyphika>(id + 0x10, 1, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<Hieroglyphika>();
        ComponentCondition<Hieroglyphika>(id + 0x20, 12.8f, comp => comp.BindsAssigned, "Binds");
        CastStartMulti(id + 0x30, [AID.HandOfTheDestroyerWrath, AID.HandOfTheDestroyerJudgment], 0.5f);
        ComponentCondition<Hieroglyphika>(id + 0x31, 2.8f, comp => comp.NumCasts > 0, "Squares")
            .ActivateOnEnter<HandOfTheDestroyerWrath>()
            .ActivateOnEnter<HandOfTheDestroyerJudgment>()
            .DeactivateOnExit<Hieroglyphika>()
            .OnExit(() => Module.Arena.Bounds = A34Eulogia.DefaultBounds);
        CastEnd(id + 0x32, 4.7f);
        Condition(id + 0x33, 0.5f, () => Module.FindComponent<HandOfTheDestroyerWrath>()?.NumCasts > 0 || Module.FindComponent<HandOfTheDestroyerJudgment>()?.NumCasts > 0, "Half-arena cleave")
            .DeactivateOnExit<HandOfTheDestroyerWrath>()
            .DeactivateOnExit<HandOfTheDestroyerJudgment>();
    }

    private void MatronsBreath(uint id, float delay)
    {
        Cast(id, AID.MatronsBreath, delay, 3)
            .ActivateOnEnter<MatronsBreath>();
        ComponentCondition<MatronsBreath>(id + 0x10, 15.1f, comp => comp.NumCasts >= 1, "Flower 1");
        ComponentCondition<MatronsBreath>(id + 0x11, 3.5f, comp => comp.NumCasts >= 2, "Flower 2");
        ComponentCondition<MatronsBreath>(id + 0x12, 3.5f, comp => comp.NumCasts >= 3, "Flower 3");
        ComponentCondition<MatronsBreath>(id + 0x13, 3.5f, comp => comp.NumCasts >= 4, "Flower 4")
            .DeactivateOnExit<MatronsBreath>();
    }

    private void TorrentialTridents(uint id, float delay)
    {
        Cast(id, AID.TorrentialTridents, delay, 2);
        ComponentCondition<TorrentialTrident>(id + 0x10, 0.9f, comp => comp.AOEs.Count > 0)
            .ActivateOnEnter<TorrentialTrident>();
        ComponentCondition<TorrentialTrident>(id + 0x20, 5.5f, comp => comp.AOEs.Count > 5, "Raidwide x6");
        ComponentCondition<TorrentialTrident>(id + 0x30, 8.1f, comp => comp.NumCasts > 0, "Explosions start");
        ComponentCondition<TorrentialTrident>(id + 0x40, 5, comp => comp.NumCasts > 5, "Explosions end")
            .DeactivateOnExit<TorrentialTrident>();
    }

    private void ByregotsStrike(uint id, float delay)
    {
        Cast(id, AID.ByregotStrike, delay, 6, "Jump")
            .ActivateOnEnter<ByregotStrikeJump>()
            .ActivateOnEnter<ByregotStrikeKnockback>()
            .ActivateOnEnter<ByregotStrikeCone>()
            .DeactivateOnExit<ByregotStrikeJump>();
        ComponentCondition<ByregotStrikeKnockback>(id + 2, 0.7f, comp => comp.NumCasts > 0, "Knockback + cones")
            .DeactivateOnExit<ByregotStrikeKnockback>()
            .DeactivateOnExit<ByregotStrikeCone>();
    }

    private void ThousandfoldThrust(uint id, float delay)
    {
        CastMulti(id, [AID.ThousandfoldThrustR, AID.ThousandfoldThrustL], delay, 5)
            .ActivateOnEnter<ThousandfoldThrust>();
        ComponentCondition<ThousandfoldThrust>(id + 0x10, 1.3f, comp => comp.NumCasts > 0, "Half-room cleave start");
        ComponentCondition<ThousandfoldThrust>(id + 0x20, 4.3f, comp => comp.NumCasts > 4, "Half-room cleave resolve")
            .DeactivateOnExit<ThousandfoldThrust>();
    }

    private void AsAboveSoBelow(uint id, float delay)
    {
        CastMulti(id, [AID.AsAboveSoBelowNald, AID.AsAboveSoBelowThal], delay, 5);
        CastMulti(id + 0x10, [AID.ClimbingShotNald, AID.ClimbingShotThal], 4.1f, 8)
            .ActivateOnEnter<AsAboveSoBelow>()
            .ActivateOnEnter<ClimbingShot>();
        ComponentCondition<ClimbingShot>(id + 0x20, 0.2f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<ClimbingShot>();
        ComponentCondition<AsAboveSoBelow>(id + 0x30, 0.8f, comp => comp.NumCasts > 0, "Exaflare start");
        Cast(id + 0x40, AID.SoaringMinuet, 2.3f, 7, "Wide cleave")
            .ActivateOnEnter<SoaringMinuet>()
            .DeactivateOnExit<SoaringMinuet>()
            .DeactivateOnExit<AsAboveSoBelow>();
    }

    private void EudaimonEorzea(uint id, float delay)
    {
        Cast(id, AID.EudaimonEorzea, delay, 22.2f);
        ComponentCondition<EudaimonEorzea>(id + 0x10, 2.7f, comp => comp.NumCasts > 0, "Raidwide x13") // note: deathwall disappears at the end of the cast
            .ActivateOnEnter<EudaimonEorzea>()
            .DeactivateOnExit<EudaimonEorzea>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}
