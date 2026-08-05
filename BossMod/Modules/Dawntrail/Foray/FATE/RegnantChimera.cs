namespace BossMod.Dawntrail.Foray.FATE.RegnantChimera;

public enum OID : uint {
    Boss = 0x4C7D,
    Helper = 0x233C,
    GlacipotentOrb = 0x4C80, // R2.000, x0 (spawn during fight)
    FulmipotentOrb = 0x4C7F, // R2.000, x0 (spawn during fight)
    Cacophony = 0x4B71, // R1.000, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 50856, // Boss->player, no cast, single-target

    TheRamsBreath = 48631, // Boss->self, 6.0s cast, range 30 120-degree cone
    TheRamsBreath1 = 48632, // Boss->self, no cast, range 30 120-degree cone
    TheRamsBreath2 = 49748, // Boss->self, no cast, range 30 120-degree cone
    TheDragonsBreath = 48629, // Boss->self, 6.0s cast, range 30 120-degree cone
    TheDragonsBreath1 = 48630, // Boss->self, no cast, range 30 120-degree cone
    TheDragonsBreath2 = 49747, // Boss->self, no cast, range 30 120-degree cone

    TheRamsVoice = 48633, // Boss->self, 4.0s cast, range 9 circle
    TheRamsVoice1 = 48635, // 4C80->location, 1.0s cast, range 12 circle
    TheDragonsVoice = 48634, // Boss->self, 4.0s cast, range 8-30 donut
    TheDragonsVoice1 = 48636, // 4C7F->location, 4.0s cast, range 8-30 donut

    Cacophony = 50113, // Boss->self, 4.0s cast, single-target
    ChaoticChorus = 50114, // 4B71->self, 1.5s cast, range 6 circle
}

public enum SID : uint {
    Gen = 5196, // Boss/4C80->4C80/Boss, extra=0x0
    Gen1 = 5197, // Boss/4C7F->4C7F/Boss, extra=0x0
}

public enum IconID : uint {
    TurnLeft = 547, // Boss->self
    TurnRight = 546, // Boss->self
}

class TheRamsVoice(BossModule module) : Components.StandardAOEs(module, AID.TheRamsVoice, 9.0f);
class TheDragonsVoice(BossModule module) : Components.GroupedAOEs(module, [AID.TheDragonsVoice, AID.TheDragonsVoice1], new AOEShapeDonut(8.0f, 30.0f));
class Cacophony(BossModule module) : Components.PersistentVoidzone(module, 6.0f, module => module.Enemies((uint)OID.Cacophony).Where(z => z.EventState != 7));

class Breath(BossModule module) : Components.GenericAOEs(module) {
    private readonly List<AOEInstance> aoes = [];
    private readonly List<AOEInstance> aoeCasters = [];
    private readonly AOEShapeCone shape = new(30.0f, 60.0f.Degrees());
    private int direction = 0; // -1 = right, 1 = left

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        if (iconID == (uint)IconID.TurnRight) {
            direction = -1;
        }

        if (iconID == (uint)IconID.TurnLeft) {
            direction = 1;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID is (uint)AID.TheRamsBreath or (uint)AID.TheDragonsBreath) {
            aoeCasters.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.TheRamsBreath or (uint)AID.TheRamsBreath1 or (uint)AID.TheRamsBreath2 or
            (uint)AID.TheDragonsBreath or (uint)AID.TheDragonsBreath1 or (uint)AID.TheDragonsBreath2) {
            aoes.Sort((a, b) => a.Activation.CompareTo(b.Activation));
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);
            }

            if (aoes.Count == 0) {
                direction = 0;
            }
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        if (aoes.Count == 0) {
            yield break;
        }

        int show = 0;
        foreach (var aoe in aoes.OrderBy(aoe => aoe.Activation).Take(2)) {
            yield return aoe with { Color = show == 0 ? ArenaColor.Danger : ArenaColor.AOE, Risky = show == 0 };
            show++;
        }
    }

    public override void Update() {
        AddFutureAOEs();
    }

    private void AddFutureAOEs() {
        if (aoeCasters.Count == 0 || direction == 0) {
            return;
        }

        List<AOEInstance> futureAOEs = [];
        var processedAOEsCount = aoeCasters.Count;
        for (int i = 0; i < processedAOEsCount; i++) {
            var aoe = aoeCasters[i];
            futureAOEs.Add(new(shape, aoe.Origin, aoe.Rotation, aoe.Activation));
            futureAOEs.Add(new(shape, aoe.Origin, (aoe.Rotation + 120.0f.Degrees() * direction).Normalized(), aoe.Activation.AddSeconds(2.7f)));
            futureAOEs.Add(new(shape, aoe.Origin, (aoe.Rotation + 240.0f.Degrees() * direction).Normalized(), aoe.Activation.AddSeconds(5.4f)));
        }

        aoeCasters.RemoveRange(0, processedAOEsCount);
        if (futureAOEs.Count > 0) {
            aoes.AddRange(futureAOEs);
        }
    }
}

class GlacipotentOrb(BossModule module) : Components.GenericAOEs(module) {
    private List<Actor> iceOrbs = [];
    private readonly AOEShapeCircle shape = new(12.0f);
    private bool active = false;

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.GlacipotentOrb) {
            iceOrbs.Add(actor);
        }
    }

    public override void Update() {
        for (int i = 0; i < iceOrbs.Count; i++) {
            var orb = iceOrbs[i];

            if (iceOrbs[i].IsDead) {
                iceOrbs.Remove(orb);
            }
        }

        if (iceOrbs.Count == 0) {
            active = false;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.TheRamsVoice) {
            active = true;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        if (iceOrbs.Count == 0 || active == false) {
            yield break;
        }

        foreach (var orb in iceOrbs) {
            yield return new AOEInstance(shape, orb.Position, orb.Rotation);
        }
    }
}

class RegnantChimeraStates : StateMachineBuilder {
    public RegnantChimeraStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<Cacophony>()
            .ActivateOnEnter<Breath>()
            .ActivateOnEnter<GlacipotentOrb>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14767)]
public class RegnantChimera(WorldState ws, Actor primary) : BossModule(ws, primary, new(95.000f, 470.000f), new ArenaBoundsCircle(30));
