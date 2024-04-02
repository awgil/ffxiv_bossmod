namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai;

class C022GoraiStates : StateMachineBuilder
{
    private bool _savage;

    public C022GoraiStates(BossModule module, bool savage) : base(module)
    {
        _savage = savage;
        DeathPhase(0, SinglePhase);
    }

    private void SinglePhase(uint id)
    {
        Unenlightenment(id, 8.2f);
        SealOfScurryingSparksFlameAndSulphur(id + 0x10000, 9.1f);
        ImpurePurgation(id + 0x20000, 3.3f);
        Thundercall(id + 0x30000, 8.1f);
        TorchingTorment(id + 0x40000, 5.6f);
        RousingReincarnation(id + 0x50000, 8.1f);
        Unenlightenment(id + 0x60000, 6.4f);
        SealOfScurryingSparksCloudToGround(id + 0x70000, 9.1f);
        FightingSpirits(id + 0x80000, 10.3f);
        TorchingTorment(id + 0x90000, 7.1f);
        MalformedReincarnation(id + 0xA0000, 8.1f);
        SealOfScurryingSparksFlameAndSulphur(id + 0xB0000, 10.6f);
        Unenlightenment(id + 0xC0000, 4.4f);
        Cast(id + 0xD0000, AID.LivingHell, 6.3f, 10, "Enrage");
    }

    private void Unenlightenment(uint id, float delay)
    {
        Cast(id, AID.Unenlightenment, delay, 5)
            .ActivateOnEnter<NUnenlightenment>(!_savage)
            .ActivateOnEnter<SUnenlightenment>(_savage);
        ComponentCondition<Unenlightenment>(id + 0x10, 0.6f, comp => comp.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<Unenlightenment>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void TorchingTorment(uint id, float delay)
    {
        Cast(id, AID.TorchingTorment, delay, 5)
            .ActivateOnEnter<TorchingTorment>();
        ComponentCondition<TorchingTorment>(id + 0x10, 0.6f, comp => comp.NumCasts > 0, "Tankbuster")
            .DeactivateOnExit<TorchingTorment>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void SealOfScurryingSparksFlameAndSulphur(uint id, float delay)
    {
        Cast(id, AID.SealOfScurryingSparks, delay, 4)
            .ActivateOnEnter<SealOfScurryingSparks>();
        Cast(id + 0x10, AID.FlameAndSulphur, 2.4f, 3);
        // +0.8s: spawn rocks/flames
        CastMulti(id + 0x20, new[] { AID.BrazenBalladExpanding, AID.BrazenBalladSplitting }, 6.4f, 5)
            .ActivateOnEnter<FlameAndSulphur>();
        ComponentCondition<FlameAndSulphur>(id + 0x30, 3.1f, comp => comp.NumCasts > 0, "Expanding/splitting aoes")
            .DeactivateOnExit<FlameAndSulphur>();
        ComponentCondition<SealOfScurryingSparks>(id + 0x31, 0.4f, comp => comp.NumMechanics > 0, "Pairs")
            .DeactivateOnExit<SealOfScurryingSparks>();
    }

    private void SealOfScurryingSparksCloudToGround(uint id, float delay)
    {
        Cast(id, AID.SealOfScurryingSparks, delay, 4)
            .ActivateOnEnter<SealOfScurryingSparks>();
        Cast(id + 0x10, AID.CloudToGround, 2.4f, 6.2f)
            .ActivateOnEnter<CloudToGround>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<CloudToGround>(id + 0x20, 0.8f, comp => comp.NumCasts > 0, "Exaflares start");
        ComponentCondition<SealOfScurryingSparks>(id + 0x30, 1.9f, comp => comp.NumMechanics > 0, "Stack/spread");
        ComponentCondition<SealOfScurryingSparks>(id + 0x40, 5.0f, comp => comp.NumMechanics > 1, "Spread/stack")
            .DeactivateOnExit<CloudToGround>() // last exaflare ~0.5s before resolve
            .DeactivateOnExit<SealOfScurryingSparks>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void ImpurePurgation(uint id, float delay)
    {
        Cast(id, AID.ImpurePurgation, delay, 3.6f)
            .ActivateOnEnter<ImpurePurgationBait>();
        ComponentCondition<ImpurePurgationBait>(id + 0x10, 0.6f, comp => comp.NumCasts > 0, "Proteans bait")
            .DeactivateOnExit<ImpurePurgationBait>();
        ComponentCondition<ImpurePurgationAOE>(id + 0x20, 2.0f, comp => comp.NumCasts > 0, "Proteans resolve")
            .ActivateOnEnter<NImpurePurgationAOE>(!_savage)
            .ActivateOnEnter<SImpurePurgationAOE>(_savage)
            .DeactivateOnExit<ImpurePurgationAOE>();
    }

    private void Thundercall(uint id, float delay)
    {
        Cast(id, AID.Thundercall, delay, 3);
        // +3.1s: spawn 6 lightning orbs
        Cast(id + 0x10, AID.HumbleHammer, 5.9f, 5, "Reduce aoe size")
            .ActivateOnEnter<Thundercall>()
            .ActivateOnEnter<Flintlock>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<Flintlock>(id + 0x20, 4.1f, comp => comp.NumCasts > 0, "Wild charge")
            .DeactivateOnExit<Flintlock>();
        ComponentCondition<Thundercall>(id + 0x21, 0.1f, comp => comp.NumCasts > 0, "AOEs")
            .DeactivateOnExit<Thundercall>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void RousingReincarnation(uint id, float delay)
    {
        Cast(id, AID.RousingReincarnation, delay, 5)
            .ActivateOnEnter<NRousingReincarnation>(!_savage)
            .ActivateOnEnter<SRousingReincarnation>(_savage);
        ComponentCondition<RousingReincarnation>(id + 0x10, 0.6f, comp => comp.NumCasts > 0, "Towers/proteans start")
            .DeactivateOnExit<RousingReincarnation>();
        Cast(id + 0x20, AID.MalformedPrayer, 1.8f, 4)
            .ActivateOnEnter<MalformedPrayer1>(); // env controls are 2s after cast end, then every 6s; bursts are 10s after corresponding envcontrol
        Cast(id + 0x30, AID.PointedPurgation, 3.4f, 8)
            .ActivateOnEnter<PointedPurgation>()
            .SetHint(StateMachine.StateHint.PositioningStart);
        ComponentCondition<MalformedPrayer1>(id + 0x40, 0.7f, comp => comp.NumCasts > 0, "Towers/proteans 1");
        ComponentCondition<MalformedPrayer1>(id + 0x50, 6, comp => comp.NumCasts > 2, "Towers/proteans 2");
        ComponentCondition<MalformedPrayer1>(id + 0x60, 6, comp => comp.NumCasts > 4, "Towers/proteans 3");
        ComponentCondition<MalformedPrayer1>(id + 0x70, 6, comp => comp.NumCasts > 6, "Towers/proteans 4")
            .DeactivateOnExit<MalformedPrayer1>()
            .DeactivateOnExit<PointedPurgation>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }

    private void FightingSpirits(uint id, float delay)
    {
        CastStart(id, AID.FightingSpirits, delay)
            .ActivateOnEnter<WorldlyPursuitBait>(); // icons appear right before cast start
        CastEnd(id + 1, 5)
            .ActivateOnEnter<NFightingSpirits>(!_savage)
            .ActivateOnEnter<SFightingSpirits>(_savage);
        ComponentCondition<FightingSpirits>(id + 2, 1.2f, comp => comp.NumCasts > 0, "Knockback")
            .DeactivateOnExit<FightingSpirits>();
        Targetable(id + 0x10, false, 2.7f, "Boss disappears");
        ComponentCondition<WorldlyPursuitBait>(id + 0x20, 0.7f, comp => comp.NumCasts > 0, "Jump 1");
        ComponentCondition<WorldlyPursuitBait>(id + 0x30, 3.1f, comp => comp.NumCasts > 1, "Jump 2");
        ComponentCondition<WorldlyPursuitBait>(id + 0x40, 3.1f, comp => comp.NumCasts > 2, "Jump 3");
        ComponentCondition<WorldlyPursuitBait>(id + 0x50, 3.1f, comp => comp.NumCasts > 3, "Jump 4")
            .DeactivateOnExit<WorldlyPursuitBait>();
        ComponentCondition<WorldlyPursuitLast>(id + 0x60, 3.1f, comp => comp.NumCasts > 0, "Jump 5")
            .ActivateOnEnter<WorldlyPursuitLast>()
            .DeactivateOnExit<WorldlyPursuitLast>();
        Targetable(id + 0x70, true, 2.5f, "Boss reappears");
    }

    private void MalformedReincarnation(uint id, float delay)
    {
        Cast(id, AID.MalformedReincarnation, delay, 5)
            .ActivateOnEnter<NMalformedReincarnation>(!_savage)
            .ActivateOnEnter<SMalformedReincarnation>(_savage);
        ComponentCondition<MalformedReincarnation>(id + 0x10, 0.6f, comp => comp.NumCasts > 0, "Towers start")
            .DeactivateOnExit<MalformedReincarnation>();
        Cast(id + 0x20, AID.MalformedPrayer, 1.8f, 4)
            .ActivateOnEnter<MalformedPrayer2>(); // first env controls are 2s after cast end
        CastStart(id + 0x30, AID.FlickeringFlame, 10.2f, "Towers drop")
            .SetHint(StateMachine.StateHint.PositioningStart);
        CastEnd(id + 0x31, 3)
            .ActivateOnEnter<NFlickeringFlame>(!_savage)
            .ActivateOnEnter<SFlickeringFlame>(_savage);
        ComponentCondition<MalformedPrayer2>(id + 0x40, 2.0f, comp => comp.NumCasts > 0, "Towers 1", 0);
        ComponentCondition<MalformedPrayer2>(id + 0x50, 2.5f, comp => comp.NumCasts > 4, "Towers 2", 0);
        ComponentCondition<MalformedPrayer2>(id + 0x60, 2.0f, comp => comp.NumCasts > 8, "Towers 3", 0)
            .DeactivateOnExit<MalformedPrayer2>();
        ComponentCondition<FlickeringFlame>(id + 0x70, 0.5f, comp => comp.NumCasts >= 8, "Criss-cross 1");
        ComponentCondition<FlickeringFlame>(id + 0x80, 2.1f, comp => comp.NumCasts >= 16, "Criss-cross 2")
            .DeactivateOnExit<FlickeringFlame>()
            .SetHint(StateMachine.StateHint.PositioningEnd);
    }
}

class C022NGoraiStates : C022GoraiStates { public C022NGoraiStates(BossModule module) : base(module, false) { } }
class C022SGoraiStates : C022GoraiStates { public C022SGoraiStates(BossModule module) : base(module, true) { } }
