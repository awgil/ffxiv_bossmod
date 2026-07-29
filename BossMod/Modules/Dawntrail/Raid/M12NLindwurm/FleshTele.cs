namespace BossMod.Dawntrail.Raid.M12NLindwurm;

sealed class FleshTele(BossModule module) : Components.GenericKnockback(module)
{
    private readonly RavenousReach _reach = module.FindComponent<RavenousReach>()!;
    private readonly Burst _burst = module.FindComponent<Burst>()!;
    private DateTime _activation;
    private BitMask forwardKb;
    private BitMask backwardKb;

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (forwardKb[slot] || backwardKb[slot])
        {
            var rot = actor.Rotation;
            return new Knockback[1] { new(actor.Position, 15f, _activation, default, forwardKb[slot] ? rot : rot + 180f.Degrees(), Kind.DirForward) };
        }
        return [];
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.Shockwave)
        {
            _activation = default;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.FleshTimer && _activation == default)
        {
            _activation = WorldState.FutureTime(5d);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.FleshForward:
                forwardKb.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.FleshBack:
                backwardKb.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.FleshForward:
                forwardKb.Clear(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.FleshBack:
                backwardKb.Clear(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnActorUntargetable(Actor actor)
    {
        if (actor == Module.PrimaryActor)
        {
            _activation = default;
            forwardKb.Reset();
            backwardKb.Reset();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!forwardKb[slot] && !backwardKb[slot] || IsImmune(slot, _activation))
        {
            return;
        }

        var act = _activation;
        var aoes = CollectionsMarshal.AsSpan(_burst.Positions);
        var len = aoes.Length;
        var rot = actor.Rotation;
        var pos = actor.Position;
        var moveDir = (forwardKb[slot] ? rot : rot + 180f.Degrees()).ToDirection();
        const float distSq = 15f * 15f;
        const float radiusSq = 12f * 12f;
        const float distSqRadiusSq = distSq - radiusSq;

        for (var i = 0; i < len; ++i)
        {
            var origin = aoes[i];
            var d = origin - pos;
            var dist = d.Length();
            if (dist is <= 12f or >= 27f) // inside aoe or max distance 15 + 12 radius
            {
                continue; // inside aoe or impossible to run into this from current position
            }
            var forward = d.Dot(moveDir);
            var sideways = d.Dot(moveDir.OrthoL());

            hints.ForbiddenDirections.Add(new(Angle.Atan2(sideways, forward), Angle.Acos((dist * dist + distSqRadiusSq) / (2f * dist * 15f)), act));
        }

        Arena.Bounds.ShapeSimplified.AddForbiddenDirectionsArena(actor, Arena.Center, hints, _activation, 15f, 1f);

        // probably not needed since the cone resolves a long time after the knockback
        // if (_reach.ActiveCasters is var aoe && aoe.Length != 0 && _reach.Shape is AOEShapeCone cone)
        // {
        //     ref readonly var aoe0 = ref aoe[0];
        //     cone.AddForbiddenDirections(actor, aoe0.Origin, aoe0.Rotation, cone.Radius * 5f, cone.HalfAngle, hints, _activation, 15f);
        // }
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        if (!Arena.InBounds(pos))
        {
            return true;
        }

        var reach = _reach.ActiveCasters;
        var reachLen = reach.Length;
        for (var i = 0; i < reachLen; ++i)
        {
            if (reach[i].Check(pos))
            {
                return true;
            }
        }

        var burst = CollectionsMarshal.AsSpan(_burst.Positions);
        var burstLen = burst.Length;
        for (var i = 0; i < burstLen; ++i)
        {
            if (pos.InCircle(burst[i], 12f))
            {
                return true;
            }
        }

        return false;
    }
}
