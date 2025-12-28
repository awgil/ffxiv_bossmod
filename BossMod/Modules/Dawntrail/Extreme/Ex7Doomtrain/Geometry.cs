namespace BossMod.Dawntrail.Extreme.Ex7Doomtrain;

static class Shapes
{
    public static IEnumerable<WDir> Fence(float x, float z) => CurveApprox.TruncatedRect(new(x, z), new WDir(2.5f, 0), new(0, 2.5f), 0.5f, CurveApprox.Corners.All);
    public static IEnumerable<WDir> Crate(float x, float z, CurveApprox.Corners c = CurveApprox.Corners.All) => CurveApprox.TruncatedRect(new(x, z), new WDir(2.9f, 0), new(0, 2.9f), 0.5f, c);

    public static Func<WPos, bool> RectOutline(WPos center, float lenFront, float lenBack, float halfWidth) => p => p.InRect(center, default(Angle), lenFront + 0.25f, lenBack + 0.25f, halfWidth + 0.25f) && !p.InRect(center, default(Angle), lenFront - 0.25f, lenBack - 0.25f, halfWidth - 0.25f);
}

class CarGeometry : BossComponent
{
    public int Car
    {
        get => field;
        set
        {
            field = value;
            switch (value)
            {
                case 2:
                    Car2();
                    break;
                case 3:
                    Car3();
                    break;
                case 4:
                    Car4();
                    break;
                case 5:
                    Car5();
                    break;
                case 6:
                    Car6();
                    break;
            }
        }
    } = 1;

    public const float CarHeight = 14.6f;

    public AOEShape GroundShape { get; private set; } = new AOEShapeRect(CarHeight, 10, CarHeight);
    public AOEShape AirShape { get; private set; } = new AOEShapeRect(CarHeight, 10, CarHeight);
    List<(WPos from, WPos to)> _portals = [];
    public IEnumerable<WPos> Portals => _portals.Select(p => p.from);

    private BitMask _highPlayers;

    BitMask _car5Crates = BitMask.Build([.. Enumerable.Range(0, 16)]);

    PolygonClipper Clipper => Arena.Bounds.Clipper;

    public CarGeometry(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.UpHigh && Raid.TryFindSlot(actor, out var slot))
            _highPlayers.Set(slot);
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.UpHigh && Raid.TryFindSlot(actor, out var slot))
            _highPlayers.Clear(slot);
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 0x10 and < 0x20 && state == 0x00100001)
        {
            _car5Crates.Clear(index - 0x10);
            Car5();
        }

        if (index == 0x20 && state == 0x00100001)
        {
            Car6(false);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        switch (Car)
        {
            case 4:
                AirShape.Outline(Arena, new WPos(100, 250), default, ArenaColor.Border);
                break;
            case 6:
                AirShape.Outline(Arena, new WPos(100, 350), default, ArenaColor.Border);
                break;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.Portals.AddRange(_portals.Select(p => (p.from, 3f, p.to)));

        switch (Car)
        {
            case 4:
                if (actor.PosRot.Y < 4)
                {
                    var c2 = new WPos(107.5f, 250);
                    hints.TemporaryObstacles.Add(Shapes.RectOutline(c2, 5, 5, 2.5f));

                    var c1 = new WPos(92.5f, 257.5f);
                    hints.TemporaryObstacles.Add(Shapes.RectOutline(c1, 7.5f, 7.5f, 2.5f));
                }
                break;
            case 6:
                if (actor.PosRot.Y < 4)
                {
                    var c3 = new WPos(92.5f, 355);
                    hints.TemporaryObstacles.Add(Shapes.RectOutline(c3, 10, 10, 2.5f));
                }
                break;
        }
    }

    void Car2()
    {
        var clipper = Arena.Bounds.Clipper;

        var rect = CurveApprox.Rect(new WDir(10, 0), new WDir(0, CarHeight));

        var poly = clipper.Difference(new(rect), new(Shapes.Fence(2.5f, 7.5f)));
        poly = clipper.Difference(new(poly), new(Shapes.Fence(-2.5f, -2.5f)));

        // crates
        poly = clipper.Difference(new(poly), new(Shapes.Crate(7.575f, -2.425f)));
        poly = clipper.Difference(new(poly), new(Shapes.Crate(-7.425f, 7.425f)));

        Arena.Bounds = new ArenaBoundsCustom(CarHeight, poly);
        Arena.Center = new(100, 150);
    }

    void Car3()
    {
        Arena.Bounds = new ArenaBoundsRect(10, CarHeight);
        Arena.Center = new(100, 200);
    }

    public void MultiCar3()
    {
        var car = CurveApprox.Rect(new(10, 0), new(0, CarHeight));

        var centroid = new WPos(100, 225);

        var multibounds = new RelSimplifiedComplexPolygon([
            new RelPolygonWithHoles([..car.Select(c => c - new WDir(0, 25))]),
            new RelPolygonWithHoles([..car.Select(c => c + new WDir(0, 25))])
        ]);

        Arena.Bounds = new ArenaBoundsCustom(39.5f, multibounds, MapResolution: 1);
        Arena.Center = new(100, 225);
    }

    void Car4()
    {
        var rect = CurveApprox.Rect(new(10, 0), new(0, CarHeight));
        var doubleFence = CurveApprox.Rect(new(5, 0), new(0, 2));

        var bounds = Clipper.Difference(new(rect), new(doubleFence.Select(f => f + new WDir(0, 2.5f))));

        Arena.Bounds = new ArenaBoundsCustom(CarHeight, bounds);
        Arena.Center = new(100, 250);

        var platformRight = CurveApprox.Rect(new WDir(7.5f, 0), new WDir(2.5f, 0), new(0, 5));
        var platformLeft = CurveApprox.Rect(new WDir(-7.5f, 7.25f), new WDir(2.5f, 0), new(0, 7.25f));
        var airContour = Clipper.Union(new(platformRight), new(platformLeft));

        AirShape = new AOEShapeCustom(airContour);
        GroundShape = new AOEShapeCustom(Clipper.Difference(new(rect), new(airContour)));

        _portals = [
            (Arena.Center + new WDir(3.5f, -2.5f), Arena.Center + new WDir(7.5f, -2.5f)),
            (Arena.Center + new WDir(-3.5f, 10), Arena.Center + new WDir(-7.5f, 10))
        ];
    }

    public void MultiCar4()
    {
        var c4Poly = ((ArenaBoundsCustom)Arena.Bounds).Poly.Transform(new WDir(0, -25), new WDir(0, 1));
        var c5Poly = CurveApprox.Rect(new WDir(0, 25), new WDir(10, 0), new(0, CarHeight));

        var combined = Clipper.Union(new(c4Poly), new(c5Poly));

        Arena.Bounds = new ArenaBoundsCustom(39.5f, combined);
        Arena.Center = new(100, 275);
    }

    // car 5 crates are ordered LTR top to bottom starting at 0x10
    // XX.00100001 -> crate disappear
    void Car5()
    {
        var rect = CurveApprox.Rect(new(10, 0), new(0, CarHeight));

        RelSimplifiedComplexPolygon? crateShape = null;

        foreach (var index in _car5Crates.SetBits())
        {
            var crateCol = index % 4;
            var crateRow = index / 4;
            var crateX = -7.5f + crateCol * 5;
            var crateZ = -7.5f + crateRow * 5;

            var crateN = crateRow > 0 && _car5Crates[index - 4];
            var crateE = crateCol == 3 || _car5Crates[index + 1];
            var crateW = crateCol == 0 || _car5Crates[index - 1];
            var crateS = crateRow < 3 && _car5Crates[index + 4];

            var c = CurveApprox.Corners.None;
            if (!crateN && !crateE)
                c |= CurveApprox.Corners.NE;
            if (!crateN && !crateW)
                c |= CurveApprox.Corners.NW;
            if (!crateS && !crateE)
                c |= CurveApprox.Corners.SE;
            if (!crateS && !crateW)
                c |= CurveApprox.Corners.SW;

            var crateRect = Shapes.Crate(crateX, crateZ, c);

            crateShape = crateShape == null ? new(crateRect) : Clipper.Union(new(crateShape), new(crateRect));
        }

        Arena.Bounds = crateShape == null
            ? new ArenaBoundsRect(10, CarHeight)
            : new ArenaBoundsCustom(CarHeight, Clipper.Difference(new(rect), new(crateShape)));
        Arena.Center = new(100, 300);
    }

    public void MultiCar5()
    {
        var c5 = ((ArenaBoundsCustom)Arena.Bounds).Poly.Transform(new WDir(0, -25), new WDir(0, 1));
        var rect = CurveApprox.Rect(new WDir(0, 25), new WDir(10, 0), new WDir(0, CarHeight));

        Arena.Bounds = new ArenaBoundsCustom(39.5f, Clipper.Union(new(c5), new(rect)));
        Arena.Center = new(100, 325);
    }

    void Car6(bool withCrates = true)
    {
        var rect = CurveApprox.Rect(new(10, 0), new(0, CarHeight));
        var platformLeft = CurveApprox.Rect(new WDir(-7.5f, 4.75f), new WDir(2.5f, 0), new(0, 9.75f));

        var fence = Shapes.Fence(2.5f, 2.5f);

        AirShape = new AOEShapeCustom(new(platformLeft));
        GroundShape = new AOEShapeCustom(Clipper.Difference(new(rect), new(platformLeft)));

        var bounds = Clipper.Difference(new(rect), new(fence));
        if (withCrates)
        {
            var cratesRight = CurveApprox.TruncatedRect(new WDir(7.5f, -10), new WDir(2.5f, 0), new WDir(0, 5), 0.5f, CurveApprox.Corners.SW);
            var cratesSouth = CurveApprox.TruncatedRect(new WDir(5, 12.5f), new WDir(5, 0), new WDir(0, 2.5f), 0.5f, CurveApprox.Corners.NW);
            bounds = Clipper.Difference(new(bounds), new(cratesSouth));
            bounds = Clipper.Difference(new(bounds), new(cratesRight));
        }
        Arena.Bounds = new ArenaBoundsCustom(CarHeight, bounds);
        Arena.Center = new(100, 350);

        _portals = [
            (Arena.Center + new WDir(-3.5f, 0), Arena.Center + new WDir(-7.5f, 0))
        ];
    }
}
