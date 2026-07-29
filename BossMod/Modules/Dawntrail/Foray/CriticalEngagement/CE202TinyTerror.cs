namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE202TinyTerror;

// TODO Improvements: Make knockback aoe easier to know the position off
// TODO first aoe showing annoying - shouldn't show AOE until it has been solved
// TODO 4 combo wave - can be knockback in between but flares act as if they're going off first

public enum OID : uint {
    TinyMage = 0x4C6D,
    Helper = 0x233C,
    TinyMageHelper = 0x4D55, // R1.000, x1
    TinyApprentice = 0x4C6E, // R1.000, x0 (spawn during fight)
    ArcaneSphereSmall = 0x4C74, // R1.000, x0 (spawn during fight)
    ArcaneSphereBig = 0x4C73, // R1.000, x0 (spawn during fight)
    FlareSphereGrow = 0x4C6F, // R0.700-1.904, x0 (spawn during fight)
    FlareSphere = 0x4C70, // R1.400, x0 (spawn during fight)
    HolySphere1Grow = 0x4C71, // R0.700-1.904, x0 (spawn during fight)
    HolySphere = 0x4C72, // R1.400, x0 (spawn during fight)
    _Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    _Gen_Actor1ec099 = 0x1EC099, // R0.500, x1, EventObj type
    _Gen_ = 0x4EBB, // R1.750, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 48305, // TinyMage->player, no cast, single-target
    TinyWarp = 48331, // TinyMage->location, no cast, single-target
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

    // TODO

    FlareSphereGrow = 0x4C6F, // R0.700-1.904, x0 (spawn during fight)
    FlareSphere = 0x4C70, // R1.400, x0 (spawn during fight)
    HolySphere1Grow = 0x4C71, // R0.700-1.904, x0 (spawn during fight)
    HolySphere = 0x4C72, // R1.400, x0 (spawn during fight)

    SmallForOne = 48306, // TinyMage->self, 3.0s cast, single-target - Spawns actors in

    TinyFlare = 48313, // 4C6F/4C70->self, no cast, single-target
    TinyFlare1 = 48311, // Helper->self, 2.0s cast, range 18 circle
    TinyHoly = 48314, // 4C72/4C71->self, no cast, single-target
    TinyHoly1 = 48312, // Helper->self, 2.0s cast, range 50 circle
    TinyHoly2 = 49058, // Helper->self, no cast, ???

    _Spell_Recharge = 48309, // 4C6E->self, 1.5s cast, single-target
    _Spell_Recharge1 = 48310, // 4C6E->self, 1.5s cast, single-target
    _Ability_Recharge = 49059, // 4C6E/TinyMage->self, no cast, single-target

    _Ability_ = 49057, // 4D55->self, no cast, range ?-25 donut
    _Ability_1 = 50530, // 4C6E->self, no cast, single-target
    _Spell_ = 50638, // 4C6E->self, no cast, single-target

    _Spell_ArcaneAggregation = 48307, // 4C6E->self, 3.0s cast, single-target
    _Spell_ArcaneAggregation1 = 49718, // 4C6E->self, 5.5s cast, single-target
    _Spell_ArcaneAggregation2 = 49719, // 4C6E->self, 5.5s cast, single-target
    _Spell_ArcaneAggregation3 = 48308, // 4C6E->self, 3.0s cast, single-target

    _Ability_AllForOne = 50762, // TinyMage->self, 3.0s cast, single-target
}

public enum SID : uint {
    _Gen_1 = 2552, // none->4C6E, extra=0x198
    _Gen_2 = 3445, // none->4C74/4C73, extra=0x15/0xA/0x1E
}

public enum TetherID : uint {
    OrbPairs = 415, // 4C72/4C70->4C72/4C70 - change name its when the orbs fire / white merge together
    _Gen_Tether_chn_m0012af = 60, // 4C74->4EBB
    CometTethers = 422, // 4C6E/TinyMage->4C74/4EBB
}

sealed class TinyThunderIII(BossModule module) : Components.RaidwideCast(module, (uint)AID.TinyThunderIIIRaidwide);

sealed class TinyQuake(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.TinyQuakeIIIInner) {
            aoes.Add(new(new AOEShapeCircle(10.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
        }

        if (spell.Action.ID == (uint)AID.TinyQuakeIIIMiddle) {
            aoes.Add(new(new AOEShapeDonut(10.0f, 20.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
        }

        if (spell.Action.ID == (uint)AID.TinyQuakeIIIOuter) {
            aoes.Add(new(new AOEShapeDonut(20.0f, 30.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.TinyQuakeIIIInner or (uint)AID.TinyQuakeIIIMiddle or (uint)AID.TinyQuakeIIIOuter) {
            aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        int show = 0;
        var currentAOEs = aoes.OrderBy(a => a.Activation).Take(2).ToList();

        foreach (ref var aoe in CollectionsMarshal.AsSpan(currentAOEs)) {
            aoe.Color = show == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = show == 0;
            show++;
        }

        return CollectionsMarshal.AsSpan(currentAOEs);
    }
}

sealed class DiminutiveDualcast(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.TinyBlizzardIII) {
            aoes.Add(new(new AOEShapeCone(40.0f, 30.0f.Degrees()), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
        }

        if (spell.Action.ID == (uint)AID.TinyFireIII) {
            aoes.Add(new(new AOEShapeCircle(14.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.TinyBlizzardIII or (uint)AID.TinyFireIII) {
            aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        var currentAOEs = aoes.OrderBy(a => a.Activation).Take(4).ToList();
        var waveTimer = currentAOEs.MinBy(a => a.Activation).Activation.AddSeconds(0.2f);

        foreach (ref var aoe in CollectionsMarshal.AsSpan(currentAOEs)) {
            if (aoe.Activation <= waveTimer) {
                aoe.Color = Colors.Danger;
                aoe.Risky = true;
            }
        }

        return CollectionsMarshal.AsSpan(currentAOEs);
    }
}

sealed class TinyMeteor(BossModule module) : Components.GenericAOEs(module, (uint)AID.TinyMeteor) {
    private List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.TinyMeteor) {
            aoes.Add(new(new AOEShapeCircle(6.0f), caster.Position, caster.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.TinyMeteor) {
            if (aoes.Count > 0) {
                aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 0.5f));
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

// TODO add timers - its not 60 seconds since it goes faster depending on the number of actors around it
// TODO _Gen_2 = 3445, // none->4C74/4C73, extra=0x15/0xA/0x1E
sealed class Comet(BossModule module) : BossComponent(module) {
    private List<CometActor> comets = new();

    private class CometActor {
        public Actor actor;
        public int tethers = 0;

        public CometActor(Actor actor) {
            this.actor = actor;
        }

        public void tetherIncrease() {
            tethers++;
        }
    }

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.ArcaneSphereSmall) {
            comets.Add(new CometActor(actor));
        }
    }

    public override void OnActorDeath(Actor actor) {
        if (actor.OID == (uint)OID.ArcaneSphereSmall) {
            var comet = comets.Find(a => a.actor.InstanceID == actor.InstanceID);
            if (comet != null) {
                comets.Remove(comet);
            }
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.CometTethers) {
            var target = WorldState.Actors.Find(tether.Target);
            if (target == null) {
                return;
            }

            var comet = comets.Find(a => a.actor.InstanceID == target.InstanceID);
            if (comet == null) {
                return;
            }

            comet.tetherIncrease();
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc) {
        if (comets.Count == 0) {
            return;
        }

        var firstComet = comets.MaxBy(a => a.tethers);
        if (firstComet == null) {
            return;
        }

        Arena.ZoneCircleOutline(firstComet.actor.Position, 2.0f, Colors.Safe, 2.0f);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        if (comets.Count == 0) {
            return;
        }

        hints.Add("Attack the comet with the green circle around it!");
    }
}

sealed class FlareGrowable(BossModule module) : Components.GenericAOEs(module) {
    private List<Actor> mages = [];
    private Actor? orb = null;
    private int startPoint = -1;
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

        if (actor.OID == (uint)OID.FlareSphereGrow) {
            orb = actor;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.TinyFlare1) {
            orb = null;
            startPoint = -1;
            direction = 0;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        if (mages.Count == 0 || orb == null) {
            return [];
        }

        if (startPoint == -1) {
            var startAOE = mages.FindIndex(a => a.Position.AlmostEqual(orb.Position, 0.5f));
            if (startAOE < 0) {
                return [];
            }

            startPoint = startAOE;
        }

        if (direction == 0) {
            var currentPoint = mages.FindIndex(a => a.Position.AlmostEqual(orb.Position, 0.5f));
            if (currentPoint >= 0 && currentPoint != startPoint) {
                direction = (currentPoint - startPoint + 4) % 4 == 1 ? 1 : -1;
            }
        }

        var targetPoint = (startPoint + 4 - direction) % 4;
        var targetActor  = mages[targetPoint];
        return new AOEInstance[1] { new AOEInstance(new AOEShapeCircle(18.0f), targetActor.Position) };
    }
}

sealed class HolyGrowable(BossModule module) : Components.GenericKnockback(module) {
    private List<Actor> mages = [];
    private Actor? orb = null;
    private int startPoint = -1;
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

        if (actor.OID == (uint)OID.HolySphere1Grow) {
            orb = actor;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.TinyHoly1) {
            orb = null;
            startPoint = -1;
            direction = 0;
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) {
        if (mages.Count == 0 || orb == null) {
            return [];
        }

        if (startPoint == -1) {
            var startAOE = mages.FindIndex(a => a.Position.AlmostEqual(orb.Position, 0.5f));
            if (startAOE < 0) {
                return [];
            }

            startPoint = startAOE;
        }

        if (direction == 0) {
            var currentPoint = mages.FindIndex(a => a.Position.AlmostEqual(orb.Position, 0.5f));
            if (currentPoint >= 0 && currentPoint != startPoint) {
                direction = (currentPoint - startPoint + 4) % 4 == 1 ? 1 : -1;
            }
        }

        var targetPoint = (startPoint + 4 - direction) % 4;
        var targetActor  = mages[targetPoint];
        return new Knockback[1] { new (targetActor.Position, 15.0f) };
    }
}

// TODO seem to be placed on movement tell?
// TODO they all start moving at the same time, its just distance base - rewrite this whole function to work base on this
// TODO turn stuff like this into a base class that can be used by it type easier - look at EX fight of final day to see how they did it?
// TODO combine with holy, so we only show the first two in the list -> flare, holy, flare, flare should show flare & holy
sealed class FlareCombo(BossModule module) : Components.GenericAOEs(module) {
    private List<(AOEInstance aoe, float distance)> aoes = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.OrbPairs && source.OID == (uint)OID.FlareSphere) {
            var orbTarget = WorldState.Actors.Find(tether.Target);
            if (orbTarget != null) {
                var midPoint = WPos.Lerp(source.Position, orbTarget.Position, 0.5f);
                var distance = (orbTarget.Position - source.Position).Length();
                aoes.Add((new AOEInstance(new AOEShapeCircle(18.0f), midPoint), distance));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.TinyFlare1 || spell.Action.ID == (uint)AID.TinyHoly1) {
            if (aoes.Count > 0) {
                aoes.RemoveAll(aoe => aoe.aoe.Origin.AlmostEqual(caster.Position, 0.5f));
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        if (aoes.Count == 0) {
            return [];
        }

        int show = 0;
        var upcomingAOEs = aoes.OrderBy(a => a.distance).Select(a => a.aoe).Take(2).ToList();
        foreach (ref var aoe in CollectionsMarshal.AsSpan(upcomingAOEs)) {
            aoe.Color = show == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = show == 0;
            show++;
        }

        return CollectionsMarshal.AsSpan(upcomingAOEs);
    }
}

sealed class HolyCombo(BossModule module) : Components.GenericKnockback(module) {
    private List<(WPos origin, float distance)> knockbacks = [];

    public override void OnTethered(Actor source, in ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.OrbPairs && source.OID == (uint)OID.HolySphere) {
            var orbTarget = WorldState.Actors.Find(tether.Target);
            if (orbTarget != null) {
                var midPoint = WPos.Lerp(source.Position, orbTarget.Position, 0.5f);
                var distance = (orbTarget.Position - source.Position).Length();
                knockbacks.Add((midPoint, distance));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.TinyFlare1 || spell.Action.ID == (uint)AID.TinyHoly1) {
            if (knockbacks.Count > 0) {
                knockbacks.RemoveAll(knockback => knockback.origin.AlmostEqual(caster.Position, 0.5f));
            }
        }
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) {
        if (knockbacks.Count == 0) {
            return [];
        }

        var knockbackIncoming = knockbacks.MinBy(knockback => knockback.distance);
        return new Knockback[1] {new(knockbackIncoming.origin, 15.0f)};
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
            .ActivateOnEnter<HolyGrowable>()
            .ActivateOnEnter<FlareGrowable>()
            .ActivateOnEnter<FlareCombo>()
            .ActivateOnEnter<HolyCombo>();
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
