namespace BossMod.Autorotation;

// the manager contains a set of rotation module instances corresponding to the selected preset/plan
public sealed class RotationModuleManager : IDisposable
{
    private Preset? _preset; // if non-null, this preset overrides the configuration
    public Preset? Preset
    {
        get => _preset;
        set
        {
            DirtyActiveModules(_preset != value);
            _preset = value;
        }
    }

    public readonly AutorotationConfig Config = Service.Config.Get<AutorotationConfig>();
    public readonly RotationDatabase Database;
    public readonly BossModuleManager Bossmods;
    public readonly int PlayerSlot; // TODO: reconsider, we rely on too many things in clientstate...
    public readonly AIHints Hints;
    public PlanExecution? Planner { get; private set; }
    private readonly PartyRolesConfig _prc = Service.Config.Get<PartyRolesConfig>();
    private readonly EventSubscriptions _subscriptions;
    private List<(RotationModuleDefinition Definition, RotationModule Module)>? _activeModules;

    public static readonly Preset ForceDisable = new(""); // empty preset, so if it's activated, rotation is force disabled

    public WorldState WorldState => Bossmods.WorldState;
    public ulong PlayerInstanceId => WorldState.Party.Members[PlayerSlot].InstanceId;
    public Actor? Player => WorldState.Party[PlayerSlot];

    // historic data for recent events that could be interesting for modules
    public DateTime CombatStart { get; private set; } // default value when player is not in combat, otherwise timestamp when player entered combat
    public (DateTime Time, ActorCastEvent? Data) LastCast { get; private set; }

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
            WorldState.Actors.StatusGain.Subscribe((a, idx) => DirtyActiveModules(PlayerInstanceId == a.InstanceID && a.Statuses[idx].ID == (uint)Roleplay.SID.RolePlaying)),
            WorldState.Actors.StatusLose.Subscribe((a, idx) => DirtyActiveModules(PlayerInstanceId == a.InstanceID && a.Statuses[idx].ID == (uint)Roleplay.SID.RolePlaying)),
            WorldState.Actors.MountChanged.Subscribe(a => DirtyActiveModules(PlayerInstanceId == a.InstanceID)),
            WorldState.Party.Modified.Subscribe(op => DirtyActiveModules(op.Slot == PlayerSlot)),
            WorldState.Client.ActionRequested.Subscribe(OnActionRequested),
            WorldState.Client.CountdownChanged.Subscribe(OnCountdownChanged),
            Database.Presets.PresetModified.Subscribe(OnPresetModified)
        );
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    public void Update(float estimatedAnimLockDelay, float forceMovementIn, bool isMoving)
    {
        // see whether current plan matches what should be active, and update if not; only rebuild actions if there is no active override
        var expectedPlan = CalculateExpectedPlan();
        if (Planner?.Module != Bossmods.ActiveModule || Planner?.Plan != expectedPlan)
        {
            Service.Log($"[RMM] Changing active plan: '{Planner?.Plan?.Guid}' -> '{expectedPlan?.Guid}'");
            Planner = Bossmods.ActiveModule != null ? new(Bossmods.ActiveModule, expectedPlan) : null;
            DirtyActiveModules(Preset == null);
        }

        // rebuild modules if needed
        _activeModules ??= Preset != null ? RebuildActiveModules(Preset.Modules.Keys) : Planner?.Plan != null ? RebuildActiveModules(Planner.Plan.Modules.Keys) : [];

        // forced target update
        if (Hints.ForcedTarget == null && Preset == null && Planner?.ActiveForcedTarget() is var forced && forced != null)
        {
            Hints.ForcedTarget = ResolveTargetOverride(forced.Value);
        }

        // auto actions
        var target = Hints.ForcedTarget ?? WorldState.Actors.Find(Player?.TargetID ?? 0);
        foreach (var m in _activeModules)
        {
            var mt = m.Module.GetType();
            var values = Preset?.ActiveStrategyOverrides(mt) ?? Planner?.ActiveStrategyOverrides(mt) ?? throw new InvalidOperationException("Both preset and plan are null, but there are active modules");
            m.Module.Execute(values, target, estimatedAnimLockDelay, forceMovementIn, isMoving);
        }
    }

    public Actor? ResolveTargetOverride(in StrategyValue strategy) => strategy.Target switch
    {
        StrategyTarget.Self => Player,
        StrategyTarget.PartyByAssignment => _prc.SlotsPerAssignment(WorldState.Party) is var spa && strategy.TargetParam < spa.Length ? WorldState.Party[spa[strategy.TargetParam]] : null,
        StrategyTarget.PartyWithLowestHP => WorldState.Party.WithoutSlot().Exclude(strategy.TargetParam != 0 ? null : Player).MinBy(a => a.HPMP.CurHP),
        StrategyTarget.EnemyWithHighestPriority => Player != null ? Hints.PriorityTargets.MinBy(e => (e.Actor.Position - Player.Position).LengthSq())?.Actor : null,
        StrategyTarget.EnemyByOID => Player != null && (uint)strategy.TargetParam is var oid && oid != 0 ? Hints.PotentialTargets.Where(e => e.Actor.OID == oid).MinBy(e => (e.Actor.Position - Player.Position).LengthSq())?.Actor : null,
        _ => null
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

    public override string ToString() => string.Join(", ", _activeModules?.Select(m => m.Module.GetType().Name) ?? []);

    // TODO: consider not recreating modules that were active and continue to be active?
    private List<(RotationModuleDefinition Definition, RotationModule Module)> RebuildActiveModules(IEnumerable<Type> types)
    {
        List<(RotationModuleDefinition Definition, RotationModule Module)> res = [];
        var player = Player;
        if (player != null)
        {
            var isRPMode = player.Statuses.Any(s => s.ID == (uint)Roleplay.SID.RolePlaying);
            foreach (var m in types)
            {
                if (!RotationModuleRegistry.Modules.TryGetValue(m, out var def))
                    continue;
                if (!def.Definition.Classes[(int)player.Class] || player.Level < def.Definition.MinLevel || player.Level > def.Definition.MaxLevel)
                    continue;
                if (!def.Definition.CanUseWhileMounted && player.MountId > 0)
                    continue;
                if (!def.Definition.CanUseWhileRoleplaying && isRPMode)
                    continue;
                res.Add((def.Definition, def.Builder(this, player)));
            }
        }
        return res;
    }

    private void DirtyActiveModules(bool condition)
    {
        if (condition)
            _activeModules = null;
    }

    private void OnCombatChanged(Actor actor)
    {
        if (PlayerInstanceId != actor.InstanceID)
            return; // don't care

        CombatStart = actor.InCombat ? WorldState.CurrentTime : default; // keep track of combat time in case rotation modules want to do something special in openers

        if (!actor.InCombat && (Preset == ForceDisable ? Config.ClearForceDisableOnCombatEnd : Config.ClearPresetOnCombatEnd))
        {
            // player exits combat => clear manual overrides
            Service.Log($"[RMM] Player exits combat => clear preset '{Preset?.Name ?? "<n/a>"}'");
            Preset = null;
        }
        else if (actor.InCombat && WorldState.Client.CountdownRemaining > Config.EarlyPullThreshold)
        {
            // player enters combat while countdown is in progress => force disable
            Service.Log($"[RMM] Player ninja pulled => force-disabling from '{Preset?.Name ?? "<n/a>"}'");
            Preset = ForceDisable;
        }
        // if player enters combat when countdown is either not active or around zero, proceed normally - if override is queued, let it run, otherwise let plan run
    }

    private void OnDeadChanged(Actor actor)
    {
        if (PlayerInstanceId != actor.InstanceID)
            return; // don't care

        // note: if combat ends while player is dead, we'll reset the preset, which is desirable
        if (actor.IsDead && actor.InCombat)
        {
            // player died in combat => force disable (otherwise there's a risk of dying immediately after rez)
            Service.Log($"[RMM] Player died in combat => force-disabling from '{Preset?.Name ?? "<n/a>"}'");
            Preset = ForceDisable;
        }
        // else: player either died outside combat (no need to touch anything) or rez'd (unless player cleared override, we stay in force disable mode)
    }

    private void OnCountdownChanged(ClientState.OpCountdownChange op)
    {
        if (op.Value == null && !(Player?.InCombat ?? false))
        {
            // countdown ended and player is not in combat - so either it was cancelled, or pull didn't happen => clear manual overrides
            // note that if pull will happen regardless after this, we'll start executing plan normally (without prepull part)
            Service.Log($"[RMM] Countdown expired or aborted => clear preset '{Preset?.Name ?? "<n/a>"}'");
            Preset = null;
        }
    }

    private void OnPresetModified(Preset? prev, Preset? curr)
    {
        if (prev != null && prev == Preset)
            Preset = curr;
    }

    private void OnActionRequested(ClientState.OpActionRequest op)
    {
#if DEBUG
        Service.Log($"[RMM] Exec #{op.Request.SourceSequence} {op.Request.Action} @ {op.Request.TargetID:X} [{string.Join(" --- ", _activeModules?.Select(m => m.Module.DescribeState()) ?? [])}]");
#endif
    }

    private void OnCastEvent(Actor actor, ActorCastEvent cast)
    {
        if (cast.SourceSequence != 0 && WorldState.Party.Members[PlayerSlot].InstanceId == actor.InstanceID)
        {
            LastCast = (WorldState.CurrentTime, cast);
#if DEBUG
            Service.Log($"[RMM] Cast #{cast.SourceSequence} {cast.Action} @ {cast.MainTargetID:X} [{string.Join(" --- ", _activeModules?.Select(m => m.Module.DescribeState()) ?? [])}]");
#endif
        }
    }
}
