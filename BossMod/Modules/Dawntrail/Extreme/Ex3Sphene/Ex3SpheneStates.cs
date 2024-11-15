namespace BossMod.Dawntrail.Extreme.Ex3Sphene;

class Ex3SpheneStates : StateMachineBuilder
{
    private readonly Ex3Sphene _module;

    public Ex3SpheneStates(Ex3Sphene module) : base(module)
    {
        _module = module;
        SimplePhase(0, Phase1, "P1")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || (Module.PrimaryActor.CastInfo?.IsSpell(AID.AuthorityEternal) ?? false);
        SimplePhase(1, Phase2, "P2")
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && (_module.BossP2()?.IsDeadOrDestroyed ?? true);
    }

    private void Phase1(uint id)
    {
        P1Aethertithe(id, 12.2f);
        P1VirtualShiftWind(id + 0x10000, 7.5f);
        P1DivideAndConquer(id + 0x20000, 3.1f);
        P1RoyalDomain(id + 0x30000, 3.0f);
        P1VirtualShiftEarth(id + 0x40000, 8.2f);
        P1ProsecutionOfWar(id + 0x50000, 7.2f);
        P1Coronation(id + 0x60000, 3.1f);
        P1AbsoluteAuthority(id + 0x70000, 6.2f);
        P1VirtualShiftIce(id + 0x80000, 12.3f);
        // TODO: prosecution > royal domain > legitimate force > royal domain > enrage
        SimpleState(id + 0xFF0000, 10000, "???");
    }

    private void Phase2(uint id)
    {
        P2Intermission(id, 0);
        P2RadicalShift(id + 0x10000, 4.1f);
        P2DimensionalDistortion(id + 0x20000, 7.2f);
        P2DyingMemory(id + 0x30000, 1.3f);
        P2RadicalShift(id + 0x40000, 11.4f);
        // TODO: enrage
        SimpleState(id + 0xFF0000, 10000, "???");
    }

    private void P1Aethertithe(uint id, float delay)
    {
        Cast(id, AID.Aethertithe, delay, 3);
        ComponentCondition<Aethertithe>(id + 0x10, 5, comp => comp.AOE != null)
            .ActivateOnEnter<Aethertithe>();
        ComponentCondition<Aethertithe>(id + 0x11, 5.1f, comp => comp.NumCasts >= 1, "Cone 1")
            .ActivateOnEnter<Retribute>();
        ComponentCondition<Retribute>(id + 0x12, 2.9f, comp => comp.NumCasts > 0, "Line stacks 1");
        ComponentCondition<Aethertithe>(id + 0x20, 1.2f, comp => comp.AOE != null);
        ComponentCondition<Aethertithe>(id + 0x21, 5.1f, comp => comp.NumCasts >= 2, "Cone 2");
        ComponentCondition<Retribute>(id + 0x22, 2.9f, comp => comp.NumCasts > 2, "Line stacks 2");
        ComponentCondition<Aethertithe>(id + 0x30, 1.2f, comp => comp.AOE != null);
        ComponentCondition<Aethertithe>(id + 0x31, 5.1f, comp => comp.NumCasts >= 3, "Cone 3")
            .DeactivateOnExit<Aethertithe>();
        ComponentCondition<Retribute>(id + 0x32, 2.9f, comp => comp.NumCasts > 4, "Line stacks 3")
            .DeactivateOnExit<Retribute>();
    }

    private void P1ProsecutionOfWar(uint id, float delay)
    {
        Cast(id, AID.ProsecutionOfWar, delay, 5, "Tankbuster 1")
            .ActivateOnEnter<ProsecutionOfWar>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<ProsecutionOfWar>(id + 2, 3.2f, comp => comp.NumCasts > 1, "Tankbuster 2")
            .DeactivateOnExit<ProsecutionOfWar>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P1DivideAndConquer(uint id, float delay)
    {
        Cast(id, AID.DivideAndConquer, delay, 7.5f)
            .ActivateOnEnter<DivideAndConquerBait>();
        ComponentCondition<DivideAndConquerBait>(id + 0x10, 0.1f, comp => comp.NumCasts > 0, "Protean 1");
        ComponentCondition<DivideAndConquerBait>(id + 0x20, 7, comp => comp.NumCasts >= 8, "Protean 8")
            .DeactivateOnExit<DivideAndConquerBait>();
        ComponentCondition<DivideAndConquerAOE>(id + 0x30, 1.1f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<DivideAndConquerAOE>();
        ComponentCondition<DivideAndConquerAOE>(id + 0x31, 3, comp => comp.NumCasts > 0, "Lines")
            .DeactivateOnExit<DivideAndConquerAOE>();
    }

    private void P1RoyalDomain(uint id, float delay)
    {
        Cast(id, AID.RoyalDomain, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P1Coronation(uint id, float delay)
    {
        Cast(id, AID.Coronation, delay, 3);
        ComponentCondition<Coronation>(id + 0x10, 2.1f, comp => comp.Groups.Count > 0)
            .ActivateOnEnter<Coronation>();
        Cast(id + 0x20, AID.AtomicRay, 1.1f, 3);
        ComponentCondition<AtomicRay>(id + 0x30, 1.2f, comp => comp.Active)
            .ActivateOnEnter<AtomicRay>();
        ComponentCondition<Coronation>(id + 0x40, 5, comp => comp.NumCasts > 0, "Coronation")
            .DeactivateOnExit<Coronation>();
        ComponentCondition<AtomicRay>(id + 0x41, 1, comp => comp.NumFinishedSpreads > 0, "Spread")
            .DeactivateOnExit<AtomicRay>();
    }

    private void P1AbsoluteAuthority(uint id, float delay)
    {
        Cast(id, AID.AbsoluteAuthorityPuddles, delay, 10);
        ComponentCondition<AbsoluteAuthorityPuddles>(id + 0x10, 0.1f, comp => comp.Casters.Count > 0, "Puddles bait")
            .ActivateOnEnter<AbsoluteAuthorityPuddles>();
        ComponentCondition<AbsoluteAuthorityExpansionBoot>(id + 0x20, 10, comp => comp.NumCasts > 0, "Spread/stack")
            .ActivateOnEnter<AbsoluteAuthorityExpansionBoot>()
            .DeactivateOnExit<AbsoluteAuthorityPuddles>() // last puddle resolves right before stack/spread
            .DeactivateOnExit<AbsoluteAuthorityExpansionBoot>();
        ComponentCondition<AbsoluteAuthorityHeel>(id + 0x30, 4, comp => comp.NumCasts > 0, "Stack")
            .ActivateOnEnter<AbsoluteAuthorityHeel>()
            .DeactivateOnExit<AbsoluteAuthorityHeel>();
        ComponentCondition<AbsoluteAuthorityKnockback>(id + 0x40, 6.9f, comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<AbsoluteAuthorityKnockback>()
            .DeactivateOnExit<AbsoluteAuthorityKnockback>();
    }

    private void P1VirtualShiftWind(uint id, float delay)
    {
        Cast(id, AID.VirtualShiftWind, delay, 5, "Raidwide (wind platform)")
            .OnExit(() => _module.Arena.Bounds = Ex3Sphene.WindBounds)
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x10, AID.LawsOfWind, 5.2f, 4);
        ComponentCondition<Aeroquell>(id + 0x20, 0.1f, comp => comp.Active)
            .ActivateOnEnter<Aeroquell>();
        ComponentCondition<Aeroquell>(id + 0x21, 5, comp => !comp.Active, "Party stacks")
            .DeactivateOnExit<Aeroquell>();

        CastStartMulti(id + 0x100, [AID.LegitimateForceFirstR, AID.LegitimateForceFirstL], 5.1f)
            .ActivateOnEnter<AeroquellTwister>(); // voidzones appear ~0.6s after stacks
        ComponentCondition<MissingLink>(id + 0x101, 0.8f, comp => comp.TethersAssigned, "Chains")
            .ActivateOnEnter<LegitimateForce>()
            .ActivateOnEnter<MissingLink>();
        CastEnd(id + 0x102, 7.2f, "Side 1");
        ComponentCondition<LegitimateForce>(id + 0x103, 3.1f, comp => comp.NumCasts > 1, "Side 2")
            .DeactivateOnExit<LegitimateForce>();

        ComponentCondition<WindOfChange>(id + 0x200, 3.2f, comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<WindOfChange>()
            .DeactivateOnExit<WindOfChange>()
            .DeactivateOnExit<MissingLink>();

        Cast(id + 0x300, AID.WorldShatterP1, 3, 5, "Raidwide + platform end")
            .OnExit(() => _module.Arena.Bounds = Ex3Sphene.NormalBounds)
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<AeroquellTwister>(id + 0x310, 2.6f, comp => !comp.Sources(Module).Any())
            .DeactivateOnExit<AeroquellTwister>();

        P1ProsecutionOfWar(id + 0x1000, 4.5f);
    }

    private void P1VirtualShiftEarth(uint id, float delay)
    {
        Cast(id, AID.VirtualShiftEarth, delay, 5, "Raidwide (earth platform)")
            .SetHint(StateMachine.StateHint.Raidwide);
        CastStart(id + 0x10, AID.LawsOfEarth, 5.2f)
            .ActivateOnEnter<VirtualShiftEarth>();
        CastEnd(id + 0x11, 4);

        CastMulti(id + 0x100, [AID.LegitimateForceFirstR, AID.LegitimateForceFirstL], 3.2f, 8, "Side 1")
            .ActivateOnEnter<LegitimateForce>();
        ComponentCondition<LegitimateForce>(id + 0x110, 3.1f, comp => comp.NumCasts > 1, "Side 2")
            .DeactivateOnExit<LegitimateForce>();
        ComponentCondition<LawsOfEarthBurst>(id + 0x120, 5, comp => comp.NumCasts > 0, "Towers")
            .ActivateOnEnter<LawsOfEarthBurst1>()
            .DeactivateOnExit<LawsOfEarthBurst>();

        Cast(id + 0x200, AID.GravitationalEmpire, 5.2f, 7)
            .ActivateOnEnter<GravityPillar>()
            .ActivateOnEnter<GravityRay>()
            .ActivateOnEnter<LawsOfEarthBurst2>();
        ComponentCondition<GravityRay>(id + 0x210, 1, comp => comp.NumCasts > 0, "Defamations + Cones")
            .DeactivateOnExit<GravityPillar>() // resolves right before cones
            .DeactivateOnExit<GravityRay>();
        ComponentCondition<LawsOfEarthBurst>(id + 0x220, 0.8f, comp => comp.NumCasts > 0, "Towers")
            .DeactivateOnExit<LawsOfEarthBurst>();

        ComponentCondition<MeteorImpact>(id + 0x300, 5.5f, comp => comp.Active)
            .ActivateOnEnter<MeteorImpact>();
        ComponentCondition<MeteorImpact>(id + 0x301, 6.1f, comp => comp.NumCasts > 0, "Meteors 1")
            .ActivateOnEnter<WeightyBlow>();
        ComponentCondition<MeteorImpact>(id + 0x310, 1, comp => comp.Active);
        ComponentCondition<MeteorImpact>(id + 0x311, 6.1f, comp => comp.NumCasts > 0, "Meteors 2")
            .DeactivateOnExit<MeteorImpact>();

        Cast(id + 0x400, AID.WeightyBlow, 2, 5);
        ComponentCondition<WeightyBlow>(id + 0x410, 0.1f, comp => comp.NumCasts > 0, "LOS 1");
        ComponentCondition<WeightyBlow>(id + 0x411, 3.1f, comp => comp.NumCasts > 2, "LOS 2");
        ComponentCondition<WeightyBlow>(id + 0x412, 3.1f, comp => comp.NumCasts > 4, "LOS 3");
        ComponentCondition<WeightyBlow>(id + 0x413, 3.1f, comp => comp.NumCasts > 6, "LOS 4")
            .DeactivateOnExit<WeightyBlow>();

        Cast(id + 0x500, AID.WorldShatterP1, 0.7f, 5, "Raidwide + platform end")
            .DeactivateOnExit<VirtualShiftEarth>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P1VirtualShiftIce(uint id, float delay)
    {
        Cast(id, AID.VirtualShiftIce, delay, 5, "Raidwide (ice platform)")
            .OnExit(() => _module.Arena.Bounds = Ex3Sphene.IceBounds)
            .SetHint(StateMachine.StateHint.Raidwide);
        CastStart(id + 0x10, AID.LawsOfIce, 5.2f);
        CastEnd(id + 0x11, 4)
            .ActivateOnEnter<LawsOfIce>();
        ComponentCondition<LawsOfIce>(id + 0x12, 1, comp => comp.NumCasts > 0, "Move")
            .OnExit(() => _module.Arena.Bounds = Ex3Sphene.IceBridgeBounds);

        ComponentCondition<Rush>(id + 0x100, 4.3f, comp => comp.Activation != default)
            .ActivateOnEnter<VirtualShiftIce>()
            .ActivateOnEnter<Rush>()
            .DeactivateOnExit<LawsOfIce>();
        CastStartMulti(id + 0x101, [AID.LegitimateForceFirstR, AID.LegitimateForceFirstL], 11.9f);
        ComponentCondition<Rush>(id + 0x102, 0.3f, comp => comp.NumCasts > 0, "Stretch tethers")
            .DeactivateOnExit<Rush>();
        CastEnd(id + 0x103, 7.7f, "Side 1")
            .ActivateOnEnter<LegitimateForce>();
        ComponentCondition<LegitimateForce>(id + 0x104, 3.1f, comp => comp.NumCasts > 1, "Side 2")
            .DeactivateOnExit<LegitimateForce>();

        Cast(id + 0x200, AID.LawsOfIce, 6.1f, 4)
            .ActivateOnEnter<LawsOfIce>();
        ComponentCondition<LawsOfIce>(id + 0x202, 1, comp => comp.NumCasts > 0, "Move")
            .ActivateOnEnter<IceDart>()
            .ActivateOnEnter<RaisedTribute>();
        ComponentCondition<IceDart>(id + 0x210, 6.1f, comp => comp.NumCasts > 0, "Tethers + Line stack 1")
            .DeactivateOnExit<LawsOfIce>();
        ComponentCondition<IceDart>(id + 0x220, 7.1f, comp => comp.NumCasts > 2, "Tethers + Line stack 2");
        ComponentCondition<IceDart>(id + 0x230, 7.1f, comp => comp.NumCasts > 4, "Tethers + Line stack 3");
        ComponentCondition<IceDart>(id + 0x240, 7.1f, comp => comp.NumCasts > 6, "Tethers + Line stack 4")
            .DeactivateOnExit<IceDart>()
            .DeactivateOnExit<RaisedTribute>();

        Cast(id + 0x300, AID.WorldShatterP1, 3.1f, 5, "Raidwide + platform end")
            .DeactivateOnExit<VirtualShiftIce>()
            .OnExit(() => _module.Arena.Bounds = Ex3Sphene.NormalBounds)
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P2Intermission(uint id, float delay)
    {
        Cast(id, AID.AuthorityEternal, delay, 10);
        Targetable(id + 0x10, false, 0.2f, "Boss disappears + Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorTargetable(id + 0x20, _module.BossP2, true, 24.8f, "Boss appears")
            .SetHint(StateMachine.StateHint.DowntimeEnd);
    }

    private void P2RadicalShift(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.RadicalShift, delay, 11, true, "Raidwide (platform change)")
            .ActivateOnEnter<RadicalShift>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<RadicalShiftAOE>(id + 0x10, 5.2f, comp => comp.NumFinishedSpreads > 0, "Spread")
            .ActivateOnEnter<RadicalShiftAOE>()
            .DeactivateOnExit<RadicalShiftAOE>();

        ActorCast(id + 0x100, _module.BossP2, AID.RadicalShift, 3, 11, true, "Raidwide (platform change)")
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<RadicalShiftAOE>(id + 0x110, 5.2f, comp => comp.NumFinishedSpreads > 0, "Spread")
            .ActivateOnEnter<RadicalShiftAOE>()
            .DeactivateOnExit<RadicalShiftAOE>();

        ActorCast(id + 0x200, _module.BossP2, AID.WorldShatterP2, 3, 5, true, "Raidwide + platform end")
            .OnExit(() => _module.Arena.Bounds = Ex3Sphene.NormalBounds)
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void P2DimensionalDistortion(uint id, float delay)
    {
        ActorCast(id, _module.BossP2, AID.DimensionalDistortion, delay, 4, true)
            .ActivateOnEnter<DimensionalDistortion>();
        ComponentCondition<DimensionalDistortion>(id + 0x10, 1, comp => comp.NumCasts > 0, "Exaflares start");

        ActorCast(id + 0x100, _module.BossP2, AID.TyrannysGrasp, 5.2f, 5, true, "Front half cleave")
            .ActivateOnEnter<TyrannysGraspAOE>()
            .ActivateOnEnter<TyrannysGraspTowers>()
            .DeactivateOnExit<DimensionalDistortion>()
            .DeactivateOnExit<TyrannysGraspAOE>();
        ComponentCondition<TyrannysGraspTowers>(id + 0x110, 1.2f, comp => comp.NumCasts >= 1, "Tankbuster tower 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<TyrannysGraspTowers>(id + 0x120, 2.7f, comp => comp.NumCasts >= 2, "Tankbuster tower 2")
            .DeactivateOnExit<TyrannysGraspTowers>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void P2DyingMemory(uint id, float delay)
    {
        ComponentCondition<DyingMemory>(id, delay, comp => comp.NumCasts > 0, "Raidwide 1")
            .ActivateOnEnter<DyingMemory>()
            .DeactivateOnExit<DyingMemory>()
            .SetHint(StateMachine.StateHint.Raidwide);
        ComponentCondition<DyingMemoryLast>(id + 0x10, 7.8f, comp => comp.NumCasts > 0, "Raidwide 8")
            .ActivateOnEnter<DyingMemoryLast>()
            .DeactivateOnExit<DyingMemoryLast>()
            .SetHint(StateMachine.StateHint.Raidwide);

        ActorCastStart(id + 0x100, _module.BossP2, AID.RoyalBanishment, 3.1f, true)
            .ActivateOnEnter<RoyalBanishment>(); // icon appears right before cast start
        ActorCastEnd(id + 0x101, _module.BossP2, 5, true);
        ComponentCondition<RoyalBanishment>(id + 0x110, 0.8f, comp => comp.NumCasts > 0, "Line stack 1");
        ComponentCondition<RoyalBanishment>(id + 0x120, 6, comp => comp.NumCasts >= 7);
        ComponentCondition<RoyalBanishment>(id + 0x130, 3, comp => comp.NumCasts >= 8, "Line stack 8")
            .DeactivateOnExit<RoyalBanishment>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }
}
