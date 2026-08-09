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

    TrackingBoltVisual = 37348, // Boss->self, 8.0s cast, single-target
    TrackingBolt = 37349, // Helper->player, 8.0s cast, range 8 circle, spread

    ApplyAccelerationBomb = 37343, // Helper->player, no cast, single-target

    HeavyBlastCannonMarker = 37347, // Helper->player, no cast, single-target
    HeavyBlastCannon = 37345 // Boss->self/players, 8.0s cast, range 36 width 8 rect, line stack
}

public enum SID : uint
{
    LaserTurretsVisual = 2056, // Boss->Boss, extra=0x2CE
    AccelerationBomb = 3802, // Helper->player, extra=0x0
    AccelerationBombNPCs = 4144 // Helper->NPCs, extra=0x0
}

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private const float Radius = 0.51f; // small cushion since fences don't seem to be positioned perfectly
    private DateTime activation;
    private ArenaBoundsCustom? preparedArena;

    private readonly WPos[] circlePositions =
    [
        new(12f, -88f), new(8f, -92f), new(4f, -88f), new(0f, -88f), new(-4f, -88f),
        new(-12f, -88f), new(-8f, -92f), new(0f, -92f), new(-4f, -96f), new(0f, -96f),
        new(4f, -96f), new(-4f, -104f), new(0f, -104f), new(4f, -104f), new(-8f, -108f),
        new(-12f, -112f), new(-4f, -112f), new(0f, -108f), new(0f, -112f), new(4f, -112f),
        new(8f, -108f), new(12f, -112f), new(12f, -104f), new(12f, -96f), new(-12f, -96f),
        new(-12f, -104f)
    ];

    private readonly (int Start, int End)[] rectanglePairs =
    [
        (0, 1), (7, 9), (5, 6), (13, 20), (17, 18), (11, 14), (21, 20), (14, 15),
        (12, 17), (1, 10), (3, 7), (6, 8), (25, 5), (25, 11), (2, 5), (4, 8),
        (16, 21), (21, 23), (23, 10), (13, 19), (15, 24), (15, 19), (16, 11),
        (24, 8), (0, 22), (0, 4), (2, 10), (22, 13),
    ];

    private readonly Polygon[] circles = new Polygon[26];
    private readonly RectangleSE[] rectangles = new RectangleSE[28];

    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Electrowave && Arena.Bounds.Radius > 21f)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [new Rectangle(center, 14.5f, 22.5f)], [new Rectangle(center, 12f, 20f)]);
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 0.4d), shapeDistance: shape.Distance(center, default))];

            // initialize shapes for later
            CreateCircles(circlePositions, Radius, 12);
            CreateRectangles(rectanglePairs, circlePositions, Radius);
            void CreateCircles(WPos[] positions, float radius, int edges)
            {
                for (var i = 0; i < 26; ++i)
                {
                    circles[i] = new Polygon(positions[i], radius, edges);
                }
            }

            void CreateRectangles((int, int)[] pairs, WPos[] positions, float width)
            {
                for (var i = 0; i < 28; ++i)
                {
                    var pair = pairs[i];
                    rectangles[i] = new RectangleSE(positions[pair.Item1], positions[pair.Item2], width);
                }
            }
        }
    }

    public override void Update()
    {
        if (preparedArena != null && activation <= WorldState.CurrentTime)
        {
            Arena.Bounds = preparedArena;
            _aoe = [];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0C && state == 0x00020001u)
        {
            ResetArena();
            _aoe = [];
            return;
        }

        if (index != 0x0D)
        {
            return;
        }

        switch (state)
        {
            case 0x08000400u:
                AddAOEAndPrepareArenaChange([6, 7, 8, 9, 10, 11], [21, 20, 14, 15, 12, 17, 1, 10, 3, 7, 6, 8]);
                break;

            case 0x01000080u:
                AddAOEAndPrepareArenaChange([0, 1, 2, 3, 4, 5], [0, 1, 7, 9, 5, 6, 13, 20, 17, 18, 11, 14]);
                break;

            case 0x00020001u:
                AddAOEAndPrepareArenaChange([12, 13, 14, 15, 16, 17, 18, 19], [2, 8, 11, 10, 13, 16]);
                break;

            case 0x00200010u:
                AddAOEAndPrepareArenaChange([20, 21, 22, 23, 24, 25, 26, 27], [4, 8, 11, 19, 13, 10]);
                break;

            case 0x02000004u:
            case 0x10000004u:
            case 0x00080004u:
            case 0x00400004u:
                ResetArena();
                break;
        }

        void ResetArena() => Arena.Bounds = new ArenaBoundsRect(12f, 20f);

        void AddAOEAndPrepareArenaChange(ReadOnlySpan<int> rectangleIndices, ReadOnlySpan<int> circleIndices)
        {
            var center = Arena.Center;
            Rectangle[] defaultBounds = [new Rectangle(center, 12f, 20f)];
            var removedShapes = CreateShapes(rectangleIndices, circleIndices);

            var shape = new AOEShapeCustom(center, defaultBounds, removedShapes);
            activation = WorldState.FutureTime(3d);

            _aoe = [new(shape, center, default, activation, shapeDistance: shape.Distance(center, default))];

            preparedArena = new ArenaBoundsCustom(defaultBounds, removedShapes);

            Shape[] CreateShapes(ReadOnlySpan<int> rectangleIndices, ReadOnlySpan<int> circleIndices)
            {
                var lenR = rectangleIndices.Length;
                var lenC = circleIndices.Length;
                var shapes = new Shape[lenR + lenC];
                var destination = shapes.AsSpan();

                for (var i = 0; i < lenR; ++i)
                {
                    destination[i] = CreateRectangle(rectangleIndices[i]);
                }

                for (var i = 0; i < lenC; ++i)
                {
                    destination[lenR + i] = CreateCircle(circleIndices[i]);
                }

                return shapes;
            }
            RectangleSE CreateRectangle(int index)
            {
                var (start, end) = rectanglePairs[index];
                return new RectangleSE(circlePositions[start], circlePositions[end], Radius);
            }
            Polygon CreateCircle(int index) => new(circlePositions[index], Radius, 12);
        }
    }
}

sealed class BatteryCircuit(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeCone _shape = new(30f, 15f.Degrees());

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BatteryCircuitFirst)
        {
            Sequences.Add(new(_shape, spell.LocXZ, spell.Rotation, -11f.Degrees(), Module.CastFinishAt(spell), 0.5d, 34, 9));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BatteryCircuitFirst or (uint)AID.BatteryCircuitRest)
        {
            AdvanceSequence(caster.Position, caster.Rotation, WorldState.CurrentTime);
        }
    }
}

sealed class HeavyBlastCannon(BossModule module) : Components.LineStack(module, aidMarker: (uint)AID.HeavyBlastCannonMarker, (uint)AID.HeavyBlastCannon, 8d, 36f);
sealed class RapidThunder(BossModule module) : Components.SingleTargetCast(module, (uint)AID.RapidThunder);
sealed class Electrowave(BossModule module) : Components.RaidwideCast(module, (uint)AID.Electrowave);

sealed class BlastCannon : Components.SimpleAOEs
{
    public BlastCannon(BossModule module) : base(module, (uint)AID.BlastCannon, new AOEShapeRect(26f, 2f), 4)
    {
        MaxDangerColor = 2;
        MaxRisky = 2;
    }
}
sealed class Shock(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Shock, 3f);

sealed class HomingCannon : Components.SimpleAOEs
{
    public HomingCannon(BossModule module) : base(module, (uint)AID.HomingCannon, new AOEShapeRect(50f, 1f))
    {
        MaxDangerColor = 4;
    }
}

sealed class Bombardment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Bombardment, 5f);
sealed class Electrowhirl(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.Electrowhirl1, (uint)AID.Electrowhirl2], 6f);

sealed class TrackingBolt(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.TrackingBolt, 8f);

sealed class AccelerationBomb(BossModule module) : Components.StayMove(module, 3f)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.AccelerationBomb or (uint)SID.AccelerationBombNPCs && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = new(Requirement.Stay, status.ExpireAt);
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.AccelerationBomb or (uint)SID.AccelerationBombNPCs && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = default;
        }
    }
}

sealed class D042ProtectorStates : StateMachineBuilder
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
            .ActivateOnEnter<Electrowhirl>()
            .ActivateOnEnter<TrackingBolt>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831u, NameID = 12757u, SortOrder = 5)]
public sealed class D042Protector(WorldState ws, Actor primary) : BossModule(ws, primary, new(0f, -100f), new ArenaBoundsRect(14.5f, 22.5f));
