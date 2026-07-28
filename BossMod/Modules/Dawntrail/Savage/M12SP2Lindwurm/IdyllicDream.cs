#pragma warning disable IDE0028
namespace BossMod.Dawntrail.Savage.M12S2Lindwurm;

sealed class IdyllicDreamStaging(BossModule module) : StagingAssignment<Replication3Role>(module, playerGroupSize: 4, cloneGroupSize: 2, hasBossTether: false)
{
    private static readonly M12S2LindwurmConfig _config = Service.Config.Get<M12S2LindwurmConfig>();

    public bool WurmsFinished;

    protected override Replication3Role DetermineCloneRole(WurmClone w)
    {
        var clock = Clockspot.GetClosest(w.Position);
        var spread = w.Shape == CloneShape.Spread;

        switch (clock)
        {
            case Clockspot.N:
            case Clockspot.NE:
                return spread ? Replication3Role.Defam1 : Replication3Role.Stack1;

            case Clockspot.E:
            case Clockspot.SE:
                return spread ? Replication3Role.Defam2 : Replication3Role.Stack2;

            case Clockspot.S:
            case Clockspot.SW:
                return spread ? Replication3Role.Defam3 : Replication3Role.Stack3;

            case Clockspot.W:
            case Clockspot.NW:
                return spread ? Replication3Role.Defam4 : Replication3Role.Stack4;

            default:
                ReportError($"Unexpected clockspot {clock} for clone {w.Actor}");
                return spread ? Replication3Role.Defam1 : Replication3Role.Stack1;
        }
    }

    protected override IEnumerable<string> GetHelpHints(int slot, Actor actor)
    {
        if (WurmsFinished)
            return [];

        var wurm = WurmsBySlot[slot];
        if (wurm != null)
        {
            var group = wurm.Position.Deg > 0 ? "N" : "S";
            return [$"Clone: {wurm.Shape}, order {wurm.SpawnOrder + 1}, group {group}"];
        }

        return base.GetHelpHints(slot, actor);
    }

    protected override Replication3Role? DeterminePlayerRole(PlayerClone c)
    {
        var eff = _config.GetReplication3();

        var clock = Clockspot.GetClosest(c.Position);
        return eff.GetRole(clock);
    }
}

sealed class IdyllicDreamPowerGusherSnakingKick(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = new();
    readonly List<AOEInstance> _portaled = new();

    public bool Visible;
    public bool Risky;
    public bool WatchTeleport;

    public void Reset() => NumCasts = 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Visible || _predicted.Count == 0)
            return default;

        if (Risky)
        {
            var span = CollectionsMarshal.AsSpan(_predicted);
            for (var i = 0; i < span.Length; ++i)
            {
                if (!span[i].Risky)
                    span[i] = span[i] with { Risky = true };
            }
        }
        return CollectionsMarshal.AsSpan(_predicted);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.PowerGusherAOEVisual => new AOEShapeCone(60f, 45f.Degrees()),
            (uint)AID.SnakingKickCastFirst => new AOEShapeCircle(10f),
            _ => null
        };

        if (shape != null)
        {
            _predicted.Add(new(
                shape,
                spell.LocXZ,
                spell.Rotation,
                WorldState.FutureTime(29.3d)
            ));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var aid = spell.Action.ID;

        if (aid is (uint)AID.PowerGusher or (uint)AID.IdyllicDreamSnakingKick)
        {
            ++NumCasts;
            Visible = false;

            if (WatchTeleport)
                _predicted.Clear();
        }

        if (!WatchTeleport)
            return;

        if (aid == (uint)AID.TemporalTearEnter)
        {
            var pos = caster.Position;

            for (var i = _predicted.Count - 1; i >= 0; --i)
            {
                if (_predicted[i].Origin.AlmostEqual(pos, 1))
                {
                    _portaled.Add(_predicted[i]);
                    _predicted.RemoveAt(i);
                }
            }
        }
        else if (aid == (uint)AID.TemporalTearExit)
        {
            var newPos = spell.TargetXZ;

            var span = CollectionsMarshal.AsSpan(_portaled);
            for (var i = 0; i < span.Length; ++i)
                _predicted.Add(span[i] with { Origin = newPos });

            _portaled.Clear();
        }
        else if (aid == (uint)AID.Dash)
        {
            var oldPos = caster.Position;
            var newPos = spell.TargetXZ;

            var span = CollectionsMarshal.AsSpan(_predicted);
            for (var i = 0; i < span.Length; ++i)
            {
                if (span[i].Origin.AlmostEqual(oldPos, 1))
                    span[i] = span[i] with { Origin = newPos };
            }
        }
    }
}
sealed class IdyllicDreamWurmStackSpread : Components.UniformStackSpread
{
    public readonly List<(Actor Target, CloneShape Shape, DateTime Activation)> Stored = new();
    public int NumCasts;
    const float SpreadDelay = 1.2f;

    public IdyllicDreamWurmStackSpread(BossModule module) : base(module, 5f, 20f, 3)
    {
        var staging = module.FindComponent<IdyllicDreamStaging>();
        if (staging == null)
        {
            ReportError("IdyllicDreamWurmStackSpread: staging component not found");
            return;
        }

        var firstCast = module.WorldState.FutureTime(10.5f);

        // Copy to avoid mutating staging state
        var clones = new List<IdyllicDreamStaging.WurmClone>(staging.WurmClones);
        clones.Sort((a, b) => a.SpawnOrder.CompareTo(b.SpawnOrder));

        var count = clones.Count;
        for (var i = 0; i < count; ++i)
        {
            var clone = clones[i];

            if (clone.Target == null || clone.Shape == null)
            {
                ReportError($"{clone.Actor} is invalid, this should never happen");
                continue;
            }

            var baseDelay = clone.SpawnOrder * 5f;
            var extraDelay = clone.Shape == CloneShape.Spread ? SpreadDelay : 0f;

            Stored.Add((
                clone.Target,
                clone.Shape.Value,
                firstCast.AddSeconds(baseDelay + extraDelay)
            ));
        }
    }

    // Suppress default UniformStackSpread hints
    public override void AddHints(int slot, Actor actor, TextHints hints) { }

    public override void Update()
    {
        Stacks.Clear();
        Spreads.Clear();

        var count = Stored.Count;
        if (count == 0)
            return;

        var limit = count > 2 ? 2 : count;

        for (var i = 0; i < limit; ++i)
        {
            var entry = Stored[i];

            if (entry.Shape == CloneShape.Spread)
                AddSpread(entry.Target, entry.Activation);
            else
                AddStack(entry.Target, entry.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var aid = spell.Action.ID;

        if (aid is (uint)AID.IdyllicDreamManaBurstVisual or (uint)AID.IdyllicDreamHeavySlam)
        {
            ++NumCasts;
        }
        if (aid is (uint)AID.IdyllicDreamManaBurst or (uint)AID.IdyllicDreamHeavySlam)
        {
            if (Stored.Count > 0)
                Stored.RemoveAt(0);
        }
    }
}

sealed class IdyllicDreamManaBurstPlayer(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    public bool Risky;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _predicted.Count;
        if (count == 0)
            return default;

        if (Risky)
        {
            var span = CollectionsMarshal.AsSpan(_predicted);
            for (var i = 0; i < span.Length; ++i)
            {
                if (!span[i].Risky)
                    span[i] = span[i] with { Risky = true };
            }
        }

        return CollectionsMarshal.AsSpan(_predicted);
    }

    public void Predict(int spawnOrder)
    {
        var staging = Module.FindComponent<IdyllicDreamStaging>();
        if (staging == null)
        {
            ReportError("IdyllicDreamManaBurstPlayer: staging component not found");
            return;
        }

        var party = Module.Raid.WithSlot(includeDead: true);
        var count = party.Length;

        for (var i = 0; i < count; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;
            var player = entry.Item2;

            var playerClone = staging.PlayersBySlot[slot];
            if (playerClone == null)
            {
                ReportError($"Player {player} has no assigned clone");
                continue;
            }

            var wurmClone = staging.WurmsBySlot[slot];
            if (wurmClone == null)
            {
                ReportError($"Player {player} has no clone tether");
                continue;
            }

            if (playerClone.SpawnOrder != spawnOrder)
                continue;

            if (wurmClone.Shape != CloneShape.Spread)
                continue;

            _predicted.Add(new AOEInstance(
                new AOEShapeCircle(20f),
                playerClone.Actor.Position,
                default,
                Module.WorldState.FutureTime(10d) // TODO: correct activation timing
            ));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.ManaBurstReplay)
            return;

        var target = Module.WorldState.Actors.Find(spell.MainTargetID);
        if (target == null)
            return;

        var pos = target.Position;

        for (var i = _predicted.Count - 1; i >= 0; --i)
        {
            if (_predicted[i].Origin.AlmostEqual(pos, 1f))
                _predicted.RemoveAt(i);
        }

        NumCasts++;
    }
}

sealed class IdyllicDreamHeavySlamPlayer(BossModule module) : Components.GenericTowers(module)
{
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        // intentionally suppress default tower hints
    }

    public void Predict(int spawnOrder)
    {
        var staging = Module.FindComponent<IdyllicDreamStaging>();
        if (staging == null)
        {
            ReportError("IdyllicDreamHeavySlamPlayer: staging component not found");
            return;
        }

        var party = Module.Raid.WithSlot(includeDead: true);
        var count = party.Length;

        for (var i = 0; i < count; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;
            var player = entry.Item2;

            var playerClone = staging.PlayersBySlot[slot];
            if (playerClone == null)
            {
                ReportError($"Player {player} has no assigned clone");
                continue;
            }

            var wurmClone = staging.WurmsBySlot[slot];
            if (wurmClone == null)
            {
                ReportError($"Player {player} has no clone tether");
                continue;
            }

            if (playerClone.SpawnOrder != spawnOrder)
                continue;

            if (wurmClone.Shape != CloneShape.Stack)
                continue;

            Towers.Add(new(
                playerClone.Actor.Position,
                5f,
                2,
                6,
                activation: Module.WorldState.FutureTime(10f) // TODO: correct timing
            ));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID != (uint)AID.HeavySlamReplay)
            return;

        var pos = caster.Position;

        for (var i = Towers.Count - 1; i >= 0; --i)
        {
            if (Towers[i].Position.AlmostEqual(pos, 1f))
                Towers.RemoveAt(i);
        }

        ++NumCasts;
    }
}

sealed class IdyllicDreamPlayerCastCounter(BossModule module) : Components.CastCounterMulti(module, [(uint)AID.ManaBurstReplay, (uint)AID.HeavySlamReplay]);