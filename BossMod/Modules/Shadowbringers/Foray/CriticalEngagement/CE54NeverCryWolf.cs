namespace BossMod.Shadowbringers.Foray.CriticalEngagement.CE54NeverCryWolf;

public enum OID : uint
{
    Boss = 0x319C, // R9.996, x1
    Helper = 0x233C, // R0.500, x18
    IceSprite = 0x319D, // R0.800, spawn during fight
    Icicle = 0x319E, // R3.000, spawn during fight
    Imaginifer = 0x319F, // R0.500, spawn during fight
}

public enum AID : uint
{
    AutoAttack = 6499, // Boss->player, no cast, single-target

    IcePillar = 23581, // Boss->self, 3.0s cast, single-target, viusal
    IcePillarAOE = 23582, // Icicle->self, 3.0s cast, range 4 circle aoe (pillar drop)
    PillarPierce = 23583, // Icicle->self, 3.0s cast, range 80 width 4 rect aoe (pillar fall)
    Shatter = 23584, // Icicle->self, 3.0s cast, range 8 circle aoe (pillar explosion after lunar cry)
    Tramontane = 23585, // Boss->self, 3.0s cast, single-target, visual
    BracingWind = 23586, // IceSprite->self, 9.0s cast, range 60 width 12 rect, visual
    BracingWindAOE = 24787, // Helper->self, no cast, range 60 width 12 rect knock-forward 40
    LunarCry = 23588, // Boss->self, 14.0s cast, range 80 circle LOSable aoe

    ThermalGust = 23589, // Imaginifer->self, 2.0s cast, range 60 width 4 rect aoe (when adds appear)
    GlaciationEnrage = 22881, // Boss->self, 20.0s cast, single-target, visual
    GlaciationEnrageAOE = 23625, // Helper->self, no cast, ???, raidwide (deadly if adds aren't killed)
    AgeOfEndlessFrostFirstCW = 23590, // Boss->self, 5.0s cast, single-target, visual
    AgeOfEndlessFrostFirstCCW = 23591, // Boss->self, 5.0s cast, single-target, visual
    AgeOfEndlessFrostFirstAOE = 23592, // Helper->self, 5.0s cast, range 40 20-degree cone
    AgeOfEndlessFrostRest = 22883, // Boss->self, no cast, single-target
    AgeOfEndlessFrostRestAOE = 23593, // Helper->self, 0.5s cast, range 40 20-degree cone

    StormWithout = 23594, // Boss->self, 5.0s cast, single-target
    StormWithoutAOE = 23595, // Helper->self, 5.0s cast, range 10-40 donut
    StormWithin = 23596, // Boss->self, 5.0s cast, single-target
    StormWithinAOE = 23597, // Helper->self, 5.0s cast, range 10 circle
    AncientGlacier = 23600, // Boss->self, 3.0s cast, single-target, visual
    AncientGlacierAOE = 23601, // Helper->location, 3.0s cast, range 6 circle puddle
    Glaciation = 23602, // Boss->self, 5.0s cast, single-target, visual
    GlaciationAOE = 23603, // Helper->self, 5.6s cast, ???, raidwide

    TeleportBoss = 23621, // Boss->location, no cast, teleport
    TeleportImaginifer = 23622, // Imaginifer->location, no cast, ???, teleport
    ActivateImaginifer = 23623, // Imaginifer->self, no cast, single-target, visual
}

class IcePillar(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IcePillarAOE), new AOEShapeCircle(4));
class PillarPierce(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PillarPierce), new AOEShapeRect(80, 2));
class Shatter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Shatter), new AOEShapeCircle(8));

class BracingWind(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.BracingWind), 40, false, 1, new AOEShapeRect(60, 6), Kind.DirForward)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var length = Module.Bounds.Radius * 2; // casters are at the border, orthogonal to borders
        foreach (var c in Casters)
        {
            hints.AddForbiddenZone(ShapeDistance.Rect(c.Position, c.CastInfo!.Rotation, length, Distance - length, 6), c.CastInfo!.NPCFinishAt);
        }
    }
}

class LunarCry(BossModule module) : Components.CastLineOfSightAOE(module, ActionID.MakeSpell(AID.LunarCry), 80, false)
{
    private readonly List<Actor> _safePillars = [];
    private readonly BracingWind? _knockback = module.FindComponent<BracingWind>();

    public override IEnumerable<Actor> BlockerActors() => _safePillars;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_knockback?.Casters.Count > 0)
            return; // resolve knockbacks first
        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Icicle)
            _safePillars.Add(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if ((AID)spell.Action.ID == AID.PillarPierce)
            _safePillars.Remove(caster);
    }
}

// this AOE only got 2s cast time, but the actors already spawn 4.5s earlier, so we can use that to our advantage
class ThermalGust(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _casters = [];
    private DateTime _activation;

    private static readonly AOEShapeRect _shape = new(60, 2);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(_shape, c.Position, c.CastInfo?.Rotation ?? c.Rotation, c.CastInfo?.NPCFinishAt ?? _activation));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.Imaginifer)
        {
            _casters.Add(actor);
            _activation = WorldState.FutureTime(6.5f);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ThermalGust)
            _casters.Remove(caster);
    }
}

class AgeOfEndlessFrost(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _increment;
    private readonly List<Angle> _angles = [];
    private DateTime _nextActivation;

    private static readonly AOEShapeCone _shape = new(40, 10.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _angles.Select(a => new AOEInstance(_shape, Module.PrimaryActor.Position, a, _nextActivation));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AgeOfEndlessFrostFirstCW:
                _increment = -40.Degrees();
                _nextActivation = spell.NPCFinishAt;
                break;
            case AID.AgeOfEndlessFrostFirstCCW:
                _increment = 40.Degrees();
                _nextActivation = spell.NPCFinishAt;
                break;
            case AID.AgeOfEndlessFrostFirstAOE:
                NumCasts = 0;
                _angles.Add(spell.Rotation);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AgeOfEndlessFrostFirstCCW or AID.AgeOfEndlessFrostFirstCW or AID.AgeOfEndlessFrostRest)
        {
            if (NumCasts == 0)
            {
                _nextActivation = WorldState.FutureTime(2.6f);
            }
            else if (NumCasts < 6)
            {
                _nextActivation = WorldState.FutureTime(2.1f);
            }
            else
            {
                _angles.Clear();
            }

            ++NumCasts;
            for (int i = 0; i < _angles.Count; ++i)
                _angles[i] += _increment;
        }
    }
}

class StormWithout(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.StormWithout), new AOEShapeDonut(10, 40));
class StormWithin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.StormWithin), new AOEShapeCircle(10));
class AncientGlacier(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.AncientGlacierAOE), 6);
class Glaciation(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Glaciation));

class CE54NeverCryWolfStates : StateMachineBuilder
{
    public CE54NeverCryWolfStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IcePillar>()
            .ActivateOnEnter<PillarPierce>()
            .ActivateOnEnter<Shatter>()
            .ActivateOnEnter<BracingWind>()
            .ActivateOnEnter<LunarCry>()
            .ActivateOnEnter<ThermalGust>()
            .ActivateOnEnter<AgeOfEndlessFrost>()
            .ActivateOnEnter<StormWithout>()
            .ActivateOnEnter<StormWithin>()
            .ActivateOnEnter<AncientGlacier>()
            .ActivateOnEnter<Glaciation>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 25)] // bnpcname=9941
public class CE54NeverCryWolf : BossModule
{
    private readonly IReadOnlyList<Actor> _adds;

    public CE54NeverCryWolf(WorldState ws, Actor primary) : base(ws, primary, new(-830, 190), new ArenaBoundsSquare(24))
    {
        _adds = Enemies(OID.Imaginifer);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        base.DrawEnemies(pcSlot, pc);
        Arena.Actors(_adds, ArenaColor.Enemy);
    }
}
