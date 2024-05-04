namespace BossMod.Endwalker.Savage.P2SHippokampos;

// state related to coherence mechanic
// TODO: i'm not 100% sure how exactly it selects target for aoe ray, I assume it is closest player except tether target?..
class Coherence(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.CoherenceRay))
{
    private Actor? _tetherTarget;
    private Actor? _rayTarget;
    private readonly AOEShapeRect _rayShape = new(50, 3);
    private Angle _rayDirection;
    private BitMask _inRay;

    private const float _aoeRadius = 10; // not sure about this - actual range is 60, but it has some sort of falloff? i have very few data points < 15

    public override void Update()
    {
        _inRay.Reset();
        _rayTarget = Raid.WithoutSlot().Exclude(_tetherTarget).Closest(Module.PrimaryActor.Position);
        if (_rayTarget != null)
        {
            _rayDirection = Angle.FromDirection(_rayTarget.Position - Module.PrimaryActor.Position);
            _inRay = Raid.WithSlot().InShape(_rayShape, Module.PrimaryActor.Position, _rayDirection).Mask();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor == _tetherTarget)
        {
            if (actor.Role != Role.Tank)
            {
                hints.Add("Pass tether to tank!");
            }
            else if (Raid.WithoutSlot().InRadiusExcluding(actor, _aoeRadius).Any())
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

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_rayTarget != null)
            _rayShape.Draw(Arena, Module.PrimaryActor.Position, _rayDirection);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // TODO: i'm not sure what are the exact mechanics - flare is probably distance-based, and ray is probably shared damage cast at closest target?..
        var head = Module.Enemies(OID.CataractHead).FirstOrDefault();
        foreach ((int i, var player) in Raid.WithSlot())
        {
            if (head?.Tether.Target == player.InstanceID)
            {
                Arena.AddLine(player.Position, Module.PrimaryActor.Position, ArenaColor.Danger);
                Arena.Actor(player, ArenaColor.Danger);
                Arena.AddCircle(player.Position, _aoeRadius, ArenaColor.Danger);
            }
            else if (player == _rayTarget)
            {
                Arena.Actor(player, ArenaColor.Danger);
            }
            else if (player != _tetherTarget)
            {
                Arena.Actor(player, _inRay[i] ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Coherence)
            _tetherTarget = WorldState.Actors.Find(tether.Target);
    }
}
