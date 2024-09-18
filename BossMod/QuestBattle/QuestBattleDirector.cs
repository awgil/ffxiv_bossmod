using BossMod.AI;
using BossMod.Autorotation;
using Dalamud.Plugin.Ipc;
using System.Threading.Tasks;

namespace BossMod.QuestBattle;

class PathfindNoop : ICallGateSubscriber<Vector3, Vector3, bool, Task<List<Vector3>>?>
{
    public void InvokeAction(Vector3 arg1, Vector3 arg2, bool arg3) { }
    public Task<List<Vector3>>? InvokeFunc(Vector3 arg1, Vector3 arg2, bool arg3) => null;
    public void Subscribe(Action<Vector3, Vector3, bool> action) { }
    public void Unsubscribe(Action<Vector3, Vector3, bool> action) { }
}

class PathReadyNoop : ICallGateSubscriber<bool>
{
    public void InvokeAction() { }
    public bool InvokeFunc() => false;
    public void Subscribe(Action action) { }
    public void Unsubscribe(Action action) { }
}

public sealed class QuestBattleDirector : IDisposable
{

    public readonly WorldState World;
    private readonly BossModuleManager bmm;
    private readonly EventSubscriptions _subscriptions;
    private readonly QuestBattleConfig _config;

    public readonly record struct NavigationWaypoint(Vector3 Position, bool SpecifiedInPath);

    public List<NavigationWaypoint> CurrentWaypoints { get; private set; } = [];
    public QuestBattle? CurrentModule { get; private set; }
    public QuestObjective? CurrentObjective { get; private set; }

    public int CurrentObjectiveNavigationProgress { get; private set; }
    public List<Waypoint> CurrentConnections { get; private set; } = [];

    public bool Paused = false;
    public bool WaitCommence = false;

    public Event<QuestBattle> QuestActivated = new();
    public Event<QuestObjective> ObjectiveChanged = new();
    public Event<QuestObjective> ObjectiveCleared = new();

    public const float Tolerance = 0.25f;

    private readonly ICallGateSubscriber<Vector3, Vector3, bool, Task<List<Vector3>>?> _pathfind;
    private readonly ICallGateSubscriber<bool> _isMeshReady;

    private bool _combatFlag;

    private static void Log(string msg) => Service.Log($"[QBD] {msg}");

    public bool Enabled => _config.Enabled && World.CurrentCFCID != 0 && World.Party.WithoutSlot(includeDead: true).Count(x => x.Type == ActorType.Player) == 1;

    public QuestBattleDirector(WorldState ws, BossModuleManager bmm)
    {
        World = ws;
        _config = Service.Config.Get<QuestBattleConfig>();
        this.bmm = bmm;

        _subscriptions = new(
#if DEBUG
            ws.Actors.StatusGain.Subscribe((a, i) =>
            {
                if (a.OID == 0)
                    return;
                Log($"{a} gain {a.Statuses[i]}");
            }),
            ws.Actors.StatusGain.Subscribe((a, i) =>
            {
                if (a.OID == 0)
                    return;
                Log($"{a} lose {a.Statuses[i]}");
            }),
            ws.Actors.ModelStateChanged.Subscribe(a =>
            {
                if (a.OID == 0)
                    return;
                Log($"{a} model state: {a.ModelState}");
            }),
            ws.Actors.EventStateChanged.Subscribe(a =>
            {
                if (a.OID == 0)
                    return;
                Log($"{a} event state: {a.EventState}");
            }),
            ws.DirectorUpdate.Subscribe(diru =>
            {
                Log($"Director update: U:0x{diru.UpdateID:X} P1:0x{diru.Param1:X} P2:0x{diru.Param2:X} P3:0x{diru.Param3:X} P4:0x{diru.Param4:X}");
            }),
            ws.Actors.EventObjectStateChange.Subscribe((act, u) =>
            {
                Log($"EObjState: {act} => 0x{u:X}");
            }),
            ws.Actors.EventObjectAnimation.Subscribe((act, p1, p2) =>
            {
                Log($"EObjAnim: {act}, {p1}, {p2}");
            }),
#endif
            ws.CurrentZoneChanged.Subscribe(OnZoneChange),
            ObjectiveChanged.Subscribe(OnObjectiveChanged),
            ObjectiveCleared.Subscribe(OnObjectiveCleared)
        );

        if (Service.PluginInterface == null)
        {
            Log($"UIDev detected, skipping initialization");
            _pathfind = new PathfindNoop();
            _isMeshReady = new PathReadyNoop();
        }
        else
        {
            _pathfind = Service.PluginInterface.GetIpcSubscriber<Vector3, Vector3, bool, Task<List<Vector3>>?>("vnavmesh.Nav.Pathfind");
            _isMeshReady = Service.PluginInterface.GetIpcSubscriber<bool>("vnavmesh.Nav.IsReady");
        }
    }

    private Task<List<Vector3>>? Pathfind(Vector3 source, Vector3 target) => _pathfind.InvokeFunc(source, target, false);
    private bool IsMeshReady() => _isMeshReady.InvokeFunc();

    private void Clear()
    {
        CurrentWaypoints.Clear();
        CurrentObjective = null;
        CurrentModule?.Dispose();
        CurrentModule = null;
    }

    private void OnPlayerEnterCombat(Actor player)
    {
        if (CurrentObjective is QuestObjective obj && obj.NavigationStrategy is NavigationStrategy.Stop or NavigationStrategy.Pause)
            CurrentWaypoints.Clear();
    }

    private void OnPlayerExitCombat(Actor player)
    {
        if (CurrentObjective is QuestObjective obj && obj.NavigationStrategy == NavigationStrategy.Pause)
        {
            Log($"player exited combat, retrying pathfind");
            TryPathfind(player.PosRot.XYZ(), obj.Connections.Skip(CurrentObjectiveNavigationProgress).ToList());
        }
    }

    public void Update(AIHints hints)
    {
        if (!Enabled || Paused || bmm?.ActiveModule?.StateMachine.ActivePhase != null)
            return;

        var player = World.Party.Player();
        if (player == null)
            return;

        if (HaveTarget(player, hints))
        {
            if (!_combatFlag)
                OnPlayerEnterCombat(player);

            _combatFlag = true;
        }
        else if (_combatFlag)
        {
            OnPlayerExitCombat(player);
            _combatFlag = false;
        }

        if (CurrentModule != null)
        {
            CurrentModule.Update();
            var obj = CurrentModule.CurrentObjective;
            if (obj == null)
            {
                if (CurrentObjective != null)
                {
                    ObjectiveCleared.Fire(CurrentObjective);
                    CurrentObjective = null;
                }
            }
            else if (obj != CurrentObjective)
            {
                ObjectiveChanged.Fire(obj);
                CurrentObjective = obj;
            }
        }

        if (CurrentObjective != null)
            MoveNext(player, CurrentObjective, hints);
    }

    private void MoveNext(Actor player, QuestObjective objective, AIHints hints)
    {
        if (CurrentWaypoints.Count == 0 || AIController.InCutscene)
            return;

        if (_config.ShowWaypoints)
            DrawWaypoints(player.PosRot.XYZ());

        var nextwp = CurrentWaypoints[0];
        var playerPos = player.PosRot.XYZ();
        var direction = nextwp.Position - playerPos;
        if (direction.XZ().Length() < Tolerance)
        {
            if (nextwp.SpecifiedInPath)
                CurrentObjectiveNavigationProgress++;

            CurrentWaypoints.RemoveAt(0);
            if (CurrentWaypoints.Count == 0)
                CurrentModule?.OnNavigationComplete();
            MoveNext(player, objective, hints);
            return;
        }
        else
        {
            Dash(player, direction, hints);
            hints.ForcedMovement = direction;
        }
    }

    private void DrawWaypoints(Vector3 playerPos)
    {
        var start = playerPos;
        var current = true;
        foreach (var wp in CurrentWaypoints)
        {
            Camera.Instance?.DrawWorldLine(start, wp.Position, current ? ArenaColor.Safe : ArenaColor.Danger);
            current = false;
            start = wp.Position;
        }
    }

    public static bool HaveTarget(Actor player, AIHints hints) => player.InCombat || hints.PriorityTargets.Any(x => hints.Bounds.Contains(x.Actor.Position - hints.Center));

    enum ActionsProhibitedStatus : uint
    {
        OutOfTheAction = 2109,
        InEvent = 1268,
    }

    private void Dash(Actor player, Vector3 direction, AIHints hints)
    {
        if (!_config.UseDash || player.Statuses.Any(s => (ActionsProhibitedStatus)s.ID is ActionsProhibitedStatus.OutOfTheAction or ActionsProhibitedStatus.InEvent || RotationModuleManager.IsRoleplayStatus(s)))
            return;

        var moveDistance = direction.Length();
        var moveAngle = Angle.FromDirection(new WDir(direction.XZ()));

        ActionID dashAction = default;
        var dashDistance = float.MaxValue;

        switch (player.Class)
        {
            case Class.PCT:
                dashAction = ActionID.MakeSpell(PCT.AID.Smudge);
                dashDistance = 15;
                break;
            case Class.DRG:
                dashAction = ActionID.MakeSpell(DRG.AID.ElusiveJump);
                dashDistance = 15;
                moveAngle = Angle.FromDirection(-moveAngle.ToDirection());
                break;
            case Class.WHM:
                dashAction = ActionID.MakeSpell(WHM.AID.AetherialShift);
                dashDistance = 15;
                break;
            case Class.RPR:
                dashAction = ActionID.MakeSpell(RPR.AID.HellsIngress);
                dashDistance = 15;
                break;
            case Class.DNC:
                dashAction = ActionID.MakeSpell(DNC.AID.EnAvant);
                dashDistance = 10;
                break;
            case Class.NIN:
                if (moveDistance > 20)
                {
                    var destination = direction / (moveDistance / 20);
                    hints.ActionsToExecute.Push(ActionID.MakeSpell(NIN.AID.Shukuchi), null, ActionQueue.Priority.Low, targetPos: player.PosRot.XYZ() + destination);
                }
                return;
        }

        if (moveDistance > dashDistance)
            hints.ActionsToExecute.Push(dashAction, null, ActionQueue.Priority.Low, facingAngle: moveAngle);
    }

    private async void TryPathfind(Vector3 start, List<Waypoint> connections, int maxRetries = 5)
    {
        CurrentConnections = connections;
        CurrentWaypoints = await TryPathfind(Enumerable.Repeat(new Waypoint(start, false), 1).Concat(connections), maxRetries).ConfigureAwait(false);
    }

    private async Task<List<NavigationWaypoint>> TryPathfind(IEnumerable<Waypoint> connectionPoints, int maxRetries = 5)
    {
        if (!IsMeshReady())
        {
            await Task.Delay(500).ConfigureAwait(false);
            return await TryPathfind(connectionPoints, maxRetries - 1).ConfigureAwait(false);
        }
        var points = connectionPoints.Take(3).ToList();
        if (points.Count < 2)
        {
            Log($"pathfind called with too few points (need 2, got {points.Count})");
            return [];
        }
        var start = points[0];
        var end = points[1];

        List<NavigationWaypoint> thesePoints;

        if (end.Pathfind)
        {

            var task = Pathfind(start.Position, end.Position);
            if (task == null)
            {
                Log($"Pathfind failure");
                return [];
            }

            var ptVecs = await task.ConfigureAwait(false);
            // returned path always contains the destination point twice for whatever reason
            ptVecs.RemoveAt(ptVecs.Count - 1);

            Log(string.Join(", ", ptVecs.Select(Utils.Vec3String)));
            thesePoints = ptVecs.Take(ptVecs.Count - 1).Select(p => new NavigationWaypoint(p, false)).ToList();
            thesePoints.Add(new NavigationWaypoint(ptVecs.Last(), true));
        }
        else
        {
            thesePoints = [new(start.Position, false), new(end.Position, true)];
        }

        if (points.Count > 2)
            thesePoints.AddRange(await TryPathfind(connectionPoints.Skip(1)).ConfigureAwait(false));
        return thesePoints;
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
    }

    private void OnObjectiveChanged(QuestObjective obj)
    {
        CurrentObjectiveNavigationProgress = 0;
        Log($"next objective: {obj}");
        if (World.Party.Player() is Actor player && (!_combatFlag || obj.NavigationStrategy == NavigationStrategy.Continue))
            TryPathfind(player.PosRot.XYZ(), obj.Connections);
    }

    private void OnObjectiveCleared(QuestObjective obj)
    {
        CurrentObjectiveNavigationProgress = 0;
        Log($"cleared objective: {obj}");
        CurrentWaypoints.Clear();
    }

    private void OnZoneChange(WorldState.OpZoneChange change)
    {
        Clear();

        if (bmm.ActiveModule != null)
            return;

        var newHandler = QuestBattleRegistry.GetHandler(World, change.CFCID, _config.MinMaturity);
        CurrentModule = newHandler;
        if (newHandler != null)
            QuestActivated.Fire(newHandler);
    }
}
