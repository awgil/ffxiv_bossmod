namespace BossMod.Dawntrail.Variant.V03DaryaSeaMaid;

class PiercingPlunge(BossModule module) : Components.RaidwideCast(module, AID.PiercingPlunge);

class FamiliarCall(BossModule module) : Components.GenericAOEs(module) {
    private List<(String name, OID animal)> vfxOrder = []; // Stores an easy-to-read name for callouts and the animal OID for aoes
    private TimeSpan cooldown = TimeSpan.FromSeconds(0.5);
    private DateTime lastVFX = DateTime.MinValue;

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

            vfxOrder.Add((name, animal));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Watersong || spell.Action.ID == (uint)AID.Watersong1 ||
            spell.Action.ID == (uint)AID.Watersong2 || spell.Action.ID == (uint)AID.Watersong3) {
            if ((WorldState.CurrentTime - lastVFX) < cooldown) {
                return;
            }

            if (vfxOrder.Count > 0) {
                vfxOrder.RemoveAt(0);
                lastVFX = WorldState.CurrentTime;
                NumCasts++;
            }
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        int show = 0;
        foreach (var nextAOE in vfxOrder.Take(2)) {
            uint colour = show == 0 ? ArenaColor.Danger : ArenaColor.AOE;
            foreach (var animal in Module.Enemies(nextAOE.animal)) {
                yield return new(new AOEShapeRect(40f, 4f), animal.Position, animal.Rotation, default, colour, show == 0);
            }
            show++;
        }
    }

    public override void AddGlobalHints(GlobalHints hints) {
        if (vfxOrder.Count > 0) {
            hints.Add("Order: " + string.Join(", ", vfxOrder.Select(o => o.name)));
        }
    }
}

class SunkenTreasure(BossModule module) : Components.GenericAOEs(module) {
    private List<Actor> orbs = [];

    public override void OnActorEAnim(Actor actor, uint state) {
        if (actor.OID == (uint)OID.BlueSphere || actor.OID == (uint)OID.DonutSphere) {
            if (state == 1048608) {
                orbs.Add(actor);
            }

            if (state == 262152) {
                orbs.Remove(actor);
                NumCasts++;
            }
        }

    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        int shown = 0;
        foreach (var orb in orbs) {
            if (shown >= 5) {
                break;
            }

            var shape = (orb.OID == (uint)OID.BlueSphere) ? (AOEShape)new AOEShapeCircle(18f) : (AOEShape)new AOEShapeDonut(4f, 20f);
            yield return new(shape, orb.Position, orb.Rotation);
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

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        foreach (var aoe in aoes) {
            yield return aoe;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action == WatchedAction) {
            aoes.Add(new AOEInstance(Shape, caster.Position, caster.Rotation));
        }
    }

    public override void OnActorEAnim(Actor actor, uint state) {
        if (state == 0x00040008) {
            aoes.RemoveAll(t => t.Origin.AlmostEqual(actor.Position, 1.0f));
        }
    }
}

// TODO tether distance is 8-10m apart once set in - todo verify this - stand one square apart when they start
//  if it fails then stand two squares part and test until 3 squares part
//  once the tether is locked see how close you can get before it breaks
//  depending on finding, remove TetherID from Assignment and assign + clean up AddAIHints
//  remove update function
class SeaShackles(BossModule module) : BossComponent(module) {
    private readonly List<(Actor, Actor)> _distant = [];
    public record struct Assignment(Role Partner, int PartnerSlot, bool tooClose, TetherID tetherID);
    public readonly Assignment[] Assignments = new Assignment[PartyState.MaxPartySize];

    public override void OnTethered(Actor source, ActorTetherInfo tether) {
        if (tether.ID == (uint)TetherID.SeaShackles) {
            Assign(source, WorldState.Actors.Find(tether.Target)!, true, (TetherID)tether.ID);
        }

        if (tether.ID == (uint)TetherID.SeaShacklesSafe) {
            Assign(source, WorldState.Actors.Find(tether.Target)!, false, (TetherID)tether.ID);
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

            if (assign.tooClose == false)
            {
                hints.Add("Stay far from partner!", false);
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);

        // TODO refactor this code depending on how tethers set in, if same then can relay on the same logic
        //  However I feel like once the tether is correctly setup it has a lower distance before it will break
        foreach (var (a, b) in _distant) {
            if (a.InstanceID == actor.InstanceID || b.InstanceID == actor.InstanceID) {
                var assign = Assignments[slot];
                if (assign.tetherID == TetherID.SeaShackles) {
                    if (a.InstanceID == actor.InstanceID) {
                        hints.AddForbiddenZone(ShapeContains.Circle(b.Position, 10.0f));
                    }

                    if (b.InstanceID == actor.InstanceID) {
                        hints.AddForbiddenZone(ShapeContains.Circle(a.Position, 10.0f));
                    }
                }

                if (assign.tetherID == TetherID.SeaShacklesSafe) {
                    if (a.InstanceID == actor.InstanceID) {
                        hints.AddForbiddenZone(ShapeContains.Circle(b.Position, 10.0f));
                    }

                    if (b.InstanceID == actor.InstanceID) {
                        hints.AddForbiddenZone(ShapeContains.Circle(a.Position, 10.0f));
                    }
                }
            }
        }
    }

    public override void Update() {
        foreach (var (a, b) in _distant) {
            Service.Logger.Info("Tether distance apart: " + (a.Position - b.Position).Length()); // TODO remove once tested
        }
    }

    private void Assign(Actor a, Actor b, bool tooClose, TetherID tetherID) {
        var aslot = Raid.FindSlot(a.InstanceID);
        var bslot = Raid.FindSlot(b.InstanceID);
        Assignments[aslot] = new(b.Role, bslot, tooClose, tetherID);
        Assignments[bslot] = new(a.Role, aslot, tooClose, tetherID);
        _distant.Add((a, b));
    }
}

// Arena is 40.0f, and it knocks you back 3 full squares so 24.0f
class TidalWave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TidalWave1, 24.0f, kind: Kind.DirForward) {
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var src in Sources(slot, actor)) {
            if (!IsImmune(slot, src.Activation)) {
                var tiles = Module.FindComponent<AquaSpear>()!.ActiveAOEs(slot, actor).Select(a => (a.Origin, a.Rotation)).ToList();
                var partnerAssigned = Module.FindComponent<SeaShackles>()!.Assignments[slot];
                var partner = partnerAssigned.Partner != Role.None ? Raid[partnerAssigned.PartnerSlot] : null;
                var distanceFromPartner = partnerAssigned.tetherID == TetherID.SeaShackles ? 10.0f : 10.0f;

                if (partner == null) {
                    continue;
                }

                var direction = src.Direction.ToDirection().Normalized();
                var center = Arena.Center;
                var bounds = Arena.Bounds;

                hints.AddForbiddenZone(p => {
                    var proj = p + direction * 24.0f;

                    if (!bounds.Contains(proj - center)) {
                        return true;
                    }

                    if (Intersect.RayCircle(p, direction, partner.Position, distanceFromPartner) <= 1000f) {
                        return true;
                    }

                    foreach (var tile in tiles) {
                        if (Intersect.RayRect(p, direction, tile.Origin, tile.Rotation.ToDirection(), 4f, 4f) <= 24.0f) {
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
            aoes.Add(new(new AOEShapeCircle(12f), actor.Position, actor.Rotation, default, ArenaColor.AOE));
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

class SwimingInTheAirStackSpread(BossModule module) : Components.IconStackSpread(module, (uint)IconID.Tidalspout,
    (uint)IconID.Hydrobullet2, AID.Tidalspout, AID.HydrobulletSpread2, 6.0f, 15.0f, 6.0f, 2, 3, true) {
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
        if (spell.Action.ID == (uint)AID.RecedingTwinTides) {
            aoes.Add(new(new AOEShapeCircle(10f), caster.Position, caster.Rotation, Risky: true));
            aoes.Add(new(new AOEShapeDonut(10.0f, 40.0f), caster.Position, caster.Rotation, Risky: false));
        }

        if (spell.Action.ID == (uint)AID.EncroachingTwinTides) {
            aoes.Add(new(new AOEShapeDonut(10.0f, 40.0f), caster.Position, caster.Rotation, Risky: true));
            aoes.Add(new(new AOEShapeCircle(10f), caster.Position, caster.Rotation, Risky: false));
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
