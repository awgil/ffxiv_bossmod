namespace BossMod.Dawntrail.Extreme.Ex6GuardianArkveld;

class ClamorousJump(BossModule module) : Components.CastCounterMulti(module, [AID.ClamorousJump1, AID.ClamorousJump2]);

class ClamorousCleave(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _predicted;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_predicted);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var angle = (spell.TargetXZ - caster.Position).ToAngle();
        switch ((AID)spell.Action.ID)
        {
            case AID.ClamorousJump1:
                _predicted = new(new AOEShapeCone(40, 90.Degrees()), spell.TargetXZ, angle + 90.Degrees(), WorldState.FutureTime(2));
                break;
            case AID.ClamorousJump2:
                _predicted = new(new AOEShapeCone(40, 90.Degrees()), spell.TargetXZ, angle - 90.Degrees(), WorldState.FutureTime(2));
                break;
            case AID.ClamorousCleave2:
            case AID.ClamorousCleave1:
                NumCasts++;
                _predicted = null;
                break;
        }
    }
}

class ClamorousBait(BossModule module) : Components.CastCounterMulti(module, [AID.ClamorousCleave1, AID.ClamorousCleave2])
{
    private readonly int[] _order = Utils.MakeArray(8, -1);
    private readonly Actor?[] _targets = new Actor?[8];
    private int _side;
    private int _nextBait = -1;
    private WPos _source;
    private DateTime _nextJump;

    // TODO: figure out actual tether length
    public const int TetherLength = 22;

    private DateTime NextCleave => _nextJump.AddSeconds(2);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var order = (int)iconID - (int)IconID.LimitCut1;
        if (order is >= 0 and < 8 && Raid.TryFindSlot(actor, out var slot))
        {
            _order[slot] = order;
            _targets[order] = actor;
            if (_order.All(o => o >= 0))
                Activate();
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ClamorousChaseRightWing:
                _side = 1;
                _nextJump = Module.CastFinishAt(spell, 0.2f);
                Activate();
                break;
            case AID.ClamorousChaseLeftWing:
                _side = -1;
                _nextJump = Module.CastFinishAt(spell, 0.2f);
                Activate();
                break;
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (GetNextBait() is var (cleaveSrc, rotation))
        {
            if (cleaveSrc == pc)
            {
                Arena.AddCone(cleaveSrc.Position, 40, rotation, 90.Degrees(), ArenaColor.Danger);
                Arena.AddCircle(cleaveSrc.Position, 6, ArenaColor.Danger);
            }
            else
            {
                Arena.ZoneCone(cleaveSrc.Position, 0, 40, rotation, 90.Degrees(), ArenaColor.AOE);
                Arena.ZoneCircle(cleaveSrc.Position, 6, ArenaColor.AOE);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (WatchedActions.Contains(spell.Action))
            NumCasts++;

        if ((AID)spell.Action.ID is AID.ClamorousJump1 or AID.ClamorousJump2)
        {
            _nextBait++;
            _source = spell.TargetXZ;
            _nextJump = WorldState.FutureTime(3);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_nextBait >= 0 && _order[slot] >= _nextBait)
            hints.Add($"Order: {_order[slot] + 1}", false);

        if (GetNextBait() is var (cleaveSrc, rotation))
        {
            var hint = _nextBait == 0 ? "Get away from boss!" : "Get away from buddy!";
            if (cleaveSrc == actor)
            {
                hints.Add(hint, actor.Position.InCircle(_source, TetherLength));

                if (Raid.WithoutSlot().Exclude(actor).Any(a => !a.Position.AlmostEqual(_source, 0.5f) && a.Position.InCone(cleaveSrc.Position, rotation, 90.Degrees())))
                    hints.Add("Bait away from raid!");
            }
            else
            {
                if (_order[slot] == _nextBait + 1)
                    hints.Add("Get away from buddy!", actor.Position.InCircle(cleaveSrc.Position, TetherLength));

                if (actor.Position.InCircle(cleaveSrc.Position, 6))
                    hints.Add("GTFO from bait!");
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (GetNextBait() is var (cleaveSrc, rotation))
        {
            if (cleaveSrc == actor)
            {
                // TODO: do we need to add a hint to prevent cleaving the party...
                hints.AddForbiddenZone(ShapeContains.Circle(_source, TetherLength), _nextJump);
                foreach (var p in Raid.WithoutSlot().Exclude(actor))
                    hints.AddForbiddenZone(ShapeContains.Circle(p.Position, 6), _nextJump);
            }
            else
            {
                hints.AddForbiddenZone(ShapeContains.Circle(cleaveSrc.Position, 6), _nextJump);
                hints.AddForbiddenZone(ShapeContains.Cone(cleaveSrc.Position, 40, rotation, 90.Degrees()), NextCleave);

                if (_order[slot] == _nextBait + 1)
                    hints.AddForbiddenZone(ShapeContains.Circle(cleaveSrc.Position, TetherLength), NextCleave.AddSeconds(1));
            }
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        if (_nextBait >= 0)
        {
            if (_order[playerSlot] == _nextBait)
                return PlayerPriority.Danger;

            if (_order[playerSlot] == _nextBait + 1)
                return PlayerPriority.Interesting;
        }

        return base.CalcPriority(pcSlot, pc, playerSlot, player, ref customColor);
    }

    private void Activate()
    {
        if (_targets.All(t => t != null) && _side != 0)
        {
            _source = Module.PrimaryActor.Position;
            _nextBait = 0;
        }
    }

    private (Actor Target, Angle Angle)? GetNextBait()
    {
        return _targets.BoundSafeAt(_nextBait) is { } cleaveSrc
            ? (cleaveSrc, (_source - cleaveSrc.Position).ToAngle() + 90.Degrees() * _side)
            : null;
    }
}
