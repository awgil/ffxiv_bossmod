namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D042Protector;

public enum OID : uint
{
    Boss = 0x4237, // R5.83
    LaserTurret = 0x4238, // R0.96
    FulminousFence = 0x4255, // R1.0
    ExplosiveTurret = 0x4239, // R0.96
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 878, // Boss->player, no cast, single-target

    Electrowave = 37161, // Boss->self, 5.0s cast, range 50 circle, raidwide

    SearchAndDestroy = 37154, // Boss->self, 3.0s cast, single-target
    BlastCannon = 37151, // LaserTurret->self, 3.0s cast, range 26 width 4 rect
    BlastCannonVisual = 37153, // Boss->self, no cast, single-target
    Shock = 37156, // ExplosiveTurret->location, 2.5s cast, range 3 circle
    HomingCannon = 37155, // LaserTurret->self, 2.5s cast, range 50 width 2 rect

    FulminousFence = 37149, // Boss->self, 3.0s cast, single-target, fences appear
    ElectrostaticContact = 37158, // FulminousFence->player, no cast, single-target

    BatteryCircuitVisual = 37159, // Boss->self, 5.0s cast, single-target
    BatteryCircuitFirst = 37351, // Helper->self, 5.0s cast, range 30 30-degree cone
    BatteryCircuitRest = 37344, // Helper->self, no cast, range 30 30-degree cone

    RapidThunder = 37162, // Boss->player, 5.0s cast, single-target
    MotionSensor = 37150, // Boss->self, 3.0s cast, single-target

    Bombardment = 39016, // Helper->location, 3.0s cast, range 5 circle

    Electrowhirl1 = 37160, // Helper->self, 3.0s cast, range 6 circle
    Electrowhirl2 = 37350, // Helper->self, 5.0s cast, range 6 circle

    TrackingBolt1 = 37348, // Boss->self, 8.0s cast, single-target
    TrackingBolt2 = 37349, // Helper->player, 8.0s cast, range 8 circle // Spread marker

    ApplyAccelerationBomb = 37343, // Helper->player, no cast, single-target

    HeavyBlastCannonMarker = 37347, // Helper->player, no cast, single-target
    HeavyBlastCannon = 37345, // Boss->self/players, 8.0s cast, range 36 width 8 rect, line stack
}

public enum SID : uint
{
    LaserTurretsVisual = 2056, // Boss->Boss, extra=0x2CE
    AccelerationBomb = 3802, // Helper->player, extra=0x0
}

class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private const float Radius = 0.5f;
    public static readonly WPos ArenaCenter = new(0, -100);
    public static readonly ArenaBounds StartingBounds = new ArenaBoundsRect(14.5f, 22.5f);
    private static readonly ArenaBounds defaultBounds = new ArenaBoundsRect(12, 20);
    private static readonly Rectangle startingRect = new(ArenaCenter, 15, 23);
    private static readonly Rectangle defaultRect = new(ArenaCenter, 12, 20);
    private static readonly WPos[] CirclePositions =
    [
        new(12, -88), new(8, -92), new(4, -88), new(0, -88), new(-4, -88),
        new(-12, -88), new(-8, -92), new(0, -92), new(-4, -96), new(0, -96),
        new(4, -96), new(-4, -104), new(0, -104), new(4, -104), new(-8, -108),
        new(-12, -112), new(-4, -112), new(0, -108), new(0, -112), new(4, -112),
        new(8, -108), new(12, -112), new(12, -104), new(12, -96), new(-12, -96),
        new(-12, -104)
    ];

    private static readonly Circle[] Circles = CirclePositions.Select(pos => new Circle(pos, Radius)).ToArray();

    private static readonly (int, int)[] RectanglePairs =
    [
        (0, 1), (7, 9), (5, 6), (13, 20), (17, 18), (11, 14), (21, 20), (14, 15),
        (12, 17), (1, 10), (3, 7), (6, 8), (25, 5), (25, 11), (2, 5), (4, 8),
        (16, 21), (21, 23), (23, 10), (13, 22), (15, 24), (15, 19), (16, 11),
        (24, 8), (0, 22), (0, 4), (2, 10), (22, 13)
    ];

    private static readonly RectangleSE[] Rectangles = RectanglePairs
        .Select(pair => new RectangleSE(Circles[pair.Item1].Center, Circles[pair.Item2].Center, Radius)).ToArray();

    private static readonly AOEShapeCustom rectArenaChange = new([startingRect], [defaultRect]);

    private static readonly List<Shape> union01000080Shapes = GetShapesForUnion([0, 1, 2, 3, 4, 5], [0, 1, 7, 9, 5, 6, 13, 20, 17, 18, 11, 14]);
    private static readonly AOEShapeCustom electricFences01000080AOE = new(union01000080Shapes);
    private static readonly ArenaBoundsComplex electricFences01000080Arena = new([defaultRect], union01000080Shapes, Offset: -Radius);

    private static readonly List<Shape> union08000400Shapes = GetShapesForUnion([6, 7, 8, 9, 10, 11], [21, 20, 14, 15, 12, 17, 1, 10, 3, 7, 6, 8]);
    private static readonly AOEShapeCustom electricFences08000400AOE = new(union08000400Shapes);
    private static readonly ArenaBoundsComplex electricFences08000400Arena = new([defaultRect], union08000400Shapes, Offset: -Radius);

    private static readonly List<Shape> union00020001Shapes = GetShapesForUnion([12, 13, 14, 15, 16, 17, 18, 19], [2, 8, 11, 10, 13, 16]);
    private static readonly AOEShapeCustom electricFences00020001AOE = new(union00020001Shapes);
    private static readonly ArenaBoundsComplex electricFences00020001Arena = new([defaultRect], union00020001Shapes, Offset: -Radius);

    private static readonly List<Shape> union00200010Shapes = GetShapesForUnion([20, 21, 22, 23, 24, 25, 19, 26], [4, 8, 11, 19, 13, 10]);
    private static readonly AOEShapeCustom electricFences00200010AOE = new(union00200010Shapes);
    private static readonly ArenaBoundsComplex electricFences00200010Arena = new([defaultRect], union00200010Shapes, Offset: -Radius);

    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    private static List<Shape> GetShapesForUnion(int[] rectIndices, int[] circleIndices)
    {
        var shapes = new List<Shape>();
        shapes.AddRange(rectIndices.Select(index => Rectangles[index]));
        shapes.AddRange(circleIndices.Select(index => Circles[index]));
        return shapes;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Electrowave && Module.Arena.Bounds == StartingBounds)
            _aoe = new(rectArenaChange, Module.Center, default, spell.NPCFinishAt.AddSeconds(0.4f));
    }

    public override void Update()
    {
        if (Module.Arena.Bounds == defaultBounds)
        {
            var player = Module.Raid.Player()!;
            var aoeChecks = new[]
            {
                new { AOE = electricFences01000080AOE, Bounds = electricFences01000080Arena },
                new { AOE = electricFences08000400AOE, Bounds = electricFences08000400Arena },
                new { AOE = electricFences00020001AOE, Bounds = electricFences00020001Arena },
                new { AOE = electricFences00200010AOE, Bounds = electricFences00200010Arena }
            };

            foreach (var check in aoeChecks)
            {
                if (ActiveAOEs(0, player).Any(c => c.Shape == check.AOE && c.Activation <= Module.WorldState.CurrentTime))
                {
                    Module.Arena.Bounds = check.Bounds;
                    _aoe = null;
                    break;
                }
            }
        }
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        var activation = Module.WorldState.FutureTime(3);
        if (state == 0x00020001 && index == 0x0C)
        {
            Module.Arena.Bounds = defaultBounds;
            _aoe = null;
        }
        else if (index == 0x0D)
        {
            switch (state)
            {
                case 0x08000400:
                    _aoe = new(electricFences08000400AOE, Module.Center, default, activation);
                    break;
                case 0x01000080:
                    _aoe = new(electricFences01000080AOE, Module.Center, default, activation);
                    break;
                case 0x00020001:
                    _aoe = new(electricFences00020001AOE, Module.Center, default, activation);
                    break;
                case 0x00200010:
                    _aoe = new(electricFences00200010AOE, Module.Center, default, activation);
                    break;
                case 0x02000004 or 0x10000004 or 0x00080004 or 0x00400004:
                    Module.Arena.Bounds = defaultBounds;
                    break;
            }
        }
    }
}

class BatteryCircuit(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly Angle _increment = -11.Degrees();
    private bool started;
    private int counter;

    private static readonly AOEShapeCone _shape = new(30, 15.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BatteryCircuitFirst && !started)
        {
            Sequences.Add(new(_shape, caster.Position, spell.Rotation, _increment, spell.NPCFinishAt, 0.5f, 34, 9));
            Sequences.Add(new(_shape, caster.Position, spell.Rotation + 180.Degrees(), _increment, spell.NPCFinishAt, 0.5f, 34, 9));
            started = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BatteryCircuitFirst or AID.BatteryCircuitRest)
            if (++counter % 2 == 0)
            {
                AdvanceSequence(1, WorldState.CurrentTime);
                AdvanceSequence(0, WorldState.CurrentTime);
            }
    }
}

class HeavyBlastCannon(BossModule module) : Components.LineStack(module, ActionID.MakeSpell(AID.HeavyBlastCannonMarker), ActionID.MakeSpell(AID.HeavyBlastCannon), 8, 36);
class RapidThunder(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.RapidThunder));
class Electrowave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Electrowave));
class BlastCannon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlastCannon), new AOEShapeRect(26, 2));
class Shock(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Shock), 3);
class HomingCannon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HomingCannon), new AOEShapeRect(50, 1));
class Bombardment(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Bombardment), 5);
class Electrowhirl1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Electrowhirl1), new AOEShapeCircle(6));
class Electrowhirl2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Electrowhirl2), new AOEShapeCircle(6));
class TrackingBolt2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.TrackingBolt2), 8);

class AccelerationBomb(BossModule module) : Components.StayMove(module)
{
    private bool pausedAI;
    private DateTime expiresAt;
    private bool expired;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.AccelerationBomb)
        {
            if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < Requirements.Length)
                Requirements[slot] = Requirement.Stay;
            if (actor == Module.Raid.Player()!)
                expiresAt = status.ExpireAt;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.AccelerationBomb)
        {
            if (Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < Requirements.Length)
            {
                Requirements[slot] = Requirement.None;
                expired = true;
                expiresAt = default;
            }
        }
    }

    public override void Update()
    {
        if (expiresAt != default && AI.AIManager.Instance?.Beh != null && expiresAt.AddSeconds(-0.1f) <= Module.WorldState.CurrentTime)
        {
            AI.AIManager.Instance?.SwitchToIdle();
            pausedAI = true;
        }
        else if (expired && pausedAI)
        {
            AI.AIManager.Instance?.SwitchToFollow(Service.Config.Get<AI.AIConfig>().FollowSlot);
            pausedAI = false;
            expired = false;
        }
    }
}

class D042ProtectorStates : StateMachineBuilder
{
    public D042ProtectorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<HeavyBlastCannon>()
            .ActivateOnEnter<AccelerationBomb>()
            .ActivateOnEnter<RapidThunder>()
            .ActivateOnEnter<Electrowave>()
            .ActivateOnEnter<BlastCannon>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<HomingCannon>()
            .ActivateOnEnter<BatteryCircuit>()
            .ActivateOnEnter<Bombardment>()
            .ActivateOnEnter<Electrowhirl1>()
            .ActivateOnEnter<Electrowhirl2>()
            .ActivateOnEnter<TrackingBolt2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12757, SortOrder = 4)]
public class D042Protector(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.ArenaCenter, ArenaChanges.StartingBounds);
