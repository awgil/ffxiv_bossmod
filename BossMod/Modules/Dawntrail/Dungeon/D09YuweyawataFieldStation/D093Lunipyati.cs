namespace BossMod.Dawntrail.Dungeon.D09YuweyawataFieldStation.D093Lunipyati.cs;

public enum OID : uint
{
    Boss = 0x464C, // R5.98
    Boulder1 = 0x1EBCC0, // R0.5
    Boulder2 = 0x1EBCC1, // R0.5
    Boulder3 = 0x1EBCC2, // R0.5
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 40621, // Boss->player, no cast, single-target
    Teleport = 40620, // Boss->location, no cast, single-target

    RagingClawVisual = 40612, // Boss->self, 5.0+0,4s cast, single-target
    RagingClawFirst = 40613, // Helper->self, 5.4s cast, range 45 180-degree cone, 5 repeats, 6 hits in total
    RagingClawRepeat = 40614, // Helper->self, no cast, range 45 180-degree cone

    LeporineLoaf = 40603, // Boss->self, 5.0s cast, range 60 circle, raidwide
    LeapingEarthVisual1 = 40662, // Helper->self, 7.0s cast, range 15 circle
    LeapingEarthVisual2 = 40661, // Helper->self, 5.0s cast, range 15 circle
    LeapingEarth = 40606, // Helper->self, 1.5s cast, range 5 circle

    BoulderDanceFirst1 = 40607, // Helper->location, 6.0s cast, range 7 circle
    BoulderDanceFirst2 = 40608, // Helper->location, 7.4s cast, range 7 circle
    BoulderDanceRest = 40609, // Helper->location, no cast, range 7 circle

    JaggedEdge = 40615, // Helper->players, 5.0s cast, range 6 circle, spread
    CraterCarveVisual = 40604, // Boss->location, 7.0+2,2s cast, single-target
    CraterCarve = 40605, // Helper->location, 9.2s cast, range 11 circle
    BeastlyRoar = 40610, // Boss->location, 8.0s cast, range 60 circle, raidwide
    RockBlast = 40611, // Helper->self, 1.0s cast, range 5 circle
    TuraliStoneIV = 40616, // Helper->players, 5.0s cast, range 6 circle, stack
    SonicHowl = 40618, // Boss->self, 5.0s cast, range 60 circle, raidwide
    Slabber = 40619 // Boss->player, 5.0s cast, single-target, tankbuster
}

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeDonut donut = new(15f, 35f);
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LeporineLoaf)
        {
            var center = Arena.Center;
            _aoe = [new(donut, center, default, Module.CastFinishAt(spell, 0.9d), shapeDistance: donut.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x19 && state == 0x00020001u)
        {
            SetArena(new ArenaBoundsCustom([new Polygon(new(34f, -710f), 15, 64)]));
        }
        else if (index == 0x11 && state == 0x00800040u)
        {
            SetArena(new ArenaBoundsCustom([new DonutV(new(34f, -710f), 11, 15, 64)]));
        }

        void SetArena(ArenaBoundsCustom bounds)
        {
            Arena.Bounds = bounds;
            Arena.Center = bounds.Center;
            _aoe = [];
        }
    }
}

sealed class CraterCarve(BossModule module) : Components.SimpleAOEs(module, (uint)AID.CraterCarve, 11f, riskyWithSecondsLeft: 2.5d);

sealed class RagingClaw(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance[] AOE = [];
    private static readonly AOEShapeCone cone = new(45f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOE;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RagingClawFirst)
        {
            AOE = [new(cone, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.RagingClawFirst:
            case (uint)AID.RagingClawRepeat:
                if (++NumCasts == 6)
                {
                    AOE = [];
                    NumCasts = 0;
                }
                break;
        }
    }
}

sealed class BoulderDance(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(6)];
    private static readonly AOEShapeCircle circle = new(7f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.BoulderDanceFirst1 or (uint)AID.BoulderDanceFirst2)
        {
            _aoes.Add(new(circle, spell.LocXZ, default, Module.CastFinishAt(spell)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.BoulderDanceFirst1:
            case (uint)AID.BoulderDanceFirst2:
            case (uint)AID.BoulderDanceRest:
                if (++NumCasts == 30)
                {
                    _aoes.Clear();
                    NumCasts = 0;
                }
                break;
        }
    }
}

sealed class JaggedEdge(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.JaggedEdge, 6f)
{
    private readonly RagingClaw _aoe = module.FindComponent<RagingClaw>()!;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoe.AOE.Length != 0 && Spreads.Count != 0 && !IsSpreadTarget(actor))
        {
            hints.AddForbiddenZone(new SDInvertedRect(Module.PrimaryActor.Position, Module.PrimaryActor.Rotation, default, 1f, 50f), Spreads.Ref(0).Activation);
            return;
        }
        base.AddAIHints(slot, actor, assignment, hints);
    }
}

sealed class TuraliStoneIV(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.TuraliStoneIV, 6f, 4, 4);
sealed class LeporineLoafSonicHowlBeastlyRoar(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.LeporineLoaf, (uint)AID.BeastlyRoar, (uint)AID.SonicHowl]);
sealed class BeastlyRoar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BeastlyRoar, 25f);
sealed class Slabber(BossModule module) : Components.SingleTargetCast(module, (uint)AID.Slabber);

sealed class LeapingEarth(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(16)];
    private readonly AOEShapeCircle circle = new(5f);
    private readonly List<float> angles = [with(4)];
    private readonly WPos[] spiralSmallPoints = [new(34f, -710f), new(31.8f, -715.5f), new(35, -721.7f), new(41, -722.5f)];
    private readonly WPos[] spiralBigPoints = [new(34f, -710f), new(28.7f, -708.2f), new(29.4f, -714), new(35.4f, -715.8f),
    new(40f, -711f), new(38.7f, -705f), new(34f, -701.5f), new(28f, -701.4f), new(24f, -704.399f), new(22f, -709.7f), new(23.1f, -715.099f),
    new(26.5f, -719.499f), new(32f, -721.699f), new(38f, -721.5f), new(43f, -717.999f), new(45.7f, -712.699f), new(45.9f, -706.699f),
    new(42.9f, -701.2f), new(38.5f, -697f), new(32.5f, -695.199f)];
    private int maxCasts;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > maxCasts ? maxCasts : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LeapingEarthVisual1)
        {
            angles.Add(spell.Rotation.Rad);
            if (angles.Count < 4)
            {
                return;
            }
            var total = 0f;
            for (var i = 0; i < 4; ++i)
            {
                total += angles[i];
            }
            if ((int)total is 4 or -1)
            {
                var center = Arena.Center;
                for (var i = 0; i < 4; ++i)
                {
                    AddAOEs(WPos.GenerateRotatedVertices(center, spiralSmallPoints, angles[i] * Angle.RadToDeg));
                }
            }
            else if ((int)(2 * total) == 3)
            {
                GenerateAOEsForMixedPattern(-45f, -135f);
            }
            else
            {
                GenerateAOEsForMixedPattern(45f, -45f);
            }
            SortHelpers.SortAOEByActivation(_aoes);
            angles.Clear();
            maxCasts = 16;
        }
        else if (spell.Action.ID == (uint)AID.LeapingEarthVisual2)
        {
            var rotatedPoints = WPos.GenerateRotatedVertices(Arena.Center, spiralBigPoints, spell.Rotation.Rad * Angle.RadToDeg);
            for (var i = 0; i < 20; ++i)
            {
                _aoes.Add(new(circle, rotatedPoints[i], default, WorldState.FutureTime(4.5d + 0.25d * i)));
            }
            maxCasts = 10;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.LeapingEarth)
        {
            _aoes.RemoveAt(0);
        }
    }

    private void GenerateAOEsForMixedPattern(float intercardinalOffset, float cardinalOffset)
    {
        for (var i = 0; i < 4; ++i)
        {
            var angle = angles[i];
            var deg = angle * Angle.RadToDeg;
            var isIntercardinal = false;

            for (var j = 0; j < 4; ++j)
            {
                var angle2 = Angle.AnglesIntercardinals[j];
                if (angle2.AlmostEqual(new(angle), Angle.DegToRad))
                {
                    isIntercardinal = true;
                    break;
                }
            }

            var adjustedAngle = isIntercardinal ? deg + intercardinalOffset : deg + cardinalOffset;
            AddAOEs(WPos.GenerateRotatedVertices(Arena.Center, spiralSmallPoints, adjustedAngle));
        }
    }

    private void AddAOEs(WPos[] points)
    {
        for (var i = 0; i < 4; ++i)
        {
            _aoes.Add(new(circle, points[i], default, WorldState.FutureTime(6.5d + 0.2d * i)));
        }
    }
}

sealed class RockBlast(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(15)];
    private readonly AOEShapeCircle circle = new(5f);
    private readonly WPos[] clockPositions = [new(34f, -697f), new(48f, -710f), new(21f, -710f), new(34f, -724f)];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 8 ? 8 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count == 0 && spell.Action.ID == (uint)AID.RockBlast)
        {
            var isClockwise = DetermineClockwise(caster, spell.Rotation);
            var dir = (isClockwise ? 1 : -1) * 22.5f;
            var pos = caster.Position;
            var center = Arena.Center;
            for (var i = 0; i < 15; ++i)
            {
                _aoes.Add(new(circle, WPos.RotateAroundOrigin(dir * i, center, pos), default, Module.CastFinishAt(spell, 0.6d * i)));
            }
        }
        bool DetermineClockwise(Actor caster, Angle rotation)
        {
            for (var i = 0; i < 4; ++i)
            {
                if (caster.Position.AlmostEqual(clockPositions[i], 1f))
                {
                    return rotation.AlmostEqual(Angle.AnglesCardinals[i], Angle.DegToRad);
                }
            }
            return false;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (_aoes.Count != 0 && spell.Action.ID == (uint)AID.RockBlast)
        {
            _aoes.RemoveAt(0);
        }
    }
}

sealed class D093LunipyatiStates : StateMachineBuilder
{
    public D093LunipyatiStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<CraterCarve>()
            .ActivateOnEnter<RagingClaw>()
            .ActivateOnEnter<BoulderDance>()
            .ActivateOnEnter<JaggedEdge>()
            .ActivateOnEnter<TuraliStoneIV>()
            .ActivateOnEnter<LeporineLoafSonicHowlBeastlyRoar>()
            .ActivateOnEnter<BeastlyRoar>()
            .ActivateOnEnter<Slabber>()
            .ActivateOnEnter<LeapingEarth>()
            .ActivateOnEnter<RockBlast>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008u, NameID = 13610u, SortOrder = 9)]
public sealed class D093Lunipyati : BossModule
{
    public D093Lunipyati(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D093Lunipyati(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        WPos[] vertices = [new(34.15f, -738.36f), new(36.85f, -737.85f), new(39.44f, -736.92f), new(40.84f, -736.68f), new(41.75f, -736.22f),
            new(42.39f, -735.99f), new(43.04f, -735.95f), new(43.74f, -735.84f), new(44.86f, -735.3f), new(46.17f, -735.07f),
            new(46.8f, -735.05f), new(47.42f, -734.84f), new(48.12f, -734.77f), new(48.61f, -734.26f), new(48.63f, -733.53f),
            new(48.34f, -732.98f), new(48.02f, -731.79f), new(48.12f, -729.75f), new(49.66f, -725.05f), new(50.65f, -724.07f),
            new(51.17f, -723.83f), new(51.85f, -723.89f), new(52.3f, -723.62f), new(53.32f, -722.67f), new(53.71f, -722.14f),
            new(54.29f, -721.76f), new(54.91f, -721.47f), new(55.4f, -721.37f), new(55.59f, -720.7f), new(55.68f, -720.06f),
            new(55.49f, -719.36f), new(55.19f, -718.71f), new(55.4f, -717.4f), new(55.68f, -716.89f), new(56.5f, -715.77f),
            new(56.78f, -715.15f), new(56.61f, -714.49f), new(56.81f, -713.88f), new(57.21f, -713.3f), new(57.42f, -712.84f),
            new(57.74f, -712.32f), new(58.32f, -712.09f), new(58.97f, -711.76f), new(58.9f, -711.19f), new(58.39f, -710.99f),
            new(56.9f, -706.96f), new(56.48f, -706.52f), new(56.21f, -705.87f), new(56.01f, -705.19f), new(55.94f, -704.45f),
            new(55.94f, -703.77f), new(56.23f, -703.18f), new(56.39f, -702.54f), new(56.81f, -702.06f), new(56.61f, -701.59f),
            new(57.35f, -698.35f), new(57.76f, -698.05f), new(57.94f, -696.91f), new(58.16f, -696.22f), new(58.41f, -695.63f),
            new(58.18f, -694.98f), new(57.83f, -694.42f), new(57.32f, -693.94f), new(54.91f, -692.32f), new(54.42f, -691.87f),
            new(53.55f, -690.85f), new(51.17f, -688.42f), new(50.29f, -687.46f), new(49.78f, -687.02f), new(47.9f, -685.06f),
            new(47.55f, -685.48f), new(47.31f, -686.09f), new(46.92f, -686.59f), new(46.46f, -687.02f), new(45.83f, -687.11f),
            new(45.42f, -687.41f), new(44.88f, -687.61f), new(44.24f, -687.69f), new(43.67f, -688.19f), new(43.46f, -688.8f),
            new(42.64f, -689.67f), new(42.03f, -689.9f), new(41.45f, -689.85f), new(40.29f, -689.65f), new(39.98f, -689.26f),
            new(39.68f, -688.63f), new(38.73f, -687.74f), new(38.37f, -687.29f), new(38.29f, -686.04f), new(38.39f, -685.43f),
            new(37.98f, -684.87f), new(37.36f, -684.45f), new(36.85f, -684.34f), new(36.12f, -684.43f), new(35.53f, -684.71f),
            new(34.22f, -684.95f), new(33.52f, -684.98f), new(32.92f, -684.7f), new(32.32f, -684.22f), new(31.97f, -684.68f),
            new(31.74f, -685.13f), new(31.57f, -685.61f), new(31.12f, -686.11f), new(30.06f, -686.95f), new(29.44f, -686.86f),
            new(28.75f, -686.81f), new(27.71f, -687.55f), new(27.26f, -688.1f), new(26.77f, -688.45f), new(26.14f, -688.75f),
            new(24.79f, -688.98f), new(24.36f, -689.35f), new(23.69f, -689.59f), new(23.24f, -689.86f), new(22.56f, -690.06f),
            new(21.83f, -690.13f), new(21.31f, -689.96f), new(20.71f, -689.69f), new(20.14f, -689.49f), new(19.71f, -689.02f),
            new(19.01f, -688.81f), new(18.54f, -689.27f), new(18.24f, -689.85f), new(17.89f, -690.2f), new(18.01f, -690.71f),
            new(18.26f, -691.32f), new(18.66f, -691.7f), new(19.04f, -692.23f), new(19.1f, -692.9f), new(19.2f, -693.53f),
            new(18.87f, -694.1f), new(18.65f, -694.68f), new(18.04f, -695.04f), new(17.53f, -695.59f), new(17.02f, -695.99f),
            new(16.46f, -696.36f), new(15.49f, -698), new(14.49f, -698.72f), new(13.95f, -698.88f), new(12.86f, -699.12f),
            new(12.21f, -699.02f), new(11.5f, -699.98f), new(10.96f, -700.22f), new(10.24f, -700.31f), new(9.65f, -700.56f),
            new(9.06f, -700.54f), new(8.46f, -700.28f), new(7.79f, -700.24f), new(7.46f, -700.64f), new(7.24f, -701.11f),
            new(6.8f, -702.48f), new(5.99f, -704.32f), new(5.66f, -705.61f), new(5.54f, -706.97f), new(5.73f, -710.37f),
            new(5.99f, -711), new(6.31f, -711.57f), new(6.77f, -712.07f), new(7.13f, -712.55f), new(7.36f, -713.05f),
            new(7.85f, -712.85f), new(8.51f, -712.76f), new(9.15f, -712.46f), new(9.7f, -712.45f), new(10.42f, -712.61f),
            new(11.53f, -713.39f), new(11.85f, -714.58f), new(12.19f, -715.21f), new(12.49f, -716.43f), new(12.79f, -716.86f),
            new(13.62f, -717.77f), new(13.94f, -718.95f), new(13.9f, -719.54f), new(13.75f, -720.24f), new(13.75f, -720.74f),
            new(14.06f, -722.03f), new(14.12f, -722.72f), new(13.95f, -723.44f), new(13.91f, -723.98f), new(13.8f, -724.63f),
            new(13.37f, -725.1f), new(13, -725.68f), new(13.04f, -726.39f), new(12.92f, -727.82f), new(13.56f, -728.87f),
            new(15.96f, -732.29f), new(18.22f, -733.88f), new(20.71f, -735.26f), new(24.26f, -737.13f), new(25.6f, -737.63f),
            new(26.83f, -737.85f), new(28.9f, -737.89f), new(31.82f, -738.23f), new(32.55f, -738.24f), new(33.9f, -738.37f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }
}
