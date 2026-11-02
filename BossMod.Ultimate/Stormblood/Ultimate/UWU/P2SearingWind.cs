namespace BossMod.Stormblood.Ultimate.UWU;

class P2SearingWind(BossModule module) : Components.StrictUniformStackSpread(module, 0, 14, alwaysShowSpreads: true, includeDeadTargets: true)
{
    private int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.InfernoHowl && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
        {
            AddSpread(target, WorldState.FutureTime(8));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SearingWind)
        {
            var index = Enumerable.Range(0, Spreads.Count).MinBy(i => (spell.TargetXZ - Spreads[i].Target.Position).LengthSq());
            if (index < Spreads.Count)
            {
                ref var spread = ref Spreads.Ref(index);
                var status = spread.Target.FindStatus(SID.SearingWind);
                if (status == null || (status.Value.ExpireAt - WorldState.CurrentTime).TotalSeconds < 6)
                {
                    NumCasts++;
                    Spreads.RemoveAt(index);
                }
                else
                {
                    spread.Activation = WorldState.FutureTime(6);
                }
            }
        }
    }

    public override (WPos, float)? GetSpreadDestination(int order, Actor target)
    {
        if (order == 0 && NumCasts == 0 && Module.FindComponent<P2Nails>() is { RelativeNorth: var rel })
            return (Arena.Center - rel.ToDirection() * 19, 1);

        if (NumCasts > 0 && Module.FindComponent<BossPositioning>() is { DesiredRotation: { } relNorth })
        {
            var spot = (relNorth + (order == 0 ? 80 : -80).Degrees()).ToDirection() * 19;
            return (Arena.Center + spot, 1);
        }

        return default;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (((UWU)Module).Ifrit() is { } ifr)
            hints.GoalZones.Add(AIHints.GoalSingleTarget(ifr.Position, 5, 0.1f));
    }
}

class P4SearingWind(BossModule module) : Components.StrictUniformStackSpread(module, 0, 14, alwaysShowSpreads: true, includeDeadTargets: true)
{
    ArcList _safety = new(default, 0);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.InfernoHowl && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
        {
            AddSpread(target, WorldState.FutureTime(8));

            _safety = new(Arena.Center, 18.5f);
            _safety.ForbidInfiniteRect(caster.Position, caster.Rotation, 9);
            _safety.ForbidInfiniteRect(caster.Position + caster.Rotation.ToDirection().OrthoR() * 20, caster.Rotation, 9);
            _safety.ForbidInfiniteRect(caster.Position + caster.Rotation.ToDirection().OrthoL() * 20, caster.Rotation, 9);
            _safety.ForbidInfiniteCone(Arena.Center, 180.Degrees(), 135.Degrees());
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SearingWind)
        {
            var index = Enumerable.Range(0, Spreads.Count).MinBy(i => (spell.TargetXZ - Spreads[i].Target.Position).LengthSq());
            if (index < Spreads.Count)
            {
                ref var spread = ref Spreads.Ref(index);
                var status = spread.Target.FindStatus(SID.SearingWind);
                if (status == null || (status.Value.ExpireAt - WorldState.CurrentTime).TotalSeconds < 6)
                    Spreads.RemoveAt(index);
                else
                    spread.Activation = WorldState.FutureTime(6);
            }
        }
    }

    public override (WPos, float)? GetSpreadDestination(int order, Actor target)
    {
        var (min, max) = _safety.NextAllowed(180.Degrees(), false);
        var closest = min.Abs() > max.Abs() ? min : max;
        return (Arena.Center + closest.ToDirection() * 18.5f, 1.5f);
    }
}

