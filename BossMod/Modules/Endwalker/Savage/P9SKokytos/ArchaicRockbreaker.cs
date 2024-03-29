namespace BossMod.Endwalker.Savage.P9SKokytos;

class ArchaicRockbreakerCenter : Components.LocationTargetedAOEs
{
    public ArchaicRockbreakerCenter() : base(ActionID.MakeSpell(AID.ArchaicRockbreakerCenter), 6) { }
}

class ArchaicRockbreakerShockwave : Components.Knockback
{
    private Uplift? _uplift;
    private DateTime _activation;

    public ArchaicRockbreakerShockwave() : base(ActionID.MakeSpell(AID.ArchaicRockbreakerShockwave), true) { }

    public override void Init(BossModule module)
    {
        _uplift = module.FindComponent<Uplift>();
        _activation = module.WorldState.CurrentTime.AddSeconds(6.5f);
    }

    public override IEnumerable<Source> Sources(BossModule module, int slot, Actor actor)
    {
        float distance = 21;
        if (_uplift?.WallDirection != null)
        {
            var offset = actor.Position - module.Bounds.Center;
            var dot = Math.Abs(_uplift.WallDirection.Value.ToDirection().Dot(offset.Normalized()));
            bool againstWall = dot > 0.9238795f || dot < 0.3826834f;
            if (againstWall)
                distance = module.Bounds.HalfSize - offset.Length() - 0.5f;
        }
        yield return new(module.Bounds.Center, distance, _activation);
    }
}

class ArchaicRockbreakerPairs : Components.UniformStackSpread
{
    public ArchaicRockbreakerPairs() : base(6, 0, 2) { }

    public override void Init(BossModule module)
    {
        foreach (var p in module.Raid.WithoutSlot(true).Where(p => p.Class.IsSupport()))
            AddStack(p, module.WorldState.CurrentTime.AddSeconds(7.8f));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ArchaicRockbreakerPairs)
            Stacks.Clear();
    }
}

class ArchaicRockbreakerLine : Components.LocationTargetedAOEs
{
    public ArchaicRockbreakerLine() : base(ActionID.MakeSpell(AID.ArchaicRockbreakerLine), 8, maxCasts: 8) { }
}

class ArchaicRockbreakerCombination : Components.GenericAOEs
{
    private List<AOEInstance> _aoes = new();

    private static readonly AOEShapeCircle _shapeOut = new(12);
    private static readonly AOEShapeDonut _shapeIn = new(8, 20);
    private static readonly AOEShapeCone _shapeCleave = new(40, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        return _aoes.Take(1);
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        base.AddHints(module, slot, actor, hints, movementHints);
        if (movementHints != null)
            foreach (var p in SafeSpots(module))
                movementHints.Add(actor.Position, p, ArenaColor.Safe);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
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
            _aoes.Add(new(inOutShape, module.PrimaryActor.Position, default, module.WorldState.CurrentTime.AddSeconds(6.9f)));
            _aoes.Add(new(_shapeCleave, module.PrimaryActor.Position, module.PrimaryActor.Rotation + offset, module.WorldState.CurrentTime.AddSeconds(10)));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.InsideRoundhouseAOE:
                PopAOE();
                _aoes.Add(new(_shapeIn, module.PrimaryActor.Position, default, module.WorldState.CurrentTime.AddSeconds(6)));
                break;
            case AID.OutsideRoundhouseAOE:
                PopAOE();
                _aoes.Add(new(_shapeOut, module.PrimaryActor.Position, default, module.WorldState.CurrentTime.AddSeconds(6)));
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

    private IEnumerable<WPos> SafeSpots(BossModule module)
    {
        if (NumCasts == 0 && _aoes.Count > 0 && _aoes[0].Shape == _shapeOut && module.FindComponent<ArchaicRockbreakerLine>() is var forbidden && forbidden?.NumCasts == 0)
        {
            var safespots = new ArcList(_aoes[0].Origin, _shapeOut.Radius + 0.25f);
            foreach (var f in forbidden.ActiveCasters)
                safespots.ForbidCircle(f.Position, forbidden.Shape.Radius);
            if (safespots.Forbidden.Segments.Count > 0)
            {
                foreach (var a in safespots.Allowed(default))
                {
                    var mid = ((a.Item1.Rad + a.Item2.Rad) * 0.5f).Radians();
                    yield return safespots.Center + safespots.Radius * mid.ToDirection();
                }
            }
        }
    }
}

class ArchaicDemolish : Components.UniformStackSpread
{
    public ArchaicDemolish() : base(6, 0, 4) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ArchaicDemolish)
            AddStacks(module.Raid.WithoutSlot(true).Where(a => a.Role == Role.Healer), spell.NPCFinishAt.AddSeconds(1.2f));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ArchaicDemolishAOE)
            Stacks.Clear();
    }
}
