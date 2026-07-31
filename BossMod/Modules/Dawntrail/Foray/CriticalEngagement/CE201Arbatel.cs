namespace BossMod.Dawntrail.Foray.CriticalEngagement.CE201Arbatel;

// TODO improve exaflares

public enum OID : uint {
    Arbatel = 0x4BD3,
    Helper = 0x233C,
    Page8 = 0x4BD6, // R1.500, x0 (spawn during fight)
    Page16 = 0x4BD5, // R3.220, x0 (spawn during fight)
    Page64 = 0x4BD4, // R2.400, x0 (spawn during fight)
    Page512 = 0x4BD7, // R1.950, x0 (spawn during fight)

    _Gen_EvieNox = 0x0, // R0.500, x0 (spawn during fight), None type
    _Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x2, EventObj type
    _Gen_Actor1ebfcd = 0x1EBFCD, // R0.500, x0 (spawn during fight), EventObj type
    _Gen_ = 0x4BD8, // R2.400, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 49056, // Arbatel->player, no cast, single-target
    Teleport = 48246, // Arbatel->location, no cast, single-target
    MarginaliaCast = 47327, // Helper->self, 5.0s cast, ???
    Marginalia = 47328, // Arbatel->self, 5.0s cast, single-target
    CoverToCoverForward = 47302, // Arbatel->self, 4.0s cast, range 30 180.000-degree cone
    CoverToCoverBackwards = 47303, // Arbatel->self, 1.0s cast, range 30 180.000-degree cone
    UnboundInk = 49492, // Arbatel->self, 4.0s cast, range 9 circle
    BookDropCast = 47319, // Arbatel->self, 3.0s cast, single-target
    BookDrop = 47322, // 4BD8->self, 8.0s cast, range 3 circle
    ThunderII = 47324, // Helper->self, 4.0s cast, range 50 width 5 rect
    FireIICast = 47326, // Arbatel->self, 4.5+0.5s cast, ???
    FireII = 47325, // Helper->self, 5.0s cast, range 60 45.000-degree cone
    ArcaneRule = 47304, // Arbatel->self, 6.0s cast, single-target
    QuadRule = 47305, // Helper->self, 6.0s cast, range 25 width 10 cross
    HorizontalRule = 47306, // Helper->self, 2.0s cast, range 50 width 6 rect
    BlotCast = 47300, // Arbatel->self, 3.0s cast, ???
    Blot = 47301, // Helper->location, 8.0s cast, range 15 circle

    SummonCast = 49055, // Arbatel->self, 3.0s cast, ???
    Summon = 47307, // Helper->location, 3.0s cast, range 4 circle
    KnowledgeLevelCorrectionCast = 47296, // Arbatel->self, 5.0s cast, ???
    KnowledgeLevelCorrection = 47297, // Helper->self, no cast, ???

    PrimeKnowledgeLevelDeathCast = 47318, // 4BD7->self, 11.0s cast, single-target
    PrimeKnowledgeLevelDeathVisual = 50561, // Helper->self, 11.0s cast, range 25 ?-degree cone
    PrimeKnowledgeLevelDeath = 49879, // Helper->self, 11.0s cast, range 25 180.000-degree cone
    PrimeKnowledgeLevelDeathVisual1 = 50560, // Helper->self, 11.0s cast, range 25 ?-degree cone
    PrimeKnowledgeLevelDeath1 = 47314, // Helper->self, 11.0s cast, range 25 120.000-degree cone

    KnowledgeLevel3FlareCast = 47316, // 4BD5->self, 11.0s cast, single-target
    KnowledgeLevel3FlareVisual = 50555, // Helper->self, 11.0s cast, range 25 ?-degree cone
    KnowledgeLevel3Flare = 47309, // Helper->self, 11.0s cast, range 25 180.000-degree cone
    KnowledgeLevel3FlareVisual1 = 50558, // Helper->self, 11.0s cast, range 25 ?-degree cone
    KnowledgeLevel3Flare1 = 47312, // Helper->self, 11.0s cast, range 25 120.000-degree cone

    KnowledgeLevel4HolyCast = 47317, // 4BD6->self, 11.0s cast, single-target
    KnowledgeLevel4HolyVisual = 50559, // Helper->self, 11.0s cast, range 25 ?-degree cone
    KnowledgeLevel4Holy = 47313, // Helper->self, 11.0s cast, range 25 120.000-degree cone

    KnowledgeLevel5DeathCast = 47315, // 4BD4->self, 11.0s cast, single-target
    KnowledgeLevel5DeathVisual = 50557, // Helper->self, 11.0s cast, range 25 ?-degree cone
    KnowledgeLevel5Death = 47311, // Helper->self, 11.0s cast, range 25 120.000-degree cone
}

public enum SID : uint {
    Correction1 = 5014, // none->player, extra=0x0
    Correction2 = 5015, // none->player, extra=0x0
    Correction3 = 5016, // none->player, extra=0x0
    Correction4 = 5017, // none->player, extra=0x0
    Correction5 = 5018, // none->player, extra=0x0
}

sealed class KnowledgeLevelCorrection(BossModule module) : Components.RaidwideCast(module, (uint)AID.KnowledgeLevelCorrectionCast);
sealed class Summon(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Summon, new AOEShapeCircle(4.0f));
sealed class Marginalia(BossModule module) : Components.RaidwideCast(module, (uint)AID.Marginalia);
sealed class UnboundInk(BossModule module) : Components.SimpleAOEs(module, (uint)AID.UnboundInk, new AOEShapeCircle(9.0f));
sealed class BookDrop(BossModule module) : Components.CastTowers(module, (uint)AID.BookDrop, 3.0f, 3, 3);
sealed class ThunderII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.ThunderII, new AOEShapeRect(50.0f, 2.5f), 10);
sealed class FireII(BossModule module) : Components.SimpleAOEs(module, (uint)AID.FireII, new AOEShapeCone(60.0f, 22.5f.Degrees()));
sealed class QuadRule(BossModule module) : Components.SimpleAOEs(module, (uint)AID.QuadRule, new AOEShapeCross(25.0f, 5.0f));
sealed class HorizontalRule(BossModule module) : Components.SimpleAOEs(module, (uint)AID.HorizontalRule, new AOEShapeRect(50.0f, 3.0f));

sealed class CoverToCover(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];
    private AOEShapeCone shape = new AOEShapeCone(30.0f, 90.0f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.CoverToCoverForward) {
            aoes.Add(new(shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            aoes.Add(new(shape, caster.Position, spell.Rotation + 180.0f.Degrees(), Module.CastFinishAt(spell, 4.2f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.CoverToCoverForward or (uint)AID.CoverToCoverBackwards) {
            if (aoes.Count > 0) {
                aoes.RemoveAt(0);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        if (aoes.Count == 0) {
            return [];
        }

        var aoe = aoes[0];
        aoe.Color = Colors.Danger;
        aoe.Risky = true;
        aoes[0] = aoe;

        return CollectionsMarshal.AsSpan(aoes);
    }
}

sealed class Blot(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];
    private readonly AOEShapeCircle shape = new(15.0f);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.Blot) {
            aoes.Add(new(shape, spell.LocXZ,  spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Blot) {
            if (aoes.Count > 0) {
                aoes.RemoveAll(aoe => aoe.ActorID == caster.InstanceID);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        int show = 0;
        var incomingAOEs = aoes.OrderBy(a => a.Activation).Take(6).ToList();
        foreach (ref var aoe in CollectionsMarshal.AsSpan(incomingAOEs)) {
            if (show >= 3) {
                break;
            }

            aoe.Color = Colors.Danger;
            aoe.Risky = true;
            show++;
        }

        return CollectionsMarshal.AsSpan(incomingAOEs);
    }
}

sealed class KnowledgeLevel(BossModule module) : Components.GenericAOEs(module) {
    private List<(AOEInstance aoe, int spellLevel)> aoes = [];
    private AOEShapeCone shapeHalf = new AOEShapeCone(25.0f, 90.0f.Degrees());
    private AOEShapeCone shapeThird = new AOEShapeCone(25.0f, 60.0f.Degrees());
    private readonly Dictionary<ulong, int> playerDebuffs = [];

    public override void OnStatusGain(Actor actor, ref ActorStatus status) {
        var level = (SID)status.ID switch {
            SID.Correction1 => 1,
            SID.Correction2 => 2,
            SID.Correction3 => 3,
            SID.Correction4 => 4,
            SID.Correction5 => 5,
            _ => 0
        };

        if (level == 0) {
            return;
        }

        playerDebuffs[actor.InstanceID] = level;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.PrimeKnowledgeLevelDeath) {
            aoes.Add(new(new(shapeHalf, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID), 1));
        }

        if (spell.Action.ID == (uint)AID.PrimeKnowledgeLevelDeath1) {
            aoes.Add(new(new(shapeThird, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID), 1));
        }

        if (spell.Action.ID == (uint)AID.KnowledgeLevel3Flare) {
            aoes.Add(new(new(shapeHalf, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID), 3));
        }

        if (spell.Action.ID == (uint)AID.KnowledgeLevel3Flare1) {
            aoes.Add(new(new(shapeThird, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID), 3));
        }

        if (spell.Action.ID == (uint)AID.KnowledgeLevel4Holy) {
            aoes.Add(new(new(shapeThird, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID), 4));
        }

        if (spell.Action.ID == (uint)AID.KnowledgeLevel5Death) {
            aoes.Add(new(new(shapeThird, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID), 5));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID is (uint)AID.PrimeKnowledgeLevelDeath or (uint)AID.PrimeKnowledgeLevelDeath1 or (uint)AID.KnowledgeLevel3Flare or
            (uint)AID.KnowledgeLevel3Flare1 or (uint)AID.KnowledgeLevel4Holy or (uint)AID.KnowledgeLevel5Death) {
            if (aoes.Count > 0) {
                aoes.RemoveAll(aoe => aoe.aoe.ActorID == caster.InstanceID);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        if (aoes.Count == 0) {
            return [];
        }

        var currentAOEs = new AOEInstance[aoes.Count];
        var playerActingLevel = playerDebuffs[actor.InstanceID] + actor.ForayInfo.Level;
        for (int i = 0; i < aoes.Count; i++) {
            var aoeInstance = aoes[i];

            // Case: prime number check
            if (aoeInstance.spellLevel == 1) {
                if (!IsPrimeNumber(playerActingLevel)) {
                    aoeInstance.aoe.Color = Colors.SafeFromAOE;
                    aoeInstance.aoe.Risky = false;
                }
            }

            // Case: number divisible check
            if (aoeInstance.spellLevel > 1) {
                if (!IsDivisibleNumber(playerActingLevel, aoeInstance.spellLevel)) {
                    aoeInstance.aoe.Color = Colors.SafeFromAOE;
                    aoeInstance.aoe.Risky = false;
                }
            }

            aoes[i] = aoeInstance;
            currentAOEs[i] = aoeInstance.aoe;
        }

        return currentAOEs;
    }

    private bool IsPrimeNumber(int number) {
        if (number <= 1) {
            return false;
        }

        for (int i = 2; i < number; i++) {
            if (number % i == 0) {
                return false;
            }
        }

        return true;
    }

    private bool IsDivisibleNumber(int number, int  divisor) {
        return number % divisor == 0;
    }
}

[SkipLocalsInit]
sealed class ArbatelStates : StateMachineBuilder {
    public ArbatelStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<KnowledgeLevelCorrection>()
            .ActivateOnEnter<Summon>()
            .ActivateOnEnter<KnowledgeLevel>()
            .ActivateOnEnter<Marginalia>()
            .ActivateOnEnter<CoverToCover>()
            .ActivateOnEnter<UnboundInk>()
            .ActivateOnEnter<BookDrop>()
            .ActivateOnEnter<ThunderII>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<QuadRule>()
            .ActivateOnEnter<HorizontalRule>()
            .ActivateOnEnter<Blot>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(ArbatelStates),
    ConfigType = null, // replace null with typeof(ArbatelConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = null, // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Arbatel,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1093u,
    NameID = 14520u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class Arbatel(WorldState ws, Actor primary) : BossModule(ws, primary, new(658.991f, 658.991f), new ArenaBoundsCircle(25f));
