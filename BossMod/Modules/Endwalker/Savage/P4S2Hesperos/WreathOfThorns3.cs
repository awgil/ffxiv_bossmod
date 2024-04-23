namespace BossMod.Endwalker.Savage.P4S2Hesperos;

// state related to act 3 wreath of thorns
// note: there should be four tethered helpers on activation
class WreathOfThorns3(BossModule module) : BossComponent(module)
{
    public enum State { RangedTowers, Knockback, MeleeTowers, Done }

    public State CurState { get; private set; } = State.RangedTowers;
    public int NumJumps { get; private set; }
    public int NumCones { get; private set; }
    private readonly AOEShapeCone _coneAOE = new(50, 45.Degrees()); // not sure about half-width...
    private readonly List<Actor> _relevantHelpers = []; // 4 towers -> knockback -> 4 towers
    private Actor? _jumpTarget; // either predicted (if jump is imminent) or last actual (if cones are imminent)
    private BitMask _coneTargets;
    private BitMask _playersInAOE;

    private IEnumerable<Actor> RangedTowers => _relevantHelpers.Take(4);
    //private IEnumerable<Actor> _knockbackThorn => _relevantHelpers.Skip(4).Take(1);
    private IEnumerable<Actor> MeleeTowers => _relevantHelpers.Skip(5);

    private const float _jumpAOERadius = 10;

    public override void Update()
    {
        _coneTargets = _playersInAOE = new();
        if (NumCones == NumJumps)
        {
            _jumpTarget = Raid.WithoutSlot().SortedByRange(Module.PrimaryActor.Position).LastOrDefault();
            _playersInAOE = _jumpTarget != null ? Raid.WithSlot().InRadiusExcluding(_jumpTarget, _jumpAOERadius).Mask() : new();
        }
        else
        {
            foreach ((int i, var player) in Raid.WithSlot().SortedByRange(Module.PrimaryActor.Position).Take(3))
            {
                _coneTargets.Set(i);
                if (player.Position != Module.PrimaryActor.Position)
                {
                    var direction = (player.Position - Module.PrimaryActor.Position).Normalized();
                    _playersInAOE |= Raid.WithSlot().Exclude(i).WhereActor(p => p.Position.InCone(Module.PrimaryActor.Position, direction, _coneAOE.HalfAngle)).Mask();
                }
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurState != State.Done)
        {
            // TODO: consider raid comps with 3+ melee or ranged...
            bool shouldSoakTower = CurState == State.RangedTowers ? actor.Role is Role.Ranged or Role.Healer : actor.Role is Role.Melee or Role.Tank;
            var soakedTower = (CurState == State.RangedTowers ? RangedTowers : MeleeTowers).InRadius(actor.Position, P4S2.WreathTowerRadius).FirstOrDefault();
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

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_coneTargets.Any())
        {
            foreach ((_, var player) in Raid.WithSlot().IncludedInMask(_coneTargets))
            {
                _coneAOE.Draw(Arena, Module.PrimaryActor.Position, Angle.FromDirection(player.Position - Module.PrimaryActor.Position));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach ((int i, var player) in Raid.WithSlot())
            Arena.Actor(player, _playersInAOE[i] ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);

        if (CurState != State.Done)
        {
            foreach (var tower in (CurState == State.RangedTowers ? RangedTowers : MeleeTowers))
                Arena.AddCircle(tower.Position, P4S2.WreathTowerRadius, ArenaColor.Safe);
        }

        if (NumCones != NumJumps)
        {
            foreach ((_, var player) in Raid.WithSlot().IncludedInMask(_coneTargets))
                Arena.Actor(player, ArenaColor.Danger);
            Arena.Actor(_jumpTarget, ArenaColor.Vulnerable);
        }
        else if (_jumpTarget != null)
        {
            Arena.Actor(_jumpTarget, ArenaColor.Danger);
            Arena.AddCircle(_jumpTarget.Position, _jumpAOERadius, ArenaColor.Danger);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.Helper && tether.ID == (uint)TetherID.WreathOfThorns)
            _relevantHelpers.Add(source);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (CurState == State.RangedTowers && (AID)spell.Action.ID == AID.AkanthaiExplodeTower)
            CurState = State.Knockback;
        else if (CurState == State.Knockback && (AID)spell.Action.ID == AID.AkanthaiExplodeKnockback)
            CurState = State.MeleeTowers;
        else if (CurState == State.MeleeTowers && (AID)spell.Action.ID == AID.AkanthaiExplodeTower)
            CurState = State.Done;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.KothornosKickJump:
                ++NumJumps;
                _jumpTarget = WorldState.Actors.Find(spell.MainTargetID);
                break;
            case AID.KothornosQuake1:
            case AID.KothornosQuake2:
                ++NumCones;
                break;
        }
    }
}
