namespace BossMod.Endwalker.Savage.P2SHippokampos;

// state related to coherence mechanic
// TODO: i'm not 100% sure how exactly it selects target for aoe ray, I assume it is closest player except tether target?..
class Coherence : Components.CastCounter
{
    private Actor? _tetherTarget;
    private Actor? _rayTarget;
    private AOEShapeRect _rayShape = new(50, 3);
    private BitMask _inRay;

    private static readonly float _aoeRadius = 10; // not sure about this - actual range is 60, but it has some sort of falloff? i have very few data points < 15

    public Coherence() : base(ActionID.MakeSpell(AID.CoherenceRay)) { }

    public override void Update(BossModule module)
    {
        _inRay.Reset();
        _rayTarget = module.Raid.WithoutSlot().Exclude(_tetherTarget).Closest(module.PrimaryActor.Position);
        if (_rayTarget != null)
        {
            _rayShape.DirectionOffset = Angle.FromDirection(_rayTarget.Position - module.PrimaryActor.Position);
            _inRay = module.Raid.WithSlot().InShape(_rayShape, module.PrimaryActor.Position, 0.Degrees()).Mask();
        }
    }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (actor == _tetherTarget)
        {
            if (actor.Role != Role.Tank)
            {
                hints.Add("Pass tether to tank!");
            }
            else if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _aoeRadius).Any())
            {
                hints.Add("GTFO from raid!");
            }
        }
        else if (actor == _rayTarget)
        {
            if (actor.Role != Role.Tank)
            {
                hints.Add("Go behind tank!");
            }
            else if (_inRay.NumSetBits() < 7)
            {
                hints.Add("Make sure ray hits everyone!");
            }
        }
        else
        {
            if (actor.Role == Role.Tank)
            {
                hints.Add("Go in front of raid!");
            }
            else if (!_inRay[slot])
            {
                hints.Add("Go behind tank!");
            }
        }
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_rayTarget != null)
            _rayShape.Draw(arena, module.PrimaryActor.Position, 0.Degrees());
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        // TODO: i'm not sure what are the exact mechanics - flare is probably distance-based, and ray is probably shared damage cast at closest target?..
        var head = module.Enemies(OID.CataractHead).FirstOrDefault();
        foreach ((int i, var player) in module.Raid.WithSlot())
        {
            if (head?.Tether.Target == player.InstanceID)
            {
                arena.AddLine(player.Position, module.PrimaryActor.Position, ArenaColor.Danger);
                arena.Actor(player, ArenaColor.Danger);
                arena.AddCircle(player.Position, _aoeRadius, ArenaColor.Danger);
            }
            else if (player == _rayTarget)
            {
                arena.Actor(player, ArenaColor.Danger);
            }
            else if (player != _tetherTarget)
            {
                arena.Actor(player, _inRay[i] ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
            }
        }
    }

    public override void OnTethered(BossModule module, Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Coherence)
            _tetherTarget = module.WorldState.Actors.Find(tether.Target);
    }
}
