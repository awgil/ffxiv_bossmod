namespace BossMod.Dawntrail.Trial.T03Everkeep;

class T03EverkeepStates : StateMachineBuilder
{
    private readonly T03Everkeep _module;

    public T03EverkeepStates(T03Everkeep module) : base(module)
    {
        _module = module;
        SimplePhase(0, Phase1, "P1")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || (Module.PrimaryActor.CastInfo?.IsSpell(AID.SoulOverflowEnrage) ?? false);
        // P2 ends only when a BossP2 has been defeated (HP = 0). Missing/destroyed BossP2 is not
        // sufficient on its own — the actor is briefly destroyed and re-spawned when the arena
        // shrinks at the big ENVC block, and we must not unload the module during that gap.
        SimplePhase(1, Phase2, "Enrage")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && (_module.BossP2()?.IsDead ?? false);
    }

    private void Phase1(uint id)
    {
        SimpleState(id, 10000, "Enrage")
            .ActivateOnEnter<SoulOverflow>()
            .ActivateOnEnter<SoulOverflowEnrage>()
            .ActivateOnEnter<PatricidalPique>()
            .ActivateOnEnter<CalamitysEdge>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<VorpalTrail>()
            .ActivateOnEnter<DoubleEdgedSwords>();
    }

    private void Phase2(uint id)
    {
        SimpleState(id, 10000, "Enrage")
            .ActivateOnEnter<DawnOfAnAge>()
            .ActivateOnEnter<Actualize>()
            .ActivateOnEnter<ChasmOfVollok>()
            .ActivateOnEnter<DutysEdge>()
            .ActivateOnEnter<ForgedTrack>()
            .ActivateOnEnter<FireIII>()
            .ActivateOnEnter<HalfFull>()
            .ActivateOnEnter<BitterReaping>()
            .ActivateOnEnter<HalfCircuitRect>()
            .ActivateOnEnter<HalfCircuitDonut>()
            .ActivateOnEnter<HalfCircuitCircle>()
            .ActivateOnEnter<SmitingCircuitDonut>()
            .ActivateOnEnter<SmitingCircuitCircle>();
    }
}
