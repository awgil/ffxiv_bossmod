namespace BossMod.Dawntrail.Ultimate.FRU;

class P1PowderMarkTrail(BossModule module) : Components.GenericBaitAway(module, AID.BurnMark, centerAtTarget: true)
{
    public bool AllowTankStacking;
    private Actor? _target;
    private Actor? _closest;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(10);
    private const float _avoidBaitDistance = 13;

    public override void Update()
    {
        CurrentBaits.Clear();
        _closest = _target != null ? Raid.WithoutSlot().Exclude(_target).Closest(_target.Position) : null;
        if (_target != null)
            CurrentBaits.Add(new(Module.PrimaryActor, _target, _shape, _activation));
        if (_closest != null)
            CurrentBaits.Add(new(Module.PrimaryActor, _closest, _shape, _activation));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target == null || _closest == null)
            return; // no baits active

        if (actor.Role == Role.Tank)
        {
            if (actor != _closest && actor != _target)
                hints.Add("Get closer to co-tank!");
            else if (Raid.WithoutSlot().InRadiusExcluding(actor, _shape.Radius).Any(p => !AllowTankStacking || p.Role != Role.Tank))
                hints.Add("Bait away from raid!");
        }
        else if (actor == _closest || actor.Position.InCircle(_target.Position, _shape.Radius) || actor.Position.InCircle(_closest.Position, _shape.Radius))
        {
            hints.Add("GTFO from tanks!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (WorldState.FutureTime(2) < _activation)
            return; // start micro adjusts only when activation is imminent; before that we have other components providing coarse positioning
        var isTank = actor.Role == Role.Tank;
        foreach (var p in Raid.WithoutSlot().Exclude(actor))
        {
            var otherTank = p.Role == Role.Tank;
            if (isTank && otherTank)
            {
                // tanks should stay near but not too near other tank
                if (!AllowTankStacking)
                    hints.AddForbiddenZone(_shape.CheckFn(p.Position, default), _activation);
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(p.Position, _avoidBaitDistance), _activation);
            }
            else if (isTank != otherTank)
            {
                // tanks should avoid non-tanks and vice versa
                hints.AddForbiddenZone(ShapeContains.Circle(p.Position, _avoidBaitDistance), _activation);
            }
            // else: non-tanks don't care about non-tanks
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.PowderMarkTrail)
        {
            _target = actor;
            _activation = status.ExpireAt;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _target = null;
        }
    }
}
