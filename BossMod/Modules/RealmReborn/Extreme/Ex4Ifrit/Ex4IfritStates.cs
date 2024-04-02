namespace BossMod.RealmReborn.Extreme.Ex4Ifrit;

// note: boss has multiple phases triggered by hp, but we represent them as states
// I don't really understand the trigger for phase changes
// for example, initial state (before first nails transition) typically (in MINE) is very short and has no incinrates; boss starts howl -> eruption -> burst sequence at ~10s
// if instead you don't hit him at all (e.g. in unsync), he will cast incinerates at 10/20/30 and only start this sequence at ~35s
// timings for nail phases seem to be extremely consistent, but they can end early depending on nail kill speed (at least starting from 2nd nails)
// TODO: biggest improvement to do would be to specify incinerate timings (these seem to be pretty consistent, at least with decent dps) and replace hardcoded cooldowns with planned
class Ex4IfritStates : StateMachineBuilder
{
    Ex4Ifrit _module;

    public Ex4IfritStates(Ex4Ifrit module) : base(module)
    {
        _module = module;
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<SearingWind>();
    }

    private void SinglePhase(uint id)
    {
        NailsSubphase<Ex4IfritAINails1, Ex4IfritAIHellfire1>(id, "4 nails at ~85%", false, false, 45);
        NailsSubphase<Ex4IfritAINails2, Ex4IfritAIHellfire2>(id + 0x10000, "8 nails at ~50%", true, true, 75);
        NailsSubphase<Ex4IfritAINails3, Ex4IfritAIHellfire3>(id + 0x20000, "13 nails at ~30%", true, false, 115);
        SimpleState(id + 0x30000, 1000, "Enrage")
            .ActivateOnEnter<Incinerate>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<InfernalFetters>()
            .ActivateOnEnter<Ex4IfritAINormal>()
            .DeactivateOnExit<Ex4IfritAINormal>();
    }

    private void NailsSubphase<AINails, AIHellfire>(uint id, string name, bool withFetters, bool startWithOT, float nailEnrage)
        where AINails : Ex4IfritAINails, new()
        where AIHellfire : Ex4IfritAIHellfire, new()
    {
        Condition(id, 1000, () => _module.SmallNails.Any(a => a.IsTargetable && !a.IsDead), name)
            .ActivateOnEnter<Incinerate>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<InfernalFetters>(withFetters)
            .ActivateOnEnter<Ex4IfritAINormal>()
            .ExecOnEnter<Ex4IfritAINormal>(comp => comp.BossTankRole = startWithOT ? PartyRolesConfig.Assignment.OT : PartyRolesConfig.Assignment.MT)
            .DeactivateOnExit<Incinerate>() // we want to reset cast counter
            .DeactivateOnExit<Ex4IfritAINormal>();
        Condition(id + 1, nailEnrage, () => !Module.PrimaryActor.IsTargetable, "Nails enrage", 1000) // note: not using invincibility as a condition here, since boss can become invincible early during nails if brought below 10%
            .ActivateOnEnter<Incinerate>()
            .ActivateOnEnter<AINails>()
            .DeactivateOnExit<Incinerate>()
            .DeactivateOnExit<Eruption>()
            .DeactivateOnExit<InfernalFetters>(withFetters)
            .DeactivateOnExit<AINails>()
            .SetHint(StateMachine.StateHint.DowntimeStart);

        CastStart(id + 0x10, AID.Hellfire, 2.7f)
            .ActivateOnEnter<Hellfire>()
            .ActivateOnEnter<AIHellfire>();
        CastEnd(id + 0x11, 2, "Raidwide")
            .DeactivateOnExit<Hellfire>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Condition(id + 0x20, 5.1f, () => Module.PrimaryActor.FindStatus(SID.Invincibility) == null, "Invincible end", 10)
            .DeactivateOnExit<AIHellfire>()
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        CastStart(id + 0x30, AID.RadiantPlume, 7.3f) // TODO: do we even need any AI here?..
            .ActivateOnEnter<Incinerate>()
            .DeactivateOnExit<Incinerate>(); // allow dodging plumes near mt
        CastEnd(id + 0x31, 2.2f)
            .ActivateOnEnter<RadiantPlume>();
        ComponentCondition<RadiantPlume>(id + 0x32, 0.9f, comp => comp.Casters.Count == 0, "Plumes")
            .DeactivateOnExit<RadiantPlume>();

        // note: cyclones could be skipped with high enough dps
        Condition(id + 0x40, 13.5f, () => !Module.PrimaryActor.IsTargetable || _module.SmallNails.Any(a => a.IsTargetable && !a.IsDead), "Disappear", 10)
            .ActivateOnEnter<Incinerate>()
            .ActivateOnEnter<Ex4IfritAINormal>()
            .DeactivateOnExit<Incinerate>()
            .DeactivateOnExit<Ex4IfritAINormal>();
        Targetable(id + 0x50, true, 15.4f, "Cyclones + Reappear")
            .ActivateOnEnter<CrimsonCyclone>()
            .DeactivateOnExit<CrimsonCyclone>();
    }
}
