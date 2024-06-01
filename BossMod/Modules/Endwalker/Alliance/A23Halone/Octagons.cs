namespace BossMod.Endwalker.Alliance.A23Halone;

// TODO: assign alliances members to a specific octagon. in duty finder it is usually:
// NW (Octagon3): Alliance A
// NE (Octagon1): Alliance C
// S (Octagon2): Alliance B
class Octagons(BossModule module) : Components.GenericAOEs(module)
{
    private ArenaBounds? _arena;
    private const float InnerRadius = 11.5f;
    private const float OuterRadius = 12.5f;
    private const int Vertices = 8;
    private static readonly WPos[] spears = [new(-686, 592), new(-700, 616.2f), new(-714, 592)];
    private static readonly Angle[] angle = [37.5f.Degrees(), 22.5f.Degrees(), -37.5f.Degrees()];
    private static readonly List<Shape> shapes = [new Polygon(spears[0], InnerRadius, Vertices, angle[0]),
    new Polygon(spears[0], OuterRadius, Vertices, angle[0]), new Polygon(spears[1], InnerRadius, Vertices, angle[1]),
    new Polygon(spears[1], OuterRadius, Vertices, angle[1]), new Polygon(spears[2], InnerRadius, Vertices, angle[2]),
    new Polygon(spears[2], OuterRadius, Vertices, angle[2])];
    private static readonly List<Shape> baseArena = [new Circle(new WPos(-700, 600), 30)];
    public readonly List<Shape> octagonsInner = [];
    public readonly List<Shape> octagonsOuter = [];
    public static readonly ArenaBounds arenaDefault = new ArenaBoundsCircle(30);
    public AOEInstance? _aoe;
    public static readonly AOEShapeCustom customShape = new(baseArena, [shapes[0], shapes[2], shapes[4]]);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        //x07 = south, x06 = east, x05 = west x00020001 walls activate, x00200004 disappear
        // telegraph - 0x00100008
        switch (state)
        {
            case 0x00100008 when index == 0x07:
                _aoe = new(customShape, Module.Arena.Center);
                break;
            case 0x00020001 when index == 0x07:
                AddOctagons();
                _aoe = null;
                break;
            case 0x00200004:
                RemoveOctagons(index);
                break;
        }
        _arena = new ArenaBoundsComplex(baseArena, octagonsOuter, octagonsInner);
        Module.Arena.Bounds = _arena;
    }

    private void AddOctagons()
    {
        octagonsInner.AddRange([shapes[0], shapes[2], shapes[4]]);
        octagonsOuter.AddRange([shapes[1], shapes[3], shapes[5]]);
    }

    private void RemoveOctagons(byte index)
    {
        switch (index)
        {
            case 0x06:
                octagonsInner.Remove(shapes[0]);
                octagonsOuter.Remove(shapes[1]);
                break;
            case 0x07:
                octagonsInner.Remove(shapes[2]);
                octagonsOuter.Remove(shapes[3]);
                break;
            case 0x05:
                octagonsInner.Remove(shapes[4]);
                octagonsOuter.Remove(shapes[5]);
                break;
        }
    }
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        //TODO fix AI map creation
    }
}
