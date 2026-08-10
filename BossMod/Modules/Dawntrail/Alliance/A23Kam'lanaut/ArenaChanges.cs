namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly AOEShapeDonut donut = new(20f, 40f);
    private readonly AOEShapeCircle circle = new(5);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    private static Rectangle[] GenerateBridges()
    {
        var northCenter = new WDir(default, -20f);
        var rects = new Rectangle[3];
        var a120 = 120f.Degrees();
        var center = new WPos(-200f, 150f);
        for (var i = 0; i < 3; ++i)
        {
            var angle = a120 * i;
            rects[i] = new(center + northCenter.Rotate(angle), 5f, 20f, angle);
        }
        return rects;
    }
    private static Polygon[] BuildP2Circle() => [new(new(-200f, 150f), 20f, 128)];
    private static Polygon[] BuildVoidzone() => [new(new(-200f, 150f), 5f, 64)];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ProvingGround)
        {
            var loc = spell.LocXZ;
            _aoe = [new(circle, loc, default, Module.CastFinishAt(spell), shapeDistance: circle.Distance(loc, default))];
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.ProvingGroundVoidzone)
        {
            _aoe = [];
            Arena.Bounds = Arena.Bounds.Radius == 29.5f ? new ArenaBoundsCustom(A23Kamlanaut.BuildP1Circle(), BuildVoidzone()) : BuildP2ArenaWithBridgesDonut();
        }
    }

    public override void OnActorRenderflagsChange(Actor actor, int renderflags)
    {
        if (renderflags == 256 && actor.OID == (uint)OID.ProvingGroundVoidzone)
        {
            Arena.Bounds = Arena.Bounds.Radius == 29.5f ? A23Kamlanaut.BuildArena().arena : BuildP2ArenaWithBridges();
        }
    }

    private static Shape[] BuildP2Shapes() => [.. BuildP2Circle(), .. GenerateBridges()];
    private static ArenaBoundsCustom BuildP2ArenaWithBridges() => new(BuildP2Shapes(), ScaleFactor: 1.15f);
    private static ArenaBoundsCustom BuildP2ArenaWithBridgesDonut() => new(BuildP2Shapes(), BuildVoidzone(), ScaleFactor: 1.15f);

    public override void OnMapEffect(byte index, uint state)
    {
        switch (index)
        {
            case 0x00: // p1/p2 transition
                switch (state)
                {
                    case 0x00020001u:
                        var center = Arena.Center;
                        AOEShapeCustom shape = new(center, A23Kamlanaut.BuildP1Circle(), BuildP2Shapes());
                        _aoe = [new(shape, center, default, WorldState.FutureTime(5.1d), shapeDistance: shape.Distance(center, default))];
                        break;
                    case 0x00200010u:
                        SetArena(BuildP2ArenaWithBridges());
                        break;
                }
                break;
            case 0x63: // bridges
                switch (state)
                {
                    case 0x00200010u:
                        var center = new WPos(-200f, 150f);
                        _aoe = [new(donut, center, default, WorldState.FutureTime(4.3d), shapeDistance: donut.Distance(center, default))];
                        break;
                    case 0x00020001u:
                        SetArena(new ArenaBoundsCustom(BuildP2Circle()));
                        _aoe = [];
                        break;
                    case 0x00080004u:
                        SetArena(BuildP2ArenaWithBridges());
                        break;
                }
                break;
        }
        void SetArena(ArenaBoundsCustom arena)
        {
            Arena.Bounds = arena;
            Arena.Center = arena.Center;
            _aoe = [];
        }
    }
}
