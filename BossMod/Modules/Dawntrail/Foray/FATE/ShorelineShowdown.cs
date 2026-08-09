namespace BossMod.Dawntrail.Foray.FATE.ShorelineShowdown;

public enum OID : uint {
    RegnantChimera = 0x4C7D,
    Helper = 0x233C,
    GlacipotentOrb = 0x4C80, // R2.000, x0 (spawn during fight)
    FulmipotentOrb = 0x4C7F, // R2.000, x0 (spawn during fight)
    Cacophony = 0x4B71, // R1.000, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 50856, // RegnantChimera->player, no cast, single-target
    TheRamsBreath = 48631, // RegnantChimera->self, 6.0s cast, range 30 120-degree cone
    TheRamsBreath1 = 48632, // RegnantChimera->self, no cast, range 30 120-degree cone
    TheRamsBreath2 = 49748, // RegnantChimera->self, no cast, range 30 120-degree cone
    TheDragonsBreath = 48629, // RegnantChimera->self, 6.0s cast, range 30 120-degree cone
    TheDragonsBreath1 = 48630, // RegnantChimera->self, no cast, range 30 120-degree cone
    TheDragonsBreath2 = 49747, // RegnantChimera->self, no cast, range 30 120-degree cone
    TheRamsVoice = 48633, // RegnantChimera->self, 4.0s cast, range 9 circle
    TheRamsVoice1 = 48635, // 4C80->location, 1.0s cast, range 12 circle
    TheDragonsVoice = 48634, // RegnantChimera->self, 4.0s cast, range 8-30 donut
    TheDragonsVoice1 = 48636, // 4C7F->location, 4.0s cast, range 8-30 donut
    Cacophony = 50113, // RegnantChimera->self, 4.0s cast, single-target
    ChaoticChorus = 50114, // 4B71->self, 1.5s cast, range 6 circle
}

public enum SID : uint {
    Gen = 5196, // RegnantChimera/4C80->4C80/RegnantChimera, extra=0x0
    Gen1 = 5197, // RegnantChimera/4C7F->4C7F/RegnantChimera, extra=0x0
}

public enum IconID : uint {
    TurnLeft = 547, // RegnantChimera->self
    TurnRight = 546, // RegnantChimera->self
}

sealed class TheRamsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheRamsVoice, new AOEShapeCircle(9.0f));
sealed class TheDragonsVoice(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.TheDragonsVoice, (uint)AID.TheDragonsVoice1],
    new AOEShapeDonut(8.0f, 30.0f));
sealed class Cacophony(BossModule module) : Components.Voidzone(module, 6.0f, module => module.Enemies((uint)OID.Cacophony).Where(z => z.EventState != 7));

sealed class Breath(BossModule module) : Components.GenericAOEs(module) {
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
            aoeCasters.Add(new(shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
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

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        if (aoes.Count == 0) {
            return [];
        }

        var incomingAOEs = aoes.OrderBy(a => a.Activation).Take(2).ToList();
        int show = 0;
        foreach (ref var aoe in CollectionsMarshal.AsSpan(incomingAOEs)) {
            aoe.Color = show == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = show == 0;
            show++;
        }

        return CollectionsMarshal.AsSpan(incomingAOEs);
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

sealed class GlacipotentOrb(BossModule module) : Components.GenericAOEs(module) {
    private List<Actor> iceOrbs = [];
    private readonly AOEShapeCircle shape = new(12.0f);
    private bool active = false;

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.GlacipotentOrb) {
            iceOrbs.Add(actor);
        }
    }

    public override void OnActorDeath(Actor actor) {
        if (actor.OID == (uint)OID.GlacipotentOrb) {
            iceOrbs.Remove(actor);

            if (iceOrbs.Count == 0) {
                active = false;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.TheRamsVoice) {
            active = true;
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        if (iceOrbs.Count == 0 || active == false) {
            return [];
        }

        List<AOEInstance> aoes = [];
        foreach (var orb in iceOrbs) {
            aoes.Add(new(shape, orb.Position, orb.Rotation));
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

[SkipLocalsInit]
sealed class RegnantChimeraStates : StateMachineBuilder {
    public RegnantChimeraStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<Breath>()
            .ActivateOnEnter<TheRamsVoice>()
            .ActivateOnEnter<GlacipotentOrb>()
            .ActivateOnEnter<TheDragonsVoice>()
            .ActivateOnEnter<Cacophony>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(RegnantChimeraStates),
    ConfigType = null, // replace null with typeof(RegnantChimeraConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = typeof(SID), // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.RegnantChimera,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14767u,
    SortOrder = 27,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class RegnantChimera(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
