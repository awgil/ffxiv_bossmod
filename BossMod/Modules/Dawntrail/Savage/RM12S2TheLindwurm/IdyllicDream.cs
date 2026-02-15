namespace BossMod.Dawntrail.Savage.RM12S2TheLindwurm;

class IdyllicDreamStaging(BossModule module) : StagingAssignment<Replication3Role>(module, playerGroupSize: 4, cloneGroupSize: 2, hasBossTether: false)
{
    readonly RM12S2TheLindwurmConfig _config = module.Config.Get<RM12S2TheLindwurmConfig>();

    public bool WurmsFinished;

    protected override Replication3Role DetermineCloneRole(WurmClone w)
    {
        return Clockspot.GetClosest(w.Position) switch
        {
            Clockspot.N or Clockspot.NE => w.Shape == CloneShape.Spread ? Replication3Role.Defam1 : Replication3Role.Stack1,
            Clockspot.E or Clockspot.SE => w.Shape == CloneShape.Spread ? Replication3Role.Defam2 : Replication3Role.Stack2,
            Clockspot.S or Clockspot.SW => w.Shape == CloneShape.Spread ? Replication3Role.Defam3 : Replication3Role.Stack3,
            Clockspot.W or Clockspot.NW => w.Shape == CloneShape.Spread ? Replication3Role.Defam4 : Replication3Role.Stack4,
            _ => throw new NotImplementedException()
        };
    }

    protected override IEnumerable<string> GetHelpHints(int slot, Actor actor)
    {
        if (WurmsFinished)
            yield break;

        if (WurmsBySlot[slot] is { } w)
        {
            yield return $"Clone: {w.Shape}, order {w.SpawnOrder + 1}, group {(w.Position.Deg > 0 ? "N" : "S")}";
        }
        else
            foreach (var h in base.GetHelpHints(slot, actor))
                yield return h;
    }

    protected override Replication3Role? DeterminePlayerRole(PlayerClone c)
    {
        if (!_config.Rep3Assignments.IsValid())
            return null;

        return _config.Rep3Assignments[Clockspot.GetClosest(c.Position)];
    }
}

class IdyllicDreamPowerGusherSnakingKick(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];
    readonly List<AOEInstance> _portaled = [];

    public bool Visible;
    public bool Risky;

    public bool WatchTeleport;

    public void Reset()
    {
        NumCasts = 0;
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Visible)
            yield break;

        foreach (var p in _predicted)
            yield return p with { Risky = Risky };
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.PowerGusherAOEVisual => new AOEShapeCone(60, 45.Degrees()),
            AID.SnakingKickCastFirst => new AOEShapeCircle(10),
            _ => null
        };
        if (shape != null)
            _predicted.Add(new(shape, spell.LocXZ, spell.Rotation, WorldState.FutureTime(29.3f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PowerGusher or AID.IdyllicDreamSnakingKick)
        {
            NumCasts++;
            Visible = false;

            if (WatchTeleport)
                _predicted.Clear();
        }

        if (WatchTeleport)
        {
            if ((AID)spell.Action.ID == AID.TemporalTearEnter)
            {
                _portaled.AddRange(_predicted.Where(p => p.Origin.AlmostEqual(caster.Position, 1)));
                _predicted.RemoveAll(p => p.Origin.AlmostEqual(caster.Position, 1));
            }

            if ((AID)spell.Action.ID == AID.TemporalTearExit)
            {
                _predicted.AddRange(_portaled.Select(p => p with { Origin = spell.TargetXZ }));
                _portaled.Clear();
            }

            if ((AID)spell.Action.ID == AID.Dash)
            {
                for (var i = 0; i < _predicted.Count; i++)
                {
                    if (_predicted[i].Origin.AlmostEqual(caster.Position, 1))
                        _predicted.Ref(i).Origin = spell.TargetXZ;
                }
            }
        }
    }
}

class IdyllicDreamWurmStackSpread : Components.UniformStackSpread
{
    public readonly List<(Actor Target, CloneShape Shape, DateTime Activation)> Stored = [];

    public int NumCasts;

    const float SpreadDelay = 1.2f;

    public IdyllicDreamWurmStackSpread(BossModule module) : base(module, 5, 20, 3, alwaysShowSpreads: true)
    {
        EnableHints = false;

        var firstCast = WorldState.FutureTime(10.5f);

        foreach (var clone in Module.FindComponent<IdyllicDreamStaging>()!.WurmClones.OrderBy(c => c.SpawnOrder))
        {
            if (clone.Target == null || clone.Shape == null)
            {
                ReportError($"{clone.Actor} is invalid, this should never happen");
                continue;
            }
            Stored.Add((clone.Target, clone.Shape.Value, firstCast.AddSeconds(clone.SpawnOrder * 5 + clone.Shape == CloneShape.Spread ? SpreadDelay : 0)));
        }
    }

    public override void Update()
    {
        Stacks.Clear();
        Spreads.Clear();

        foreach (var (t, sh, a) in Stored.Take(2))
        {
            if (sh == CloneShape.Spread)
                AddSpread(t, a);
            else
                AddStack(t, a);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.IdyllicDreamManaBurstVisual or AID.IdyllicDreamHeavySlam)
        {
            NumCasts++;
        }

        if ((AID)spell.Action.ID is AID.IdyllicDreamManaBurst or AID.IdyllicDreamHeavySlam)
        {
            if (Stored.Count > 0)
                Stored.RemoveAt(0);
        }
    }
}

class IdyllicDreamManaBurstPlayer(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    public bool Risky;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Select(p => p with { Risky = Risky });

    public void Predict(int spawnOrder)
    {
        var stg = Module.FindComponent<IdyllicDreamStaging>()!;

        foreach (var (i, player) in Raid.WithSlot(includeDead: true))
        {
            if (stg.PlayersBySlot[i] is not { } pc)
            {
                ReportError($"Player {player} has no assigned clone");
                continue;
            }
            if (stg.WurmsBySlot[i] is not { } pw)
            {
                ReportError($"Player {player} has no clone tether");
                continue;
            }

            if (pc.SpawnOrder != spawnOrder || pw.Shape != CloneShape.Spread)
                continue;

            // TODO: fix activation time
            _predicted.Add(new(new AOEShapeCircle(20), pc.Actor.Position, default, WorldState.FutureTime(10)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ManaBurstReplay)
        {
            var targetpos = WorldState.Actors.Find(spell.MainTargetID)?.Position ?? default;
            _predicted.RemoveAll(p => p.Origin.AlmostEqual(targetpos, 1));
            NumCasts++;
        }
    }
}

class IdyllicDreamHeavySlamPlayer : Components.GenericTowers
{
    public IdyllicDreamHeavySlamPlayer(BossModule module) : base(module)
    {
        EnableHints = false;
    }

    public void Predict(int spawnOrder)
    {
        var stg = Module.FindComponent<IdyllicDreamStaging>()!;

        foreach (var (i, player) in Raid.WithSlot(includeDead: true))
        {
            if (stg.PlayersBySlot[i] is not { } pc)
            {
                ReportError($"Player {player} has no assigned clone");
                continue;
            }
            if (stg.WurmsBySlot[i] is not { } pw)
            {
                ReportError($"Player {player} has no clone tether");
                continue;
            }

            if (pc.SpawnOrder != spawnOrder || pw.Shape != CloneShape.Stack)
                continue;

            // TODO: fix activation time
            Towers.Add(new(pc.Actor.Position, 5, 2, 6, activation: WorldState.FutureTime(10)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HeavySlamReplay)
        {
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
            NumCasts++;
        }
    }
}

class IdyllicDreamPlayerCastCounter(BossModule module) : Components.CastCounterMulti(module, [AID.ManaBurstReplay, AID.HeavySlamReplay]);
