namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

// state related to ania mechanic
class Ania(BossModule module) : BossComponent(module)
{
    private Actor? _target;

    private const float _aoeRadius = 3;

    public bool Done => _target == null;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target == null)
            return;

        if (actor == _target)
        {
            if (Raid.WithoutSlot(false, true, true).InRadiusExcluding(actor, _aoeRadius).Any())
                hints.Add("GTFO from raid!");
            if (Module.PrimaryActor.TargetID == _target.InstanceID)
                hints.Add("Pass aggro!");
        }
        else
        {
            if (actor.Position.InCircle(_target.Position, _aoeRadius))
                hints.Add("GTFO from tank!");
            if (actor.Role == Role.Tank && Module.PrimaryActor.TargetID != actor.InstanceID)
                hints.Add("Taunt!");
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_target == null)
            return;

        Arena.ZoneCircleOutline(_target.Position, _aoeRadius, Colors.Danger);
        if (pc == _target)
        {
            foreach (var a in Raid.WithoutSlot(false, true, true).Exclude(pc))
                Arena.Actor(a, a.Position.InCircle(_target.Position, _aoeRadius) ? Colors.PlayerInteresting : Colors.PlayerGeneric);
        }
        else
        {
            Arena.Actor(_target, Colors.Danger);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AniaAOE)
            _target = WorldState.Actors.Find(spell.TargetID);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AniaAOE)
            _target = null;
    }
}
