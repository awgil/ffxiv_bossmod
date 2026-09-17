namespace BossMod.Stormblood.Ultimate.UCOB;

class P3TenstrikeMeteorStream : MeteorStream
{
    public bool HatchAssigned;
    private readonly int[] _order = Utils.MakeArray(PartyState.MaxPartySize, -1);

    public P3TenstrikeMeteorStream(BossModule module) : base(module)
    {
        AddSpreads(Raid.WithoutSlot(true), WorldState.FutureTime(3.2f));
        foreach (var (slot, group) in Service.Config.Get<UCOBConfig>().P3QuickmarchTrioAssignments.Resolve(Raid))
            _order[slot] = group;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!HatchAssigned)
        {
            var order = _order[slot];
            if (order >= 0)
            {
                var sign = order > 3 ? -1 : 1;

                // intentionally asymmetrical (175 vs 180), you can guess why
                hints.AddForbiddenZone(ShapeDistance.PrecisePosition(Arena.Center + (175 + (22.5f + 45 * (order % 4)) * sign).Degrees().ToDirection() * 9, new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f));
            }

            return;
        }

        if (Module.FindComponent<Hatch>()?.IsTarget(slot) == true)
            return;

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.MeteorStream)
        {
            ++NumCasts;
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
            for (var i = 0; i < Spreads.Count; i++)
                Spreads.Ref(i).Activation = WorldState.FutureTime(1);
        }
    }
}

class P3TenstrikeEarthShaker : P3EarthShaker
{
    int _numHatches;
    readonly int[] _qmOrder = Utils.MakeArray(8, -1);

    public P3TenstrikeEarthShaker(BossModule module) : base(module)
    {
        foreach (var (slot, group) in Service.Config.Get<UCOBConfig>().P3QuickmarchTrioAssignments.Resolve(Raid))
            _qmOrder[slot] = group;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        base.OnEventIcon(actor, iconID, targetID);

        if ((IconID)iconID == IconID.Earthshaker)
        {
            var haveOrder = _qmOrder.All(q => q >= 0);
            if (CurrentBaits.Count == 4)
                CurrentBaits.SortBy(b => haveOrder ? (ulong)_qmOrder.BoundSafeAt(Raid.FindSlot(b.Target.InstanceID)) : b.Target.InstanceID);
            if (_futureBaits.Count == 4)
                _futureBaits.SortBy(b => haveOrder ? (ulong)_qmOrder.BoundSafeAt(Raid.FindSlot(b.Target.InstanceID)) : b.Target.InstanceID);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (CurrentBaits.Count == 4)
        {
            var myOrder = CurrentBaits.FindIndex(b => b.Target == actor);
            if (myOrder >= 0)
            {
                var myAngle = (90 - myOrder * 60).Degrees();
                hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, myAngle, 50, 0, 1), CurrentBaits[myOrder].Activation);
                hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, NumCasts >= 4 ? 6.5f : 16), CurrentBaits[myOrder].Activation);
            }
            else
                // non baiters stand north
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(0, -8), 4), CurrentBaits[0].Activation);
        }
        else if (CurrentBaits.Count == 0)
        {
            // after hatches, before baits go off, everyone should hang out around D marker
            if (_numHatches >= 6)
                hints.GoalZones.Add(AIHints.GoalSingleTarget(new(0, 8), 5));

            // during hatch, non hatchers gtfo to edge so they don't clip hatchers with meteor stream
            else if (Module.FindComponent<Hatch>()?.IsUntargeted(slot) == true)
            {
                hints.AddForbiddenZone(ShapeDistance.Circle(Arena.Center, 19), DateTime.MaxValue);
                foreach (var n in Module.Enemies(OID.Neurolink))
                {
                    var dir = n.Position - Arena.Center;
                    hints.AddForbiddenZone(ShapeDistance.Rect(Arena.Center, dir.ToAngle(), 50, 0, 6));
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID.Hatch)
            _numHatches++;
    }
}
