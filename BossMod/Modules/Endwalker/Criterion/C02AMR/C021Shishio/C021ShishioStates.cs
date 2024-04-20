namespace BossMod.Endwalker.Criterion.C02AMR.C021Shishio;

class C021ShishioStates : StateMachineBuilder
{
    private readonly bool _savage;

    public C021ShishioStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Enkyo(id, 6.2f);
        StormcloudSummonsRokujoRevel(id + 0x10000, 4.5f);
        SplittingCrySlither(id + 0x20000, 3.4f); // note: delay could be 4.7 or 8.2, depending on breath-in count
        NoblePursuit(id + 0x30000, 7.6f);
        UnnaturalWailHauntingCry(id + 0x40000, 4.3f);
        StormcloudSummonsLightningBolt(id + 0x50000, 6.6f); // always 1 breath-in
        UnnaturalWailEyeVortex(id + 0x60000, 4.4f);
        Enkyo(id + 0x70000, 3.5f);
        HauntingCryAddsTowers(id + 0x80000, 5.2f);
        ThunderVortex(id + 0x90000, 2.1f);
        SplittingCrySlither(id + 0xA0000, 2.1f);
        StormcloudSummonsRokujoRevel(id + 0xB0000, 7.4f);
        StormcloudSummonsLightningBolt(id + 0xC0000, 4.7f); // note: delay could be 6.1 or 9.7, depending on breath-in count; always 2 or 3 breath-ins?
        UnnaturalWailEyeVortex(id + 0xD0000, 0.6f); // note: delay could be 2.9, depending on breath-in count
        Cast(id + 0xE0000, _savage ? AID.SEnrage : AID.NEnrage, 5.2f, 10, "Enrage");
    }

    private void Enkyo(uint id, float delay)
    {
        Cast(id, _savage ? AID.SEnkyo : AID.NEnkyo, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void StormcloudSummonsRokujoRevel(uint id, float delay)
    {
        Cast(id, _savage ? AID.SStormcloudSummons : AID.NStormcloudSummons, delay, 3);
        // +0.7s: envc 34/35 - circular arena?
        // +1.0s: spawn 18x raiun
        Cast(id + 0x10, _savage ? AID.SSmokeaterFirst : AID.NSmokeaterFirst, 2.2f, 2.5f)
            .ActivateOnEnter<RokujoRevel>();
        // +1.5s: first absorbs
        CastStart(id + 0x20, _savage ? AID.SRokujoRevelFirst : AID.NRokujoRevelFirst, 2.1f) // note: delay could be 4.2 or 6.3, depending on breath-in count
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastEnd(id + 0x21, 7.5f);
        // subsequent revel casts start with 2.5s delay
        ComponentCondition<RokujoRevel>(id + 0x30, 0.5f, comp => comp.NumCasts > 0, "Lines + circles start");
        ComponentCondition<RokujoRevel>(id + 0x40, 4.3f, comp => !comp.Active, "Lines + circles resolve", 3) // note: delay could be 5.0 or 5.8, depending on breath-in count
            .DeactivateOnExit<RokujoRevel>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void StormcloudSummonsLightningBolt(uint id, float delay)
    {
        Cast(id, _savage ? AID.SStormcloudSummons : AID.NStormcloudSummons, delay, 3);
        // +0.7s: envc 34/35 - circular arena?
        // +1.0s: spawn 18x raiun
        Cast(id + 0x10, _savage ? AID.SSmokeaterFirst : AID.NSmokeaterFirst, 2.2f, 2.5f);
        // +1.5s: first absorbs
        Cast(id + 0x20, _savage ? AID.SLightningBolt : AID.NLightningBolt, 2.1f, 3) // note: delay could be 4.2 or 6.3, depending on breath-in count
            .ActivateOnEnter<NLightningBolt>(!_savage)
            .ActivateOnEnter<SLightningBolt>(_savage);
        ComponentCondition<LightningBolt>(id + 0x30, 1.0f, comp => comp.NumCasts > 0, "Lines start")
            .DeactivateOnExit<LightningBolt>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<CloudToCloud>(id + 0x40, 0.9f, comp => comp.Active)
            .ActivateOnEnter<CloudToCloud>();
        ComponentCondition<CloudToCloud>(id + 0x50, 18.3f, comp => !comp.Active, "Lines resolve", 5) // note: delay could be 20.8 or 21.2, depending on breath-in count
            .DeactivateOnExit<CloudToCloud>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void SplittingCrySlither(uint id, float delay)
    {
        Cast(id, _savage ? AID.SSplittingCry : AID.NSplittingCry, delay, 5, "Tankbuster")
            .ActivateOnEnter<NSplittingCry>(!_savage)
            .ActivateOnEnter<SSplittingCry>(_savage)
            .ActivateOnEnter<Slither>()
            .DeactivateOnExit<SplittingCry>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        Cast(id + 0x10, _savage ? AID.SSlither : AID.NSlither, 2.2f, 2, "Back cleave")
            .DeactivateOnExit<Slither>();
    }

    private void NoblePursuit(uint id, float delay)
    {
        CastStart(id, _savage ? AID.SNoblePursuitFirst : AID.NNoblePursuitFirst, delay)
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastEnd(id + 1, 8, "Charge 1")
            .ActivateOnEnter<NoblePursuit>();
        ComponentCondition<NoblePursuit>(id + 0x10, 1.4f, comp => comp.NumCasts > 1, "Charge 2");
        ComponentCondition<NoblePursuit>(id + 0x20, 1.4f, comp => comp.NumCasts > 2, "Charge 3");
        ComponentCondition<NoblePursuit>(id + 0x30, 1.4f, comp => comp.NumCasts > 3, "Charge 4");
        ComponentCondition<NoblePursuit>(id + 0x40, 0.8f, comp => !comp.Active)
            .DeactivateOnExit<NoblePursuit>()
            .SetHint(StateMachine.StateHint.PositioningEnd);

        Enkyo(id + 0x1000, 0.7f);
    }

    private void UnnaturalWailHauntingCry(uint id, float delay)
    {
        Cast(id, _savage ? AID.SUnnaturalWail : AID.NUnnaturalWail, delay, 3)
            .ActivateOnEnter<UnnaturalWail>(); // debuffs are assigned 0.9s after cast ends
        Cast(id + 0x10, _savage ? AID.SHauntingCry : AID.NHauntingCry, 2.2f, 3);
        ComponentCondition<HauntingCrySwipes>(id + 0x20, 12, comp => comp.NumCasts > 0, "Swipes")
            .ActivateOnEnter<HauntingCrySwipes>();
        ComponentCondition<UnnaturalWail>(id + 0x21, 0.9f, comp => comp.NumMechanics > 0, "Spread/stack 1");
        ComponentCondition<HauntingCrySwipes>(id + 0x30, 6.1f, comp => comp.NumCasts > 4, "Swipes")
            .DeactivateOnExit<HauntingCrySwipes>();
        ComponentCondition<UnnaturalWail>(id + 0x31, 0.9f, comp => comp.NumMechanics > 1, "Stack/spread 2")
            .DeactivateOnExit<UnnaturalWail>();
    }

    private void UnnaturalWailEyeVortex(uint id, float delay)
    {
        Cast(id, _savage ? AID.SUnnaturalWail : AID.NUnnaturalWail, delay, 3)
            .ActivateOnEnter<UnnaturalWail>(); // debuffs are assigned 0.9s after cast ends
        CastMulti(id + 0x10, new[] { _savage ? AID.SEyeOfTheThunderVortexFirst : AID.NEyeOfTheThunderVortexFirst, _savage ? AID.SVortexOfTheThunderEyeFirst : AID.NVortexOfTheThunderEyeFirst }, 2.2f, 5.2f, "In/out")
            .ActivateOnEnter<EyeThunderVortex>();
        ComponentCondition<UnnaturalWail>(id + 0x20, 0.6f, comp => comp.NumMechanics > 0, "Spread/stack 1");
        ComponentCondition<EyeThunderVortex>(id + 0x30, 3.4f, comp => comp.NumCasts > 1, "Out/in")
            .DeactivateOnExit<EyeThunderVortex>();
        ComponentCondition<UnnaturalWail>(id + 0x31, 0.6f, comp => comp.NumMechanics > 1, "Stack/spread 2")
            .DeactivateOnExit<UnnaturalWail>();
    }

    private void HauntingCryAddsTowers(uint id, float delay)
    {
        Cast(id, _savage ? AID.SHauntingCry : AID.NHauntingCry, delay, 3)
            .ActivateOnEnter<HauntingCryReisho>();
        // +0.9s: tethers appear
        ComponentCondition<HauntingCryReisho>(id + 0x10, 6.0f, comp => comp.NumCasts > 0, "Ghost aoes start");
        CastStart(id + 0x20, _savage ? AID.SVengefulSouls : AID.NVengefulSouls, 1.5f);
        ComponentCondition<HauntingCryReisho>(id + 0x30, 0.5f, comp => comp.NumCasts > 1)
            .ActivateOnEnter<NHauntingCryVermilionAura>(!_savage)
            .ActivateOnEnter<NHauntingCryStygianAura>(!_savage)
            .ActivateOnEnter<SHauntingCryVermilionAura>(_savage)
            .ActivateOnEnter<SHauntingCryStygianAura>(_savage);
        ComponentCondition<HauntingCryReisho>(id + 0x31, 2.1f, comp => comp.NumCasts > 2);
        ComponentCondition<HauntingCryReisho>(id + 0x32, 2.1f, comp => comp.NumCasts > 3);
        ComponentCondition<HauntingCryReisho>(id + 0x33, 2.1f, comp => comp.NumCasts > 4);
        ComponentCondition<HauntingCryReisho>(id + 0x34, 2.1f, comp => comp.NumCasts > 5);
        ComponentCondition<HauntingCryReisho>(id + 0x35, 2.1f, comp => comp.NumCasts > 6);
        ComponentCondition<HauntingCryReisho>(id + 0x36, 2.1f, comp => comp.NumCasts > 7, "Ghost aoes end")
            .DeactivateOnExit<HauntingCryReisho>();
        CastEnd(id + 0x40, 2.1f, "Towers/defamations")
            .DeactivateOnExit<HauntingCryVermilionAura>()
            .DeactivateOnExit<HauntingCryStygianAura>();
    }

    private void ThunderVortex(uint id, float delay)
    {
        Cast(id, _savage ? AID.SThunderVortex : AID.NThunderVortex, delay, 5, "In")
            .ActivateOnEnter<NThunderVortex>(!_savage)
            .ActivateOnEnter<SThunderVortex>(_savage)
            .DeactivateOnExit<ThunderVortex>();
    }
}

class C021NShishioStates(BossModule module) : C021ShishioStates(module, false);
class C021SShishioStates(BossModule module) : C021ShishioStates(module, true);
