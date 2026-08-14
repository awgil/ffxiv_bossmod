namespace BossMod.RealmReborn.Dungeon.D01Sastasha.D013DennTheOrcatoothed;

public enum OID : uint
{
    // Boss
    Boss = 0x1A1, // Denn The Orcatoothed

    // Trash
    BaleenGuard = 0x1A2, // Spawn during fight

    // EventObj
    // EventState 00080008 if spawned, 00000000 if inactive, 00040004 & 00040000 if going to be active
    UnnaturalRipples1 = 0x1E8615,
    UnnaturalRipples2 = 0x1E8616,
    UnnaturalRipples3 = 0x1E8617,
    UnnaturalRipples4 = 0x1E8618
}

public enum AID : uint
{
    // Boss
    AutoAttackBoss = 871, // Boss->player, no cast
    TrueThrust = 722, // Boss->player, no cast
    JumpingThrust = 834, // Boss->player, no cast
    Hydroball = 556, // Boss->self, 3.5s cast, range 8 90-degree cone aoe

    // BaleenGuard
    AutoAttackGuard = 870, // Guard->player, no cast
    Ambuscade = 835, // Guard->self, no cast, range 8 circle aoe
    WaterCannon = 555 // Gaurd->player, no cast
}

class Hydroball(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Hydroball, new AOEShapeCone(8f, 45f.Degrees()));

class TrashSpawning(BossModule module) : BossComponent(module)
{
    private readonly Actor?[] _ripples = new Actor?[4];  // Stores ripple actors
    private readonly uint[] _rippleStates = new uint[4]; // Stores states for each ripple

    private static readonly WDir _rippleOffset = new(0f, -3f);
    private static readonly Angle _rippleDirectionOffset = new(0); // No rotation, just offset
    private static readonly AOEShapeRect _rippleShape = new(6, 2, 0, _rippleDirectionOffset);

    // Using bitwise check to determine if the ripple state matches 0x00040000 (to indicate active spawn)
    private IEnumerable<Actor?> ActiveRipples => _ripples.Where((ripple, index) => ripple != null && (_rippleStates[index] & 0x00040000) == 0x00040000);

    public override void Update()
    {
        // Loop through all possible ripple OIDs to populate the _ripples array
        for (var i = 0; i < _ripples.Length; ++i)
        {
            var oid = (uint)((uint)OID.UnnaturalRipples1 + i);
            _ripples[i] ??= Module.Enemies(oid).FirstOrDefault();
        }
    }

    // Updating ripple state when an animation state change happens
    public override void OnActorEAnim(Actor actor, uint state)
    {
        for (var i = 0; i < _ripples.Length; ++i)
        {
            if (_ripples[i]?.OID == actor.OID)
            {
                _rippleStates[i] = state;
                break;
            }
        }
    }

    // Add hint if active ripples exist
    public override void AddGlobalHints(GlobalHints hints)
    {
        if (ActiveRipples.Any())
        {
            hints.Add("Adds spawning soon!");
        }
    }

    // Drawing the ripple shapes where adds will spawn
    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var ripple in ActiveRipples)
        {
            if (ripple != null)
            {
                // Apply the offset
                var offsetPosition = ripple.Position + _rippleOffset;

                // Convert the offset position back to WPos
                WPos finalPosition = new(offsetPosition.X, offsetPosition.Z);

                // Draw the ripple shape at the adjusted position
                _rippleShape.Outline(Arena, finalPosition, _rippleDirectionOffset, Colors.PlayerInteresting);
            }
        }
    }
}

class D013DennTheOrcatoothedStates : StateMachineBuilder
{
    public D013DennTheOrcatoothedStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<TrashSpawning>()
            .ActivateOnEnter<Hydroball>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Chuggalo", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 4, NameID = 1206)]
public class D013DennTheOrcatoothed(WorldState ws, Actor primary) : BossModule(ws, primary, arena.Center, arena)
{
    private static readonly WPos[] vertices = [new(-317.25f, 279.22f), new(-306.29f, 279.22f), new(-305.66f, 279.32f), new(-294.90f, 279.36f), new(-293.81f, 279.22f),
    new(-280.86f, 279.22f), new(-281.20f, 279.83f), new(-283.61f, 281.55f), new(-284.76f, 283.14f), new(-284.96f, 283.66f),
    new(-285.28f, 291.13f), new(-284.56f, 294.38f), new(-283.23f, 295.76f), new(-282.95f, 296.41f), new(-282.71f, 298.46f),
    new(-282.37f, 299.02f), new(-280.73f, 301.05f), new(-280.62f, 301.55f), new(-281.33f, 307.69f), new(-282.45f, 309.38f),
    new(-282.62f, 309.98f), new(-282.75f, 311.37f), new(-283.04f, 312.01f), new(-284.00f, 313.03f), new(-284.31f, 313.54f),
    new(-284.87f, 315.39f), new(-285.18f, 321.93f), new(-284.63f, 323.69f), new(-284.56f, 324.41f), new(-284.71f, 325.79f),
    new(-284.60f, 326.41f), new(-283.68f, 328.90f), new(-283.38f, 329.43f), new(-281.52f, 332.05f), new(-281.17f, 332.64f),
    new(-280.39f, 335.85f), new(-280.75f, 339.08f), new(-281.01f, 339.75f), new(-282.34f, 341.22f), new(-282.62f, 341.71f),
    new(-282.90f, 343.62f), new(-284.88f, 347.16f), new(-286.35f, 347.31f), new(-288.07f, 346.90f), new(-288.58f, 347.40f),
    new(-288.68f, 348.54f), new(-288.68f, 354.15f), new(-288.18f, 354.54f), new(-285.52f, 354.55f), new(-285.26f, 355.01f),
    new(-284.98f, 357.52f), new(-285.59f, 357.57f), new(-286.96f, 357.49f), new(-287.62f, 357.72f), new(-288.12f, 357.62f),
    new(-288.76f, 357.40f), new(-289.28f, 357.45f), new(-289.68f, 357.87f), new(-289.79f, 359.44f), new(-290.26f, 359.68f),
    new(-290.53f, 360.13f), new(-290.67f, 360.77f), new(-290.00f, 361.02f), new(-289.77f, 361.48f), new(-289.77f, 364.34f),
    new(-289.61f, 364.88f), new(-289.03f, 364.97f), new(-288.84f, 365.45f), new(-288.78f, 366.76f), new(-289.23f, 368.01f),
    new(-289.64f, 368.58f), new(-290.91f, 369.99f), new(-291.93f, 370.83f), new(-292.58f, 371.09f), new(-292.81f, 370.42f),
    new(-292.78f, 368.12f), new(-292.85f, 367.55f), new(-294.03f, 367.36f), new(-300.87f, 367.36f), new(-301.16f, 366.78f),
    new(-301.73f, 366.48f), new(-302.33f, 366.57f), new(-302.49f, 367.36f), new(-305.49f, 367.30f), new(-306.17f, 367.36f),
    new(-306.54f, 367.85f), new(-306.55f, 370.52f), new(-307.11f, 370.92f), new(-309.54f, 371.72f), new(-310.14f, 372.03f),
    new(-311.67f, 373.18f), new(-312.31f, 373.49f), new(-314.29f, 373.57f), new(-315.80f, 374.80f), new(-316.36f, 375.19f),
    new(-318.27f, 375.45f), new(-322.80f, 374.94f), new(-323.42f, 374.79f), new(-325.63f, 373.29f), new(-327.76f, 372.93f),
    new(-331.39f, 371.01f), new(-332.01f, 370.87f), new(-336.11f, 370.72f), new(-336.75f, 370.78f), new(-339.95f, 371.56f),
    new(-345.81f, 365.91f), new(-345.30f, 364.86f), new(-345.14f, 364.19f), new(-345.21f, 360.82f), new(-345.11f, 360.18f),
    new(-345.37f, 359.60f), new(-345.95f, 359.49f), new(-347.75f, 359.50f), new(-348.30f, 359.61f), new(-348.51f, 360.18f),
    new(-349.01f, 360.30f), new(-349.60f, 360.50f), new(-350.30f, 360.64f), new(-350.80f, 360.48f), new(-351.42f, 360.43f),
    new(-351.80f, 360.79f), new(-352.96f, 359.15f), new(-353.14f, 358.68f), new(-353.14f, 349.81f), new(-352.81f, 349.21f),
    new(-352.19f, 348.88f), new(-351.56f, 348.83f), new(-351.29f, 348.39f), new(-351.23f, 346.44f), new(-351.49f, 345.86f),
    new(-351.46f, 344.63f), new(-351.53f, 344.09f), new(-351.76f, 343.49f), new(-352.15f, 343.09f), new(-352.78f, 343.02f),
    new(-353.13f, 333.50f), new(-353.15f, 310.38f), new(-353.30f, 309.80f), new(-357.19f, 309.72f), new(-357.40f, 308.78f),
    new(-357.38f, 306.74f), new(-356.87f, 306.56f), new(-356.33f, 306.27f), new(-356.25f, 305.72f), new(-356.25f, 305.03f),
    new(-356.69f, 304.65f), new(-357.40f, 304.54f), new(-357.39f, 296.50f), new(-356.88f, 296.29f), new(-355.86f, 296.29f),
    new(-355.33f, 296.35f), new(-353.71f, 296.84f), new(-353.28f, 296.56f), new(-353.10f, 295.94f), new(-353.08f, 293.84f),
    new(-353.27f, 293.33f), new(-355.24f, 292.83f), new(-355.62f, 292.37f), new(-357.24f, 292.25f), new(-357.40f, 291.74f),
    new(-357.40f, 285.54f), new(-357.70f, 284.99f), new(-359.78f, 284.96f), new(-360.06f, 279.52f), new(-356.43f, 279.23f),
    new(-355.80f, 279.33f), new(-334.90f, 279.37f), new(-334.30f, 279.26f), new(-317.25f, 279.22f)];
    // Centroid of the polygon is at: (-319.210f, 323.281f)

    public static readonly ArenaBoundsCustom arena = new([new PolygonCustom(vertices)]);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            e.Priority = e.Actor.OID switch
            {
                (uint)OID.BaleenGuard => 2,
                (uint)OID.Boss => 1,
                _ => 0
            };
        }
    }
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.BaleenGuard));
    }
}
