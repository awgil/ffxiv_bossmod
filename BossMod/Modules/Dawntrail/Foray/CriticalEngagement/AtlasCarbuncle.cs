namespace BossMod.Dawntrail.Foray.CriticalEngagement.AtlasCarbuncle;

public enum OID : uint
{
    Boss = 0x4C4F, // R9.067, x1
    Helper = 0x233C, // R0.500, x20, Helper type
    TopazStone = 0x4C50, // R1.000, x12
    AtlasCarbuncle = 0x4D88, // R1.000, x1
}

public enum AID : uint
{
    DeathWall = 49104, // 4D88->self, no cast, ???
    AutoAttack = 50852, // Boss->player, no cast, single-target
    SonicHowlCast = 48298, // Boss->self, 5.0s cast, ???
    SonicHowl = 49505, // Helper->self, no cast, ???
    TopazStones = 48280, // Boss->self, 3.0s cast, single-target
    TopazRay1 = 48281, // TopazStone->self, 3.0s cast, range 4 circle
    TopazRay2 = 48282, // TopazStone->self, 3.0s cast, range 4 circle
    RubyGlowCast = 48284, // Boss->self, 3.0s cast, ???
    RubyGlow = 50637, // Helper->self, no cast, ???
    ReflectiveCoat = 50418, // Boss->self, 3.0s cast, single-target
    RubyReflection1 = 48285, // Helper->self, no cast, range 20 width 20 rect
    RubyReflection2 = 48286, // Helper->self, no cast, range 40 width 40 rect
    RubyReflection3 = 48287, // Helper->self, no cast, range 40 width 40 rect
    Jump = 48299, // Boss->location, no cast, single-target
    ClawToTailCast = 48294, // Boss->self, 6.0s cast, range 45 180-degree cone
    TailToClawCast = 48295, // Boss->self, 6.0s cast, range 45 180-degree cone
    ClawToTailInstant = 48296, // Boss->self, no cast, range 40 180-degree cone
    TailToClawInstant = 48297, // Boss->self, no cast, range 40 180-degree cone
    StampedeTelegraphRect = 48289, // Helper->self, 2.5s cast, range 40 width 60 rect
    StampedeTelegraphCircle = 48288, // Helper->self, 2.5s cast, range 60 circle
    SpinebreakingStampedeBoss1 = 48291, // Boss->location, 8.0s cast, ???
    SpinebreakingStampedeBoss2 = 48292, // Boss->location, no cast, ???
    SpinebreakingStampedeRect = 49507, // Helper->self, no cast, rect, distance 15 knockback (right or left)
    SpinebreakingStampedeCircle = 49506, // Helper->self, no cast, circle, distance 30 knockback from source
    Unk = 50461, // Boss->self, no cast, single-target
}

// 1EC045 00100020
//  _______
// |   |   |
// |___|___|
// |   |   |
// |___|___|

// 1EC046
// 01000200 (oriented to north, if eventobj is rotated so is pattern)
//  _______
// |_  |_  |
// | | | | |
// | |_| |_|
// |___|___|

// 00100020
//  _______
// | |___  |
// |_____|_|
// | |___  |
// |_____|_|

class SonicHowl(BossModule module) : Components.RaidwideCastDelay(module, AID.SonicHowlCast, AID.SonicHowl, 0.8f);
class RubyGlow(BossModule module) : Components.RaidwideCastDelay(module, AID.RubyGlowCast, AID.RubyGlow, 0.7f);

class ClawToTail(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ClawToTailCast or AID.TailToClawCast)
        {
            _predicted.Add(new(new AOEShapeCone(45, 90.Degrees()), spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell)));
            _predicted.Add(new(new AOEShapeCone(45, 90.Degrees()), spell.LocXZ, spell.Rotation + 180.Degrees(), Module.CastFinishAt(spell, 3.1f)));
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
        if ((AID)spell.Action.ID is AID.ClawToTailCast or AID.ClawToTailInstant or AID.TailToClawCast or AID.TailToClawInstant)
        {
            if (_predicted.Count > 0)
                _predicted.RemoveAt(0);
        }
    }
}

class RubyReflection(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<(BitMask Cells, bool Dangerous)> _rooms = [];
    readonly Wall[] _walls = new Wall[16];
    BitMask _dangerRooms;

    DateTime _nextActivation;

    readonly List<(Actor Actor, int Room)> _topaz = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_nextActivation != default)
            foreach (var (c, _) in _rooms.Where(r => r.Dangerous))
                foreach (var bit in c.SetBits())
                    yield return new(new AOEShapeRect(5, 5, 5), TileCenter(bit), default, _nextActivation);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EC045 && state == 0x00100020)
            SetRooms([BitMask.Build(0, 1, 4, 5), BitMask.Build(2, 3, 6, 7), BitMask.Build(8, 9, 12, 13), BitMask.Build(10, 11, 14, 15)]);

        if (actor.OID == 0x1EC045 && state == 0x00040008)
            SetRooms([]);

        if (actor.OID == 0x1EC046 && state == 0x01000200)
        {
            if (actor.Rotation.AlmostEqual(default, 0.1f))
                SetRooms([BitMask.Build(0, 1, 5, 9), BitMask.Build(2, 3, 7, 11), BitMask.Build(4, 8, 12, 13), BitMask.Build(6, 10, 14, 15)]);
            if (actor.Rotation.AlmostEqual(90.Degrees(), 0.1f))
                SetRooms([BitMask.Build(0, 1, 2, 4), BitMask.Build(3, 5, 6, 7), BitMask.Build(8, 9, 10, 12), BitMask.Build(11, 13, 14, 15)]);
        }

        if (actor.OID == 0x1EC046 && state == 0x00100020)
        {
            if (actor.Rotation.AlmostEqual(default, 0.1f))
                SetRooms([BitMask.Build(0, 4, 5, 6), BitMask.Build(1, 2, 3, 7), BitMask.Build(8, 12, 13, 14), BitMask.Build(9, 10, 11, 15)]);
            if (actor.Rotation.AlmostEqual(90.Degrees(), 0.1f))
                SetRooms([BitMask.Build(0, 1, 4, 8), BitMask.Build(2, 3, 6, 10), BitMask.Build(5, 9, 12, 13), BitMask.Build(7, 11, 14, 15)]);
        }

        if (actor.OID == 0x1EC046 && state is 0x00040080 or 0x00040008)
            SetRooms([]);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == 0x4C50 && id == 0x2489)
        {
            var cell = ((actor.Position - Arena.Center + new WDir(20, 20)) / 10).Floor();
            _topaz.Add((actor, (int)cell.Z * 4 + (int)cell.X));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TopazRay2 or AID.TopazRay1)
            _topaz.RemoveAll(t => t.Actor == caster);

        if ((AID)spell.Action.ID is AID.RubyReflection1 or AID.RubyReflection2 or AID.RubyReflection3)
            _nextActivation = default;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        for (var bit = 0; bit < _walls.Length; bit++)
        {
            var center = TileCenter(bit);
            if (_walls[bit].HasFlag(Wall.N))
                Arena.AddLine(center + new WDir(-5, -5), center + new WDir(5, -5), 0xFF8F78C9, 2);
            if (_walls[bit].HasFlag(Wall.E))
                Arena.AddLine(center + new WDir(5, -5), center + new WDir(5, 5), 0xFF8F78C9, 2);
            if (_walls[bit].HasFlag(Wall.S))
                Arena.AddLine(center + new WDir(-5, 5), center + new WDir(5, 5), 0xFF8F78C9, 2);
            if (_walls[bit].HasFlag(Wall.W))
                Arena.AddLine(center + new WDir(-5, -5), center + new WDir(-5, 5), 0xFF8F78C9, 2);
        }
    }

    WPos TileCenter(int tile)
    {
        var col = tile % 4;
        var row = tile / 4;
        return Arena.Center - new WDir(15, 15) + new WDir(10 * col, 10 * row);
    }

    bool TouchingWall(Actor crystal, int room)
    {
        var center = TileCenter(room);
        var ws = _walls[room];
        var pos = crystal.Position;

        if (ws.HasFlag(Wall.N) && pos.InRect(center + new WDir(0, -1), new(0, -4), 5))
            return true;
        if (ws.HasFlag(Wall.E) && pos.InRect(center + new WDir(1, 0), new(4, 0), 5))
            return true;
        if (ws.HasFlag(Wall.S) && pos.InRect(center + new WDir(0, 1), new(0, 4), 5))
            return true;
        if (ws.HasFlag(Wall.W) && pos.InRect(center + new WDir(-1, 0), new(-4, 0), 5))
            return true;

        return false;
    }

    [Flags]
    enum Wall
    {
        None = 0,
        N = 1,
        E = 2,
        S = 4,
        W = 8
    }

    Wall CalcWalls(BitMask room, int cell)
    {
        var col = cell % 4;
        var row = cell / 4;

        var walls = Wall.None;
        if (row == 0 || !room[cell - 4])
            walls |= Wall.N;
        if (col == 3 || !room[cell + 1])
            walls |= Wall.E;
        if (row == 3 || !room[cell + 4])
            walls |= Wall.S;
        if (col == 0 || !room[cell - 1])
            walls |= Wall.W;

        return walls;
    }

    void SetRooms(IEnumerable<BitMask> rooms)
    {
        _dangerRooms.Reset();
        _rooms.Clear();
        Array.Fill(_walls, default);
        _rooms.AddRange(rooms.Select(r => (r, false)));
        foreach (var (room, _) in _rooms)
            foreach (var bit in room.SetBits())
                _walls[bit] = CalcWalls(room, bit);

        foreach (var (a, c) in _topaz)
            if (TouchingWall(a, c))
                _dangerRooms.Set(c);

        for (var i = 0; i < _rooms.Count; i++)
            if ((_rooms[i].Cells & _dangerRooms).Any())
                _rooms.Ref(i).Dangerous = true;

        if (_rooms.Count > 0)
            _nextActivation = WorldState.FutureTime(14.9f);
    }
}

class TopazRay(BossModule module) : Components.GroupedAOEs(module, [AID.TopazRay2, AID.TopazRay1], new AOEShapeCircle(4));

class SpinebreakingStampede(BossModule module) : Components.Knockback(module)
{
    readonly List<Source> _sources = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _sources;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.StampedeTelegraphRect)
        {
            _sources.Add(new(Arena.Center, 15, Module.CastFinishAt(spell, 5.1f), new AOEShapeRect(30, 30), spell.Rotation + 90.Degrees(), Kind.DirForward));
            _sources.Add(new(Arena.Center, 15, Module.CastFinishAt(spell, 5.1f), new AOEShapeRect(30, 30), spell.Rotation - 90.Degrees(), Kind.DirForward));
        }

        if ((AID)spell.Action.ID == AID.StampedeTelegraphCircle)
            _sources.Add(new(spell.LocXZ, 30, Module.CastFinishAt(spell, 6)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SpinebreakingStampedeRect)
            _sources.RemoveAll(s => s.Kind == Kind.DirForward);
        if ((AID)spell.Action.ID == AID.SpinebreakingStampedeCircle)
            _sources.RemoveAll(s => s.Kind == Kind.AwayFromOrigin);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // circle kb, preposition if arms length will fall off after horizontal kb
        if (_sources.Count == 1 || _sources.Count > 1 && IsImmune(slot, _sources[0].Activation) && !IsImmune(slot, _sources[^1].Activation))
        {
            var src = _sources[^1].Origin;
            var center = Arena.Center;
            hints.AddForbiddenZone(Sdf.Discrete(p =>
            {
                var dir = (p - src).Normalized();
                var proj = p + dir * 30;
                return !proj.AlmostEqual(center, 20);
            }), _sources[^1].Activation);
        }

        // horizontal kb
        if (_sources.Count > 1 && !IsImmune(slot, _sources[0].Activation))
        {
            var dirSafe = (_sources[^1].Origin - Arena.Center).ToAngle();
            hints.AddForbiddenZone(ShapeDistance.InvertedCone(Arena.Center, 4, dirSafe, 90.Degrees()), _sources[0].Activation);
        }

        if (_sources.Count > 0)
            hints.AddPredictedDamage(new(0xFF), _sources[0].Activation);
        if (_sources.Count > 1)
            hints.AddPredictedDamage(new(0xFF), _sources[^1].Activation);
    }
}

class AtlasCarbuncleStates : StateMachineBuilder
{
    public AtlasCarbuncleStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SonicHowl>()
            .ActivateOnEnter<RubyGlow>()
            .ActivateOnEnter<RubyReflection>()
            .ActivateOnEnter<TopazRay>()
            .ActivateOnEnter<ClawToTail>()
            .ActivateOnEnter<SpinebreakingStampede>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14791)]
public class AtlasCarbuncle(WorldState ws, Actor primary) : CEModule(ws, primary, new(238, 352), new ArenaBoundsSquare(20));
