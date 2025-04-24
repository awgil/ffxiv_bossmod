﻿namespace BossMod.Autorotation;

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

    public Preset? Preset
    {
        get;
        set
        {
            DirtyActiveModules(field != value);
            field = value;
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
    private List<ActiveModule>? _activeModules;

    public static readonly Preset ForceDisable = new(""); // empty preset, so if it's activated, rotation is force disabled

    public WorldState WorldState => Bossmods.WorldState;
    public ulong PlayerInstanceId => WorldState.Party.Members[PlayerSlot].InstanceId;
    public Actor? Player => WorldState.Party[PlayerSlot];

    // historic data for recent events that could be interesting for modules
    public DateTime CombatStart { get; private set; } // default value when player is not in combat, otherwise timestamp when player entered combat
    public (DateTime Time, ActorCastEvent? Data) LastCast { get; private set; }

    // list of status effects that disable the player's default action set, but do not disable *all* actions
    // in these cases, we want to prevent active rotation modules from queueing any actions, because they might affect positioning or rotation, or interfere with player's attempt to manually use an action
    // TODO can this be sourced entirely from sheet data? i can't find a field that uniquely identifies these statuses while excluding "stuns" and transformations that do not inhibit the use of actions
    public static readonly uint[] TransformationStatuses = [
        (uint)Roleplay.SID.RolePlaying, // used for almost all solo duties
        (uint)Roleplay.SID.BorrowedFlesh, // used specifically for In from the Cold (Endwalker)
        (uint)Roleplay.SID.FreshPerspective, // sapphire weapon quest
        565, // "Transfiguration" from certain pomanders in Palace of the Dead
        439, // "Toad", palace of the dead
        1546, // "Odder", heaven-on-high
        3502, // "Owlet", EO
        404, // "Transporting", not a transformation but prevents actions
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
            Database.Presets.PresetModified.Subscribe(OnPresetModified)
        );
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    public void Update(float estimatedAnimLockDelay, bool isMoving)
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
        _activeModules ??= Preset != null ? RebuildActiveModules(Preset.Modules) : Planner?.Plan != null ? RebuildActiveModules(Planner.Plan.Modules) : [];

        // forced target update
        if (Hints.ForcedTarget == null && Preset == null && Planner?.ActiveForcedTarget() is var forced && forced != null)
        {
            Hints.ForcedTarget = forced.Value.Target != StrategyTarget.Automatic
                ? ResolveTargetOverride(forced.Value.Target, forced.Value.TargetParam)
                : (ResolveTargetOverride(StrategyTarget.EnemyWithHighestPriority, 0) ?? (Bossmods.ActiveModule?.PrimaryActor is var primary && primary != null && !primary.IsDeadOrDestroyed && primary.IsTargetable ? primary : null));
        }

        // auto actions
        var target = Hints.ForcedTarget ?? WorldState.Actors.Find(Player?.TargetID ?? 0);
        foreach (var m in _activeModules)
        {
            var values = Preset?.ActiveStrategyOverrides(m.DataIndex) ?? Planner?.ActiveStrategyOverrides(m.DataIndex) ?? throw new InvalidOperationException("Both preset and plan are null, but there are active modules");
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

    public override string ToString() => string.Join(", ", _activeModules?.Select(m => m.Module.GetType().Name) ?? []);

    private IEnumerable<Actor> FilteredPartyMembers(StrategyPartyFiltering filter)
    {
        var fullMask = new BitMask(~0ul);
        var allowedMask = fullMask;
        if (!filter.HasFlag(StrategyPartyFiltering.IncludeSelf))
            allowedMask.Clear(PlayerSlot);
        if (filter.HasFlag(StrategyPartyFiltering.ExcludeNoPredictedDamage))
        {
            var predictedDamage = Hints.PredictedDamage.Aggregate(default(BitMask), (s, p) => s | p.players);
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
    private List<ActiveModule> RebuildActiveModules<T>(List<T> modules) where T : IRotationModuleData
    {
        List<ActiveModule> res = [];
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
                res.Add(new(i, def, modules[i].Builder(this, player)));
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
