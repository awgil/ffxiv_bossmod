namespace BossMod.Stormblood.Ultimate.UWU;

// casts are 3s long and 2s apart (overlapping)
class P2Eruption(BossModule module, int numTargets) : Components.StandardAOEs(module, AID.EruptionAOE, 8)
{
    public int NumCastsStarted { get; private set; }
    public BitMask Targets;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Targets[pcSlot])
            Arena.AddCircle(pc.Position, 8, ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);
        if ((AID)spell.Action.ID == AID.EruptionAOE)
        {
            if (NumCastsStarted < numTargets)
            {
                if (NumCastsStarted == 0)
                    Targets.Reset();
                var (baiterSlot, baiter) = Raid.WithSlot().ExcludedFromMask(Targets).Closest(spell.LocXZ);
                if (baiter != null)
                    Targets.Set(baiterSlot);
            }
            ++NumCastsStarted;
            if (NumCastsStarted >= numTargets * 4)
                Targets.Reset();
        }
    }

    public override void Update()
    {
        if (NumCastsStarted == 0)
        {
            var source = ((UWU)Module).Ifrit();
            if (source != null)
                Targets = Raid.WithSlot().WhereActor(a => a.FindStatus(SID.SearingWind) == null).SortedByRange(source.Position).TakeLast(numTargets).Mask();
        }
    }
}

class P2Eruption1 : P2Eruption
{
    readonly List<Actor> BaitersPriorityOrder = [];

    bool _fetters;
    bool _howl;

    bool Active => _fetters && _howl;

    public P2Eruption1(BossModule module) : base(module, 2)
    {
        BaitersPriorityOrder.AddRange(Raid.WithoutSlot().OrderBy(s => (s.Role switch
        {
            Role.Ranged => 0,
            Role.Healer => 1,
            Role.Melee => 2,
            _ => 3,
        }, s.InstanceID)));
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        base.OnStatusGain(actor, status);

        if ((SID)status.ID == SID.InfernalFetters)
        {
            _fetters = true;
            BaitersPriorityOrder.Remove(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if ((AID)spell.Action.ID == AID.InfernoHowl)
        {
            _howl = true;
            BaitersPriorityOrder.RemoveAll(b => b.InstanceID == spell.TargetID);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (!Active || Module.FindComponent<P2Nails>() is not { } nails)
            return;

        var myOrder = BaitersPriorityOrder.IndexOf(actor);

        if (myOrder is < 0 or > 1)
            return;

        var sign = myOrder == 0 ? -1 : 1;

        var start = nails.RelativeNorth + (sign * 67.5f).Degrees();
        var s2 = Arena.Center + start.ToDirection() * 12;
        var s3 = start + (90 * -sign).Degrees();

        var myStart = s2 - s3.ToDirection() * 12.5f;
        var myEnd = s2 + s3.ToDirection() * 12.5f;

        float progress = -1;

        switch (NumCastsStarted)
        {
            case < 2:
                progress = 0;
                break;
            case < 4:
                progress = 0.33f;
                break;
            case < 6:
                progress = 0.67f;
                break;
            case < 8:
                progress = 1;
                break;
        }

        if (progress >= 0)
            hints.AddForbiddenZone(ShapeDistance.PrecisePosition(WPos.Lerp(myStart, myEnd, progress), new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f));
    }
}

class P2Eruption2(BossModule module) : P2Eruption(module, 2)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (assignment is not (PartyRolesConfig.Assignment.R1 or PartyRolesConfig.Assignment.R2))
            return;

        if (Module.FindComponent<BossPositioning>() is not { DesiredRotation: { } relNorth })
            return;

        if (NumCastsStarted >= 8)
            return;

        var desiredOffset = (relNorth - (135 - 30 * (NumCastsStarted / 2)).Degrees()).ToDirection() * 18;

        hints.AddForbiddenZone(ShapeDistance.PrecisePosition(Arena.Center + desiredOffset, new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f));
    }
}

class P4Eruption(BossModule module) : P2Eruption(module, 2)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (NumCastsStarted == 0 && assignment is PartyRolesConfig.Assignment.H2 or PartyRolesConfig.Assignment.R1)
            hints.AddForbiddenZone(ShapeDistance.Circle(new(100, 80.5f), 20));
    }
}

class P5Eruption(BossModule module) : P2Eruption(module, 3)
{
    bool MistralSongDone;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID.MistralSongAdds)
            MistralSongDone = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (MistralSongDone || actor.Role == Role.Tank || NumCastsStarted == 0)
            return;

        if (Targets[slot])
        {
            WPos dest = NumCastsStarted switch
            {
                <= 3 => new(94, 94),
                <= 6 => new(101, 96.5f),
                <= 9 => new(110, 96.5f),
                _ => new(110, 105.5f)
            };

            hints.AddForbiddenZone(ShapeDistance.PrecisePosition(dest, new(0, 1), 0.5f, actor.Position, 0.1f));
        }
        else
        {
            // hide behind tanks
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(new(91, 111), 1));
        }
    }
}

class P5Eruption2(BossModule module) : P2Eruption(module, 2);
