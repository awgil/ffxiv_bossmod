namespace BossMod.Dawntrail.Savage.M09SVampFatale;

sealed class BlastBeat(BossModule module) : Components.GenericAOEs(module)
{
    class Vampette
    {
        public Actor Actor;
        public Angle Rotation;
        public DateTime Activation;
        public WPos EndPos;
    }

    private readonly List<Vampette> _vampettes = [];
    private readonly double _blastDelay = 3.5d;

    public bool IsActive => _vampettes.Count > 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_vampettes.Count == 0)
            return [];

        var vamps = CollectionsMarshal.AsSpan(_vampettes);
        var len = vamps.Length;
        List<AOEInstance> aoes = [];

        // delay displaying bat blast longer? for easier debuff popping
        for (var i = 0; i < len; i++)
        {
            if (vamps[i].Activation == default)
                continue;

            if (vamps[i].Activation.AddSeconds(-1 * _blastDelay) <= WorldState.CurrentTime)
            {
                //aoes.Add(new(new AOEShapeCircle(8f), vamps[i].EndPos, activation: vamps[i].Activation));
                aoes.Add(new(new AOEShapeCircle(8.25f), vamps[i].EndPos, activation: vamps[i].Activation));
            }
        }

        return CollectionsMarshal.AsSpan([.. aoes]);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.VampetteFatale)
        {
            if (id == 0x11D1)
            {
                _vampettes.Add(new() { Actor = actor, Rotation = actor.Rotation });
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.VampetteDistance)
        {
            var vamps = CollectionsMarshal.AsSpan(_vampettes);
            var len = vamps.Length;

            for (var i = 0; i < len; i++)
            {
                // predicted explosion on at least 1st bats are incorrect, still clipped despite showing safe on radar; 90 degrees too far?
                // between replays it appears to be anywhere from 88 to 92 degrees; mal says maybe due calculated by server ticks not hard rotation
                // either increase radius slightly above 8f, or remove predicted AOE and replace with actual on cast start 
                if (vamps[i].Actor.InstanceID == actor.InstanceID)
                {
                    var vamp = vamps[i];
                    var direction = actor.Rotation.ToDirection().OrthoL().Dot(actor.DirectionTo(Arena.Center)) > 0 ? 1 : -1;
                    var rotation = direction * 90.Degrees();
                    var vampdiff = actor.Position - Arena.Center;
                    var final = vampdiff.Rotate(rotation);

                    vamp.EndPos = Arena.Center + final;
                    vamp.Activation = status.Extra == 0x25 ? WorldState.FutureTime(4.6d) : status.Extra == 0x33 ? WorldState.FutureTime(8.1d) : WorldState.FutureTime(11.6d);
                }
            }

            _vampettes.Sort((a, b) => a.Activation < b.Activation ? 1 : -1);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.BlastBeatVampette)
        {
            var vamps = CollectionsMarshal.AsSpan(_vampettes);
            var len = vamps.Length;

            for (var i = 0; i < len; i++)
            {
                if (vamps[i].Actor.InstanceID == caster.InstanceID)
                {
                    NumCasts++;
                    _vampettes.RemoveAt(i);
                    return;
                }
            }
        }
    }
    /*
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.BlastBeatVampette)
        {
            var predictedVamps = CollectionsMarshal.AsSpan(_vampettes);
            var len = predictedVamps.Length;
            var index = -1;

            for (var i = 0; i < len; i++)
            {
                var v = predictedVamps[i];
                if (caster.InstanceID == v.Actor.InstanceID)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
                return;

            _vampettes.RemoveAt(index);
            var vamp = new Vampette() { Actor = caster, Rotation = default, Activation = Module.CastFinishAt(spell), EndPos = spell.LocXZ };
            _vampettes.Add(vamp);
        }
    }
    */
}

// show spreads based on proximity to ring?
sealed class CurseOfTheBombpyre(BossModule module) : Components.GenericStackSpread(module)
{
    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.CurseOfTheBombpyre)
            Spreads.Add(new(actor, 8f));
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.CurseOfTheBombpyre)
        {
            var count = Spreads.Count;
            var spreads = CollectionsMarshal.AsSpan(Spreads);
            for (var i = 0; i < count; ++i)
            {
                if (spreads[i].Target.InstanceID == actor.InstanceID)
                {
                    Spreads.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class BombpyreRing(BossModule module) : BossComponent(module)
{
    private DateTime _start;
    private float _radius;
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.VampStompRing)
        {
            _start = WorldState.CurrentTime;
        }
    }

    public override void Update()
    {
        if (_start == default)
            return;

        _radius = (float)((WorldState.CurrentTime - _start).TotalMilliseconds / 500);
        if (_radius > 56.6f)
        {
            _start = default;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_start == default)
            return;

        Arena.ZoneCircleOutline(Arena.Center, _radius, Colors.Danger);
    }
}
