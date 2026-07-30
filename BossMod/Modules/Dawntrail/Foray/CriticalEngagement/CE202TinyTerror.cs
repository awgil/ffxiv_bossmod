namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE202TinyTerror;

public enum OID : uint {
    TinyMage = 0x4C6D,
    Helper = 0x233C,
    TinyMageHelper = 0x4D55, // R1.000, x1
    TinyApprentice = 0x4C6E, // R1.000, x0 (spawn during fight)
    ArcaneSphereSmall = 0x4C74, // R1.000, x0 (spawn during fight)
    ArcaneSphereBig = 0x4C73, // R1.000, x0 (spawn during fight)
    FlareSphereGrow = 0x4C6F, // R0.700-1.904, x0 (spawn during fight)
    FlareSphere = 0x4C70, // R1.400, x0 (spawn during fight)
    HolySphereGrow = 0x4C71, // R0.700-1.904, x0 (spawn during fight)
    HolySphere = 0x4C72, // R1.400, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 48305, // TinyMage->player, no cast, single-target
    TinyWarp = 48331, // TinyMage->location, no cast, single-target
    SmallForOne = 48306, // TinyMage->self, 3.0s cast, single-target
    AllForOne = 50762, // TinyMage->self, 3.0s cast, single-target

    ArcaneAggregation = 48307, // 4C6E->self, 3.0s cast, single-target
    ArcaneAggregation1 = 49718, // 4C6E->self, 5.5s cast, single-target
    ArcaneAggregation2 = 49719, // 4C6E->self, 5.5s cast, single-target
    ArcaneAggregation3 = 48308, // 4C6E->self, 3.0s cast, single-target

    TinyThunderIIIRaidwide = 48329, // TinyMage->self, 5.0s cast, single-target
    TinyThunderIII = 48330, // Helper->self, no cast, ???

    TinyQuakeIII = 48322, // TinyMage->self, 3.5+0.5s cast, single-target
    TinyQuakeIIIInner = 48323, // Helper->self, 4.0s cast, range 10 circle
    TinyQuakeIIIMiddle = 48324, // Helper->self, 4.0s cast, range 10-20 donut
    TinyQuakeIIIOuter = 48325, // Helper->self, 4.0s cast, range 20-30 donut

    DiminutiveDualcast = 48317, // TinyMage->self, 5.5+0.5s cast, single-target
    TinyBlizzardIII = 48319, // Helper->self, 6.0s cast, range 40 60.000-degree cone
    TinyFireIII = 48318, // Helper->self, 6.0s cast, range 14 circle

    TinyMeteorCast = 48320, // TinyMage->self, 5.0s cast, single-target
    TinyMeteor = 48321, // Helper->location, 4.0s cast, range 6 circle

    Comet = 48327, // 4C74->self, 60.0s cast, range 60 circle
    Comet1 = 49061, // Helper->self, no cast, ???
    Meteor = 48326, // 4C73->self, 130.0s cast, single-target

    TinyFlare = 48313, // 4C6F/4C70->self, no cast, single-target
    TinyFlare1 = 48311, // Helper->self, 2.0s cast, range 18 circle
    TinyHoly = 48314, // 4C72/4C71->self, no cast, single-target
    TinyHoly1 = 48312, // Helper->self, 2.0s cast, range 50 circle
    TinyHoly2 = 49058, // Helper->self, no cast, ???

    Recharge = 48309, // 4C6E->self, 1.5s cast, single-target
    Recharge1 = 48310, // 4C6E->self, 1.5s cast, single-target
    Recharge2 = 49059, // 4C6E/TinyMage->self, no cast, single-target

    Ability = 49057, // 4D55->self, no cast, range ?-25 donut
    Ability1 = 50530, // 4C6E->self, no cast, single-target
    Spell1 = 50638, // 4C6E->self, no cast, single-target
}

public enum SID : uint {
    Gen1 = 2552, // none->4C6E, extra=0x198
    Gen2 = 3445, // none->4C74/4C73, extra=0x15/0xA/0x1E
}

public enum TetherID : uint {
    FlareHolyMergeTether = 415, // 4C72/4C70->4C72/4C70
    ArcaneSphereTether = 422, // 4C6E/TinyMage->4C74/4EBB
    CometMeteorTether = 60, // 4C74->4EBB
}

sealed class TinyThunderIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.TinyThunderIIIRaidwide);

sealed class TinyQuake(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> storedAOEs = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.TinyQuakeIIIInner) {
            storedAOEs.Add(new(new AOEShapeCircle(10.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, risky: false));
        }

        if (spell.Action.ID == (uint)AID.TinyQuakeIIIMiddle) {
            storedAOEs.Add(new(new AOEShapeDonut(10.0f, 20.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, risky: false));
        }

        if (spell.Action.ID == (uint)AID.TinyQuakeIIIOuter) {
            storedAOEs.Add(new(new AOEShapeDonut(20.0f, 30.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID, risky: false));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.TinyQuakeIIIInner or (uint)AID.TinyQuakeIIIMiddle or (uint)AID.TinyQuakeIIIOuter) {
            storedAOEs.Sort((a, b) => a.Activation.CompareTo(b.Activation));
            if (storedAOEs.Count > 0) {
                storedAOEs.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        int show = 0;
        var aoes = storedAOEs.OrderBy(a => a.Activation).Take(2).ToList();
        foreach (ref var aoe in CollectionsMarshal.AsSpan(aoes)) {
            aoe.Color = show == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = show == 0;
            show++;
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class DiminutiveDualcast(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> storedAOEs = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.TinyBlizzardIII) {
            storedAOEs.Add(new(new AOEShapeCone(40.0f, 30.0f.Degrees()), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }

        if (spell.Action.ID == (uint)AID.TinyFireIII) {
            storedAOEs.Add(new(new AOEShapeCircle(14.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.TinyBlizzardIII or (uint)AID.TinyFireIII) {
            if (storedAOEs.Count > 0) {
                storedAOEs.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        var aoes = storedAOEs.OrderBy(a => a.Activation).Take(4).ToList();
        var waveTimer = aoes.MinBy(a => a.Activation).Activation.AddSeconds(0.2f);

        foreach (ref var aoe in CollectionsMarshal.AsSpan(aoes)) {
            if (aoe.Activation <= waveTimer) {
                aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class TinyMeteor(BossModule module) : Components.GenericAOEs(module, (uint)AID.TinyMeteor) {
    private List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.TinyMeteor) {
            aoes.Add(new(new AOEShapeCircle(6.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.TinyMeteor) {
            if (aoes.Count > 0) {
                aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        var waveTimer = aoes.MinBy(a => a.Activation).Activation.AddSeconds(0.2f);

        foreach (ref var aoe in CollectionsMarshal.AsSpan(aoes)) {
            if (aoe.Activation <= waveTimer) {
                aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class Comet(BossModule module) : BossComponent(module) {
    private List<ArcaneSphere> arcaneSpheres = [];

    private class ArcaneSphere {
        public Actor arcaneSphere;
        public int tethers;

        public ArcaneSphere(Actor actor) {
            arcaneSphere = actor;
            tethers = 0;
        }
    }

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.ArcaneSphereSmall) {
            arcaneSpheres.Add(new ArcaneSphere(actor));
        }
    }

    public override void OnActorDeath(Actor actor) {
        if (actor.OID == (uint)OID.ArcaneSphereSmall) {
            var sphere = arcaneSpheres.Find(a => a.arcaneSphere.InstanceID == actor.InstanceID);
            if (sphere != null) {
                arcaneSpheres.Remove(sphere);
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.ArcaneSphereTether) {
            var target = WorldState.Actors.Find(tether.Target);
            if (target == null) {
                return;
            }

            var sphere = arcaneSpheres.Find(a => a.arcaneSphere.InstanceID == target.InstanceID);
            if (sphere != null) {
                sphere.tethers++;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (arcaneSpheres.Count == 0) {
            return;
        }

        var firstArcaneSphere = arcaneSpheres.MaxBy(a => a.tethers);
        if (firstArcaneSphere != null) {
            Arena.ZoneCircleOutline(firstArcaneSphere.arcaneSphere.Position, 2.0f, Colors.Safe, 2.0f);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (arcaneSpheres.Count == 0) {
            return;
        }

        hints.Add("Attack the arcane sphere with the green circle around it!", false);
    }
}

sealed class FlareHolyMerge(BossModule module) : BossComponent(module) {
    private static readonly AOEShapeCircle flareShape = new (18.0f);
    private const float holyKnockBackDistance = 15.0f;

    private readonly record struct MergeCombination(WPos origin, float distance, bool isFlare);
    private readonly List<MergeCombination> mergeCombinations = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.FlareHolyMergeTether) {
            var sphere = WorldState.Actors.Find(tether.Target);
            if (sphere != null) {
                var midPoint = WPos.Lerp(source.Position, sphere.Position, 0.5f);
                var distance = (source.Position - sphere.Position).Length();
                mergeCombinations.Add(new(midPoint, distance, source.OID == (uint)OID.FlareSphere));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.TinyFlare or (uint)AID.TinyHoly1) {
            if (mergeCombinations.Count > 0) {
                mergeCombinations.RemoveAll(c => c.origin.AlmostEqual(caster.Position, 0.5f));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (mergeCombinations.Count == 0) {
            return;
        }

        var nextCombinations = mergeCombinations.OrderBy(c => c.distance).Take(2).ToList();

        for (int i = 0; i < nextCombinations.Count; i++) {
            var combination = nextCombinations[i];
            if (combination.isFlare) {
                flareShape.Draw(Arena, combination.origin, default, i == 0 ? Colors.Danger : Colors.AOE);
            }

            if (!combination.isFlare) {
                var endPoint = Components.GenericKnockback.AwayFromSource(pc.Position, combination.origin, holyKnockBackDistance);
                Components.GenericKnockback.DrawKnockback(pc, endPoint, Arena);
            }
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc) {
        if (mergeCombinations.Count == 0) {
            return;
        }

        var nextCombinations = mergeCombinations.OrderBy(c => c.distance).Take(2).ToList();

        foreach (var combination in nextCombinations) {
            if (!combination.isFlare) {
                Arena.ZoneCircle(combination.origin, 2.0f, Colors.Other7);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (mergeCombinations.Count == 0) {
            return;
        }

        var nextCombinations = mergeCombinations.OrderBy(c => c.distance).Take(2).ToList();
        foreach (var combination in nextCombinations) {
            if (combination.isFlare && flareShape.Check(actor.Position, combination.origin, default)) {
                hints.Add("GTFO from aoe!");
            }

            if (!combination.isFlare) {
                var endPoint = Components.GenericKnockback.AwayFromSource(actor.Position, combination.origin, holyKnockBackDistance);
                if (!Arena.InBounds(endPoint)) {
                    hints.Add("About to be knocked into wall!");
                }
            }
        }
    }
}

sealed class SphereGrowable(BossModule module) : BossComponent(module) {
    private static readonly AOEShapeCircle flareShape = new (18.0f);
    private const float holyKnockBackDistance = 15.0f;
    private List<Actor> mages = [];
    private Actor? orb = null;
    private int startIndex = -1;
    private int direction = 0;

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.TinyApprentice) {
            mages.Add(actor);

            if (mages.Count == 4) {
                mages.Sort(delegate (Actor x,  Actor y) {
                    var north = Angle.AnglesCardinals[2];
                    var xAngle = (x.Position - Arena.Center).ToAngle();
                    var yAngle = (y.Position - Arena.Center).ToAngle();

                    var xDeg = xAngle.AlmostEqual(north, 0.01f) ? 180f : xAngle.Deg;
                    var yDeg = yAngle.AlmostEqual(north, 0.01f) ? 180f : yAngle.Deg;

                    return xDeg < yDeg ? 1 : -1;
                });
            }
        }

        if (actor.OID is (uint)OID.FlareSphereGrow or (uint)OID.HolySphereGrow) {
            orb = actor;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.TinyFlare1 or (uint)AID.TinyHoly1) {
            orb = null;
            startIndex = -1;
            direction = 0;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        var target = SolveExplosionMage();

        if (target == null || orb == null) {
            return;
        }

        if (orb.OID == (uint)OID.FlareSphereGrow) {
            flareShape.Draw(Arena, target.Position);
        }

        if (orb.OID == (uint)OID.HolySphereGrow) {
            var endPoint = Components.GenericKnockback.AwayFromSource(pc.Position, target.Position, holyKnockBackDistance);
            Components.GenericKnockback.DrawKnockback(pc, endPoint, Arena);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc) {
        var target = SolveExplosionMage();

        if (target == null || orb == null) {
            return;
        }

        if (orb.OID == (uint)OID.HolySphereGrow) {
            Arena.ZoneCircle(target.Position, 2.0f, Colors.Other7);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        var target = SolveExplosionMage();

        if (target == null || orb == null) {
            return;
        }

        if (orb.OID == (uint)OID.FlareSphereGrow && flareShape.Check(actor.Position, target.Position, default)) {
            hints.Add("GTFO from aoe!");
        }

        if (orb.OID == (uint)OID.HolySphereGrow) {
            var endPoint = Components.GenericKnockback.AwayFromSource(actor.Position, target.Position, holyKnockBackDistance);
            if (!Arena.InBounds(endPoint)) {
                hints.Add("About to be knocked into wall!");
            }
        }
    }

    private Actor? SolveExplosionMage() {
        if (mages.Count == 0 || orb == null) {
            return null;
        }

        if (startIndex == -1) {
            var startAOE = mages.FindIndex(a => a.Position.AlmostEqual(orb.Position, 0.5f));
            if (startAOE < 0) {
                return null;
            }

            startIndex = startAOE;
        }

        if (direction == 0) {
            var currentIndex = mages.FindIndex(a => a.Position.AlmostEqual(orb.Position, 0.5f));
            if (currentIndex >= 0 && currentIndex != startIndex) {
                direction = (currentIndex - startIndex + 4) % 4 == 1 ? 1 : -1;
            } else {
                return null;
            }
        }

        return mages[(startIndex + 4 - direction) % 4];
    }
}

[SkipLocalsInit]
sealed class TinyMageStates : StateMachineBuilder {
    public TinyMageStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<TinyThunderIII>()
            .ActivateOnEnter<TinyQuake>()
            .ActivateOnEnter<DiminutiveDualcast>()
            .ActivateOnEnter<TinyMeteor>()
            .ActivateOnEnter<Comet>()
            .ActivateOnEnter<FlareHolyMerge>()
            .ActivateOnEnter<SphereGrowable>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(TinyMageStates),
    ConfigType = null, // replace null with typeof(TinyMageConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = null, // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.TinyMage,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14795u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class TinyMage(WorldState ws, Actor primary) : BossModule(ws, primary, new(152.000f, 716.000f), new ArenaBoundsCircle(20f)) {
    protected override void DrawEnemies(int pcSlot, Actor pc) {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.ArcaneSphereSmall));
        Arena.Actors(Enemies((uint)OID.ArcaneSphereBig));
    }
}
