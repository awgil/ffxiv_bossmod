namespace BossMod.RealmReborn.Dungeon.D17PharosSirius.D173Tyrant;

public enum OID : uint
{
    Tyrant = 0x918,
    Helper = 0x233C,
    ZombieSailor = 0x91A, // R0.750, x?
}

public enum AID : uint
{
    AutoAttack = 870, // Tyrant->player, no cast, single-target
    AutoAttack1 = 872, // ZombieSailor->player, no cast, single-target
    AeroBlast = 1672, // Tyrant->self, 3.0s cast, range 40+R circle
    Whipcrack = 1673, // Tyrant->self, no cast, range 4+R width 3 rect
    Wallop = 1670, // ZombieSailor->self, no cast, range 3+R width 3 rect
    Bombination = 1674, // Tyrant->self, 3.0s cast, range 6+R circle
}

public enum SID : uint
{
    Windburn = 269, // Tyrant->player, extra=0x0
}

sealed class AeroBlast(BossModule module) : Components.RaidwideCast(module, (uint)AID.AeroBlast);

sealed class Bombination(BossModule module)
    : Components.SimpleAOEs(module, (uint)AID.Bombination, new AOEShapeCircle(6f));

sealed class SailorAdds(BossModule module) : Components.Adds(module, (uint)OID.ZombieSailor);

[SkipLocalsInit]
sealed class TyrantStates : StateMachineBuilder
{
    public TyrantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AeroBlast>()
            .ActivateOnEnter<Bombination>()
            .ActivateOnEnter<SailorAdds>()
            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(TyrantStates),
    ConfigType = null, // replace null with typeof(TyrantConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Tyrant,
    Contributors = "wen",
    Expansion = BossModuleInfo.Expansion.RealmReborn,
    Category = BossModuleInfo.Category.Dungeon,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 17u,
    NameID = 2264u,
    SortOrder = 3,
    PlanLevel = 0)]
// (0, 140, 0) for hyperborea
[SkipLocalsInit]
public sealed class D173Tyrant : BossModule
{
    public D173Tyrant(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    // Constructor so we can build arena
    private D173Tyrant(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    public static readonly WPos ArenaCenter = new(0f, 0f);

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        List<Shape> _shapes = [];

        // Starting position for rotation around origin.
        WPos startPos = new(20f, 0f);

        // 4 light columns at intercardinals
        Array.ForEach(Angle.AnglesIntercardinals, interCardinal =>
        {
            var pos = WPos.RotateAroundOrigin(interCardinal.Deg, ArenaCenter, startPos);
            _shapes.Add(new Rectangle(pos, 1f, 1f, interCardinal));
        });

        // The cutouts for border pipes and other shapes start southeast and move ccw around until sw.
        ArenaBoundsCustom arena = new(
            [
                new Circle(ArenaCenter, 20f)
            ],
            [
                .. _shapes,
                new Rectangle(new(-0f, -20.0f), 4.45f, 0.5f, -0f.Degrees()), // flat shape for gate
                new Rectangle(new(0f, 20.0f), 4.45f, 0.5f, 0f.Degrees()), // flat shape for gate
                new Rectangle(new WPos(8.1f, 18.2f), 1.8f, 1f,
                    30.Degrees()), // first pipe box that connects to pipe column from south ccw
                new Circle(new WPos(9.5f, 16.5f), 1f), new Rectangle(new WPos(10f, 17.3f), 1f, 1f, 30f.Degrees()),
                new Rectangle(new WPos(18.5f, 7.5f), 1f, 0.75f, 65f.Degrees()),
                new Circle(new WPos(19f, 2.5f), 1f), new Rectangle(new WPos(19.8f, 2.6f), 1f, 1f, 1f.Degrees()),
                new Rectangle(new WPos(20f, 0f), 1f, 0.75f),
                new Circle(new WPos(18.8f, -2.6f), 1f), new Rectangle(new WPos(19.8f, -2.8f), 1f, 1f, 6f.Degrees()),
                new Square(new WPos(18.5f, -7.7f), 0.75f, 26f.Degrees()),
                new Rectangle(new WPos(11.7f, -16.2f), 1.8f, 1f, -30f.Degrees()),
                new Circle(new WPos(9.5f, -16.5f), 1f), new Rectangle(new WPos(10.0f, -17.3f), 1f, 1f, 60f.Degrees()),
                new Rectangle(new WPos(-18.25f, -7.0f), 1.8f, 1f, -105f.Degrees()),
                new Circle(new WPos(-18.4f, -4.9f), 1f),
                new Rectangle(new WPos(-19.3f, -5.2f), 1f, 1f, -106.5f.Degrees()),
                new Circle(new WPos(-18.8f, -2.6f), 1f),
                new Rectangle(new WPos(-19.8f, -2.8f), 1f, 1f, -90.5f.Degrees()),
                new Square(new WPos(-20f, 0f), 0.75f),
                new Circle(new WPos(-18.8f, 2.6f), 1f), new Rectangle(new WPos(-19.8f, 2.7f), 1f, 1f, 3f.Degrees()),
                new Square(new WPos(-18.5f, 7.7f), 0.75f, 25f.Degrees()), // last square cutout before sw lantern.
            ]);
        return (ArenaCenter, arena);
    }
}
