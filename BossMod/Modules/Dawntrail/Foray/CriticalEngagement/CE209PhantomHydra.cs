namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE209PhantomHydra;

public enum OID : uint {
    PhantomHydra = 0x4BC5,
    Helper = 0x233C,
    BallOfLevin = 0x4BC9, // R2.300, x3
    SwirlingOrb = 0x4BC8, // R0.500, x3
    BallOfFire = 0x4BC7, // R1.500, x12
    HolySphere = 0x4BC6, // R1.200, x2
    PoisonOrb = 0x1EBFC7, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint {
    AutoAttack = 50759, // PhantomHydra->player, no cast, single-target
    Discordance = 47209, // PhantomHydra->self, 5.0s cast, single-target - raidwide
    DiscordanceVisual = 47210, // Helper->self, no cast, ???
    NighDrawnEruption = 47197, // PhantomHydra->self, 5.0+2.0s cast, single-target
    FarFlungEruption = 47198, // PhantomHydra->self, 5.0+2.0s cast, single-target
    ElementalCascade = 47201, // Helper->location, 7.0s cast, range 8 circle
    ElementalCascade1 = 47199, // Helper->location, 7.0s cast, range 8 circle
    ElementalCascade2 = 47202, // Helper->location, 7.0s cast, range 8 circle
    ElementalCascade3 = 47203, // Helper->location, 7.0s cast, range 8 circle
    ElementalCascade4 = 47200, // Helper->location, 7.0s cast, range 8 circle

    ElementalCascadeCast = 47184, // PhantomHydra->self, 3.0s cast, single-target
    ElementalCascadeFire = 47188, // Helper->location, 3.0s cast, range 6 circle
    ElementalCascadePoison = 47185, // Helper->location, 3.0s cast, range 6 circle
    ElementalCascadeLightning = 47187, // Helper->location, 3.0s cast, range 6 circle
    ElementalCascadeLight = 47189, // Helper->location, 3.0s cast, range 6 circle
    ElementalCascadeIce = 47186, // Helper->location, 3.0s cast, range 6 circle

    ScarletThread = 47190, // 4BC7->self, 3.0s cast, range 70 width 4 rect - Fire orb
    Dissipate = 47193, // Helper->self, no cast, range 1 circle - Poison orb
    Shock = 47194, // Helper->location, 4.0s cast, range 10 circle - Lightning orb
    LevinRing = 47195, // Helper->location, 7.0s cast, range 10-20 donut - Lightning orb
    LevinRing1 = 47196, // Helper->location, 10.0s cast, range 20-30 donut - Lightning orb
    StunningSheen = 47191, // 4BC6->self, 5.0s cast, range 40 circle - Light orb
    IceBurst = 47192, // Helper->self, 3.0s cast, range 40 20.000-degree cone - Ice orb

    ManyHeadedBreathCast = 47213, // PhantomHydra->self, 8.0s cast, single-target
    ManyHeadedBreathVisual = 47212, // Helper->self, 1.0s cast, range 30 120.000-degree cone
    ManyHeadedBreathFront = 50673, // Helper->self, 0.8s cast, range 30 120.000-degree cone
    ManyHeadedBreathLeft = 50675, // Helper->self, 0.8s cast, range 30 120.000-degree cone
    ManyHeadedBreathRight = 50674, // Helper->self, 0.8s cast, range 30 120.000-degree cone
    ManyHeadedBreathVisual1 = 47205, // PhantomHydra->self, no cast, ???
    ManyHeadedBreathVisual2 = 47207, // PhantomHydra->self, no cast, ???
    ManyHeadedBreathVisual3 = 47206, // PhantomHydra->self, no cast, ???
    RadiantBreath = 47208, // PhantomHydra->self, no cast, single-target
}

sealed class ElementalCascade(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ElementalCascade, (uint)AID.ElementalCascade1,
            (uint)AID.ElementalCascade2, (uint)AID.ElementalCascade3, (uint)AID.ElementalCascade4], new AOEShapeCircle(8.0f));
sealed class Discordance(BossModule module) : Components.RaidwideCast(module, (uint)AID.Discordance);
sealed class ElementalCascadeElements(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.ElementalCascadeFire, (uint)AID.ElementalCascadePoison,
    (uint)AID.ElementalCascadeLightning, (uint)AID.ElementalCascadeLight, (uint)AID.ElementalCascadeIce ], new AOEShapeCircle(6.0f));
sealed class StunningSheen(BossModule module) : Components.CastGaze(module, (uint)AID.StunningSheen);

sealed class ScarletThread : Components.SimpleAOEs {
    public ScarletThread(BossModule module) : base(module, (uint)AID.ScarletThread, new AOEShapeRect(70.0f, 2.0f)) {
        Color = Colors.Danger;
    }
}

sealed class IceBurst : Components.SimpleAOEs {
    public IceBurst(BossModule module) : base(module, (uint)AID.IceBurst, new AOEShapeCone(40.0f, 10.0f.Degrees())) {
        Color = Colors.Danger;
    }
}

sealed class Shock(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.Shock) {
            aoes.Add(new(new AOEShapeCircle(10.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, risky: false));
        }

        if (spell.Action.ID == (uint)AID.LevinRing) {
            aoes.Add(new(new AOEShapeDonut(10.0f, 20.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, risky: false));
        }

        if (spell.Action.ID == (uint)AID.LevinRing1) {
            aoes.Add(new(new AOEShapeDonut(20.0f, 30.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, risky: false));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.Shock or (uint)AID.LevinRing or (uint)AID.LevinRing1) {
            if (aoes.Count > 0) {
                aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        int show = 0;
        var incomingAOEs = aoes.OrderBy(a => a.Activation).Take(2).ToList();
        foreach (ref var aoe in CollectionsMarshal.AsSpan(incomingAOEs)) {
            aoe.Color = show == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = show == 0;
            show++;
        }

        return CollectionsMarshal.AsSpan(incomingAOEs);
    }
}

sealed class ManyHeadedBreath(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];
    private readonly AOEShapeCone shape = new AOEShapeCone(30.0f, 60.0f.Degrees());

    public override void OnCastFinished(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID is (uint)AID.ManyHeadedBreathVisual) {
            aoes.Add(new(shape, spell.LocXZ, spell.Rotation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.ManyHeadedBreathFront or (uint)AID.ManyHeadedBreathLeft or (uint)AID.ManyHeadedBreathRight) {
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        int show = 0;
        var incomingAOEs = aoes.Take(2).ToList();
        foreach (ref var aoe in CollectionsMarshal.AsSpan(incomingAOEs)) {
            aoe.Color = show == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = show == 0;
            show++;
        }

        return CollectionsMarshal.AsSpan(incomingAOEs);
    }
}

sealed class Dissipate(BossModule module) : Components.Voidzone(module, 8.5f, module => module.Enemies((uint)OID.PoisonOrb).Where(z => z.EventState != 7)) {
    private bool active = false;

    public override void OnActorEAnim(Actor actor, uint state) {
        if (actor.OID == (uint)OID.PoisonOrb) {
            active = true;
        }
    }

    public override void OnActorDestroyed(Actor actor) {
        if (actor.OID == (uint)OID.PoisonOrb) {
            active = false;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        var aoes = new List<AOEInstance>();

        foreach (var source in Sources(Module)) {
            aoes.Add(new(Shape, source.Position, source.Rotation, color: active == true ? Colors.Danger : Colors.AOE));
        }
        return CollectionsMarshal.AsSpan(aoes);
    }
}

[SkipLocalsInit]
sealed class PhantomHydraStates : StateMachineBuilder {
    public PhantomHydraStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<ElementalCascade>()
            .ActivateOnEnter<Discordance>()
            .ActivateOnEnter<ElementalCascadeElements>()
            .ActivateOnEnter<ScarletThread>()
            .ActivateOnEnter<StunningSheen>()
            .ActivateOnEnter<IceBurst>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<Dissipate>()
            .ActivateOnEnter<ManyHeadedBreath>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(PhantomHydraStates),
    ConfigType = null, // replace null with typeof(PhantomHydraConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.PhantomHydra,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14523u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class PhantomHydra(WorldState ws, Actor primary) : BossModule(ws, primary, new(-82.000f, 485.000f), new ArenaBoundsCircle(20f));
