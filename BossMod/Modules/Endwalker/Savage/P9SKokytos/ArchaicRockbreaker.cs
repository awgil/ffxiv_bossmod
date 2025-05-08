namespace BossMod.Endwalker.Savage.P9SKokytos;

class ArchaicRockbreakerCenter(BossModule module) : Components.StandardAOEs(module, AID.ArchaicRockbreakerCenter, 6);

class ArchaicRockbreakerShockwave(BossModule module) : Components.Knockback(module, AID.ArchaicRockbreakerShockwave, true)
{
    private readonly Uplift? _uplift = module.FindComponent<Uplift>();
    private readonly DateTime _activation = module.WorldState.FutureTime(6.5f);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        float distance = 21;
        if (_uplift?.WallDirection != null)
        {
            var offset = actor.Position - Module.Center;
            var dot = Math.Abs(_uplift.WallDirection.Value.ToDirection().Dot(offset.Normalized()));
            bool againstWall = dot is > 0.9238795f or < 0.3826834f;
            if (againstWall)
                distance = Module.Bounds.Radius - offset.Length() - 0.5f;
        }
        yield return new(Module.Center, distance, _activation);
    }
}

class ArchaicRockbreakerPairs : Components.UniformStackSpread
{
    public ArchaicRockbreakerPairs(BossModule module) : base(module, 6, 0, 2)
    {
        foreach (var p in Raid.WithoutSlot(true).Where(p => p.Class.IsSupport()))
            AddStack(p, WorldState.FutureTime(7.8f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ArchaicRockbreakerPairs)
            Stacks.Clear();
    }
}

class ArchaicRockbreakerLine(BossModule module) : Components.StandardAOEs(module, AID.ArchaicRockbreakerLine, 8, maxCasts: 8);

class ArchaicRockbreakerCombination(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shapeOut = new(12);
    private static readonly AOEShapeDonut _shapeIn = new(8, 20);
    private static readonly AOEShapeCone _shapeCleave = new(40, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _aoes.Take(1);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var p in SafeSpots())
            movementHints.Add(actor.Position, p, ArenaColor.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (inOutShape, offset) = (AID)spell.Action.ID switch
        {
            AID.FrontCombinationOut => (_shapeOut, 0.Degrees()),
            AID.FrontCombinationIn => (_shapeIn, 0.Degrees()),
            AID.RearCombinationOut => (_shapeOut, 180.Degrees()),
            AID.RearCombinationIn => (_shapeIn, 180.Degrees()),
            _ => ((AOEShape?)null, 0.Degrees())
        };
        if (inOutShape != null)
        {
            _aoes.Add(new(inOutShape, Module.PrimaryActor.Position, default, WorldState.FutureTime(6.9f)));
            _aoes.Add(new(_shapeCleave, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + offset, WorldState.FutureTime(10)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.InsideRoundhouseAOE:
                PopAOE();
                _aoes.Add(new(_shapeIn, Module.PrimaryActor.Position, default, WorldState.FutureTime(6)));
                break;
            case AID.OutsideRoundhouseAOE:
                PopAOE();
                _aoes.Add(new(_shapeOut, Module.PrimaryActor.Position, default, WorldState.FutureTime(6)));
                break;
            case AID.SwingingKickFrontAOE:
            case AID.SwingingKickRearAOE:
                PopAOE();
                break;
        }
    }

    private void PopAOE()
    {
        ++NumCasts;
        if (_aoes.Count > 0)
            _aoes.RemoveAt(0);
    }

    private IEnumerable<WPos> SafeSpots()
    {
        if (NumCasts == 0 && _aoes.Count > 0 && _aoes[0].Shape == _shapeOut && Module.FindComponent<ArchaicRockbreakerLine>() is var forbidden && forbidden?.NumCasts == 0)
        {
            var safespots = new ArcList(_aoes[0].Origin, _shapeOut.Radius + 0.25f);
            foreach (var f in forbidden.ActiveCasters)
                safespots.ForbidCircle(f.Position, ((AOEShapeCircle)forbidden.Shape).Radius);
            if (safespots.Forbidden.Segments.Count > 0)
            {
                foreach (var a in safespots.Allowed(default))
                {
                    var mid = ((a.min.Rad + a.max.Rad) * 0.5f).Radians();
                    yield return safespots.Center + safespots.Radius * mid.ToDirection();
                }
            }
        }
    }
}

class ArchaicDemolish(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ArchaicDemolish)
            AddStacks(Raid.WithoutSlot(true).Where(a => a.Role == Role.Healer), Module.CastFinishAt(spell, 1.2f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ArchaicDemolishAOE)
            Stacks.Clear();
    }
}
