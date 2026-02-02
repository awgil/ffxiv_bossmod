namespace BossMod.Dawntrail.Savage.RM12S2TheLindwurm;

class Replication2Assignments(BossModule module) : BossComponent(module)
{
    readonly RM12S2TheLindwurmConfig _config = Service.Config.Get<RM12S2TheLindwurmConfig>();
    public readonly Assignment?[] Assignments = new Assignment?[8];
    public bool Assigned { get; private set; }

    public record class Assignment(Actor Actor, Clockspot SpotRelative, Clockspot SpotAbsolute, Replication2Role Role);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        //if (Assignments[slot] is var (_, spot, spotA, role))
        //    hints.Add($"Position: {UICombo.EnumString(spot)} ({UICombo.EnumString(spotA)}), role: {role} wanted", false);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.Understudy && Raid.TryFindSlot(tether.Target, out var slot))
        {
            var clocksAll = typeof(Clockspot).GetEnumValues().Cast<Clockspot>().ToList();
            var angle = (source.Position - Arena.Center).ToAngle();

            var relNorth = _config.Rep2Assignments.RelativeNorth;
            var angleAdj = (angle - relNorth.Angle + 180.Degrees()).Normalized();

            var clockReal = clocksAll.MinBy(c => MathF.Abs(c.Angle.Rad - angle.Rad));
            var clockAdj = clocksAll.MinBy(c => MathF.Abs(c.Angle.Rad - angleAdj.Rad));

            Assignments[slot] = new(source, clockAdj, clockReal, _config.Rep2Assignments[clockAdj]);
            if (Assignments.All(a => a != null))
                Assigned = true;
        }
    }
}

class Replication2CloneTethers(BossModule module) : BossComponent(module)
{
    readonly RM12S2TheLindwurmConfig _config = Service.Config.Get<RM12S2TheLindwurmConfig>();
    readonly Replication2Assignments _playerAssignment = module.FindComponent<Replication2Assignments>()!;

    public readonly Clone?[] ClonesBySlot = new Clone?[8];

    public bool Assigned { get; private set; }
    public bool Locked { get; private set; }

    public record class Clone(Actor Actor, Actor Target, int Shape, Replication2Role Role = default, bool Locked = false)
    {
        public Actor Target = Target;
        public Replication2Role Role = Role;
        public bool Locked = Locked;
    }
    readonly List<Clone> _clones = [];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var assignment = _playerAssignment.Assignments[pcSlot];
        if (assignment == null)
            return; // realistically, this should never happen

        //if (Locked)
        //    return;

        void tether(Actor a, Actor b, uint color, float thickness)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddLine(a.Position, b.Position, 0xFF000000, thickness + 1);
            Arena.AddLine(a.Position, b.Position, color, thickness);
        }

        foreach (var (a, t, _, r, l) in _clones)
        {
            if (l)
            {
                //tether(a, t, ArenaColor.Border, 1);
                continue;
            }

            if (assignment.Role == r)
                tether(a, t, ArenaColor.Safe, t == pc ? 1 : 2);
            else
                tether(a, t, ArenaColor.Danger, t == pc ? 2 : 1);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var assignment = _playerAssignment.Assignments[slot];
        if (assignment == null || Locked)
            return;

        if (_clones.Any(c => c.Role == assignment.Role && c.Target != actor))
            hints.Add("Grab correct tether!");

        if (_clones.Any(c => c.Target == actor && c.Role != assignment.Role))
            hints.Add("Pass tether!");
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var shape = (TetherID)tether.ID switch
        {
            TetherID.RepCone => 0,
            TetherID.RepSpread => 1,
            TetherID.RepStack => 2,
            TetherID.RepBoss => 3,
            TetherID.Fixed => 4,
            _ => -1
        };

        if (shape < 0)
            return;

        var ix = _clones.FindIndex(c => c.Actor == source);
        if (ix >= 0)
        {
            // existing clone, reassign target
            _clones.Ref(ix).Target = WorldState.Actors.Find(tether.Target)!;
            if (shape == 4)
            {
                _clones.Ref(ix).Locked = true;
                if (_clones.All(c => c.Locked))
                    AssignToPlayers();
            }
        }
        else if (shape < 4)
        {
            _clones.Add(new(source, WorldState.Actors.Find(tether.Target)!, shape));
            if (_clones.Count == 7)
                DetermineOrder();
        }
    }

    void DetermineOrder()
    {
        var relNorth = _config.Rep2Assignments.RelativeNorth;

        var bossOrder = _clones.FindIndex(c => c.Shape == 3);
        if (bossOrder < 0)
        {
            ReportError($"Unable to find boss tether");
            return;
        }

        var boss = _clones[bossOrder];
        _clones.RemoveAt(bossOrder);

        _clones.SortByReverse(c =>
        {
            var cAngle = (c.Actor.Position - Arena.Center).ToAngle();
            var angleAdj = (cAngle - relNorth.Angle + 180.Degrees()).Normalized();
            return angleAdj.Rad;
        });
        Replication2Role[] roles = [Replication2Role.Cone1, Replication2Role.Defam1, Replication2Role.Stack1];

        for (var i = 0; i < _clones.Count; i++)
            _clones.Ref(i).Role = roles[_clones[i].Shape]++;

        _clones.Add(boss);

        Assigned = true;
    }

    void AssignToPlayers()
    {
        foreach (var c in _clones)
            if (Raid.TryFindSlot(c.Target, out var slot))
                ClonesBySlot[slot] = c;

        Locked = true;
    }
}

class Replication2FirefallSplash : Components.UniformStackSpread
{
    public int NumCasts;

    public Replication2FirefallSplash(BossModule module) : base(module, 0, 5, includeDeadTargets: true)
    {
        var tethers = module.FindComponent<Replication2CloneTethers>()!;

        foreach (var (i, player) in Raid.WithSlot(true))
        {
            if (tethers.ClonesBySlot[i]?.Role == Replication2Role.Boss)
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

    public Replication2ScaldingWaves(BossModule module) : base(module, AID._Spell_ScaldingWaves)
    {
        var tethers = module.FindComponent<Replication2CloneTethers>()!;
        _activation = WorldState.FutureTime(6.2f);
        _source = Raid.WithSlot(true).WhereSlot(s => tethers.ClonesBySlot[s]?.Role == Replication2Role.Boss).Select(s => s.Item2).FirstOrDefault();
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
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        var pos = _sourcePos ?? _source?.Position;
        if (pos == null)
            return;

        var targets = Raid.WithSlot().Exclude(_source).SortedByRange(pos.Value).Take(4).Mask();
        CurrentBaits.Add(new(pos.Value, targets, new AOEShapeCone(50, 5.Degrees()), _activation, 4));
    }
}

class Replication2ManaBurst : Components.UniformStackSpread
{
    public int NumCasts;

    public Replication2ManaBurst(BossModule module) : base(module, 0, 20, includeDeadTargets: true)
    {
        var tethers = module.FindComponent<Replication2CloneTethers>()!;

        foreach (var (i, player) in Raid.WithSlot(true))
        {
            var clone = tethers.ClonesBySlot[i];
            if (clone == null || clone.Role.IsDefam)
                AddSpread(player, WorldState.FutureTime(8));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_ManaBurst1:
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
        var tethers = module.FindComponent<Replication2CloneTethers>()!;

        foreach (var (i, player) in Raid.WithSlot(true))
        {
            if (tethers.ClonesBySlot[i]?.Role.IsStack == true)
                AddStack(player, WorldState.FutureTime(7));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_HeavySlam)
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

    public Replication2HemorrhagicProjection(BossModule module) : base(module, centerAtTarget: true)
    {
        EnableHints = false;
        var tethers = module.FindComponent<Replication2CloneTethers>()!;
        _activation = WorldState.FutureTime(8.8f);

        foreach (var (i, _) in Raid.WithSlot(true))
            if (tethers.ClonesBySlot[i]?.Role.IsCone == true)
                _targets.Set(i);
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        foreach (var i in _targets.SetBits())
        {
            if (Raid[i] is not { } target)
                continue;

            CurrentBaits.Add(new(Module.PrimaryActor, target, new AOEShapeCone(50, 25.Degrees(), target.Rotation), _activation, true));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_HemorrhagicProjection)
        {
            NumCasts++;
            _targets.Reset();
        }
    }
}

class Replication2ReenactmentOrder(BossModule module) : BossComponent(module)
{
    public enum Shape
    {
        Boss,
        Defam,
        Cone,
        Stack
    }

    public readonly List<(Actor Understudy, Shape Shape, int Group, int Order)> Replay = [];

    public override void AddGlobalHints(GlobalHints hints)
    {
        foreach (var (u, s, _, _) in Replay)
            hints.Add($"{s} @ {u}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Reenactment)
        {
            var tt = Module.FindComponent<Replication2CloneTethers>()!;
            var aa = Module.FindComponent<Replication2Assignments>()!;

            foreach (var (i, player) in Raid.WithSlot(true))
            {
                var role = tt.ClonesBySlot[i]?.Role ?? Replication2Role.None;
                if (aa.Assignments[i] is not { } assignment)
                {
                    ReportError($"No assignment for #{i} {player}, not sure how to resolve");
                    return;
                }
                var order = assignment.SpotAbsolute.SpawnOrder;
                var shape = role switch
                {
                    Replication2Role.Boss => Shape.Boss,
                    Replication2Role.Defam1 or Replication2Role.Defam2 or Replication2Role.None => Shape.Defam,
                    Replication2Role.Cone1 or Replication2Role.Cone2 => Shape.Cone,
                    Replication2Role.Stack1 or Replication2Role.Stack2 => Shape.Stack,
                    _ => throw new Exception("invalid")
                };
                Replay.Add((assignment.Actor, shape, assignment.SpotAbsolute.Group, assignment.SpotAbsolute.SpawnOrder));
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
        if ((AID)spell.Action.ID == AID._Weaponskill_Reenactment)
        {
            var mechStart = Module.CastFinishAt(spell, 8.2f);

            foreach (var (actor, mechShape, _, order) in Module.FindComponent<Replication2ReenactmentOrder>()!.Replay)
            {
                var initialCast = mechStart.AddSeconds(4 * order);
                (AOEShape?, float) shape = mechShape switch
                {
                    Replication2ReenactmentOrder.Shape.Boss => (new AOEShapeCircle(5), 0),
                    Replication2ReenactmentOrder.Shape.Defam => (new AOEShapeCircle(20), DefamDelay),
                    Replication2ReenactmentOrder.Shape.Cone => (new AOEShapeCone(50, 25.Degrees()), ConeDelay),
                    _ => (null, 0)
                };
                if (shape.Item1 != null)
                    _predicted.Add((new(shape.Item1, actor.Position, actor.Rotation, initialCast.AddSeconds(shape.Item2)), initialCast.AddSeconds(shape.Item2)));
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
            case AID._Weaponskill_FirefallSplash:
            case AID._Weaponskill_ManaBurst3:
            case AID._Weaponskill_HemorrhagicProjection1:
                _predicted.RemoveAt(0);
                NumCasts++;
                break;
            case AID._Weaponskill_HeavySlam2:
                _predicted.RemoveAt(0);
                break;
        }
    }
}

class Replication2ReenactmentTowers(BossModule module) : Components.GenericTowers(module, AID._Weaponskill_HeavySlam2)
{
    readonly List<(WPos?, DateTime)> _predicted = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_Reenactment)
        {
            var mechStart = Module.CastFinishAt(spell, 8.2f);

            foreach (var (u, s, g, o) in Module.FindComponent<Replication2ReenactmentOrder>()!.Replay)
            {
                var initialCast = mechStart.AddSeconds(4 * o);

                if (s == Replication2ReenactmentOrder.Shape.Stack)
                    _predicted.Add((u.Position, initialCast.AddSeconds(Replication2ReenactmentAOEs.StackDelay)));
                else
                    _predicted.Add((null, initialCast));
            }
            _predicted.SortBy(p => p.Item2);
        }
    }

    public override void Update()
    {
        Towers.Clear();

        var i = 0;
        foreach (var (a, b) in _predicted.Take(4))
        {
            if (a.HasValue)
                Towers.Add(new(a.Value, 5, 1, 4, new(i >= 2 ? 0xFFu : 0u), b));
            i++;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID._Weaponskill_FirefallSplash:
            case AID._Weaponskill_ManaBurst3:
            case AID._Weaponskill_HemorrhagicProjection1:
                _predicted.RemoveAt(0);
                break;
            case AID._Weaponskill_HeavySlam2:
                _predicted.RemoveAt(0);
                NumCasts++;
                break;
        }
    }
}
