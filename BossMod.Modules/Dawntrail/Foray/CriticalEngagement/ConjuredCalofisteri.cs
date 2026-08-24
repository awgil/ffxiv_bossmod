namespace BossMod.Dawntrail.Foray.CriticalEngagement.ConjuredCalofisteri;

public enum OID : uint
{
    Boss = 0x4BB8, // R5.500, x1
    Helper = 0x233C, // R0.500, x16, Helper type
    Entanglement = 0x4BB9, // R4.440, x0 (spawn during fight)
    LitheLock = 0x4BBA, // R1.000, x0 (spawn during fight)
    DashingCutUnk = 0x4BBB, // R1.000, x0 (spawn during fight)
    DashingCutMarker = 0x4BBC, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50122, // Boss->player, no cast, single-target
    AuraBurstCast = 47079, // Boss->self, 5.0s cast, single-target
    AuraBurst = 47080, // Helper->self, no cast, ???
    AsymmetricCoifChange1 = 47054, // Boss->self, 3.0s cast, single-target
    AsymmetricCoifChange2 = 47055, // Boss->self, 3.0s cast, single-target
    CoifChange1 = 47056, // Boss->self, no cast, single-target
    CoifChange2 = 47057, // Boss->self, no cast, single-target
    DualCutBossL1 = 47058, // Boss->self, 2.0s cast, single-target
    DualCutBossR1 = 47059, // Boss->self, 2.0s cast, single-target
    DualCutBossL2 = 47060, // Boss->self, no cast, single-target
    DualCutBossR2 = 47061, // Boss->self, no cast, single-target
    DualCutFirst = 50691, // Helper->self, 2.8s cast, range 60 180-degree cone
    DualCutSecond = 50692, // Helper->self, 4.8s cast, range 60 180-degree cone
    ResettingSpray1 = 47062, // Boss->self, no cast, single-target
    ResettingSpray2 = 47063, // Boss->self, no cast, single-target
    ResettingSpray3 = 47064, // Boss->self, no cast, single-target
    ResettingSpray4 = 47065, // Boss->self, no cast, single-target
    Extension = 47069, // Boss->self, 3.0s cast, single-target
    Graft = 47070, // 4BBA->self, 3.0s cast, range 6 circle
    BalefulBlowout = 47071, // Boss->self, 5.0s cast, single-target
    MaliciousWeave = 47072, // 4BB9->self, 5.5s cast, range 6 circle
    Garrote = 47073, // 4BB9->self, 10.0s cast, range 6 circle
    GarroteInstant = 47074, // 4BB9->self, no cast, single-target
    DashingCutMarker = 47066, // 4BBC->location, no cast, single-target
    DashingCutSlowCast = 47067, // Boss->location, 6.0s cast, single-target
    DashingCutSlow = 49052, // Helper->location, 6.5s cast, width 10 rect charge
    DashingCutFastCast = 47068, // Boss->location, 0.5s cast, single-target
    DashingCutFast = 49053, // Helper->location, 1.0s cast, width 10 rect charge
    HairShearsCast = 47075, // Boss->self, 5.0s cast, single-target
    HairShears1 = 47077, // Helper->self, 5.0s cast, range 60 width 4 cross
    HairShears2 = 47599, // Helper->self, no cast, range 60 width 4 cross
    HairShearsCircleUnk = 47076, // Helper->self, 5.0s cast, range 10 circle, don't know what this does, doesn't hit players
    MaliciousWeaveFast = 47078, // 4BB9->self, 1.0s cast, range 6 circle
}

class AuraBurst(BossModule module) : Components.RaidwideCastDelay(module, AID.AuraBurstCast, AID.AuraBurst, 0.8f);

class Graft(BossModule module) : Components.StandardAOEs(module, AID.Graft, 6);
class MaliciousWeave(BossModule module) : Components.StandardAOEs(module, AID.MaliciousWeave, 6);
class Entanglement(BossModule module) : Components.Adds(module, (uint)OID.Entanglement, 1, true);

class DualCut(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DualCutFirst or AID.DualCutSecond)
        {
            _predicted.Add(new(new AOEShapeCone(60, 90.Degrees()), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _predicted.SortBy(p => p.Activation);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_predicted.Count > 1)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(_predicted[0].Origin, _predicted[0].Rotation, 2, 2, 40), _predicted[1].Activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DualCutFirst or AID.DualCutSecond)
        {
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class DashingCut(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(WPos From, WPos To, DateTime Activation, bool Imminent)> _sources = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (f, t, a, i) in _sources)
            yield return new(new AOEShapeRect((t - f).Length(), 5), f, (t - f).ToAngle(), a, Risky: i);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DashingCutMarker)
        {
            if (_sources.Count == 0)
                _sources.Add((Module.PrimaryActor.Position, caster.Position, WorldState.FutureTime(5.1f), true));
            _sources.Add((caster.Position, spell.TargetXZ, WorldState.FutureTime(_sources.Count == 1 ? 12.1f : 14.1f), false));
        }

        if ((AID)spell.Action.ID is AID.DashingCutSlow or AID.DashingCutFast)
        {
            if (_sources.Count > 0)
                _sources.RemoveAt(0);
        }

        if ((AID)spell.Action.ID == AID.DualCutSecond)
        {
            if (_sources.Count > 0)
                _sources.Ref(0).Imminent = true;
        }
    }
}

class HairShears(BossModule module) : Components.GenericAOEs(module, AID.HairShears2)
{
    readonly List<AOEInstance> _sources = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _sources;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.HairShears1)
            _sources.Add(new(new AOEShapeCross(60, 2), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell, 1)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HairShears2)
            _sources.RemoveAll(s => s.Origin.AlmostEqual(caster.Position, 1) && s.Rotation.AlmostEqual(spell.Rotation, 0.1f));
    }
}

class ConjuredCalofisteriStates : StateMachineBuilder
{
    public ConjuredCalofisteriStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AuraBurst>()
            .ActivateOnEnter<Entanglement>()
            .ActivateOnEnter<DualCut>()
            .ActivateOnEnter<Graft>()
            .ActivateOnEnter<MaliciousWeave>()
            .ActivateOnEnter<DashingCut>()
            .ActivateOnEnter<HairShears>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14517)]
public class ConjuredCalofisteri(WorldState ws, Actor primary) : CEModule(ws, primary, new(-215, -65), new ArenaBoundsCircle(22));

