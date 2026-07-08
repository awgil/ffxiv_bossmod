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
            if (state == (uint)State.FirstState) {
                orbs.Add(actor);
            }

            if (state == (uint)State.BlowUpState) {
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

            var shape = (orb.OID == (uint)OID.BlueSphere) ? (AOEShape)new AOEShapeCircle(18f) : (AOEShape)new AOEShapeDonut(6f, 20f);
            yield return new(shape, orb.Position, orb.Rotation);
            shown++;
        }
    }
}

// Players without anything can stand in the bait as well with no worry
class Hydrobullet : Components.BaitAwayIcon {
    public Hydrobullet(BossModule module) : base(module, new AOEShapeCircle(5.0f), (uint)IconID.Hydrobullet, AID.HydrobulletSpread, centerAtTarget: true) {
        EnableHints = false;
    }
}

class Hydrocannon(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70f, 3f), (uint)IconID.Hydrocannon, AID.Hydrocannon1, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster);

class AquaSpear(BossModule module) : Components.StandardAOEs(module, AID.AquaSpear1, new AOEShapeRect(8f, 4f)) {
    private List<AOEInstance> aoes = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        foreach (var aoe in aoes) {
            yield return aoe;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action == WatchedAction) {
            aoes.Add(new AOEInstance(Shape, caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation, Module.CastFinishAt(spell), Color, Risky));
        }
    }
}

// TODO does it matter if the players are really far away? or will it auto solve if so
class SeaShackles(BossModule module) : BossComponent(module) {
    private readonly List<(Actor, Actor)> _distant = [];
    public record struct Assignment(Role Partner, int PartnerSlot, bool tooClose);
    public readonly Assignment[] Assignments = new Assignment[PartyState.MaxPartySize];

    public override void OnTethered(Actor source, ActorTetherInfo tether) {
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
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc) {
        foreach (var (a, b) in _distant) {
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

    private void Assign(Actor a, Actor b, bool tooClose) {
        var aslot = Raid.FindSlot(a.InstanceID);
        var bslot = Raid.FindSlot(b.InstanceID);
        Assignments[aslot] = new(b.Role, bslot, tooClose);
        Assignments[bslot] = new(a.Role, aslot, tooClose);
        _distant.Add((a, b));
    }
}

// Arena is 40.0f, and it knocks you back 3 full squares so 24.0f - slightly over to ensure the player will be safe
class TidalWave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TidalWave1, 24.5f, kind: Kind.DirForward);

class SwimmingInTheAir(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];

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

class SwimmingInTheAirSpread : Components.BaitAwayIcon {
    public SwimmingInTheAirSpread(BossModule module) : base(module, new AOEShapeCircle(15.0f), (uint)IconID.Hydrobullet2, AID.HydrobulletSpread2, centerAtTarget: true) {
        EnableHints = false;
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

[ModuleInfo(Incomplete = true, PrimaryActorOID = (uint)OID.DaryaSeaMaid, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1084, NameID = 14291)]
public class V03DaryaTheSeaMaid(WorldState ws, Actor primary) : BossModule(ws, primary, new(375, 530), new ArenaBoundsSquare(20));
