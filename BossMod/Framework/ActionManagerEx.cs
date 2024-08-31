using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using System.Runtime.InteropServices;
using CSActionType = FFXIVClientStructs.FFXIV.Client.Game.ActionType;

namespace BossMod;

// extensions and utilities for interacting with game's ActionManager singleton
// handles following features:
// 1. automatic action execution (provided by autorotation or ai modules, if enabled); does nothing if no automatic actions are provided
// 2. effective animation lock reduction (a-la xivalex)
// 3. framerate-dependent cooldown reduction
// 4. slidecast assistant aka movement block
//    cast is interrupted if player moves when remaining cast time is greater than ~0.5s (moving during that window without interrupting is known as slidecasting)
//    this feature blocks WSAD input to prevent movement while this would interrupt a cast, allowing slidecasting efficiently while just holding movement button
//    other ways of moving (eg LMB+RMB, jumping etc) are not blocked, allowing for emergency movement even while the feature is active
//    movement is blocked a bit before cast start and unblocked as soon as action effect packet is received
// 5. preserving character facing direction
// 6. ground-targeted action queueing
//    ground-targeted actions can't be queued, making using them efficiently tricky
//    this feature allows queueing them, plus provides options to execute them automatically either at target's position or at cursor's position
// 7. auto cancel cast utility
// TODO: should not be public!
public sealed unsafe class ActionManagerEx : IDisposable
{
    public ActionID CastSpell => new(ActionType.Spell, _inst->CastSpellId);
    public ActionID CastAction => new((ActionType)_inst->CastActionType, _inst->CastActionId);
    public float CastTimeRemaining => _inst->CastSpellId != 0 ? _inst->CastTimeTotal - _inst->CastTimeElapsed : 0;
    public float ComboTimeLeft => _inst->Combo.Timer;
    public uint ComboLastMove => _inst->Combo.Action;
    public ActionID QueuedAction => new((ActionType)_inst->QueuedActionType, _inst->QueuedActionId);

    public float EffectiveAnimationLock => _inst->AnimationLock + CastTimeRemaining; // animation lock starts ticking down only when cast ends, so this is the minimal time until next action can be requested
    public float AnimationLockDelayEstimate => _animLockTweak.DelayEstimate;

    public Event<ClientActionRequest> ActionRequestExecuted = new();
    public Event<ulong, ActorCastEvent> ActionEffectReceived = new();

    public ActionTweaksConfig Config = Service.Config.Get<ActionTweaksConfig>();
    public ActionQueue.Entry AutoQueue { get; private set; }
    public bool MoveMightInterruptCast { get; private set; } // if true, moving now might cause cast interruption (for current or queued cast)
    public bool ForceCancelCastNextFrame;
    private readonly ActionManager* _inst = ActionManager.Instance();
    private readonly WorldState _ws;
    private readonly AIHints _hints;
    private readonly MovementOverride _movement;
    private readonly ManualActionQueueTweak _manualQueue;
    private readonly AnimationLockTweak _animLockTweak = new();
    private readonly CooldownDelayTweak _cooldownTweak = new();
    private readonly RestoreRotationTweak _restoreRotTweak = new();
    private readonly CancelCastTweak _cancelCastTweak;

    private readonly HookAddress<ActionManager.Delegates.Update> _updateHook;
    private readonly HookAddress<ActionManager.Delegates.UseAction> _useActionHook;
    private readonly HookAddress<ActionManager.Delegates.UseActionLocation> _useActionLocationHook;
    private readonly HookAddress<PublicContentBozja.Delegates.UseFromHolster> _useBozjaFromHolsterDirectorHook;
    private readonly HookAddress<ActionEffectHandler.Delegates.Receive> _processPacketActionEffectHook;

    private delegate void ExecuteCommandGTDelegate(uint commandId, Vector3* position, uint param1, uint param2, uint param3, uint param4);
    private readonly ExecuteCommandGTDelegate _executeCommandGT;
    private DateTime _nextAllowedExecuteCommand;

    public ActionManagerEx(WorldState ws, AIHints hints, MovementOverride movement)
    {
        _ws = ws;
        _hints = hints;
        _movement = movement;
        _manualQueue = new(ws, hints);
        _cancelCastTweak = new(ws);

        Service.Log($"[AMEx] ActionManager singleton address = 0x{(ulong)_inst:X}");
        _updateHook = new(ActionManager.Addresses.Update, UpdateDetour);
        _useActionHook = new(ActionManager.Addresses.UseAction, UseActionDetour);
        _useActionLocationHook = new(ActionManager.Addresses.UseActionLocation, UseActionLocationDetour);
        _useBozjaFromHolsterDirectorHook = new(PublicContentBozja.Addresses.UseFromHolster, UseBozjaFromHolsterDirectorDetour);
        _processPacketActionEffectHook = new(ActionEffectHandler.Addresses.Receive, ProcessPacketActionEffectDetour);

        var executeCommandGTAddress = Service.SigScanner.ScanText("E8 ?? ?? ?? ?? EB 1E 48 8B 53 08");
        Service.Log($"ExecuteCommandGT address: 0x{executeCommandGTAddress:X}");
        _executeCommandGT = Marshal.GetDelegateForFunctionPointer<ExecuteCommandGTDelegate>(executeCommandGTAddress);
    }

    public void Dispose()
    {
        _processPacketActionEffectHook.Dispose();
        _useBozjaFromHolsterDirectorHook.Dispose();
        _useActionLocationHook.Dispose();
        _useActionHook.Dispose();
        _updateHook.Dispose();
    }

    public void QueueManualActions()
    {
        _manualQueue.RemoveExpired();
        _manualQueue.FillQueue(_hints.ActionsToExecute);
    }

    // finish gathering candidate actions for this frame: sort by priority and select best action to execute
    public void FinishActionGather()
    {
        var player = _ws.Party.Player();
        AutoQueue = player != null ? _hints.ActionsToExecute.FindBest(_ws, player, _ws.Client.Cooldowns, EffectiveAnimationLock, _hints, _animLockTweak.DelayEstimate) : default;
        if (AutoQueue.Delay > 0)
            AutoQueue = default;
    }

    public Vector3? GetWorldPosUnderCursor()
    {
        Vector3 res = new();
        return _inst->GetGroundPositionForCursor(&res) ? res : null;
    }

    public void FaceTarget(Vector3 position, ulong unkObjID = 0xE0000000) => _inst->AutoFaceTargetPosition(&position, unkObjID);
    public void FaceDirection(WDir direction)
    {
        var player = GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        if (player != null)
            FaceTarget(player->Position.ToSystem() + direction.ToVec3());
    }

    public void GetCooldown(ref Cooldown result, RecastDetail* data)
    {
        if (data->IsActive != 0)
        {
            result.Elapsed = data->Elapsed;
            result.Total = data->Total;
        }
        else
        {
            result.Elapsed = result.Total = 0;
        }
    }

    public void GetCooldowns(Span<Cooldown> cooldowns)
    {
        // [0,80) are stored in actionmanager, [80,81) are stored in director
        var rg = _inst->GetRecastGroupDetail(0);
        for (int i = 0; i < 80; ++i)
            GetCooldown(ref cooldowns[i], rg++);
        rg = _inst->GetRecastGroupDetail(80);
        if (rg != null)
        {
            for (int i = 80; i < 82; ++i)
                GetCooldown(ref cooldowns[i], rg++);
        }
        else
        {
            for (int i = 80; i < 82; ++i)
                cooldowns[i] = default;
        }
    }

    public float GCD()
    {
        var gcd = _inst->GetRecastGroupDetail(ActionDefinitions.GCDGroup);
        return gcd->Total - gcd->Elapsed;
    }

    public ActionID GetDutyAction(ushort slot)
    {
        var id = ActionManager.GetDutyActionId(slot);
        return id != 0 ? new(ActionType.Spell, id) : default;
    }
    public (ActionID, ActionID) GetDutyActions() => (GetDutyAction(0), GetDutyAction(1));

    public uint GetAdjustedActionID(uint actionID) => _inst->GetAdjustedActionId(actionID);

    public uint GetSpellIdForAction(ActionID action) => ActionManager.GetSpellIdForAction((CSActionType)action.Type, action.ID);

    public uint GetActionStatus(ActionID action, ulong target, bool checkRecastActive = true, bool checkCastingActive = true, uint* outOptExtraInfo = null)
    {
        if (action.Type is ActionType.BozjaHolsterSlot0 or ActionType.BozjaHolsterSlot1)
            action = BozjaActionID.GetHolster(action.As<BozjaHolsterID>()); // see BozjaContentDirector.useFromHolster
        return _inst->GetActionStatus((CSActionType)action.Type, action.ID, target, checkRecastActive, checkCastingActive, outOptExtraInfo);
    }

    // returns time in ms
    public int GetAdjustedCastTime(ActionID action, bool applyProcs = true, ActionManager.CastTimeProc* outOptProc = null)
        => ActionManager.GetAdjustedCastTime((CSActionType)action.Type, action.ID, applyProcs, outOptProc);

    public int GetAdjustedRecastTime(ActionID action, bool applyClassMechanics = true) => ActionManager.GetAdjustedRecastTime((CSActionType)action.Type, action.ID, applyClassMechanics);

    public bool IsRecastTimerActive(ActionID action)
        => _inst->IsRecastTimerActive((CSActionType)action.Type, action.ID);

    public int GetRecastGroup(ActionID action)
        => _inst->GetRecastGroup((int)action.Type, action.ID);

    // see ActionEffectHandler.Receive - there are a few hardcoded actions here
    private bool ExpectAnimationLockUpdate(ActionEffectHandler.Header* header)
        => header->SourceSequence != 0 && !(header->ActionType == CSActionType.Action && (NIN.AID)header->ActionId is NIN.AID.Ten1 or NIN.AID.Chi1 or NIN.AID.Jin1 or NIN.AID.Ten2 or NIN.AID.Chi2 or NIN.AID.Jin2)
        || header->ForceAnimationLock;

    // perform some action transformations to simplify implementation of queueing; UseActionLocation expects some normalization to be already done
    private ActionID NormalizeActionForQueue(ActionID action)
    {
        switch (action.Type)
        {
            case ActionType.Spell:
                // for normal actions, we want to do adjustment immediately, before action is queued; there are several reasons for that:
                // 1. transformation can affect action targeting (eg MNK meditation/chakra); queue will check the properties of the queued action
                // 2. for classes that start at high level and then are synced down, the 'base' action can be some upgrade; the queue will ignore non-adjusted actions, assuming they aren't unlocked yet
                // note that when action is executed several frames later, the adjustment will be done again, in case something changes in the state
                return new(ActionType.Spell, GetAdjustedActionID(action.ID));
            case ActionType.General:
                // for general actions, we want to convert things we care about to spells; UseActionLocation will expect that to be done
                if (action == ActionDefinitions.IDGeneralLimitBreak)
                {
                    var lb = LimitBreakController.Instance();
                    var level = lb->BarUnits != 0 ? lb->CurrentUnits / lb->BarUnits : 0;
                    var id = level > 0 ? lb->GetActionId((Character*)GameObjectManager.Instance()->Objects.IndexSorted[0].Value, (byte)(level - 1)) : 0;
                    return id != 0 ? new(ActionType.Spell, id) : action;
                }
                else if (action == ActionDefinitions.IDGeneralSprint || action == ActionDefinitions.IDGeneralDuty1 || action == ActionDefinitions.IDGeneralDuty2)
                {
                    return new(ActionType.Spell, GetSpellIdForAction(action));
                }
                else
                {
                    return action;
                }
            default:
                return action;
        }
    }

    // skips queueing etc
    private bool ExecuteAction(ActionID action, ulong targetId, Vector3 targetPos)
    {
        if (action.Type is ActionType.BozjaHolsterSlot0 or ActionType.BozjaHolsterSlot1)
        {
            // fake action type - using action from bozja holster
            var state = PublicContentBozja.GetState(); // note: if it's non-null, the director instance can't be null too
            var holsterIndex = state != null ? state->HolsterActions.IndexOf((byte)action.ID) : -1;
            return holsterIndex >= 0 && PublicContentBozja.GetInstance()->UseFromHolster((uint)holsterIndex, action.Type == ActionType.BozjaHolsterSlot1 ? 1u : 0);
        }
        else if (action.Type == ActionType.PetAction && action.ID == 3)
        {
            // pet actions are special; TODO support actions other than place (3), these use different send-packet function
            var now = DateTime.Now;
            if (_nextAllowedExecuteCommand > now)
                return false;
            _nextAllowedExecuteCommand = now.AddMilliseconds(100);
            _executeCommandGT(1800, &targetPos, action.ID, 0, 0, 0);
            return true;
        }
        else
        {
            // real action type, just execute our UAL hook
            // note that for items extraParam should be 0xFFFF (since we want to use any item, not from first inventory slot)
            // note that for 'summon carbuncle/eos/titan/ifrit/garuda' actions, extraParam can be used to select glamour
            var extraParam = action.Type switch
            {
                ActionType.Spell => ActionManager.GetExtraParamForSummonAction(action.ID), // will return 0 for non-summon actions
                ActionType.Item => 0xFFFFu,
                _ => 0u
            };
            return _inst->UseActionLocation((CSActionType)action.Type, action.ID, targetId, &targetPos, extraParam);
        }
    }

    private void UpdateDetour(ActionManager* self)
    {
        var fwk = Framework.Instance();
        var dt = fwk->GameSpeedMultiplier * fwk->FrameDeltaTime;
        var imminentAction = _inst->ActionQueued ? QueuedAction : AutoQueue.Action;
        var imminentActionAdj = imminentAction.Type == ActionType.Spell ? new(ActionType.Spell, GetAdjustedActionID(imminentAction.ID)) : imminentAction;
        var imminentRecast = imminentActionAdj ? _inst->GetRecastGroupDetail(GetRecastGroup(imminentActionAdj)) : null;

        _cooldownTweak.StartAdjustment(_inst->AnimationLock, imminentRecast != null && imminentRecast->IsActive != 0 ? imminentRecast->Total - imminentRecast->Elapsed : 0, dt);
        _updateHook.Original(self);

        // check whether movement is safe; block movement if not and if desired
        MoveMightInterruptCast &= CastTimeRemaining > 0; // previous cast could have ended without action effect
        MoveMightInterruptCast |= imminentActionAdj && CastTimeRemaining <= 0 && _inst->AnimationLock < 0.1f && GetAdjustedCastTime(imminentActionAdj) > 0 && GCD() < 0.1f; // if we're not casting, but will start soon, moving might interrupt future cast
        bool blockMovement = Config.PreventMovingWhileCasting && MoveMightInterruptCast;

        // restore rotation logic; note that movement abilities (like charge) can take multiple frames until they allow changing facing
        var player = GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        if (player != null && !MoveMightInterruptCast && _restoreRotTweak.TryRestore(player->Rotation.Radians(), out var restore))
        {
            FaceDirection(restore.ToDirection());
        }

        // note: if we cancel movement and start casting immediately, it will be canceled some time later - instead prefer to delay for one frame
        if (EffectiveAnimationLock <= 0 && AutoQueue.Action && !IsRecastTimerActive(AutoQueue.Action) && !(blockMovement && _movement.IsMoving()))
        {
            var actionAdj = NormalizeActionForQueue(AutoQueue.Action);
            var targetID = AutoQueue.Target?.InstanceID ?? 0xE0000000;
            var status = GetActionStatus(actionAdj, targetID);
            if (status == 0)
            {
                if (AutoQueue.FacingAngle != null)
                    FaceDirection(AutoQueue.FacingAngle.Value.ToDirection());

                var res = ExecuteAction(actionAdj, targetID, AutoQueue.TargetPos);
                //Service.Log($"[AMEx] Auto-execute {AutoQueue.Source} action {AutoQueue.Action} (=> {actionAdj}) @ {targetID:X} {Utils.Vec3String(AutoQueue.TargetPos)} => {res}");
            }
            else
            {
                Service.Log($"[AMEx] Can't execute prio {AutoQueue.Priority} action {AutoQueue.Action} (=> {actionAdj}) @ {targetID:X}: status {status} '{Service.LuminaRow<Lumina.Excel.GeneratedSheets.LogMessage>(status)?.Text}'");
                blockMovement = false;
            }
        }

        _cooldownTweak.StopAdjustment(); // clear any potential adjustments

        _movement.MovementBlocked = blockMovement;

        if (_ws.Party.Player()?.CastInfo != null && _cancelCastTweak.ShouldCancel(_ws.CurrentTime, ForceCancelCastNextFrame))
            UIState.Instance()->Hotbar.CancelCast();
        ForceCancelCastNextFrame = false;
    }

    // note: targetId is usually your current primary target (or 0xE0000000 if you don't target anyone), unless you do something like /ac XXX <f> etc
    private bool UseActionDetour(ActionManager* self, CSActionType actionType, uint actionId, ulong targetId, uint extraParam, ActionManager.UseActionMode mode, uint comboRouteId, bool* outOptAreaTargeted)
    {
        var action = new ActionID((ActionType)actionType, actionId);
        //Service.Log($"[AMEx] UA: {action} @ {targetId:X}: {extraParam} {mode} {comboRouteId}");
        action = NormalizeActionForQueue(action);

        // if mouseover mode is enabled AND target is a usual primary target AND current mouseover is valid target for action, then we override target to mouseover
        var primaryTarget = TargetSystem.Instance()->Target;
        var primaryTargetId = primaryTarget != null ? primaryTarget->GetGameObjectId() : 0xE0000000;
        bool targetOverridden = targetId != primaryTargetId;
        if (Config.PreferMouseover && !targetOverridden)
        {
            var mouseoverTarget = PronounModule.Instance()->UiMouseOverTarget;
            if (mouseoverTarget != null && ActionManager.CanUseActionOnTarget(GetSpellIdForAction(action), mouseoverTarget))
            {
                targetId = mouseoverTarget->GetGameObjectId();
                targetOverridden = true;
            }
        }

        (ulong, Vector3?) getAreaTarget() => targetOverridden ? (targetId, null) :
            (Config.GTMode == ActionTweaksConfig.GroundTargetingMode.AtTarget ? targetId : 0xE0000000, Config.GTMode == ActionTweaksConfig.GroundTargetingMode.AtCursor ? GetWorldPosUnderCursor() : null);

        // note: only standard mode can be filtered
        // note: current implementation introduces slight input lag (on button press, next autorotation update will pick state updates, which will be executed on next action manager update)
        if (mode == ActionManager.UseActionMode.None && action.Type is ActionType.Spell or ActionType.Item && _manualQueue.Push(action, targetId, !targetOverridden, getAreaTarget))
            return false;

        bool areaTargeted = false;
        var res = _useActionHook.Original(self, actionType, actionId, targetId, extraParam, mode, comboRouteId, &areaTargeted);
        if (outOptAreaTargeted != null)
            *outOptAreaTargeted = areaTargeted;
        if (areaTargeted && Config.GTMode == ActionTweaksConfig.GroundTargetingMode.AtCursor)
            self->AreaTargetingExecuteAtCursor = true;
        if (areaTargeted && Config.GTMode == ActionTweaksConfig.GroundTargetingMode.AtTarget)
            self->AreaTargetingExecuteAtObject = targetId;
        return res;
    }

    private bool UseActionLocationDetour(ActionManager* self, CSActionType actionType, uint actionId, ulong targetId, Vector3* location, uint extraParam)
    {
        var player = GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        var prevSeq = _inst->LastUsedActionSequence;
        var prevRot = player != null ? player->Rotation.Radians() : default;
        bool ret = _useActionLocationHook.Original(self, actionType, actionId, targetId, location, extraParam);
        var currSeq = _inst->LastUsedActionSequence;
        var currRot = player != null ? player->Rotation.Radians() : default;
        if (currSeq != prevSeq)
        {
            HandleActionRequest(new((ActionType)actionType, actionId), currSeq, targetId, *location, prevRot, currRot);
        }
        return ret;
    }

    private bool UseBozjaFromHolsterDirectorDetour(PublicContentBozja* self, uint holsterIndex, uint slot)
    {
        var player = GameObjectManager.Instance()->Objects.IndexSorted[0].Value;
        var prevRot = player != null ? player->Rotation.Radians() : default;
        var res = _useBozjaFromHolsterDirectorHook.Original(self, holsterIndex, slot);
        var currRot = player != null ? player->Rotation.Radians() : default;
        if (res)
        {
            var entry = (BozjaHolsterID)self->State.HolsterActions[(int)holsterIndex];
            HandleActionRequest(ActionID.MakeBozjaHolster(entry, (int)slot), 0, 0xE0000000, default, prevRot, currRot);
        }
        return res;
    }

    private void ProcessPacketActionEffectDetour(uint casterID, Character* casterObj, Vector3* targetPos, ActionEffectHandler.Header* header, ActionEffectHandler.TargetEffects* effects, GameObjectId* targets)
    {
        // notify listeners about the event
        // note: there's a slight difference with dispatching event from here rather than from packet processing (ActionEffectN) functions
        // 1. action id is already unscrambled
        // 2. this function won't be called if caster object doesn't exist
        // the last point is deemed to be minor enough for us to not care, as it simplifies things (no need to hook 5 functions)
        var info = new ActorCastEvent
        {
            Action = new ActionID((ActionType)header->ActionType, header->ActionId),
            MainTargetID = header->AnimationTargetId,
            AnimationLockTime = header->AnimationLock,
            MaxTargets = header->NumTargets,
            TargetPos = *targetPos,
            SourceSequence = header->SourceSequence,
            GlobalSequence = header->GlobalSequence,
        };
        var rawEffects = (ulong*)effects;
        for (int i = 0; i < header->NumTargets; ++i)
        {
            var targetEffects = new ActionEffects();
            for (int j = 0; j < ActionEffects.MaxCount; ++j)
                targetEffects[j] = rawEffects[i * 8 + j];
            info.Targets.Add(new(targets[i], targetEffects));
        }
        ActionEffectReceived.Fire(casterID, info);

        // call the hooked function and observe the effects
        var packetAnimLock = header->AnimationLock;
        var prevAnimLock = _inst->AnimationLock;
        _processPacketActionEffectHook.Original(casterID, casterObj, targetPos, header, effects, targets);
        var currAnimLock = _inst->AnimationLock;

        if (casterID != UIState.Instance()->PlayerState.EntityId || !ExpectAnimationLockUpdate(header))
        {
            // this action is either executed by non-player, or is non-player-initiated
            // TODO: reconsider the condition:
            // - do we want to do non-anim-lock related things (eg unblock movement override) when we get action with 'force anim lock' flag?
            if (currAnimLock != prevAnimLock)
                Service.Log($"[AMEx] Animation lock updated by non-player-initiated action: #{header->SourceSequence} {casterID:X} {info.Action} {prevAnimLock:f3} -> {currAnimLock:f3}");
            return;
        }

        MoveMightInterruptCast = false; // slidecast window start
        _movement.MovementBlocked = false; // unblock input unconditionally on successful cast (I assume there are no instances where we need to immediately start next GCD?)

        // animation lock delay update
        var animLockReduction = _animLockTweak.Apply(header->SourceSequence, prevAnimLock, _inst->AnimationLock, packetAnimLock, header->AnimationLock, out var animLockDelay);
        _inst->AnimationLock -= animLockReduction;
        Service.Log($"[AMEx] AEP #{header->SourceSequence} {prevAnimLock:f3} {info.Action} -> ALock={currAnimLock:f3} (delayed by {animLockDelay:f3}) -> {_inst->AnimationLock:f3}), Flags={header->Flags:X}, CTR={CastTimeRemaining:f3}, GCD={GCD():f3}");
    }

    private void HandleActionRequest(ActionID action, uint seq, ulong targetID, Vector3 targetPos, Angle prevRot, Angle currRot)
    {
        _manualQueue.Pop(action);
        _animLockTweak.RecordRequest(seq, _inst->AnimationLock);
        _restoreRotTweak.Preserve(prevRot, currRot);
        MoveMightInterruptCast = CastTimeRemaining > 0;

        var recast = _inst->GetRecastGroupDetail(GetRecastGroup(action));

        if (CastTimeRemaining > 0)
            _inst->CastTimeElapsed += _cooldownTweak.Adjustment;
        else
            _inst->AnimationLock = Math.Max(0, _inst->AnimationLock - _cooldownTweak.Adjustment);

        if (recast != null)
            recast->Elapsed += _cooldownTweak.Adjustment;

        var (castElapsed, castTotal) = _inst->CastSpellId != 0 ? (_inst->CastTimeElapsed, _inst->CastTimeTotal) : (0, 0);
        var (recastElapsed, recastTotal) = recast != null ? (recast->Elapsed, recast->Total) : (0, 0);
        Service.Log($"[AMEx] UAL #{seq} {action} @ {targetID:X} / {Utils.Vec3String(targetPos)}, ALock={_inst->AnimationLock:f3}, CTR={CastTimeRemaining:f3}, CD={recastElapsed:f3}/{recastTotal:f3}, GCD={GCD():f3}");
        ActionRequestExecuted.Fire(new(action, targetID, targetPos, seq, _inst->AnimationLock, castElapsed, castTotal, recastElapsed, recastTotal));
    }
}
