namespace BossMod.Dawntrail.Foray.ForkedTower.FT02DeadStars;

class FT02DeadStarsStates : StateMachineBuilder
{
    private readonly FT02DeadStars _module;

    private bool IsEffectivelyDead(Actor? actor) => actor != null && (actor.IsDeadOrDestroyed || actor.HPMP.CurHP == 1);

    public FT02DeadStarsStates(FT02DeadStars module) : base(module)
    {
        _module = module;
        SimplePhase(0, Phase1, "Water + ice phase")
            .ActivateOnEnter<DecisiveBattle>()
            .ActivateOnEnter<DeathWall>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed
                || IsEffectivelyDead(_module.IceTriton())
                && IsEffectivelyDead(_module.IcePhobos())
                && _module.Nereid()?.CastInfo == null;

        SimplePhase(1, Phase2, "Fire phase")
            .Raw.Update = () =>
                (_module.DeathWall?.IsDeadOrDestroyed ?? true)
                || IsEffectivelyDead(_module.FireNereid())
                && IsEffectivelyDead(_module.FirePhobos())
                && _module.Triton()?.CastInfo == null;

        SimplePhase(2, Phase3, "Enrage phase")
            .Raw.Update = () =>
                (_module.DeathWall?.IsDeadOrDestroyed ?? true)
                || IsEffectivelyDead(_module.Multiboss());
    }

    private void Phase1(uint id)
    {
        ActorCast(id, _module.Triton, AID.DecisiveBattle1, 10.6f, 6, isBoss: true, "Debuffs + death wall appear")
            .ActivateOnEnter<DecisiveBattleAOE>();

        ActorCast(id + 0x100, _module.Triton, AID.SliceNDiceCast, 5.2f, 5, true, "Tankbusters")
            .SetHint(StateMachine.StateHint.Tankbuster)
            .ActivateOnEnter<SliceNDice>()
            .DeactivateOnExit<SliceNDice>();

        WaterPhase(id + 0x10000, 7.7f);
        Intermission1(id + 0x20000, 4.5f);
        IcePhase(id + 0x30000, 7.4f);
    }

    private void Phase2(uint id)
    {
        Intermission2(id, 4.4f);
        FirePhase(id + 0x10000, 8.7f);
    }

    private void Phase3(uint id)
    {
        EnragePhase(id, 4.3f);
    }

    private void WaterPhase(uint id, float delay)
    {
        ActorCastStart(id, _module.Phobos, AID.ThreeBodyProbl, delay, true)
            .ActivateOnEnter<NoisomeNuisance>();

        ComponentCondition<NoisomeNuisance>(id + 0x10, 7, n => n.NumCasts > 0, "AOEs")
            .DeactivateOnExit<NoisomeNuisance>();
        ActorTargetable(id + 0x20, _module.Nereid, false, 0.4f, "Bosses disappear");

        ActorCastStart(id + 0x100, _module.Phobos, AID.PrimordialChaosCast, 7, true)
            .ActivateOnEnter<PrimordialChaos>()
            .ActivateOnEnter<Ooze>();
        ComponentCondition<PrimordialChaos>(id + 0x110, 6.3f, p => p.NumCasts > 0, "Raidwide + debuffs")
            .DeactivateOnExit<PrimordialChaos>();

        ComponentCondition<Ooze>(id + 0x120, 15.5f, o => o.NumCasts > 0, "Puddles 1");
        ComponentCondition<Ooze>(id + 0x121, 5.6f, o => o.NumCasts > 2, "Puddles 2");
        ComponentCondition<Ooze>(id + 0x122, 5.6f, o => o.NumCasts > 4, "Puddles 3");
        ComponentCondition<Ooze>(id + 0x123, 5.6f, o => o.NumCasts > 6, "Puddles 4")
            .DeactivateOnExit<Ooze>();

        ActorCastStart(id + 0x200, _module.Phobos, AID.NoxiousNovaCast, 4.2f, true)
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

    private void DeltaAttack(uint id, float delay)
    {
        ActorCastStart(id, _module.Triton, AID.DeltaAttackCast, delay)
            .ActivateOnEnter<DeltaAttack>();
        ComponentCondition<DeltaAttack>(id + 1, 5.5f, d => d.NumCasts > 0, "Raidwide 1");
        ComponentCondition<DeltaAttack>(id + 2, 2.3f, d => d.NumCasts >= 6, "Raidwide 3")
            .DeactivateOnExit<DeltaAttack>();
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
        ActorCastStart(id, _module.Nereid, AID.ThreeBodyProbl, delay, true)
            .ActivateOnEnter<NoisomeNuisance>();

        ComponentCondition<NoisomeNuisance>(id + 0x10, 7, n => n.NumCasts > 0, "AOEs")
            .DeactivateOnExit<NoisomeNuisance>();
        ActorTargetable(id + 0x20, _module.Enemies(OID.FrozenTriton).FirstOrDefault, true, 0.5f, "Snowballs appear")
            .ActivateOnEnter<SnowballAdds>()
            .ActivateOnEnter<IceboundBuffoonery>()
            .ActivateOnEnter<SnowBoulder>();

        ComponentCondition<SnowBoulder>(id + 0x100, 14, s => s.NumCasts > 0, "Charges 1");
        ComponentCondition<SnowBoulder>(id + 0x110, 2.6f, s => s.NumCasts > 2, "Charges 2");
        ComponentCondition<SnowBoulder>(id + 0x120, 2.6f, s => s.NumCasts > 4, "Charges 3")
            .DeactivateOnExit<SnowBoulder>();

        ActorCast(id + 0x200, _module.Nereid, AID.ChillingCollisionCast, 0, 5, true)
            .ActivateOnEnter<ChillingCollision>()
            .ActivateOnEnter<Avalaunch>();

        ComponentCondition<ChillingCollision>(id + 0x210, 1.2f, c => c.NumRealCasts > 0, "Knockback")
            .DeactivateOnExit<ChillingCollision>()
            .ExecOnExit<Avalaunch>(a => a.EnableHints = true);

        ComponentCondition<Avalaunch>(id + 0x220, 1.7f, a => a.NumFinishedStacks > 0, "Stacks")
            .ActivateOnEnter<SelfDestruct>()
            .DeactivateOnExit<Avalaunch>();

        ActorCastStart(id + 0x230, _module.Nereid, AID.ToTheWinds1, 5.3f, true, "Snowball enrage start");
        ActorCastEnd(id + 0x231, _module.Nereid, 13, true, "Snowballs enrage")
            .DeactivateOnExit<SnowballAdds>()
            .DeactivateOnExit<IceboundBuffoonery>()
            .DeactivateOnExit<SelfDestruct>();
    }

    private void Intermission2(uint id, float delay)
    {
        VengefulCone(id, _module.Triton, delay);

        ComponentCondition<DecisiveBattle>(id + 0x100, 4.6f, d => d.Active, "Debuffs reappear");

        DeltaAttack(id + 0x110, 7.6f);

        ComponentCondition<SliceNDice>(id + 0x120, 9.6f, f => f.NumCasts > 0, "Tankbusters")
            .ActivateOnEnter<SliceNDice>()
            .ActivateOnEnter<Firestrike2>();

        ComponentCondition<Firestrike2>(id + 0x121, 1.3f, f => f.NumCasts > 0, "Stacks")
            .DeactivateOnExit<SliceNDice>()
            .DeactivateOnExit<Firestrike2>();
    }

    private void FirePhase(uint id, float delay)
    {
        ActorCastStart(id, _module.Triton, AID.ThreeBodyProbl, delay, true)
            .ActivateOnEnter<NoisomeNuisance>()
            .ActivateOnEnter<FireballAdds>();

        ComponentCondition<NoisomeNuisance>(id + 0x10, 7, n => n.NumCasts > 0, "AOEs")
            .DeactivateOnExit<NoisomeNuisance>();
        ActorTargetable(id + 0x20, _module.Phobos, false, 0.4f, "Bosses disappear");

        ComponentCondition<FireballTowerHint>(id + 0x100, 4.9f, t => t.FireballCount == 2, "Arrows appear")
            .ActivateOnEnter<FireballTowerHint>();

        FireSpread(id + 0x200, 9.4f);

        ComponentCondition<GeothermalRupture>(id + 0x210, 4.6f, g => g.Casters.Count > 0, "Baits start")
            .ActivateOnEnter<GeothermalRupture>()
            .ActivateOnEnter<FlameThrower>();

        ComponentCondition<FlameThrower>(id + 0x211, 7.1f, f => f.NumCasts > 0, "Line stacks")
            .DeactivateOnExit<FlameThrower>()
            .DeactivateOnExit<GeothermalRupture>();

        FireSpread(id + 0x300, 5.5f)
            .DeactivateOnExit<FireballTowerHint>();

        ActorCastStart(id + 0x230, _module.Triton, AID.ToTheWinds2, 4.8f, true, "Fireball enrage start")
            .ActivateOnEnter<FireDestruct>();
        ActorCastEnd(id + 0x231, _module.Triton, 7, true, "Fireballs enrage")
            .DeactivateOnExit<FireDestruct>();
    }

    private State FireSpread(uint id, float delay)
    {
        ComponentCondition<FireSpread>(id, delay, f => f.NumCasts == 4, "Stacks + tower 1")
            .ActivateOnEnter<FireSpread>()
            .ActivateOnEnter<ElementalImpact>();

        ComponentCondition<FireSpread>(id + 1, 4.1f, f => f.NumCasts == 8, "Stacks + tower 2");
        return ComponentCondition<FireSpread>(id + 2, 4.1f, f => f.NumCasts == 12, "Stacks + tower 3")
            .DeactivateOnExit<FireSpread>()
            .DeactivateOnExit<ElementalImpact>();
    }

    private void EnragePhase(uint id, float delay)
    {
        VengefulCone(id, _module.Nereid, delay);

        ComponentCondition<SixHandedFistfight>(id + 0x100, 24.3f, s => s.NumCasts > 0, "Voidzone appear")
            .ActivateOnEnter<SixHandedFistfight>()
            .ActivateOnEnter<SixHandedRaidwide>()
            .ActivateOnEnter<Multiboss>();

        ComponentCondition<CollateralJet>(id + 0x200, 12.1f, c => c.NumCasts == 3, "Cones 1")
            .ActivateOnEnter<CollateralJet>();
        ComponentCondition<CollateralJet>(id + 0x201, 2, c => c.NumCasts == 6, "Cones 2")
            .DeactivateOnExit<CollateralJet>();

        ComponentCondition<CollateralBall>(id + 0x210, 5.2f, b => b.NumCasts > 0, "Spreads start")
            .ActivateOnEnter<CollateralBall>();
        ComponentCondition<CollateralBall>(id + 0x220, 5.1f, b => b.Spreads.Count == 0, "Spreads finish");

        ComponentCondition<CollateralJet>(id + 0x300, 10.1f, c => c.NumCasts == 3, "Cones 1")
            .ActivateOnEnter<CollateralJet>();
        ComponentCondition<CollateralJet>(id + 0x301, 2, c => c.NumCasts == 6, "Cones 2")
            .DeactivateOnExit<CollateralJet>();

        ComponentCondition<CollateralBall>(id + 0x310, 5.2f, b => b.NumCasts > 0, "Spreads start")
            .ActivateOnEnter<CollateralBall>();
        ComponentCondition<CollateralBall>(id + 0x320, 5.1f, b => b.Spreads.Count == 0, "Spreads finish");
    }
}
