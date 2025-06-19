namespace BossMod.Dawntrail.Foray.ForkedTower.FT03MarbleDragon;

class ImitationBlizzard3(BossModule module) : ImitationBlizzard1(module)
{
    private BitMask _targets;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.WickedWater)
            _targets.Set(Raid.FindSlot(actor.InstanceID));
        if ((SID)status.ID == SID.GelidGaol)
            _targets.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        DateTime firstActivation = default;

        foreach (var cur in Puddles())
        {
            if (firstActivation == default)
                firstActivation = cur.Activation.AddSeconds(0.5f);

            var shape = cur.Actor.OID == (uint)OID.IcePuddle ? Circle : Cross;
            var safe = cur.Safe && _targets[slot];

            if (cur.Activation < firstActivation)
                yield return new AOEInstance(shape, cur.Actor.Position, cur.Actor.Rotation, cur.Activation, safe ? ArenaColor.SafeFromAOE : ArenaColor.Danger, Risky: !safe);
            else if (cur.Activation < firstActivation.AddSeconds(1))
                yield return new AOEInstance(shape, cur.Actor.Position, cur.Actor.Rotation, cur.Activation, safe ? ArenaColor.SafeFromAOE : ArenaColor.AOE, Risky: !safe);
            else
                break;
        }
    }

    protected override void AddLine(List<Actor> actorList, int[][] ixs, ActorCastInfo spell)
    {
        var start = Module.CastFinishAt(spell, 4.6f);
        for (var ixix = 0; ixix < ixs.Length; ixix++)
        {
            var group = ixs[ixix];

            foreach (var ix in group)
                _aoes.Add((actorList[ix], start, ixs.Length == 3 && ixix == 2));
            start = start.AddSeconds(1);
        }
        _aoes.SortBy(l => l.Activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_targets[slot])
            hints.Add("Stand in ice puddle!", false);
    }
}

class GelidGaol(BossModule module) : Components.Adds(module, (uint)OID.GelidGaol, 1)
{
    private BitMask _targets;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.GelidGaol)
            _targets.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.GelidGaol)
            _targets.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_targets[slot] && Actors.Any(i => i.IsTargetable))
            hints.Add("Destroy ice cubes!", false);
    }
}
