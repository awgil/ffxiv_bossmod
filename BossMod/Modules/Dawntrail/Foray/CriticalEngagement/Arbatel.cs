namespace BossMod.Dawntrail.Foray.CriticalEngagement.Arbatel;

public enum OID : uint
{
    Boss = 0x4BD3, // R3.060, x1
    Helper = 0x233C, // R0.500, x40, Helper type
    Page64 = 0x4BD4, // R2.400, x0 (spawn during fight)
    Page16 = 0x4BD5, // R3.220, x0 (spawn during fight)
    Page8 = 0x4BD6, // R1.500, x0 (spawn during fight)
    Page512 = 0x4BD7, // R1.950, x0 (spawn during fight)
    TowerCaster = 0x4BD8, // R2.400, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 49056, // Boss->player, no cast, single-target
    KnowledgeLevelCorrectionBoss = 47296, // Boss->self, 5.0s cast, ???
    KnowledgeLevelCorrectionHelper = 47297, // Helper->self, no cast, ???
    SummonCast = 49055, // Boss->self, 3.0s cast, ???
    SummonAOE = 47307, // Helper->location, 3.0s cast, range 4 circle
    MarginaliaCast = 47328, // Boss->self, 5.0s cast, single-target
    Marginalia = 47327, // Helper->self, 5.0s cast, ???
    Teleport = 48246, // Boss->location, no cast, single-target
    CoverToCoverFirst = 47302, // Boss->self, 4.0s cast, range 30 180-degree cone
    CoverToCoverSecond = 47303, // Boss->self, 1.0s cast, range 30 180-degree cone
    UnboundInk = 49492, // Boss->self, 4.0s cast, range 9 circle
    BookDropCast = 47319, // Boss->self, 3.0s cast, single-target
    BookDrop = 47322, // 4BD8->self, 8.0s cast, range 3 circle
    ThunderII = 47324, // Helper->self, 4.0s cast, range 50 width 5 rect
    FireIICast = 47326, // Boss->self, 4.5+0.5s cast, ???
    FireII = 47325, // Helper->self, 5.0s cast, range 60 45-degree cone
    ArcaneRule = 47304, // Boss->self, 6.0s cast, single-target
    QuadRule = 47305, // Helper->self, 6.0s cast, range 25 width 10 cross
    HorizontalRule = 47306, // Helper->self, 2.0s cast, range 50 width 6 rect
    BlotCast = 47300, // Boss->self, 3.0s cast, ???
    BlotAOE = 47301, // Helper->location, 8.0s cast, range 15 circle

    PrimeKnowledgeLevelDeathCast = 47318, // Page512->self, 11.0s cast, single-target
    PrimeKnowledgeLevelDeath180Cast1 = 50561, // Helper->self, 11.0s cast, range 25 180-degree cone
    PrimeKnowledgeLevelDeath180Cast2 = 49879, // Helper->self, 11.0s cast, range 25 180-degree cone
    PrimeKnowledgeLevelDeath120Cast1 = 50560, // Helper->self, 11.0s cast, range 25 120-degree cone
    PrimeKnowledgeLevelDeath120Cast2 = 47314, // Helper->self, 11.0s cast, range 25 120-degree cone

    KnowledgeLevel3FlareCast = 47316, // Page16->self, 11.0s cast, single-target
    KnowledgeLevel3Flare180Cast1 = 50555, // Helper->self, 11.0s cast, range 25 180-degree cone
    KnowledgeLevel3Flare180Cast2 = 47309, // Helper->self, 11.0s cast, range 25 180-degree cone
    KnowledgeLevel3Flare120Cast1 = 50558, // Helper->self, 11.0s cast, range 25 120-degree cone
    KnowledgeLevel3Flare120Cast2 = 47312, // Helper->self, 11.0s cast, range 25 120-degree cone

    KnowledgeLevel4HolyCast = 47317, // Page8->self, 11.0s cast, single-target
    KnowledgeLevel4Holy120Cast1 = 50559, // Helper->self, 11.0s cast, range 25 120-degree cone
    KnowledgeLevel4Holy120Cast2 = 47313, // Helper->self, 11.0s cast, range 25 120-degree cone
    KnowledgeLevel4Holy180Cast1 = 50556, // Helper->self, 11.0s cast, range 25 180-degree cone
    KnowledgeLevel4Holy180Cast2 = 47310, // Helper->self, 11.0s cast, range 25 180-degree cone

    KnowledgeLevel5DeathCast = 47315, // Page64->self, 11.0s cast, single-target
    KnowledgeLevel5Death120Cast1 = 50557, // Helper->self, 11.0s cast, range 25 120-degree cone
    KnowledgeLevel5Death120Cast2 = 47311, // Helper->self, 11.0s cast, range 25 120-degree cone
    KnowledgeLevel5Death180Cast1 = 50554, // Helper->self, 11.0s cast, range 25 180-degree cone
    KnowledgeLevel5Death180Cast2 = 47308 // Helper->self, 11.0s cast, range 25 180-degree cone
}

public enum SID : uint
{
    Correction1 = 5014, // none->player, extra=0x0
    Correction2 = 5015, // none->player, extra=0x0
    Correction3 = 5016, // none->player, extra=0x0
    Correction4 = 5017, // none->player, extra=0x0
    Correction5 = 5018, // none->player, extra=0x0
    Invincibility = 4875, // none->4BD7/4BD4/4BD6/4BD8/4BD5, extra=0x0
}

public enum IconID : uint
{
    Checkmark = 136, // player->self
}

public enum TetherID : uint
{
    Tether = 245, // 4BD7/4BD4/4BD6/4BD5->Boss
}

class KnowledgeLevelCorrection(BossModule module) : Components.RaidwideCastDelay(module, AID.KnowledgeLevelCorrectionBoss, AID.KnowledgeLevelCorrectionHelper, 0.9f);
class Invincible(BossModule module) : Components.InvincibleStatus(module, (uint)SID.Invincibility)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in ForbiddenTargets(pcSlot, pc))
            Arena.Actor(t, ArenaColor.Object);
    }
}
class Summon(BossModule module) : Components.StandardAOEs(module, AID.SummonAOE, 4);
class KnowledgeLevel(BossModule module) : Components.GenericAOEs(module)
{
    enum Filter
    {
        N3,
        N4,
        N5,
        Prime
    }

    readonly List<(Actor Caster, Filter Filter, float Width)> _casters = [];

    readonly int[] _adjusted = new int[8];

    static bool Matches(int level, Filter filter) => filter switch
    {
        Filter.N3 => level % 3 == 0,
        Filter.N4 => level % 4 == 0,
        Filter.N5 => level % 5 == 0,
        Filter.Prime => level % 2 > 0 && level % 3 > 0 && level % 5 > 0,
        _ => false
    };

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (caster, filter, width) in _casters)
            if (Matches(actor.ForayInfo.Level + _adjusted[slot], filter))
                yield return new(new AOEShapeCone(25, width.Degrees()), caster.CastInfo!.LocXZ, caster.CastInfo!.Rotation, Module.CastFinishAt(caster.CastInfo));
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        var adj = (SID)status.ID switch
        {
            SID.Correction1 => 1,
            SID.Correction2 => 2,
            SID.Correction3 => 3,
            SID.Correction4 => 4,
            SID.Correction5 => 5,
            _ => 0
        };

        if (adj > 0 && Raid.TryFindSlot(actor, out var slot))
            _adjusted[slot] += adj;
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        var adj = (SID)status.ID switch
        {
            SID.Correction1 => 1,
            SID.Correction2 => 2,
            SID.Correction3 => 3,
            SID.Correction4 => 4,
            SID.Correction5 => 5,
            _ => 0
        };

        if (adj > 0 && Raid.TryFindSlot(actor, out var slot))
            _adjusted[slot] -= adj;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        (int, Filter?) x = (AID)spell.Action.ID switch
        {
            AID.PrimeKnowledgeLevelDeath120Cast1 => (60, Filter.Prime),
            AID.PrimeKnowledgeLevelDeath180Cast1 => (90, Filter.Prime),
            AID.KnowledgeLevel3Flare120Cast1 => (60, Filter.N3),
            AID.KnowledgeLevel3Flare180Cast1 => (90, Filter.N3),
            AID.KnowledgeLevel4Holy120Cast1 => (60, Filter.N4),
            AID.KnowledgeLevel4Holy180Cast1 => (90, Filter.N4),
            AID.KnowledgeLevel5Death120Cast1 => (60, Filter.N5),
            AID.KnowledgeLevel5Death180Cast1 => (90, Filter.N5),
            _ => (0, null)
        };

        if (x.Item2 != null && !_casters.Any(c => c.Caster.CastInfo!.Action.ID == spell.Action.ID))
            _casters.Add((caster, x.Item2.Value, x.Item1));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PrimeKnowledgeLevelDeath120Cast1 or AID.PrimeKnowledgeLevelDeath180Cast1 or AID.KnowledgeLevel3Flare120Cast1 or AID.KnowledgeLevel3Flare180Cast1 or AID.KnowledgeLevel4Holy120Cast1 or AID.KnowledgeLevel4Holy180Cast1 or AID.KnowledgeLevel5Death120Cast1 or AID.KnowledgeLevel5Death180Cast1)
            _casters.RemoveAll(c => c.Caster == caster);
    }
}

class Marginalia(BossModule module) : Components.RaidwideCastDelay(module, AID.MarginaliaCast, AID.Marginalia, 0);

class CoverToCover(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    int seq;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(1);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (seq == 1 && _predicted.Count > 0)
        {
            var p = _predicted[0];
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(p.Origin, p.Rotation, 2, 2, 40), p.Activation.AddSeconds(4.3f));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CoverToCoverFirst)
        {
            seq = 1;
            _predicted.Add(new(new AOEShapeCone(30, 90.Degrees()), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _predicted.Add(new(new AOEShapeCone(30, 90.Degrees()), spell.LocXZ, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 4.3f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CoverToCoverFirst:
                seq = 2;
                if (_predicted.Count > 0)
                    _predicted.RemoveAt(0);
                break;
            case AID.CoverToCoverSecond:
                seq = 0;
                if (_predicted.Count > 0)
                    _predicted.RemoveAt(0);
                break;
        }
    }
}

class UnboundInk(BossModule module) : Components.StandardAOEs(module, AID.UnboundInk, 9);
class BookDrop(BossModule module) : Components.CastTowers(module, AID.BookDrop, 3, 3);
class ThunderII(BossModule module) : Components.StandardAOEs(module, AID.ThunderII, new AOEShapeRect(50, 2.5f))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => base.ActiveAOEs(slot, actor).TakeSpan(TimeSpan.FromSeconds(1));
}
class FireII(BossModule module) : Components.StandardAOEs(module, AID.FireII, new AOEShapeCone(60, 22.5f.Degrees()));
class ArcaneRule(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _imminent = [];
    readonly List<AOEInstance> _future = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => [.. _future.Take(4), .. _imminent.Select(i => i with { Color = ArenaColor.Danger })];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var i in _imminent)
            hints.AddForbiddenZone(i.Shape.Distance(i.Origin, i.Rotation), i.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.QuadRule)
        {
            _imminent.Add(new(new AOEShapeCross(25, 5), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            for (var j = 0; j < 4; j++)
            {
                for (var i = 0; i < 4; i++)
                {
                    var rot = (90 * i).Degrees();
                    var adv1 = new WDir(0, 5).Rotate(rot);
                    var adv2 = new WDir(0, 6).Rotate(rot);
                    _future.Add(new(new AOEShapeRect(6, 25), spell.LocXZ + adv1 + adv2 * j, rot, Module.CastFinishAt(spell, 2 * (j + 1))));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.QuadRule or AID.HorizontalRule)
        {
            if (_imminent.Count > 0)
                _imminent.RemoveAt(0);
            if (_imminent.Count == 0 && _future.Count >= 4)
            {
                _imminent.AddRange(_future.Take(4));
                _future.RemoveRange(0, 4);
            }
        }
    }
}

class Blot(BossModule module) : Components.StandardAOEs(module, AID.BlotAOE, 15, 6);

class ArbatelStates : StateMachineBuilder
{
    public ArbatelStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<KnowledgeLevelCorrection>()
            .ActivateOnEnter<KnowledgeLevel>()
            .ActivateOnEnter<Invincible>()
            .ActivateOnEnter<Summon>()
            .ActivateOnEnter<Marginalia>()
            .ActivateOnEnter<CoverToCover>()
            .ActivateOnEnter<UnboundInk>()
            .ActivateOnEnter<BookDrop>()
            .ActivateOnEnter<ThunderII>()
            .ActivateOnEnter<FireII>()
            .ActivateOnEnter<ArcaneRule>()
            .ActivateOnEnter<Blot>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14520)]
public class Arbatel(WorldState ws, Actor primary) : CEModule(ws, primary, new(659, 659), new ArenaBoundsCircle(24.5f));
