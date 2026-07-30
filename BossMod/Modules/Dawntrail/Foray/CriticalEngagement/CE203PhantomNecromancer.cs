namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE203PhantomNecromancer;

public enum OID : uint {
    PhantomNecromancer = 0x4BC1,
    Helper = 0x233C,
    LongDeadExplorer = 0x4BC2, // R1.000, x0 (spawn during fight)
    LongDeadPirate = 0x4BC3, // R2.600, x0 (spawn during fight)
    PhantomNecromancer1 = 0x4C75, // R1.000, x1

    _Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    _Gen_Actor1ebfc1 = 0x1EBFC1, // R0.500, x1, EventObj type
    _Gen_Actor1ebff5 = 0x1EBFF5, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint {
    AutoAttack = 50761, // PhantomNecromancer->player, no cast, single-target
    DarkII = 47181, // PhantomNecromancer->self, 5.0s cast, range 50 width 50 rect
    DarkFlareCast = 47182, // PhantomNecromancer->self, 5.0s cast, single-target
    DarkFlare = 47183, // Helper->self, no cast, ???

    RiseOfTheFallen = 47174, // PhantomNecromancer->self, 3.0s cast, single-target
    LongDeadExplorerExplosion = 47175, // 4BC2->self, 2.0s cast, range 8 circle
    LongDeadPirateExplosion = 47176, // 4BC3->self, 4.0s cast, range 80 width 7 cross

    ArcaneRevelation = 47179, // PhantomNecromancer->self, 3.0s cast, single-target
    Necrosurge = 47180, // Helper->self, 7.0s cast, range 70 width 12 rect

    _Ability_ = 47173, // 4C75->self, no cast, ???
}

public enum SID : uint {
    ExplosionTimer = 2056, // none->LongDeadExplorer, extra=0x26B
}

sealed class DarkII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DarkII, new AOEShapeRect(50.0f, 25.0f));
sealed class DarkFlare(BossModule module) : Components.RaidwideCast(module, (uint)AID.DarkFlareCast);
sealed class Necrosurge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Necrosurge, new AOEShapeRect(70.0f, 6.0f));

// TODO try and find a way to show these earlier maybe? actors spawn way earlier, but 8 spawn at the same time, how to tell the difference between them?
sealed class LongDeadPirate(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LongDeadPirateExplosion, new AOEShapeCross(80.0f, 3.5f));

// TODO clean up - instead when status gain is done add it to a wave of similar times instead of creating multiple lists
// TODO when rewriting this, just add the aoes on status gain, then we can filter by expireAt to get the wave and filter by instanceID on the ones with the
//  same times


sealed class LongDeadExplorer(BossModule module) : Components.GenericAOEs(module, (uint)AID.LongDeadExplorerExplosion) {
    private List<AOEInstance> aoes = [];
    private AOEShapeCircle shape = new AOEShapeCircle(8.0f);
    private bool active = false;
    private int waveSize = 0;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.RiseOfTheFallen) {
            active = true;
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        if (actor.OID == (uint)OID.LongDeadExplorer && status.ID == (uint)SID.ExplosionTimer) {
            aoes.Add(new(shape, actor.Position, actor.Rotation, status.ExpireAt, actorID: actor.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.LongDeadExplorerExplosion) {
            if (aoes.Count > 0) {
                aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        if (!active || aoes.Count == 0) {
            active = false;
            waveSize = 0;
            return [];
        }

        var sorted = aoes.OrderBy(a => a.Activation).ToList();
        if (waveSize == 0) {
            waveSize = sorted.Count(a => a.Activation < sorted[0].Activation.AddSeconds(0.2));
        }

        List<AOEInstance> ordered = [];
        List<AOEInstance> cluster = [];
        foreach (var aoe in sorted) {
            if (cluster.Count > 0 && aoe.Activation >= cluster[0].Activation.AddSeconds(0.2)) {
                ordered.AddRange(cluster.OrderByDescending(a => a.ActorID));
                cluster.Clear();
            }
            cluster.Add(aoe);
        }
        ordered.AddRange(cluster.OrderByDescending(a => a.ActorID));

        sorted = ordered.Take(waveSize).ToList();
        var half = waveSize / 2;
        for (int i = 0; i < sorted.Count; i++) {
            var aoe = sorted[i];
            aoe.Color = i < half ? Colors.Danger : Colors.AOE;
            aoe.Risky = i < half;
            sorted[i] = aoe;
        }

        return CollectionsMarshal.AsSpan(sorted);
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
    ActionIDType = null, // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
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
