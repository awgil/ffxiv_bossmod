namespace BossMod.Dawntrail.Ultimate.FRU;

class P1PowderMarkTrail(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.BurnMark), centerAtTarget: true)
{
    private Actor? _target;
    private Actor? _closest;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(10);

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
        base.AddHints(slot, actor, hints);
        if (_closest != null && _closest.Role != Role.Tank)
        {
            if (actor == _closest)
                hints.Add("GTFO from tank!");
            else if (actor == _target || actor.Role == Role.Tank)
                hints.Add("Get closer to co-tank!");
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
