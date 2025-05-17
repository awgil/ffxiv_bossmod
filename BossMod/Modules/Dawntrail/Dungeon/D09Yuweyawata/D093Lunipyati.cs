namespace BossMod.Dawntrail.Dungeon.D09Yuweyawata.D093Lunipyati;

public enum OID : uint
{
    Boss = 0x464C, // R5.980, x1
    Helper = 0x233C, // R0.500, x28, Helper type
    BoulderDance1 = 0x1EBCC0, // R0.500, x0 (spawn during fight), EventObj type
    BoulderDance2 = 0x1EBCC1, // R0.500, x0 (spawn during fight), EventObj type
    BoulderDance3 = 0x1EBCC2, // R0.500, x0 (spawn during fight), EventObj type
}

public enum AID : uint
{
    AutoAttack = 40621, // Boss->player, no cast, single-target
    Teleport = 40620, // Boss->location, no cast, single-target
    RagingClaw = 40612, // Boss->self, 5.0+0.4s cast, single-target, visual (multi-hit cleave)
    RagingClawFirst = 40613, // Helper->self, 5.4s cast, range 45 180-degree cone
    RagingClawRest = 40614, // Helper->self, no cast, range 45 180-degree cone
    LeporineLoaf = 40603, // Boss->self, 5.0s cast, range 60 circle, raidwide + curves start
    BoulderDancePrimary = 40607, // Helper->location, 6.0s cast, range 7 circle
    BoulderDanceSecondary = 40608, // Helper->location, 7.4s cast, range 7 circle
    BoulderDanceRepeat = 40609, // Helper->location, no cast, range 7 circle
    JaggedEdge = 40615, // Helper->player, 5.0s cast, range 6 circle spread
    LeapingEarthSpiral = 40661, // Helper->self, 5.0s cast, range 15 circle
    LeapingEarthCurve = 40662, // Helper->self, 7.0s cast, range 15 circle, visual (4 aoes in a curve from center to edge)
    LeapingEarth = 40606, // Helper->self, 1.5s cast, range 5 circle
    CraterCarve = 40604, // Boss->location, 7.0+2.2s cast, single-target, visual (create hole)
    CraterCarveAOE = 40605, // Helper->location, 9.2s cast, range 11 circle
    BeastlyRoar = 40610, // Boss->location, 8.0s cast, range 60 circle with ? falloff
    RockBlast = 40611, // Helper->self, 1.0s cast, range 5 circle
    TuraliStone = 40616, // Helper->players, 5.0s cast, range 6 circle stack
    SonicHowl = 40618, // Boss->self, 5.0s cast, range 60 circle, raidwide
    Slabber = 40619, // Boss->player, 5.0s cast, single-target, tankbuster
}

public enum IconID : uint
{
    JaggedEdge = 139, // player->self
    TuraliStone = 161, // player->self
    Slabber = 218, // player->self
}

class RagingClaw(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShapeCone _shape = new(45, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RagingClawFirst)
        {
            _aoe = new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell));
            NumCasts = 0;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RagingClawRest && ++NumCasts >= 5)
            _aoe = null;
    }
}

class LeporineLoaf(BossModule module) : Components.RaidwideCast(module, AID.LeporineLoaf);

class BoulderDance(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(AOEInstance aoe, int remaining)> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(7);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Select(aoe => aoe.aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.BoulderDancePrimary or AID.BoulderDanceSecondary)
        {
            _aoes.Add((new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)), 4));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.BoulderDanceRepeat)
        {
            var index = _aoes.FindIndex(aoe => aoe.aoe.Origin.AlmostEqual(caster.Position, 1));
            if (index >= 0)
            {
                ref var elem = ref _aoes.Ref(index);
                if (--elem.remaining <= 0)
                    _aoes.RemoveAt(index);
            }
            else
            {
                ReportError($"Failed to find aoe at {caster.Position}");
            }
        }
    }
}

class JaggedEdge(BossModule module) : Components.SpreadFromCastTargets(module, AID.JaggedEdge, 6);

class LeapingEarthCurve(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Angle direction, DateTime activation, int numCasts)> _curves = [];

    private static readonly AOEShapeCircle _shape = new(5);
    private static readonly (float distance, Angle direction)[] _offsets = [(0, default), (5.92f, 66.8f.Degrees()), (11.74f, 40.1f.Degrees()), (14.33f, 15.8f.Degrees())];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _curves.Take(4))
            foreach (var off in _offsets.Skip(c.numCasts))
                yield return new(_shape, Module.Center + off.distance * (c.direction + off.direction).ToDirection(), default, c.activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LeapingEarthCurve)
            _curves.Add((spell.Rotation, Module.CastFinishAt(spell), 0));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LeapingEarth && _curves.Count > 0)
        {
            var index = -1;
            var advance = 1;

            for (var i = 0; i < Math.Min(_curves.Count, 4); i++)
            {
                var c = _curves[i];
                bool matches(int cnt) => spell.LocXZ.AlmostEqual(Module.Center + _offsets[cnt].distance * (c.direction + _offsets[cnt].direction).ToDirection(), 1);

                if (matches(c.numCasts))
                {
                    index = i;
                    break;
                }

                // casts 0/1 and 2/3 can happen on the same frame and might even happen in reverse order, so we skip the cast if we get a later one early
                if (c.numCasts < 3 && matches(c.numCasts + 1))
                {
                    index = i;
                    advance = 2;
                    break;
                }
            }
            if (index >= 0)
            {
                ref var curve = ref _curves.Ref(index);
                curve.numCasts += advance;
                if (curve.numCasts >= 4)
                    _curves.RemoveAt(index);
            }
            else
            {
                ReportError($"Failed to find curve at {spell.LocXZ}");
            }
        }
    }
}

class LeapingEarthSpiral(BossModule module) : Components.GenericAOEs(module)
{
    private WDir _direction;
    private DateTime _firstActivation;

    private static readonly AOEShapeCircle _shape = new(5);
    private static readonly WDir[] _offsets = [default, new(-5.3f, 1.8f), new(-4.6f, -4f), new(1.4f, -5.8f), new(6.0f, -1.0f),
        new(4.7f, 5.0f), new(0.0f, 8.5f), new(-6.0f, 8.6f), new(-10.0f, 5.6f), new(-12.0f, 0.3f),
        new(-10.9f, -5.1f), new(-7.5f, -9.5f), new(-2.0f, -11.7f), new(4.0f, -11.5f), new(9.0f, -8.0f),
        new(11.7f, -2.7f), new(11.9f, 3.3f), new(8.9f, 8.8f), new(4.5f, 13.0f), new(-1.5f, 14.8f)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_direction == default || NumCasts >= _offsets.Length)
            yield break;
        for (int i = NumCasts, max = Math.Min(_offsets.Length, NumCasts + 10); i < max; ++i)
            yield return new(_shape, Module.Center + _offsets[i].Rotate(_direction), default, _firstActivation.AddSeconds(i * 0.3f));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LeapingEarthSpiral)
        {
            NumCasts = 0;
            _direction = (-spell.Rotation).ToDirection();
            _firstActivation = WorldState.FutureTime(4.5f);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.LeapingEarth && _direction != default && NumCasts < _offsets.Length)
        {
            var expected = Module.Center + _offsets[NumCasts].Rotate(_direction);
            if (!caster.Position.AlmostEqual(expected, 0.1f))
                ReportError($"Unexpected @ {caster.Position} instead of {expected}");
            ++NumCasts;
        }
    }
}

class CraterCarve(BossModule module) : Components.StandardAOEs(module, AID.CraterCarveAOE, new AOEShapeCircle(11))
{
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        base.OnCastFinished(caster, spell);
        if (spell.Action == WatchedAction)
            Module.Arena.Bounds = D093Lunipyati.HoleBounds;
    }
}

class BeastlyRoar(BossModule module) : Components.StandardAOEs(module, AID.BeastlyRoar, 25, warningText: "GTFO from proximity marker!"); // TODO: verify falloff

class RockBlast(BossModule module) : Components.GenericAOEs(module, AID.RockBlast)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(10);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && _aoes.Count == 0)
        {
            var toCaster = caster.Position - Module.Center;
            var cw = toCaster.OrthoL().Dot(spell.Rotation.ToDirection()) < 0;
            var delta = (cw ? -22.5f : 22.5f).Degrees().ToDirection();
            for (int i = 0; i < 15; ++i)
            {
                _aoes.Add(new(_shape, Module.Center + toCaster, default, Module.CastFinishAt(spell, i * 0.6f)));
                toCaster = toCaster.Rotate(delta);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && _aoes.Count > 0)
        {
            if (!_aoes[0].Origin.AlmostEqual(caster.Position, 1))
                ReportError($"Unexpected: {caster.Position} instead of {_aoes[0].Origin}");
            _aoes.RemoveAt(0);
        }
    }
}

class TuraliStone(BossModule module) : Components.StackWithCastTargets(module, AID.TuraliStone, 6, 4);
class SonicHowl(BossModule module) : Components.RaidwideCast(module, AID.SonicHowl);
class Slabber(BossModule module) : Components.SingleTargetCast(module, AID.Slabber);

class D093LunipyatiStates : StateMachineBuilder
{
    public D093LunipyatiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<RagingClaw>()
            .ActivateOnEnter<LeporineLoaf>()
            .ActivateOnEnter<BoulderDance>()
            .ActivateOnEnter<JaggedEdge>()
            .ActivateOnEnter<LeapingEarthCurve>()
            .ActivateOnEnter<LeapingEarthSpiral>()
            .ActivateOnEnter<CraterCarve>()
            .ActivateOnEnter<BeastlyRoar>()
            .ActivateOnEnter<RockBlast>()
            .ActivateOnEnter<TuraliStone>()
            .ActivateOnEnter<SonicHowl>()
            .ActivateOnEnter<Slabber>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008, NameID = 13610)]
public class D093Lunipyati(WorldState ws, Actor primary) : BossModule(ws, primary, new(34, -710), NormalBounds)
{
    public static readonly ArenaBoundsCircle NormalBounds = new(15);
    public static readonly ArenaBoundsCustom HoleBounds = BuildHoleBounds();

    private static ArenaBoundsCustom BuildHoleBounds()
    {
        var poly = new RelPolygonWithHoles([.. CurveApprox.Circle(15, 0.05f)]);
        poly.AddHole(CurveApprox.Circle(11, 0.05f));
        return new(NormalBounds.Radius, new([poly]));
    }
}
