namespace BossMod.Heavensward.Dungeon.D13SohrKhai.D132Poqhiraj;

public enum OID : uint
{
    Boss = 0x155C, // R2.5
    PrayerWall = 0x155E, // R5.0
    DarkCloud = 0x155D, // R1.0
    WallHelperNW1 = 0x1EA07A, //R2.0
    WallHelperNE1 = 0x1EA076, //R2.0
    WallHelperNW2 = 0x1EA07B, //R2.0
    WallHelperNE2 = 0x1EA077, //R2.0
    WallHelperSW1 = 0x1EA07C, //R2.0
    WallHelperSE1 = 0x1EA078, //R2.0
    WallHelperSW2 = 0x1EA07D, //R2.0
    WallHelperSE2 = 0x1EA079, //R2.0
    Helper = 0x1B2
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    RearHoof = 6013, // Boss->player, no cast, single-target

    BurningBright = 6011, // Boss->self/players, 3.0s cast, range 26+R width 6 rect
    TouchdownVisual = 6240, // Helper->self, 3.0s cast, range 40+R circle
    Touchdown = 6012, // Boss->self, no cast, range 40+R circle
    GallopVisual = 5777, // Boss->location, 4.5s cast, width 10 rect charge
    GallopAOE = 5778, // Helper->self, 4.9s cast, range 40+R width 2 rect, player gets thrown into air + vuln stack
    GallopKB = 5823, // Helper->self, 4.9s cast, range 4+R width 40 rect, knockback 10, forward
    CloudCall = 6009, // Boss->self, 4.0s cast, single-target
    LightningBolt = 6010 // DarkCloud->self, 3.0s cast, range 8 circle
}

public enum IconID : uint
{
    CloudCall = 24 // player
}

class ArenaChanges(BossModule module) : BossComponent(module)
{
    private readonly GallopKB _kb = module.FindComponent<GallopKB>()!;
    private static readonly float[] xPositions = [395.25f, 404.75f];
    private static readonly WPos[] wallPositions = WallPositionz();
    private readonly List<Rectangle> removedWalls = [];
    private static readonly Rectangle[] baseArena = [new(D132Poqhiraj.ArenaCenter, 4.5f, 19.75f)];
    private static readonly WDir offset = new(default, 0.125f);

    private static WPos[] WallPositionz()
    {
        const float zStart = 89.161f;
        const int zStep = 10;
        var index = 0;
        var walls = new WPos[8];
        for (var i = 0; i < 2; ++i)
            for (var j = 0; j < 4; ++j)
                walls[index++] = new(xPositions[i], zStart + j * zStep);
        return walls;
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state != 0x00100020u)
            return;

        var wallIndex = actor.OID switch
        {
            (uint)OID.WallHelperNW1 => 0,
            (uint)OID.WallHelperNE1 => 4,
            (uint)OID.WallHelperNW2 => 1,
            (uint)OID.WallHelperNE2 => 5,
            (uint)OID.WallHelperSW1 => 2,
            (uint)OID.WallHelperSE1 => 6,
            (uint)OID.WallHelperSW2 => 3,
            (uint)OID.WallHelperSE2 => 7,
            _ => default
        };

        var wallPos = wallPositions[wallIndex];
        var adjustment = wallIndex is 0 or 4 ? offset : wallIndex is 3 or 7 ? -offset : default;
        removedWalls.Add(new(wallPos + adjustment, 0.25f, adjustment != default ? 4.875f : 5f));
        var safewalls = CollectionsMarshal.AsSpan(_kb.safeWalls);
        var len = safewalls.Length;
        var pos = new WPos(GallopKB.xPositions[wallIndex / 4], wallPos.Z - 5f);
        for (var i = 0; i < len; ++i)
        {
            ref readonly var safewall = ref safewalls[i];
            if (safewall.Vertex1 == pos)
            {
                _kb.safeWalls.RemoveAt(i);
                break;
            }
        }

        ArenaBoundsCustom arena = new([.. baseArena, .. removedWalls]);
        Arena.Bounds = arena;
        Arena.Center = arena.Center;
    }
}

class GallopAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GallopAOE, new AOEShapeRect(40.5f, 1f));

class GallopKB(BossModule module) : Components.GenericKnockback(module)
{
    public static readonly float[] xPositions = [395.5f, 404.5f];
    private static readonly AOEShapeRect rect = new(4.5f, 20f);
    private readonly List<Knockback> _sources = [with(2)];
    public readonly List<SafeWall> safeWalls = GenerateSafeWalls();

    private static List<SafeWall> GenerateSafeWalls()
    {
        const float zStart = 89.161f;
        const int zStep = 10;

        List<SafeWall> list = [with(8)];

        for (var i = 0; i < 2; ++i)
        {
            for (var j = 0; j < 4; ++j)
            {
                list.Add(new(new(xPositions[i], zStart + j * zStep - 5f), new(xPositions[i], zStart + j * zStep + 5f)));
            }
        }
        return list;
    }

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(_sources);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GallopKB)
        {
            _sources.Add(new(spell.LocXZ, 30f, Module.CastFinishAt(spell), rect, spell.Rotation, Kind.DirForward, default, safeWalls));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GallopKB)
        {
            _sources.Clear();
        }
    }
}

class GallopKBHint(BossModule module) : Components.GenericAOEs(module)
{
    private readonly GallopKB _kb = module.FindComponent<GallopKB>()!;

    private static readonly Angle[] angles = [-89.982f.Degrees(), 89.977f.Degrees()];
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_aoe.Length == 0 && spell.Action.ID == (uint)AID.GallopKB)
        {
            var count = _kb.safeWalls.Count;
            if (count is 0 or 8)
            {
                return;
            }
            List<RectangleSE> rects = [with(count)];
            for (var i = 0; i < count; ++i)
            {
                var safeWall = _kb.safeWalls[i].Vertex1;
                var dir = (safeWall.X == GallopKB.xPositions[0] ? angles[0] : angles[1]).ToDirection();
                var pos = new WPos(safeWall.X, safeWall.Z + 5f);
                rects.Add(new(pos + dir, pos - 3.5f * dir, 5f));
            }
            AOEShapeCustom aoe = new([.. rects], invertForbiddenZone: true);
            _aoe = [new(aoe, Arena.Center, default, Module.CastFinishAt(spell), Colors.SafeFromAOE, true)];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.GallopKB)
        {
            _aoe = [];
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.Length != 0)
        {
            ref var aoe = ref _aoe[0];
            hints.Add("Walk into safespot for knockback!", !aoe.Check(actor.Position));
        }
    }
}

class Touchdown(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TouchdownVisual, 25f);

class BurningBright(BossModule module) : Components.BaitAwayCast(module, (uint)AID.BurningBright, new AOEShapeRect(28.5f, 3f), endsOnCastEvent: true)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (CurrentBaits.Count != 0 && CurrentBaits.Ref(0).Target == pc)
        {
            var walls = Module.Enemies((uint)OID.PrayerWall);
            var count = walls.Count;
            for (var i = 0; i < count; ++i)
            {
                var a = walls[i];
                Arena.ZoneCircleOutline(a.Position, a.HitboxRadius, Colors.Danger);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count == 0)
        {
            return;
        }
        if (CurrentBaits.Ref(0).Target != actor)
        {
            base.AddHints(slot, actor, hints);
        }
        else
        {
            hints.Add("Bait away, avoid intersecting wall hitboxes!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (CurrentBaits.Count != 0)
        {
            ref var bait = ref CurrentBaits.Ref(0);
            if (bait.Target == actor)
            {
                var walls = Module.Enemies((uint)OID.PrayerWall);
                var count = walls.Count;
                if (count is <= 4 and > 0) // don't care if most walls are up plus most of the arena would likely be forbidden anyway depending on player positioning
                {
                    var forbidden = new ShapeDistance[count];
                    for (var i = 0; i < count; ++i)
                    {
                        var a = walls[i];
                        forbidden[i] = new SDCone(bait.Source.Position, 100f, bait.Source.AngleTo(a), Angle.Asin(8f / (a.Position - bait.Source.Position).Length()));
                    }
                    hints.AddForbiddenZone(new SDUnion(forbidden), bait.Activation);
                }
            }
        }
    }
}

class RearHoof(BossModule module) : Components.SingleTargetInstant(module, (uint)AID.RearHoof)
{
    private bool start = true, firstKB;

    public override void Update()
    {
        if (start)
        {
            AddTankbuster(6.1d); // its assumed that the tank will aggro the boss first
            start = false;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (!firstKB && spell.Action.ID == (uint)AID.GallopKB)
        {
            AddTankbuster(7d);
            firstKB = true;
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.DarkCloud)
        {
            AddTankbuster(4d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.RearHoof)
        {
            Targets.Clear();
        }
    }

    private void AddTankbuster(double delay)
    {
        var id = Module.PrimaryActor.TargetID;
        if (WorldState.Actors.Find(id) is Actor t)
        {
            Targets.Add((Raid.FindSlot(id), WorldState.FutureTime(delay), id, Module.PrimaryActor, t));
        }
    }
}

class CloudCall(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    public static readonly AOEShapeCircle Circle = new(8f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.CloudCall)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, Circle, WorldState.FutureTime(4.9d)));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.DarkCloud)
        {
            CurrentBaits.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (CurrentBaits.Count != 0 && CurrentBaits.Ref(0).Target == actor)
        {
            hints.AddForbiddenZone(new SDRect(Arena.Center, new WDir(default, 1f), 19f, 19f, 5f));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count == 0)
        {
            return;
        }
        if (CurrentBaits.Ref(0).Target != actor)
        {
            base.AddHints(slot, actor, hints);
        }
        else
        {
            hints.Add("Bait cloud away, avoid intersecting wall hitboxes!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (CurrentBaits.Count != 0 && CurrentBaits.Ref(0).Target == pc)
        {
            var walls = Module.Enemies((uint)OID.PrayerWall);
            var count = walls.Count;
            for (var i = 0; i < count; ++i)
            {
                var a = walls[i];
                Arena.ZoneCircleOutline(a.Position, a.HitboxRadius);
            }
        }
    }
}

class LightningBolt(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.DarkCloud)
        {
            _aoe = [new(CloudCall.Circle, actor.Position, default, WorldState.FutureTime(7.8d))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.LightningBolt)
        {
            _aoe = [];
        }
    }
}

class D132PoqhirajStates : StateMachineBuilder
{
    public D132PoqhirajStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GallopKB>()
            .ActivateOnEnter<GallopKBHint>()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<GallopAOE>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<BurningBright>()
            .ActivateOnEnter<CloudCall>()
            .ActivateOnEnter<LightningBolt>()
            .ActivateOnEnter<RearHoof>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 171, NameID = 4952, SortOrder = 4)]
public class D132Poqhiraj(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsRect(4.5f, 19.75f))
{
    public static readonly WPos ArenaCenter = new(400f, 104.166f);
}
