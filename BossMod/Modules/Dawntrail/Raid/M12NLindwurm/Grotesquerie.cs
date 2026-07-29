namespace BossMod.Dawntrail.Raid.M12NLindwurm;

sealed class BurstingGrotesquerie(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.SpreadBurstingGrotesquerie, (uint)AID.DramaticLysis, 5f, 5d)
{
    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.BurstingGrotesquerie)
        {
            Spreads.Clear();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        // copied from base class, in 2nd phase uses spread icons without status
        if (spell.Action.ID is (uint)AID.DramaticLysis or (uint)AID.DramaticLysis1)
        {
            var count = Spreads.Count;
            var id = spell.MainTargetID;
            var spreads = CollectionsMarshal.AsSpan(Spreads);
            for (var i = 0; i < count; ++i)
            {
                if (spreads[i].Target.InstanceID == id)
                {
                    Spreads.RemoveAt(i);
                    ++NumFinishedSpreads;
                    return;
                }
            }
            // spread not found, probably due to being self targeted
            if (count != 0)
            {
                ++NumFinishedSpreads;
                Spreads.RemoveAt(0);
            }
        }
    }
}
sealed class SharedGrotesquerie(BossModule module) : Components.StackWithIcon(module, (uint)IconID.SharedGrotesquerie, (uint)AID.FourthWallFusion, 6f, 5d);
sealed class DirectedGrotesquerie(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private readonly int[] _direction = new int[8];

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID._Gen_Direction)
        {
            // 408 = front, 409 = right, 40A = behind, 40B = left
            if (status.Extra is < 0x408 or > 0x40B)
                return;

            _direction[Raid.FindSlot(actor.InstanceID)] = status.Extra;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Countdown)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            var extra = _direction[slot];
            if (extra == 0)
                return;

            var rotation = ((extra - 0x408) * -90f).Degrees();
            AOEShapeCone cone = new(60f, 15f.Degrees(), rotation);
            CurrentBaits.Add(new(actor, actor, cone, WorldState.FutureTime(5.5d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.HemorrhagicProjection)
        {
            CurrentBaits.Clear();
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (IsBaitTarget(actor))
        {
            hints.Add("Bait away!");
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count == 0)
        {
            return;
        }
        base.AddAIHints(slot, actor, assignment, hints);
        var activeBaits = CollectionsMarshal.AsSpan(ActiveBaitsOn(actor));
        if (activeBaits.Length != 0)
        {
            ref var b = ref activeBaits[0];
            var party = Raid.WithoutSlot(false, true, true);
            var lenP = party.Length;
            var pos = actor.Position;
            var angle = 15f.Degrees();
            var offset = ((_direction[slot] - 0x408) * -90f).Degrees();
            var act = b.Activation;
            for (var j = 0; j < lenP; ++j)
            {
                hints.ForbiddenDirections.Add((Angle.FromDirection(party[j].Position - pos) - offset, angle, act));
            }
        }
    }
}
