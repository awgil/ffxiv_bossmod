namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

sealed class CthonicFury(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    public bool Active => _aoe.Length != 0 || Arena.Bounds is not ArenaBoundsCircle;
    public readonly AOEShapeCustom AOEBurningBattlements = new(module.Arena.Center, GetDefaultSquare(), [new Square(new(150f, 800f), 11.5f, 45f.Degrees())]);
    private readonly AOEShapeCustom aoeCthonicFury = new(module.Arena.Center, GetDefaultSquare(), GetCustomShape());
    private static Square[] GetDefaultSquare() => [new Square(new(150f, 800f), 30f)]; // using a square for the difference instead of a circle since less vertices will result in slightly better performance

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public static Shape[] GetCustomShape()
    {
        const int RadiusSmall = 8;
        const int HalfWidth = 2;
        const int Edges = 64;
        Polygon[] circles = [new(new(166.251f, 800f), RadiusSmall, Edges), new(new(133.788f, 800f), RadiusSmall, Edges),
        new(new(150f, 816.227f), RadiusSmall, Edges), new(new(150f, 783.812f), RadiusSmall, Edges)]; // the circle coordinates are not perfectly placed for some reason, got these from analyzing the collision data
        RectangleSE[] rects = [new(circles[1].Center, circles[2].Center, HalfWidth), new(circles[1].Center, circles[3].Center, HalfWidth),
        new(circles[3].Center, circles[0].Center, HalfWidth), new(circles[0].Center, circles[2].Center, HalfWidth)];
        Shape[] combined = [.. circles, .. rects];
        return combined;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.CthonicFuryStart)
        {
            var loc = Arena.Center;
            _aoe = [new(aoeCthonicFury, loc, default, Module.CastFinishAt(spell), shapeDistance: aoeCthonicFury.Distance(loc, default))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CthonicFuryStart:
                _aoe = [];
                var arena = new ArenaBoundsCustom(GetCustomShape());
                SetArena(arena, arena.Center);
                break;
            case (uint)AID.CthonicFuryEnd:
                SetArena(new ArenaBoundsCircle(30f), new(150f, 800f));
                break;
        }

        void SetArena(ArenaBounds bounds, WPos center)
        {
            Arena.Bounds = bounds;
            Arena.Center = center;
        }
    }
}

sealed class BurningCourtMoatKeepBattlements(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [with(5)];
    private readonly AOEShape _shapeC = new AOEShapeCircle(8f);
    private readonly AOEShape _shapeM = new AOEShapeDonut(5f, 15f);
    private readonly AOEShape _shapeK = new AOEShapeRect(23f, 11.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = spell.Action.ID switch
        {
            (uint)AID.BurningCourt => _shapeC,
            (uint)AID.BurningMoat => _shapeM,
            (uint)AID.BurningKeep => _shapeK,
            (uint)AID.BurningBattlements => Module.FindComponent<CthonicFury>()!.AOEBurningBattlements,
            _ => null
        };
        if (shape != null)
        {
            var loc = spell.LocXZ;
            var rot = spell.Rotation;
            AOEs.Add(new(shape, loc, rot, Module.CastFinishAt(spell), actorID: caster.InstanceID, shapeDistance: shape.Distance(loc, rot)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is >= (uint)AID.BurningCourt and <= (uint)AID.BurningBattlements)
        {
            ++NumCasts;
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            var len = aoes.Length;
            var id = caster.InstanceID;
            for (var i = 0; i < len; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == id)
                {
                    AOEs.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class EchoesOfAgony(BossModule module) : Components.StackWithIcon(module, (uint)IconID.EchoesOfAgony, (uint)AID.EchoesOfAgonyAOE, 5f, 9.2d, PartyState.MaxAllianceSize, PartyState.MaxAllianceSize)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.EchoesOfAgony)
        {
            NumFinishedStacks = 0;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == StackAction)
        {
            if (++NumFinishedStacks >= 5)
            {
                Stacks.Clear();
            }
        }
    }
}
