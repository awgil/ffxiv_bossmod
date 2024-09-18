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

    public Action<Actor, AIHints> AddAIHints = (_, _) => { };
    public Action<Actor> OnModelStateChanged = (_) => { };
    public Action<Actor, ActorStatus> OnStatusGain = (_, _) => { };
    public Action<Actor, ActorStatus> OnStatusLose = (_, _) => { };
    public Action<Actor> OnActorEventStateChanged = (_) => { };
    public Action<Actor, ushort> OnEventObjectStateChanged = (_, _) => { };
    public Action<Actor, ushort, ushort> OnEventObjectAnimation = (_, _, _) => { };
    public Action<Actor> OnActorCreated = (_) => { };
    public Action<Actor> OnActorDestroyed = (_) => { };
    public Action<Actor> OnActorCombatChanged = (_) => { };
    public Action<Actor> OnActorKilled = (_) => { };
    public Action<Actor> OnActorTargetableChanged = (_) => { };
    public Action<WorldState.OpDirectorUpdate> OnDirectorUpdate = (_) => { };
    public Action<ConditionFlag, bool> OnConditionChange = (_, _) => { };
    public Action OnNavigationComplete = () => { };
    public Action Update = () => { };

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

    public QuestObjective CompleteOnKilled(uint oid)
    {
        OnActorKilled += (act) => CompleteIf(act.OID == oid);
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

    public override string ToString() => $"{Name}{(Connections.Count == 0 ? "" : Utils.Vec3String(Connections.Last().Position))}";

    public void CompleteIf(bool c) { Completed |= c; }
}

public abstract class QuestBattle : IDisposable
{
    public readonly WorldState World;
    private readonly EventSubscriptions _subscriptions;

    public readonly List<QuestObjective> Objectives = [];
    public int CurrentObjectiveIndex { get; private set; } = 0;
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
            ws.Actors.EventStateChanged.Subscribe(act => CurrentObjective?.OnActorEventStateChanged(act)),
            ws.Actors.StatusLose.Subscribe((act, ix) => CurrentObjective?.OnStatusLose(act, act.Statuses[ix])),
            ws.Actors.StatusGain.Subscribe((act, ix) => CurrentObjective?.OnStatusGain(act, act.Statuses[ix])),
            ws.Actors.ModelStateChanged.Subscribe(act => CurrentObjective?.OnModelStateChanged(act)),
            ws.Actors.Added.Subscribe(act => CurrentObjective?.OnActorCreated(act)),
            ws.Actors.Removed.Subscribe(act => CurrentObjective?.OnActorDestroyed(act)),
            ws.Actors.InCombatChanged.Subscribe(act => CurrentObjective?.OnActorCombatChanged(act)),
            ws.Actors.IsDeadChanged.Subscribe(act =>
            {
                if (act.IsDead)
                    CurrentObjective?.OnActorKilled(act);
            }),
            ws.Actors.EventObjectStateChange.Subscribe((act, u) => CurrentObjective?.OnEventObjectStateChanged(act, u)),
            ws.Actors.EventObjectAnimation.Subscribe((act, p1, p2) => CurrentObjective?.OnEventObjectAnimation(act, p1, p2)),
            ws.DirectorUpdate.Subscribe(op => CurrentObjective?.OnDirectorUpdate(op)),
            ws.Actors.IsTargetableChanged.Subscribe(act => CurrentObjective?.OnActorTargetableChanged(act))
        );
        if (Service.Condition == null)
            Service.Log($"[QuestBattle] UIDev detected, not registering hook");
        else
            Service.Condition.ConditionChange += OnConditionChange;
    }

    public virtual List<QuestObjective> DefineObjectives(WorldState ws) => [];

    public void Update()
    {
        CurrentObjective?.Update();
        if (CurrentObjective?.Completed ?? false)
            CurrentObjectiveIndex++;
    }
    public void OnNavigationComplete()
    {
        CurrentObjective?.OnNavigationComplete();
    }
    public virtual void AddQuestAIHints(Actor player, AIHints hints, float maxCastTime) { }
    public void AddAIHints(Actor player, AIHints hints, float maxCastTime)
    {
        AddQuestAIHints(player, hints, maxCastTime);
        CurrentObjective?.AddAIHints(player, hints);
    }
    public void Advance() => CurrentObjectiveIndex++;
    public void Reset() => CurrentObjectiveIndex = 0;
    public void OnConditionChange(ConditionFlag flag, bool value) => CurrentObjective?.OnConditionChange(flag, value);
}
