namespace BossMod.Components;

// generic tank-swap component for multi-hit tankbusters, with optional aoe
// assume that target of the first hit is locked when mechanic starts, then subsequent targets are selected based on who the boss targets
// TODO: this version assumes that boss cast and first-hit are potentially from different actors; the target lock could also be things like icons, etc - generalize more...
public class TankSwap(BossModule module, Enum bossCast, Enum firstCast, Enum subsequentHit, float timeBetweenHits, AOEShape? shape, bool centerAtTarget) : GenericBaitAway(module, centerAtTarget: centerAtTarget)
{
    protected Actor? _source;
    protected ulong _prevTarget; // before first cast, this is the target of the first hit
    protected DateTime _activation;

    public readonly ActionID BossCast = ActionID.MakeSpell(bossCast);
    public readonly ActionID FirstCast = ActionID.MakeSpell(firstCast);
    public readonly ActionID SubsequentHit = ActionID.MakeSpell(subsequentHit);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source != null && shape != null)
        {
            var target = WorldState.Actors.Find(NumCasts == 0 ? _prevTarget : _source.TargetID);
            if (target != null)
            {
                CurrentBaits.Add(new(Module.PrimaryActor, target, shape, _activation));
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_source?.TargetID == _prevTarget && actor.Role == Role.Tank)
            hints.Add(_prevTarget != actor.InstanceID ? "Taunt!" : "Pass aggro!");
        base.AddHints(slot, actor, hints);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == BossCast)
        {
            _source = caster;
        }
        if (spell.Action == FirstCast)
        {
            NumCasts = 0;
            _prevTarget = spell.TargetID;
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == FirstCast || spell.Action == SubsequentHit)
        {
            ++NumCasts;
            _prevTarget = spell.MainTargetID == caster.InstanceID && spell.Targets.Count != 0 ? spell.Targets[0].ID : spell.MainTargetID;
            _activation = Module.WorldState.FutureTime(timeBetweenHits);
        }
    }
}
