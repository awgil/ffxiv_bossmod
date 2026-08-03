namespace BossMod.RealmReborn.Dungeon.D17PharosSirius.D173Tyrant;

public enum OID : uint
{
    Tyrant = 0x918,
    Helper = 0x233C,
    /*
    _Gen_Actor1e8f28 = 0x1E8F28, // R2.000, x?, EventObj type
    _Gen_Actor1e8f36 = 0x1E8F36, // R2.000, x?, EventObj type
    _Gen_Actor1e8f38 = 0x1E8F38, // R2.000, x?, EventObj type
    _Gen_Actor1e8f27 = 0x1E8F27, // R2.000, x?, EventObj type
    _Gen_Actor1e8f63 = 0x1E8F63, // R2.000, x?, EventObj type
    _Gen_Actor1e8f26 = 0x1E8F26, // R2.000, x?, EventObj type
    _Gen_Actor1e8f24 = 0x1E8F24, // R2.000, x?, EventObj type
    _Gen_Actor1e8f2f = 0x1E8F2F, // R0.500, x?, EventObj type
    _Gen_Actor1e8f22 = 0x1E8F22, // R2.000, x?, EventObj type
    _Gen_Actor1e8f37 = 0x1E8F37, // R2.000, x?, EventObj type
    _Gen_Actor1e8f21 = 0x1E8F21, // R2.000, x?, EventObj type
    _Gen_Actor1e8f20 = 0x1E8F20, // R2.000, x?, EventObj type
    _Gen_Actor1e8f1e = 0x1E8F1E, // R2.000, x?, EventObj type
    _Gen_Actor1e91ec = 0x1E91EC, // R2.000, x?, EventObj type
    _Gen_Actor1e8f29 = 0x1E8F29, // R2.000, x?, EventObj type
    _Gen_Actor1e8f25 = 0x1E8F25, // R2.000, x?, EventObj type
    _Gen_Actor1e8f23 = 0x1E8F23, // R2.000, x?, EventObj type
    _Gen_DriftingSoul = 0x8FF, // R0.800, x?
    */
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



// AeroBlast = 1672, // Tyrant->self, 3.0s cast, range 40+R circle
sealed class AeroBlast(BossModule module) : Components.RaidwideCast(module, (uint)AID.AeroBlast);

// Whipcrack = 1673, // Tyrant->self, no cast, range 4+R width 3 rect
sealed class Whipcrack(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Whipcrack, new AOEShapeRect(2f, 1.5f));

// Wallop = 1670, // ZombieSailor->self, no cast, range 3+R width 3 rect
sealed class Wallop(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Wallop, new AOEShapeRect(3f, 1.5f));

// Bombination = 1674, // Tyrant->self, 3.0s cast, range 6+R circle
sealed class Bombination(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Bombination, new AOEShapeCircle(6f));

sealed class SailorAdds(BossModule module) : Components.Adds(module, (uint)OID.ZombieSailor);



[SkipLocalsInit]
sealed class TyrantStates : StateMachineBuilder
{
    public TyrantStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AeroBlast>()
            .ActivateOnEnter<Whipcrack>() //Instant cast
            .ActivateOnEnter<Wallop>()    //Instant cast
            .ActivateOnEnter<Bombination>()
            .ActivateOnEnter<SailorAdds>()


            ;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(TyrantStates),
    ConfigType = null, // replace null with typeof(TyrantConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.Tyrant,
    Contributors = "wen",
    Expansion = BossModuleInfo.Expansion.RealmReborn,
    Category = BossModuleInfo.Category.Dungeon,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 17u,
    NameID = 2264u,
    SortOrder = 1,
    PlanLevel = 0)]
// (0, 140, 0) for hyperborea
[SkipLocalsInit]
//public sealed class Tyrant(WorldState ws, Actor primary) : BossModule(ws, primary, new(0f, -0f), new ArenaBoundsCircle(20f));
public sealed class D173Tyrant : BossModule
{
    public D173Tyrant(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }


// Constructor so we can build arena
    private D173Tyrant(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary,
        a.center,
        a.arena)
    {
    }

    public static readonly WPos ArenaCenter = new(0f, 0f);


    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        /*
         * cardinal directions have circles at plus and minus 15 degrees on outside edge for eight circle cutouts.
         * intercardinal direction for 4 rectangle cutouts.
         */
        List<Shape> _shapes = [];

        // Starting position for rotation around origin.
        WPos startPos = new(20f, 0f);

        // outer eastern circle boundary
        WPos [] shape1 =
        [
            new(19.50993f, 3.88077f), new(18.37795f, 7.61240f), new(16.53972f, 11.05149f), new(14.06588f, 14.06588f),
            new(11.05148f, 16.53972f), new(7.61239f, 18.37795f), new(5.03392f, 19.16012f), new(4.66002f, 18.89043f),
            new(4.83478f, 18.83735f), new(4.28392f, 18.83735f), new(3.61032f, 18.00042f), new(5.78392f, 18.00050f),
            new(5.78392f, 18.54907f), new(7.47095f, 18.03667f), new(10.84624f, 16.23253f), new(13.80468f, 13.80468f),
            new(16.23259f, 10.84627f), new(18.03669f, 7.47105f), new(19.14759f, 3.80864f), new(19.52277f, 0.00000f),
            new(19.14760f, -3.80877f), new(18.03669f, -7.47105f), new(16.23259f, -10.84627f),
            new(13.80468f, -13.80468f), new(10.84627f, -16.23268f), new(7.47098f, -18.03671f),
            new(5.78392f, -18.54909f), new(5.78392f, -18.00050f), new(3.61042f, -18.00060f), new(4.28392f, -18.83735f),
            new(4.83479f, -18.83735f), new(4.66002f, -18.89043f), new(5.03392f, -19.16012f), new(7.61239f, -18.37795f),
            new(11.05148f, -16.53972f), new(14.06588f, -14.06588f), new(16.53972f, -11.05149f),
            new(18.37795f, -7.61240f), new(19.50993f, -3.88077f), new(19.89215f, 0.00000f)
        ];

        // inner circle shape that encloses the see through mesh floor. Can get rid of this
        WPos[] shape2 =
        [
            new(3.03971f, -15.28205f), new(5.96272f, -14.39530f), new(8.65654f, -12.95543f), new(11.01768f, -11.01768f),
            new(12.95543f, -8.65654f), new(14.39530f, -5.96273f), new(15.28195f, -3.03984f), new(15.58137f, 0.00000f),
            new(15.28197f, 3.03977f), new(14.39530f, 5.96273f), new(12.95544f, 8.65654f), new(11.01768f, 11.01769f),
            new(8.65654f, 12.95542f), new(5.96273f, 14.39528f), new(3.03977f, 15.28190f), new(0.00001f, 15.58135f),
            new(-3.03996f, 15.28193f), new(-5.96272f, 14.39529f), new(-8.65653f, 12.95543f), new(-11.01768f, 11.01768f),
            new(-12.95542f, 8.65654f), new(-14.39557f, 5.96269f), new(-15.28196f, 3.03978f), new(-15.58136f, 0.00000f),
            new(-15.28196f, -3.03976f), new(-14.39530f, -5.96272f), new(-12.95543f, -8.65653f),
            new(-11.01768f, -11.01768f), new(-8.65654f, -12.95543f), new(-5.96273f, -14.39529f),
            new(-3.04002f, -15.28205f), new(-0.00001f, -15.58136f)
        ];

        WPos[] shape3 =
        [
            new(-3.34724f, -19.71335f), new(-3.25694f, -19.16426f), new(-4.46291f, -19.14930f)
        ];

        WPos[] shape4 =
        [
            new(18.25881f, -2.15230f), new(18.73322f, -1.95583f), new(18.73325f, -1.77341f), new(18.12994f, -2.02330f),
            new(18.12985f, -2.02338f), new(17.88005f, -2.62661f), new(18.12985f, -3.23000f), new(18.73314f, -3.47992f),
            new(18.73325f, -3.47981f), new(18.73322f, -3.29750f), new(18.25881f, -3.10103f), new(18.06228f, -2.62661f)
        ];

        WPos[] shape5 =
        [
            new(-19.13924f, -0.50008f), new(-19.13920f, -0.50000f), new(-19.13920f, 0.50000f),
            new(-19.13927f, 0.49995f), new(-19.13923f, -0.13639f)

        ];

        WPos[] shape6 =
        [
            new(-3.25684f, 19.16416f), new(-2.14110f, 19.98951f), new(-3.34722f, 19.71315f)

        ];

        WPos[] shape7 =
        [
            new(-18.61823f, -3.48031f), new(-18.01482f, -3.23030f), new(-17.76492f, -2.62700f),
            new(-18.01482f, -2.02370f), new(-18.61813f, -1.77380f), new(-18.61823f, -1.77385f),
            new(-18.61813f, -1.77390f), new(-18.61821f, -1.95621f), new(-18.14378f, -2.15267f),
            new(-17.94735f, -2.62710f), new(-18.14378f, -3.10141f), new(-18.61821f, -3.29787f),
            new(-18.61813f, -3.48021f)

        ];

        // western side outer arena circle shape
        WPos[] shape8 =
        [
            new(-19.52277f, 0.00000f), new(-19.14769f, -3.80877f), new(-18.03669f, -7.47105f),
            new(-16.23259f, -10.84627f), new(-13.80468f, -13.80468f), new(-10.84627f, -16.23260f),
            new(-7.47113f, -18.03677f), new(-5.78392f, -18.54913f), new(-5.78392f, -18.00050f),
            new(-3.61047f, -18.00055f), new(-4.28392f, -18.83735f), new(-4.83481f, -18.83735f),
            new(-4.66002f, -18.89043f), new(-5.03392f, -19.16012f), new(-7.61239f, -18.37795f),
            new(-11.05148f, -16.53972f), new(-14.06588f, -14.06588f), new(-16.53972f, -11.05149f),
            new(-18.37795f, -7.61240f), new(-19.50993f, -3.88077f), new(-19.89215f, 0.00000f),
            new(-19.50993f, 3.88077f), new(-18.37795f, 7.61240f), new(-16.53972f, 11.05149f),
            new(-14.06588f, 14.06588f), new(-11.05148f, 16.53972f), new(-7.61239f, 18.37795f),
            new(-5.03392f, 19.16012f), new(-4.66002f, 18.89043f), new(-4.83477f, 18.83735f), new(-4.28392f, 18.83735f),
            new(-3.61054f, 18.00046f), new(-5.78392f, 18.00050f), new(-5.78392f, 18.54905f), new(-7.47114f, 18.03657f),
            new(-10.84627f, 16.23260f), new(-13.80468f, 13.80468f), new(-16.23259f, 10.84627f),
            new(-18.03669f, 7.47105f), new(-19.14769f, 3.80864f)

        ];

        WPos[] shape9 =
        [
            new(3.25671f, -19.16430f), new(3.34709f, -19.71330f), new(2.14110f, -19.98951f)
        ];


        WPos[] shape10 =
        [
            new(3.25679f, 19.16413f), new(3.34709f, 19.71322f), new(4.46276f, 19.14918f)
        ];

        WPos[] shape11 =
        [
            new(19.13917f, -0.10160f), new(19.13912f, 0.49995f), new(19.13920f, 0.50000f), new(19.13920f, -0.50000f),
            new(19.13915f, -0.50008f)
        ];

        WPos[] shape12 =
        [
            new(-18.73326f, 1.77333f), new(-18.73325f, 1.77341f), new(-18.73332f, 1.95571f), new(-18.25891f, 2.15218f),
            new(-18.06249f, 2.62649f), new(-18.25891f, 3.10091f), new(-18.73332f, 3.29738f), new(-18.73325f, 3.47979f),
            new(-18.73326f, 3.47980f), new(-18.73325f, 3.47981f), new(-18.12994f, 3.22991f), new(-17.88005f, 2.62661f),
            new(-18.12994f, 2.02330f)
        ];

        WPos[] holeShape1 =
        [
            new(12.73196f, 13.70406f), new(13.70301f, 12.73301f), new(13.70296f, 12.73296f)
        ];

        WPos[] holeShape2 =
        [
            new(18.01482f, 2.02365f), new(17.76492f, 2.62700f), new(18.01482f, 3.23030f), new(18.61813f, 3.48021f),
            new(18.61810f, 3.29776f), new(18.14367f, 3.10129f), new(17.94724f, 2.62698f), new(18.14367f, 2.15255f),
            new(18.61810f, 1.95608f), new(18.61813f, 1.77380f), new(18.61812f, 1.77372f)

        ];


        // 8 circle conduit plus and minus fifteen degrees of cardinal directions.
        /*Array.ForEach(Angle.AnglesCardinals, cardinal =>
        {
            WPos minusFifteenDegrees = WPos.RotateAroundOrigin(cardinal.Deg - 15.0f, ArenaCenter, startPos);
            WPos plusFifteenDegrees = WPos.RotateAroundOrigin(cardinal.Deg + 15.0f, ArenaCenter, startPos);
            _shapes.Add(new Circle(plusFifteenDegrees, 1f));
            _shapes.Add(new Circle(minusFifteenDegrees, 1f));
        });*/

        // 4 rectangle lights at intercardinals
        Array.ForEach(Angle.AnglesIntercardinals, interCardinal =>
        {
            WPos pos = WPos.RotateAroundOrigin(interCardinal.Deg, ArenaCenter, startPos);
            //_shapes.Add(new Square(pos, 1f, interCardinal));
            _shapes.Add(new Rectangle(pos, 1f, 1f, interCardinal));
        });

        // Add in rectangles for the gate entrance and exit.
        //ArenaBoundsCustom arena = new([new Circle(ArenaCenter, 20f)],
        ArenaBoundsCustom arena = new(
            [
                new Circle(ArenaCenter, 20f)
            ],
        [
            .._shapes, new Rectangle(new(-0f, -20.0f), 4.45f, 0.5f, -0f.Degrees()),
            new Rectangle(new(0f, 20.0f), 4.45f, 0.5f, 0f.Degrees()),
            new Rectangle(new WPos(8.1f, 18.2f), 1.8f, 1f, 30.Degrees()), // first pipe box that connects to pipe column from south ccw
            new Circle(new WPos(9.5f, 16.5f), 1f), new Rectangle(new WPos(10f, 17.3f), 1f, 1f, 30f.Degrees()), //verified
            new Rectangle(new WPos(18.5f, 7.5f), 1f, 0.75f, 65f.Degrees()), //verified
            new Circle(new WPos(19f, 2.5f), 1f), new Rectangle(new WPos(19.8f, 2.6f), 1f, 1f, 1f.Degrees()), //verified
            new Rectangle(new WPos(20f, 0f), 1f, 0.75f), //verified
            new Circle(new WPos(18.8f, -2.6f), 1f), new Rectangle(new WPos(19.8f, -2.8f), 1f, 1f, 6f.Degrees()), //verified
            new Square(new WPos(18.5f, -7.7f), 0.75f, 26f.Degrees()),
            new Rectangle(new WPos(11.7f, -16.2f), 1.8f, 1f, -30f.Degrees()), //verified
            new Circle(new WPos(9.5f, -16.5f), 1f), new Rectangle(new WPos(10.0f, -17.3f), 1f, 1f, 60f.Degrees()), //verified
            new Rectangle(new WPos(-18.25f, -7.0f), 1.8f, 1f, -105f.Degrees()), //verified
            new Circle(new WPos(-18.4f, -4.9f), 1f), new Rectangle(new WPos(-19.3f, -5.2f), 1f, 1f, -106.5f.Degrees()), //verified
            new Circle(new WPos(-18.8f, -2.6f), 1f), new Rectangle(new WPos(-19.8f, -2.8f), 1f, 1f, -90.5f.Degrees()), //verified
            new Square(new WPos(-20f, 0f), 0.75f),
            new Circle(new WPos(-18.8f, 2.6f), 1f), new Rectangle(new WPos(-19.8f, 2.7f), 1f, 1f, 3f.Degrees()),
            new Square(new WPos(-18.5f, 7.7f), 0.75f, 25f.Degrees()),

        ]);


        /*
         * Vertices differences shapes
         *
         *
         *
           new PolygonCustom(shape3),
           new PolygonCustom(shape4), new PolygonCustom(shape5), new PolygonCustom(shape6),
           new PolygonCustom(shape7), new PolygonCustom(shape9), new PolygonCustom(shape10),
           new PolygonCustom(shape11), new PolygonCustom(shape12), new PolygonCustom(holeShape1), new PolygonCustom(holeShape2),
         */
        /*[
            new Rectangle(new(-0f, -20.0f), 4.45f, 0.5f, -0f.Degrees()),
            new Rectangle(new(0f, 20.0f), 4.45f, 0.5f, 0f.Degrees()), new PolygonCustom(shape1), new PolygonCustom(shape2), new PolygonCustom(shape3), new PolygonCustom(shape3), new PolygonCustom(shape4), new PolygonCustom(shape5), new PolygonCustom(shape6), new PolygonCustom(shape7), new PolygonCustom(shape8), new PolygonCustom(shape9), new PolygonCustom(shape10), new PolygonCustom(shape11), new PolygonCustom(shape12),

        ]);*/

        return (ArenaCenter, arena);
    }
}







/*
var outers = new WPos[][] {
   };
   var holes = new WPos[][][] {
       new[] {
       new[] {  },
       new[] {  },
       new[] {  },
       new[] {  },
       new[] {  },
       new[] {  },
       new[] {  },
       new[] {  },
       new[] {  },
       new[] {  },
       new[] {  }
   };




*/
