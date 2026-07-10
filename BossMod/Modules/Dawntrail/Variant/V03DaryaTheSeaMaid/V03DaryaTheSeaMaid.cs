namespace BossMod.Dawntrail.Variant.V03DaryaSeaMaid;

class PiercingPlunge(BossModule module) : Components.RaidwideCast(module, AID.PiercingPlunge);

class FamiliarCall(BossModule module) : Components.GenericAOEs(module) {
    protected record struct Caster(Actor source, AOEShape shape, DateTime activation);
    protected readonly List<List<Caster>> sources = [];
    protected readonly List<String> vfxOrder = []; // Stores an easy-to-read name for callouts and the animal OID for aoes

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.FamiliarCall) {
            vfxOrder.Clear();
            sources.Clear();
        }
    }

    public override void OnEventVFX(Actor actor, uint vfxID, ulong targetID) {
        if (actor.OID == (uint)OID.DaryaSeaMaid) {
            var (name, animal) = (VfxID)vfxID switch {
                VfxID.Crab => ("Crab", OID.SeabornSoldier),
                VfxID.Horse => ("Horse", OID.SeabornSteed),
                VfxID.Turtle => ("Turtle", OID.SeabornSteward),
                VfxID.Stalwart => ("Stalwart", OID.SeabornStalwart),
                _ => ("null", default)
            };

            if (animal == default) {
                return;
            }

            var activation = sources.Count > 0 ? sources[^1][0].activation.AddSeconds(3.0f) : WorldState.FutureTime(11.6f);
            sources.Add([..Module.Enemies(animal).Select(c => new Caster(c, new AOEShapeRect(40f, 4f), activation))]);
            vfxOrder.Add(name);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Watersong || spell.Action.ID == (uint)AID.Watersong1 ||
            spell.Action.ID == (uint)AID.Watersong2 || spell.Action.ID == (uint)AID.Watersong3) {

            if (sources.Count > 0) {
                sources[0].RemoveAll(c => c.source == caster);
                if (sources[0].Count == 0) {
                    NumCasts++;
                    sources.RemoveAt(0);

                    if (vfxOrder.Count > 0) {
                        vfxOrder.RemoveAt(0);
                    }
                }
            }
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        int shown = 0;
        foreach (var srcs in sources.Take(2)) {
            foreach (var src in srcs) {
                yield return new(src.shape, src.source.Position, src.source.Rotation, src.activation, shown == 0 ? ArenaColor.Danger : ArenaColor.AOE, shown == 0);
            }
            shown++;
        }
    }

    public override void AddGlobalHints(GlobalHints hints) {
        if (vfxOrder.Count > 0) {
            hints.Add("Order: " + string.Join(", ", vfxOrder));
        }
    }
}

class SunkenTreasure(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> sources = [];

    public override void OnActorEAnim(Actor actor, uint state) {
        if (state == 1048608) {
            if (actor.OID == (uint)OID.BlueSphere) {
                sources.Add(new(new AOEShapeCircle(18f), actor.Position, actor.Rotation, WorldState.FutureTime(7.2f)));
            }

            if (actor.OID == (uint)OID.DonutSphere) {
                sources.Add(new(new AOEShapeDonut(4f, 20f), actor.Position, actor.Rotation, WorldState.FutureTime(7.2f)));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.SphereShatter || spell.Action.ID == (uint)AID.SphereShatter1) {
            NumCasts++;
            sources.RemoveAll(a => a.Origin.AlmostEqual(caster.Position, 1.0f));
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        int shown = 0;
        foreach (var source in sources) {
            if (shown >= 5) {
                break;
            }

            yield return new(source.Shape, source.Origin, source.Rotation, source.Activation);
            shown++;
        }
    }
}

class Hydrobullet(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Hydrobullet, AID.HydrobulletSpread, 15.0f, 5.0f) {
    private AlluringOrderForcedMovement alluringOrderForcedMovement = module.FindComponent<AlluringOrderForcedMovement>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (alluringOrderForcedMovement.active == true) {
            return;
        }

        var actorSpread = Spreads.FirstOrDefault(s => s.Target == actor);
        if (actorSpread.Target != null) {
            foreach (var p in Raid.WithoutSlot().Exclude(actor).Where(p => Spreads.Any(s => s.Target == p))) {
                hints.AddForbiddenZone(ShapeContains.Circle(p.Position, actorSpread.Radius), actorSpread.Activation);
            }
        }
    }
}

class Hydrocannon(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70f, 3f), (uint)IconID.Hydrocannon, AID.Hydrocannon1, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster);

class AquaSpear(BossModule module) : Components.StandardAOEs(module, AID.AquaSpear1, new AOEShapeRect(4f, 4f, 4f)) {
    private List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.AquaSpear) {
            aoes.Clear();
        }

        if (spell.Action == WatchedAction) {
            aoes.Add(new AOEInstance(Shape, caster.Position, caster.Rotation));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state) {
        if (state == 0x00040008) {
            aoes.RemoveAll(t => t.Origin.AlmostEqual(actor.Position, 1.0f));
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        foreach (var aoe in aoes) {
            yield return aoe;
        }
    }
}

class SeaShackles(BossModule module) : BossComponent(module) {
    private readonly List<(Actor, Actor)> _distant = [];
    public record struct Assignment(Role Partner, int PartnerSlot, bool tooClose);
    public Assignment[] Assignments = new Assignment[PartyState.MaxPartySize];
    private bool active = false; // Game instance doesn't delete tethers on wipes, so we have to guard against them
    private int NumCasts = 0;
    private DateTime activation = DateTime.MaxValue;

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.SeaShackles) {
            active = true;
            activation = WorldState.FutureTime(4.0f);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether) {
        if (active == false) {
            return;
        }

        if (tether.ID == (uint)TetherID.SeaShackles) {
            Assign(source, WorldState.Actors.Find(tether.Target)!, true);
        }

        if (tether.ID == (uint)TetherID.SeaShacklesSafe) {
            Assign(source, WorldState.Actors.Find(tether.Target)!, false);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.SeaShackles || tether.ID == (uint)TetherID.SeaShacklesSafe) {
            Assignments[Raid.FindSlot(source.InstanceID)] = default;
            _distant.RemoveAll(t => t.Item1.InstanceID == source.InstanceID || t.Item2.InstanceID == source.InstanceID);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status) {
        if (status.ID == (uint)SID.NearShoreShackles) {
            Assignments[Raid.FindSlot(actor.InstanceID)] = default;
            _distant.RemoveAll(t => t.Item1.InstanceID == actor.InstanceID || t.Item2.InstanceID == actor.InstanceID);
            NumCasts++;

            if (NumCasts == Raid.WithoutSlot().Count()) {
                active = false;
            }

        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc) {
        foreach (var (a, b) in _distant) {
            if (a.InstanceID != pc.InstanceID && b.InstanceID != pc.InstanceID) {
                continue;
            }

            var assign = Assignments[pcSlot];
            var colour = assign.tooClose ? ArenaColor.Danger : ArenaColor.Safe;
            if (assign.Partner != Role.None) {
                if (assign.tooClose == true) {
                    Arena.AddLine(a.Position, b.Position, colour);
                }

                if (assign.tooClose == false) {
                    Arena.AddLine(a.Position, b.Position, colour);
                }
            }

            Arena.Actor(a, colour);
            Arena.Actor(b, colour);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints) {
        var assign = Assignments[slot];
        if (assign.Partner != Role.None) {
            if (assign.tooClose == true) {
                hints.Add("Stay far from partner!", true);
            }

            if (assign.tooClose == false) {
                hints.Add("Stay far from partner!", false);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);

        foreach (var (a, b) in _distant) {
            if (a.InstanceID == actor.InstanceID || b.InstanceID == actor.InstanceID) {
                if (a.InstanceID == actor.InstanceID) {
                    hints.AddForbiddenZone(ShapeContains.Circle(b.Position, 13.0f), activation);
                }

                if (b.InstanceID == actor.InstanceID) {
                    hints.AddForbiddenZone(ShapeContains.Circle(a.Position, 13.0f), activation);
                }
            }
        }
    }

    private void Assign(Actor a, Actor b, bool tooClose) {
        var aslot = Raid.FindSlot(a.InstanceID);
        var bslot = Raid.FindSlot(b.InstanceID);
        Assignments[aslot] = new(b.Role, bslot, tooClose);
        Assignments[bslot] = new(a.Role, aslot, tooClose);
        _distant.Add((a, b));
    }
}

// Arena is 40.0f, and it knocks you back slightly over 3 full squares so 24.0f + a tiny bit
class TidalWave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TidalWave1, 25.0f, kind: Kind.DirForward) {
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var src in Sources(slot, actor)) {
            if (!IsImmune(slot, src.Activation)) {
                var tiles = Module.FindComponent<AquaSpear>()!.ActiveAOEs(slot, actor).Select(a => (a.Origin, a.Rotation)).ToList();
                var partnerAssigned = Module.FindComponent<SeaShackles>()!.Assignments[slot];
                var partner = partnerAssigned.Partner != Role.None ? Raid[partnerAssigned.PartnerSlot] : null;

                if (partner == null) {
                    continue;
                }

                var direction = src.Direction.ToDirection().Normalized();
                var center = Arena.Center;
                var bounds = Arena.Bounds;

                hints.AddForbiddenZone(p => {
                    var proj = p + direction * 25.0f;

                    if (!bounds.Contains(proj - center)) {
                        return true;
                    }

                    if (Intersect.RayCircle(p, direction, partner.Position, 10.0f) <= 1000f) {
                        return true;
                    }

                    foreach (var tile in tiles) {
                        if (Intersect.RayRect(p, direction, tile.Origin, tile.Rotation.ToDirection(), 4f, 4f) <= 25.0f) {
                            return true;
                        }
                    }

                    return false;
                }, src.Activation);
            }
        }
    }
}

class SwimmingInTheAir(BossModule module) : Components.GenericAOEs(module) {
    public List<AOEInstance> aoes = [];

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.SwimmingInTheAirOrb) {
            aoes.Add(new(new AOEShapeCircle(12f), actor.Position, actor.Rotation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Hydrofall) {
           aoes.Clear();
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        foreach (var aoe in aoes) {
            yield return aoe;
        }
    }
}

class SwimingInTheAirStackSpread(BossModule module) : Components.IconStackSpread(module, (uint)IconID.Tidalspout, (uint)IconID.Hydrobullet2, AID.Tidalspout, AID.HydrobulletSpread2, 6.0f, 15.0f, 6.0f, 2, 3, true) {
    private SwimmingInTheAir? swimmingInTheAir = module.FindComponent<SwimmingInTheAir>();
    private WPos? safeSpot;

    private WPos[] possibleSafeSpots = [
        new(363.000f, 518.000f), // NW
        new(387.000f, 518.000f), // NE
        new(363.000f, 542.000f), // SW
        new(387.000f, 542.000f) // SE
    ];

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        base.OnEventCast(caster, spell);

        if (spell.Action.ID == (uint)AID.Tidalspout) {
            safeSpot = null;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);

        if (safeSpot != null) {
            if (!IsStackTarget(actor) && !IsSpreadTarget(actor)) {
                hints.GoalZones.Add(hints.GoalProximity(safeSpot.Value, 6.0f, 1000.0f));
            }
        }
    }

    public override void Update() {
        var stackMask = new BitMask();

        if (swimmingInTheAir == null) {
            return;
        }

        if (Stacks.Count > 0) {
            var stack = Stacks[0];

            foreach (var (slot, player) in Raid.WithSlot()) {
                if (!IsStackTarget(player) && !IsSpreadTarget(player)) {
                    stackMask.Set(slot);
                }
            }

            stack.ForbiddenPlayers = ~stackMask;
            Stacks[0] = stack;
            safeSpot = possibleSafeSpots.Where(p => !swimmingInTheAir.aoes.Any(o => o.Shape.Check(p, o.Origin, o.Rotation))).MinBy(p => (p - stack.Target.Position).LengthSq());
        }
    }
}

class CeaselessCurrent(BossModule module) : Components.Exaflare(module, new AOEShapeRect(8.0f, 20.0f)) {
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        foreach (var (c, t, r) in FutureAOEs()) {
            yield return new(Shape, c, r, t, FutureColor, Risky: false);
        }

        foreach (var (c, t, r) in ImminentAOEs()) {
            yield return new(Shape, c, r, t, ImminentColor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if ((AID)spell.Action.ID == AID.CeaselessCurrentFirst) {
            Lines.Add(new() {
                Next = spell.LocXZ,
                Advance = spell.Rotation.ToDirection() * 8,
                Rotation = spell.Rotation.ToDirection().Rounded().ToAngle(),
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.1f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if ((AID)spell.Action.ID is AID.CeaselessCurrentFirst or AID.CeaselessCurrentRest) {
            NumCasts++;
            var ix = Lines.FindIndex(l => l.Rotation.AlmostEqual(caster.Rotation, 0.1f));
            if (ix >= 0) {
                AdvanceLine(Lines[ix], Lines[ix].Next);
            }
        }
    }
}

class SurgingCurrent(BossModule module) : Components.StandardAOEs(module, AID.SurgingCurrent1, new AOEShapeCone(60.0f, 45.0f.Degrees()), highlightImminent: true);

class AlluringOrderRaidwide(BossModule module) : Components.RaidwideCast(module, AID.AlluringOrder);

class AlluringOrderForcedMovement(BossModule module) : Components.StatusDrivenForcedMarch(module, 3.0f, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace) {
    public bool active = false;

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.AlluringOrder) {
            active = true;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status) {
        base.OnStatusLose(actor, status);

        if (status.ID == (uint)SID.ForcedMarch) {
            active = false;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        if (active == false) {
            return;
        }

        base.AddAIHints(slot, actor, assignment, hints);

        var state = State.GetValueOrDefault(actor.InstanceID);
        if (state == null || state.PendingMoves.Count == 0) {
            return;
        }
        var dangerousPositions = Raid.WithoutSlot().Exclude(actor).Where(HasForcedMovements).Select(a => ForcedMovements(a).Last().to).ToList();

        var direction = (actor.Rotation + state.PendingMoves[0].dir).ToDirection();
        var distance = MovementSpeed * state.PendingMoves[0].duration;
        var bounds = Arena.Bounds;
        var center = Arena.Center;

        hints.AddForbiddenZone(p => {
            if (!bounds.Contains(p + distance * direction - center)) {
                return true;
            }

            if (dangerousPositions.Any(d => (p + distance * direction).InCircle(d, 15.0f))) {
                return true;
            }

            return false;
        }, state.PendingMoves[0].activation);
    }
}

class AquaBall(BossModule module) : Components.StandardAOEs(module, AID.AquaBall1, new AOEShapeCircle(5f));

class RecedingTwinTides(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.NearTideReceding) {
            var activation = Module.CastFinishAt(spell);
            aoes.Add(new(new AOEShapeCircle(10f), caster.Position, caster.Rotation, activation, Risky: true));
            aoes.Add(new(new AOEShapeDonut(10.0f, 40.0f), caster.Position, caster.Rotation, WorldState.FutureTime(spell.RemainingTime + 3.2f), Risky: false));
        }

        if (spell.Action.ID == (uint)AID.FarTideEncroaching) {
            var activation = Module.CastFinishAt(spell);
            aoes.Add(new(new AOEShapeDonut(10.0f, 40.0f), caster.Position, caster.Rotation, activation, Risky: true));
            aoes.Add(new(new AOEShapeCircle(10f), caster.Position, caster.Rotation, WorldState.FutureTime(spell.RemainingTime + 3.2f), Risky: false));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.NearTideReceding || spell.Action.ID == (uint)AID.FarTideReceding ||
            spell.Action.ID == (uint)AID.NearTideEncroaching || spell.Action.ID == (uint)AID.FarTideEncroaching) {
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);
            }
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        int shown = 0;
        foreach (var aoe in aoes) {
            uint colour = shown == 0 ? ArenaColor.Danger : ArenaColor.AOE;
            yield return aoe with { Color = colour, Risky = shown == 0 };
            shown++;
        }
    }
}

[ModuleInfo(Incomplete = true, PrimaryActorOID = (uint)OID.DaryaSeaMaid, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1084, NameID = 14291)]
public class V03DaryaTheSeaMaid(WorldState ws, Actor primary) : BossModule(ws, primary, new(375, 530), new ArenaBoundsSquare(20));
