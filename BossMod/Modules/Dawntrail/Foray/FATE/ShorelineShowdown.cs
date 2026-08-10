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
    LeftDuobreath = 50111, // Boss->self, 5.0s cast, range 40 180-degree cone
    TheRamsBreath3 = 50116, // Boss->self, no cast, range 40 180-degree cone
    RightDuobreath = 50112, // Boss->self, 5.0s cast, range 40 180-degree cone
    TheDragonsBreath3 = 50115, // Boss->self, no cast, range 40 180-degree cone
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
sealed class TheDragonsVoice(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheDragonsVoice1, new AOEShapeDonut(8.0f, 30.0f));
sealed class ChaoticChorus(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ChaoticChorus, new AOEShapeCircle(6.0f));

sealed class Breath(BossModule module) : Components.GenericRotatingAOE(module) {
    private ActorCastInfo? spellInfo;
    private Angle increment;
    private readonly AOEShapeCone shape = new(30.0f, 60.0f.Degrees());

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID) {
        increment = iconID switch {
            (uint)IconID.TurnLeft => 120.0f.Degrees(),
            (uint)IconID.TurnRight => -120.0f.Degrees(),
            _ => default
        };

        InitIfReady();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID is (uint)AID.TheRamsBreath or (uint)AID.TheDragonsBreath) {
            spellInfo = spell;
            InitIfReady();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.TheRamsBreath or (uint)AID.TheRamsBreath1 or (uint)AID.TheRamsBreath2 or
            (uint)AID.TheDragonsBreath or (uint)AID.TheDragonsBreath1 or (uint)AID.TheDragonsBreath2) {
            if (Sequences.Count > 0) {
                AdvanceSequence(0, WorldState.CurrentTime);
            }
        }
    }

    private void InitIfReady() {
        if (spellInfo != null && increment != default) {
            Sequences.Add(new(shape, spellInfo.LocXZ, spellInfo.Rotation, increment, Module.CastFinishAt(spellInfo), 2.7f, 3));
            spellInfo = null;
            increment = default;
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

sealed class TheDragonsVoiceBoss(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheDragonsVoice, new AOEShapeDonut(8.0f, 30.0f)) {
    private readonly List<Actor> orbs = [];

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.FulmipotentOrb) {
            orbs.Add(actor);
        }
    }

    public override void OnActorDestroyed(Actor actor) {
        if (actor.OID == (uint)OID.FulmipotentOrb) {
            orbs.Remove(actor);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);
        if (Casters.Count == 0) {
            return;
        }

        if (orbs.Count <= 2) {
            return;
        }

        Actor? singleOrb = null;
        var bestDistance = float.MinValue;
        foreach (var orb in orbs) {
            var distance = orbs.Where(o => o != orb).Min(o => (o.Position - orb.Position).LengthSq());
            if (distance > bestDistance) {
                bestDistance = distance;
                singleOrb = orb;
            }
        }

        if (singleOrb == null) {
            return;
        }

        var spellInstance = Casters[0];
        var distanceToOrb = spellInstance.Origin + (singleOrb.Position - spellInstance.Origin).Normalized() * 6.0f;
        hints.GoalZones.Add(AIHints.GoalProximity(distanceToOrb, 7.8f, 100.0f));
    }
}

sealed class Cacophony(BossModule module) : Components.GenericAOEs(module) {
    private List<Actor> orbs = [];
    private readonly AOEShapeCircle shape = new(6.0f);

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.Cacophony) {
            orbs.Add(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.ChaoticChorus) {
            orbs.Remove(caster);
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        if (orbs.Count == 0) {
            return [];
        }

        List<AOEInstance> aoes = [];
        foreach (var orb in orbs) {
            aoes.Add(new(shape, orb.Position, orb.Rotation, WorldState.FutureTime(1.5f)));
        }

        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class Duobreath(BossModule module) : Components.GenericAOEs(module) {
    private readonly List<AOEInstance> aoes = [];
    private readonly AOEShapeCone shape = new(40.0f, 90.0f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID is (uint)AID.LeftDuobreath or (uint)AID.RightDuobreath) {
            aoes.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            aoes.Add(new(shape, spell.LocXZ, spell.Rotation + 180.0f.Degrees(), Module.CastFinishAt(spell, 3.0f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.LeftDuobreath or (uint)AID.RightDuobreath or (uint)AID.TheRamsBreath3 or (uint)AID.TheDragonsBreath3) {
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        var count = aoes.Count;
        if (count == 0) {
            return [];
        }

        var max = count > 2 ? 2 : count;
        var nextAOEs = CollectionsMarshal.AsSpan(aoes);

        for (int i = 0; i < max; i++) {
            ref var aoe = ref nextAOEs[i];
            aoe.Color = i == 0 ? Colors.Danger : Colors.AOE;
            aoe.Risky = i == 0;
        }

        return nextAOEs[..max];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) {
        base.AddAIHints(slot, actor, assignment, hints);
        if (aoes.Count == 0) {
            return;
        }

        var nextAOE =  aoes[0];
        var distance = nextAOE.Shape.Distance(nextAOE.Origin, nextAOE.Rotation);
        hints.GoalZones.Add(p => distance.Distance(p) is > 0.0f and <= 1.0f ? 100.0f : 0.0f);
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
            .ActivateOnEnter<TheDragonsVoiceBoss>()
            .ActivateOnEnter<Cacophony>()
            .ActivateOnEnter<ChaoticChorus>()
            .ActivateOnEnter<Duobreath>();
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
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2076u,
    SortOrder = 5,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class RegnantChimera(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);
