using Dalamud.Hooking;
using ImGuiNET;

namespace BossMod;

sealed class Autorotation : IDisposable
{
    public readonly AutorotationConfig Config = Service.Config.Get<AutorotationConfig>();
    public readonly BossModuleManager Bossmods;
    public WorldState WorldState => Bossmods.WorldState;
    private readonly AutoHints _autoHints;
    private readonly UISimpleWindow _ui;
    private readonly EventSubscriptions _subscriptions;

    public CommonActions? ClassActions { get; private set; }

    public Actor? PrimaryTarget; // this is usually a normal (hard) target, but AI can override; typically used for damage abilities
    public Actor? SecondaryTarget; // this is usually a mouseover, but AI can override; typically used for heal and utility abilities
    public AIHints Hints = new();
    public float EffAnimLock => ActionManagerEx.Instance!.EffectiveAnimationLock;
    public float AnimLockDelay => ActionManagerEx.Instance!.AnimationLockDelayEstimate;

    private static readonly ActionID IDSprintGeneral = new(ActionType.General, 4);

    private unsafe delegate bool UseActionDelegate(FFXIVClientStructs.FFXIV.Client.Game.ActionManager* self, ActionType actionType, uint actionID, ulong targetID, uint itemLocation, uint callType, uint comboRouteID, bool* outOptGTModeStarted);
    private readonly Hook<UseActionDelegate> _useActionHook;

    public unsafe Autorotation(BossModuleManager bossmods)
    {
        Bossmods = bossmods;
        _autoHints = new(bossmods.WorldState);
        _ui = new("Autorotation", DrawOverlay, false, new(100, 100), ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoFocusOnAppearing) { RespectCloseHotkey = false };
        _subscriptions = new
        (
            WorldState.Client.ActionRequested.Subscribe(OnActionRequested),
            WorldState.Actors.CastEvent.Subscribe(OnCastEvent)
        );

        _useActionHook = Service.Hook.HookFromSignature<UseActionDelegate>("E8 ?? ?? ?? ?? EB 64 B1 01", UseActionDetour);
        _useActionHook.Enable();
    }

    public void Dispose()
    {
        _subscriptions.Dispose();
        _ui.Dispose();
        _useActionHook.Dispose();
        ClassActions?.Dispose();
        _autoHints.Dispose();
    }

    public unsafe void Update()
    {
        var player = WorldState.Party.Player();
        PrimaryTarget = WorldState.Actors.Find(player?.TargetID ?? 0);
        SecondaryTarget = WorldState.Actors.Find(Utils.MouseoverID());

        Hints.Clear();
        if (player != null)
        {
            var playerAssignment = Service.Config.Get<PartyRolesConfig>()[WorldState.Party.ContentIDs[PartyState.PlayerSlot]];
            var activeModule = Bossmods.ActiveModule?.StateMachine.ActivePhase != null ? Bossmods.ActiveModule : null;
            Hints.FillPotentialTargets(WorldState, playerAssignment == PartyRolesConfig.Assignment.MT || playerAssignment == PartyRolesConfig.Assignment.OT && !WorldState.Party.WithoutSlot().Any(p => p != player && p.Role == Role.Tank));
            Hints.FillForcedTarget(Bossmods.ActiveModule, WorldState, player);
            Hints.FillPlannedActions(Bossmods.ActiveModule, PartyState.PlayerSlot, player); // note that we might fill some actions even if module is not active yet (prepull)
            if (activeModule != null)
                activeModule.CalculateAIHints(PartyState.PlayerSlot, player, playerAssignment, Hints);
            else
                _autoHints.CalculateAIHints(Hints, player);
        }
        Hints.Normalize();
        if (Hints.ForcedTarget != null && PrimaryTarget != Hints.ForcedTarget)
        {
            PrimaryTarget = Hints.ForcedTarget;
            var obj = Hints.ForcedTarget.SpawnIndex >= 0 ? FFXIVClientStructs.FFXIV.Client.Game.Object.GameObjectManager.Instance()->Objects.IndexSorted[Hints.ForcedTarget.SpawnIndex].Value : null;
            if (obj != null && obj->EntityId != Hints.ForcedTarget.InstanceID)
                Service.Log($"[AR] Unexpected new target: expected {Hints.ForcedTarget.InstanceID:X} at #{Hints.ForcedTarget.SpawnIndex}, but found {obj->EntityId:X}");
            FFXIVClientStructs.FFXIV.Client.Game.Control.TargetSystem.Instance()->Target = obj;
        }

        Type? classType = null;
        if (Config.Enabled && player != null)
        {
            classType = player.Class switch
            {
                Class.WAR => typeof(WAR.Actions),
                Class.PLD => player.Level <= 60 ? typeof(PLD.Actions) : null,
                Class.MNK => typeof(MNK.Actions),
                Class.DRG => typeof(DRG.Actions),
                Class.BRD => typeof(BRD.Actions),
                Class.BLM => player.Level <= 60 ? typeof(BLM.Actions) : null,
                Class.SMN => player.Level <= 30 ? typeof(SMN.Actions) : null,
                Class.WHM => typeof(WHM.Actions),
                Class.SCH => player.Level <= 60 ? typeof(SCH.Actions) : null,
                Class.RPR => typeof(RPR.Actions),
                Class.GNB => typeof(GNB.Actions),
                Class.SAM => typeof(SAM.Actions),
                Class.DNC => typeof(DNC.Actions),
                _ => null
            };
        }

        if (ClassActions?.GetType() != classType || ClassActions?.Player != player)
        {
            ClassActions?.Dispose();
            ClassActions = classType != null ? (CommonActions?)Activator.CreateInstance(classType, this, player) : null;
        }

        ClassActions?.Update();
        var nextAction = ClassActions?.CalculateNextAction() ?? default;
        if (nextAction.Target != null && Hints.ForbiddenTargets.FirstOrDefault(e => e.Actor == nextAction.Target)?.Priority == AIHints.Enemy.PriorityForbidFully)
            nextAction = default;
        ActionManagerEx.Instance!.AutoQueue = nextAction; // TODO: this delays action for 1 frame after downtime, reconsider...

        ClassActions?.FillStatusesToCancel(Hints.StatusesToCancel);
        foreach (var s in Hints.StatusesToCancel)
        {
            var res = FFXIVClientStructs.FFXIV.Client.Game.StatusManager.ExecuteStatusOff(s.statusId, s.sourceId != 0 ? (uint)s.sourceId : Dalamud.Game.ClientState.Objects.Types.GameObject.InvalidGameObjectId);
            Service.Log($"[AR] Canceling status {s.statusId} from {s.sourceId:X} -> {res}");
        }

        _ui.IsOpen = ClassActions != null && Config.ShowUI;

        if (Config.ShowPositionals && PrimaryTarget != null && ClassActions != null && ClassActions.AutoAction != CommonActions.AutoActionNone && !PrimaryTarget.Omnidirectional)
        {
            var strategy = ClassActions.GetStrategy();
            var color = PositionalColor(strategy);
            switch (strategy.NextPositional)
            {
                case Positional.Flank:
                    Camera.Instance?.DrawWorldCone(PrimaryTarget.PosRot.XYZ(), PrimaryTarget.HitboxRadius + 1, PrimaryTarget.Rotation + 90.Degrees(), 45.Degrees(), color);
                    Camera.Instance?.DrawWorldCone(PrimaryTarget.PosRot.XYZ(), PrimaryTarget.HitboxRadius + 1, PrimaryTarget.Rotation - 90.Degrees(), 45.Degrees(), color);
                    break;
                case Positional.Rear:
                    Camera.Instance?.DrawWorldCone(PrimaryTarget.PosRot.XYZ(), PrimaryTarget.HitboxRadius + 1, PrimaryTarget.Rotation + 180.Degrees(), 45.Degrees(), color);
                    break;
            }
        }
    }

    private void DrawOverlay()
    {
        if (ClassActions == null)
            return;
        var next = ActionManagerEx.Instance!.AutoQueue;
        var state = ClassActions.GetState();
        var strategy = ClassActions.GetStrategy();
        ImGui.TextUnformatted($"[{ClassActions.AutoAction}] Next: {next.Action} ({next.Source})");
        if (ClassActions.AutoAction != CommonActions.AutoActionNone && strategy.NextPositional != Positional.Any)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, PositionalColor(strategy));
            ImGui.TextUnformatted(strategy.NextPositional.ToString());
            ImGui.PopStyleColor();
            ImGui.SameLine();
        }
        ImGui.TextUnformatted(strategy.ToString());
        ImGui.TextUnformatted($"Raidbuffs: {state.RaidBuffsLeft:f2}s left, next in {strategy.RaidBuffsIn:f2}s");
        ImGui.TextUnformatted($"Downtime: {strategy.FightEndIn:f2}s, pos-lock: {strategy.PositionLockIn:f2}");
        ImGui.TextUnformatted($"GCD={WorldState.Client.Cooldowns[CommonDefinitions.GCDGroup].Remaining:f3}, AnimLock={EffAnimLock:f3}+{AnimLockDelay:f3}, Combo={state.ComboTimeLeft:f3}");
    }

    private void OnActionRequested(ClientState.OpActionRequest op)
    {
        ClassActions?.NotifyActionExecuted(op.Request);
    }

    private void OnCastEvent(Actor actor, ActorCastEvent cast)
    {
        if (cast.SourceSequence != 0 && actor == WorldState.Party.Player())
            ClassActions?.NotifyActionSucceeded(cast);
    }

    private uint PositionalColor(CommonRotation.Strategy strategy)
    {
        return strategy.NextPositionalImminent
            ? (strategy.NextPositionalCorrect ? 0xff00ff00 : 0xff0000ff)
            : (strategy.NextPositionalCorrect ? 0xffffffff : 0xff00ffff);
    }

    // note: current implementation introduces slight input lag (on button press, next autorotation update will pick state updates, which will be executed on next action manager update)
    private unsafe bool UseActionDetour(FFXIVClientStructs.FFXIV.Client.Game.ActionManager* self, ActionType actionType, uint actionID, ulong targetID, uint itemLocation, uint callType, uint comboRouteID, bool* outOptGTModeStarted)
    {
        //Service.Log($"UA: {new ActionID(actionType, actionID)} @ {targetID:X}: {itemLocation} {callType} {comboRouteID}");
        if (callType != 0 || ClassActions == null)
        {
            // pass to hooked function transparently
            return _useActionHook.Original(self, actionType, actionID, targetID, itemLocation, callType, comboRouteID, outOptGTModeStarted);
        }

        var action = new ActionID(actionType, actionID);
        if (action == IDSprintGeneral)
            action = CommonDefinitions.IDSprint;
        bool nullTarget = targetID is 0 or Dalamud.Game.ClientState.Objects.Types.GameObject.InvalidGameObjectId;
        var target = nullTarget ? null : WorldState.Actors.Find(targetID);
        if (target == null && !nullTarget || !ClassActions.HandleUserActionRequest(action, target))
        {
            // unknown target (e.g. quest object) or unsupported action - pass to hooked function
            return _useActionHook.Original(self, actionType, actionID, targetID, itemLocation, callType, comboRouteID, outOptGTModeStarted);
        }

        return false;
    }
}
