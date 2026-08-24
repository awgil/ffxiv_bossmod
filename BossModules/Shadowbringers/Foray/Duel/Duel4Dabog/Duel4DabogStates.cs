namespace BossMod.Shadowbringers.Foray.Duel.Duel4Dabog;

class Duel4DabogStates : StateMachineBuilder
{
    public Duel4DabogStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        LSlash(id, 10.2f);
        RCometLCutter(id + 0x10000, 6.4f);
        LLCRRayLCutter(id + 0x20000, 14.3f);
        RLCRCometLCutter(id + 0x30000, 5.1f);

        CellDivision(id + 0x100000, 14.3f);
        LWaveRBlaster(id + 0x110000, 1.5f);
        LLCArmUnit(id + 0x120000, 8.2f);
        LSlash(id + 0x130000, 9.1f);
        RLCRRay(id + 0x140000, 9.2f);
        RLCArmUnit(id + 0x150000, 1.3f);
        LSlash(id + 0x160000, 9.1f);

        CellDivision(id + 0x200000, 9.5f);
        LWaveRBlaster(id + 0x210000, 1.5f);
        RLCArmUnit(id + 0x220000, 8.2f);
        LSlash(id + 0x230000, 9.1f); // not sure about timings below
        RLCRRay(id + 0x240000, 9.2f);
        LLCArmUnit(id + 0x250000, 1.3f);
        LSlash(id + 0x260000, 9.1f);

        Cast(id + 0x300000, AID.Enrage, 4.4f, 10, "Enrage");
    }

    private void LSlash(uint id, float delay)
    {
        Cast(id, AID.LeftArmSlash, delay, 3, "Cleave")
            .ActivateOnEnter<LeftArmSlash>()
            .DeactivateOnExit<LeftArmSlash>();
    }

    private void RCometLCutter(uint id, float delay)
    {
        Cast(id, AID.RightArmComet, delay, 5);

        CastStart(id + 0x100, AID.LeftArmMetalCutter, 6.1f)
            .ActivateOnEnter<RightArmCometShort>(); // tower appears ~0.8s after previous cast end
        ComponentCondition<RightArmComet>(id + 0x110, 2.2f, comp => comp.NumCasts > 0, "Tower knockback short")
            .ActivateOnEnter<LeftArmMetalCutterAOE>() // casts start together with visual cast start
            .DeactivateOnExit<RightArmComet>();
        CastEnd(id + 0x120, 2.8f)
            .ActivateOnEnter<LeftArmMetalCutterKnockbackShort>();
        ComponentCondition<LeftArmMetalCutterKnockback>(id + 0x130, 0.6f, comp => comp.NumCasts > 0, "Boss knockback short")
            .DeactivateOnExit<LeftArmMetalCutterKnockback>();

        ComponentCondition<LeftArmMetalCutterAOE>(id + 0x200, 0.4f, comp => comp.CurState == LeftArmMetalCutterAOE.State.SecondAOEs, "Cones 1");
        ComponentCondition<LeftArmMetalCutterAOE>(id + 0x300, 5.1f, comp => comp.CurState == LeftArmMetalCutterAOE.State.Done, "Cones 2")
            .DeactivateOnExit<LeftArmMetalCutterAOE>();
    }

    private void LLCRRayLCutter(uint id, float delay)
    {
        Cast(id, AID.LeftArmLimitCut, delay, 4);
        Cast(id + 0x10, AID.RightArmRayNormal, 3.3f, 4)
            .ActivateOnEnter<RightArmRayNormal>(); // aoes start ~0.8s after cast end
        Cast(id + 0x20, AID.LeftArmMetalCutter, 3.5f, 5)
            .ActivateOnEnter<LeftArmMetalCutterAOE>()
            .ActivateOnEnter<LeftArmMetalCutterKnockbackLong>();
        ComponentCondition<LeftArmMetalCutterKnockback>(id + 0x30, 0.6f, comp => comp.NumCasts > 0, "Boss knockback long")
            .DeactivateOnExit<LeftArmMetalCutterKnockback>();
        ComponentCondition<LeftArmMetalCutterAOE>(id + 0x40, 0.4f, comp => comp.CurState == LeftArmMetalCutterAOE.State.SecondAOEs, "Cones 1");
        ComponentCondition<RightArmRayNormal>(id + 0x50, 2.3f, comp => comp.NumCasts > 0, "Circles")
            .DeactivateOnExit<RightArmRayNormal>();
        ComponentCondition<LeftArmMetalCutterAOE>(id + 0x60, 2.8f, comp => comp.CurState == LeftArmMetalCutterAOE.State.Done, "Cones 2")
            .DeactivateOnExit<LeftArmMetalCutterAOE>();
    }

    private void RLCRCometLCutter(uint id, float delay)
    {
        Cast(id, AID.RightArmLimitCut, delay, 4);
        Cast(id + 0x10, AID.RightArmComet, 3.3f, 5);

        CastStart(id + 0x100, AID.LeftArmMetalCutter, 5.9f)
            .ActivateOnEnter<RightArmCometLong>(); // tower appears ~0.8s after previous cast end
        ComponentCondition<RightArmComet>(id + 0x110, 2.4f, comp => comp.NumCasts > 0, "Tower knockback long")
            .ActivateOnEnter<LeftArmMetalCutterAOE>() // casts start together with visual cast start
            .DeactivateOnExit<RightArmComet>();
        CastEnd(id + 0x120, 2.6f)
            .ActivateOnEnter<LeftArmMetalCutterKnockbackShort>();
        ComponentCondition<LeftArmMetalCutterKnockback>(id + 0x130, 0.6f, comp => comp.NumCasts > 0, "Boss knockback short")
            .DeactivateOnExit<LeftArmMetalCutterKnockback>();

        ComponentCondition<LeftArmMetalCutterAOE>(id + 0x200, 0.4f, comp => comp.CurState == LeftArmMetalCutterAOE.State.SecondAOEs, "Cones 1");
        ComponentCondition<LeftArmMetalCutterAOE>(id + 0x300, 5.1f, comp => comp.CurState == LeftArmMetalCutterAOE.State.Done, "Cones 2")
            .DeactivateOnExit<LeftArmMetalCutterAOE>();
    }

    private void CellDivision(uint id, float delay)
    {
        Cast(id, AID.CellDivision, delay, 4); // first pair starts cast 9.2s after this cast end, second pair starts 21.3s after this cast end
        // +1.0s: 2 fragments spawn
        // +4.2s: 2 more fragments spawn

        ComponentCondition<RightArmBlasterFragment>(id + 0x10, 9.2f, comp => comp.Casters.Count > 0, "Lines 1 bait")
            .ActivateOnEnter<RightArmBlasterFragment>();
        CastStart(id + 0x11, AID.LeftArmMetalCutter, 3.2f);
        ComponentCondition<RightArmBlasterFragment>(id + 0x12, 0.8f, comp => comp.Casters.Count == 0, "Lines 1 resolve")
            .ActivateOnEnter<LeftArmMetalCutterAOE>()
            .ActivateOnEnter<LeftArmMetalCutterKnockbackShort>()
            .DeactivateOnExit<RightArmBlasterFragment>();
        CastEnd(id + 0x13, 4.2f);
        ComponentCondition<LeftArmMetalCutterKnockback>(id + 0x20, 0.6f, comp => comp.NumCasts > 0, "Boss knockback short")
            .DeactivateOnExit<LeftArmMetalCutterKnockback>();
        ComponentCondition<LeftArmMetalCutterAOE>(id + 0x30, 0.4f, comp => comp.CurState == LeftArmMetalCutterAOE.State.SecondAOEs, "Cones 1");

        ComponentCondition<RightArmBlasterFragment>(id + 0x40, 2.7f, comp => comp.Casters.Count > 0, "Lines 2 bait")
            .ActivateOnEnter<RightArmBlasterFragment>();
        ComponentCondition<LeftArmMetalCutterAOE>(id + 0x50, 2.4f, comp => comp.CurState == LeftArmMetalCutterAOE.State.Done, "Cones 2")
            .DeactivateOnExit<LeftArmMetalCutterAOE>();
        ComponentCondition<RightArmBlasterFragment>(id + 0x60, 1.6f, comp => comp.Casters.Count == 0, "Lines 2 resolve")
            .DeactivateOnExit<RightArmBlasterFragment>();
    }

    private void LWaveRBlaster(uint id, float delay)
    {
        Cast(id, AID.LeftArmWave, delay, 5, "Large aoe bait");
        ComponentCondition<LeftArmWave>(id + 0x10, 3.1f, comp => comp.NumCasts > 0, "Large aoe resolve")
            .ActivateOnEnter<LeftArmWave>()
            .DeactivateOnExit<LeftArmWave>();

        Cast(id + 0x100, AID.RightArmBlasterBoss, 0.2f, 4, "Cleave")
            .ActivateOnEnter<RightArmBlasterBoss>()
            .DeactivateOnExit<RightArmBlasterBoss>();
    }

    private void LLCArmUnit(uint id, float delay)
    {
        Cast(id, AID.LeftArmLimitCut, delay, 4);
        Cast(id + 0x10, AID.ArmUnit, 3.5f, 5)
            .ActivateOnEnter<LeftArmMetalCutterAOE>()
            .ActivateOnEnter<LeftArmMetalCutterKnockbackLong>()
            .ActivateOnEnter<RightArmCometShort>();
        ComponentCondition<LeftArmMetalCutterKnockback>(id + 0x20, 0.6f, comp => comp.NumCasts > 0, "Boss knockback long")
            .DeactivateOnExit<LeftArmMetalCutterKnockback>();
        ComponentCondition<LeftArmMetalCutterAOE>(id + 0x30, 0.4f, comp => comp.CurState == LeftArmMetalCutterAOE.State.SecondAOEs, "Cones 1");
        ComponentCondition<RightArmComet>(id + 0x40, 1.5f, comp => comp.NumCasts > 0, "Tower knockback short")
            .DeactivateOnExit<RightArmComet>();
        ComponentCondition<LeftArmMetalCutterAOE>(id + 0x50, 2.6f, comp => comp.CurState == LeftArmMetalCutterAOE.State.Done, "Cones 2")
            .DeactivateOnExit<LeftArmMetalCutterAOE>();
    }

    private void RLCArmUnit(uint id, float delay)
    {
        Cast(id, AID.RightArmLimitCut, delay, 4);
        Cast(id + 0x10, AID.ArmUnit, 4.6f, 5)
            .ActivateOnEnter<LeftArmMetalCutterAOE>()
            .ActivateOnEnter<LeftArmMetalCutterKnockbackShort>()
            .ActivateOnEnter<RightArmCometLong>();
        ComponentCondition<LeftArmMetalCutterKnockback>(id + 0x20, 0.6f, comp => comp.NumCasts > 0, "Boss knockback short")
            .DeactivateOnExit<LeftArmMetalCutterKnockback>();
        ComponentCondition<LeftArmMetalCutterAOE>(id + 0x30, 0.4f, comp => comp.CurState == LeftArmMetalCutterAOE.State.SecondAOEs, "Cones 1");
        ComponentCondition<RightArmComet>(id + 0x40, 1.5f, comp => comp.NumCasts > 0, "Tower knockback long")
            .DeactivateOnExit<RightArmComet>();
        ComponentCondition<LeftArmMetalCutterAOE>(id + 0x50, 2.6f, comp => comp.CurState == LeftArmMetalCutterAOE.State.Done, "Cones 2")
            .DeactivateOnExit<LeftArmMetalCutterAOE>();
    }

    private void RLCRRay(uint id, float delay)
    {
        Cast(id, AID.RightArmLimitCut, delay, 4);
        Cast(id + 0x10, AID.RightArmRayBuffed, 3.2f, 4);

        ComponentCondition<RightArmRayBuffed>(id + 0x20, 5.8f, comp => comp.Active)
            .ActivateOnEnter<RightArmRayBuffed>();
        ComponentCondition<RightArmRayVoidzone>(id + 0x30, 8.8f, comp => comp.NumCasts > 0, "Rotating aoes + voidzones start")
            .ActivateOnEnter<RightArmRayVoidzone>();

        ComponentCondition<RightArmRayBuffed>(id + 0x40, 17.3f, comp => !comp.Active, "Rotating aoes end")
            .DeactivateOnExit<RightArmRayBuffed>()
            .DeactivateOnExit<RightArmRayVoidzone>(); // TODO: voidzones remain active for some time, overlapping next mechanic
    }
}
