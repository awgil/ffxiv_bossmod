namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to act 3 wreath of thorns
// note: there should be four tethered helpers on activation
class WreathOfThorns3 : BossComponent
{
    public enum State { RangedTowers, Knockback, MeleeTowers, Done }

    public State CurState { get; private set; } = State.RangedTowers;
    public int NumJumps { get; private set; } = 0;
    public int NumCones { get; private set; } = 0;
    private AOEShapeCone _coneAOE = new(50, 45.Degrees()); // not sure about half-width...
    private List<Actor> _relevantHelpers = new(); // 4 towers -> knockback -> 4 towers
    private Actor? _jumpTarget = null; // either predicted (if jump is imminent) or last actual (if cones are imminent)
    private BitMask _coneTargets;
    private BitMask _playersInAOE;

    private IEnumerable<Actor> _rangedTowers => _relevantHelpers.Take(4);
    private IEnumerable<Actor> _knockbackThorn => _relevantHelpers.Skip(4).Take(1);
    private IEnumerable<Actor> _meleeTowers => _relevantHelpers.Skip(5);

    private static readonly float _jumpAOERadius = 10;

    public override void Update(BossModule module)
    {
        _coneTargets = _playersInAOE = new();
        if (NumCones == NumJumps)
        {
            _jumpTarget = module.Raid.WithoutSlot().SortedByRange(module.PrimaryActor.Position).LastOrDefault();
            _playersInAOE = _jumpTarget != null ? module.Raid.WithSlot().InRadiusExcluding(_jumpTarget, _jumpAOERadius).Mask() : new();
        }
        else
        {
            foreach ((int i, var player) in module.Raid.WithSlot().SortedByRange(module.PrimaryActor.Position).Take(3))
            {
                _coneTargets.Set(i);
                if (player.Position != module.PrimaryActor.Position)
                {
                    var direction = (player.Position - module.PrimaryActor.Position).Normalized();
                    _playersInAOE |= module.Raid.WithSlot().Exclude(i).WhereActor(p => p.Position.InCone(module.PrimaryActor.Position, direction, _coneAOE.HalfAngle)).Mask();
                }
            }
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (CurState != State.Done)
        {
            // TODO: consider raid comps with 3+ melee or ranged...
            bool shouldSoakTower = CurState == State.RangedTowers
                ? (actor.Role == Role.Ranged || actor.Role == Role.Healer)
                : (actor.Role == Role.Melee || actor.Role == Role.Tank);
            var soakedTower = (CurState == State.RangedTowers ? _rangedTowers : _meleeTowers).InRadius(actor.Position, P4S2.WreathTowerRadius).FirstOrDefault();
            if (shouldSoakTower)
            {
                hints.Add("Soak the tower!", soakedTower == null);
            }
            else if (soakedTower != null)
            {
                hints.Add("GTFO from tower!");
            }
        }

        if (_playersInAOE[slot])
        {
            hints.Add("GTFO from aoe!");
        }
        if (NumCones == NumJumps && actor == _jumpTarget && _playersInAOE.Any())
        {
            hints.Add("GTFO from raid!");
        }
        if (NumCones != NumJumps && actor == _jumpTarget && _coneTargets[slot])
        {
            hints.Add("GTFO from boss!");
        }
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_coneTargets.Any())
        {
            foreach ((_, var player) in module.Raid.WithSlot().IncludedInMask(_coneTargets))
            {
                _coneAOE.Draw(arena, module.PrimaryActor.Position, Angle.FromDirection(player.Position - module.PrimaryActor.Position));
            }
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach ((int i, var player) in module.Raid.WithSlot())
            arena.Actor(player, _playersInAOE[i] ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);

        if (CurState != State.Done)
        {
            foreach (var tower in (CurState == State.RangedTowers ? _rangedTowers : _meleeTowers))
                arena.AddCircle(tower.Position, P4S2.WreathTowerRadius, ArenaColor.Safe);
        }

        if (NumCones != NumJumps)
        {
            foreach ((_, var player) in module.Raid.WithSlot().IncludedInMask(_coneTargets))
                arena.Actor(player, ArenaColor.Danger);
            arena.Actor(_jumpTarget, ArenaColor.Vulnerable);
        }
        else if (_jumpTarget != null)
        {
            arena.Actor(_jumpTarget, ArenaColor.Danger);
            arena.AddCircle(_jumpTarget.Position, _jumpAOERadius, ArenaColor.Danger);
        }
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Helper && tether.ID == (uint)TetherID.WreathOfThorns)
            _relevantHelpers.Add(source);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (CurState == State.RangedTowers && (AID)spell.Action.ID == AID.AkanthaiExplodeTower)
            CurState = State.Knockback;
        else if (CurState == State.Knockback && (AID)spell.Action.ID == AID.AkanthaiExplodeKnockback)
            CurState = State.MeleeTowers;
        else if (CurState == State.MeleeTowers && (AID)spell.Action.ID == AID.AkanthaiExplodeTower)
            CurState = State.Done;
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.KothornosKickJump:
                ++NumJumps;
                _jumpTarget = module.WorldState.Actors.Find(spell.MainTargetID);
                break;
            case AID.KothornosQuake1:
            case AID.KothornosQuake2:
                ++NumCones;
                break;
        }
    }
}
