using Dalamud.Game.ClientState.Conditions;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Ipc;
using ImGuiNET;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

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

// TODO: get rid of this (runtime dependency on vnav)
class PathfindNoop : ICallGateSubscriber<Vector3, Vector3, bool, Task<List<Vector3>>?>
{
    bool ICallGateSubscriber.HasAction => false;
    bool ICallGateSubscriber.HasFunction => true;
    public void InvokeAction(Vector3 arg1, Vector3 arg2, bool arg3) { }
    public Task<List<Vector3>>? InvokeFunc(Vector3 arg1, Vector3 arg2, bool arg3) => null;
    public void Subscribe(Action<Vector3, Vector3, bool> action) { }
    public void Unsubscribe(Action<Vector3, Vector3, bool> action) { }
}

class PathReadyNoop : ICallGateSubscriber<bool>
{
    bool ICallGateSubscriber.HasAction => false;
    bool ICallGateSubscriber.HasFunction => true;
    public void InvokeAction() { }
    public bool InvokeFunc() => true;
    public void Subscribe(Action action) { }
    public void Unsubscribe(Action action) { }
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

public abstract class QuestBattle : ZoneModule
{
    private readonly EventSubscriptions _subscriptions;
    private readonly ZoneModuleConfig _config = Service.Config.Get<ZoneModuleConfig>();

    public readonly List<QuestObjective> Objectives;
    public int CurrentObjectiveIndex { get; private set; }
    public QuestObjective? CurrentObjective => CurrentObjectiveIndex >= 0 && CurrentObjectiveIndex < Objectives.Count ? Objectives[CurrentObjectiveIndex] : null;

    // these fields are moved here from QBD
    private readonly record struct NavigationWaypoint(Vector3 Position, bool SpecifiedInPath);
    private Task<List<NavigationWaypoint>>? PathfindTask;
    private List<NavigationWaypoint> CurrentWaypoints = [];
    private int CurrentObjectiveNavigationProgress;
    //private List<Waypoint> CurrentConnections = [];
    private bool Paused;
    private const float Tolerance = 0.25f;
    private readonly ICallGateSubscriber<Vector3, Vector3, bool, Task<List<Vector3>>?> _pathfind;
    private readonly ICallGateSubscriber<bool> _meshIsReady;
    private bool _combatFlag;
    private bool _playerLoaded;

    // and these are from QBW
    private delegate void AbandonDuty(bool a1);
    private readonly AbandonDuty? _abandonDuty;
    private readonly List<WPos> _debugWaymarks = [];

    // low-resolution bounds centered on player character, with radius roughly equal to object load range
    // this allows AI to pathfind to any priority target regardless of distance, as long as it's loaded - this makes it easier to complete quest objectives which require combat
    // note that precision for aoe avoidance will obviously suffer
    public static readonly ArenaBoundsSquare OverworldBounds = new(100, 2.5f);

    protected static Vector3 V3(float x, float y, float z) => new(x, y, z); // TODO: this is cruft, remove...
    private static void Log(string msg) => Service.Log($"[QBD] {msg}");

    protected QuestBattle(WorldState ws) : base(ws)
    {
#pragma warning disable CA2214 // TODO: this is kinda working rn, but still not good...
        Objectives = DefineObjectives(ws);
#pragma warning restore CA2214

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

        // TODO: get rid of stuff below, this is bad...
        if (Service.Condition != null)
            Service.Condition.ConditionChange += OnConditionChange;

        //_subscriptions = new(
        //    ObjectiveChanged.Subscribe(OnObjectiveChanged),
        //    ObjectiveCleared.Subscribe(OnObjectiveCleared)
        //);

        if (Service.PluginInterface == null)
        {
            //Log($"UIDev detected, skipping initialization");
            _pathfind = new PathfindNoop();
            _meshIsReady = new PathReadyNoop();
        }
        else
        {
            _pathfind = Service.PluginInterface.GetIpcSubscriber<Vector3, Vector3, bool, Task<List<Vector3>>?>("vnavmesh.Nav.Pathfind");
            _meshIsReady = Service.PluginInterface.GetIpcSubscriber<bool>("vnavmesh.Nav.IsReady");
            _abandonDuty = Marshal.GetDelegateForFunctionPointer<AbandonDuty>(Service.SigScanner.ScanText("E8 ?? ?? ?? ?? 41 B2 01 EB 39"));
        }
    }

    protected override void Dispose(bool disposing)
    {
        _subscriptions.Dispose();

        // TODO: get rid of stuff below, this is bad...
        if (Service.Condition != null)
            Service.Condition.ConditionChange -= OnConditionChange;

        base.Dispose(disposing);
    }

    public override void Update()
    {
        if (!_config.EnableQuestBattles)
            return;

        if (!_playerLoaded)
        {
            var player = World.Party.Player();
            if (player != null)
            {
                _playerLoaded = true;
                if (CurrentObjective != null)
                    TryPathfind(player.PosRot.XYZ(), CurrentObjective.Connections);
            }
        }

        if (PathfindTask?.IsCompletedSuccessfully ?? false)
        {
            CurrentWaypoints = PathfindTask.Result;
            PathfindTask = null;
        }
    }

    public override void CalculateAIHints(int playerSlot, Actor player, AIHints hints)
    {
        if (!_config.EnableQuestBattles)
            return;

        var restartPathfind = false;

        // update combat flag (TODO: the name is not great...)
        var flag = HaveTarget(player, hints);
        if (flag != _combatFlag)
        {
            _combatFlag = flag;
            if (flag)
            {
                if (CurrentObjective is QuestObjective obj && obj.NavigationStrategy is NavigationStrategy.Stop or NavigationStrategy.Pause)
                {
                    CurrentWaypoints.Clear();
                }
            }
            else
            {
                if (CurrentObjective is QuestObjective obj && obj.NavigationStrategy == NavigationStrategy.Pause)
                {
                    Log($"player exited combat, retrying pathfind");
                    restartPathfind = true;
                }
            }
        }

        var curObjective = CurrentObjective;
        if (curObjective != null)
        {
            curObjective.Update?.Invoke();
            if (curObjective.Completed)
            {
                CurrentObjectiveIndex++;
                restartPathfind |= OnObjectiveChanged();
            }
        }

        AddQuestAIHints(player, hints);

        curObjective = CurrentObjective;
        if (curObjective != null)
        {
            curObjective.AddAIHints?.Invoke(player, hints);

            if (restartPathfind)
            {
                TryPathfind(player.PosRot.XYZ(), curObjective.Connections[CurrentObjectiveNavigationProgress..]);
            }

            if (!Paused && !World.Party.Members[playerSlot].InCutscene)
            {
                MoveNext(player, curObjective, hints);
            }
        }
    }

    public void DrawDebugInfo()
    {
        if (UIMisc.Button("Leave duty", !ImGui.GetIO().KeyShift, "Hold shift to leave"))
            _abandonDuty?.Invoke(false);
        ImGui.SameLine();
        UIMisc.HelpMarker("Attempt to leave duty by directly sending the \"abandon duty\" packet, which may be able to bypass the out-of-combat restriction. Only works in some duties.");

        ImGui.Text($"Module: {GetType().Name}");
        DrawObjectives();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextUnformatted($"Zone: {World.CurrentZone} / CFC: {World.CurrentCFCID}");
        ImGui.SameLine();
        DrawGenerateModule();
        if (World.Party.Player() is Actor player)
        {
            ImGui.TextUnformatted($"Position: {Utils.Vec3String(player.PosRot.XYZ())}");
            ImGui.SameLine();
            if (ImGui.Button("Copy vec"))
            {
                var x = player.PosRot.X;
                var y = player.PosRot.Y;
                var z = player.PosRot.Z;
                ImGui.SetClipboardText($"new Vector3({x:F2}f, {y:F2}f, {z:F2}f)");
            }
            ImGui.SameLine();
            if (ImGui.Button("Copy moveto"))
            {
                var x = player.PosRot.X;
                var y = player.PosRot.Y;
                var z = player.PosRot.Z;
                ImGui.SetClipboardText($"/vnav moveto {x:F2} {y:F2} {z:F2}");
            }
            if (World.Actors.Find(player.TargetID) is Actor tar)
            {
                ImGui.TextUnformatted($"Target: {tar.Name} ({tar.Type}; {tar.OID:X}) (hb={tar.HitboxRadius})");
                ImGui.TextUnformatted($"Distance: {player.DistanceToHitbox(tar)}");
                ImGui.TextUnformatted($"Angle: {player.AngleTo(tar)}");
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button("Record position"))
                _debugWaymarks.Add(player.Position);

            if (ImGui.Button("Copy all"))
                ImGui.SetClipboardText(string.Join(", ", _debugWaymarks.Select(w => $"new({w.X:F2}f, {w.Z:F2}f)")));

            foreach (var w in _debugWaymarks)
                ImGui.TextUnformatted($"{w}");
        }
    }

    public virtual List<QuestObjective> DefineObjectives(WorldState ws) => [];
    public virtual void AddQuestAIHints(Actor player, AIHints hints) { }

    private Task<List<Vector3>>? Pathfind(Vector3 source, Vector3 target) => _pathfind.InvokeFunc(source, target, false);
    private bool MeshIsReady() => _meshIsReady.InvokeFunc();

    public static bool HaveTarget(Actor player, AIHints hints) => player.InCombat || hints.PriorityTargets.Any(x => hints.PathfindMapBounds.Contains(x.Actor.Position - hints.PathfindMapCenter));

    // returns true if we want to restart pathfinding
    private bool OnObjectiveChanged()
    {
        var obj = CurrentObjective;
        CurrentObjectiveNavigationProgress = 0;
        Log($"next objective: {obj}");
        CurrentWaypoints.Clear();
        return obj != null && (!_combatFlag || obj.NavigationStrategy == NavigationStrategy.Continue);
    }

    private void TryPathfind(Vector3 start, List<Waypoint> connections, int maxRetries = 5)
    {
        //CurrentConnections = connections;

        if (connections.Count == 0)
            return;

        if (Service.PluginInterface == null)
        {
            Service.Log($"[QBD] UIDev detected, returning player's current position for waypoint");
            CurrentWaypoints = [new(start, true)];
            return;
        }

        PathfindTask = Task.Run(() => TryPathfind([new Waypoint(start), .. connections]));
    }

    private async Task<List<NavigationWaypoint>> TryPathfind(IEnumerable<Waypoint> connectionPoints)
    {
        while (!MeshIsReady())
        {
            Log($"navmesh is not ready - waiting");
            await Task.Delay(500).ConfigureAwait(true);
        }
        var points = connectionPoints.Take(3).ToList();
        if (points.Count < 2)
        {
            Log($"pathfind called with too few points (need 2, got {string.Join(", ", points)})");
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

            var ptVecs = await task.ConfigureAwait(true);
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
            thesePoints.AddRange(await TryPathfind(connectionPoints.Skip(1)).ConfigureAwait(true));
        return thesePoints;
    }

    private void MoveNext(Actor player, QuestObjective objective, AIHints hints)
    {
        if (CurrentWaypoints.Count == 0)
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
                objective.OnNavigationComplete?.Invoke();
            MoveNext(player, objective, hints);
        }
        else
        {
            Dash(player, direction, hints);
            hints.ForcedMovement = direction;
        }
    }

    enum ActionsProhibitedStatus : uint
    {
        OutOfTheAction = 2109,
        InEvent = 1268,
    }

    private void Dash(Actor player, Vector3 direction, AIHints hints)
    {
        if (!_config.UseDash || player.MountId > 0 || player.Statuses.Any(s => (ActionsProhibitedStatus)s.ID is ActionsProhibitedStatus.OutOfTheAction or ActionsProhibitedStatus.InEvent || Autorotation.RotationModuleManager.IsTransformStatus(s)))
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
                if (player.FindStatus(NIN.SID.Hidden) != null)
                    return;

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

    public void OnConditionChange(ConditionFlag flag, bool value) => CurrentObjective?.OnConditionChange?.Invoke(flag, value); // TODO: this should be redesigned

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

    private void DrawObjectives()
    {
        if (ImGui.Button(Paused ? "Resume" : "Pause"))
            Paused ^= true;

        ImGui.SameLine();

        if (ImGui.Button("Skip current step"))
            CurrentObjectiveIndex++;
        ImGui.SameLine();
        if (ImGui.Button("Restart from step 1"))
            CurrentObjectiveIndex = 0;

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        for (var i = 0; i < Objectives.Count; i++)
        {
            var n = Objectives[i];
            var highlight = i == CurrentObjectiveIndex;
            using var c = ImRaii.PushColor(ImGuiCol.Text, highlight ? ImGuiColors.DalamudWhite : ImGuiColors.DalamudGrey);
            ImGui.TextUnformatted($"#{i + 1} {n.DisplayName}");
            if (highlight)
            {
                foreach (var vec in CurrentWaypoints)
                {
                    if (vec.SpecifiedInPath)
                    {
                        UIMisc.IconText(FontAwesomeIcon.Star, "*");
                        ImGui.SameLine();
                    }
                    ImGui.TextUnformatted(Utils.Vec3String(vec.Position));
                }
            }
        }
    }

    private void DrawGenerateModule()
    {
        if (World.CurrentCFCID == 0)
            return;

        if (ImGui.Button("Generate module stub"))
        {
            var cfc = Service.LuminaRow<Lumina.Excel.Sheets.ContentFinderCondition>(World.CurrentCFCID);
            if (cfc == null)
                return;

            string name;
            if (cfc.Value.ContentLinkType == 5)
            {
                var qb = Service.LuminaRow<Lumina.Excel.Sheets.QuestBattle>(cfc.Value.Content.RowId)!;
                var quest = Service.LuminaRow<Lumina.Excel.Sheets.Quest>(qb.Value.Quest.RowId)!;
                name = quest.Value.Name.ToString();
            }
            else
            {
                name = cfc.Value.Name.ToString();
            }

            var expansion = cfc.Value.ClassJobLevelSync switch
            {
                > 0 and <= 50 => "ARealmReborn",
                > 50 and <= 60 => "Heavensward",
                > 60 and <= 70 => "Stormblood",
                > 70 and <= 80 => "Shadowbringers",
                > 80 and <= 90 => "Endwalker",
                > 90 and <= 100 => "Dawntrail",
                _ => "Unknown"
            };

            var questname = Utils.StringToIdentifier(name);

            var module = $"namespace BossMod.QuestBattle.{expansion};\n" +
                        $"\n" +
                        $"[ZoneModuleInfo(BossModuleInfo.Maturity.Contributed, {World.CurrentCFCID})]\n" +
                        $"internal class {questname}(WorldState ws) : QuestBattle(ws)\n" +
                        "{\n" +
                        "   public override List<QuestObjective> DefineObjectives(WorldState ws) => [\n" +
                        "       new QuestObjective(ws)\n" +
                        "   ];\n" +
                        "}\n";

            ImGui.SetClipboardText(module);
        }
    }
}
