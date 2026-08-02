namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE211LostontheWind;

public enum OID : uint {
    Abductor = 0x4BE1,
    Abductor1 = 0x4BE4, // R1.000, x1
    Helper = 0x233C,
    Plume = 0x4BE3, // R1.000, x0 (spawn during fight)
    BuffetWind = 0x1EBFA9, // R0.500, x0 (spawn during fight), EventObj type
    BitingWind = 0x4BE2, // R1.000, x0 (spawn during fight)
}

public enum AID : uint {
    MapArenaChange = 47435, // 4BE4->self, no cast, range ?-30 donut
    AutoAttack = 47434, // Abductor->player, no cast, single-target
    Teleport = 47433, // Abductor->location, no cast, single-target
    WindBlade = 47441, // Abductor->self, 5.0s cast, range 60 180-degree cone
    CyclonicRingTeleport = 47447, // Abductor->location, no cast, single-target
    CyclonicRing = 47449, // Helper->self, 5.5s cast, range 5-60 donut
    PlumefallTrap = 47442, // Abductor->self, 3.0s cast, single-target
    Splinter = 47443, // 4BE3->self, 4.5s cast, range 13 circle
    SkydiveTeleport = 47446, // Abductor->location, no cast, single-target
    Skydive = 47448, // Helper->self, 5.5s cast, range 15 circle
    Hurricane = 47436, // Abductor->self, 5.0s cast, single-target
    Hurricane1 = 48120, // Helper->self, no cast, ???
    AerosnareCast = 47444, // Abductor->self, 3.5+0.5s cast, single-target
    Aerosnare = 47445, // Helper->self, 4.0s cast, range 60 60-degree cone
    Buffet = 48250, // Helper->self, 4.0s cast, range 60 width 60 rect
    Buffet1 = 47440, // Helper->self, no cast, ???

    StrongWind = 47437, // Helper->self, no cast, range 4 circle
    TendonRipper = 47438, // 4BE2->self, 1.0s cast, single-target
    TendonRipper1 = 47439, // Helper->self, 1.0s cast, range 60 width 8 cross
}

public enum SID : uint {
    Sprint = 4520, // none->4BE2, extra=0xE4/0x40
}

public enum IconID : uint {
    BitingWindAOE = 506, // 4BE2->self
}

sealed class WindBlade(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WindBlade, new AOEShapeCone(60.0f, 90.0f.Degrees()));
sealed class CyclonicRing(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CyclonicRing, new AOEShapeDonut(5.0f, 60.0f));
sealed class Splinter(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Splinter, new AOEShapeCircle(13.0f));
sealed class Skydive(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Skydive, new AOEShapeCircle(15.0f));
sealed class Hurricane(BossModule module) : Components.RaidwideCast(module, (uint)AID.Hurricane);

sealed class Aerosnare : Components.SimpleAOEs {
    public Aerosnare(BossModule module) : base(module, (uint)AID.Aerosnare, new AOEShapeCone(60.0f, 30.0f.Degrees())) {
        MaxDangerColor = 3;
    }
}

sealed class Buffet(BossModule module) : Components.GenericKnockback(module) {
    private List<Knockback> knockbacks = [];
    private BuffetWind? _aoe;

    public override void OnActorEAnim(Actor actor, uint state) {
        if (actor.OID == (uint)OID.BuffetWind && state == 65538) {
            knockbacks.Add(new(actor.Position, 24.0f, WorldState.FutureTime(11.1f), direction: actor.Rotation, kind: Kind.DirForward, actorID: actor.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Buffet) {
            knockbacks.Clear();
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(knockbacks);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (knockbacks.Count == 0) {
            return;
        }

        ref readonly var k = ref knockbacks.Ref(0);
        _aoe ??= Module.FindComponent<BuffetWind>();

        var activation = k.Activation;
        if (!IsImmune(slot, activation)) {
            var aoes = CollectionsMarshal.AsSpan(_aoe!.aoes);
            var len = aoes.Length;
            var circles = new (WPos origin, float Radius)[len];
            for (var i = 0; i < len; ++i) {
                ref var aoe = ref aoes[i];
                circles[i] = (aoe.Origin, 4.0f);
            }

            hints.AddForbiddenZone(new SDKnockbackInAABBSquareAwayFromOriginPlusAOECirclesMixedRadii(Arena.Center, k.Origin, 20f, 19f, circles, len), activation);
        }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) {
        _aoe ??= Module.FindComponent<BuffetWind>();
        var aoes = CollectionsMarshal.AsSpan(_aoe!.aoes);
        var len = aoes.Length;
        for (var i = 0; i < len; ++i) {
            if (aoes[i].Check(pos)) {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }
}

sealed class BuffetWind(BossModule module) : Components.GenericAOEs(module) {
    public List<AOEInstance> aoes = [];
    private List<(Actor actor, bool incomingWind)> winds = []; // incomingWind is for when the actor has an active icon
    private readonly AOEShapeCross cross = new(60.0f, 5.0f);
    private readonly AOEShapeCircle circle = new(4.0f);

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.BitingWind) {
            winds.Add((actor, false));
        }
    }

    public override void OnActorDestroyed(Actor actor) {
        if (actor.OID == (uint)OID.BitingWind) {
            var index = winds.FindIndex(windInstance => windInstance.actor.InstanceID == actor.InstanceID);
            if (index < 0) {
                return;
            }

            winds.RemoveAt(index);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        if (actor.OID == (uint)OID.BitingWind && iconID == (uint)IconID.BitingWindAOE) {
            var index = winds.FindIndex(windInstance => windInstance.actor.InstanceID == actor.InstanceID);
            if (index < 0) {
                return;
            }

            var wind = winds[index];
            wind.incomingWind = true;
            winds[index] = wind;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.TendonRipper) {
            var index = winds.FindIndex(windInstance => windInstance.actor.InstanceID == caster.InstanceID);
            if (index < 0) {
                return;
            }

            var wind = winds[index];
            wind.incomingWind = false;
            winds[index] = wind;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        aoes.Clear();

        if (winds.Count == 0) {
            return [];
        }

        foreach (var wind in winds) {
            aoes.Add(new(circle, wind.actor.Position, wind.actor.Rotation));
            if (wind.incomingWind == true) {
                aoes.Add(new(cross, wind.actor.Position, Angle.AnglesIntercardinals[1], WorldState.FutureTime(5.1f)));
                aoes.Add(new(cross, wind.actor.Position, Angle.AnglesCardinals[1], WorldState.FutureTime(5.1f)));
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

[SkipLocalsInit]
sealed class CE211LostontheWindStates : StateMachineBuilder {
    public CE211LostontheWindStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<WindBlade>()
            .ActivateOnEnter<CyclonicRing>()
            .ActivateOnEnter<Splinter>()
            .ActivateOnEnter<Skydive>()
            .ActivateOnEnter<Hurricane>()
            .ActivateOnEnter<Aerosnare>()
            .ActivateOnEnter<Buffet>()
            .ActivateOnEnter<BuffetWind>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(CE211LostontheWindStates),
    ConfigType = null, // replace null with typeof(LostontheWindConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Abductor,
    Contributors = "The Combat Reborn Team (LTS) & Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14505u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class CE211LostontheWind(WorldState ws, Actor primary) : BossModule(ws, primary, new(-150f, -860f), new ArenaBoundsCircle(23.9f)) {
    protected override bool CheckPull() => base.CheckPull() && Raid.Player()!.Position.InCircle(Arena.Center, 24f);
}
