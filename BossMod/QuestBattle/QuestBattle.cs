using Dalamud.Game.ClientState.Conditions;

namespace BossMod.QuestBattle;

public record struct Waypoint(Vector3 Position, bool Pathfind = true);

public enum NavigationStrategy
{
    // keep moving to destination even if in combat
    Continue,
    // clear waypoints upon entering combat
    Stop,
    // clear waypoints upon entering combat, repath when combat ends
    Pause,
}

public class QuestObjective(WorldState ws)
{
    public readonly WorldState World = ws;
    public string Name { get; private set; } = "";
    public readonly List<Waypoint> Connections = [];
    public NavigationStrategy NavigationStrategy = NavigationStrategy.Pause;

    public string DisplayName
    {
        get
        {
            if (Name.Length > 0)
                return Name;

            if (Connections.Count > 0)
                return Utils.Vec3String(Connections.Last().Position);

            return "<none>";
        }
    }

    public bool ForceStopNavigation;
    public bool Completed;

    public Action<Actor, AIHints>? AddAIHints;
    public Action<Actor>? OnModelStateChanged;
    public Action<Actor, ActorStatus>? OnStatusGain;
    public Action<Actor, ActorStatus>? OnStatusLose;
    public Action<Actor>? OnActorEventStateChanged;
    public Action<Actor, ushort>? OnEventObjectStateChanged;
    public Action<Actor, ushort, ushort>? OnEventObjectAnimation;
    public Action<Actor>? OnActorCreated;
    public Action<Actor>? OnActorDestroyed;
    public Action<Actor>? OnActorCombatChanged;
    public Action<Actor>? OnActorKilled;
    public Action<Actor>? OnActorTargetableChanged;
    public Action<Actor, ActorCastEvent>? OnEventCast;
    public Action<WorldState.OpEnvControl>? OnEnvControl;
    public Action<WorldState.OpDirectorUpdate>? OnDirectorUpdate;
    public Action<ClientState.OpDutyActionsChange>? OnDutyActionsChange;
    public Action<ConditionFlag, bool>? OnConditionChange;
    public Action? OnNavigationComplete;
    public Action? Update;

    public QuestObjective Named(string name)
    {
        Name = name;
        return this;
    }

    public QuestObjective WithConnection(Vector3 conn) => WithConnection(new Waypoint(conn));
    public QuestObjective WithConnection(Waypoint conn)
    {
        Connections.Add(conn);
        return this;
    }

    public QuestObjective WithConnections(params Vector3[] connections)
    {
        Connections.AddRange(connections.Select(c => new Waypoint(c)));
        return this;
    }
    public QuestObjective WithConnections(params Waypoint[] connections)
    {
        Connections.AddRange(connections);
        return this;
    }

    public QuestObjective With(Action<QuestObjective> act)
    {
        act(this);
        return this;
    }

    public QuestObjective MoveHint(WPos destination, float weight = 0.5f)
    {
        AddAIHints += (player, hints) =>
        {
            hints.GoalZones.Add(p => p.InCircle(destination, 2) ? weight : 0);
        };
        return this;
    }

    public QuestObjective MoveHint(Vector3 destination) => MoveHint(new WPos(destination.XZ()));

    public QuestObjective PauseForCombat(bool pause)
    {
        NavigationStrategy = pause ? NavigationStrategy.Pause : NavigationStrategy.Continue;
        return this;
    }

    public QuestObjective StopOnCombat()
    {
        NavigationStrategy = NavigationStrategy.Stop;
        return this;
    }

    public QuestObjective CompleteOnTargetable(uint oid, bool targetable)
    {
        OnActorTargetableChanged += (act) => CompleteIf(act.OID == oid && act.IsTargetable == targetable);
        return this;
    }

    public QuestObjective CompleteOnCreated(uint oid)
    {
        OnActorCreated += (act) => CompleteIf(act.OID == oid);
        return this;
    }

    public QuestObjective CompleteOnKilled(uint oid, int required = 1)
    {
        var killed = 0;
        OnActorKilled += (act) =>
        {
            if (act.OID == oid && ++killed >= required)
                Completed = true;
        };
        return this;
    }

    public QuestObjective CompleteOnDestroyed(uint oid)
    {
        OnActorDestroyed += (act) => CompleteIf(act.OID == oid);
        return this;
    }

    public QuestObjective CompleteOnState7(uint oid)
    {
        OnActorEventStateChanged += (act) => CompleteIf(act.OID == oid && act.EventState == 7);
        return this;
    }

    public QuestObjective ThenWait(float seconds)
    {
        var until = DateTime.MaxValue;
        OnNavigationComplete += () => until = World.FutureTime(seconds);
        Update += () => CompleteIf(World.CurrentTime > until);
        return this;
    }

    public QuestObjective Hints(Action<Actor, AIHints> addHints)
    {
        AddAIHints += addHints;
        return this;
    }

    public QuestObjective WithInteract<OID>(OID targetOid, bool allowInCombat = false) where OID : Enum
        => WithInteract((uint)(object)targetOid, allowInCombat);

    public QuestObjective WithInteract(uint targetOid, bool allowInCombat = false)
    {
        AddAIHints += (player, hints) =>
        {
            if (!player.InCombat || allowInCombat)
                hints.InteractWithOID(World, targetOid);
        };
        return this;
    }

    public QuestObjective CompleteAtDestination()
    {
        OnNavigationComplete += () => Completed = true;
        return this;
    }

    public static QuestObjective Combat(WorldState ws, params Vector3[] connections)
        => new QuestObjective(ws).WithConnections(connections)
            .With(obj =>
            {
                obj.OnActorCombatChanged += (act) => obj.CompleteIf(act.OID == 0 && !act.InCombat);
            });

    public static QuestObjective StandardInteract(WorldState ws, uint oid, params Vector3[] connections) => new QuestObjective(ws)
        .WithConnections(connections)
        .WithInteract(oid)
        .CompleteOnState7(oid);

    public override string ToString() => $"{Name}{(Connections.Count == 0 ? "" : Utils.Vec3String(Connections.Last().Position))}";

    public void CompleteIf(bool c) { Completed |= c; }
}

public abstract class QuestBattle : IDisposable
{
    public readonly WorldState World;
    private readonly EventSubscriptions _subscriptions;

    public List<QuestObjective> Objectives { get; private set; } = [];
    public int CurrentObjectiveIndex { get; private set; }
    public QuestObjective? CurrentObjective => CurrentObjectiveIndex >= 0 && CurrentObjectiveIndex < Objectives.Count ? Objectives[CurrentObjectiveIndex] : null;

    // low-resolution bounds centered on player character, with radius roughly equal to object load range
    // this allows AI to pathfind to any priority target regardless of distance, as long as it's loaded - this makes it easier to complete quest objectives which require combat
    // note that precision for aoe avoidance will obviously suffer
    public static readonly ArenaBoundsSquare OverworldBounds = new(100, 2.5f);

    protected static Vector3 V3(float x, float y, float z) => new(x, y, z);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    protected virtual void Dispose(bool disposing)
    {
        _subscriptions.Dispose();
        if (Service.Condition != null)
            Service.Condition.ConditionChange -= OnConditionChange;
    }

    protected QuestBattle(WorldState ws)
    {
        World = ws;
        Objectives = DefineObjectives(ws);

        _subscriptions = new(
            ws.Actors.EventStateChanged.Subscribe(act => CurrentObjective?.OnActorEventStateChanged?.Invoke(act)),
            ws.Actors.CastEvent.Subscribe((a, b) => CurrentObjective?.OnEventCast?.Invoke(a, b)),
            ws.Actors.StatusLose.Subscribe((act, ix) => CurrentObjective?.OnStatusLose?.Invoke(act, act.Statuses[ix])),
            ws.Actors.StatusGain.Subscribe((act, ix) => CurrentObjective?.OnStatusGain?.Invoke(act, act.Statuses[ix])),
            ws.Actors.ModelStateChanged.Subscribe(act => CurrentObjective?.OnModelStateChanged?.Invoke(act)),
            ws.Actors.Added.Subscribe(act => CurrentObjective?.OnActorCreated?.Invoke(act)),
            ws.Actors.Removed.Subscribe(act => CurrentObjective?.OnActorDestroyed?.Invoke(act)),
            ws.Actors.InCombatChanged.Subscribe(act => CurrentObjective?.OnActorCombatChanged?.Invoke(act)),
            ws.Actors.IsDeadChanged.Subscribe(act =>
            {
                if (act.IsDead)
                    CurrentObjective?.OnActorKilled?.Invoke(act);
            }),
            ws.Actors.EventObjectStateChange.Subscribe((act, u) => CurrentObjective?.OnEventObjectStateChanged?.Invoke(act, u)),
            ws.Actors.EventObjectAnimation.Subscribe((act, p1, p2) => CurrentObjective?.OnEventObjectAnimation?.Invoke(act, p1, p2)),
            ws.DirectorUpdate.Subscribe(op => CurrentObjective?.OnDirectorUpdate?.Invoke(op)),
            ws.EnvControl.Subscribe(op => CurrentObjective?.OnEnvControl?.Invoke(op)),
            ws.Actors.IsTargetableChanged.Subscribe(act => CurrentObjective?.OnActorTargetableChanged?.Invoke(act)),
            ws.Client.DutyActionsChanged.Subscribe(op => CurrentObjective?.OnDutyActionsChange?.Invoke(op))
        );
        if (Service.Condition == null)
            Service.Log($"[QuestBattle] UIDev detected, not registering hook");
        else
            Service.Condition.ConditionChange += OnConditionChange;
    }

    public void Init()
    {
        Objectives = DefineObjectives(World);
    }

    public virtual List<QuestObjective> DefineObjectives(WorldState ws) => [];

    public void Update()
    {
        CurrentObjective?.Update?.Invoke();
        if (CurrentObjective?.Completed ?? false)
            CurrentObjectiveIndex++;
    }
    public void OnNavigationComplete()
    {
        CurrentObjective?.OnNavigationComplete?.Invoke();
    }
    public virtual void AddQuestAIHints(Actor player, AIHints hints) { }
    public void AddAIHints(Actor player, AIHints hints)
    {
        AddQuestAIHints(player, hints);
        CurrentObjective?.AddAIHints?.Invoke(player, hints);
    }
    public void Advance() => CurrentObjectiveIndex++;
    public void Reset() => CurrentObjectiveIndex = 0;
    public void OnConditionChange(ConditionFlag flag, bool value) => CurrentObjective?.OnConditionChange?.Invoke(flag, value);
}
