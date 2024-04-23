namespace BossMod.Endwalker.Criterion.C01ASS.C011Silkie;

class C011SilkieStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C011SilkieStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<PuffTracker>();
    }

    private void SinglePhase(uint id)
    {
        FizzlingSuds(id, 8.2f);
        DustBluster(id + 0x10000, 5.2f);
        SqueakyCleanSlipperySoap(id + 0x20000, 2.3f);
        TotalWash(id + 0x30000, 6.2f);
        Tethers1(id + 0x40000, 2.1f);
        SudsSlipperySoap(id + 0x50000, 2.1f, true);
        TotalWash(id + 0x60000, 9.6f);
        EasternEwers(id + 0x70000, 2.1f);
        ChillingSudsCarpetBeaterSlipperySoap(id + 0x80000, 4.2f);
        BracingSudsSoapingSpree(id + 0x90000, 9.5f);
        DustBluster(id + 0xA0000, 6.2f);
        Tethers2(id + 0xB0000, 2.3f);
        TotalWash(id + 0xC0000, 2.1f);
        SudsSlipperySoap(id + 0xD0000, 6.2f);
        SudsSlipperySoap(id + 0xE0000, 2.5f);
        Cast(id + 0xF0000, AID.Enrage, 3.2f, 10, "Enrage");
    }

    private void CarpetBeater(uint id, float delay)
    {
        Cast(id, _savage ? AID.SCarpetBeater : AID.NCarpetBeater, delay, 5, "Tankbuster")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void TotalWash(uint id, float delay)
    {
        Cast(id, _savage ? AID.STotalWash : AID.NTotalWash, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private State SoapingSpree(uint id, float delay, string name = "Puffs")
    {
        return Cast(id, _savage ? AID.SSoapingSpreeBoss : AID.NSoapingSpreeBoss, delay, 5, name)
            .ActivateOnEnter<NChillingDusterPuff>(!_savage)
            .ActivateOnEnter<NBracingDusterPuff>(!_savage)
            .ActivateOnEnter<NFizzlingDusterPuff>(!_savage)
            .ActivateOnEnter<SChillingDusterPuff>(_savage)
            .ActivateOnEnter<SBracingDusterPuff>(_savage)
            .ActivateOnEnter<SFizzlingDusterPuff>(_savage)
            .DeactivateOnExit<ChillingDusterPuff>()
            .DeactivateOnExit<BracingDusterPuff>()
            .DeactivateOnExit<FizzlingDusterPuff>();
    }

    private void FizzlingSuds(uint id, float delay)
    {
        Cast(id, _savage ? AID.SFizzlingSuds : AID.NFizzlingSuds, delay, 3);
        Cast(id + 0x10, _savage ? AID.SSoapsUp : AID.NSoapsUp, 2.1f, 4)
            .ActivateOnEnter<NFizzlingDuster>(!_savage)
            .ActivateOnEnter<SFizzlingDuster>(_savage);
        ComponentCondition<FizzlingDuster>(id + 0x20, 1, comp => comp.NumCasts > 0, "Cones")
            .DeactivateOnExit<FizzlingDuster>();
    }

    private void DustBluster(uint id, float delay)
    {
        Cast(id, _savage ? AID.SDustBluster : AID.NDustBluster, delay, 5, "Knockback")
            .ActivateOnEnter<NDustBluster>(!_savage)
            .ActivateOnEnter<SDustBluster>(_savage)
            .DeactivateOnExit<DustBluster>();
    }

    private void SqueakyClean(uint id, float delay)
    {
        CastMulti(id, new AID[] { _savage ? AID.SSqueakyCleanE : AID.NSqueakyCleanE, _savage ? AID.SSqueakyCleanW : AID.NSqueakyCleanW }, delay, 4.5f)
            .ActivateOnEnter<NSqueakyCleanE>(!_savage)
            .ActivateOnEnter<NSqueakyCleanW>(!_savage)
            .ActivateOnEnter<SSqueakyCleanE>(_savage)
            .ActivateOnEnter<SSqueakyCleanW>(_savage);
        Condition(id + 2, 4.7f, () => Module.FindComponent<SqueakyCleanE>()!.NumCasts + Module.FindComponent<SqueakyCleanW>()!.NumCasts > 0, "Recolor")
            .DeactivateOnExit<SqueakyCleanE>()
            .DeactivateOnExit<SqueakyCleanW>();
    }

    private void SlipperySoap(uint id, float delay, bool longCharge = false)
    {
        CastStart(id, _savage ? AID.SSlipperySoap : AID.NSlipperySoap, delay)
            .ActivateOnEnter<SlipperySoapCharge>(); // target selection and cast start happen at the same time
        CastEnd(id + 1, 5);
        ComponentCondition<SlipperySoapCharge>(id + 2, longCharge ? 0.5f : 0.3f, comp => !comp.ChargeImminent, "Charge")
            .DeactivateOnExit<SlipperySoapCharge>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<SlipperySoapAOE>(id + 3, 3.6f, comp => !comp.Active, "AOE")
            .ActivateOnEnter<SlipperySoapAOE>()
            .ActivateOnEnter<SoapsudStatic>()
            .DeactivateOnExit<SlipperySoapAOE>()
            .DeactivateOnExit<SoapsudStatic>();
    }

    private void SudsSlipperySoap(uint id, float delay, bool longCharge = false)
    {
        CastMulti(id, new AID[] { _savage ? AID.SBracingSuds : AID.NBracingSuds, _savage ? AID.SChillingSuds : AID.NChillingSuds, _savage ? AID.SFizzlingSuds : AID.NFizzlingSuds }, delay, 3);
        SlipperySoap(id + 0x100, 2.1f, longCharge);
    }

    private void ChillingSudsCarpetBeaterSlipperySoap(uint id, float delay)
    {
        Cast(id, _savage ? AID.SChillingSuds : AID.NChillingSuds, delay, 3);
        CarpetBeater(id + 0x100, 2.2f);
        SlipperySoap(id + 0x200, 2.1f);
    }

    private void SqueakyCleanSlipperySoap(uint id, float delay)
    {
        Cast(id, _savage ? AID.SFreshPuff : AID.NFreshPuff, delay, 4);
        // +1.1s: debuffs on 3 puffs with offsets (0, -10), (12.5, 7), (-12.5, 7); no point showing anything for them, since they will be recolored
        Cast(id + 0x10, _savage ? AID.SBracingSuds : AID.NBracingSuds, 2.1f, 3);

        // TODO: as soon as next cast starts, we can determine desired position for bait: N if remaining puff is yellow, recolored side if remaining puff is blue
        SqueakyClean(id + 0x20, 2.1f);

        SudsSlipperySoap(id + 0x1000, 4.2f);
        // TODO: consider starting showing puff aoes now

        CarpetBeater(id + 0x2000, 2.6f);
        SoapingSpree(id + 0x3000, 4.1f);
    }

    private void EasternEwers(uint id, float delay)
    {
        Cast(id, _savage ? AID.SFreshPuff : AID.NFreshPuff, delay, 4);
        Cast(id + 0x10, _savage ? AID.SEasternEwers : AID.NEasternEwers, 2.1f, 4);
        ComponentCondition<EasternEwers>(id + 0x20, 1.2f, comp => comp.Active)
            .ActivateOnEnter<EasternEwers>();
        // TODO: consider showing puff aoes early, as soon as exaflares start...
        SoapingSpree(id + 0x1000, 10, "Exaflare + Puffs")
            .DeactivateOnExit<EasternEwers>();
    }

    // TODO: consider grouping with prev and showing early blue puff hints
    private void BracingSudsSoapingSpree(uint id, float delay)
    {
        Cast(id, _savage ? AID.SBracingSuds : AID.NBracingSuds, delay, 3);
        SoapingSpree(id + 0x100, 2.2f);
    }

    private void Tethers1(uint id, float delay)
    {
        CastMulti(id, new AID[] { _savage ? AID.SBracingSuds : AID.NBracingSuds, _savage ? AID.SChillingSuds : AID.NChillingSuds }, delay, 3)
            .ActivateOnEnter<PuffTethers1>();
        Cast(id + 0x10, _savage ? AID.SFreshPuff : AID.NFreshPuff, 2.1f, 4);
        // +1.1s: puffs appear
        // +3.2s: tethers appear
        // +9.4s/+10.1s: puff and tumble start
        // +10.9s/+11.7s: puff and tumble end

        SoapingSpree(id + 0x1000, 12.3f, "Tethers + Puffs")
            .DeactivateOnExit<PuffTethers1>();
    }

    private void Tethers2(uint id, float delay)
    {
        Cast(id, _savage ? AID.SBracingSuds : AID.NBracingSuds, delay, 3)
            .ActivateOnEnter<PuffTethers2>();
        Cast(id + 0x10, _savage ? AID.SFreshPuff : AID.NFreshPuff, 2.1f, 4);
        // +1.1s: puffs appear
        // +3.2s: tethers appear
        // +10.0s: puff and tumble start
        // +11.6s: puff and tumble end

        SqueakyClean(id + 0x20, 2.1f);
        SoapingSpree(id + 0x30, 4.1f, "Tethers + Puffs")
            .DeactivateOnExit<PuffTethers2>();
    }
}

class C011NSilkieStates(BossModule module) : C011SilkieStates(module, false);
class C011SSilkieStates(BossModule module) : C011SilkieStates(module, true);
