namespace BossMod.Dawntrail.Dungeon.D02WorqorZormor.D022Kahderyor;

public enum OID : uint
{
    Boss = 0x415D, // R7.000, x1
    Helper = 0x233C, // R0.500, x20, Helper type
    CrystallineDebris = 0x415E, // R1.400, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    WindUnbound = 36282, // Boss->self, 5.0s cast, range 60 circle, raidwide
    CrystallineCrush = 36285, // Boss->location, 5.0+1.0s cast, single-target, visual (tower)
    CrystallineCrushAOE = 36153, // Helper->self, 6.3s cast, range 6 circle tower
    CrystallineStorm = 36286, // Boss->self, 3.0+1.0s cast, single-target, visual (rects)
    CrystallineStormAOE = 36290, // Helper->self, 4.0s cast, range 50 width 2 rect
    WindShot = 36284, // Boss->self, 5.5s cast, single-target, visual (donut spreads)
    WindShotAOE = 36296, // Helper->player, 6.0s cast, range ?-10 donut spread
    WindShotFail = 36300, // Helper->player, no cast, single-target, vuln on player outside safe zone
    EarthenShot = 36283, // Boss->self, 5.0+0.5s cast, single-target, visual (circle spreads)
    EarthenShotAOE = 36295, // Helper->player, 6.0s cast, range 6 circle spread
    SeedCrystals = 36291, // Boss->self, 4.5+0.5s cast, single-target, visual (spread + spawn debris)
    SeedCrystalsAOE = 36298, // Helper->player, 5.0s cast, range 6 circle spread
    SharpenedSights = 36287, // Boss->self, 3.0s cast, single-target, visual (gaze buff)
    EyeOfTheFierce = 36297, // Helper->self, 5.0s cast, range 60 circle gaze
    StalagmiteCircle = 36288, // Boss->self, 5.0s cast, single-target, visual (gaze + out)
    StalagmiteCircleAOE = 36293, // Helper->self, 5.0s cast, range 15 circle
    CyclonicRing = 36289, // Boss->self, 5.0s cast, single-target, visual (gaze + in)
    CyclonicRingAOE = 36294, // Helper->self, 5.0s cast, range 8-40 donut
}

public enum IconID : uint
{
    CrystallineCrush = 62, // Helper
    WindShot = 511, // player
    EarthenShot = 169, // player
    SeedCrystals = 311, // player
}

class WindUnbound(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.WindUnbound));

class CrystallineCrush(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.CrystallineCrushAOE), 6, maxSoakers: 4)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Towers.Count > 0)
            hints.AddForbiddenZone(new AOEShapeDonut(6, 40), Towers[0].Position);
    }
}
class CrystallineStorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CrystallineStormAOE), new AOEShapeRect(25, 1, 25));
class WindShot(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.WindShotAOE), new AOEShapeDonut(5, 10), true); // TODO: verify inner radius
class EarthenShot(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.EarthenShotAOE), 6);

class CrystalInOut(BossModule module) : Components.GenericAOEs(module)
{
    private enum Mechanic { None, In, Out }
    private record struct Source(AOEShape InShape, AOEShape OutShape, WPos Center, Angle Rotation);

    private readonly List<Source> _sources = [];
    private Mechanic _mechanic;
    private DateTime _activation;

    // TODO: verify aoe sizes...
    private static readonly AOEShapeCircle _crushIn = new(8);
    private static readonly AOEShapeCircle _crushOut = new(15);
    private static readonly AOEShapeRect _stormIn = new(25, 1, 25);
    private static readonly AOEShapeRect _stormOut = new(25, 7, 25);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_mechanic == Mechanic.None)
            yield break;

        foreach (var s in _sources)
        {
            yield return _mechanic == Mechanic.In
                ? new AOEInstance(s.InShape, s.Center, s.Rotation, _activation, ArenaColor.SafeFromAOE, Risky: false)
                : new AOEInstance(s.OutShape, s.Center, s.Rotation, _activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_mechanic == Mechanic.In && ActiveAOEs(slot, actor).All(c => !c.Check(actor.Position)))
            hints.Add("Get to safe zone!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var aoes = ActiveAOEs(slot, actor);
        var shapes = aoes.Select(s => s.Shape.Distance(s.Origin, s.Rotation)).ToList();
        if (shapes.Count == 0)
            return;

        float distance(WPos p)
        {
            var dist = shapes.Select(s => s(p)).Min();
            return _mechanic == Mechanic.Out ? dist : -dist;
        }
        hints.AddForbiddenZone(distance, _activation);

        // for out-rects, if playing as ranged, duty support loves taking up entire mid, so gtfo...
        if (_mechanic == Mechanic.Out && _sources[0].OutShape == _stormOut && actor.Role is not Role.Tank and not Role.Melee)
            hints.AddForbiddenZone(ShapeDistance.Circle(Module.Center, 12), _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CrystallineCrushAOE:
                _sources.Add(new(_crushIn, _crushOut, caster.Position, spell.Rotation));
                break;
            case AID.CrystallineStormAOE:
                _sources.Add(new(_stormIn, _stormOut, caster.Position, spell.Rotation));
                break;
            case AID.WindShotAOE:
                _mechanic = Mechanic.In;
                _activation = Module.CastFinishAt(spell);
                break;
            case AID.EarthenShotAOE:
                _mechanic = Mechanic.Out;
                _activation = Module.CastFinishAt(spell);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.WindShot:
            case AID.EarthenShot:
                ++NumCasts; // we count visual casts, since there's always one per mechanic
                break;
            case AID.WindShotAOE:
            case AID.EarthenShotAOE:
                _mechanic = Mechanic.None;
                if (NumCasts >= 2)
                {
                    NumCasts = 0;
                    _sources.Clear();
                    _activation = default;
                }
                break;
        }
    }
}

class SeedCrystals(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SeedCrystalsAOE), 6);
class CrystallineDebris(BossModule module) : Components.Adds(module, (uint)OID.CrystallineDebris)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ActiveActors.Any())
            hints.Add("Break debris!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var t in hints.PotentialTargets.Where(e => (OID)e.Actor.OID == OID.CrystallineDebris))
            t.Priority = 1;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var c in ActiveActors)
            Arena.AddCircle(c.Position, 1.4f, ArenaColor.Danger);
    }
}

class EyeOfTheFierce(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.EyeOfTheFierce));
class StalagmiteCircle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.StalagmiteCircleAOE), new AOEShapeCircle(15));
class CyclonicRing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CyclonicRingAOE), new AOEShapeDonut(8, 40));

class D022KahderyorStates : StateMachineBuilder
{
    public D022KahderyorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<WindUnbound>()
            .ActivateOnEnter<CrystallineCrush>()
            .ActivateOnEnter<CrystallineStorm>()
            .ActivateOnEnter<WindShot>()
            .ActivateOnEnter<EarthenShot>()
            .ActivateOnEnter<CrystalInOut>()
            .ActivateOnEnter<SeedCrystals>()
            .ActivateOnEnter<CrystallineDebris>()
            .ActivateOnEnter<EyeOfTheFierce>()
            .ActivateOnEnter<StalagmiteCircle>()
            .ActivateOnEnter<CyclonicRing>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "xan", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 824, NameID = 12703)]
public class D022Kahderyor(WorldState ws, Actor primary) : BossModule(ws, primary, new(-53, -57), new ArenaBoundsCircle(20));
