namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

class BoundsOfSinInOut(BossModule module) : Components.GenericAOEs(module)
{
    private bool _active;
    private bool _donut;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active)
        {
            if (_donut)
                yield return new(new AOEShapeDonut(8, 30), Arena.Center);
            else
                yield return new(new AOEShapeCircle(8), Arena.Center);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (!_active && (AID)spell.Action.ID == AID.BoundsOfSinIcicleDrop)
        {
            var dir = caster.DirectionTo(Arena.Center);
            _donut = dir.Dot(caster.Rotation.ToDirection()) < 0;
            _active = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.BoundsOfSinJailInside or AID.BoundsOfSinJailOutside)
        {
            NumCasts++;
            _active = false;
        }
    }
}

class BoundsOfSinIcicle(BossModule module) : Components.StandardAOEs(module, AID.BoundsOfSinIcicleDrop, 3, maxCasts: 10);

class BoundsOfSinBind(BossModule module) : Components.CastCounter(module, AID.BoundsOfSinBind);
