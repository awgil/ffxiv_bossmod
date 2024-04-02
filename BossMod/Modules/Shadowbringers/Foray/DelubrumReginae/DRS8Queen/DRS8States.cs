namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS8Queen;

class DRS8States : StateMachineBuilder
{
    public DRS8States(BossModule module) : base(module)
    {
        SimplePhase(0, Phase1, "P1")
            .Raw.Update = () => Module.PrimaryActor.IsDestroyed || Module.PrimaryActor.HP.Cur <= 1 || (Module.PrimaryActor.CastInfo?.IsSpell(AID.GodsSaveTheQueen) ?? false);
        DeathPhase(1, Phase2);
    }

    private void Phase1(uint id)
    {
        EmpyreanIniquity(id, 10.2f);
        QueensWill(id + 0x10000, 13.7f);
        CleansingSlash(id + 0x20000, 10.9f);
        EmpyreanIniquity(id + 0x30000, 4.2f);
        QueensEdict(id + 0x40000, 11.5f);
        SimpleState(id + 0x50000, 12.5f, "Second phase");
    }

    private void Phase2(uint id)
    {
        GodsSaveTheQueen(id, 0);
        MaelstromsBolt(id + 0x10000, 31.7f);
        RelentlessPlay1(id + 0x20000, 7.3f);
        CleansingSlash(id + 0x30000, 4.4f);
        RelentlessPlay2(id + 0x40000, 8.2f);
        EmpyreanIniquity(id + 0x50000, 5.1f);
        QueensEdict(id + 0x60000, 11.5f);
        CleansingSlash(id + 0x70000, 2.1f);
        RelentlessPlay3(id + 0x80000, 10.2f);
        MaelstromsBolt(id + 0x90000, 16.3f);
        EmpyreanIniquity(id + 0xA0000, 6.2f);
        RelentlessPlay4(id + 0xB0000, 8.2f);
        RelentlessPlay5(id + 0xC0000, 0.1f);
        // TODO: boss gains damage up at +6.6, then presumably would start some enrage cast...
        SimpleState(id + 0xD0000, 15, "Enrage");
    }

    private void EmpyreanIniquity(uint id, float delay)
    {
        Cast(id, AID.EmpyreanIniquity, delay, 5, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void CleansingSlash(uint id, float delay)
    {
        Cast(id, AID.CleansingSlashFirst, delay, 5, "Tankbuster 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<CleansingSlashSecond>(id + 2, 3.1f, comp => comp.NumCasts > 0, "Tankbuster 2")
            .ActivateOnEnter<CleansingSlashSecond>()
            .DeactivateOnExit<CleansingSlashSecond>()
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void QueensWill(uint id, float delay)
    {
        // right before cast start: ENVC 19.00200010, guards gain 2056 status with extra 0xE1 + PATE 1E43
        Cast(id, AID.QueensWill, delay, 5)
            .ActivateOnEnter<QueensWill>() // statuses appear ~0.7s after cast end
            .OnEnter(() => Module.Arena.Bounds = new ArenaBoundsSquare(Module.Bounds.Center, Module.Bounds.HalfSize)); // deathwall changes around cast start
        Cast(id + 0x10, AID.NorthswainsGlow, 3.2f, 3)
            .ActivateOnEnter<NorthswainsGlow>(); // aoe casts start ~0.8s after visual cast end
        Cast(id + 0x20, AID.BeckAndCallToArmsWillKW, 3.1f, 5);
        ComponentCondition<NorthswainsGlow>(id + 0x30, 2.6f, comp => comp.NumCasts > 0)
            .DeactivateOnExit<NorthswainsGlow>();

        CastStart(id + 0x40, AID.BeckAndCallToArmsWillSG, 0.5f);
        ComponentCondition<QueensWill>(id + 0x41, 1.1f, comp => comp.NumCasts >= 2, "Easy chess 1");
        CastEnd(id + 0x42, 3.9f);
        ComponentCondition<QueensWill>(id + 0x43, 4.3f, comp => comp.NumCasts >= 4, "Easy chess 2")
            .DeactivateOnExit<QueensWill>()
            .OnExit(() => Module.Arena.Bounds = new ArenaBoundsCircle(Module.Bounds.Center, Module.Bounds.HalfSize)); // deathwall changes ~4.7s after this
    }

    private void QueensEdict(uint id, float delay)
    {
        Cast(id, AID.QueensEdict, delay, 5)
            .ActivateOnEnter<QueensEdict>() // safezone envcontrol, statuses on guards and players appear ~0.8s after cast end
            .OnEnter(() => Module.Arena.Bounds = new ArenaBoundsSquare(Module.Bounds.Center, Module.Bounds.HalfSize)); // deathwall changes around cast start
        Targetable(id + 0x10, false, 3.1f, "Disappear");
        Cast(id + 0x20, AID.BeckAndCallToArmsEdictKW, 0.1f, 16.3f);

        CastStart(id + 0x30, AID.BeckAndCallToArmsEdictSG, 3.2f);
        ComponentCondition<QueensEdict>(id + 0x31, 1.3f, comp => comp.NumCasts >= 2, "Super chess rows"); // 1st edict movement starts ~0.2s before this
        CastEnd(id + 0x32, 7.4f);

        ComponentCondition<QueensEdict>(id + 0x40, 2.4f, comp => comp.NumStuns > 0, "Super chess columns");
        ComponentCondition<QueensEdict>(id + 0x41, 1.9f, comp => comp.NumCasts >= 4);

        CastStart(id + 0x50, AID.GunnhildrsBlades, 2.8f);
        ComponentCondition<QueensEdict>(id + 0x51, 1.3f, comp => comp.NumStuns == 0); // 2nd edict movement starts ~1.0s before this
        ComponentCondition<QueensEdict>(id + 0x52, 9, comp => comp.NumStuns > 0, "Super chess safespot");
        CastEnd(id + 0x53, 3.7f)
            .DeactivateOnExit<QueensEdict>();

        Targetable(id + 0x60, true, 3.1f, "Reappear")
            .OnExit(() => Module.Arena.Bounds = new ArenaBoundsCircle(Module.Bounds.Center, Module.Bounds.HalfSize)); // deathwall changes ~1.9s after this
    }

    private void GodsSaveTheQueen(uint id, float delay)
    {
        Cast(id, AID.GodsSaveTheQueen, delay, 5);
        ComponentCondition<GodsSaveTheQueen>(id + 0x10, 2.1f, comp => comp.NumCasts > 0, "Raidwide")
            .ActivateOnEnter<GodsSaveTheQueen>()
            .DeactivateOnExit<GodsSaveTheQueen>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void MaelstromsBolt(uint id, float delay)
    {
        ComponentCondition<MaelstromsBolt>(id, delay, comp => comp.NumCasts > 0, "Reflect/raidwide")
            .ActivateOnEnter<MaelstromsBolt>()
            .DeactivateOnExit<MaelstromsBolt>();
    }

    private void RelentlessPlay1(uint id, float delay)
    {
        Cast(id, AID.RelentlessPlay, delay, 5);

        // +3.0s: tethers/icons
        ActorCast(id + 0x10, Module.Enemies(OID.QueensWarrior).FirstOrDefault, AID.ReversalOfForces, 3.1f, 4); // gunner casts automatic turret - starts 1s later, ends at the same time

        // +1.0s: tethers are replaced with statuses
        CastStart(id + 0x20, AID.NorthswainsGlow, 1.1f);
        // +0.1s: turrets spawn
        ActorCastStart(id + 0x21, Module.Enemies(OID.QueensGunner).FirstOrDefault, AID.Reading, 2.1f);
        CastEnd(id + 0x22, 0.9f);
        ActorCastEnd(id + 0x23, Module.Enemies(OID.QueensGunner).FirstOrDefault, 2.1f)
            .ActivateOnEnter<NorthswainsGlow>(); // aoes start ~0.8s after visual cast end
        // +1.0s: unseen statuses

        ActorCastStart(id + 0x30, Module.Enemies(OID.QueensWarrior).FirstOrDefault, AID.WindsOfWeight, 3.0f);
        ActorCastStart(id + 0x31, Module.Enemies(OID.QueensGunner).FirstOrDefault, AID.QueensShot, 0.2f)
            .ActivateOnEnter<WindsOfWeight>();
        ActorCastEnd(id + 0x32, Module.Enemies(OID.QueensWarrior).FirstOrDefault, 5.8f, false, "Wind/gravity")
            .ActivateOnEnter<QueensShot>()
            .DeactivateOnExit<WindsOfWeight>()
            .DeactivateOnExit<NorthswainsGlow>();
        ActorCastEnd(id + 0x33, Module.Enemies(OID.QueensGunner).FirstOrDefault, 1.2f, false, "Face gunner")
            .DeactivateOnExit<QueensShot>();

        // +0.5s: turrets start their casts
        ComponentCondition<TurretsTourUnseen>(id + 0x40, 3.5f, comp => comp.NumCasts > 0, "Face turret")
            .ActivateOnEnter<TurretsTourUnseen>()
            .DeactivateOnExit<TurretsTourUnseen>();
    }

    private void RelentlessPlay2(uint id, float delay)
    {
        Cast(id, AID.RelentlessPlay, delay, 5);
        ActorCast(id + 0x10, Module.Enemies(OID.QueensKnight).FirstOrDefault, AID.ShieldOmen, 3.2f, 3);

        ActorCastStart(id + 0x20, Module.Enemies(OID.QueensSoldier).FirstOrDefault, AID.DoubleGambit, 4.5f);
        Targetable(id + 0x21, false, 0.4f, "Disappear");
        ActorCastStart(id + 0x22, Module.Enemies(OID.QueensKnight).FirstOrDefault, AID.OptimalOffensive, 1.4f);
        CastStartMulti(id + 0x23, new[] { AID.JudgmentBladeR, AID.JudgmentBladeL }, 1.9f)
            .ActivateOnEnter<OptimalOffensive>()
            .ActivateOnEnter<OptimalOffensiveKnockback>()
            .ActivateOnEnter<UnluckyLotAetherialSphere>();
        ActorCastEnd(id + 0x24, Module.Enemies(OID.QueensSoldier).FirstOrDefault, 1.3f)
            .ActivateOnEnter<JudgmentBlade>();
        ActorCastStart(id + 0x25, Module.Enemies(OID.QueensSoldier).FirstOrDefault, AID.SecretsRevealed, 3.2f); // right before cast start, 2 unsafe avatars are tethered to caster
        ActorCastEnd(id + 0x26, Module.Enemies(OID.QueensKnight).FirstOrDefault, 0.6f, false, "Charge + Knockback")
            .DeactivateOnExit<OptimalOffensive>()
            .DeactivateOnExit<OptimalOffensiveKnockback>();
        CastEnd(id + 0x27, 1.9f);
        ComponentCondition<JudgmentBlade>(id + 0x28, 0.3f, comp => comp.NumCasts > 0, "Cleave")
            .DeactivateOnExit<JudgmentBlade>();
        ComponentCondition<UnluckyLotAetherialSphere>(id + 0x29, 0.5f, comp => comp.NumCasts > 0, "Sphere explosion")
            .DeactivateOnExit<UnluckyLotAetherialSphere>();
        ActorCastEnd(id + 0x2A, Module.Enemies(OID.QueensSoldier).FirstOrDefault, 1.7f);

        CastMulti(id + 0x30, new[] { AID.JudgmentBladeR, AID.JudgmentBladeL }, 2.2f, 7)
            .ActivateOnEnter<JudgmentBlade>()
            .ActivateOnEnter<PawnOff>(); // cast starts ~2.7s after judgment blade; we could show hints much earlier based on tethers
        ComponentCondition<JudgmentBlade>(id + 0x32, 0.3f, comp => comp.NumCasts > 0, "Cleave")
            .DeactivateOnExit<JudgmentBlade>();
        ComponentCondition<PawnOff>(id + 0x33, 2.4f, comp => comp.NumCasts > 0, "Real/fake aoes")
            .DeactivateOnExit<PawnOff>();

        Targetable(id + 0x40, true, 2.0f, "Reappear");
    }

    private void RelentlessPlay3(uint id, float delay)
    {
        Cast(id, AID.RelentlessPlay, delay, 5);
        ActorCastMulti(id + 0x10, Module.Enemies(OID.QueensKnight).FirstOrDefault, new[] { AID.SwordOmen, AID.ShieldOmen }, 3.1f, 3);

        // note: gunner starts automatic turret visual together with optimal play
        ActorCastMulti(id + 0x20, Module.Enemies(OID.QueensKnight).FirstOrDefault, new[] { AID.OptimalPlaySword, AID.OptimalPlayShield }, 6.5f, 5, false, "Cone + circle/donut")
            .ActivateOnEnter<OptimalPlaySword>()
            .ActivateOnEnter<OptimalPlayShield>()
            .ActivateOnEnter<OptimalPlayCone>()
            .DeactivateOnExit<OptimalPlaySword>()
            .DeactivateOnExit<OptimalPlayShield>()
            .DeactivateOnExit<OptimalPlayCone>();

        ActorCast(id + 0x30, Module.Enemies(OID.QueensGunner).FirstOrDefault, AID.TurretsTour, 1, 5, false, "Turrets start")
            .ActivateOnEnter<TurretsTour>();
        ComponentCondition<TurretsTour>(id + 0x40, 1.7f, comp => comp.NumCasts >= 4, "Turrets resolve")
            .DeactivateOnExit<TurretsTour>();
    }

    private void RelentlessPlay4(uint id, float delay)
    {
        Cast(id, AID.RelentlessPlay, delay, 5);
        ActorCast(id + 0x10, Module.Enemies(OID.QueensWarrior).FirstOrDefault, AID.Bombslinger, 3.1f, 3);
        // +0.9s: bombs spawn

        ActorCastStart(id + 0x20, Module.Enemies(OID.QueensWarrior).FirstOrDefault, AID.ReversalOfForces, 3.2f); // icons/tethers appear ~0.1s before cast start
        CastStart(id + 0x21, AID.HeavensWrath, 2.9f);
        ActorCastEnd(id + 0x22, Module.Enemies(OID.QueensWarrior).FirstOrDefault, 1.1f);
        CastEnd(id + 0x23, 1.9f);

        ComponentCondition<HeavensWrathAOE>(id + 0x30, 0.8f, comp => comp.Casters.Count > 0)
            .ActivateOnEnter<HeavensWrathAOE>();
        ActorCastStartMulti(id + 0x31, Module.Enemies(OID.QueensSoldier).FirstOrDefault, new[] { AID.FieryPortent, AID.IcyPortent }, 2.2f)
            .ActivateOnEnter<HeavensWrathKnockback>()
            .ActivateOnEnter<AboveBoard>();
        ComponentCondition<HeavensWrathAOE>(id + 0x32, 2.8f, comp => comp.NumCasts > 0, "Knockback")
            .ActivateOnEnter<FieryIcyPortent>()
            .DeactivateOnExit<HeavensWrathAOE>()
            .DeactivateOnExit<HeavensWrathKnockback>();
        ActorCastStart(id + 0x33, Module.Enemies(OID.QueensWarrior).FirstOrDefault, AID.AboveBoard, 0.5f);
        Targetable(id + 0x34, false, 1.8f, "Boss disappears");
        ActorCastEnd(id + 0x35, Module.Enemies(OID.QueensSoldier).FirstOrDefault, 0.9f, false, "Move/stay")
            .DeactivateOnExit<FieryIcyPortent>();
        ActorCastEnd(id + 0x36, Module.Enemies(OID.QueensWarrior).FirstOrDefault, 3.3f);

        ComponentCondition<AboveBoard>(id + 0x40, 1.0f, comp => comp.CurState == AboveBoard.State.ThrowUpDone, "Throw up");
        ComponentCondition<AboveBoard>(id + 0x41, 2.0f, comp => comp.CurState == AboveBoard.State.ShortExplosionsDone, "Bombs 1");
        ComponentCondition<AboveBoard>(id + 0x42, 4.2f, comp => comp.CurState == AboveBoard.State.LongExplosionsDone, "Bombs 2")
            .DeactivateOnExit<AboveBoard>();

        Targetable(id + 0x50, true, 3.0f, "Boss reappears");
    }

    private void RelentlessPlay5(uint id, float delay)
    {
        Cast(id, AID.RelentlessPlay, delay, 5);

        ActorCastStart(id + 0x10, Module.Enemies(OID.QueensWarrior).FirstOrDefault, AID.SoftEnrageW, 3.1f);
        ActorCastStart(id + 0x11, Module.Enemies(OID.QueensSoldier).FirstOrDefault, AID.SoftEnrageS, 3);
        ActorCastEnd(id + 0x12, Module.Enemies(OID.QueensWarrior).FirstOrDefault, 2, false, "Raidwide 1")
            .SetHint(StateMachine.StateHint.Raidwide);
        ActorCastEnd(id + 0x13, Module.Enemies(OID.QueensSoldier).FirstOrDefault, 3, false, "Raidwide 2")
            .SetHint(StateMachine.StateHint.Raidwide);

        ActorCastStart(id + 0x20, Module.Enemies(OID.QueensWarrior).FirstOrDefault, AID.SoftEnrageW, 0.1f);
        ActorCastStart(id + 0x21, Module.Enemies(OID.QueensSoldier).FirstOrDefault, AID.SoftEnrageS, 3);
        ActorCastEnd(id + 0x22, Module.Enemies(OID.QueensWarrior).FirstOrDefault, 2, false, "Raidwide 3")
            .SetHint(StateMachine.StateHint.Raidwide);
        CastStart(id + 0x23, AID.EmpyreanIniquity, 0.8f);
        ActorCastEnd(id + 0x24, Module.Enemies(OID.QueensSoldier).FirstOrDefault, 2.2f, false, "Raidwide 4")
            .SetHint(StateMachine.StateHint.Raidwide);
        CastEnd(id + 0x25, 2.8f, "Raidwide 5")
            .SetHint(StateMachine.StateHint.Raidwide);

        ActorCastStart(id + 0x30, Module.Enemies(OID.QueensKnight).FirstOrDefault, AID.SoftEnrageK, 1.3f);
        ActorCastStart(id + 0x31, Module.Enemies(OID.QueensWarrior).FirstOrDefault, AID.SoftEnrageW, 3);
        ActorCastEnd(id + 0x32, Module.Enemies(OID.QueensKnight).FirstOrDefault, 2, false, "Raidwide 6")
           .SetHint(StateMachine.StateHint.Raidwide);
        ActorCastStart(id + 0x33, Module.Enemies(OID.QueensSoldier).FirstOrDefault, AID.SoftEnrageS, 1);
        ActorCastEnd(id + 0x34, Module.Enemies(OID.QueensWarrior).FirstOrDefault, 2, false, "Raidwide 7")
           .SetHint(StateMachine.StateHint.Raidwide);
        ActorCastStart(id + 0x35, Module.Enemies(OID.QueensGunner).FirstOrDefault, AID.SoftEnrageG, 1);
        ActorCastEnd(id + 0x36, Module.Enemies(OID.QueensSoldier).FirstOrDefault, 2, false, "Raidwide 8")
           .SetHint(StateMachine.StateHint.Raidwide);
        ActorCastEnd(id + 0x37, Module.Enemies(OID.QueensGunner).FirstOrDefault, 3, false, "Raidwide 9")
           .SetHint(StateMachine.StateHint.Raidwide);
    }
}
