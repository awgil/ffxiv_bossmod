namespace BossMod.Dawntrail.Foray.ForkedTower.FT02DeadStars;

class FT02DeadStarsStates : StateMachineBuilder
{
    private readonly FT02DeadStars _module;

    public FT02DeadStarsStates(FT02DeadStars module) : base(module)
    {
        _module = module;
        SimplePhase(0, Phase1, "P1")
            .ActivateOnEnter<DecisiveBattle>()
            .ActivateOnEnter<DeathWall>();
    }

    private void Phase1(uint id)
    {
        ActorCast(id, _module.Triton, AID._Ability_DecisiveBattle2, 10.6f, 6, isBoss: true, "Debuffs + death wall appear")
            .ActivateOnEnter<DecisiveBattleAOE>();

        ActorCast(id + 0x100, _module.Triton, AID._Ability_SliceNDice1, 5.2f, 5, true, "Tankbusters")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .ActivateOnEnter<SliceNDice>()
            .DeactivateOnExit<SliceNDice>();

        WaterPhase(id + 0x10000, 7.7f);
        Intermission1(id + 0x20000, 4.5f);
        IcePhase(id + 0x30000, 7.4f);
        Intermission2(id + 0x40000, 4.4f);

        Timeout(id + 0xFF0000, 9999, "???");
    }

    private void WaterPhase(uint id, float delay)
    {
        ActorCastStart(id, _module.Phobos, AID._Ability_ThreeBodyProbl, delay, true)
            .ActivateOnEnter<NoisomeNuisance>();

        ComponentCondition<NoisomeNuisance>(id + 0x10, 7, n => n.NumCasts > 0, "AOEs")
            .DeactivateOnExit<NoisomeNuisance>();
        ActorTargetable(id + 0x20, _module.Nereid, false, 0.4f, "Bosses disappear");

        ActorCastStart(id + 0x100, _module.Phobos, AID._Spell_PrimordialChaos, 7, true)
            .ActivateOnEnter<PrimordialChaos>()
            .ActivateOnEnter<Ooze>();
        ComponentCondition<PrimordialChaos>(id + 0x110, 6.3f, p => p.NumCasts > 0, "Raidwide + debuffs")
            .DeactivateOnExit<PrimordialChaos>();

        ComponentCondition<Ooze>(id + 0x120, 15.5f, o => o.NumCasts > 0, "Puddles 1");
        ComponentCondition<Ooze>(id + 0x121, 5.6f, o => o.NumCasts > 2, "Puddles 2");
        ComponentCondition<Ooze>(id + 0x122, 5.6f, o => o.NumCasts > 4, "Puddles 3");
        ComponentCondition<Ooze>(id + 0x123, 5.6f, o => o.NumCasts > 6, "Puddles 4")
            .DeactivateOnExit<Ooze>();

        ActorCastStart(id + 0x200, _module.Phobos, AID._Spell_NoxiousNova, 4.2f, true)
            .ActivateOnEnter<NoxiousNova>();
        ComponentCondition<NoxiousNova>(id + 0x201, 5.8f, n => n.NumCasts > 0, "Raidwide + resolve")
            .DeactivateOnExit<NoxiousNova>();
    }

    private void VengefulCone(uint id, Func<Actor?> _actor, float delay)
    {
        ActorTargetable(id, _actor, true, delay, "Bosses reappear")
            .ActivateOnEnter<VengefulCone>()
            .ActivateOnEnter<VengefulConeHint>();

        ComponentCondition<VengefulConeHint>(id + 0x10, 2.1f, c => c.NumCasts > 0, "Bosses jump");
        ComponentCondition<VengefulCone>(id + 0x20, 7.9f, v => v.NumCasts > 0, "Safe triangle")
            .DeactivateOnExit<VengefulCone>()
            .DeactivateOnExit<VengefulConeHint>();
    }

    private void Intermission1(uint id, float delay)
    {
        VengefulCone(id, _module.Nereid, delay);

        ComponentCondition<DecisiveBattle>(id + 0x100, 4.5f, d => d.Active, "Debuffs reappear");
        DeltaAttack(id + 0x110, 7.6f);

        ComponentCondition<Firestrike>(id + 0x200, 9.8f, f => f.NumCasts > 0, "Party stacks")
            .ActivateOnEnter<Firestrike>()
            .DeactivateOnExit<Firestrike>();
    }

    private void IcePhase(uint id, float delay)
    {
        ActorCastStart(id, _module.Nereid, AID._Ability_ThreeBodyProbl, delay, true)
            .ActivateOnEnter<NoisomeNuisance>();

        ComponentCondition<NoisomeNuisance>(id + 0x10, 7, n => n.NumCasts > 0, "AOEs")
            .DeactivateOnExit<NoisomeNuisance>();
        ActorTargetable(id + 0x20, _module.Enemies(OID.FrozenTriton).FirstOrDefault, true, 0.5f, "Snowballs appear")
            .ActivateOnEnter<SnowballAdds>()
            .ActivateOnEnter<SnowBoulder>();

        ComponentCondition<SnowBoulder>(id + 0x100, 14, s => s.NumCasts > 0, "Charges 1");
        ComponentCondition<SnowBoulder>(id + 0x110, 2.6f, s => s.NumCasts > 2, "Charges 2");
        ComponentCondition<SnowBoulder>(id + 0x120, 2.6f, s => s.NumCasts > 4, "Charges 3")
            .DeactivateOnExit<SnowBoulder>();

        ActorCast(id + 0x200, _module.Nereid, AID._Spell_ChillingCollision1, 0, 5, true)
            .ActivateOnEnter<ChillingCollision>()
            .ActivateOnEnter<Avalaunch>();

        ComponentCondition<ChillingCollision>(id + 0x210, 1.2f, c => c.NumRealCasts > 0, "Knockback")
            .DeactivateOnExit<ChillingCollision>()
            .ExecOnExit<Avalaunch>(a => a.EnableHints = true);

        ComponentCondition<Avalaunch>(id + 0x220, 1.7f, a => a.NumFinishedStacks > 0, "Stacks")
            .ActivateOnEnter<SelfDestruct>()
            .DeactivateOnExit<Avalaunch>();

        ActorCastStart(id + 0x230, _module.Nereid, AID._Spell_ToTheWinds, 5.3f, true, "Enrage start");
        ActorCastEnd(id + 0x231, _module.Nereid, 13, true, "Enrage")
            .DeactivateOnExit<SnowballAdds>()
            .DeactivateOnExit<SelfDestruct>();
    }

    private void Intermission2(uint id, float delay)
    {
        VengefulCone(id, _module.Triton, delay);

        ComponentCondition<DecisiveBattle>(id + 0x100, 4.6f, d => d.Active, "Debuffs reappear");

        DeltaAttack(id + 0x110, 7.6f);
    }

    private void DeltaAttack(uint id, float delay)
    {
        ActorCastStart(id, _module.Triton, AID._Spell_DeltaAttack1, delay)
            .ActivateOnEnter<DeltaAttack>();
        ComponentCondition<DeltaAttack>(id + 1, 5.5f, d => d.NumCasts > 0, "Raidwide 1");
        ComponentCondition<DeltaAttack>(id + 2, 2.3f, d => d.NumCasts >= 6, "Raidwide 3")
            .DeactivateOnExit<DeltaAttack>();

        ComponentCondition<SliceNDice>(id + 0x100, 9.6f, f => f.NumCasts > 0, "Tankbusters")
            .ActivateOnEnter<SliceNDice>()
            .ActivateOnEnter<Firestrike2>();

        ComponentCondition<Firestrike2>(id + 0x110, 1.3f, f => f.NumCasts > 0, "Stacks");
    }
}
