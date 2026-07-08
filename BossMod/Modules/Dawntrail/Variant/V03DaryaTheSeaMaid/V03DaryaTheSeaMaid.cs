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

class Hydrobullet : Components.BaitAwayIcon {
    public Hydrobullet(BossModule module) : base(module, new AOEShapeCircle(5.0f), (uint)IconID.Hydrobullet, AID.HydrobulletSpread, centerAtTarget: true) {
        EnableHints = false;
    }
}


[ModuleInfo(Incomplete = true, PrimaryActorOID = (uint)OID.DaryaSeaMaid, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1084, NameID = 14291)]
public class V03DaryaTheSeaMaid(WorldState ws, Actor primary) : BossModule(ws, primary, new(375, 530), new ArenaBoundsSquare(20));
