namespace BossMod.Dawntrail.Savage.M12S2Lindwurm;

class Replication2Staging(BossModule module)
    : StagingAssignment<Replication2Role>(module, playerGroupSize: 2, cloneGroupSize: 6, hasBossTether: true)
{
    readonly M12S2LindwurmConfig _config = Service.Config.Get<M12S2LindwurmConfig>();
    readonly List<WurmClone> _sortedClones = [];

    protected override Replication2Role? DeterminePlayerRole(PlayerClone c)
    {
        var eff = _config.GetReplication2();

        var relNorth = eff.RelativeNorth;
        var angleAdj = (c.Position - relNorth.Angle + 180f.Degrees()).Normalized();

        return eff.GetRole(Clockspot.GetClosest(angleAdj));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        var p = PlayersBySlot[pcSlot];
        if (p == null || WurmsAssigned)
            return;

        for (var i = 0; i < WurmClones.Count; ++i)
        {
            var w = WurmClones[i];
            if (w.Locked || w.Target == null)
                continue;

            var correct = RoleEq(p.WantedRole, w.AssignedRole);

            if (correct)
                Arena.ZoneCircleOutline(w.Actor.Position, 1.25f, Colors.Safe);
        }
    }

    protected override Replication2Role DetermineCloneRole(WurmClone w)
    {
        var northAngle = _config.GetReplication2().RelativeNorth.Angle;

        _sortedClones.Clear();

        var clones = CollectionsMarshal.AsSpan(WurmClones);
        for (var i = 0; i < clones.Length; ++i)
        {
            _sortedClones.Add(clones[i]);
        }

        var span = CollectionsMarshal.AsSpan(_sortedClones);
        var count = span.Length;
        var center = Module.Center;

        // selection sort descending by adjusted angle
        for (var i = 0; i < count - 1; ++i)
        {
            var maxIdx = i;

            var maxVal = ((span[i].Actor.Position - center).ToAngle() - northAngle + 180f.Degrees()).Normalized().Rad;

            for (var j = i + 1; j < count; ++j)
            {
                var val = ((span[j].Actor.Position - center).ToAngle() - northAngle + 180f.Degrees()).Normalized().Rad;

                if (val > maxVal)
                {
                    maxVal = val;
                    maxIdx = j;
                }
            }

            if (maxIdx != i)
                (span[i], span[maxIdx]) = (span[maxIdx], span[i]);
        }

        Replication2Role[] roles =
        [
            Replication2Role.Boss,
            Replication2Role.Cone1,
            Replication2Role.Defam1,
            Replication2Role.Stack1
        ];

        for (var i = 0; i < count; ++i)
        {
            ref var c = ref span[i];
            var idx = (int)c.Shape!;
            var role = roles[idx]++;
            if (c.Actor == w.Actor)
                return role;
        }

        return Replication2Role.None;
    }
}

class Replication2FirefallSplash : Components.UniformStackSpread
{
    public int NumCasts;

    public Replication2FirefallSplash(BossModule module)
        : base(module, 0, 5, includeDeadTargets: true)
    {
        var tethers = module.FindComponent<Replication2Staging>()!;
        var activation = WorldState.FutureTime(5.9f);

        var party = Raid.WithSlot(true);
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;
            var player = entry.Item2;

            var worm = tethers.WurmsBySlot[slot];
            if (worm != null && worm.AssignedRole == Replication2Role.Boss)
                AddSpread(player, activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FirefallSplash)
        {
            ++NumCasts;

            // remove only spreads with radius 5 (defensive-safe)
            for (var i = Spreads.Count - 1; i >= 0; --i)
            {
                if (Spreads[i].Radius == 5)
                    Spreads.RemoveAt(i);
            }
        }
    }
}

class Replication2ScaldingWaves : Components.GenericBaitProximity
{
    private Actor? _source;
    private WPos? _sourcePos;
    private readonly DateTime _activation;
    private readonly Replication2Staging _staging;

    public BitMask Targets;

    public static readonly AOEShapeCone Shape = new(50, 5f.Degrees());

    public Replication2ScaldingWaves(BossModule module)
        : base(module)
    {
        _staging = module.FindComponent<Replication2Staging>()!;
        _activation = WorldState.FutureTime(6.2f);

        var party = Raid.WithSlot(true);
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;
            var player = entry.Item2;

            var clone = _staging.WurmsBySlot[slot];
            if (clone != null && clone.AssignedRole == Replication2Role.Boss)
            {
                _source = player;
                break;
            }
        }
    }

    // jump target should not be warned
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (actor == _source)
            return;

        base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor == _source)
            return;

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        var clone = _staging.WurmsBySlot[pcSlot];
        if (clone == null || _source == null)
            return;

        var role = clone.AssignedRole;
        if (!role.IsCone && !role.IsStack)
            return;

        // Calculate bait position behind boss tether player
        var config = Service.Config.Get<M12S2LindwurmConfig>();
        var eff = config.GetReplication2();

        // Boss player goes to relative north for DN or relative west for BC
        var bossCardinal = eff.RelativeNorth.Angle;
        var bossPos = Module.Center + 18f * bossCardinal.ToDirection();

        // Calculate perpendicular direction for spreading
        var perpDir = bossCardinal.ToDirection().OrthoR(); // Right is CW side

        WPos targetPos;
        // DN order (left to right): Stack1, Cone1, Cone2, Stack2
        // BC order (left to right): Cone2, Stack2, Stack1, Cone1
        var isDN = eff.RelativeNorth == Clockspot.N;

        if (isDN)
        {
            // DN positioning
            if (role == Replication2Role.Stack1)
                targetPos = bossPos + 6f * perpDir; // Leftmost
            else if (role == Replication2Role.Cone1)
                targetPos = bossPos + 2f * perpDir; // Left-center
            else if (role == Replication2Role.Cone2)
                targetPos = bossPos - 2f * perpDir; // Right-center
            else if (role == Replication2Role.Stack2)
                targetPos = bossPos - 6f * perpDir; // Rightmost
            else
                return; // Not a cone or stack role
        }
        else
        {
            // BC positioning
            if (role == Replication2Role.Cone2)
                targetPos = bossPos + 6f * perpDir; // Leftmost
            else if (role == Replication2Role.Stack2)
                targetPos = bossPos + 2f * perpDir; // Left-center
            else if (role == Replication2Role.Stack1)
                targetPos = bossPos - 2f * perpDir; // Right-center
            else if (role == Replication2Role.Cone1)
                targetPos = bossPos - 6f * perpDir; // Rightmost
            else
                return; // Not a cone or stack role
        }

        // Draw positioning hint
        var isInPosition = pc.Position.InCircle(targetPos, 2f);
        Arena.ZoneCircleOutline(targetPos, 1f, isInPosition ? Colors.Safe : Colors.Vulnerable);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var aid = spell.Action.ID;

        if (aid == (uint)AID.FirefallSplash)
        {
            _sourcePos = spell.TargetXZ;
        }
        else if (aid == (uint)AID.ScaldingWaves)
        {
            _source = null;
            _sourcePos = null;
            ++NumCasts;

            // determine closest target to cone centerline
            var party = Raid.WithSlot(true, true, true);
            var len = party.Length;

            var bestSlot = -1;
            var bestDiff = float.MaxValue;

            for (var i = 0; i < len; ++i)
            {
                ref var entry = ref party[i];
                var slot = entry.Item1;
                var player = entry.Item2;

                if (Targets[slot])
                    continue;

                var angle = (player.Position - caster.Position).ToAngle();
                var diff = MathF.Abs(angle.Rad - spell.Rotation.Rad);

                if (diff < bestDiff)
                {
                    bestDiff = diff;
                    bestSlot = slot;
                }
            }

            if (bestSlot >= 0)
                Targets.Set(bestSlot);
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        var pos = _sourcePos ?? _source?.Position;
        if (pos == null)
            return;

        // select 4 nearest players excluding source
        CurrentBaits.Add(new Bait(
            pos.Value,
            Shape,
            _activation,
            numTargets: 4,
            nearest: true
        ));
    }
}

class Replication2ManaBurst : Components.UniformStackSpread
{
    public int NumCasts;
    private readonly Replication2Staging _staging;

    public Replication2ManaBurst(BossModule module)
        : base(module, 0, 20, includeDeadTargets: true)
    {
        _staging = module.FindComponent<Replication2Staging>()!;

        var party = Raid.WithSlot(true, true, true);
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;
            var player = entry.Item2;

            var clone = _staging.WurmsBySlot[slot];
            if (clone == null || clone.AssignedRole.IsDefam)
                AddSpread(player, WorldState.FutureTime(8));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        var clone = _staging.WurmsBySlot[pcSlot];
        if (clone == null)
            return;

        var role = clone.AssignedRole;

        // Only show hints for players with no tether (Boss role) and defamation players
        if (!role.IsDefam)
            return;

        // Calculate safe spread positions for no-tether and defamation players
        var config = Service.Config.Get<M12S2LindwurmConfig>();
        var eff = config.GetReplication2();

        // Boss tether players go to relative north (DN) or west (BC)
        var bossCardinal = eff.RelativeNorth.Angle;

        WPos targetPos;

        if (role == Replication2Role.Defam1)
        {
            // Defam1 at -100 degrees from boss position
            var defamAngle = (bossCardinal - 100.Degrees()).Normalized();
            targetPos = Module.Center + 19f * defamAngle.ToDirection();
        }
        else if (role == Replication2Role.Defam2)
        {
            // Defam2 at +100 degrees from boss position
            var defamAngle = (bossCardinal + 100.Degrees()).Normalized();
            targetPos = Module.Center + 19f * defamAngle.ToDirection();
        }
        else // None
        {
            // None at opposite side from boss position (180 degrees)
            var noneAngle = (bossCardinal + 180.Degrees()).Normalized();
            targetPos = Module.Center + 19f * noneAngle.ToDirection();
        }

        // Draw positioning hint
        var isInPosition = pc.Position.InCircle(targetPos, 2f);
        Arena.ZoneCircleOutline(targetPos, 1f, isInPosition ? Colors.Safe : Colors.Vulnerable);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.ManaBurstAOE)
            return;

        // find closest spread target to explosion location
        var spreads = CollectionsMarshal.AsSpan(Spreads);
        var count = spreads.Length;

        var bestIndex = -1;
        var bestDist = float.MaxValue;
        var targetPos = spell.TargetXZ;

        for (var i = 0; i < count; ++i)
        {
            ref var spread = ref spreads[i];
            var dist = (spread.Target.Position - targetPos).LengthSq();
            if (dist < bestDist)
            {
                bestDist = dist;
                bestIndex = i;
            }
        }

        if (bestIndex >= 0)
            Spreads.RemoveAt(bestIndex);

        ++NumCasts;
    }
}
class Replication2HeavySlam : Components.UniformStackSpread
{
    public int NumCasts;
    private readonly Replication2Staging _staging;
    private Replication2TimelessSpite? _timelessSpite;

    public Replication2HeavySlam(BossModule module)
        : base(module, 5, 0, minStackSize: 3, includeDeadTargets: true)
    {
        _staging = module.FindComponent<Replication2Staging>()!;

        var party = Raid.WithSlot(true, true, true);
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;
            var player = entry.Item2;

            var clone = _staging.WurmsBySlot[slot];
            if (clone != null && clone.AssignedRole.IsStack)
                AddStack(player, WorldState.FutureTime(7));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // Lazy-load the TimelessSpite component
        _timelessSpite ??= Module.FindComponent<Replication2TimelessSpite>();

        // Determine player's light party based on their role
        var playerClone = _staging.WurmsBySlot[pcSlot];
        if (playerClone == null)
        {
            base.DrawArenaForeground(pcSlot, pc);
            return;
        }

        var playerRole = playerClone.AssignedRole;
        var config = Service.Config.Get<M12S2LindwurmConfig>();
        var eff = config.GetReplication2();
        var isBCStrat = eff.RelativeNorth == Clockspot.W;

        // For BC strat: after Netherwrath resolves, Cone/Stack players bait cones, others take stacks
        if (isBCStrat && _timelessSpite != null && _timelessSpite.NumCasts > 0)
        {
            // Cone and Stack players should NOT see stack indicators after Netherwrath
            if (playerRole.IsCone || playerRole.IsStack)
                return;
        }

        // Determine which light party the player belongs to
        // CW light party: Boss, Cone1, Stack1, Defam1
        // CCW light party: None, Cone2, Stack2, Defam2
        var playerIsCW = playerRole is Replication2Role.Boss or Replication2Role.Cone1 or Replication2Role.Stack1 or Replication2Role.Defam1;

        // Show only the stack for the player's light party
        var stacks = CollectionsMarshal.AsSpan(Stacks);
        for (var i = 0; i < stacks.Length; ++i)
        {
            ref var stack = ref stacks[i];
            var stackTarget = stack.Target;

            // Find the role of the stack target
            var stackSlot = Raid.FindSlot(stackTarget.InstanceID);
            if (stackSlot < 0)
                continue;

            var stackClone = _staging.WurmsBySlot[stackSlot];
            if (stackClone == null)
                continue;

            var stackRole = stackClone.AssignedRole;

            // Determine which stack this is
            var stackIsCW = stackRole == Replication2Role.Stack1;

            // Only show stack if it matches the player's light party
            if (playerIsCW == stackIsCW)
            {
                // Draw the stack using same logic as base class
                bool dangerColor;
                if (stack.ForbiddenPlayers[pcSlot])
                {
                    dangerColor = true;
                }
                else if (stackTarget == pc)
                {
                    dangerColor = false;
                }
                else
                {
                    var numInside = stack.NumInside(Module);
                    var isInside = stack.IsInside(pc);
                    var max = stack.MaxSize;
                    dangerColor = !isInside && numInside >= max || isInside && numInside > max || IsStackTarget(pc) || IsSpreadTarget(pc);
                }

                Arena.ZoneCircleOutline(stackTarget.Position.Quantized(), stack.Radius, dangerColor ? default : Colors.Safe);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.HeavySlam)
            return;

        var stacks = CollectionsMarshal.AsSpan(Stacks);
        var count = stacks.Length;

        var bestIndex = -1;
        var bestDist = float.MaxValue;
        var targetPos = spell.TargetXZ;

        for (var i = 0; i < count; ++i)
        {
            ref var stack = ref stacks[i];
            var dist = (stack.Target.Position - targetPos).LengthSq();
            if (dist < bestDist)
            {
                bestDist = dist;
                bestIndex = i;
            }
        }

        if (bestIndex >= 0)
            Stacks.RemoveAt(bestIndex);

        ++NumCasts;
    }
}

sealed class Replication2HemorrhagicProjection : Components.GenericBaitAway
{
    private readonly DateTime _activation;
    private BitMask _targets;

    public static readonly AOEShapeCone Shape = new(50f, 22.5f.Degrees());

    public Replication2HemorrhagicProjection(BossModule module)
        : base(module, centerAtTarget: true)
    {
        var staging = module.FindComponent<Replication2Staging>()!;
        _activation = WorldState.FutureTime(8.8d);

        var party = Raid.WithSlot(true, true, true);
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;

            var clone = staging.WurmsBySlot[slot];
            if (clone != null && clone.AssignedRole.IsCone)
                _targets.Set(slot);
        }
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        var party = Raid.WithSlot(true, true, true);
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            if (!_targets[i])
                continue;

            var target = Raid[i];
            if (target == null)
                continue;

            CurrentBaits.Add(new Bait(
                Module.PrimaryActor,
                target,
                Shape,
                _activation,
                forbidden: default,
                customRotation: target.Rotation,
                maxCasts: 1
            ));
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        var count = baits.Length;

        var isTarget = false;
        for (var i = 0; i < count; ++i)
        {
            if (baits[i].Target == actor)
            {
                isTarget = true;
                break;
            }
        }

        if (!isTarget)
            return;

        var forbidden = new ArcList(actor.Position, 60);

        var raid = Raid.WithoutSlot(true, true, true);
        var rlen = raid.Length;

        for (var i = 0; i < rlen; ++i)
        {
            var ally = raid[i];
            if (ally == actor)
                continue;

            var angle = actor.AngleTo(ally);
            forbidden.ForbidInfiniteCone(actor.Position, angle, 28f.Degrees());
        }

        var segments = forbidden.Forbidden.Segments;
        var slen = segments.Count;

        for (var i = 0; i < slen; ++i)
        {
            var (from, to) = segments[i];
            var center = (to + from) * 0.5f;
            var width = (to - from) * 0.5f;

            hints.ForbiddenDirections.Add((center.Radians(), width.Radians(), _activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.HemorrhagicProjection)
            return;

        ++NumCasts;
        _targets.Reset();
    }
}

sealed class Replication2ReenactmentOrder(BossModule module) : BossComponent(module)
{
    public readonly List<(Actor Understudy, CloneShape Shape, int Order)> Replay = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.Reenactment)
            return;

        Replay.Clear();

        var staging = Module.FindComponent<Replication2Staging>()!;
        var party = Raid.WithSlot(true);
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;
            var player = entry.Item2;

            var assignment = staging.PlayersBySlot[slot];
            if (assignment == null)
            {
                ReportError($"No clone assigned for raid member {player}");
                continue;
            }

            var clone = staging.WurmsBySlot[slot];
            var shape = clone?.Shape ?? CloneShape.Spread;
            var order = assignment.SpawnOrder;

            Replay.Add((assignment.Actor, shape, order));
        }

        var span = CollectionsMarshal.AsSpan(Replay);
        var count = span.Length;

        for (var i = 1; i < count; ++i)
        {
            var key = span[i];
            var j = i - 1;

            while (j >= 0 && span[j].Order > key.Order)
            {
                span[j + 1] = span[j];
                --j;
            }

            span[j + 1] = key;
        }
    }
}

sealed class Replication2ReenactmentAOEs(BossModule module) : Components.GenericAOEs(module)
{
    // We store nullable AOEInstance to preserve timing "holes"
    private readonly List<AOEInstance?> _predicted = [];

    public const float DefamDelay = 1.2f;
    public const float ConeDelay = 1.9f;
    public const float StackDelay = 1.6f;

    // Reusable buffer (max 4 visible at once)
    private readonly AOEInstance[] _active = new AOEInstance[4];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _predicted.Count;
        if (count == 0)
            return [];

        var span = CollectionsMarshal.AsSpan(_predicted);

        var visible = 0;
        var limit = Math.Min(4, count);

        for (var i = 0; i < limit; ++i)
        {
            var p = span[i];
            if (p == null)
                continue;

            var inst = p.Value;

            var risky = i < 2;
            inst.Risky = risky;
            inst.Color = risky ? Colors.Danger : Colors.AOE;

            _active[visible++] = inst;
        }

        return _active.AsSpan(0, visible);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.Reenactment)
            return;

        var mechStart = Module.CastFinishAt(spell, 8.2d);
        var order = Module.FindComponent<Replication2ReenactmentOrder>()!.Replay;

        var replaySpan = CollectionsMarshal.AsSpan(order);
        var len = replaySpan.Length;

        for (var i = 0; i < len; ++i)
        {
            var (actor, mechShape, spawnOrder) = replaySpan[i];
            var initialCast = mechStart.AddSeconds(4 * spawnOrder);

            AOEShape? shape = null;
            var delay = 0f;

            switch (mechShape)
            {
                case CloneShape.Boss:
                    shape = new AOEShapeCircle(5f);
                    break;

                case CloneShape.Spread:
                    shape = new AOEShapeCircle(20f);
                    delay = DefamDelay;
                    break;

                case CloneShape.Cone:
                    shape = Replication2HemorrhagicProjection.Shape;
                    delay = ConeDelay;
                    break;
            }

            if (shape != null)
            {
                var activation = initialCast.AddSeconds(delay);
                _predicted.Add(new AOEInstance(
                    shape,
                    actor.Position,
                    actor.Rotation,
                    activation
                ));
            }
            else
            {
                _predicted.Add(null);
            }
        }

        // Sort by activation time without LINQ
        _predicted.Sort(static (a, b) =>
        {
            if (a == null && b == null)
                return 0;
            if (a == null)
                return 1;
            if (b == null)
                return -1;
            return a.Value.Activation.CompareTo(b.Value.Activation);
        });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FirefallSplashReplay:
            case (uint)AID.ManaBurstReplay:
            case (uint)AID.HemorrhagicProjectionReplay:
                if (_predicted.Count > 0)
                {
                    _predicted.RemoveAt(0);
                    ++NumCasts;
                }
                break;

            case (uint)AID.HeavySlamReplay:
                if (_predicted.Count > 0)
                    _predicted.RemoveAt(0);
                break;
        }
    }
}
sealed class Replication2ReenactmentTowers(BossModule module) : Components.GenericTowers(module, (uint)AID.HeavySlamReplay)
{
    private readonly List<(WPos? Position, DateTime Activation)> _predicted = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.Reenactment)
            return;

        var mechStart = Module.CastFinishAt(spell, 8.2f);
        var replay = Module.FindComponent<Replication2ReenactmentOrder>()!.Replay;

        var span = CollectionsMarshal.AsSpan(replay);
        var len = span.Length;

        for (var i = 0; i < len; ++i)
        {
            var (actor, shape, order) = span[i];
            var initialCast = mechStart.AddSeconds(4 * order);

            if (shape == CloneShape.Stack)
            {
                _predicted.Add((
                    actor.Position,
                    initialCast.AddSeconds(Replication2ReenactmentAOEs.StackDelay)
                ));
            }
            else
            {
                _predicted.Add((null, initialCast));
            }
        }

        // Sort by activation without LINQ
        _predicted.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
    }

    public override void Update()
    {
        Towers.Clear();

        var count = _predicted.Count;
        if (count == 0)
            return;

        var span = CollectionsMarshal.AsSpan(_predicted);
        var limit = Math.Min(4, count);

        for (var i = 0; i < limit; ++i)
        {
            var (pos, activation) = span[i];
            if (pos == null)
                continue;

            // First two = safe color (0), later = danger color
            var color = i >= 2 ? Colors.Danger : default;

            Towers.Add(new(
                pos.Value,
                5,          // radius
                1,          // min players
                4,          // max players
                default,    // forbidden mask
                activation,
                color
            ));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.FirefallSplashReplay:
            case (uint)AID.ManaBurstReplay:
            case (uint)AID.HemorrhagicProjectionReplay:
                if (_predicted.Count > 0)
                    _predicted.RemoveAt(0);
                break;

            case (uint)AID.HeavySlamReplay:
                if (_predicted.Count > 0)
                {
                    _predicted.RemoveAt(0);
                    ++NumCasts;
                }
                break;
        }
    }
}

sealed class Replication2ReenactmentScaldingWaves(BossModule module) : Components.GenericBaitAway(module)
{
    private readonly List<(Actor? Source, DateTime Activation)> _predicted = [];
    private BitMask _targets;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID != (uint)AID.Reenactment)
            return;

        _targets = Module.FindComponent<Replication2ScaldingWaves>()!.Targets;

        var mechStart = Module.CastFinishAt(spell, 8.2d);
        var replay = Module.FindComponent<Replication2ReenactmentOrder>()!.Replay;

        var span = CollectionsMarshal.AsSpan(replay);
        var len = span.Length;

        for (var i = 0; i < len; ++i)
        {
            var (actor, shape, order) = span[i];
            var initialCast = mechStart.AddSeconds(4d * order);

            if (shape == CloneShape.Boss)
            {
                _predicted.Add((
                    actor,
                    initialCast.AddSeconds(Replication2ReenactmentAOEs.ConeDelay)
                ));
            }
            else
            {
                _predicted.Add((null, initialCast));
            }
        }

        // Sort by activation
        _predicted.Sort(static (a, b) => a.Activation.CompareTo(b.Activation));
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        var count = _predicted.Count;
        if (count == 0)
            return;

        var span = CollectionsMarshal.AsSpan(_predicted);
        var limit = Math.Min(2, count);

        for (var i = 0; i < limit; ++i)
        {
            var (source, activation) = span[i];
            if (source == null)
                continue;

            var raid = Raid.WithSlot(true, true, true);
            var raidLen = raid.Length;

            for (var j = 0; j < raidLen; ++j)
            {
                ref var entry = ref raid[j];
                var slot = entry.Item1;

                if (!_targets[slot])
                    continue;

                CurrentBaits.Add(new Bait(
                    source,
                    entry.Item2,
                    Replication2ScaldingWaves.Shape,
                    activation
                ));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.ManaBurstReplay:
            case (uint)AID.HemorrhagicProjectionReplay:
            case (uint)AID.HeavySlamReplay:
                if (_predicted.Count > 0)
                    _predicted.RemoveAt(0);
                break;

            case (uint)AID.ScaldingWavesReplay:
                _predicted.Clear();
                ++NumCasts;
                break;
        }
    }
}

sealed class Replication2TimelessSpite(BossModule module) : Components.UniformStackSpread(module, 6f, 0f, maxStackSize: 2)
{
    private DateTime _activation;
    private bool _far;
    public int NumCasts;
    private readonly Replication2Staging _staging = module.FindComponent<Replication2Staging>()!;
    private bool _netherwrathActive;

    public override void Update()
    {
        Stacks.Clear();

        if (_activation == default)
            return;

        var raid = Raid.WithoutSlot(true, true, true);
        var len = raid.Length;

        if (len == 0)
            return;

        var bossPos = Module.PrimaryActor.Position;

        // we need the 2 nearest or 2 farthest players
        // do a partial selection without full sort

        var first = -1;
        var second = -1;

        var best1 = _far ? float.MinValue : float.MaxValue;
        var best2 = _far ? float.MinValue : float.MaxValue;

        for (var i = 0; i < len; ++i)
        {
            var actor = raid[i];
            var dist = (actor.Position - bossPos).LengthSq();

            if (_far)
            {
                // looking for largest distances
                if (dist > best1)
                {
                    best2 = best1;
                    second = first;

                    best1 = dist;
                    first = i;
                }
                else if (dist > best2)
                {
                    best2 = dist;
                    second = i;
                }
            }
            else
            {
                // looking for smallest distances
                if (dist < best1)
                {
                    best2 = best1;
                    second = first;

                    best1 = dist;
                    first = i;
                }
                else if (dist < best2)
                {
                    best2 = dist;
                    second = i;
                }
            }
        }

        if (first >= 0)
            AddStack(raid[first], _activation);

        if (second >= 0)
            AddStack(raid[second], _activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var clone = _staging.WurmsBySlot[pcSlot];
        if (clone == null)
        {
            base.DrawArenaForeground(pcSlot, pc);
            return;
        }

        var role = clone.AssignedRole;
        var config = Service.Config.Get<M12S2LindwurmConfig>();
        var eff = config.GetReplication2();
        var isBCStrat = eff.RelativeNorth == Clockspot.W;

        // For BC strat during Netherwrath phase
        if (isBCStrat && _netherwrathActive && _activation != default)
        {
            // Show positioning hints based on role
            // Use actual boss position (the boss has been pulled to the W clockspot)
            var bossPos = Module.PrimaryActor.Position;
            var bossCardinal = eff.RelativeNorth.Angle; // West for BC
            var perpDir = bossCardinal.ToDirection().OrthoL(); // Perpendicular for left/right

            WPos targetPos;

            // BC Strat positioning for Netherwrath
            if (role.IsCone)
            {
                // Cone players stand near clones to either side of boss
                if (role == Replication2Role.Cone1)
                    targetPos = bossPos - 7f * perpDir; // Right side near clone
                else // Cone2
                    targetPos = bossPos + 7f * perpDir; // Left side near clone

                var isInPosition = pc.Position.InCircle(targetPos, 2f);
                Arena.ZoneCircleOutline(targetPos, 1f, isInPosition ? Colors.Safe : Colors.Vulnerable);
                return; // Don't show stack indicators
            }
            else if (role.IsStack)
            {
                // Stack players stand between cone players and in boss hitbox
                if (role == Replication2Role.Stack1)
                    targetPos = bossPos - 2f * perpDir; // Between Cone1 and boss
                else // Stack2
                    targetPos = bossPos + 2f * perpDir; // Between Cone2 and boss
            }
            else
            {
                // All other players (Boss, None, Defam) stand west of boss (outside Nether AOE range)
                targetPos = bossPos + 5f * 270.Degrees().ToDirection();
            }

            var inPosition = pc.Position.InCircle(targetPos, 2f);
            Arena.ZoneCircleOutline(targetPos, 1f, inPosition ? Colors.Safe : Colors.Vulnerable);
        }

        base.DrawArenaForeground(pcSlot, pc);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NetherwrathNear:
                _activation = Module.CastFinishAt(spell, 1.2d);
                _far = false;
                _netherwrathActive = true;
                break;

            case (uint)AID.NetherwrathFar:
                _activation = Module.CastFinishAt(spell, 1.2d);
                _far = true;
                _netherwrathActive = true;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TimelessSpite)
        {
            ++NumCasts;
            if (NumCasts >= 2)
            {
                _activation = default;
                _netherwrathActive = false;
            }
        }
    }
}
