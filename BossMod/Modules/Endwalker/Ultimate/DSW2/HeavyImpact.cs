namespace BossMod.Endwalker.Ultimate.DSW2;

// used by two trio mechanics, in p2 and in p5
class HeavyImpact(BossModule module, float activationDelay) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;
    private readonly float _activationDelay = activationDelay;

    private const float _impactRadiusIncrement = 6;

    public bool Active => _aoe != null;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E43 && (OID)actor.OID == OID.SerGuerrique)
        {
            _aoe = new(new AOEShapeCircle(_impactRadiusIncrement), actor.Position, default, WorldState.FutureTime(_activationDelay));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HeavyImpactHit1 or AID.HeavyImpactHit2 or AID.HeavyImpactHit3 or AID.HeavyImpactHit4 or AID.HeavyImpactHit5)
        {
            if (++NumCasts < 5)
            {
                var inner = _impactRadiusIncrement * NumCasts;
                _aoe = new(new AOEShapeDonut(inner, inner + _impactRadiusIncrement), caster.Position, default, WorldState.FutureTime(1.9f));
            }
            else
            {
                _aoe = null;
            }
        }
    }
}
