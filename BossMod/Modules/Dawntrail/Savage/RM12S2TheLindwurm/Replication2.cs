namespace BossMod.Dawntrail.Savage.RM12S2TheLindwurm;

class Replication2Staging(BossModule module) : StagingAssignment<Replication2Role>(module, playerGroupSize: 2, cloneGroupSize: 6, hasBossTether: true)
{
    readonly RM12S2TheLindwurmConfig _config = module.Config.Get<RM12S2TheLindwurmConfig>();

    protected override Replication2Role? DeterminePlayerRole(PlayerClone c)
    {
        if (!_config.Rep2Assignments.IsValid())
            return null;

        var angleReal = c.Position;
        var relNorth = _config.Rep2Assignments.RelativeNorth;
        var angleAdj = (angleReal - relNorth.Angle + 180.Degrees()).Normalized();

        return _config.Rep2Assignments[Clockspot.GetClosest(angleAdj)];
    }

    protected override Replication2Role DetermineCloneRole(WurmClone w)
    {
        if (w.Shape == CloneShape.Boss)
            return Replication2Role.Boss;

        var relNorth = _config.Rep2Assignments.RelativeNorth;
        var clockSorted = WurmClones.Where(w => w.Shape != CloneShape.Boss).OrderByDescending(c =>
        {
            var angleAbs = (c.Actor.Position - Arena.Center).ToAngle();
            var angleRel = (angleAbs - relNorth.Angle + 180.Degrees()).Normalized();
            return angleRel.Rad;
        });
        Replication2Role[] roles = [Replication2Role.Boss, Replication2Role.Cone1, Replication2Role.Defam1, Replication2Role.Stack1];

        foreach (var c in clockSorted)
        {
            var r = roles[(int)c.Shape!]++;
            if (c.Actor == w.Actor)
                return r;
        }

        return Replication2Role.None;
    }
}

class Replication2FirefallSplash : Components.UniformStackSpread
{
    public int NumCasts;

    public Replication2FirefallSplash(BossModule module) : base(module, 0, 5, includeDeadTargets: true)
    {
        var tethers = module.FindComponent<Replication2Staging>()!;

        foreach (var (i, player) in Raid.WithSlot(true))
        {
            if (tethers.WurmsBySlot[i]?.AssignedRole == Replication2Role.Boss)
                AddSpread(player, WorldState.FutureTime(5.9f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FirefallSplash:
                NumCasts++;
                Spreads.RemoveAll(s => s.Radius == 5);
                break;
        }
    }
}

class Replication2ScaldingWaves : Components.UntelegraphedBait
{
    Actor? _source;
    WPos? _sourcePos;
    readonly DateTime _activation;

    public BitMask Targets;

    public static readonly AOEShapeCone Shape = new(50, 5.Degrees());

    public Replication2ScaldingWaves(BossModule module) : base(module, AID.ScaldingWaves)
    {
        var tethers = module.FindComponent<Replication2Staging>()!;
        _activation = WorldState.FutureTime(6.2f);
        _source = Raid.WithSlot(true).WhereSlot(s => tethers.WurmsBySlot[s]?.AssignedRole == Replication2Role.Boss).Select(s => s.Item2).FirstOrDefault();
    }

    // it doesn't seem that jump target can be hit by cones regardless of where they move after the jump
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor != _source)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor != _source)
            base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FirefallSplash)
            _sourcePos = spell.TargetXZ;

        if (spell.Action == WatchedAction)
        {
            _source = null;
            _sourcePos = null;
            NumCasts++;

            var (slot, closest) = Raid.WithSlot().ExcludedFromMask(Targets).MinBy(a =>
            {
                var (i, p) = a;
                var angle = (p.Position - caster.Position).ToAngle();
                return MathF.Abs(angle.Rad - spell.Rotation.Rad);
            });
            if (closest != null)
                Targets.Set(slot);
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        var pos = _sourcePos ?? _source?.Position;
        if (pos == null)
            return;

        var targets = Raid.WithSlot().Exclude(_source).SortedByRange(pos.Value).Take(4).Mask();
        CurrentBaits.Add(new(pos.Value, targets, Shape, _activation, 4));
    }
}

class Replication2ManaBurst : Components.UniformStackSpread
{
    public int NumCasts;

    public Replication2ManaBurst(BossModule module) : base(module, 0, 20, includeDeadTargets: true)
    {
        var tethers = module.FindComponent<Replication2Staging>()!;

        foreach (var (i, player) in Raid.WithSlot(true))
        {
            var clone = tethers.WurmsBySlot[i];
            if (clone == null || clone.AssignedRole.IsDefam)
                AddSpread(player, WorldState.FutureTime(8));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ManaBurstAOE:
                var closest = Spreads.MinBy(s => (s.Target.Position - spell.TargetXZ).LengthSq());
                if (closest.Target != null)
                    Spreads.Remove(closest);
                NumCasts++;
                break;
        }
    }
}

class Replication2HeavySlam : Components.UniformStackSpread
{
    public int NumCasts;

    public Replication2HeavySlam(BossModule module) : base(module, 5, 0, 3, includeDeadTargets: true)
    {
        EnableHints = false;
        var tethers = module.FindComponent<Replication2Staging>()!;

        foreach (var (i, player) in Raid.WithSlot(true))
        {
            if (tethers.WurmsBySlot[i]?.AssignedRole.IsStack == true)
                AddStack(player, WorldState.FutureTime(7));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HeavySlam)
        {
            var closest = Stacks.MinBy(s => (s.Target.Position - spell.TargetXZ).LengthSq());
            if (closest.Target != null)
                Stacks.Remove(closest);
            NumCasts++;
        }
    }
}

class Replication2HemorrhagicProjection : Components.GenericBaitAway
{
    readonly DateTime _activation;
    BitMask _targets;

    public static readonly AOEShapeCone Shape = new(50, 25.Degrees());

    public Replication2HemorrhagicProjection(BossModule module) : base(module, centerAtTarget: true)
    {
        EnableHints = false;
        var tethers = module.FindComponent<Replication2Staging>()!;
        _activation = WorldState.FutureTime(8.8f);

        foreach (var (i, _) in Raid.WithSlot(true))
            if (tethers.WurmsBySlot[i]?.AssignedRole.IsCone == true)
                _targets.Set(i);
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        foreach (var i in _targets.SetBits())
        {
            if (Raid[i] is not { } target)
                continue;

            CurrentBaits.Add(new(Module.PrimaryActor, target, Shape with { DirectionOffset = target.Rotation }, _activation, true));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!CurrentBaits.Any(b => b.Target == actor))
            return;

        var forbidden = new ArcList(actor.Position, 60);
        foreach (var ally in Raid.WithoutSlot().Exclude(actor))
        {
            var angle = actor.AngleTo(ally);
            forbidden.ForbidInfiniteCone(actor.Position, angle, 28.Degrees());
        }

        foreach (var (from, to) in forbidden.Forbidden.Segments)
        {
            var center = (to + from) * 0.5f;
            var width = (to - from) * 0.5f;
            hints.ForbiddenDirections.Add((center.Radians(), width.Radians(), _activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HemorrhagicProjection)
        {
            NumCasts++;
            _targets.Reset();
        }
    }
}

class Replication2ReenactmentOrder(BossModule module) : BossComponent(module)
{
    public readonly List<(Actor Understudy, CloneShape Shape, int Order)> Replay = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Reenactment)
        {
            var aa = Module.FindComponent<Replication2Staging>()!;

            foreach (var (i, player) in Raid.WithSlot(true))
            {
                if (aa.PlayersBySlot[i] is not { } assignment)
                {
                    ReportError($"No clone assigned for raid member {player}, this should never happen");
                    continue;
                }
                var shape = aa.WurmsBySlot[i]?.Shape ?? CloneShape.Spread;
                var order = assignment.SpawnOrder;
                Replay.Add((assignment.Actor, shape, assignment.SpawnOrder));
            }
            Replay.SortBy(r => r.Order);
        }
    }
}

class Replication2ReenactmentAOEs(BossModule module) : Components.GenericAOEs(module)
{
    // list of predicted aoes with "holes" so we don't overlap with towers
    readonly List<(AOEInstance?, DateTime)> _predicted = [];

    public const float DefamDelay = 1.2f;
    public const float ConeDelay = 1.9f;
    public const float StackDelay = 1.6f;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var ((p, _), i) in _predicted.Take(4).Zip(Enumerable.Range(0, 100)).Reverse())
            if (p != null)
                yield return p.Value with { Risky = i < 2, Color = i < 2 ? ArenaColor.Danger : ArenaColor.AOE };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Reenactment)
        {
            var mechStart = Module.CastFinishAt(spell, 8.2f);

            foreach (var (actor, mechShape, order) in Module.FindComponent<Replication2ReenactmentOrder>()!.Replay)
            {
                var initialCast = mechStart.AddSeconds(4 * order);
                var (shape, delay) = mechShape switch
                {
                    CloneShape.Boss => (new AOEShapeCircle(5), 0f),
                    CloneShape.Spread => (new AOEShapeCircle(20), DefamDelay),
                    CloneShape.Cone => (Replication2HemorrhagicProjection.Shape, ConeDelay),
                    _ => (default(AOEShape), 0f)
                };
                if (shape != null)
                    _predicted.Add((new(shape, actor.Position, actor.Rotation, initialCast.AddSeconds(delay)), initialCast.AddSeconds(delay)));
                else
                    _predicted.Add((null, initialCast));
            }
            _predicted.SortBy(p => p.Item2);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FirefallSplashReplay:
            case AID.ManaBurstReplay:
            case AID.HemorrhagicProjectionReplay:
                _predicted.RemoveAt(0);
                NumCasts++;
                break;
            case AID.HeavySlamReplay:
                _predicted.RemoveAt(0);
                break;
        }
    }
}

class Replication2ReenactmentTowers(BossModule module) : Components.GenericTowers(module, AID.HeavySlamReplay)
{
    readonly List<(WPos?, DateTime)> _predicted = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Reenactment)
        {
            var mechStart = Module.CastFinishAt(spell, 8.2f);

            foreach (var (u, s, o) in Module.FindComponent<Replication2ReenactmentOrder>()!.Replay)
            {
                var initialCast = mechStart.AddSeconds(4 * o);

                if (s == CloneShape.Stack)
                    _predicted.Add((u.Position, initialCast.AddSeconds(Replication2ReenactmentAOEs.StackDelay)));
                else
                    _predicted.Add((null, initialCast));
            }
            _predicted.SortBy(p => p.Item2);
        }
    }

    // TODO: do this in a less highly regarded way
    public override void Update()
    {
        Towers.Clear();

        foreach (var ((a, b), i) in _predicted.Take(4).Zip(Enumerable.Range(0, 100)))
            if (a.HasValue)
                Towers.Add(new(a.Value, 5, 1, 4, new(i >= 2 ? 0xFFu : 0u), b));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FirefallSplashReplay:
            case AID.ManaBurstReplay:
            case AID.HemorrhagicProjectionReplay:
                _predicted.RemoveAt(0);
                break;
            case AID.HeavySlamReplay:
                _predicted.RemoveAt(0);
                NumCasts++;
                break;
        }
    }
}

class Replication2ReenactmentScaldingWaves(BossModule module) : Components.GenericBaitAway(module)
{
    readonly List<(Actor?, DateTime)> _predicted = [];

    BitMask _targets;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Reenactment)
        {
            _targets = Module.FindComponent<Replication2ScaldingWaves>()!.Targets;
            var mechStart = Module.CastFinishAt(spell, 8.2f);

            foreach (var (u, s, o) in Module.FindComponent<Replication2ReenactmentOrder>()!.Replay)
            {
                var initialCast = mechStart.AddSeconds(4 * o);

                if (s == CloneShape.Boss)
                    _predicted.Add((u, initialCast.AddSeconds(Replication2ReenactmentAOEs.ConeDelay)));
                else
                    _predicted.Add((null, initialCast));
            }
            _predicted.SortBy(p => p.Item2);
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        foreach (var (a, b) in _predicted.Take(2))
        {
            if (a != null)
            {
                foreach (var (_, player) in Raid.WithSlot().IncludedInMask(_targets))
                    CurrentBaits.Add(new(a, player, Replication2ScaldingWaves.Shape, b));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ManaBurstReplay:
            case AID.HemorrhagicProjectionReplay:
            case AID.HeavySlamReplay:
                if (_predicted.Count > 0)
                    _predicted.RemoveAt(0);
                break;
            case AID.ScaldingWavesReplay:
                _predicted.Clear();
                NumCasts++;
                break;
        }
    }
}

class Replication2TimelessSpite(BossModule module) : Components.UniformStackSpread(module, 6, 0, maxStackSize: 2)
{
    DateTime Activation;
    bool Far;
    public int NumCasts;

    public override void Update()
    {
        Stacks.Clear();

        if (Activation == default)
            return;

        var targets = Raid.WithoutSlot().SortedByRange(Module.PrimaryActor.Position);
        if (Far)
            targets = targets.Reverse();
        foreach (var t in targets.Take(2))
            AddStack(t, Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NetherwrathNear:
                Activation = Module.CastFinishAt(spell, 1.2f);
                Far = false;
                break;
            case AID.NetherwrathFar:
                Activation = Module.CastFinishAt(spell, 1.2f);
                Far = true;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TimelessSpite)
        {
            NumCasts++;
            Activation = default;
        }
    }
}
