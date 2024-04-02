namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

// state related to ania mechanic
class Ania : BossComponent
{
    private Actor? _target;

    private static readonly float _aoeRadius = 3;

    public bool Done => _target == null;

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_target == null)
            return;

        if (actor == _target)
        {
            if (module.Raid.WithoutSlot().InRadiusExcluding(actor, _aoeRadius).Any())
                hints.Add("GTFO from raid!");
            if (module.PrimaryActor.TargetID == _target.InstanceID)
                hints.Add("Pass aggro!");
        }
        else
        {
            if (actor.Position.InCircle(_target.Position, _aoeRadius))
                hints.Add("GTFO from tank!");
            if (actor.Role == Role.Tank && module.PrimaryActor.TargetID != actor.InstanceID)
                hints.Add("Taunt!");
        }
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (_target == null)
            return;

        arena.AddCircle(_target.Position, _aoeRadius, ArenaColor.Danger);
        if (pc == _target)
        {
            foreach (var a in module.Raid.WithoutSlot().Exclude(pc))
                arena.Actor(a, a.Position.InCircle(_target.Position, _aoeRadius) ? ArenaColor.PlayerInteresting : ArenaColor.PlayerGeneric);
        }
        else
        {
            arena.Actor(_target, ArenaColor.Danger);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AniaAOE)
            _target = module.WorldState.Actors.Find(spell.TargetID);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AniaAOE)
            _target = null;
    }
}
