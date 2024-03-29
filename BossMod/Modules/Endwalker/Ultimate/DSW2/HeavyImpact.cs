namespace BossMod.Endwalker.Ultimate.DSW2;

// used by two trio mechanics, in p2 and in p5
class HeavyImpact : Components.GenericAOEs
{
    private AOEInstance? _aoe;
    private float _activationDelay;

    private static readonly float _impactRadiusIncrement = 6;

    public bool Active => _aoe != null;

    public HeavyImpact(float activationDelay)
    {
        _activationDelay = activationDelay;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
    {
        if (id == 0x1E43 && (OID)actor.OID == OID.SerGuerrique)
        {
            _aoe = new(new AOEShapeCircle(_impactRadiusIncrement), actor.Position, default, module.WorldState.CurrentTime.AddSeconds(_activationDelay));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.HeavyImpactHit1 or AID.HeavyImpactHit2 or AID.HeavyImpactHit3 or AID.HeavyImpactHit4 or AID.HeavyImpactHit5)
        {
            if (++NumCasts < 5)
            {
                var inner = _impactRadiusIncrement * NumCasts;
                _aoe = new(new AOEShapeDonut(inner, inner + _impactRadiusIncrement), caster.Position, default, module.WorldState.CurrentTime.AddSeconds(1.9f));
            }
            else
            {
                _aoe = null;
            }
        }
    }
}
