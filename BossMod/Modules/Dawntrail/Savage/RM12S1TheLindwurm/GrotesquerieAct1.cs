namespace BossMod.Dawntrail.Savage.RM12S1TheLindwurm;

class RavenousReach(BossModule module) : Components.StandardAOEs(module, AID.RavenousReachCone, new AOEShapeCone(35, 60.Degrees()));
class DirectedGrotesquerie(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    readonly Angle?[] _offset = new Angle?[8];
    DateTime _activation;

    public override void Update()
    {
        CurrentBaits.Clear();

        for (var i = 0; i < _offset.Length; i++)
        {
            if (_offset[i] is not { } offset)
                continue;

            if (Raid[i] is not { } target)
                continue;

            CurrentBaits.Add(new(Module.PrimaryActor, target, new AOEShapeCone(60, 15.Degrees(), target.Rotation + offset), _activation, true));
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DirectedGrotesquerieVisual && Raid.TryFindSlot(actor, out var slot))
        {
            _offset[slot] = status.Extra switch
            {
                0x40D => -90.Degrees(),
                0x40E => 180.Degrees(),
                0x40F => 90.Degrees(),
                _ => default
            };
        }

        if ((SID)status.ID == SID.DirectedGrotesquerie)
            _activation = status.ExpireAt;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.DirectedGrotesquerieVisual && Raid.TryFindSlot(actor, out var slot))
            _offset[slot] = null;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_offset[slot] is not { } offset)
            return;

        var forbidden = new ArcList(actor.Position, 60);
        foreach (var ally in Raid.WithoutSlot().Exclude(actor))
        {
            var angle = actor.AngleTo(ally) - offset;
            forbidden.ForbidInfiniteCone(actor.Position, angle, 18.Degrees());
        }

        foreach (var (from, to) in forbidden.Forbidden.Segments)
        {
            var center = (to + from) * 0.5f;
            var width = (to - from) * 0.5f;
            hints.ForbiddenDirections.Add((center.Radians(), width.Radians(), _activation));
        }
    }
}

class PhagocyteSpotlightPlayer(BossModule module) : Components.StandardAOEs(module, AID.PhagocyteSpotlightPlayer, 5)
{
    public float FinalCastDelay = 5f;
    public bool Finished;
    DateTime _firstCast;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
        {
            if (_firstCast == default)
                _firstCast = WorldState.CurrentTime;
            else if (_firstCast.AddSeconds(FinalCastDelay) < WorldState.CurrentTime)
                Finished = true;
        }
    }
}

class Act1GrotesquerieSpreadHint(BossModule module) : BossComponent(module)
{
    readonly string?[] Job = new string?[8];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.BurstingGrotesquerie:
                Job[Raid.FindSlot(actor.InstanceID)] = "Spread";
                Assign();
                break;
            case SID.SharedGrotesquerie:
                Job[Raid.FindSlot(actor.InstanceID)] = "Stack";
                Assign();
                break;
        }
    }

    void Assign()
    {
        if (Job.Count(c => c == null) == 3)
        {
            for (var i = 0; i < Job.Length; i++)
                Job[i] ??= "Stack";
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Job.BoundSafeAt(slot) is { } j)
            hints.Add($"Next: {j}", false);
    }
}

class Act1GrotesquerieStackSpread(BossModule module) : Components.UniformStackSpread(module, 6, 6, 4, 4)
{
    public int NumCasts;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.BurstingGrotesquerie:
                AddSpread(actor, status.ExpireAt);
                break;
            case SID.SharedGrotesquerie:
                AddStack(actor, status.ExpireAt);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BurstingGrotesquerieDramaticLysis:
                Stacks.Clear();
                NumCasts++;
                break;
            case AID.SharedGrotesquerieFourthWallFusion:
                Spreads.Clear();
                NumCasts++;
                break;
        }
    }
}

class BurstPre(BossModule module) : Components.GenericAOEs(module, AID.Burst)
{
    readonly List<(Actor, DateTime)> _casters = [];

    public bool Risky = true;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(new AOEShapeCircle(12), c.Item1.Position, Activation: c.Item2, Risky: Risky));

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EBF29 && state == 0x00100020)
            _casters.Add((actor, WorldState.FutureTime(6.7f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Clear();
    }
}
class Burst(BossModule module) : Components.StandardAOEs(module, AID.Burst, 12);

class BurstStackSpread(BossModule module) : Components.UniformStackSpread(module, 6, 6, 6)
{
    public int NumCasts;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.Share)
            AddStack(actor, WorldState.FutureTime(5.2f));
        if ((IconID)iconID == IconID.Tankbuster)
            AddSpread(actor, WorldState.FutureTime(5.2f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Act1FourthWallFusion:
                NumCasts++;
                Stacks.Clear();
                break;
            case AID.VisceralBurst:
                NumCasts++;
                Spreads.Clear();
                break;
        }
    }
}
