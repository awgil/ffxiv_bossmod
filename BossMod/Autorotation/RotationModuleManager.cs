using FFXIVClientStructs.FFXIV.Common.Component.BGCollision;

namespace BossMod.Autorotation;

public interface IRotationModuleData
{
    public Type Type { get; }
    public RotationModuleDefinition Definition { get; }
    public Func<RotationModuleManager, Actor, RotationModule> Builder { get; }
}

// the manager contains a set of rotation module instances corresponding to the selected preset/plan
public sealed class RotationModuleManager : IDisposable
{
    private readonly record struct ActiveModule(int DataIndex, RotationModuleDefinition Definition, RotationModule Module);
    public readonly record struct LineOfSightFix(ulong TargetID, WPos Origin, WPos Destination);

    public readonly List<Preset> Presets = [];

    public readonly AutorotationConfig Config = Service.Config.Get<AutorotationConfig>();
    public readonly RotationDatabase Database;
    public readonly BossModuleManager Bossmods;
    public int PlayerSlot; // TODO: reconsider, we rely on too many things in clientstate...
    public readonly AIHints Hints;
    public PlanExecution? Planner { get; private set; }
    private readonly PartyRolesConfig _prc = Service.Config.Get<PartyRolesConfig>();
    private readonly EventSubscriptions _subscriptions;
    private List<(int index, ActiveModule module)>? _activeModules;
    private IEnumerable<ActiveModule> ActiveModulesFlat => _activeModules?.Select(m => m.module) ?? [];
    private bool WantsLoSFix => ActiveModulesFlat.Any(m => m.Module.WantsLoSFix);

    public static readonly Preset ForceDisable = new(""); // empty preset, so if it's activated, rotation is force disabled

    public bool IsForceDisabled => Presets.Count == 1 && Presets[0] == ForceDisable;

    public string PresetNames => Presets.Count > 0 ? string.Join(", ", Presets.Select(p => p.Name)) : "<n/a>";

    public WorldState WorldState => Bossmods.WorldState;
    public ulong PlayerInstanceId => WorldState.Party.Members[PlayerSlot].InstanceId;
    public Actor? Player => WorldState.Party[PlayerSlot];

    // historic data for recent events that could be interesting for modules
    public DateTime CombatStart { get; private set; } // default value when player is not in combat, otherwise timestamp when player entered combat
    public (DateTime Time, ActorCastEvent? Data) LastCast { get; private set; }
    public LineOfSightFix? LoSFix { get; private set; }

    public volatile float LastRasterizeMs;
    public volatile float LastPathfindMs;

    // list of status effects that disable the player's default action set, but do not disable *all* actions
    // in these cases, we want to prevent active rotation modules from queueing any actions, because they might affect positioning or rotation, or interfere with player's attempt to manually use an action
    // TODO can this be sourced entirely from sheet data? i can't find a field that uniquely identifies these statuses while excluding "stuns" and transformations that do not inhibit the use of actions
    public static readonly uint[] TransformationStatuses = [
        (uint)Roleplay.SID.RolePlaying, // used for almost all solo duties
        (uint)Roleplay.SID.BorrowedFlesh, // used specifically for In from the Cold (Endwalker)
        (uint)Roleplay.SID.FreshPerspective, // sapphire weapon quest

        // hacking interlude gimmick in Paradigm's Breach boss 3
        (uint)Shadowbringers.Alliance.A34RedGirl.SID.Program000000,
        (uint)Shadowbringers.Alliance.A34RedGirl.SID.ProgramFFFFFFF,

        565, // "Transfiguration" from certain pomanders in Palace of the Dead
        439, // "Toad", palace of the dead
        1546, // "Odder", heaven-on-high
        3502, // "Owlet", EO
        404, // "Transporting", not a transformation but prevents actions
        4235, // "Rage" status from Phantom Berserker, prevents all actions and movement
        4376, // "Transporting", variant in Occult Crescent
        4586, // "Away with the Fae", PT
        4708, // "Transfiguration", PT
    ];

    public static bool IsTransformStatus(ActorStatus st) => TransformationStatuses.Contains(st.ID);

    public RotationModuleManager(RotationDatabase db, BossModuleManager bmm, AIHints hints, int playerSlot = PartyState.PlayerSlot)
    {
        Database = db;
        Bossmods = bmm;
        PlayerSlot = playerSlot;
        Hints = hints;
        _subscriptions = new
        (
            WorldState.Actors.Added.Subscribe(a => DirtyActiveModules(PlayerInstanceId == a.InstanceID)),
            WorldState.Actors.Removed.Subscribe(a => DirtyActiveModules(PlayerInstanceId == a.InstanceID)),
            WorldState.Actors.ClassChanged.Subscribe(a => DirtyActiveModules(PlayerInstanceId == a.InstanceID)),
            WorldState.Actors.InCombatChanged.Subscribe(OnCombatChanged),
            WorldState.Actors.IsDeadChanged.Subscribe(OnDeadChanged),
            WorldState.Actors.CastEvent.Subscribe(OnCastEvent),
            WorldState.Actors.StatusGain.Subscribe((a, idx) => DirtyActiveModules(PlayerInstanceId == a.InstanceID && IsTransformStatus(a.Statuses[idx]))),
            WorldState.Actors.StatusLose.Subscribe((a, idx) => DirtyActiveModules(PlayerInstanceId == a.InstanceID && IsTransformStatus(a.Statuses[idx]))),
            WorldState.Party.Modified.Subscribe(op => DirtyActiveModules(op.Slot == PlayerSlot)),
            WorldState.Client.ActionRequested.Subscribe(OnActionRequested),
            WorldState.Client.CountdownChanged.Subscribe(OnCountdownChanged),
            WorldState.Client.ActionFailedLoS.Subscribe(OnLoSFailed),
            Database.Presets.PresetModified.Subscribe(OnPresetModified)
        );
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    public void Update(float estimatedAnimLockDelay, bool isMoving, bool dutyRecorder)
    {
        // see whether current plan matches what should be active, and update if not; only rebuild actions if there is no active override
        var expectedPlan = CalculateExpectedPlan();
        if (Planner?.Module != Bossmods.ActiveModule || Planner?.Plan != expectedPlan)
        {
            Service.Log($"[RMM] Changing active plan: '{Planner?.Plan?.Guid}' -> '{expectedPlan?.Guid}'");
            Planner = Bossmods.ActiveModule != null ? new(Bossmods.ActiveModule, expectedPlan) : null;
            DirtyActiveModules(Presets.Count == 0);
        }

        // rebuild modules if needed
        _activeModules ??= Presets.Count > 0 ? [.. Presets.SelectMany((p, i) => RebuildActiveModules(p.Modules, i))] : Planner?.Plan != null ? RebuildActiveModules(Planner.Plan.Modules, 0) : [];

        _activeModules?.SortBy(m => m.module.Definition.Order);

        // trying to change target or use actions is a waste of cpu cycles during duty recorder playback
        if (dutyRecorder)
            return;

        // forced target update
        if (Hints.ForcedTarget == null && Presets.Count == 0 && Planner?.ActiveForcedTarget() is var forced && forced != null)
        {
            Hints.ForcedTarget = forced.Target != StrategyTarget.Automatic
                ? ResolveTargetOverride(forced.Target, forced.TargetParam)
                : (ResolveTargetOverride(StrategyTarget.EnemyWithHighestPriority, 0) ?? Bossmods.ActiveModule?.GetDefaultTarget(PlayerSlot));
        }

        // auto actions
        var target = Hints.ForcedTarget ?? WorldState.Actors.Find(Player?.TargetID ?? 0);
        foreach (var (slot, m) in _activeModules ?? [])
        {
            var values = Presets.BoundSafeAt(slot)?.ActiveStrategyOverrides(m.DataIndex) ?? Planner?.ActiveStrategyOverrides(m.DataIndex) ?? throw new InvalidOperationException("Both preset and plan are null, but there are active modules");
            m.Module.Execute(values, ref target, estimatedAnimLockDelay, isMoving);
        }
    }

    public Actor? ResolveTargetOverride(StrategyTarget strategy, int param) => strategy switch
    {
        StrategyTarget.Self => Player,
        StrategyTarget.PartyByAssignment => _prc.SlotsPerAssignment(WorldState.Party) is var spa && param < spa.Length ? WorldState.Party[spa[param]] : null,
        StrategyTarget.PartyWithLowestHP => FilteredPartyMembers((StrategyPartyFiltering)param).MinBy(a => a.HPMP.CurHP),
        StrategyTarget.EnemyWithHighestPriority => Hints.PriorityTargets.MaxBy(RateEnemy((StrategyEnemySelection)param))?.Actor,
        StrategyTarget.EnemyByOID => Player != null && (uint)param is var oid && oid != 0 ? Hints.PotentialTargets.Where(e => e.Actor.OID == oid).MinBy(e => (e.Actor.Position - Player.Position).LengthSq())?.Actor : null,
        _ => null
    };

    public WPos ResolveTargetLocation(StrategyTarget strategy, int param, float off1, float off2) => strategy switch
    {
        StrategyTarget.PointAbsolute => new(off1, off2),
        StrategyTarget.PointWaymark => WorldState.Waymarks[(Waymark)param] is var wm && wm != null ? new WPos(wm.Value.XZ()) + off1 * off2.Degrees().ToDirection() : default,
        StrategyTarget.PointCenter or StrategyTarget.Automatic => (Bossmods.ActiveModule?.Center + off1 * off2.Degrees().ToDirection()) ?? Player?.Position ?? default,
        _ => (ResolveTargetOverride(strategy, param)?.Position + off1 * off2.Degrees().ToDirection()) ?? Player?.Position ?? default,
    };

    public override string ToString() => string.Join(", ", ActiveModulesFlat.Select(m => m.Module.GetType().Name));

    public IEnumerable<Type> DuplicateModules => ActiveModulesFlat.GroupBy(m => m.Module.GetType()).Where(x => x.Skip(1).Any()).Select(p => p.Key);

    public void Activate(Preset p, bool exclusive = false)
    {
        var dirty = Presets.Remove(ForceDisable);
        if (exclusive)
            Presets.Clear();
        if (!Presets.Contains(p))
        {
            Presets.Add(p);
            dirty = true;
        }
        DirtyActiveModules(dirty);
    }

    public void Deactivate(Preset p)
    {
        if (Presets.Remove(p))
            DirtyActiveModules(true);
    }

    public void Toggle(Preset p, bool exclusive = false)
    {
        if (!Presets.Remove(p))
        {
            if (exclusive)
                Presets.Clear();
            Presets.Add(p);
        }
        if (p != ForceDisable)
            Presets.Remove(ForceDisable);
        DirtyActiveModules(true);
    }

    public void Clear()
    {
        Presets.Clear();
        DirtyActiveModules(true);
    }

    public void SetForceDisabled()
    {
        Presets.Clear();
        Presets.Add(ForceDisable);
        DirtyActiveModules(true);
    }

    private IEnumerable<Actor> FilteredPartyMembers(StrategyPartyFiltering filter)
    {
        var fullMask = new BitMask(~0ul);
        var allowedMask = fullMask;
        if (!filter.HasFlag(StrategyPartyFiltering.IncludeSelf))
            allowedMask.Clear(PlayerSlot);
        if (filter.HasFlag(StrategyPartyFiltering.ExcludeNoPredictedDamage))
        {
            var predictedDamage = Hints.PredictedDamage.Aggregate(default(BitMask), (s, p) => s | p.Players);
            allowedMask &= predictedDamage;
        }

        if (allowedMask.None())
            return [];
        var players = allowedMask != fullMask ? WorldState.Party.WithSlot().IncludedInMask(allowedMask).Actors() : WorldState.Party.WithoutSlot();
        if ((filter & (StrategyPartyFiltering.ExcludeTanks | StrategyPartyFiltering.ExcludeHealers | StrategyPartyFiltering.ExcludeMelee | StrategyPartyFiltering.ExcludeRanged)) != StrategyPartyFiltering.None)
        {
            players = players.Where(p => p.Role switch
            {
                Role.Tank => !filter.HasFlag(StrategyPartyFiltering.ExcludeTanks),
                Role.Healer => !filter.HasFlag(StrategyPartyFiltering.ExcludeHealers),
                Role.Melee => !filter.HasFlag(StrategyPartyFiltering.ExcludeMelee),
                Role.Ranged => !filter.HasFlag(StrategyPartyFiltering.ExcludeRanged),
                _ => true,
            });
        }
        return players;
    }

    private Func<AIHints.Enemy, float> RateEnemy(StrategyEnemySelection criterion) => criterion switch
    {
        StrategyEnemySelection.Closest => Player != null ? e => -(e.Actor.Position - Player.Position).LengthSq() : _ => 0,
        StrategyEnemySelection.LowestCurHP => e => -e.Actor.HPMP.CurHP,
        StrategyEnemySelection.HighestCurHP => e => e.Actor.HPMP.CurHP,
        StrategyEnemySelection.LowestMaxHP => e => -e.Actor.HPMP.MaxHP,
        StrategyEnemySelection.HighestMaxHP => e => e.Actor.HPMP.MaxHP,
        _ => _ => 0
    };

    private Plan? CalculateExpectedPlan()
    {
        var player = Player;
        if (player == null || Bossmods.ActiveModule == null)
            return null; // nothing loaded/active, so no plan
        if (Bossmods.ActiveModule.StateMachine.ActiveState == null && WorldState.Client.CountdownRemaining == null)
            return null; // neither pull nor prepull
        var plans = Database.Plans.GetPlans(Bossmods.ActiveModule.GetType(), player.Class);
        return plans.SelectedIndex >= 0 ? plans.Plans[plans.SelectedIndex] : null;
    }

    // TODO: consider not recreating modules that were active and continue to be active?
    private List<(int, ActiveModule)> RebuildActiveModules<T>(List<T> modules, int index) where T : IRotationModuleData
    {
        List<(int, ActiveModule)> res = [];
        var player = Player;
        if (player != null)
        {
            var isRPMode = player.Statuses.Any(IsTransformStatus);
            for (int i = 0; i < modules.Count; ++i)
            {
                var def = modules[i].Definition;
                if (!def.Classes[(int)player.Class] || player.Level < def.MinLevel || player.Level > def.MaxLevel)
                    continue;
                if (!def.CanUseWhileRoleplaying && isRPMode)
                    continue;
                res.Add((index, new(i, def, modules[i].Builder(this, player))));
            }
        }
        return res;
    }

    private void DirtyActiveModules(bool condition)
    {
        if (condition)
        {
            LastRasterizeMs = 0;
            LastPathfindMs = 0;
            _activeModules = null;
        }
    }

    private void OnCombatChanged(Actor actor)
    {
        if (PlayerInstanceId != actor.InstanceID)
            return; // don't care

        CombatStart = actor.InCombat ? WorldState.CurrentTime : default; // keep track of combat time in case rotation modules want to do something special in openers

        if (!actor.InCombat && (IsForceDisabled ? Config.ClearForceDisableOnCombatEnd : Config.ClearPresetOnCombatEnd))
        {
            // player exits combat => clear manual overrides
            Service.Log($"[RMM] Player exits combat => clear presets '{PresetNames}'");
            Clear();
        }
        else if (actor.InCombat && WorldState.Client.CountdownRemaining > Config.EarlyPullThreshold)
        {
            // player enters combat while countdown is in progress => force disable
            Service.Log($"[RMM] Player ninja pulled => force-disabling from '{PresetNames}'");
            SetForceDisabled();
        }
        // if player enters combat when countdown is either not active or around zero, proceed normally - if override is queued, let it run, otherwise let plan run
    }

    private void OnDeadChanged(Actor actor)
    {
        if (PlayerInstanceId != actor.InstanceID)
            return; // don't care

        // note: if combat ends while player is dead, we'll reset the preset, which is desirable
        if (actor.IsDead && actor.InCombat && Config.ClearPresetOnDeath)
        {
            // player died in combat => force disable (otherwise there's a risk of dying immediately after rez)
            Service.Log($"[RMM] Player died in combat => force-disabling from '{PresetNames}'");
            SetForceDisabled();
        }
        // else: player either died outside combat (no need to touch anything) or rez'd (unless player cleared override, we stay in force disable mode)
    }

    private void OnCountdownChanged(ClientState.OpCountdownChange op)
    {
        if (op.Value == null && !(Player?.InCombat ?? false))
        {
            // countdown ended and player is not in combat - so either it was cancelled, or pull didn't happen => clear manual overrides
            // note that if pull will happen regardless after this, we'll start executing plan normally (without prepull part)
            Service.Log($"[RMM] Countdown expired or aborted => clear presets '{PresetNames}'");
            Clear();
        }
    }

    private void OnPresetModified(Preset? prev, Preset? curr)
    {
        var wasActive = prev != null && Presets.Any(p => p.Name == prev.Name);

        if (prev != null)
            Deactivate(prev);
        if (wasActive && curr != null)
            Activate(curr);
    }

    private void OnActionRequested(ClientState.OpActionRequest op)
    {
        if (Service.IsDev)
            Service.Log($"[RMM] Exec #{op.Request.SourceSequence} {op.Request.Action} @ {op.Request.TargetID:X} [{string.Join(" --- ", ActiveModulesFlat.Select(m => m.Module.DescribeState()))}]");
    }

    private void OnCastEvent(Actor actor, ActorCastEvent cast)
    {
        if (cast.SourceSequence != 0 && WorldState.Party.Members[PlayerSlot].InstanceId == actor.InstanceID)
        {
            LastCast = (WorldState.CurrentTime, cast);
            if (Service.IsDev)
                Service.Log($"[RMM] Cast #{cast.SourceSequence} {cast.Action} @ {cast.MainTargetID:X} [{string.Join(" --- ", ActiveModulesFlat.Select(m => m.Module.DescribeState()))}]");
        }

        if (cast.Action.ID == 6276 && Config.ClearPresetOnLuring)
        {
            Service.Log($"[RMM] Luring Trap triggered, force-disabling from '{PresetNames}'");
            SetForceDisabled();
        }

        if (actor.InstanceID == PlayerInstanceId)
        {
            LoSFix = null; // successful cast means we're not stuck anymore
        }
    }

    private void OnLoSFailed(ClientState.OpActionFailedLoS op)
    {
        if (!WantsLoSFix)
        {
            LoSFix = null;
            return;
        }

        if (Hints.PathfindMapObstacles.Bitmap is null)
            return;

        // don't reevaluate if there's one fix for current target. Performance cost is huge
        if (LoSFix?.TargetID == op.TargetId)
            return;

        if (Player is null || WorldState.Actors.Find(op.TargetId) is not { } target)
        {
            LoSFix = null;
            return;
        }

        var dest = FindLosDestination(Hints.PathfindMapObstacles, Hints.PathfindMapCenter, Player, target, out var debug);
        if (Service.IsDev)
        {
            Service.Log($"[RMM] LoS failed for action {op.ActionId} on target {target.Name}#{op.TargetId:X}, setting LoS destination to {dest}");
            Service.Log($"[RMM] {debug}");
        }
        LoSFix = dest != null ? new(op.TargetId, Player.Position, dest.Value) : null;
    }

    private static WPos? FindLosDestination(Bitmap.Region obstacleRegion, WPos mapCenter, Actor player, Actor target, out string debug)
    {
        var map = obstacleRegion.Bitmap!;
        var w = map.Width;
        var h = map.Height;
        if (w <= 0 || h <= 0)
        {
            debug = $"invalid-map-size={w}x{h}";
            return null;
        }

        // clamp to nearest in-bounds cell
        var centerCellX = (obstacleRegion.Rect.Left + obstacleRegion.Rect.Right) * 0.5f;
        var centerCellY = (obstacleRegion.Rect.Top + obstacleRegion.Rect.Bottom) * 0.5f;
        var invRes = 1.0f / map.PixelSize;
        var delta = (player.Position - mapCenter) * invRes;
        var sx = (int)MathF.Round(centerCellX + delta.X);
        var sy = (int)MathF.Round(centerCellY + delta.Z);
        sx = Math.Clamp(sx, 0, w - 1);
        sy = Math.Clamp(sy, 0, h - 1);

        var visited = new bool[w * h];
        var q = new Queue<(int x, int y)>();
        var pass1Visited = 0;
        var pass1BitmapReject = 0;
        var pass1RayReject = 0;
        var pass2Visited = 0;
        var pass2RayReject = 0;
        var startPos = obstacleRegion.CellCenterToWorld(mapCenter, sx, sy);
        var startRayLoS = HasLineOfSightFrom(startPos, player.PosRot.Y, target);
        var startBitmapLoS = obstacleRegion.HasObstacleMapLineOfSight(mapCenter, startPos, target.Position);
        bool Passable(int x, int y) => (uint)x < (uint)w && (uint)y < (uint)h && !map[x, y];

        void Enqueue(int x, int y)
        {
            if (!Passable(x, y))
                return;
            ref var slot = ref visited[y * w + x];
            if (slot)
                return;
            slot = true;
            q.Enqueue((x, y));
        }

        bool SeedFromNearestPassable(out int seededCount)
        {
            seededCount = 0;
            Enqueue(sx, sy);
            if (q.Count > 0)
            {
                seededCount = q.Count;
                return true;
            }

            // start can be blocked, grow until passable seed found
            var maxR = Math.Max(w, h);
            for (var r = 1; r < maxR; ++r)
            {
                var any = false;
                var xmin = sx - r;
                var xmax = sx + r;
                var ymin = sy - r;
                var ymax = sy + r;

                for (var x = xmin; x <= xmax; ++x)
                {
                    var before = q.Count;
                    Enqueue(x, ymin);
                    Enqueue(x, ymax);
                    any |= q.Count != before;
                }
                for (var y = ymin + 1; y < ymax; ++y)
                {
                    var before = q.Count;
                    Enqueue(xmin, y);
                    Enqueue(xmax, y);
                    any |= q.Count != before;
                }
                if (any)
                {
                    seededCount = q.Count;
                    return true;
                }
            }
            return false;
        }

        if (!SeedFromNearestPassable(out var seededPass1))
        {
            debug = $"seed-failed start=({sx},{sy}) start-passable={Passable(sx, sy)} start-ray={startRayLoS} start-bitmap={startBitmapLoS}";
            return null;
        }

        // check bitmap los + raycast los
        while (q.Count > 0)
        {
            var (x, y) = q.Dequeue();
            ++pass1Visited;
            var wpos = obstacleRegion.CellCenterToWorld(mapCenter, x, y);
            var bitmapOK = obstacleRegion.HasObstacleMapLineOfSight(mapCenter, wpos, target.Position);
            var rayOK = HasLineOfSightFrom(wpos, player.PosRot.Y, target);
            if (!bitmapOK)
                ++pass1BitmapReject;
            if (!rayOK)
                ++pass1RayReject;
            if ((x != sx || y != sy) && bitmapOK && rayOK)
            {
                debug = $"ok-pass1 start=({sx},{sy}) seed1={seededPass1} p1v={pass1Visited} p1b-rej={pass1BitmapReject} p1r-rej={pass1RayReject}";
                return obstacleRegion.CellCenterToWorld(mapCenter, x, y);
            }

            Enqueue(x + 1, y);
            Enqueue(x - 1, y);
            Enqueue(x, y + 1);
            Enqueue(x, y - 1);
        }

        Array.Clear(visited, 0, visited.Length);
        q.Clear();
        if (!SeedFromNearestPassable(out var seededPass2))
        {
            debug = $"seed2-failed start=({sx},{sy}) pass1-visited={pass1Visited} b-reject={pass1BitmapReject} r-reject={pass1RayReject}";
            return null;
        }

        // in case the bitmap is shit, just do raycast only
        while (q.Count > 0)
        {
            var (x, y) = q.Dequeue();
            ++pass2Visited;
            var wpos = obstacleRegion.CellCenterToWorld(mapCenter, x, y);
            var rayOK = HasLineOfSightFrom(wpos, player.PosRot.Y, target);
            if (!rayOK)
                ++pass2RayReject;
            if ((x != sx || y != sy) && rayOK)
            {
                debug = $"ok-pass2 start=({sx},{sy}) seed1={seededPass1} p1v={pass1Visited} p1b-rej={pass1BitmapReject} p1r-rej={pass1RayReject} seed2={seededPass2} p2v={pass2Visited} p2r-rej={pass2RayReject}";
                return obstacleRegion.CellCenterToWorld(mapCenter, x, y);
            }

            Enqueue(x + 1, y);
            Enqueue(x - 1, y);
            Enqueue(x, y + 1);
            Enqueue(x, y - 1);
        }

        debug = $"null start=({sx},{sy}) passable={Passable(sx, sy)} start-ray={startRayLoS} start-bitmap={startBitmapLoS} seed1={seededPass1} p1v={pass1Visited} p1b-rej={pass1BitmapReject} p1r-rej={pass1RayReject} seed2={seededPass2} p2v={pass2Visited} p2r-rej={pass2RayReject}";
        return null;
    }

    // FIXME: shouldn't be using ffi stuff in this module
    private static bool HasLineOfSightFrom(WPos from, float sourceY, Actor target)
    {
        var sourcePos = from.ToVec3(sourceY + 2);
        var targetPos = target.Position.ToVec3(target.PosRot.Y + 2);
        var offset = targetPos - sourcePos;
        var maxDist = offset.Length();
        if (maxDist <= 1e-3f)
            return true;
        var direction = offset / maxDist;
        return !BGCollisionModule.RaycastMaterialFilter(sourcePos, direction, out _, maxDist);
    }
}
