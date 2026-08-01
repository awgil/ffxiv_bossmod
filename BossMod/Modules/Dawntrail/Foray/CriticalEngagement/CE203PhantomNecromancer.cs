namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE203PhantomNecromancer;

public enum OID : uint {
    PhantomNecromancer = 0x4BC1,
    Helper = 0x233C,
    LongDeadExplorer = 0x4BC2, // R1.000, x0 (spawn during fight)
    LongDeadPirate = 0x4BC3, // R2.600, x0 (spawn during fight)
    PhantomNecromancer1 = 0x4C75, // R1.000, x1
}

public enum AID : uint {
    AutoAttack = 50761, // PhantomNecromancer->player, no cast, single-target
    Ability = 47173, // 4C75->self, no cast, ???
    DarkII = 47181, // PhantomNecromancer->self, 5.0s cast, range 50 width 50 rect
    DarkFlareCast = 47182, // PhantomNecromancer->self, 5.0s cast, single-target
    DarkFlare = 47183, // Helper->self, no cast, ???
    ArcaneRevelation = 47179, // PhantomNecromancer->self, 3.0s cast, single-target
    Necrosurge = 47180, // Helper->self, 7.0s cast, range 70 width 12 rect

    RiseOfTheFallen = 47174, // PhantomNecromancer->self, 3.0s cast, single-target
    LongDeadExplorerExplosion = 47175, // 4BC2->self, 2.0s cast, range 8 circle
    LongDeadPirateExplosion = 47176, // 4BC3->self, 4.0s cast, range 80 width 7 cross
}

public enum SID : uint {
    ExplosionTimer = 2056, // none->LongDeadExplorer, extra=0x26B
}

sealed class DarkII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkII, new AOEShapeRect(50.0f, 25.0f));
sealed class DarkFlare(BossModule module) : Components.RaidwideCast(module, (uint)AID.DarkFlareCast);
sealed class Necrosurge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Necrosurge, new AOEShapeRect(70.0f, 6.0f));

sealed class LongDeadExplorer(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];
    private readonly AOEShapeCircle shape = new(8.0f);
    private List<Actor> longDeadExplorerWave = []; // Actors waiting to be sorted in order of activation
    private int? waveSize;
    private DateTime pendingExpireAt; // last expireAt SID timer
    private DateTime lastWaveAdded; // So we add the final wave
    private DateTime? nextActivation; // without this waves can overlap - note the cast timers for each actor seem to be different, so this is the only way to order
    public bool active = false;

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if (status.ID == (uint)SID.ExplosionTimer && actor.OID == (uint)OID.LongDeadExplorer) {
            if (longDeadExplorerWave.Count > 0 && (status.ExpireAt - pendingExpireAt).Duration() > TimeSpan.FromSeconds(1.0f)) {
                SetupWave();
            }

            longDeadExplorerWave.Add(actor);
            pendingExpireAt = status.ExpireAt;
            lastWaveAdded = WorldState.CurrentTime;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.LongDeadExplorerExplosion) {
            if (aoes.Count > 0) {
                aoes.RemoveAll(a => a.ActorID == caster.InstanceID);
            }

            if (aoes.Count == 0) {
                active = false;
                waveSize = null;
                nextActivation = null;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.RiseOfTheFallen) {
            active = true;
        }
    }

    public override void Update() {
        if (longDeadExplorerWave.Count > 0 && (WorldState.CurrentTime - lastWaveAdded) > TimeSpan.FromSeconds(2.0f)) {
            SetupWave();
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        if (!active || aoes.Count == 0) {
            active = false;
            return [];
        }

        if (waveSize == null) {
            return [];
        }

        var incomingAOEs = aoes.OrderBy(aoe => aoe.Activation).Take(waveSize.Value * 2).ToList();
        for (int i = 0; i < incomingAOEs.Count; i++) {
            var aoeInstance = incomingAOEs[i];
            aoeInstance.Color = i < waveSize ? Colors.Danger : Colors.AOE;
            incomingAOEs[i] = aoeInstance;
        }

        return CollectionsMarshal.AsSpan(incomingAOEs);
    }

    private void SetupWave() {
        if (longDeadExplorerWave.Count == 0) {
            return;
        }

        var sort = longDeadExplorerWave.OrderByDescending(actor => actor.InstanceID).ToList();
        waveSize ??= sort.Count / 2;
        nextActivation ??= pendingExpireAt;

        foreach (var aoe in sort.Take(waveSize.Value)) {
            aoes.Add(new(shape, aoe.Position, aoe.Rotation, nextActivation.Value, actorID: aoe.InstanceID));
        }

        nextActivation = nextActivation.Value + TimeSpan.FromSeconds(3.0f);
        if (sort.Count > waveSize.Value) {
            foreach (var aoe in sort.Skip(waveSize.Value)) {
                aoes.Add(new(shape, aoe.Position, aoe.Rotation, nextActivation.Value, actorID: aoe.InstanceID));
            }
            nextActivation = nextActivation.Value + TimeSpan.FromSeconds(3.0f);
        }

        longDeadExplorerWave.Clear();
    }
}

sealed class LongDeadPirate(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];
    private readonly AOEShapeCross shape = new (80.0f, 3.5f);
    private List<Actor> longDeadPirateWave = []; // Actors waiting to be sorted in order of activation
    private DateTime lastWaveAdded; // So we add the final wave
    private int? waveSize;
    private LongDeadExplorer longDeadExplorer = module.FindComponent<LongDeadExplorer>()!;

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.LongDeadPirate) {
            if (longDeadPirateWave.Count > 0 && (WorldState.CurrentTime - lastWaveAdded).Duration() > TimeSpan.FromSeconds(1.0f)) {
                SetupWave();
            }

            longDeadPirateWave.Add(actor);
            lastWaveAdded = WorldState.CurrentTime;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.LongDeadPirateExplosion) {
            if (aoes.Count > 0) {
                aoes.RemoveAll(a => a.ActorID == caster.InstanceID);
            }

            if (aoes.Count == 0) {
                waveSize = null;
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        if (longDeadExplorer.active || aoes.Count == 0) {
            return [];
        }

        return CollectionsMarshal.AsSpan(aoes.OrderBy(aoe => aoe.Activation).Take(4).ToList());
    }

    public override void Update() {
        if (longDeadPirateWave.Count > 0 && (WorldState.CurrentTime - lastWaveAdded) > TimeSpan.FromSeconds(2.0f)) {
            SetupWave();
        }
    }

    private void SetupWave() {
        if (longDeadPirateWave.Count == 0) {
            return;
        }

        var sort = longDeadPirateWave.OrderByDescending(actor => actor.InstanceID).ToList();
        waveSize ??= sort.Count / 2;
        var now = WorldState.CurrentTime;

        // Case: 8 actors created at the same time
        if (sort.Count > waveSize.Value) {
            // Always 21.4f from spawn time
            foreach (var pirate in sort.Take(waveSize.Value)) {
                aoes.Add(new(shape, pirate.Position, pirate.Rotation, now + TimeSpan.FromSeconds(21.4f), actorID: pirate.InstanceID));
            }

            // Always 32.9f from spawn time
            foreach (var pirate in sort.Skip(waveSize.Value)) {
                aoes.Add(new(shape, pirate.Position, pirate.Rotation, now + TimeSpan.FromSeconds(32.9f), actorID: pirate.InstanceID));
            }
        } else {
            // Case single wave - always 32.0f from spawn time (these spawn later on compared to the others)
            foreach (var pirate in sort) {
                aoes.Add(new(shape, pirate.Position, pirate.Rotation, now + TimeSpan.FromSeconds(32.0f), actorID: pirate.InstanceID));
            }
        }

        longDeadPirateWave.Clear();
    }
}

[SkipLocalsInit]
sealed class PhantomNecromancerStates : StateMachineBuilder {
    public PhantomNecromancerStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<DarkII>()
            .ActivateOnEnter<DarkFlare>()
            .ActivateOnEnter<LongDeadExplorer>()
            .ActivateOnEnter<LongDeadPirate>()
            .ActivateOnEnter<Necrosurge>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(PhantomNecromancerStates),
    ConfigType = null, // replace null with typeof(PhantomNecromancerConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.PhantomNecromancer,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14512u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class PhantomNecromancer(WorldState ws, Actor primary) : BossModule(ws, primary, new(224.000f, -860.000f), new ArenaBoundsSquare(20f));
