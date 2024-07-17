namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

// TODO: generalize to 'aoe tankswap tankbuster'
class BitterWhirlwind(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private Actor? _source;
    private ulong _prevTarget;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(5);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source != null)
        {
            var target = WorldState.Actors.Find(_source.CastInfo?.TargetID ?? Module.PrimaryActor.TargetID);
            if (target != null)
            {
                CurrentBaits.Add(new(Module.PrimaryActor, target, _shape, _activation));
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Any(b => b.Target.InstanceID == _prevTarget) && actor.Role == Role.Tank)
            hints.Add(_prevTarget != actor.InstanceID ? "Taunt!" : "Pass aggro!");
        base.AddHints(slot, actor, hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BitterWhirlwindAOEFirst)
        {
            _source = caster;
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BitterWhirlwindAOEFirst or AID.BitterWhirlwindAOERest)
        {
            ++NumCasts;
            _prevTarget = spell.MainTargetID;
        }
    }
}
